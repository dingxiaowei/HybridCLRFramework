/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits
{
    using Opsive.Shared.Events;
    using UnityEngine;

    /// <summary>
    /// Extends the Respawner by listening/executing character related events.
    /// </summary>
    public class CharacterRespawner : Respawner
    {
        private bool m_Active;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterActivate", OnActivate);
            m_Active = true;
        }

        /// <summary>
        /// Does the respawn by setting the position and rotation to the specified values.
        /// Enable the GameObject and let all of the listening objects know that the object has been respawned.
        /// </summary>
        /// <param name="position">The respawn position.</param>
        /// <param name="rotation">The respawn rotation.</param>
        /// <param name="transformChange">Was the position or rotation changed?</param>
        public override void Respawn(Vector3 position, Quaternion rotation, bool transformChange)
        {
            base.Respawn(position, rotation, transformChange);

            // Execute OnCharacterImmediateTransformChange after OnRespawn to ensure all of the interested components are using the new position/rotation.
            if (transformChange) {
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterImmediateTransformChange", true);
            }
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        protected override void OnDisable()
        {
            // If the GameObject was deactivated then the respawner shouldn't respawn.
            if (m_Active) {
                base.OnDisable();
            }
        }

        /// <summary>
        /// The character has been activated or deactivated.
        /// </summary>
        /// <param name="activate">Was the character activated?</param>
        private void OnActivate(bool activate)
        {
            m_Active = activate;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterActivate", OnActivate);
        }
    }
}