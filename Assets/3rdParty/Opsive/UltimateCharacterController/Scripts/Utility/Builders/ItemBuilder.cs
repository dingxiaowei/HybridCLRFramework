/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility.Builders
{
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEngine;

    /// <summary>
    /// Builds a new item.
    /// </summary>
    public class ItemBuilder
    {
        /// <summary>
        /// The type of action to create. 
        /// </summary>
        public enum ActionType {
            ShootableWeapon,    // The item uses a ShootableWeapon.
            MeleeWeapon,        // The item uses a MeleeWeapon.
            Shield,             // The item uses a Shield.
            MagicItem,          // The item uses a MagicItem.
            ThrowableItem,      // The item uses a ThrowableItem.
            GrenadeItem,        // The item uses a GrenadeItem.
            Flashlight,         // The item uses a Flashlight.
            Nothing             // The item doesn't have any actions.
        }

        /// <summary>
        /// Builds the item with the specified parameters.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <param name="itemDefinition">The ItemDefinition that the item uses (optional).</param>
        /// <param name="animatorItemID">The ID of the item within the animator.</param>
        /// <param name="character">The character that the item should be attached to (optional).</param>
        /// <param name="slotID">The ID of the slot that the item is parented to.</param>
        /// <param name="addToDefaultLoadout">Should the item be added to the character's default loadout?</param>
        /// <param name="addFirstPersonPerspective">Should the first person perspective be added?</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonObjectAnimatorController">A reference to the animator controller added to the first person object. Can be null.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item. Can be null.</param>
        /// <param name="firstPersonItemSlot">A reference to the ItemSlot to add the visible item to.</param>
        /// <param name="firstPersonVisibleItemAnimatorController">A reference to the animator controller added to the first person visible item. Can be null.</param>
        /// <param name="addThirdPersonPerspective">Should the third person perspective be added?</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        /// <param name="thirdPersonItemSlot">A reference to the ItemSlot to add the third person item to.</param>
        /// <param name="thirdPersonObjectAnimatorController">A reference to the animator controller added to the third person object. Can be null.</param>
        /// <param name="invisibleShadowCasterMaterial">A reference to the invisible shadow caster material. This is only used for first person characters.</param>
        /// <param name="actionType">The type of item to create.</param>
        /// <param name="actionItemDefinition">The ItemDefinition that the action uses (optional).</param>
        public static GameObject BuildItem(string name, ItemDefinitionBase itemDefinition, int animatorItemID, GameObject character, int slotID, bool addToDefaultLoadout, bool addFirstPersonPerspective,
            GameObject firstPersonObject, RuntimeAnimatorController firstPersonObjectAnimatorController, GameObject firstPersonVisibleItem, ItemSlot firstPersonItemSlot, 
            RuntimeAnimatorController firstPersonVisibleItemAnimatorController, bool addThirdPersonPerspective, GameObject thirdPersonObject, ItemSlot thirdPersonItemSlot, 
            RuntimeAnimatorController thirdPersonObjectAnimatorController, Material invisibleShadowCasterMaterial, ActionType actionType, ItemDefinitionBase actionItemDefinition)
        {
            var itemGameObject = new GameObject(name);
            var itemSlotID = (character == null || (firstPersonItemSlot == null && thirdPersonItemSlot == null)) ? slotID :
                                                    (firstPersonItemSlot != null ? firstPersonItemSlot.ID : thirdPersonItemSlot.ID);

            // If character is null then a prefab will be created.
            if (character != null) {
                // The attach to object must have an ItemPlacement component.
                var itemPlacement = character.GetComponentInChildren<ItemPlacement>();
                if (itemPlacement == null) {
                    Debug.LogError("Error: Unable to find the ItemPlacement component within " + character.name + ".");
                    return null;
                }

                // Organize the main item GameObject under the ItemPlacement GameObject.
                itemGameObject.transform.SetParentOrigin(itemPlacement.transform);

                // The item can automatically be added to the inventory's default loadout.
                if (itemDefinition != null && addToDefaultLoadout) {
                    var inventory = character.GetComponent<Inventory>();
                    var defaultLoadout = inventory.DefaultLoadout;
                    if (defaultLoadout == null) {
                        defaultLoadout = new ItemDefinitionAmount[0];
                    }
                    var hasItemDefinition = false;
                    for (int i = 0; i < defaultLoadout.Length; ++i) {
                        // If the ItemIdentifier has already been added then a new ItemIdentifier doesn't need to be added.
                        if (defaultLoadout[i].ItemDefinition == itemDefinition) {
                            defaultLoadout[i].Amount++;
                            hasItemDefinition = true;
                            break;
                        }
                    }
                    if (!hasItemDefinition) {
                        System.Array.Resize(ref defaultLoadout, defaultLoadout.Length + 1);
                        defaultLoadout[defaultLoadout.Length - 1] = new ItemDefinitionAmount(itemDefinition, 1);
                    }
                    // The actionItemIdentifier should also be added.
                    if (actionItemDefinition != null) {
                        hasItemDefinition = false;
                        for (int i = 0; i < defaultLoadout.Length; ++i) {
                            // If the ItemIdentifier has already been added then a new action ItemDefinition doesn't need to be added.
                            if (defaultLoadout[i].ItemDefinition == actionItemDefinition) {
                                hasItemDefinition = true;
                                break;
                            }
                        }

                        if (!hasItemDefinition) {
                            System.Array.Resize(ref defaultLoadout, defaultLoadout.Length + 1);
                            defaultLoadout[defaultLoadout.Length - 1] = new ItemDefinitionAmount(actionItemDefinition, 100);
                        }
                    }
                    inventory.DefaultLoadout = defaultLoadout;

                    // The ItemIdentifier should be added to the ItemSetManager as well.
                    var itemSetManager = character.GetComponent<ItemSetManager>();
                    if (itemSetManager != null && itemDefinition.GetItemCategory() != null) {
                        itemSetManager.Initialize(false);
                        var index = itemSetManager.CategoryToIndex(itemDefinition.GetItemCategory());

                        if (index > -1) {
                            var category = itemSetManager.CategoryItemSets[index];
                            hasItemDefinition = false;
                            for (int j = 0; j < category.ItemSetList.Count; ++j) {
                                if (category.ItemSetList[j].Slots[itemSlotID] == itemDefinition) {
                                    hasItemDefinition = true;
                                    break;
                                }
                            }

                            if (!hasItemDefinition) {
                                category.ItemSetList.Add(new ItemSet(Mathf.Max(inventory.SlotCount, itemSlotID + 1), itemSlotID, itemDefinition, null, string.Empty));
                            }
                        }
                    }
                }
            }
            var item = itemGameObject.AddComponent<Item>();
            item.ItemDefinition = itemDefinition;
            item.SlotID = itemSlotID;
            item.AnimatorItemID = animatorItemID;

#if FIRST_PERSON_CONTROLLER
            // Add the first person object.
            if (addFirstPersonPerspective) {
                AddFirstPersonObject(character, name, itemGameObject, ref firstPersonObject, firstPersonObjectAnimatorController, ref firstPersonVisibleItem, firstPersonItemSlot,
                                        firstPersonVisibleItemAnimatorController);
                // If the character doesn't have an animator then the item should be equipped by a timer.
                if (character != null && character.GetComponent<Animator>() == null) {
                    item.EquipEvent.WaitForAnimationEvent = false;
                }
            }
#endif

            // Add the third person object. The character will always have a third person object if the character has an animator.
            if (addThirdPersonPerspective) {
                AddThirdPersonObject(character, name, itemGameObject, ref thirdPersonObject, thirdPersonItemSlot, thirdPersonObjectAnimatorController, invisibleShadowCasterMaterial,
                                    !addFirstPersonPerspective || firstPersonObject != null || firstPersonVisibleItem != null);
            }

            // Add the specified action type.
            AddAction(itemGameObject, firstPersonObject, firstPersonVisibleItem, thirdPersonObject, actionType, actionItemDefinition);

            return itemGameObject;
        }

        /// <summary>
        /// Creates a GameObject as the child of the parent.
        /// </summary>
        /// <param name="name">The name of the GameObject.</param>
        /// <param name="parent">The parent of the new GameObject.</param>
        /// <returns>The Trasnform of the non duplicate GameObject.</returns>
        private static Transform CreateGameObject(string name, Transform parent)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParentOrigin(parent);
            return gameObject.transform;
        }

#if FIRST_PERSON_CONTROLLER
        /// <summary>
        /// Adds the first person object to the specified item.
        /// </summary>
        /// <param name="character">The character that the first person object is being added to.</param>
        /// <param name="name">The name of the item.</param>
        /// <param name="itemGameObject">A reference to the item's GameObject.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonObjectAnimatorController">A reference to the animator controller added to the first person object. Can be null.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item. Can be null.</param>
        /// <param name="firstPersonItemSlot">A reference to the ItemSlot to add the visible item to.</param>
        /// <param name="firstPersonVisibleItemAnimatorController">A reference to the animator controller added to the first person visible item. Can be null.</param>
        public static void AddFirstPersonObject(GameObject character, string name, GameObject itemGameObject, 
            ref GameObject firstPersonObject, RuntimeAnimatorController firstPersonObjectAnimatorController, ref GameObject firstPersonVisibleItem, ItemSlot firstPersonItemSlot,
            RuntimeAnimatorController firstPersonVisibleItemAnimatorController)
        {
            var parentFirstPersonObject = false;
            if (firstPersonObject != null && (character == null || !firstPersonObject.transform.IsChildOf(character.transform))) {
                parentFirstPersonObject = true;
                var origFirstPersonPerspectiveItem = firstPersonVisibleItem;
                var visibleItemName = string.Empty;
                var visibleItemSearchName = string.Empty;
                // The visible item is a child of the object. When the object is instantiated the new visible item should be found again.
                // This is done by giving the visible item a unique name.
                if (firstPersonVisibleItem != null) {
                    visibleItemName = firstPersonVisibleItem.name;
                    firstPersonVisibleItem.name += Random.value.ToString();

                    // Remember the path so the newly created visible item can be found again.
                    var parent = firstPersonVisibleItem.transform.parent;
                    visibleItemSearchName = firstPersonVisibleItem.name;
                    while (parent != firstPersonObject.transform && parent != null) {
                        visibleItemSearchName = parent.name + "/" + visibleItemSearchName;
                        parent = parent.parent;
                    }
                }

                firstPersonObject = GameObject.Instantiate(firstPersonObject);
                if (character == null) {
                    firstPersonObject.name = "First Person " + name;
                } else {
                    firstPersonObject.name = firstPersonObject.name.Substring(0, firstPersonObject.name.Length - 7); // Remove "(Clone)".
                }

                AddFirstPersonArms(character, firstPersonObject, firstPersonObjectAnimatorController);

                // An ItemSlot must also be added to the base object if no visible item exists.
                if (firstPersonVisibleItem == null) {
                    firstPersonObject.AddComponent<ItemSlot>();
                }

                // A new visible item would have been created.
                if (firstPersonVisibleItem != null) {
                    var foundVisibleItem = firstPersonObject.transform.Find(visibleItemSearchName);
                    if (foundVisibleItem != null) {
                        // The newly created visible item is now the main visible item.
                        firstPersonVisibleItem = foundVisibleItem.gameObject;
                    } else {
                        // The visible item may not have been a child of the first person object GameObject.
                        firstPersonVisibleItem = GameObject.Instantiate(firstPersonVisibleItem);

                        // The ItemSlot reference also needs to be updated.
                        var itemSlots = firstPersonObject.GetComponentsInChildren<ItemSlot>();
                        for (int i = 0; i < itemSlots.Length; ++i) {
                            if (itemSlots[i].ID == firstPersonItemSlot.ID) {
                                firstPersonItemSlot = itemSlots[i];
                                break;
                            }
                        }
                        firstPersonVisibleItem.transform.SetParentOrigin(firstPersonItemSlot.transform);
                    }
                    origFirstPersonPerspectiveItem.name = firstPersonVisibleItem.name = visibleItemName;
                }
            } else if (firstPersonVisibleItem != null) {
                firstPersonVisibleItem = GameObject.Instantiate(firstPersonVisibleItem);
                firstPersonVisibleItem.name = (character == null ? "First Person " : "") + name;
            }
            var perspectiveItem = itemGameObject.AddComponent<FirstPersonController.Items.FirstPersonPerspectiveItem>();
            perspectiveItem.Object = firstPersonObject;
            perspectiveItem.VisibleItem = firstPersonVisibleItem;

            if (firstPersonVisibleItem != null) {
                if (firstPersonVisibleItem.GetComponent<AudioSource>() == null) {
                    var audioSource = firstPersonVisibleItem.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 1;
                    audioSource.maxDistance = 20;
                }
            }

            // The visible item can use an animator.
            if (firstPersonVisibleItemAnimatorController != null && firstPersonVisibleItem != null) {
                Animator animator;
                if ((animator = firstPersonVisibleItem.GetComponent<Animator>()) == null) {
                    animator = firstPersonVisibleItem.AddComponent<Animator>();
                }
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.runtimeAnimatorController = firstPersonVisibleItemAnimatorController;
                if (firstPersonVisibleItem.GetComponent<ChildAnimatorMonitor>() == null) {
                    firstPersonVisibleItem.AddComponent<ChildAnimatorMonitor>();
                }
            }

            Transform parentTransform = null;
            if (character != null) {
                // The object should be a child of the First Person Objects GameObject.
                if (firstPersonObject != null && parentFirstPersonObject) {
                    var firstPersonObjects = character.GetComponentInChildren<FirstPersonController.Character.FirstPersonObjects>();
                    if (firstPersonObjects == null) {
                        Debug.LogError("Error: Unable to find the FirstPersonObjects component within " + character.name + ".");
                        return;
                    } else {
                        parentTransform = firstPersonObjects.transform;
                    }
                } else if (firstPersonVisibleItem != null) {
                    parentTransform = firstPersonItemSlot.transform;
                }
            } else {
                // The object should be a child of the item GameObject.
                parentTransform = itemGameObject.transform;
            }

            // Assign the transform. The object will contain the visible item if it exists.
            var obj = firstPersonObject && parentFirstPersonObject ? firstPersonObject : firstPersonVisibleItem;
            if (obj != null) {
                obj.transform.SetParentOrigin(parentTransform);

                // The item's object should be on the first person overlay layer so it'll render over all other objects.
                obj.transform.SetLayerRecursively(LayerManager.Overlay);
            } else if (firstPersonVisibleItem != null) {
                firstPersonVisibleItem.transform.SetLayerRecursively(LayerManager.Overlay);
            }

            // Add any properties for actions which have already been added.
            AddPropertiesToActions(itemGameObject, firstPersonObject, firstPersonVisibleItem, null);
        }

        /// <summary>
        /// Adds the FirstPersonBaseObject to the arms.
        /// </summary>
        /// <param name="character">The character that contains the FirstPersonObject.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonObjectAnimatorController">A reference to the animator controller added to the first person object. Can be null.</param>
        public static void AddFirstPersonArms(GameObject character, GameObject firstPersonObject, RuntimeAnimatorController firstPersonObjectAnimatorController)
        {
            var maxID = -1;
            if (character != null && firstPersonObject.GetComponent<FirstPersonController.Character.Identifiers.FirstPersonBaseObject>() == null) {
                // The base object ID must be unique.
                var baseObjects = character.GetComponentsInChildren<FirstPersonController.Character.Identifiers.FirstPersonBaseObject>();
                for (int i = 0; i < baseObjects.Length; ++i) {
                    if (baseObjects[i].ID > maxID) {
                        maxID = baseObjects[i].ID;
                    }
                }
            }
            var baseObject = firstPersonObject.AddComponent<FirstPersonController.Character.Identifiers.FirstPersonBaseObject>();
            baseObject.ID = maxID + 1;
            firstPersonObject.transform.SetLayerRecursively(LayerManager.Overlay);

            if (firstPersonObjectAnimatorController != null) {
                Animator animator;
                if ((animator = firstPersonObject.GetComponent<Animator>()) == null) {
                    animator = firstPersonObject.AddComponent<Animator>();
                }
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.runtimeAnimatorController = firstPersonObjectAnimatorController;
                if (firstPersonObject.GetComponent<ChildAnimatorMonitor>() == null) {
                    firstPersonObject.AddComponent<ChildAnimatorMonitor>();
                }
            }
        }

        /// <summary>
        /// Removes the third person item.
        /// </summary>
        /// <param name="firstPersonPerspectiveItem">The item to remove.</param>
        public static void RemoveFirstPersonObject(FirstPersonController.Items.FirstPersonPerspectiveItem firstPersonPerspectiveItem)
        {
            // Remove any properties which use the first person object.
            var itemProperties = firstPersonPerspectiveItem.GetComponents<FirstPersonController.Items.FirstPersonItemProperties>();
            for (int i = itemProperties.Length - 1; i > -1; --i) {
                Object.DestroyImmediate(itemProperties[i], true);
            }

            if (firstPersonPerspectiveItem.VisibleItem != null) {
                Object.DestroyImmediate(firstPersonPerspectiveItem.VisibleItem, true);
            }
            Object.DestroyImmediate(firstPersonPerspectiveItem, true);
        }
#endif

        /// <summary>
        /// Adds the third person object to the specified item.
        /// </summary>
        /// <param name="character">The character that the third person object is being added to.</param>
        /// <param name="name">The name of the item.</param>
        /// <param name="itemGameObject">A reference to the item's GameObject.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        /// <param name="thirdPersonItemSlot">A reference to the ItemSlot to add the third person item to.</param>
        /// <param name="thirdPersonObjectAnimatorController">A reference to the animator controller added to the third person object. Can be null.</param>
        /// <param name="invisibleShadowCasterMaterial">A reference to the invisible shadow caster material. This is only used for first person characters.</param>
        /// <param name="defaultAddThirdPersonObject">Should the ThirdPersonObject component be added to the object?</param>
        public static void AddThirdPersonObject(GameObject character, string name, GameObject itemGameObject, ref GameObject thirdPersonObject, ItemSlot thirdPersonItemSlot,
                                                RuntimeAnimatorController thirdPersonObjectAnimatorController, Material invisibleShadowCasterMaterial, bool defaultAddThirdPersonObject)
        {
            var visibleItem = itemGameObject.AddComponent<ThirdPersonController.Items.ThirdPersonPerspectiveItem>();
            if (thirdPersonObject != null) {
                thirdPersonObject = GameObject.Instantiate(thirdPersonObject);
                thirdPersonObject.name = (character == null ? "Third Person " : "") + name;
                visibleItem.Object = thirdPersonObject;

                var addThirdPersonObject = defaultAddThirdPersonObject;
#if THIRD_PERSON_CONTROLLER
                if (character != null && !addThirdPersonObject) {
                    var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
                    var movementTypes = characterLocomotion.GetSerializedMovementTypes();
                    if (movementTypes != null) {
                        for (int i = 0; i < movementTypes.Length; ++i) {
                            if (characterLocomotion.MovementTypes[i].GetType().FullName.Contains("ThirdPerson")) {
                                addThirdPersonObject = true;
                                break;
                            }
                        }
                    }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                    var networkInfo = character.GetComponent<Networking.INetworkInfo>();
                    if (networkInfo != null) {
                        addThirdPersonObject = true;
                    }
#endif
                }
#else
                addThirdPersonObject = false;
#endif

                if (addThirdPersonObject) {
                    // The ThirdPersonObject component is added so the PerspectiveMonitor knows what objects should use the invisible shadow caster material.
                    thirdPersonObject.AddComponent<Character.Identifiers.ThirdPersonObject>();
                } else {
                    // If the ThirdPersonObject isn't added then the renderer should be directly attached.
                    var renderers = thirdPersonObject.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; ++i) {
                        var materials = renderers[i].sharedMaterials;
                        for (int j = 0; j < materials.Length; ++j) {
                            materials[j] = invisibleShadowCasterMaterial;
                        }
                        renderers[i].sharedMaterials = materials;
                    }
                }

                if (thirdPersonObject.GetComponent<AudioSource>() == null) {
                    var audioSource = thirdPersonObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 1;
                    audioSource.maxDistance = 20;
                }
                // Optionally add the animator.
                if (thirdPersonObjectAnimatorController != null) {
                    Animator animator;
                    if ((animator = thirdPersonObject.GetComponent<Animator>()) == null) {
                        animator = thirdPersonObject.AddComponent<Animator>();
                    }
                    animator.applyRootMotion = false;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    animator.runtimeAnimatorController = thirdPersonObjectAnimatorController;
                    if (thirdPersonObject.GetComponent<ChildAnimatorMonitor>() == null) {
                        thirdPersonObject.AddComponent<ChildAnimatorMonitor>();
                    }
                }
                Transform parentTransform = null;
                if (character != null) {
                    parentTransform = thirdPersonItemSlot.transform;
                } else {
                    // The object should be a child of the item GameObject.
                    parentTransform = itemGameObject.transform;
                }

                // Assign the transform position and layer.
                thirdPersonObject.transform.SetParentOrigin(parentTransform);
                thirdPersonObject.transform.SetLayerRecursively(LayerManager.SubCharacter);
            }

            // Add any properties for actions which have already been added.
            AddPropertiesToActions(itemGameObject, null, null, thirdPersonObject);
        }

        /// <summary>
        /// Adds the properties to any actions already created.
        /// </summary>
        /// <param name="itemGameObject">A reference to the item's GameObject.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        private static void AddPropertiesToActions(GameObject itemGameObject, GameObject firstPersonObject, GameObject firstPersonVisibleItem, GameObject thirdPersonObject)
        {
            var actions = itemGameObject.GetComponents<ItemAction>();
            for (int i = 0; i < actions.Length; ++i) {
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
                if (actions[i] is ShootableWeapon) {
                    AddShootableWeaponProperties(itemGameObject, actions[i].ID, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    continue;
                }
#endif
#if ULTIMATE_CHARACTER_CONTROLLER_MELEE
                if (actions[i] is MeleeWeapon) {
                    AddMeleeWeaponProperties(itemGameObject, actions[i].ID, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    continue;
                }
#endif
                if (actions[i] is GrenadeItem) {
                    AddGrenadeItemProperties(itemGameObject, actions[i].ID, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    continue;
                }
                if (actions[i] is ThrowableItem) {
                    AddThrowableItemProperties(itemGameObject, actions[i].ID, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    continue;
                }
                if (actions[i] is Flashlight) {
                    AddFlashlightProperties(itemGameObject, actions[i].ID, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    continue;
                }
            }
        }

        /// <summary>
        /// Removes the third person item.
        /// </summary>
        /// <param name="thirdPersonVisibleItem">The item to remove.</param>
        public static void RemoveThirdPersonObject(ThirdPersonController.Items.ThirdPersonPerspectiveItem thirdPersonVisibleItem)
        {
            // Remove any properties which use the third person object.
            var itemProperties = thirdPersonVisibleItem.GetComponents<ThirdPersonController.Items.ThirdPersonItemProperties>();
            for (int i = itemProperties.Length - 1; i > -1; --i) {
                Object.DestroyImmediate(itemProperties[i], true);
            }

            Object.DestroyImmediate(thirdPersonVisibleItem.Object, true);
            Object.DestroyImmediate(thirdPersonVisibleItem, true);
        }

        /// <summary>
        /// Adds the specified ActionType to the item.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to add the action to.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        /// <param name="actionType">The type of action to add.</param>
        /// <param name="actionItemDefinition">The ItemDefinition that the action uses (optional).</param>
        public static void AddAction(GameObject itemGameObject, GameObject firstPersonObject, GameObject firstPersonVisibleItem, GameObject thirdPersonObject, 
                                        ActionType actionType, ItemDefinitionBase actionItemDefinition)
        {
            // The action ID must be unique.
            var maxID = -1;
            var actions = itemGameObject.GetComponents<ItemAction>();
            for (int i = 0; i < actions.Length; ++i) {
                if (actions[i].ID > maxID) {
                    maxID = actions[i].ID;
                }
            }

            switch (actionType) {
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
                case ActionType.ShootableWeapon:
                    var shootableWeapon = itemGameObject.AddComponent<ShootableWeapon>();
                    shootableWeapon.ID = maxID + 1;
                    shootableWeapon.ConsumableItemDefinition = actionItemDefinition;
                    AddShootableWeaponProperties(itemGameObject, shootableWeapon.ID, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    break;
#endif
#if ULTIMATE_CHARACTER_CONTROLLER_MELEE
                case ActionType.MeleeWeapon:
                    var meleeWeapon = itemGameObject.AddComponent<MeleeWeapon>();
                    meleeWeapon.ID = maxID + 1;
                    meleeWeapon.FaceTarget = false;
                    AddMeleeWeaponProperties(itemGameObject, meleeWeapon.ID, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    break;
                case ActionType.Shield:
                    var shield = itemGameObject.AddComponent<Shield>();
                    shield.ID = maxID + 1;
                    var shieldAttributeManager = itemGameObject.AddComponent<Traits.AttributeManager>();
                    shieldAttributeManager.Attributes[0].Name = "Durability"; // Rename the Health attribute to Durability.
                    AddShieldProperties(shield, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    // The Block ability should be added if it isn't already.
                    var characterLocomotion = itemGameObject.GetComponentInParent<UltimateCharacterLocomotion>();
                    if (characterLocomotion != null) {
                        var blockAbility = characterLocomotion.GetAbility<Character.Abilities.Items.Block>();
                        if (blockAbility == null) {
                            AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.Items.Block));
                        }
                    }
                    break;
#endif
                case ActionType.MagicItem:
                    var magicItem = itemGameObject.AddComponent<MagicItem>();
                    var item = itemGameObject.GetComponent<Item>();
                    item.EquipEvent = new AnimationEventTrigger(false, 0);
                    item.UnequipEvent = new AnimationEventTrigger(false, 0);
                    AddMagicItemProperties(itemGameObject, magicItem.ID, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    break;
                case ActionType.ThrowableItem:
                    var throwableItem = itemGameObject.AddComponent<ThrowableItem>();
                    throwableItem.ID = maxID + 1;
                    throwableItem.CanEquipEmptyItem = false;
                    AddThrowableItemProperties(itemGameObject, throwableItem.ID, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    break;
                case ActionType.GrenadeItem:
                    var grenadeItem = itemGameObject.AddComponent<GrenadeItem>();
                    grenadeItem.ID = maxID + 1;
                    grenadeItem.CanEquipEmptyItem = false;
                    AddGrenadeItemProperties(itemGameObject, grenadeItem.ID, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    break;
                case ActionType.Flashlight:
                    var flashLight = itemGameObject.AddComponent<Flashlight>();
                    flashLight.ID = maxID + 1;
                    flashLight.UseEvent = new AnimationEventTrigger(false, 0);
                    flashLight.UseCompleteEvent = new AnimationEventTrigger(false, 0);
                    var flashlightAttributeManager = itemGameObject.AddComponent<Traits.AttributeManager>();
                    flashlightAttributeManager.Attributes[0].Name = "Battery"; // Rename the Health attribute to Battery.
                    AddFlashlightProperties(itemGameObject, flashLight.ID, firstPersonObject, firstPersonVisibleItem, thirdPersonObject);
                    break;
            }
        }

#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
        /// <summary>
        /// Adds the ShootableWeaponProperties to the specified GameObject.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to add the properties to.</param>
        /// <param name="actionID">The ActionID of the properties.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        private static void AddShootableWeaponProperties(GameObject itemGameObject, int actionID, GameObject firstPersonObject, GameObject firstPersonVisibleItem, GameObject thirdPersonObject)
        {
#if FIRST_PERSON_SHOOTER
            if (firstPersonObject != null || firstPersonVisibleItem != null) {
                var parent = firstPersonVisibleItem != null ? firstPersonVisibleItem.transform : firstPersonObject.transform;
                var shootableProperties = itemGameObject.AddComponent<FirstPersonController.Items.FirstPersonShootableWeaponProperties>();
                // Setup the standard references.
                shootableProperties.ActionID = actionID;
                shootableProperties.FirePointLocation = CreateGameObject("Fire Point", parent);
                shootableProperties.MuzzleFlashLocation = CreateGameObject("Muzzle Flash", parent);
                shootableProperties.ShellLocation = CreateGameObject("Shell Eject Point", parent);
            }
#endif
            if (thirdPersonObject != null) {
                var shootableProperties = itemGameObject.AddComponent<ThirdPersonController.Items.ThirdPersonShootableWeaponProperties>();
                // Setup the standard references.
                shootableProperties.ActionID = actionID;
                shootableProperties.FirePointLocation = CreateGameObject("Fire Point", thirdPersonObject.transform);
                shootableProperties.MuzzleFlashLocation = CreateGameObject("Muzzle Flash", thirdPersonObject.transform);
                shootableProperties.ShellLocation = CreateGameObject("Shell Eject Point", thirdPersonObject.transform);
            }
        }
#endif

#if ULTIMATE_CHARACTER_CONTROLLER_MELEE
        /// <summary>
        /// Adds the MeleeWeaponProperties to the specified GameObject.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to add the properties to.</param>
        /// <param name="actionID">The ActionID of the properties.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        private static void AddMeleeWeaponProperties(GameObject itemGameObject, int actionID, GameObject firstPersonObject, GameObject firstPersonVisibleItem, GameObject thirdPersonObject)
        {
#if FIRST_PERSON_MELEE
            if (firstPersonObject != null || firstPersonVisibleItem != null) {
                var meleeWeaponProperties = itemGameObject.AddComponent<FirstPersonController.Items.FirstPersonMeleeWeaponProperties>();
                meleeWeaponProperties.ActionID = actionID;

                if (firstPersonVisibleItem != null) {
                    BoxCollider boxCollider;
                    if ((boxCollider = firstPersonVisibleItem.GetComponent<BoxCollider>()) == null) {
                        boxCollider = firstPersonVisibleItem.AddComponent<BoxCollider>();
                    }
                    meleeWeaponProperties.Hitboxes = new MeleeWeapon.MeleeHitbox[] { new MeleeWeapon.MeleeHitbox(boxCollider) };
                }
            }
#endif
            var character = itemGameObject.GetComponentInParent<UltimateCharacterLocomotion>();
            if (thirdPersonObject != null || (character != null && character.GetComponent<Animator>() != null)) {
                var meleeWeaponProperties = itemGameObject.AddComponent<ThirdPersonController.Items.ThirdPersonMeleeWeaponProperties>();
                meleeWeaponProperties.ActionID = actionID;

                if (thirdPersonObject != null) {
                    BoxCollider boxCollider;
                    if ((boxCollider = thirdPersonObject.GetComponent<BoxCollider>()) == null) {
                        boxCollider = thirdPersonObject.AddComponent<BoxCollider>();
                    }
                    meleeWeaponProperties.Hitboxes = new MeleeWeapon.MeleeHitbox[] { new MeleeWeapon.MeleeHitbox(boxCollider) };
                }
            }
        }

        /// <summary>
        /// Adds the shield properties to the specified GameObject.
        /// </summary>
        /// <param name="shield">A reference to the parent Shield component.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        private static void AddShieldProperties(Shield shield, GameObject firstPersonObject, GameObject firstPersonVisibleItem, GameObject thirdPersonObject)
        {
#if FIRST_PERSON_CONTROLLER
            if (firstPersonObject != null || firstPersonVisibleItem != null) {
                var shieldCollider = firstPersonVisibleItem.AddComponent<Objects.ItemAssist.ShieldCollider>();
                shieldCollider.Shield = shield;
                shieldCollider.FirstPersonPerspective = true;

                if (firstPersonVisibleItem.GetComponent<BoxCollider>() == null) {
                    firstPersonVisibleItem.AddComponent<BoxCollider>();
                }
            }
#endif
            if (thirdPersonObject != null) {
                var shieldCollider = thirdPersonObject.AddComponent<Objects.ItemAssist.ShieldCollider>();
                shieldCollider.Shield = shield;

                if (thirdPersonObject.GetComponent<BoxCollider>() == null) {
                    thirdPersonObject.AddComponent<BoxCollider>();
                }
            }
        }
#endif

        /// <summary>
        /// Adds the MagicItemProperties to the specified GameObject.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to add the properties to.</param>
        /// <param name="actionID">The ActionID of the properties.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        private static void AddMagicItemProperties(GameObject itemGameObject, int actionID, GameObject firstPersonObject, GameObject firstPersonVisibleItem, GameObject thirdPersonObject)
        {
#if FIRST_PERSON_CONTROLLER
            if (firstPersonObject != null || firstPersonVisibleItem != null) {
                var parent = firstPersonVisibleItem != null ? firstPersonVisibleItem.transform : firstPersonObject.transform;
                var magicItemProperties = itemGameObject.AddComponent<FirstPersonController.Items.FirstPersonMagicItemProperties>();
                // Setup the standard references.
                magicItemProperties.ActionID = actionID;
                magicItemProperties.OriginLocation = CreateGameObject("Origin", parent);
            }
#endif
            var character = itemGameObject.GetComponentInParent<UltimateCharacterLocomotion>();
            if (thirdPersonObject != null || (character != null && character.GetComponent<Animator>() != null)) {
                var magicItemProperties = itemGameObject.AddComponent<ThirdPersonController.Items.ThirdPersonMagicItemProperties>();
                // Setup the standard references.
                magicItemProperties.ActionID = actionID;
                magicItemProperties.OriginLocation = CreateGameObject("Origin", thirdPersonObject.transform);
            }
        }

        /// <summary>
        /// Adds the ThrowableItemProperties to the specified GameObject.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to add the properties to.</param>
        /// <param name="actionID">The ActionID of the properties.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        private static void AddThrowableItemProperties(GameObject itemGameObject, int actionID, GameObject firstPersonObject, GameObject firstPersonVisibleItem, GameObject thirdPersonObject)
        {
#if FIRST_PERSON_CONTROLLER
            if (firstPersonObject != null || firstPersonVisibleItem != null) {
                var throwableProperties = itemGameObject.AddComponent<FirstPersonController.Items.FirstPersonThrowableItemProperties>();
                // Setup the standard references.
                throwableProperties.ActionID = actionID;
                throwableProperties.ThrowLocation = throwableProperties.TrajectoryLocation = (firstPersonVisibleItem != null ? firstPersonVisibleItem : firstPersonObject).transform;
            }
#endif
            if (thirdPersonObject != null) {
                var throwableProperties = itemGameObject.AddComponent<ThirdPersonController.Items.ThirdPersonThrowableItemProperties>();
                // Setup the standard references.
                throwableProperties.ActionID = actionID;
                throwableProperties.ThrowLocation = throwableProperties.TrajectoryLocation = thirdPersonObject.transform;
            }

            // Throwable items should be completely dropped.
            var item = itemGameObject.GetComponent<Item>();
            item.FullInventoryDrop = true;
        }

        /// <summary>
        /// Adds the GrenadeItemProperties to the specified GameObject.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to add the properties to.</param>
        /// <param name="actionID">The ActionID of the properties.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        private static void AddGrenadeItemProperties(GameObject itemGameObject, int actionID, GameObject firstPersonObject, GameObject firstPersonVisibleItem, GameObject thirdPersonObject)
        {
#if FIRST_PERSON_CONTROLLER
            if (firstPersonObject != null || firstPersonVisibleItem != null) {
                var grenadeProperties = itemGameObject.AddComponent<FirstPersonController.Items.FirstPersonGrenadeItemProperties>();
                // Setup the standard references.
                grenadeProperties.ActionID = actionID;
                grenadeProperties.ThrowLocation = grenadeProperties.TrajectoryLocation = (firstPersonVisibleItem != null ? firstPersonVisibleItem : firstPersonObject).transform;

                // The Grenade component should not exist on the first person visible item.
                if (firstPersonVisibleItem != null && firstPersonVisibleItem.GetComponent<Objects.Grenade>() != null) {
                    GameObject.DestroyImmediate(firstPersonVisibleItem.GetComponent<Objects.Grenade>(), true);

                    // If the grenade component exists then a collider does as well.
                    if (firstPersonVisibleItem.GetComponent<Collider>() != null) {
                        GameObject.DestroyImmediate(firstPersonVisibleItem.GetComponent<Collider>(), true);
                    }
                }
            }
#endif
            if (thirdPersonObject != null) {
                var grenadeProperties = itemGameObject.AddComponent<ThirdPersonController.Items.ThirdPersonGrenadeItemProperties>();
                // Setup the standard references.
                grenadeProperties.ActionID = actionID;
                grenadeProperties.ThrowLocation = grenadeProperties.TrajectoryLocation = thirdPersonObject.transform;

                // The Grenade component should not exist on the third person object.
                if (thirdPersonObject.GetComponent<Objects.Grenade>() != null) {
                    GameObject.DestroyImmediate(thirdPersonObject.GetComponent<Objects.Grenade>(), true);

                    // If the grenade component exists then a collider does as well.
                    if (thirdPersonObject.GetComponent<Collider>() != null) {
                        GameObject.DestroyImmediate(thirdPersonObject.GetComponent<Collider>(), true);
                    }
                }
            }

            // Grenades should be completely dropped.
            var item = itemGameObject.GetComponent<Item>();
            item.FullInventoryDrop = true;
        }

        /// <summary>
        /// Adds the FlashlightProperties to the specified GameObject.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to add the properties to.</param>
        /// <param name="actionID">The ActionID of the properties.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        private static void AddFlashlightProperties(GameObject itemGameObject, int actionID, GameObject firstPersonObject, GameObject firstPersonVisibleItem, GameObject thirdPersonObject)
        {
#if FIRST_PERSON_CONTROLLER
            if (firstPersonObject != null || firstPersonVisibleItem != null) {
                var flashlight = itemGameObject.AddComponent<FirstPersonController.Items.FirstPersonFlashlightProperties>();
                // Setup the standard references.
                flashlight.ActionID = actionID;
                var lightGameObject = new GameObject("Light", typeof(Light));
                lightGameObject.transform.SetParentOrigin((firstPersonVisibleItem != null ? firstPersonVisibleItem : firstPersonObject).transform);
                flashlight.Light = lightGameObject;
            }
#endif
            if (thirdPersonObject != null) {
                var flashlight = itemGameObject.AddComponent<ThirdPersonController.Items.ThirdPersonFlashlightProperties>();
                // Setup the standard references.
                flashlight.ActionID = actionID;
                var lightGameObject = new GameObject("Light", typeof(Light));
                lightGameObject.transform.SetParentOrigin(thirdPersonObject.transform);
                flashlight.Light = lightGameObject;
            }
        }

        /// <summary>
        /// Adds the specified ActionType to the item.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to add the action to.</param>
        /// <param name="actionType">The type of action to add.</param>
        /// <param name="actionItemDefinition">The ItemDefinition that the action uses (optional).</param>
        public static void AddAction(GameObject itemGameObject, ActionType actionType, ItemDefinitionBase actionItemDefinition)
        {
            GameObject firstPersonObject = null, firstPersonVisibleItemGameObject = null, thirdPersonObject = null;
            PopulatePerspectiveObjects(itemGameObject, ref firstPersonObject, ref firstPersonVisibleItemGameObject, ref thirdPersonObject);
            AddAction(itemGameObject, firstPersonObject, firstPersonVisibleItemGameObject, thirdPersonObject, actionType, actionItemDefinition);
        }

        /// <summary>
        /// Populates the first and third person objects for the specified item GameObject.
        /// </summary>
        /// <param name="itemGameObject">The GameObject to get the first and third person references of.</param>
        /// <param name="firstPersonObject">A reference to the GameObject used in first person view.</param>
        /// <param name="firstPersonVisibleItem">A reference to the visible first person item.</param>
        /// <param name="thirdPersonObject">A reference to the GameObject used in third person view.</param>
        private static void PopulatePerspectiveObjects(GameObject itemGameObject, ref GameObject firstPersonObject, ref GameObject firstPersonVisibleItemGameObject, ref GameObject thirdPersonObject)
        {
#if FIRST_PERSON_CONTROLLER
            var firstPersonVisibleItem = itemGameObject.GetComponent<FirstPersonController.Items.FirstPersonPerspectiveItem>();
            if (firstPersonVisibleItem != null) {
                firstPersonObject = firstPersonVisibleItem.Object;
                firstPersonVisibleItemGameObject = firstPersonVisibleItem.VisibleItem;
            }
#endif
            var thirdPersonVisibleItem = itemGameObject.GetComponent<ThirdPersonController.Items.ThirdPersonPerspectiveItem>();
            if (thirdPersonVisibleItem != null) {
                thirdPersonObject = thirdPersonVisibleItem.Object;
            }
        }

        /// <summary>
        /// Removes the specified action.
        /// </summary>
        /// <param name="itemAction">The action to remove.</param>
        public static void RemoveAction(ItemAction itemAction)
        {
            // Remove the matching perspective properties first so the ID can be matched.
            RemovePerspectiveProperties(itemAction.gameObject, itemAction.ID);
            Object.DestroyImmediate(itemAction, true);
        }

        /// <summary>
        /// Removes the perspective properties on the item with the specified ID.
        /// </summary>
        /// <param name="itemGameObject">The GameObject which has the ItemPerpsectiveProperties.</param>
        private static void RemovePerspectiveProperties(GameObject itemGameObject, int actionID)
        {
            var perspectiveProperties = itemGameObject.GetComponents<ItemPerspectiveProperties>();
            for (int i = perspectiveProperties.Length - 1; i > -1; --i) {
                if (perspectiveProperties[i].ActionID != actionID) {
                    continue;
                }

#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
                var shootableWeaponPerspectiveProperties = perspectiveProperties[i] as IShootableWeaponPerspectiveProperties;
                if (shootableWeaponPerspectiveProperties != null) {
                    var propertyTransform = shootableWeaponPerspectiveProperties.FirePointLocation;
                    if (propertyTransform != null) {
                        Object.DestroyImmediate(propertyTransform.gameObject, true);
                    }
                    propertyTransform = shootableWeaponPerspectiveProperties.MuzzleFlashLocation;
                    if (propertyTransform != null) {
                        Object.DestroyImmediate(propertyTransform.gameObject, true);
                    }
                    propertyTransform = shootableWeaponPerspectiveProperties.ShellLocation;
                    if (propertyTransform != null) {
                        Object.DestroyImmediate(propertyTransform.gameObject, true);
                    }
                    propertyTransform = shootableWeaponPerspectiveProperties.SmokeLocation;
                    if (propertyTransform != null) {
                        Object.DestroyImmediate(propertyTransform.gameObject, true);
                    }
                }
#endif

                Object.DestroyImmediate(perspectiveProperties[i], true);
            }
        }
    }
}