/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility.Builders
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Identifiers;
    using Opsive.UltimateCharacterController.Character.MovementTypes;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Inventory;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Allows for the Ultimate Character Controller components to be added/removed at runtime.
    /// </summary>
    public static class CharacterBuilder
    {
        private const string c_MovingStateGUID = "527d884c54f1a4b4a82fed73411305a8";

        /// <summary>
        /// Adds the essnetial components to the specified character and sets the MovementType.
        /// </summary>
        /// <param name="character">The GameObject of the character.</param>
        /// <param name="addAnimator">Should the animator components be added?</param>
        /// <param name="animatorController">A reference to the animator controller.</param>
        /// <param name="firstPersonMovementType">The first person MovementType that should be added.</param>
        /// <param name="thirdPersonMovementType">The third person MovementType that should be added.</param>
        /// <param name="startFirstPersonPerspective">Should the character start in a first person perspective?</param>
        /// <param name="firstPersonHiddenObjects">The objects that should be hidden in first person view.</param>
        /// <param name="invisibleShadowCasterMaterial">The shadow caster material applied to the invisible first person objects.</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        public static void BuildCharacter(GameObject character, bool addAnimator, RuntimeAnimatorController animatorController, string firstPersonMovementType, string thirdPersonMovementType, bool startFirstPersonPerspective,
            GameObject[] firstPersonHiddenObjects, Material invisibleShadowCasterMaterial, bool aiAgent)
        {
            // Determine if the ThirdPersonObject component should be added or the invisible object renderer should be directly set to the invisible shadow caster.
            if (firstPersonHiddenObjects != null) {
                for (int i = 0; i < firstPersonHiddenObjects.Length; ++i) {
                    if (firstPersonHiddenObjects[i] == null) {
                        continue;
                    }

                    if (string.IsNullOrEmpty(thirdPersonMovementType)) {
                        var renderers = firstPersonHiddenObjects[i].GetComponents<Renderer>();
                        for (int j = 0; j < renderers.Length; ++j) {
                            var materials = renderers[j].sharedMaterials;
                            for (int k = 0; k < materials.Length; ++k) {
                                materials[k] = invisibleShadowCasterMaterial;
                            }
                            renderers[j].sharedMaterials = materials;
                        }
                    } else {
                        // The PerspectiveMonitor component is responsible for switching out the material.
                        firstPersonHiddenObjects[i].AddComponent<ThirdPersonObject>();
                    }
                }
            }

            AddEssentials(character, addAnimator, animatorController, !string.IsNullOrEmpty(firstPersonMovementType) && !string.IsNullOrEmpty(thirdPersonMovementType), invisibleShadowCasterMaterial, aiAgent);

            // The last added MovementType is starting movement type.
            if (startFirstPersonPerspective) {
                if (!string.IsNullOrEmpty(thirdPersonMovementType)) {
                    AddMovementType(character, thirdPersonMovementType);
                }
                if (!string.IsNullOrEmpty(firstPersonMovementType)) {
                    AddMovementType(character, firstPersonMovementType);
                }
            } else {
                if (!string.IsNullOrEmpty(firstPersonMovementType)) {
                    AddMovementType(character, firstPersonMovementType);
                }
                if (!string.IsNullOrEmpty(thirdPersonMovementType)) {
                    AddMovementType(character, thirdPersonMovementType);
                }
            }
        }

        /// <summary>
        /// Adds the Ultimate Character Controller essential components to the specified character.
        /// </summary>
        /// <param name="character">The character to add the components to.</param>
        /// <param name="addAnimator">Should the animator components be added?</param>
        /// <param name="animatorController">A reference to the animator controller.</param>
        /// <param name="addPerspectiveMonitor">Should the perspective monitor be added?</param>
        /// <param name="invisibleShadowCasterMaterial">The shadow caster material applied to the invisible first person objects.</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        public static void AddEssentials(GameObject character, bool addAnimator, RuntimeAnimatorController animatorController, bool addPerspectiveMonitor, Material invisibleShadowCasterMaterial, bool aiAgent)
        {
            if (!aiAgent) {
                character.tag = "Player";
            }
            character.layer = LayerManager.Character;
            if (character.GetComponent<CharacterLayerManager>() == null) {
                character.AddComponent<CharacterLayerManager>();
            }

            var rigidbody = character.GetComponent<Rigidbody>();
            if (rigidbody == null) {
                rigidbody = character.AddComponent<Rigidbody>();
            }
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            GameObject collider = null;
            var colliderIdentifier = character.GetComponent<CharacterColliderBaseIdentifier>();
            if (colliderIdentifier == null) {
                var colliders = new GameObject("Colliders");
                colliders.layer = LayerManager.Character;
                colliders.transform.SetParentOrigin(character.transform);
                collider = new GameObject("CapsuleCollider");
                collider.layer = LayerManager.Character;
                collider.transform.SetParentOrigin(colliders.transform);
                var capsuleCollider = collider.AddComponent<CapsuleCollider>();
                capsuleCollider.center = new Vector3(0, 1, 0);
                capsuleCollider.height = 2;
                capsuleCollider.radius = 0.4f;
            }

            if (addAnimator) {
                AddAnimator(character, animatorController, aiAgent);
            }

            if (character.GetComponent<UltimateCharacterLocomotion>() == null) {
#if UNITY_EDITOR
                var characterLocomotion = character.AddComponent<UltimateCharacterLocomotion>();
                if (!Application.isPlaying) {
                    // The Moving state should automatically be added.
                    var presetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(c_MovingStateGUID);
                    if (!string.IsNullOrEmpty(presetPath)) {
                        var preset = UnityEditor.AssetDatabase.LoadAssetAtPath(presetPath, typeof(StateSystem.PersistablePreset)) as StateSystem.PersistablePreset;
                        if (preset != null) {
                            var states = characterLocomotion.States;
                            System.Array.Resize(ref states, states.Length + 1);
                            // Default must always be at the end.
                            states[states.Length - 1] = states[0];
                            states[states.Length - 2] = new StateSystem.State("Moving", preset, null);
                            characterLocomotion.States = states;
                        }
                    }
                }
#else
                character.AddComponent<UltimateCharacterLocomotion>();
#endif
            }

            if (collider != null) {
                var positioner = collider.AddComponent<CapsuleColliderPositioner>();
                positioner.FirstEndCapTarget = character.transform;

                var animator = character.GetComponent<Animator>();
                if (animator != null) {
                    // The CapsuleColliderPositioner should follow the character's movements.
                    var head = animator.GetBoneTransform(HumanBodyBones.Head);
                    if (head != null) {
                        positioner.SecondEndCapTarget = head;
                        positioner.RotationBone = positioner.PositionBone = animator.GetBoneTransform(HumanBodyBones.Hips);
                    }
                }
            }

            if (aiAgent) {
                AddAIAgent(character);
            } else {
                AddUnityInput(character);

                if (character.GetComponent<UltimateCharacterLocomotionHandler>() == null) {
                    character.AddComponent<UltimateCharacterLocomotionHandler>();
                }
            }

#if THIRD_PERSON_CONTROLLER
            if (addPerspectiveMonitor && character.GetComponent<ThirdPersonController.Character.PerspectiveMonitor>() == null) {
                var perspectiveMonitor = character.AddComponent<ThirdPersonController.Character.PerspectiveMonitor>();
                if (perspectiveMonitor.InvisibleMaterial == null) {
                    perspectiveMonitor.InvisibleMaterial = invisibleShadowCasterMaterial;
                }
            }
#endif

            // All of the child GameObjects should be set to the SubCharacter layer to prevent any added-colliders from interferring with the locomotion.
            SetRecursiveLayer(character, LayerManager.SubCharacter, LayerManager.Character);
        }

        /// <summary>
        /// Adds the animator with the specified controller to the character.
        /// </summary>
        /// <param name="character">The character to add the animator to.</param>
        /// <param name="animatorController">A reference to the animator controller.</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        public static void AddAnimator(GameObject character, RuntimeAnimatorController animatorController, bool aiAgent)
        {
            Animator animator;
            if ((animator = character.GetComponent<Animator>()) == null) {
                animator = character.AddComponent<Animator>();
            }
            animator.runtimeAnimatorController = animatorController;
            if (!aiAgent) {
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            if (character.GetComponent<AnimatorMonitor>() == null) {
                character.AddComponent<AnimatorMonitor>();
            }
        }

        /// <summary>
        /// Removes the animator from the character.
        /// </summary>
        /// <param name="character">The character to remove the animator from.</param>
        public static void RemoveAnimator(GameObject character)
        {
            var animator = character.GetComponent<Animator>();
            if (animator != null) {
                GameObject.DestroyImmediate(animator, true);
            }

            var animatorMonitor = character.GetComponent<AnimatorMonitor>();
            if (animatorMonitor != null) {
                GameObject.DestroyImmediate(animatorMonitor, true);
            }
        }

        /// <summary>
        /// Sets the GameObject to the specified layer. Will recursively set the children unless the child contains a component that shouldn't be set.
        /// </summary>
        /// <param name="gameObject">The GameObject to set.</param>
        /// <param name="layer">The layer to set the GameObject to.</param>
        /// <param name="characterLayer">The layer of the character. GameObjects with this layer will not be set to the specified layer.</param>
        private static void SetRecursiveLayer(GameObject gameObject, int layer, int characterLayer)
        {
            var children = gameObject.transform.childCount;
            for (int i = 0; i < gameObject.transform.childCount; ++i) {
                var child = gameObject.transform.GetChild(i);
                // Do not set the layer if the child is already set to the Character layer or contains the item identifier components.
                if (child.gameObject.layer == characterLayer || child.GetComponent<Items.ItemPlacement>() != null) {
                    continue;
                }

#if FIRST_PERSON_CONTROLLER
                // First person objects do not need to be set.
                if (child.GetComponent<FirstPersonController.Character.FirstPersonObjects>() != null) {
                    continue;
                }
#endif

                // Set the layer.
                child.gameObject.layer = layer;
                SetRecursiveLayer(child.gameObject, layer, characterLayer);
            }
        }

        /// <summary>
        /// Removes the Ultimate Character Controller essential components from the specified character.
        /// </summary>
        /// <param name="character">The character to remove the components from.</param>
        public static void RemoveEssentials(GameObject character)
        {
            var rigidbody = character.GetComponent<Rigidbody>();
            if (rigidbody != null) {
                GameObject.DestroyImmediate(rigidbody, true);
            }

            var collider = character.GetComponent<CharacterColliderBaseIdentifier>();
            if (collider != null) {
                GameObject.DestroyImmediate(collider, true);
            }

            var ultimateCharacterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
            if (ultimateCharacterLocomotion != null) {
                GameObject.DestroyImmediate(ultimateCharacterLocomotion, true);
            }

            var ultimateCharacterLocomotionHandler = character.GetComponent<UltimateCharacterLocomotionHandler>();
            if (ultimateCharacterLocomotionHandler != null) {
                GameObject.DestroyImmediate(ultimateCharacterLocomotionHandler, true);
            }

            var localLookSource = character.GetComponent<LocalLookSource>();
            if (localLookSource != null) {
                GameObject.DestroyImmediate(localLookSource, true);
            }

            var layerManager = character.GetComponent<CharacterLayerManager>();
            if (layerManager != null) {
                GameObject.DestroyImmediate(layerManager, true);
            }

#if THIRD_PERSON_CONTROLLER
            var perspectiveMonitor = character.GetComponent<ThirdPersonController.Character.PerspectiveMonitor>();
            if (perspectiveMonitor != null) {
                GameObject.DestroyImmediate(perspectiveMonitor, true);
            }
#endif
        }

        /// <summary>
        /// Adds the specified MovementType to the character.
        /// </summary>
        /// <param name="character">The character to add the MovementType to.</param>
        /// <param name="movementType">The MovementType to add.</param>
        public static void AddMovementType(GameObject character, string movementType)
        {
            var ultimateCharacterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
            if (ultimateCharacterLocomotion != null) {
                // Don't allow duplicate MovementTypes.
                var type = System.Type.GetType(movementType);
                ultimateCharacterLocomotion.DeserializeMovementTypes();
                var movementTypes = ultimateCharacterLocomotion.MovementTypes;
                var add = true;
                if (movementTypes != null) {
                    for (int i = 0; i < movementTypes.Length; ++i) {
                        if (movementTypes[i].GetType() == type) {
                            add = false;
                        }
                    }
                }
                if (add) {
                    var movementTypesList = new List<MovementType>();
                    if (movementTypes != null) {
                        movementTypesList.AddRange(movementTypes);
                    }
                    var movementTypeObj = System.Activator.CreateInstance(type) as MovementType;
                    movementTypesList.Add(movementTypeObj);
                    ultimateCharacterLocomotion.MovementTypes = movementTypesList.ToArray();
                    ultimateCharacterLocomotion.MovementTypeData = Shared.Utility.Serialization.Serialize<MovementType>(movementTypesList);

                    // If the character has already been initialized then the movement type should be initialized.
                    if (Application.isPlaying) {
                        movementTypeObj.Initialize(ultimateCharacterLocomotion);
                        movementTypeObj.Awake();
                    }
                }

                // Set the added movement type as the default.
                ultimateCharacterLocomotion.SetMovementType(type);
            }
        }

        /// <summary>
        /// Adds the non-essential Ultimate Character Controller components to the character.
        /// </summary>
        /// <param name="character">The character to add the components to.</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        /// <param name="addItems">Should the item components be added?</param>
        /// <param name="itemCollection">A reference to the ItemCollection component.</param>
        /// <param name="firstPersonItems">Does the character support first person items?</param>
        /// <param name="addHealth">Should the health components be added?</param>
        /// <param name="addUnityIK">Should the CharacterIK component be added?</param>
        /// <param name="addFootEffects">Should the CharacterFootEffects component be added?</param>
        /// <param name="addStandardAbilities">Should the standard abilities be added?</param>
        /// <param name="addNavMeshAgent">Should the NavMeshAgent component be added?</param>
        public static void BuildCharacterComponents(GameObject character, bool aiAgent, bool addItems, 
            ItemCollection itemCollection, bool firstPersonItems, bool addHealth, bool addUnityIK, bool addFootEffects, bool addStandardAbilities, bool addNavMeshAgent)
        {
            if (addItems) {
                AddItemSupport(character, itemCollection, aiAgent, firstPersonItems);
            }
            if (addHealth) {
                AddHealth(character);
            }
            if (addUnityIK) {
                AddUnityIK(character);
            }
            if (addFootEffects) {
                AddFootEffects(character);
            }
            if (addStandardAbilities) {
                // Add the Jump, Fall, Speed Change, and Height Change abilities.
                var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
                AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.Jump));
                AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.Fall));
                AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.MoveTowards));
                AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.SpeedChange));
                AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.HeightChange));
                // The abilities should not use an input related start type.
                if (aiAgent) {
                    var abilities = characterLocomotion.GetAbilities<Character.Abilities.Ability>();
                    for (int i = 0; i < abilities.Length; ++i) {
                        if (abilities[i].StartType != Character.Abilities.Ability.AbilityStartType.Automatic &&
                            abilities[i].StartType != Character.Abilities.Ability.AbilityStartType.Manual) {
                            abilities[i].StartType = Character.Abilities.Ability.AbilityStartType.Manual;
                        }
                        if (abilities[i].StopType != Character.Abilities.Ability.AbilityStopType.Automatic &&
                            abilities[i].StopType != Character.Abilities.Ability.AbilityStopType.Manual) {
                            abilities[i].StopType = Character.Abilities.Ability.AbilityStopType.Manual;
                        }
                        if (abilities[i] is Character.Abilities.Items.Use) {
                            abilities[i].StopType = Character.Abilities.Ability.AbilityStopType.Manual;
                        }
                    }
                    AbilityBuilder.SerializeAbilities(characterLocomotion);
                }
            }
            if (addNavMeshAgent) {
                var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
                var abilities = characterLocomotion.Abilities;
                var index = abilities != null ? abilities.Length : 0;
                if (abilities != null) {
                    for (int i = 0; i < abilities.Length; ++i) {
                        if (abilities[i] is Character.Abilities.SpeedChange) {
                            index = i;
                            break;
                        }
                    }
                }
                // The ability should be positioned before the SpeedChange ability.
                AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.AI.NavMeshAgentMovement), index);
                var navMeshAgent = character.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (navMeshAgent != null) {
                    navMeshAgent.stoppingDistance = 0.1f;
                }
            }
            if (addItems) {
                // Add the Equip, Aim, Use, and Reload item abilities.
                var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
                AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.Reload));
