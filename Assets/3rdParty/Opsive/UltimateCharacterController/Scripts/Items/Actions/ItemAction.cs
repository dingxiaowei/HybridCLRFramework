/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Inventory;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking;
    using Opsive.UltimateCharacterController.Networking.Character;
#endif
    using Opsive.UltimateCharacterController.StateSystem;
    using UnityEngine;

    /// <summary>
    /// An ItemAction is any item that can be interacted with by the character.
    /// </summary>
    public abstract class ItemAction : StateBehavior
    {
        [Tooltip("The ID of the action. Used with the perspective properties and item abilities to allow multiple actions to exist on the same item.")]
        [SerializeField] protected int m_ID;

        [NonSerialized] public int ID { get { return m_ID; } set { m_ID = value; } }

        protected GameObject m_GameObject;
        protected Item m_Item;
        protected InventoryBase m_Inventory;
        protected GameObject m_Character;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        protected INetworkInfo m_NetworkInfo;
        protected INetworkCharacter m_NetworkCharacter;
#endif

        protected ItemPerspectiveProperties m_FirstPersonPerspectiveProperties;
        protected ItemPerspectiveProperties m_ThirdPersonPerspectiveProperties;
        protected ItemPerspectiveProperties m_ActivePerspectiveProperties;

        public Item Item { get { return m_Item; } }
        public ItemPerspectiveProperties FirstPersonPerspectiveProperties { get { return m_FirstPersonPerspectiveProperties; } }
        public ItemPerspectiveProperties ThirdPersonPerspectiveProperties { get { return m_ThirdPersonPerspectiveProperties; } }
        public ItemPerspectiveProperties ActivePerspectiveProperties { get { return m_ActivePerspectiveProperties; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_Item = m_GameObject.GetCachedComponent<Item>();
            var characterLocomotion = m_GameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            m_Character = characterLocomotion.gameObject;
            m_Inventory = m_Character.GetCachedComponent<InventoryBase>();
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            m_NetworkInfo = m_Character.GetCachedComponent<INetworkInfo>();
            m_NetworkCharacter = m_Character.GetCachedComponent<INetworkCharacter>();
            if (m_NetworkInfo != null && m_NetworkCharacter == null) {
                Debug.LogError("Error: The character " + m_Character.name + " must have a NetworkCharacter component.");
            }
#endif

            var perspectiveProperties = GetComponents<ItemPerspectiveProperties>();
            for (int i = 0; i < perspectiveProperties.Length; ++i) {
                // The perspective properties Action ID must match. The ID allows multiple ItemActions/PerpsectiveProperties to be added to the same item.
                // An action ID of -1 can be used with any action.
                if (m_ID != perspectiveProperties[i].ActionID && perspectiveProperties[i].ActionID != -1) {
                    continue;
                }
                if (perspectiveProperties[i].FirstPersonItem) {
                    m_FirstPersonPerspectiveProperties = perspectiveProperties[i];
                } else {
                    m_ThirdPersonPerspectiveProperties = perspectiveProperties[i];
                }
            }
            m_ActivePerspectiveProperties = characterLocomotion.FirstPersonPerspective ? m_FirstPersonPerspectiveProperties : m_ThirdPersonPerspectiveProperties;

            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }

        /// <summary>
        /// Initializes any values that require on other components to first initialize.
        /// </summary>
        protected virtual void Start()
        {
            if (m_ActivePerspectiveProperties == null) {
                var characterLocomotion = m_Character.GetCachedComponent<UltimateCharacterLocomotion>();
                m_ActivePerspectiveProperties = characterLocomotion.FirstPersonPerspective ? m_FirstPersonPerspectiveProperties : m_ThirdPersonPerspectiveProperties;
            }
        }

        /// <summary>
        /// The item has been picked up by the character.
        /// </summary>
        public virtual void Pickup() { }

        /// <summary>
        /// Can the visible object be activated? An example of when it shouldn't be activated is when a grenade can be thrown but it is not the primary item
        /// so it shouldn't be thrown until after the throw action has started.
        /// </summary>
        /// <returns>True if the visible object can be activated.</returns>
        public virtual bool CanActivateVisibleObject() { return true; }

        /// <summary>
        /// The item will be equipped.
        /// </summary>
        public virtual void WillEquip() { }

        /// <summary>
        /// The item has been equipped by the character.
        /// </summary>
        public virtual void Equip() { }

        /// <summary>
        /// The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="inFirstPerson">Is the camera in a first person view?</param>
        protected virtual void OnChangePerspectives(bool firstPersonPerspective)
        {
            m_ActivePerspectiveProperties = firstPersonPerspective ? m_FirstPersonPerspectiveProperties : m_ThirdPersonPerspectiveProperties;
        }

        /// <summary>
        /// The item has started to be unequipped by the character.
        /// </summary>
        public virtual void StartUnequip() { }

        /// <summary>
        /// The item has been unequipped by the character.
        /// </summary>
        public virtual void Unequip() { }

        /// <summary>
        /// The item has been removed by the character.
        /// </summary>
        public virtual void Remove() { }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }
    }
}