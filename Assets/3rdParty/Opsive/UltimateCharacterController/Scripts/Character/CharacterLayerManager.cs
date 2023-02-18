/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.UltimateCharacterController.Game;
    using UnityEngine;

    /// <summary>
    /// Sets up custom layers for the character.
    /// </summary>
    public class CharacterLayerManager : MonoBehaviour
    {
        [Tooltip("Layer Mask that specifies the layer that the enemies use.")]
        [SerializeField] protected LayerMask m_EnemyLayers = 1 << LayerManager.Enemy;
        [Tooltip("Layer Mask that specifies any layers that are invisible to the character (such as water or invisible planes placed on top of stairs). ")]
        [SerializeField] protected LayerMask m_InvisibleLayers = (1 << LayerManager.TransparentFX) | (1 << LayerManager.IgnoreRaycast) | (1 << LayerManager.UI) | (1 << LayerManager.VisualEffect) | (1 << LayerManager.Overlay) | (1 << LayerManager.SubCharacter);
        [Tooltip("Layer mask that specifies any layers that represent a solid object (such as the ground or a moving platform).")]
        [SerializeField] protected LayerMask m_SolidObjectLayers = ~((1 << LayerManager.IgnoreRaycast) | (1 << LayerManager.Water) | (1 << LayerManager.UI) | (1 << LayerManager.VisualEffect) | (1 << LayerManager.Overlay) | (1 << LayerManager.SubCharacter));

        public LayerMask EnemyLayers { get { return m_EnemyLayers; } }
        public LayerMask InvisibleLayers { get { return m_InvisibleLayers; } }
        public LayerMask SolidObjectLayers { get { return m_SolidObjectLayers; } }

        // Represents the mask that ignores any invisible objects.
        public int IgnoreInvisibleLayers { get { return ~m_InvisibleLayers; } }
        // Represents the mask that ignores any invisible objects and the character.
        public int IgnoreInvisibleCharacterLayers { get { return ~(m_InvisibleLayers | m_CharacterLayer); } }
        // Represents the mask that ignores any invisible objects and the character/water.
        public int IgnoreInvisibleCharacterWaterLayers { get { return ~(m_InvisibleLayers | m_CharacterLayer | (1 << LayerManager.Water)); } }

        private LayerMask m_CharacterLayer = 1 << LayerManager.Character;

        public LayerMask CharacterLayer { get { return m_CharacterLayer; } }

        /// <summary>
        /// Setups the character layer.
        /// </summary>
        public void Awake()
        {
            m_CharacterLayer = 1 << gameObject.layer;

            LayerManager.Initialize();
        }
    }
}