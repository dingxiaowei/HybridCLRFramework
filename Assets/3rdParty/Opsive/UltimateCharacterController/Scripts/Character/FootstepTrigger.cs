/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character
{
    /// <summary>
    /// Notifies the CharacterFootEffects component when the foot has collided with the ground.
    /// </summary>
    public class FootstepTrigger : MonoBehaviour
    {
        [Tooltip("Should the footprint texture be flipped?")]
        [SerializeField] protected bool m_FlipFootprint;

        public bool FlipFootprint { get { return m_FlipFootprint; } set { m_FlipFootprint = value; } }

        private Transform m_Transform;
        private CharacterFootEffects m_FootEffects;
        private CharacterLayerManager m_CharacterLayerManager;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            m_FootEffects = GetComponentInParent<CharacterFootEffects>();
            m_CharacterLayerManager = GetComponentInParent<CharacterLayerManager>();
        }

        /// <summary>
        /// The trigger has collided with another object.
        /// </summary>
        /// <param name="other">The Collider that the trigger collided with.</param>
        private void OnTriggerEnter(Collider other)
        {
            // Notify the CharacterFootEffects component if the layer is valid.
            if (MathUtility.InLayerMask(other.gameObject.layer, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers)) {
                m_FootEffects.FootStep(m_Transform, m_FlipFootprint);
            }
        }
    }
}