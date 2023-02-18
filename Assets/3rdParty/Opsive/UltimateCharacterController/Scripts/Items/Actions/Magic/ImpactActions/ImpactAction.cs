/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.ImpactActions
{
    using Opsive.UltimateCharacterController.StateSystem;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// Impact Actions will perform an action when the cast has impacted another object.
    /// </summary>
    [System.Serializable]
    [UnityEngine.Scripting.Preserve]
    [AllowDuplicateTypes]
    public abstract class ImpactAction : StateObject
    {
        protected MagicItem m_MagicItem;
        protected int m_Index;

        private Dictionary<uint, HashSet<Transform>> m_ImpactedObjectsMap = new Dictionary<uint, HashSet<Transform>>();
        private HashSet<Transform> m_ImpactedObjects;

        /// <summary>
        /// Initializes the ImpactAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the ImpactAction belongs to.</param>
        /// <param name="index">The index of the ImpactAction.</param>
        public virtual void Initialize(GameObject character, MagicItem magicItem, int index)
        {
            base.Initialize(character);

            m_MagicItem = magicItem;
            m_Index = index;
        }

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="source">The object that caused the cast.</param>
        /// <param name="target">The object that was hit by the cast.</param>
        /// <param name="hit">The raycast that caused the impact.</param>
        public void Impact(uint castID, GameObject source, GameObject target, RaycastHit hit)
        {
            if (!m_MagicItem.ContinuousCast) {
                if (!m_ImpactedObjectsMap.TryGetValue(castID, out m_ImpactedObjects)) {
                    m_ImpactedObjects = new HashSet<Transform>();
                    m_ImpactedObjectsMap.Add(castID, m_ImpactedObjects);
                }
                // Don't call impact if the object has already been impacted by the same id.
                if (m_ImpactedObjects.Contains(target.transform)) {
                    return;
                }
                m_ImpactedObjects.Add(target.transform);
            }

            ImpactInternal(castID, source, target, hit);
        }

        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast spawn.</param>
        /// <param name="source">The object that caused the cast.</param>
        /// <param name="target">The object that was hit by the cast.</param>
        /// <param name="hit">The raycast that caused the impact.</param>
        protected abstract void ImpactInternal(uint castID, GameObject source, GameObject target, RaycastHit hit);

        /// <summary>
        /// Has the specified object been impacted?
        /// </summary>
        /// <param name="obj">The object that may have been impacted.</param>
        /// <returns>True if the specified object has been impacted.</returns>
        protected bool HasImpacted(Transform obj)
        {
            if (m_ImpactedObjects == null) {
                return false;
            }
            return m_ImpactedObjects.Contains(obj);
        }

        /// <summary>
        /// Resets the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast to reset.</param>
        public virtual void Reset(uint castID)
        {
            if (m_ImpactedObjectsMap.TryGetValue(castID, out var impactedObjects)) {
                impactedObjects.Clear();
            }
        }

        /// <summary>
        /// The action has been destroyed.
        /// </summary>
        public virtual void OnDestroy() { }
    }
}