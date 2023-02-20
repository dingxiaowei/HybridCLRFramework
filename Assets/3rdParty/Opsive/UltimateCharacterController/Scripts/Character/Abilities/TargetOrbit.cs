using UnityEngine;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// Orbits around the target when the character moves.
    /// </summary>
    [DefaultStartType(AbilityStartType.Automatic)]
    [DefaultStopType(AbilityStopType.Automatic)]
    public class TargetOrbit : Ability
    {
        [Tooltip("Should the ability use the aim assist target?")]
        [SerializeField] protected bool m_UseAimAssistTarget;
        [Tooltip("Specifies the target transform if the aim assist target is not used.")]
        [SerializeField] protected Transform m_Target;

        public bool UseAimAssistTarget { get { return m_UseAimAssistTarget; } set { m_UseAimAssistTarget = value; } }

        private AimAssist m_AimAssist;

        public override bool IsConcurrent { get { return true; } }

        private Transform Target
        {
            get
            {
                Transform target = null;
                if (m_UseAimAssistTarget && m_AimAssist != null && m_AimAssist.HasTarget()) {
                    target = m_AimAssist.Target;
                } else {
                    target = m_Target;
                }
                return target;
            }
        }
        
        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
        }
        
        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            if (lookSource == null) {
                m_AimAssist = null;
            } else {
                m_AimAssist = lookSource.GameObject.GetCachedComponent<AimAssist>();
            }
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            if (!base.CanStartAbility()) {
                return false;
            }

            return Target != null;
        }

        /// <summary>
        /// Verify the position values. Called immediately before the position is applied.
        /// </summary>
        public override void ApplyPosition()
        {
            // The character's z relative direction can change when input is applied to the y input vector. It can also change when there
            // is no input, during this time the velocity is changing the position.
            if (Mathf.Abs(m_CharacterLocomotion.InputVector.y) > 0.0001f || m_CharacterLocomotion.RawInputVector.sqrMagnitude == 0) {
                return;
            }

            // The character's z relative direction should not change when the character is orbiting around the target.
            var targetPosition = m_Transform.position + m_CharacterLocomotion.MoveDirection;
            var rotation = Quaternion.LookRotation((Target.position - targetPosition).normalized, m_CharacterLocomotion.Up);
            var direction = MathUtility.InverseTransformDirection(m_CharacterLocomotion.MoveDirection, rotation);
            direction.z = 0;
            m_CharacterLocomotion.MoveDirection = MathUtility.TransformDirection(direction, rotation);
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            return Target == null;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
        }
    }
}