#endif
                AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.Use));
                AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.EquipUnequip));
                AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.ToggleEquip));
                AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.EquipNext));
                AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.EquipPrevious));
                AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.EquipScroll));
                AbilityBuilder.AddItemAbility(characterLocomotion, typeof(Character.Abilities.Items.Aim));
                // The buttons should not use an input related start type.
                if (aiAgent) {
                    var itemAbilities = characterLocomotion.GetAbilities<Character.Abilities.Items.ItemAbility>();
                    for (int i = 0; i < itemAbilities.Length; ++i) {
                        if (itemAbilities[i].StartType != Character.Abilities.Ability.AbilityStartType.Automatic &&
                            itemAbilities[i].StartType != Character.Abilities.Ability.AbilityStartType.Manual) {
                            itemAbilities[i].StartType = Character.Abilities.Ability.AbilityStartType.Manual;
                        }
                        if (itemAbilities[i].StopType != Character.Abilities.Ability.AbilityStopType.Automatic &&
                            itemAbilities[i].StopType != Character.Abilities.Ability.AbilityStopType.Manual) {
                            itemAbilities[i].StopType = Character.Abilities.Ability.AbilityStopType.Manual;
                        }
                    }
                    AbilityBuilder.SerializeItemAbilities(characterLocomotion);
                }

                // The ItemEquipVerifier needs to be added after the item abilities.
                AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.ItemEquipVerifier));
                AbilityBuilder.SerializeAbilities(characterLocomotion);
            }
        }

        /// <summary>
        /// Adds the ai agent components to the character.
        /// </summary>
        /// <param name="character">The character to add the ai agent components to.</param>
        public static void AddAIAgent(GameObject character)
        {
            if (character.GetComponent<LocalLookSource>() == null) {
                character.AddComponent<LocalLookSource>();
            }

            var locomotionHandler = character.GetComponent<UltimateCharacterLocomotionHandler>();
            if (locomotionHandler != null) {
                GameObject.DestroyImmediate(locomotionHandler, true);
            }

            var itemHandler = character.GetComponent<ItemHandler>();
            if (itemHandler != null) {
                GameObject.DestroyImmediate(itemHandler, true);
            }

            RemoveUnityInput(character);
        }

        /// <summary>
        /// Removes the ai agent components from the character.
        /// </summary>
        /// <param name="character">The character to remove the ai agent components to.</param>
        public static void RemoveAIAgent(GameObject character)
        {
            var localLookSource = character.GetComponent<LocalLookSource>();
            if (localLookSource != null) {
                GameObject.DestroyImmediate(localLookSource, true);
            }

            if (character.GetComponent<UltimateCharacterLocomotionHandler>() == null) {
                character.AddComponent<UltimateCharacterLocomotionHandler>();
            }

            if (character.GetComponent<ItemHandler>() == null) {
                character.AddComponent<ItemHandler>();
            }

            AddUnityInput(character);
            AbilityBuilder.RemoveAbility<Character.Abilities.AI.NavMeshAgentMovement>(character.GetComponent<UltimateCharacterLocomotion>());

            var navMeshAgent = character.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navMeshAgent != null) {
                GameObject.DestroyImmediate(navMeshAgent, true);
            }
        }

        /// <summary>
        /// Adds the UnityInput component to the character.
        /// </summary>
        /// <param name="character">The character to add the UnityInput component to.</param>
        public static void AddUnityInput(GameObject character)
        {
            if (character.GetComponent<UltimateCharacterController.Input.UnityInput>() == null) {
                character.AddComponent<UltimateCharacterController.Input.UnityInput>();
            }
        }

        /// <summary>
        /// Removes the UnityInput component from the character.
        /// </summary>
        /// <param name="character">The character to remove the UnityInput component from.</param>
        public static void RemoveUnityInput(GameObject character)
        {
            var unityInput = character.GetComponent<UltimateCharacterController.Input.UnityInput>();
            if (unityInput != null) {
                GameObject.DestroyImmediate(unityInput, true);
            }
        }

        /// <summary>
        /// Adds support for items to the character.
        /// </summary>
        /// <param name="character">The character to add support for items to.</param>
        /// <param name="itemCollection">A reference to the inventory's ItemCollection.</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        /// <param name="firstPersonItems">Does the character support first person items?</param>
        public static void AddItemSupport(GameObject character, ItemCollection itemCollection, bool aiAgent, bool firstPersonItems)
        {
            // Even if the character doesn't have an animator the items may make use of one.
            if (character.GetComponent<AnimatorMonitor>() == null) {
                character.AddComponent<AnimatorMonitor>();
            }

            if (character.GetComponentInChildren<Items.ItemPlacement>() == null) {
                var items = new GameObject("Items");
                items.transform.parent = character.transform;
                items.AddComponent<Items.ItemPlacement>();
            }

            var animator = character.GetComponent<Animator>();
            if (animator != null) {
                var head = animator.GetBoneTransform(HumanBodyBones.Head);
                if (head != null) {
                    var leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                    var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    if (leftHand != null && rightHand != null) {
                        if (leftHand.GetComponentInChildren<Items.ItemSlot>() == null) {
                            var items = new GameObject("Items");
                            items.transform.SetParentOrigin(leftHand.transform);
                            var itemSlot = items.AddComponent<Items.ItemSlot>();
                            itemSlot.ID = 1;
                        }
                        if (rightHand.GetComponentInChildren<Items.ItemSlot>() == null) {
                            var items = new GameObject("Items");
                            items.transform.SetParentOrigin(rightHand.transform);
                            items.AddComponent<Items.ItemSlot>();
                        }
                    }
                }
            }

            // Items use the inventory for being equip/unequip.
            if (character.GetComponent<Inventory>() == null) {
                character.AddComponent<Inventory>();
            }

#if FIRST_PERSON_CONTROLLER
            if (firstPersonItems && character.GetComponentInChildren<FirstPersonController.Character.FirstPersonObjects>() == null) {
                var firstPersonObjects = new GameObject("First Person Objects");
                firstPersonObjects.transform.parent = character.transform;
                firstPersonObjects.AddComponent<FirstPersonController.Character.FirstPersonObjects>();
            }
#endif

            ItemSetManager itemSetManager;
            if ((itemSetManager = character.GetComponent<ItemSetManager>()) == null) {
                itemSetManager = character.AddComponent<ItemSetManager>();
            }
            itemSetManager.ItemCollection = itemCollection;
            if (!aiAgent && character.GetComponent<ItemHandler>() == null) {
                character.AddComponent<ItemHandler>();
            }
        }

        /// <summary>
        /// Removes support for items from the character.
        /// </summary>
        /// <param name="character">The character to remove support for the items from.</param>
        public static void RemoveItemSupport(GameObject character)
        {
            var animatorMonitor = character.GetComponent<ItemHandler>();
            if (animatorMonitor != null && character.GetComponent<Animator>() == null) {
                character.AddComponent<Animator>();
            }
            var itemHandler = character.GetComponent<ItemHandler>();
            if (itemHandler != null) {
                GameObject.DestroyImmediate(itemHandler, true);
            }
            var itemPlacement = character.GetComponentInChildren<Items.ItemPlacement>();
            if (itemPlacement != null) {
                GameObject.DestroyImmediate(itemPlacement.gameObject, true);
            }
#if FIRST_PERSON_CONTROLLER
            var firstPersonObjects = character.GetComponentInChildren<FirstPersonController.Character.FirstPersonObjects>();
            if (firstPersonObjects != null) {
                GameObject.DestroyImmediate(firstPersonObjects, true);
            }
#endif

            var itemSlots = character.GetComponentsInChildren<Items.ItemSlot>();
            if (itemSlots != null && itemSlots.Length > 0) {
                for (int i = itemSlots.Length - 1; i >= 0; --i) {
                    GameObject.DestroyImmediate(itemSlots[i].gameObject, true);
                }
            }

            var inventory = character.GetComponent<Inventory>();
            if (inventory != null) {
                GameObject.DestroyImmediate(inventory, true);
            }

            var itemSetManager = character.GetComponent<ItemSetManager>();
            if (itemSetManager != null) {
                GameObject.DestroyImmediate(itemSetManager, true);
            }
        }

        /// <summary>
        /// Adds the health components to the character.
        /// </summary>
        /// <param name="character">The character to add the health components to.</param>
        public static void AddHealth(GameObject character)
        {
            if (character.GetComponent<Traits.AttributeManager>() == null) {
                character.AddComponent<Traits.AttributeManager>();
            }

            if (character.GetComponent<Traits.CharacterHealth>() == null) {
                character.AddComponent<Traits.CharacterHealth>();
            }

            if (character.GetComponent<Traits.CharacterRespawner>() == null) {
                character.AddComponent<Traits.CharacterRespawner>();
            }
        }

        /// <summary>
        /// Removes the health components from the character.
        /// </summary>
        /// <param name="character">The character to remove the health components from.</param>
        public static void RemoveHealth(GameObject character)
        {
            var health = character.GetComponent<Traits.CharacterHealth>();
            if (health != null) {
                GameObject.DestroyImmediate(health, true);
            }

            var attributeManager = character.GetComponent<Traits.AttributeManager>();
            if (attributeManager != null) {
                GameObject.DestroyImmediate(attributeManager, true);
            }

            var respawner = character.GetComponent<Traits.CharacterRespawner>();
            if (respawner != null) {
                GameObject.DestroyImmediate(respawner, true);
            }
        }

        /// <summary>
        /// Adds the CharacterIK component to the character.
        /// </summary>
        /// <param name="character">The character to add the CharacterIK component to.</param>
        public static void AddUnityIK(GameObject character)
        {
            if (character.GetComponent<CharacterIK>() == null) {
                character.AddComponent<CharacterIK>();
            }
        }

        /// <summary>
        /// Removes the CharacterIK component from the character.
        /// </summary>
        /// <param name="character">The character to remove the CharacterIK component from.</param>
        public static void RemoveUnityIK(GameObject character)
        {
            var characterIK = character.GetComponent<CharacterIK>();
            if (characterIK != null) {
                GameObject.DestroyImmediate(characterIK, true);
            }
        }

        /// <summary>
        /// Adds the CharacterFootEffects component to the character.
        /// </summary>
        /// <param name="character">The character to add the CharacterFootEffects component to.</param>
        public static void AddFootEffects(GameObject character)
        {
            if (character.GetComponent<CharacterFootEffects>() == null) {
                var footEffects = character.AddComponent<CharacterFootEffects>();
                footEffects.InitializeHumanoidFeet();
            }
        }

        /// <summary>
        /// Removes the CharacterFootEffects component from the character.
        /// </summary>
        /// <param name="character">The character to remove the CharacterFootEffects component from.</param>
        public static void RemoveFootEffects(GameObject character)
        {
            var footEffects = character.GetComponent<CharacterFootEffects>();
            if (footEffects != null) {
                GameObject.DestroyImmediate(footEffects, true);
            }
        }
    }
}
