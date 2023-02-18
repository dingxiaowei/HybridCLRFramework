/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking.Game;
#endif
    using UnityEngine;

    /// <summary>
    /// An item that can shine a light.
    /// </summary>
    public class Flashlight : UsableItem
    {
        [Tooltip("The battery attribute that should be modified when the flashlight is active.")]
        [HideInInspector] [SerializeField] protected Traits.AttributeModifier m_BatteryModifier = new Traits.AttributeModifier("Battery", 0, Traits.Attribute.AutoUpdateValue.Decrease);

        public Traits.AttributeModifier BatteryModifier { get { return m_BatteryModifier; } set { m_BatteryModifier = value; } }

        private IFlashlightPerspectiveProperties m_FlashlightPerpectiveProperties;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_FlashlightPerpectiveProperties = m_ActivePerspectiveProperties as IFlashlightPerspectiveProperties;

            if (m_BatteryModifier != null) {
                if (m_BatteryModifier.Initialize(m_GameObject)) {
                    EventHandler.RegisterEvent(m_BatteryModifier.Attribute, "OnAttributeReachedDestinationValue", OnBatteryEmpty);
                }
            }
        }

        /// <summary>
        /// Initialize the visible object transform.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (m_FlashlightPerpectiveProperties == null) {
                m_FlashlightPerpectiveProperties = m_ActivePerspectiveProperties as IFlashlightPerspectiveProperties;

                if (m_FlashlightPerpectiveProperties == null) {
                    Debug.LogError("Error: The First/Third Person Flashlight Item Properties component cannot be found for the Item " + name + "." +
                                   "Ensure the component exists and the component's Action ID matches the Action ID of the Item (" + m_ID + ")");
                }
            }
        }

        /// <summary>
        /// Returns the ItemIdentifier which can be used by the item.
        /// </summary>
        /// <returns>The ItemIdentifier which can be used by the item.</returns>
        public override IItemIdentifier GetConsumableItemIdentifier()
        {
            return null;
        }

        /// <summary>
        /// Returns the amout of UsableItemIdentifier which has been consumed by the UsableItem.
        /// </summary>
        /// <returns>The amount consumed of the UsableItemIdentifier.</returns>
        public override int GetConsumableItemIdentifierAmount()
        {
            return -1;
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="itemAbility">The itemAbility that is trying to use the item.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanUseItem(ItemAbility itemAbility, UseAbilityState abilityState)
        {
            if (!base.CanUseItem(itemAbility, abilityState)) {
                return false;
            }

            // The flashlight can't be used if there is no battery left.
            if (m_BatteryModifier != null && !m_BatteryModifier.IsValid()) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="itemAbility">The item ability that is using the item.</param>
        public override void StartItemUse(ItemAbility itemAbility)
        {
            base.StartItemUse(itemAbility);

            ToggleFlashlight(!m_FlashlightPerpectiveProperties.Light.activeSelf);
        }

        /// <summary>
        /// Activates or deactives the flashlight.
        /// </summary>
        /// <param name="active">Should the flashlight be activated?</param>
        public void ToggleFlashlight(bool active)
        {
            m_FlashlightPerpectiveProperties.Light.SetActive(active);
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null) {
                if (!m_NetworkInfo.IsLocalPlayer()) {
                    return;
                }
                m_NetworkCharacter.ToggleFlashlight(this, active);
            }
#endif
            if (m_BatteryModifier != null) {
                m_BatteryModifier.EnableModifier(active);
            }
        }

        /// <summary>
        /// The flashlight battery is empty.
        /// </summary>
        private void OnBatteryEmpty()
        {
            ToggleFlashlight(false);
        }

        /// <summary>
        /// The item has started to be unequipped by the character.
        /// </summary>
        public override void StartUnequip()
        {
            base.StartUnequip();

            ToggleFlashlight(false);
        }

        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        protected override void OnChangePerspectives(bool firstPersonPerspective)
        {
            base.OnChangePerspectives(firstPersonPerspective);

            var active = m_FlashlightPerpectiveProperties.Light.activeSelf;
            m_FlashlightPerpectiveProperties.Light.SetActive(false);
            m_FlashlightPerpectiveProperties = m_ActivePerspectiveProperties as IFlashlightPerspectiveProperties;
            if (active) {
                m_FlashlightPerpectiveProperties.Light.SetActive(true);
            }
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_BatteryModifier != null && m_BatteryModifier.Attribute != null) {
                EventHandler.UnregisterEvent(m_BatteryModifier.Attribute, "OnAttributeReachedDestinationValue", OnBatteryEmpty);
            }
        }
    }
}