/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.Items
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Items;
    using UnityEngine;

    /// <summary>
    /// Component which represents the item object actually rendererd.
    /// </summary>
    public class ThirdPersonPerspectiveItem : PerspectiveItem
    {
        [Tooltip("Should the Object object be spawned based on the character's humanoid bone?")]
        [SerializeField] protected bool m_UseParentHumanoidBone = true;
        [Tooltip("If using the humanoid bone, specifies which bone to use.")]
        [SerializeField] protected HumanBodyBones m_ParentHumanoidBone = HumanBodyBones.RightHand;
        [Tooltip("The location of the non-dominant hand which should be placed by the IK implementation.")]
        [SerializeField] protected Transform m_NonDominantHandIKTarget;
        [Tooltip("The location of the non-dominant hand hint which should be placed by the IK implementation.")]
        [SerializeField] protected Transform m_NonDominantHandIKTargetHint;
        [Tooltip("The transform that the item should be holstered to when unequipped.")]
        [SerializeField] protected Transform m_HolsterTarget;
        [Tooltip("The ID of the ObjectIdentifier component that the item should be holstered to when unequipped. This id will be used when holster target is null and the the ID is -1.")]
        [SerializeField] protected int m_HolsterID = -1;

        [NonSerialized] public Transform NonDominantHandIKTarget { get { return m_NonDominantHandIKTarget; } set { m_NonDominantHandIKTarget = value; } }
        [NonSerialized] public Transform NonDominantHandIKTargetHint { get { return m_NonDominantHandIKTargetHint; } set { m_NonDominantHandIKTargetHint = value; } }
        [NonSerialized] public Transform HolsterTarget { get { return m_HolsterTarget; } set { m_HolsterTarget = value; } }

        private CharacterIKBase m_CharacterIK;
        private Transform m_ParentBone;
        private Transform m_ObjectTransform;
        private Transform m_StartParentTransform;
        private Vector3 m_StartLocalPosition;
        private Quaternion m_StartLocalRotation;
        private bool m_PickedUp;

        public override bool FirstPersonItem { get { return false; } }

        /// <summary>
        /// Initialize the perspective item.
        /// </summary>
        /// <param name="character">The character GameObject that the item is parented to.</param>
        /// <returns>True if the item was initialized successfully.</returns>
        public override bool Initialize(GameObject character)
        {
            if (!base.Initialize(character)) {
                return false;
            }

            m_CharacterIK = m_Character.GetCachedComponent<CharacterIKBase>();

            if (m_Object != null) {
                m_ObjectTransform = m_Object.transform;
                m_StartParentTransform = m_ObjectTransform.parent; // Represents the Items GameObject.
                m_StartLocalPosition = m_ObjectTransform.localPosition;
                m_StartLocalRotation = m_ObjectTransform.localRotation;
                m_ParentBone = m_StartParentTransform.parent; // Represents the bone that the item is equipped to.
            }

            // If the holster ID isn't -1 then the HolsterTarget reference will contain the Transform that the item should be attached to.
            if (m_HolsterTarget == null && m_HolsterID != -1) {
                var objectIDs = m_Character.GetComponentsInChildren<Objects.ObjectIdentifier>();
                for (int i = 0; i < objectIDs.Length; ++i) {
                    if (m_HolsterID == objectIDs[i].ID) {
                        m_HolsterTarget = objectIDs[i].transform;
                        break;
                    }
                }
            }

            if (m_HolsterTarget != null) {
                // The holster target will be enabled when the item is picked up.
                m_HolsterTarget.gameObject.SetActive(false);
            }

            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_Character, "OnRespawn", OnRespawn);
            return true;
        }

        /// <summary>
        /// Returns the parent that the VisibleItem object should spawn at.
        /// </summary>
        /// <param name="character">The character that the item should spawn under.</param>
        /// <param name="slotID">The character slot that the VisibleItem object should spawn under.</param>
        /// <param name="parentToItemSlotID">Should the object be parented to the item slot ID?</param>
        /// <returns>The parent that the VisibleItem object should spawn at.</returns>
        protected override Transform GetSpawnParent(GameObject character, int slotID, bool parentToItemSlotID)
        {
            Transform parent = null;
            // If using a humanoid bone then use the character's Animator to find the bone and then get the ItemPlacement component from that bone.
            if (m_UseParentHumanoidBone) {
                var characterAnimator = character.GetCachedComponent<Animator>();
                if (characterAnimator != null) {
                    parent = characterAnimator.GetBoneTransform(m_ParentHumanoidBone);
                    if (parent != null) {
                        var itemSlot = parent.GetComponentInChildren<ItemSlot>(true);
                        if (itemSlot != null) {
                            parent = itemSlot.transform;
                        }
                    }
                }
            }

            // Fallback to using the ItemSlot ID.
            if (parent == null) {
                var itemSlots = character.GetComponentsInChildren<ItemSlot>(true);
                for (int i = 0; i < itemSlots.Length; ++i) {
                    if (itemSlots[i].ID == slotID) {
                        parent = itemSlots[i].transform;
                        break;
                    }
                }
            }

            return parent;
        }

        /// <summary>
        /// Is the VisibleItem active?
        /// </summary>
        /// <returns>True if the VisibleItem is active.</param>
        public override bool IsActive()
        {
            // If a holster target is specified then the VisibleItem will never completely deactivate. Determine if it is active by the Transform parent.
            if (m_HolsterTarget != null) {
                return m_ObjectTransform.parent == m_StartParentTransform;
            } else {
                if (m_Object == null) {
                    return m_Item.VisibleObjectActive;
                }
                return base.IsActive();
            }
        }

        /// <summary>
        /// Activates or deactivates the VisibleItem.
        /// </summary>
        /// <param name="active">Should the VisibleItem be activated?</param>
        public override void SetActive(bool active)
        {
            // If a holster target is specified then deactivating the VisibleItem will mean setting the parent transform of the object to that holster target.
            if (m_HolsterTarget != null) {
                if (active) {
                    m_ObjectTransform.parent = m_StartParentTransform;
                    m_ObjectTransform.localPosition = m_StartLocalPosition;
                    m_ObjectTransform.localRotation = m_StartLocalRotation;
                } else {
                    m_ObjectTransform.parent = m_HolsterTarget;
                    m_ObjectTransform.localPosition = Vector3.zero;
                    m_ObjectTransform.localRotation = Quaternion.identity;
                }
            } else if (m_Object != null) {
                // Allow the base object to activate or deactivate the actual object.
                base.SetActive(active);
            }

            // When the item activates or deactivates it should specify the IK target of the non-dominant hand (if any).
            if (m_CharacterIK != null) {
                m_CharacterIK.SetItemIKTargets(active ? m_ObjectTransform : null, m_ParentBone, active ? m_NonDominantHandIKTarget : null, active ? m_NonDominantHandIKTargetHint : null);
            }
        }

        /// <summary>
        /// The VisibleItem has been picked up by the character.
        /// </summary>
        public override void Pickup()
        {
            base.Pickup();

            m_PickedUp = true;

            // The object should always be active if it is holstered.
            if (m_HolsterTarget != null) {
                m_Object.SetActive(true);

                // The holster target will be disabled until the character picks up the item.
                m_HolsterTarget.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// The item has been removed.
        /// </summary>
        public override void Remove()
        {
            base.Remove();

            m_PickedUp = false;

            // The object should always be active if it is holstered.
            if (m_HolsterTarget != null) {
                m_HolsterTarget.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            // When the character dies disable the holster. Do not disable the item's object because that may not be activated again
            // when the character respawns, whereas the holster target should always be activated since it's an empty GameObject.
            if (m_HolsterTarget != null && m_PickedUp) {
                m_HolsterTarget.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            if (m_HolsterTarget != null && m_PickedUp) {
                m_HolsterTarget.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Called when the item is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_Character == null) {
                return;
            }

            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_Character, "OnRespawn", OnRespawn);
        }
    }
}