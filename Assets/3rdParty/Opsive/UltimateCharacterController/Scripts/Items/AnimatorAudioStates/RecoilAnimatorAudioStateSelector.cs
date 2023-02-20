/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Objects.ItemAssist;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Items.AnimatorAudioStates
{
    /// <summary>
    /// The RecoilAnimatorAudioState will return a Item Substate Index parameter based on the object's recoil state.
    /// If the character hit a blocking object then the block recoil state index value will be added to the current index value.
    /// </summary>
    public abstract class RecoilAnimatorAudioStateSelector : AnimatorAudioStateSelector
    {
        [Tooltip("The base index when the melee weapon is blocked. The state's index will be added to this value.")]
        [SerializeField] protected int m_BlockedRecoilItemSubstateIndex;

        protected int m_HitColliderCount;
        protected Collider[] m_HitColliders;
        protected int m_UseStateIndex;

        /// <summary>
        /// Moves to the next state.
        /// </summary>
        /// <param name="hitCollidersCount">The number of colliders that were hit.</param>
        /// <param name="hitColliders">The colliders that were hit.</param>
        /// <param name="useStateIndex">The index that was played by the use state.</param>
        public virtual void NextState(int hitColliderCount, Collider[] hitColliders, int useStateIndex)
        {
            m_HitColliderCount = hitColliderCount;
            m_HitColliders = hitColliders;
            m_UseStateIndex = useStateIndex;
            NextState();
        }

        /// <summary>
        /// Returns an additional value that should be added to the Item Substate Index.
        /// </summary>
        /// <returns>An additional value that should be added to the Item Substate Index.</returns>
        public override int GetAdditionalItemSubstateIndex()
        {
            if (IsBlocked()) {
                return m_BlockedRecoilItemSubstateIndex;
            }
            return base.GetAdditionalItemSubstateIndex();
        }

        /// <summary>
        /// Is the item currently being blocked by an object that should cause recoil?
        /// </summary>
        /// <returns>True if the item is currently being blocked by an object that should cause recoil.</returns>
        private bool IsBlocked()
        {
            for (int i = 0; i < m_HitColliderCount; ++i) {
                ShieldCollider shieldCollider;
                var hitGameObject = m_HitColliders[i].gameObject;
                if ((shieldCollider = hitGameObject.GetCachedComponent<ShieldCollider>()) != null) {
                    if (shieldCollider.Shield.DurabilityValue > 0) {
                        return true;
                    }
                } else if (hitGameObject.GetCachedComponent<RecoilObject>() != null) {
                    return true;
                }
            }
            return false;
        }
    }
}