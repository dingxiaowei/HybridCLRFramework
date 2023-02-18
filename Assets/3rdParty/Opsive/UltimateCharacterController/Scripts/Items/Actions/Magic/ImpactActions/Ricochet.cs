/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.ImpactActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The Ricochet action will cast a new CastAction for nearby objects, creating a ricochet effect.
    /// </summary>
    public class Ricochet : ImpactAction
    {
        [Tooltip("The radius of the ricochet.")]
        [SerializeField] protected float m_Radius = 10;
        [Tooltip("The maximum number of ricochets that can occur from a single cast. Set to -1 to disable.")]
        [SerializeField] protected int m_MaxChainCount = 1;
        [Tooltip("The maximum number of objects that the ricochet can detect.")]
        [SerializeField] protected int m_MaxCollisionCount = 50;

        public float Radius { get { return m_Radius; } set { m_Radius = value; } }
        public int MaxChainCount { get { return m_MaxChainCount; } set { m_MaxChainCount = value; } }

        private CharacterLayerManager m_CharacterLayerManager;
        private Collider[] m_HitColliders;
        private Dictionary<uint, int> m_ChainCountMap = new Dictionary<uint, int>();

        /// <summary>
        /// Initializes the ImpactAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the ImpactAction belongs to.</param>
        /// <param name="index">The index of the ImpactAction.</param>
        public override void Initialize(GameObject character, MagicItem magicItem, int index)
        {
            base.Initialize(character, magicItem, index);

            m_CharacterLayerManager = character.GetCachedComponent<CharacterLayerManager>();
            m_HitColliders = new Collider[m_MaxCollisionCount];
        }

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="source">The object that caused the cast.</param>
        /// <param name="target">The object that was hit by the cast.</param>
        /// <param name="hit">The raycast that caused the impact.</param>
        protected override void ImpactInternal(uint castID, GameObject source, GameObject target, RaycastHit hit)
        {
            // Prevent the ricochet from bouncing between too many objects.
            if (m_ChainCountMap.TryGetValue(castID, out var count)) {
                if (m_MaxChainCount != -1 && count >= m_MaxChainCount) {
                    return;
                }
                m_ChainCountMap[castID] = count + 1;
            } else {
                m_ChainCountMap.Add(castID, 1);
            }

            var hitCount = Physics.OverlapSphereNonAlloc(hit.point, m_Radius, m_HitColliders, m_MagicItem.DetectLayers, QueryTriggerInteraction.Ignore);
#if UNITY_EDITOR
            if (hitCount == m_HitColliders.Length) {
                Debug.LogWarning("Warning: The hit count is equal to the max collider array size. This will cause objects to be missed. Consider increasing the max collision count size.");
            }
#endif

            // Perform the cast action in the direction of each hit object.
            for (int i = 0; i < hitCount; ++i) {
                var hitTransform = m_HitColliders[i].transform;
                if (HasImpacted(hitTransform)) {
                    continue;
                }

                // The object must be within view.
                var visibleTransform = false;
                Vector3 position;
                PivotOffset pivotOffset;
                if ((pivotOffset = hitTransform.gameObject.GetCachedComponent<PivotOffset>()) != null) {
                    position = hitTransform.TransformPoint(pivotOffset.Offset);
                } else {
                    position = hitTransform.position;
                }
                if (Physics.Linecast(hit.point, position, out var raycastHit, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                    if (raycastHit.transform.IsChildOf(hitTransform)) {
                        visibleTransform = true;
                    }
                }
                if (!visibleTransform) {
                    continue;
                }

                var direction = (position - hit.point).normalized;
                for (int j = 0; j < m_MagicItem.CastActions.Length; ++j) {
                    m_MagicItem.CastActions[j].Cast(source.transform, direction, position);
                }
            }
        }

        /// <summary>
        /// Resets the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast to reset.</param>
        public override void Reset(uint castID)
        {
            base.Reset(castID);

            if (m_ChainCountMap.ContainsKey(castID)) {
                m_ChainCountMap[castID] = 0;
            }
        }
    }
}