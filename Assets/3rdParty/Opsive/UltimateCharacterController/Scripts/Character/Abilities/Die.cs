/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// Plays a death animation when the character dies.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    [DefaultState("Death")]
    [DefaultAbilityIndex(4)]
    public class Die : Ability
    {
        [Tooltip("The amount of force to add to the camera. This value will be multiplied by the death force magnitude.")]
        [SerializeField] protected Vector3 m_CameraRotationalForce = new Vector3(0, 0, 0.75f);

        public Vector3 CameraRotationalForce { get { return m_CameraRotationalForce; } set { m_CameraRotationalForce = value; } }

        /// <summary>
        /// The type of animation that the ability should play.
        /// </summary>
        private enum DeathType {
            Forward, // Play a forward death animation.
            Backward // Play a backward death animation.
        }

        private int m_DeathTypeIndex;
        private Vector3 m_Force;
        private Vector3 m_Position;

        [NonSerialized] public Vector3 Force { get { return m_Force; } set { m_Force = value; } }
        [NonSerialized] public Vector3 Position { get { return m_Position; } set { m_Position = value; } }

        public override int AbilityIntData { get { return m_DeathTypeIndex; } }
        public override bool CanStayActivatedOnDeath { get { return true; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }

        /// <summary>
        /// The character has died. Start the ability.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            if (!Enabled) {
                return;
            }

            m_Force = force;
            m_Position = position;
            m_DeathTypeIndex = GetDeathTypeIndex(position, force, attacker);
            StartAbility();
        }

        /// <summary>
        /// Returns the value that the AbilityIntData parameter should be set to.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        /// <returns>The value that the AbilityIntData parameter should be set to.</returns>
        protected virtual int GetDeathTypeIndex(Vector3 position, Vector3 force, GameObject attacker)
        {
            return (int)(m_Transform.InverseTransformPoint(position).z > 0 ? DeathType.Forward : DeathType.Backward);
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_CharacterLocomotion.ResetRotationPosition();
            EventHandler.ExecuteEvent(m_GameObject, "OnCameraRotationalForce", m_CameraRotationalForce * m_Force.magnitude);
        }

        /// <summary>
        /// The character has respawned. Stop the die ability.
        /// </summary>
        private void OnRespawn()
        {
            if (!Enabled) {
                return;
            }
            StopAbility();
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }
    }
}
