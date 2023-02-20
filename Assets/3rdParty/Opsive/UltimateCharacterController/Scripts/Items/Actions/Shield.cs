/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Items.AnimatorAudioStates;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Items.Actions
{
    /// <summary>
    /// The Shield will absorb damage applied to the character. It has its own strength factor so when too much damage has been taken it will no longer be effective.
    /// </summary>
    public class Shield : ItemAction
    {
        [Tooltip("Does the shield only protect the player when the character is aiming?")]
        [SerializeField] protected bool m_RequireAim;
        [Tooltip("Determines how much damage the shield absorbs. A value of 1 will absorb all of the damage, a value of 0 will not absorb any of the damage.")]
        [Range(0, 1)] [SerializeField] protected float m_AbsorptionFactor = 1;
        [Tooltip("Should the shield absorb damage caused by explosions?")]
        [SerializeField] protected bool m_AbsorbExplosions;
        [Tooltip("Should an impact be applied when the weapon is hit by another object?")]
        [SerializeField] protected bool m_ApplyImpact = true;
        [Tooltip("Specifies the animator and audio state for when the shield is impacted by another object.")]
        [SerializeField] protected AnimatorAudioStateSet m_ImpactAnimatorAudioStateSet = new AnimatorAudioStateSet();
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemImpactComplete animation event or wait for the specified duration before completing the impact.")]
        [SerializeField] protected AnimationEventTrigger m_ImpactCompleteEvent = new AnimationEventTrigger(false, 0.2f);
        [Tooltip("The name of the shield's durability attribute. When the durability reaches 0 the shield will not absorb any damage.")]
        [SerializeField] protected string m_DurabilityAttributeName = "Durability";
        [Tooltip("Should the item be dropped from the character when the durability is depleted?")]
        [SerializeField] protected bool m_DropWhenDurabilityDepleted;

        public bool RequireAim { get { return m_RequireAim; } set { m_RequireAim = value; } }
        public float AbsorptionFactor { get { return m_AbsorptionFactor; } set { m_AbsorptionFactor = value; } }
        public bool AbsorbExplosions { get { return m_AbsorbExplosions; } set { m_AbsorbExplosions = value; } }
        public bool ApplyImpact { get { return m_ApplyImpact; } set { m_ApplyImpact = value; } }
        public AnimatorAudioStateSet ImpactAnimatorAudioStateSet { get { return m_ImpactAnimatorAudioStateSet; } set { m_ImpactAnimatorAudioStateSet = value; } }
        public AnimationEventTrigger ImpactCompleteEvent { get { return m_ImpactCompleteEvent; } set { m_ImpactCompleteEvent = value; } }
        public string DurabilityAttributeName
        {
            get { return m_DurabilityAttributeName; }
            set
            {
                m_DurabilityAttributeName = value;
                if (Application.isPlaying) {
                    if (!string.IsNullOrEmpty(m_DurabilityAttributeName) && m_AttributeManager != null) {
                        m_DurabilityAttribute = m_AttributeManager.GetAttribute(m_DurabilityAttributeName);
                    } else {
                        m_DurabilityAttribute = null;
                    }
                }
            }
        }
        public bool DropWhenDurabilityDepleted { get { return m_DropWhenDurabilityDepleted; } set { m_DropWhenDurabilityDepleted = value; } }

        private AttributeManager m_AttributeManager;
        private Attribute m_DurabilityAttribute;
        private bool m_Aiming;
        private bool m_HasImpact;

        public float DurabilityValue { get { return (m_DurabilityAttribute != null ? m_DurabilityAttribute.Value : 0); } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_AttributeManager = GetComponent<AttributeManager>();
            if (!string.IsNullOrEmpty(m_DurabilityAttributeName)) {
                if (m_AttributeManager == null) {
                    Debug.LogError("Error: The shield " + m_GameObject.name + " has a durability attribute specified but no Attribute Manager component.");
                } else {
                    m_DurabilityAttribute = m_AttributeManager.GetAttribute(m_DurabilityAttributeName);

                    if (m_DurabilityAttribute != null) {
                        EventHandler.RegisterEvent(m_DurabilityAttribute, "OnAttributeReachedDestinationValue", DurabilityDepleted);
                    }
                }
            }

            m_ImpactAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(m_Item, m_Character.GetCachedComponent<UltimateCharacterLocomotion>());
            m_ImpactAnimatorAudioStateSet.Awake(m_Item.gameObject);
            EventHandler.RegisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
        }

        /// <summary>
        /// Returns the substate index that the item should be in.
        /// </summary>
        /// <returns>the substate index that the item should be in.</returns>
        public int GetItemSubstateIndex()
        {
            if (m_HasImpact) {
                return m_ImpactAnimatorAudioStateSet.GetItemSubstateIndex();
            }
            return -1;
        }

        /// <summary>
        /// Damages the shield.
        /// </summary>
        /// <param name="source">The object that is trying to damage the shield.</param>
        /// <param name="amount">The amount of damage to apply/</param>
        /// <returns>The amount of damage remaining which should be applied to the character.</returns>
        public float Damage(object source, float amount)
        {
            // The shield can't absorb damage if it requires the character to be aiming and the character isn't aiming.
            if (!m_Aiming && m_RequireAim) {
                return amount;
            }

            // The shield may not be able to absorb damage caused by explosions.
            if ((source is Objects.Explosion) && !m_AbsorbExplosions) {
                return amount;
            }

            if (m_ApplyImpact) {
                m_HasImpact = true;
                m_ImpactAnimatorAudioStateSet.StartStopStateSelection(true);
                m_ImpactAnimatorAudioStateSet.NextState();
                EventHandler.ExecuteEvent(m_Character, "OnShieldImpact", this, source);
            }

            // If the shield is invincible then no damage is applied to it and the resulting absorption factor should be returned.
            if (m_DurabilityAttribute == null) {
                return 0;
            }

            // If the shield's durability is depleted then the entire damage amount should be applied to the character.
            if (m_DurabilityAttribute.Value == 0) {
                return amount;
            }

            // Damage the shield and amount of damage which be applied to the character.
            var damageAmount = Mathf.Min(amount * m_AbsorptionFactor, m_DurabilityAttribute.Value);
            m_DurabilityAttribute.Value -= damageAmount;
            return amount - damageAmount;
        }

        /// <summary>
        /// The block animation has played - reset the impact.
        /// </summary>
        public void StopBlockImpact()
        {
            m_HasImpact = false;
            m_ImpactAnimatorAudioStateSet.StartStopStateSelection(false);
        }

        /// <summary>
        /// The Aim ability has started or stopped.
        /// </summary>
        /// <param name="start">Has the Aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        private void OnAim(bool aim, bool inputStart)
        {
            if (!inputStart) {
                return;
            }
            m_Aiming = aim;
        }

        /// <summary>
        /// The shield is no longer durable.
        /// </summary>
        private void DurabilityDepleted()
        {
            if (!m_DropWhenDurabilityDepleted) {
                return;
            }

            // Remove the item from the inventory before dropping it. This will ensure the dropped prefab does not contain any ItemType count so the
            // item can't be picked up again.
            m_Inventory.RemoveItem(m_Item.ItemType, m_Item.SlotID, false);
            m_Item.Drop(true);
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_ImpactAnimatorAudioStateSet.OnDestroy();
            EventHandler.UnregisterEvent<bool, bool>(m_Character, "OnBlockAbilityStart", OnAim);
            if (m_DurabilityAttribute != null) {
                EventHandler.UnregisterEvent(m_DurabilityAttribute, "OnAttributeReachedDestinationValue", DurabilityDepleted);
            }
            EventHandler.UnregisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
        }
    }
}