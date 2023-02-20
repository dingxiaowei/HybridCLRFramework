/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.SurfaceSystem;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    /// <summary>
    /// ItemAbility which will start using the MeleeWeapon while in the air.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Fire1")]
    [DefaultItemStateIndex(2)]
    [DefaultState("Use")]
    [AllowMultipleAbilityTypes]
    public class InAirMeleeUse : Use
    {
        [Tooltip("The amount of force that should be applied at the start of the ability.")]
        [SerializeField] protected Vector3 m_UpwardForce = new Vector3(0, 1, 0);
        [Tooltip("The number of frames that the upward force is applied in.")]
        [SerializeField] protected int m_UpwardForceFrames = 4;
        [Tooltip("The amount of force that should be applied when the character starts to fall.")]
        [SerializeField] protected Vector3 m_DownwardForce = new Vector3(0, -15, 0);
        [Tooltip("The number of frames that the downward force is applied in.")]
        [SerializeField] protected int m_DownwardForceFrames = 4;
        [Tooltip("The value of the ItemSubstateIndex parameter when the character has becomes grounded after performing the use.")]
        [SerializeField] protected int m_OnGroundedSubstateIndex = 11;
        [Tooltip("The impact effect that should be played when the character is grounded.")]
        [SerializeField] protected SurfaceImpact m_GroundImpact;
        [Tooltip("The amount of positional force to add to the camera when the character is grounded.")]
        [SerializeField] protected MinMaxVector3 m_GroundPositionCameraRecoil;
        [Tooltip("The amount of rotational recoil to add to the camera when the character is grounded.")]
        [SerializeField] protected MinMaxVector3 m_GroundRotationCameraRecoil = new MinMaxVector3(new Vector3(-40f, -40f, 0), new Vector3(40f, 40f, 0), new Vector3(20, 20, 0));

        public Vector3 UpwardForce { get { return m_UpwardForce; } set { m_UpwardForce = value; } }
        public int UpwardForceFrames { get { return m_UpwardForceFrames; } set { m_UpwardForceFrames = value; } }
        public Vector3 DownwardForce { get { return m_DownwardForce; } set { m_DownwardForce = value; } }
        public int DownwardForceFrames { get { return m_DownwardForceFrames; } set { m_DownwardForceFrames = value; } }
        public MinMaxVector3 GroundPositionCameraRecoil { get { return m_GroundPositionCameraRecoil; } set { m_GroundPositionCameraRecoil = value; } }
        public MinMaxVector3 GroundRotationCameraRecoil { get { return m_GroundRotationCameraRecoil; } set { m_GroundRotationCameraRecoil = value; } }

        private Jump m_JumpAbility;
        private bool m_AppliedDownwardForce;
        private bool m_UseOnGroundedSubstate;

        public override bool CanReceiveMultipleStarts { get { return false; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_JumpAbility = m_CharacterLocomotion.GetAbility<Jump>();

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartAbility()
        {
            // The character needs to be in the air for the ability to start.
            if (m_CharacterLocomotion.Grounded) {
                return false;
            }

            if (!base.CanStartAbility()) {
                return false;
            }

            // The IUsableWeapon must be a MeleeWeapon.
            var usableMeleeWeapon = false;
            if (m_SlotID == -1) {
                for (int i = 0; i < m_UsableItems.Length; ++i) {
                    if (m_UsableItems[i] is MeleeWeapon) {
                        usableMeleeWeapon = true;
                    } else {
                        m_UsableItems[i] = null;
                    }
                }
            } else {
                if (m_UsableItems[0] is MeleeWeapon) {
                    usableMeleeWeapon = true;
                } else {
                    m_UsableItems[0] = null;
                }
            }

            return usableMeleeWeapon;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            if (base.ShouldStopActiveAbility(activeAbility)) {
                return true;
            }

            return activeAbility is Jump || activeAbility is Use;
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            if (base.ShouldBlockAbilityStart(startingAbility)) {
                return true;
            }

            return startingAbility is Jump || startingAbility is Use;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_CharacterLocomotion.AddRelativeForce(m_UpwardForce, m_UpwardForceFrames);
            m_AppliedDownwardForce = false;
            m_UseOnGroundedSubstate = false;
            // If the character is near the ground the use complete event may occur before use. Prevent the character from getting stuck by
            // allowing the complete event to occur.
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                ScheduleCompleteEvent(i, false);
            }
        }

        /// <summary>
        /// Returns the Item Substate Index which corresponds to the slot ID.
        /// </summary>
        /// <param name="slotID">The ID of the slot that corresponds to the Item Substate Index.</param>
        /// <returns>The Item Substate Index which corresponds to the slot ID.</returns>
        public override int GetItemSubstateIndex(int slotID)
        {
            // The OnGrounded Substate Index value should be triggered when the character lands with the melee attack. The ability will then stop when the item use event is sent.
            if (m_UseOnGroundedSubstate) {
                if ((m_SlotID == -1 && m_UsableItems[slotID] != null) || (m_SlotID != -1 && m_UsableItems[0] != null)) {
                    return m_OnGroundedSubstateIndex;
                }
            }

            return base.GetItemSubstateIndex(slotID);
        }

        /// <summary>
        /// Updates the ability after the controller has updated. This will ensure the character is in the most up to date position.
        /// </summary>
        public override void LateUpdate()
        {
            base.LateUpdate();

            if (!m_AppliedDownwardForce && m_CharacterLocomotion.LocalVelocity.y < 0) {
                m_CharacterLocomotion.AddRelativeForce(m_DownwardForce, m_DownwardForceFrames);
                m_AppliedDownwardForce = true;
            }
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            // The ability can only stop when the character is grounded.
            if (!m_CharacterLocomotion.Grounded && (m_JumpAbility == null || !m_JumpAbility.IsActive)) {
                return false;
            }
            return base.CanStopAbility();
        }

        /// <summary>
        /// The character has changed grounded states. 
        /// </summary>
        /// <param name="grounded">Is the character on the ground?</param>
        private void OnGrounded(bool grounded)
        {
            if (!IsActive) {
                return;
            }

            if (grounded) {
                SurfaceManager.SpawnEffect(m_CharacterLocomotion.GroundRaycastHit, m_GroundImpact, m_CharacterLocomotion.GravityDirection, m_CharacterLocomotion.TimeScale, m_GameObject);
                EventHandler.ExecuteEvent(m_GameObject, "OnAddSecondaryCameraForce", m_GroundPositionCameraRecoil.RandomValue, m_GroundRotationCameraRecoil.RandomValue, 0f);

                // The item may have completed its use before the object is grounded.
                if (CanStopAbility()) {
                    StopAbility();
                } else {
                    m_UseOnGroundedSubstate = true;
                    m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();
                }
            }
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
        }
    }
}