/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using UnityEngine;

    /// <summary>
    /// Teleports the character.
    /// </summary>
    [System.Serializable]
    public class Teleport : CastAction
    {
        [Tooltip("Should the character's animator be snapped?")]
        [SerializeField] protected bool m_SnapAnimator;

        public bool SnapAnimator { get { return m_SnapAnimator; } set { m_SnapAnimator = value; } }

        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private CharacterLayerManager m_CharacterLayerManager;

        /// <summary>
        /// Initializes the CastAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the CastAction belongs to.</param>
        /// <param name="index">The index of the CastAction.</param>
        public override void Initialize(GameObject character, MagicItem magicItem, int index)
        {
            base.Initialize(character, magicItem, index);

            m_CharacterLocomotion = character.GetCachedComponent<UltimateCharacterLocomotion>();
            m_CharacterLayerManager = character.GetCachedComponent<CharacterLayerManager>();
        }

        /// <summary>
        /// Is the specified position a valid target position?
        /// </summary>
        /// <param name="position">The position that may be a valid target position.</param>
        /// <param name="normal">The normal of the position.</param>
        /// <returns>True if the specified position is a valid target position.</returns>
        public override bool IsValidTargetPosition(Vector3 position, Vector3 normal)
        {
            // The slope must be less than the slope limit.
            if (Vector3.Angle(m_CharacterLocomotion.Up, normal) > m_CharacterLocomotion.SlopeLimit + m_CharacterLocomotion.SlopeLimitSpacing) {
                return false;
            }

            // There must be enough space to stand.
            if (Physics.SphereCast(new Ray(position - m_CharacterLocomotion.Up * m_CharacterLocomotion.Radius, m_CharacterLocomotion.Up), m_CharacterLocomotion.Radius, 
                                        m_CharacterLocomotion.Height, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Performs the cast.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="origin">The location that the cast should spawn from.</param>
        /// <param name="direction">The direction of the cast.</param>
        /// <param name="targetPosition">The target position of the cast.</param>
        public override void Cast(Transform origin, Vector3 direction, Vector3 targetPosition)
        {
            direction = Vector3.ProjectOnPlane(targetPosition - m_GameObject.transform.position, m_CharacterLocomotion.Up);
            m_CharacterLocomotion.SetPositionAndRotation(targetPosition, Quaternion.LookRotation(direction), m_SnapAnimator, false);
        }
    }
}