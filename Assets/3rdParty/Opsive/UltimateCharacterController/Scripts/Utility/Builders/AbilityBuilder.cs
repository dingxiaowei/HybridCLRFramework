/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility.Builders
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.StateSystem;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Adds and serializes UltimateCharacterLocomotion abilities.
    /// </summary>
    public static class AbilityBuilder
    {
        private static Dictionary<Type, UnityEngine.RequireComponent[]> s_RequiredComponents = new Dictionary<Type, UnityEngine.RequireComponent[]>();
        private static Dictionary<Type, DefaultInputName[]> s_DefaultInputName = new Dictionary<Type, DefaultInputName[]>();
        private static Dictionary<Type, DefaultStartType> s_DefaultStartType = new Dictionary<Type, DefaultStartType>();
        private static Dictionary<Type, DefaultStopType> s_DefaultStopType = new Dictionary<Type, DefaultStopType>();
        private static Dictionary<Type, DefaultAbilityIndex> s_DefaultAbilityIndex = new Dictionary<Type, DefaultAbilityIndex>();
        private static Dictionary<Type, DefaultAbilityIntData> s_DefaultAbilityIntData = new Dictionary<Type, DefaultAbilityIntData>();
        private static Dictionary<Type, DefaultItemStateIndex> s_DefaultItemStateIndex = new Dictionary<Type, DefaultItemStateIndex>();
        private static Dictionary<Type, DefaultState> s_DefaultState = new Dictionary<Type, DefaultState>();
        private static Dictionary<Type, DefaultAllowPositionalInput> s_DefaultAllowPositionalInput = new Dictionary<Type, DefaultAllowPositionalInput>();
        private static Dictionary<Type, DefaultAllowRotationalInput> s_DefaultAllowRotationalInput = new Dictionary<Type, DefaultAllowRotationalInput>();
        private static Dictionary<Type, DefaultUseGravity> s_DefaultUseGravity = new Dictionary<Type, DefaultUseGravity>();
        private static Dictionary<Type, DefaultUseRootMotionPosition> s_DefaultUseRootMotionPosition = new Dictionary<Type, DefaultUseRootMotionPosition>();
        private static Dictionary<Type, DefaultUseRootMotionRotation> s_DefaultUseRootMotionRotation = new Dictionary<Type, DefaultUseRootMotionRotation>();
        private static Dictionary<Type, DefaultDetectHorizontalCollisions> s_DefaultDetectHorizontalCollisions = new Dictionary<Type, DefaultDetectHorizontalCollisions>();
        private static Dictionary<Type, DefaultDetectVerticalCollisions> s_DefaultDetectVerticalCollisions = new Dictionary<Type, DefaultDetectVerticalCollisions>();
        private static Dictionary<Type, DefaultObjectDetection> s_DefaultObjectDetection = new Dictionary<Type, DefaultObjectDetection>();
        private static Dictionary<Type, DefaultUseLookDirection> s_DefaultUseLookDirection = new Dictionary<Type, DefaultUseLookDirection>();
        private static Dictionary<Type, DefaultCastOffset> s_DefaultCastOffset = new Dictionary<Type, DefaultCastOffset>();
        private static Dictionary<Type, DefaultEquippedSlots> s_DefaultEquippedSlots = new Dictionary<Type, DefaultEquippedSlots>();
        private static Dictionary<Type, DefaultReequipSlots> s_DefaultReequipSlots = new Dictionary<Type, DefaultReequipSlots>();
        private static Dictionary<Type, AddState[]> s_AddStates = new Dictionary<Type, AddState[]>();

        /// <summary>
        /// Adds the ability with the specified type.
        /// </summary>
        /// <param name="characterLocomotion">The character to add the ability to.</param>
        /// <param name="abilityType">The type of ability to add.</param>
        /// <returns>The added ability.</returns>
        public static Ability AddAbility(UltimateCharacterLocomotion characterLocomotion, Type abilityType)
        {
            if (typeof(ItemAbility).IsAssignableFrom(abilityType)) {
                return AddItemAbility(characterLocomotion, abilityType);
            }

            var abilities = characterLocomotion.GetSerializedAbilities();
            var index = abilities == null ? 0 : abilities.Length;
            return AddAbility(characterLocomotion, abilityType, index);
        }

        /// <summary>
        /// Adds the ability with the specified type.
        /// </summary>
        /// <param name="characterLocomotion">The character to add the ability to.</param>
        /// <param name="abilityType">The type of ability to add.</param>
        /// <param name="index">The index to add the ability to.</param>
        /// <returns>The added ability.</returns>
        public static Ability AddAbility(UltimateCharacterLocomotion characterLocomotion, Type abilityType, int index)
        {
            var abilities = characterLocomotion.GetSerializedAbilities();
            if (abilities == null) {
                abilities = new Ability[1];
            } else {
                Array.Resize(ref abilities, abilities.Length + 1);
            }
            var ability = Activator.CreateInstance(abilityType) as Ability;

            // Assign the default values specified by any added attribtes.
            SetAbilityDefaultValues(ability);

            for (int i = abilities.Length - 1; i > index; --i) {
                abilities[i] = abilities[i - 1];
            }
            abilities[index] = ability;
            characterLocomotion.Abilities = abilities;
            SerializeAbilities(characterLocomotion);

            // The ability may require other components in order to operate.
            var requiredComponents = GetRequiredComponents(abilityType);
            if (requiredComponents != null && requiredComponents.Length > 0) {
                for (int i = 0; i < requiredComponents.Length; ++i) {
                    characterLocomotion.gameObject.AddComponent(requiredComponents[i].m_Type0);
                }
            }

            return ability;
        }

        /// <summary>
        /// Adds the item ability with the specified type.
        /// </summary>
        /// <param name="characterLocomotion">The character to add the ability to.</param>
        /// <param name="abilityType">The type of ability to add.</param>
        /// <returns>The added ability.</returns>
        public static ItemAbility AddItemAbility(UltimateCharacterLocomotion characterLocomotion, Type abilityType)
        {
            var itemAbilities = characterLocomotion.GetSerializedItemAbilities();
            var index = itemAbilities == null ? 0 : itemAbilities.Length;
            return AddItemAbility(characterLocomotion, abilityType, index);
        }

        /// <summary>
        /// Adds the item ability with the specified type.
        /// </summary>
        /// <param name="characterLocomotion">The character to add the ability to.</param>
        /// <param name="abilityType">The type of ability to add.</param>
        /// <returns>The added ability.</returns>
        public static ItemAbility AddItemAbility(UltimateCharacterLocomotion characterLocomotion, Type abilityType, int index)
        {
            var itemAbilities = characterLocomotion.GetSerializedItemAbilities();
            if (itemAbilities == null) {
                itemAbilities = new ItemAbility[1];
            } else {
                Array.Resize(ref itemAbilities, itemAbilities.Length + 1);
            }
            var itemAbility = Activator.CreateInstance(abilityType) as ItemAbility;

            // Assign the default values specified by any added attribtes.
            SetAbilityDefaultValues(itemAbility);

            for (int i = itemAbilities.Length - 1; i > index; --i) {
                itemAbilities[i] = itemAbilities[i - 1];
            }
            itemAbilities[itemAbilities.Length - 1] = itemAbility;
            characterLocomotion.ItemAbilities = itemAbilities;
            SerializeItemAbilities(characterLocomotion);
            return itemAbility;
        }

        /// <summary>
        /// Serialize all of the abilities to the AbilityData array.
        /// </summary>
        /// <param name="characterLocomotion">The character to serialize.</param>
        public static void SerializeAbilities(UltimateCharacterLocomotion characterLocomotion)
        {
            var abilities = characterLocomotion.Abilities == null ? new List<Ability>() : new List<Ability>(characterLocomotion.Abilities);
            characterLocomotion.AbilityData = Shared.Utility.Serialization.Serialize<Ability>(abilities);
            characterLocomotion.Abilities = abilities.ToArray();
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(characterLocomotion);
#endif
        }

        /// <summary>
        /// Serialize all of the item abilities to the ItemAbilityData array.
        /// </summary>
        /// <param name="characterLocomotion">The character to serialize.</param>
        public static void SerializeItemAbilities(UltimateCharacterLocomotion characterLocomotion)
        {
            var itemAbilities = characterLocomotion.ItemAbilities == null ? new List<ItemAbility>() : new List<ItemAbility>(characterLocomotion.ItemAbilities);
            characterLocomotion.ItemAbilityData = Shared.Utility.Serialization.Serialize<ItemAbility>(itemAbilities);
            characterLocomotion.ItemAbilities = itemAbilities.ToArray();
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(characterLocomotion);
#endif
        }

        /// <summary>
        /// Removes the specified ability from the ability array.
        /// </summary>
        /// <param name="characterLocomotion">The character to remove the ability from.</param>
        public static void RemoveAbility<T>(UltimateCharacterLocomotion characterLocomotion) where T : Ability
        {
            var ability = characterLocomotion.GetAbility<T>();
            if (ability != null) {
                RemoveAbility(characterLocomotion, ability);
            }
        }

        /// <summary>
        /// Removes the specified ability from the ability array.
        /// </summary>
        /// <param name="characterLocomotion">The character to remove the ability from.</param>
        /// <param name="ability">The ability to remove.</param>
        public static void RemoveAbility(UltimateCharacterLocomotion characterLocomotion, Ability ability)
        {
            if (ability == null) {
                return;
            }

            if (typeof(ItemAbility).IsAssignableFrom(ability.GetType())) {
                RemoveItemAbility(characterLocomotion, ability);
                return;
            }

            var abilities = new Ability[characterLocomotion.Abilities.Length - 1];
            var index = 0;
            for (int i = 0; i < characterLocomotion.Abilities.Length; ++i) {
                if (characterLocomotion.Abilities[i] != ability) {
                    abilities[index] = characterLocomotion.Abilities[i];
                    index++;
                }
            }

            characterLocomotion.Abilities = abilities;
            SerializeAbilities(characterLocomotion);
        }

        /// <summary>
        /// Removes the specified ability from the item ability array.
        /// </summary>
        /// <param name="characterLocomotion">The character to remove the ability from.</param>
        /// <param name="ability">The ability to remove.</param>
        public static void RemoveItemAbility(UltimateCharacterLocomotion characterLocomotion, Ability ability)
        {
            var abilities = new ItemAbility[characterLocomotion.ItemAbilities.Length - 1];
            var index = 0;
            for (int i = 0; i < characterLocomotion.ItemAbilities.Length; ++i) {
                if (characterLocomotion.ItemAbilities[i] != ability) {
                    abilities[index] = characterLocomotion.ItemAbilities[i];
                }
            }

            characterLocomotion.ItemAbilities = abilities;
            SerializeItemAbilities(characterLocomotion);
        }

        /// <summary>
        /// Returns the RequiredComponent of the specified ability type.
        /// </summary>
        /// <param name="abilityType">The type of ability.</param>
        /// <returns>The RequiredComponent of the specified ability type. Can be null.</returns>
        private static UnityEngine.RequireComponent[] GetRequiredComponents(Type type)
        {
            UnityEngine.RequireComponent[] requiredComponents;
            if (s_RequiredComponents.TryGetValue(type, out requiredComponents)) {
                return requiredComponents;
            }

            if (type.GetCustomAttributes(typeof(UnityEngine.RequireComponent), true).Length > 0) {
                requiredComponents = type.GetCustomAttributes(typeof(UnityEngine.RequireComponent), true) as UnityEngine.RequireComponent[];
            }
            s_RequiredComponents.Add(type, requiredComponents);
            return requiredComponents;
        }

        /// <summary>
        /// Sets the default values for the ability.
        /// </summary>
        /// <param name="ability">The ability to set the default values of.</param>
        private static void SetAbilityDefaultValues(Ability ability)
        {
            var abilityType = ability.GetType();
            var defaultInputNames = GetDefaultInputNames(abilityType);
            if (defaultInputNames != null && defaultInputNames.Length > 0) {
                ability.InputNames = new string[defaultInputNames.Length];
                for (int i = 0; i < defaultInputNames.Length; ++i) {
                    ability.InputNames[defaultInputNames[i].Index] = defaultInputNames[i].InputName;
                }
            }
            var defaultStartType = GetDefaultStartType(abilityType);
            if (defaultStartType != null) {
                ability.StartType = defaultStartType.StartType;
            }
            var defaultStopType = GetDefaultStopType(abilityType);
            if (defaultStopType != null) {
                ability.StopType = defaultStopType.StopType;
            }
            var defaultAbilityIndex = GetDefaultAbilityIndex(abilityType);
            if (defaultAbilityIndex != null) {
                ability.AbilityIndexParameter = defaultAbilityIndex.Value;
            }
            var defaultAbilityIntData = GetDefaultAbilityIntData(abilityType);
            if (defaultAbilityIntData != null) {
                ability.AbilityIntData = defaultAbilityIntData.Value;
            }
            var defaultState = GetDefaultState(abilityType);
            if (defaultState != null) {
                ability.State = defaultState.Value;
            }
            if (typeof(ItemAbility).IsAssignableFrom(abilityType)) {
                var defaultItemStateIndex = GetDefaultItemStateIndex(abilityType);
                if (defaultItemStateIndex != null) {
                    (ability as ItemAbility).ItemStateIndex = defaultItemStateIndex.Value;
                }
            }
            var defaultAllowPositionalInput = GetDefaultAllowPositionalInput(abilityType);
            if (defaultAllowPositionalInput != null) {
                ability.AllowPositionalInput = defaultAllowPositionalInput.Value;
            }
            var defaultAllowRotationalInput = GetDefaultAllowRotationalInput(abilityType);
            if (defaultAllowRotationalInput != null) {
                ability.AllowRotationalInput = defaultAllowRotationalInput.Value;
            }
            var defaultUseGravity = GetDefaultUseGravity(abilityType);
            if (defaultUseGravity != null) {
                ability.UseGravity = defaultUseGravity.Value;
            }
            var defaultUseRootMotionPosition = GetDefaultUseRootMotionPosition(abilityType);
            if (defaultUseRootMotionPosition != null) {
                ability.UseRootMotionPosition = defaultUseRootMotionPosition.Value;
            }
            var defaultUseRootMotionRotation = GetDefaultUseRootMotionRotation(abilityType);
            if (defaultUseRootMotionRotation != null) {
                ability.UseRootMotionRotation = defaultUseRootMotionRotation.Value;
            }
            var defaultDetectHorizontalCollisions = GetDefaultDetectHorizontalCollisions(abilityType);
            if (defaultDetectHorizontalCollisions != null) {
                ability.DetectHorizontalCollisions = defaultDetectHorizontalCollisions.Value;
            }
            var defaultDetectVerticalCollisions = GetDefaultDetectVerticalCollisions(abilityType);
            if (defaultDetectVerticalCollisions != null) {
                ability.DetectVerticalCollisions = defaultDetectVerticalCollisions.Value;
            }
            if (ability is DetectObjectAbilityBase) {
                var defaultObjectDetection = GetDefaultObjectDetection(abilityType);
                if (defaultObjectDetection != null) {
                    (ability as DetectObjectAbilityBase).ObjectDetection = defaultObjectDetection.Value;

                    // If the detection layer is a trigger then the layer should include the ignore layer.
                    if ((defaultObjectDetection.Value & DetectObjectAbilityBase.ObjectDetectionMode.Trigger) != 0) {
                        (ability as DetectObjectAbilityBase).DetectLayers |= 1 << LayerManager.IgnoreRaycast;
                    }
                }

                var defaultUseLookDirection = GetDefaultUseLookDirection(abilityType);
                if (defaultUseLookDirection != null) {
                    (ability as DetectObjectAbilityBase).UseLookDirection = defaultUseLookDirection.Value;
                }

                var defaultCastOffset = GetDefaultCastOffset(abilityType);
                if (defaultCastOffset != null) {
                    (ability as DetectObjectAbilityBase).CastOffset = defaultCastOffset.Value;
                }

                var defaultEquippedSlots = GetDefaultEquippedSlots(abilityType);
                if (defaultEquippedSlots != null) {
                    (ability as DetectObjectAbilityBase).AllowEquippedSlotsMask = defaultEquippedSlots.Value;
                }

                var defaultReequipSlots = GetDefaultReequipSlots(abilityType);
                if (defaultReequipSlots != null) {
                    (ability as DetectObjectAbilityBase).ReequipSlots = defaultReequipSlots.Value;
                }
            }

#if UNITY_EDITOR
            var addStates = GetAddStates(abilityType);
            if (addStates != null && addStates.Length > 0) {
                var states = ability.States;
                var addedStates = 0;
                var stateLength = states.Length;
                Array.Resize(ref states, stateLength + addStates.Length);
                // Default must always be at the end.
                states[states.Length - 1] = states[0];
                for (int i = 0; i < addStates.Length; ++i) {
                    var presetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(addStates[i].PresetGUID);
                    if (!string.IsNullOrEmpty(presetPath)) {
                        var preset = UnityEditor.AssetDatabase.LoadAssetAtPath(presetPath, typeof(PersistablePreset)) as PersistablePreset;
                        if (preset != null) {
                            states[i] = new State(addStates[i].Name, preset, null);
                            addedStates++;
                        }
                    }
                }
                if (addedStates != addStates.Length) {
                    Array.Resize(ref states, stateLength + addedStates);
                }
                ability.States = states;
            }
#endif
        }

        /// <summary>
        /// Returns the DefaultInputName of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultInputName of the specified ability type. Can be null.</returns>
        private static DefaultInputName[] GetDefaultInputNames(Type type)
        {
            DefaultInputName[] defaultInputNames;
            if (s_DefaultInputName.TryGetValue(type, out defaultInputNames)) {
                return defaultInputNames;
            }

            if (type.GetCustomAttributes(typeof(DefaultInputName), true).Length > 0) {
                defaultInputNames = type.GetCustomAttributes(typeof(DefaultInputName), true) as DefaultInputName[];
            }
            s_DefaultInputName.Add(type, defaultInputNames);
            return defaultInputNames;
        }

        /// <summary>
        /// Returns the DefaultStartType of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultStartType of the specified ability type. Can be null.</returns>
        private static DefaultStartType GetDefaultStartType(Type type)
        {
            DefaultStartType defaultStartType;
            if (s_DefaultStartType.TryGetValue(type, out defaultStartType)) {
                return defaultStartType;
            }

            if (type.GetCustomAttributes(typeof(DefaultStartType), true).Length > 0) {
                defaultStartType = type.GetCustomAttributes(typeof(DefaultStartType), true)[0] as DefaultStartType;
            }
            s_DefaultStartType.Add(type, defaultStartType);
            return defaultStartType;
        }

        /// <summary>
        /// Returns the DefaultStopType of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultStopType of the specified ability type. Can be null.</returns>
        private static DefaultStopType GetDefaultStopType(Type type)
        {
            DefaultStopType defaultStopType;
            if (s_DefaultStopType.TryGetValue(type, out defaultStopType)) {
                return defaultStopType;
            }

            if (type.GetCustomAttributes(typeof(DefaultStopType), true).Length > 0) {
                defaultStopType = type.GetCustomAttributes(typeof(DefaultStopType), true)[0] as DefaultStopType;
            }
            s_DefaultStopType.Add(type, defaultStopType);
            return defaultStopType;
        }

        /// <summary>
        /// Returns the DefaultAbilityIndex of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultAbilityIndex of the specified ability type. Can be null.</returns>
        private static DefaultAbilityIndex GetDefaultAbilityIndex(Type type)
        {
            DefaultAbilityIndex defaultAbilityIndex;
            if (s_DefaultAbilityIndex.TryGetValue(type, out defaultAbilityIndex)) {
                return defaultAbilityIndex;
            }

            if (type.GetCustomAttributes(typeof(DefaultAbilityIndex), true).Length > 0) {
                defaultAbilityIndex = type.GetCustomAttributes(typeof(DefaultAbilityIndex), true)[0] as DefaultAbilityIndex;
            }
            s_DefaultAbilityIndex.Add(type, defaultAbilityIndex);
            return defaultAbilityIndex;
        }

        /// <summary>
        /// Returns the DefaultAbilityIntData of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultAbilityIntData of the specified ability type. Can be null.</returns>
        private static DefaultAbilityIntData GetDefaultAbilityIntData(Type type)
        {
            DefaultAbilityIntData defaultStateIndex;
            if (s_DefaultAbilityIntData.TryGetValue(type, out defaultStateIndex)) {
                return defaultStateIndex;
            }

            if (type.GetCustomAttributes(typeof(DefaultAbilityIntData), true).Length > 0) {
                defaultStateIndex = type.GetCustomAttributes(typeof(DefaultAbilityIntData), true)[0] as DefaultAbilityIntData;
            }
            s_DefaultAbilityIntData.Add(type, defaultStateIndex);
            return defaultStateIndex;
        }

        /// <summary>
        /// Returns the DefaultItemStateIndex of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultItemStateIndex of the specified ability type. Can be null.</returns>
        private static DefaultItemStateIndex GetDefaultItemStateIndex(Type type)
        {
            DefaultItemStateIndex defaultItemStateIndex;
            if (s_DefaultItemStateIndex.TryGetValue(type, out defaultItemStateIndex)) {
                return defaultItemStateIndex;
            }

            if (type.GetCustomAttributes(typeof(DefaultItemStateIndex), true).Length > 0) {
                defaultItemStateIndex = type.GetCustomAttributes(typeof(DefaultItemStateIndex), true)[0] as DefaultItemStateIndex;
            }
            s_DefaultItemStateIndex.Add(type, defaultItemStateIndex);
            return defaultItemStateIndex;
        }

        /// <summary>
        /// Returns the DefaultState of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultState of the specified ability type. Can be null.</returns>
        private static DefaultState GetDefaultState(Type type)
        {
            DefaultState defaultState;
            if (s_DefaultState.TryGetValue(type, out defaultState)) {
                return defaultState;
            }

            if (type.GetCustomAttributes(typeof(DefaultState), true).Length > 0) {
                defaultState = type.GetCustomAttributes(typeof(DefaultState), true)[0] as DefaultState;
            }
            s_DefaultState.Add(type, defaultState);
            return defaultState;
        }

        /// <summary>
        /// Returns the DefaultAllowPositionalInput of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultAllowPositionalInput of the specified ability type. Can be null.</returns>
        private static DefaultAllowPositionalInput GetDefaultAllowPositionalInput(Type type)
        {
            DefaultAllowPositionalInput defaultAllowPositionalInput;
            if (s_DefaultAllowPositionalInput.TryGetValue(type, out defaultAllowPositionalInput)) {
                return defaultAllowPositionalInput;
            }

            if (type.GetCustomAttributes(typeof(DefaultAllowPositionalInput), true).Length > 0) {
                defaultAllowPositionalInput = type.GetCustomAttributes(typeof(DefaultAllowPositionalInput), true)[0] as DefaultAllowPositionalInput;
            }
            s_DefaultAllowPositionalInput.Add(type, defaultAllowPositionalInput);
            return defaultAllowPositionalInput;
        }

        /// <summary>
        /// Returns the DefaultAllowRotationalInput of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultAllowRotationalInput of the specified ability type. Can be null.</returns>
        private static DefaultAllowRotationalInput GetDefaultAllowRotationalInput(Type type)
        {
            DefaultAllowRotationalInput defaultAllowRotationalInput;
            if (s_DefaultAllowRotationalInput.TryGetValue(type, out defaultAllowRotationalInput)) {
                return defaultAllowRotationalInput;
            }

            if (type.GetCustomAttributes(typeof(DefaultAllowRotationalInput), true).Length > 0) {
                defaultAllowRotationalInput = type.GetCustomAttributes(typeof(DefaultAllowRotationalInput), true)[0] as DefaultAllowRotationalInput;
            }
            s_DefaultAllowRotationalInput.Add(type, defaultAllowRotationalInput);
            return defaultAllowRotationalInput;
        }

        /// <summary>
        /// Returns the DefaultUseGravity of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultUseGravity of the specified ability type. Can be null.</returns>
        private static DefaultUseGravity GetDefaultUseGravity(Type type)
        {
            DefaultUseGravity defaultUseGravity;
            if (s_DefaultUseGravity.TryGetValue(type, out defaultUseGravity)) {
                return defaultUseGravity;
            }

            if (type.GetCustomAttributes(typeof(DefaultUseGravity), true).Length > 0) {
                defaultUseGravity = type.GetCustomAttributes(typeof(DefaultUseGravity), true)[0] as DefaultUseGravity;
            }
            s_DefaultUseGravity.Add(type, defaultUseGravity);
            return defaultUseGravity;
        }

        /// <summary>
        /// Returns the DefaultUseRootMotionPosition of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultUseRootMotionPosition of the specified ability type. Can be null.</returns>
        private static DefaultUseRootMotionPosition GetDefaultUseRootMotionPosition(Type type)
        {
            DefaultUseRootMotionPosition defaultUseRootMotionPosition;
            if (s_DefaultUseRootMotionPosition.TryGetValue(type, out defaultUseRootMotionPosition)) {
                return defaultUseRootMotionPosition;
            }

            if (type.GetCustomAttributes(typeof(DefaultUseRootMotionPosition), true).Length > 0) {
                defaultUseRootMotionPosition = type.GetCustomAttributes(typeof(DefaultUseRootMotionPosition), true)[0] as DefaultUseRootMotionPosition;
            }
            s_DefaultUseRootMotionPosition.Add(type, defaultUseRootMotionPosition);
            return defaultUseRootMotionPosition;
        }

        /// <summary>
        /// Returns the DefaultUseRootMotionRotation of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultUseRootMotionRotation of the specified ability type. Can be null.</returns>
        private static DefaultUseRootMotionRotation GetDefaultUseRootMotionRotation(Type type)
        {
            DefaultUseRootMotionRotation defaultUseRootMotionRotation;
            if (s_DefaultUseRootMotionRotation.TryGetValue(type, out defaultUseRootMotionRotation)) {
                return defaultUseRootMotionRotation;
            }

            if (type.GetCustomAttributes(typeof(DefaultUseRootMotionRotation), true).Length > 0) {
                defaultUseRootMotionRotation = type.GetCustomAttributes(typeof(DefaultUseRootMotionRotation), true)[0] as DefaultUseRootMotionRotation;
            }
            s_DefaultUseRootMotionRotation.Add(type, defaultUseRootMotionRotation);
            return defaultUseRootMotionRotation;
        }

        /// <summary>
        /// Returns the DefaultDetectHorizontalCollisions of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultDetectHorizontalCollisions of the specified ability type. Can be null.</returns>
        private static DefaultDetectHorizontalCollisions GetDefaultDetectHorizontalCollisions(Type type)
        {
            DefaultDetectHorizontalCollisions defaultDetectHorizontalCollisions;
            if (s_DefaultDetectHorizontalCollisions.TryGetValue(type, out defaultDetectHorizontalCollisions)) {
                return defaultDetectHorizontalCollisions;
            }

            if (type.GetCustomAttributes(typeof(DefaultDetectHorizontalCollisions), true).Length > 0) {
                defaultDetectHorizontalCollisions = type.GetCustomAttributes(typeof(DefaultDetectHorizontalCollisions), true)[0] as DefaultDetectHorizontalCollisions;
            }
            s_DefaultDetectHorizontalCollisions.Add(type, defaultDetectHorizontalCollisions);
            return defaultDetectHorizontalCollisions;
        }

        /// <summary>
        /// Returns the DefaultDetectVerticalCollisions of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultDetectVerticalCollisions of the specified ability type. Can be null.</returns>
        private static DefaultDetectVerticalCollisions GetDefaultDetectVerticalCollisions(Type type)
        {
            DefaultDetectVerticalCollisions defaultDetectVerticalCollisions;
            if (s_DefaultDetectVerticalCollisions.TryGetValue(type, out defaultDetectVerticalCollisions)) {
                return defaultDetectVerticalCollisions;
            }

            if (type.GetCustomAttributes(typeof(DefaultDetectVerticalCollisions), true).Length > 0) {
                defaultDetectVerticalCollisions = type.GetCustomAttributes(typeof(DefaultDetectVerticalCollisions), true)[0] as DefaultDetectVerticalCollisions;
            }
            s_DefaultDetectVerticalCollisions.Add(type, defaultDetectVerticalCollisions);
            return defaultDetectVerticalCollisions;
        }

        /// <summary>
        /// Returns the DefaultObjectDetection of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultObjectDetection of the specified ability type. Can be null.</returns>
        private static DefaultObjectDetection GetDefaultObjectDetection(Type type)
        {
            DefaultObjectDetection defaultObjectDetection;
            if (s_DefaultObjectDetection.TryGetValue(type, out defaultObjectDetection)) {
                return defaultObjectDetection;
            }

            if (type.GetCustomAttributes(typeof(DefaultObjectDetection), true).Length > 0) {
                defaultObjectDetection = type.GetCustomAttributes(typeof(DefaultObjectDetection), true)[0] as DefaultObjectDetection;
            }
            s_DefaultObjectDetection.Add(type, defaultObjectDetection);
            return defaultObjectDetection;
        }

        /// <summary>
        /// Returns the DefaultUseLookDirection of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultUseLookDirection of the specified ability type. Can be null.</returns>
        private static DefaultUseLookDirection GetDefaultUseLookDirection(Type type)
        {
            DefaultUseLookDirection defaultUseLookDirection;
            if (s_DefaultUseLookDirection.TryGetValue(type, out defaultUseLookDirection)) {
                return defaultUseLookDirection;
            }

            if (type.GetCustomAttributes(typeof(DefaultUseLookDirection), true).Length > 0) {
                defaultUseLookDirection = type.GetCustomAttributes(typeof(DefaultUseLookDirection), true)[0] as DefaultUseLookDirection;
            }
            s_DefaultUseLookDirection.Add(type, defaultUseLookDirection);
            return defaultUseLookDirection;
        }

        /// <summary>
        /// Returns the DefaultCastOffset of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultCastOffset of the specified ability type. Can be null.</returns>
        private static DefaultCastOffset GetDefaultCastOffset(Type type)
        {
            DefaultCastOffset defaultCastOffset;
            if (s_DefaultCastOffset.TryGetValue(type, out defaultCastOffset)) {
                return defaultCastOffset;
            }

            if (type.GetCustomAttributes(typeof(DefaultCastOffset), true).Length > 0) {
                defaultCastOffset = type.GetCustomAttributes(typeof(DefaultCastOffset), true)[0] as DefaultCastOffset;
            }
            s_DefaultCastOffset.Add(type, defaultCastOffset);
            return defaultCastOffset;
        }

        /// <summary>
        /// Returns the DefaultEquippedSlots of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultEquippedSlots of the specified ability type. Can be null.</returns>
        private static DefaultEquippedSlots GetDefaultEquippedSlots(Type type)
        {
            DefaultEquippedSlots defaultEquippedSlots;
            if (s_DefaultEquippedSlots.TryGetValue(type, out defaultEquippedSlots)) {
                return defaultEquippedSlots;
            }

            if (type.GetCustomAttributes(typeof(DefaultEquippedSlots), true).Length > 0) {
                defaultEquippedSlots = type.GetCustomAttributes(typeof(DefaultEquippedSlots), true)[0] as DefaultEquippedSlots;
            }
            s_DefaultEquippedSlots.Add(type, defaultEquippedSlots);
            return defaultEquippedSlots;
        }

        /// <summary>
        /// Returns the DefaultReequipSlots of the specified ability type.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <returns>The DefaultReequipSlots of the specified ability type. Can be null.</returns>
        private static DefaultReequipSlots GetDefaultReequipSlots(Type type)
        {
            DefaultReequipSlots defaultReequipSlots;
            if (s_DefaultReequipSlots.TryGetValue(type, out defaultReequipSlots)) {
                return defaultReequipSlots;
            }

            if (type.GetCustomAttributes(typeof(DefaultReequipSlots), true).Length > 0) {
                defaultReequipSlots = type.GetCustomAttributes(typeof(DefaultReequipSlots), true)[0] as DefaultReequipSlots;
            }
            s_DefaultReequipSlots.Add(type, defaultReequipSlots);
            return defaultReequipSlots;
        }

        /// <summary>
        /// Returns the AddState of the specified ability type.
        /// </summary>
        /// <param name="type">The view type.</param>
        /// <returns>The AddState of the specified ability type. Can be null.</returns>
        private static AddState[] GetAddStates(Type type)
        {
            AddState[] addStates;
            if (s_AddStates.TryGetValue(type, out addStates)) {
                return addStates;
            }

            if (type.GetCustomAttributes(typeof(AddState), true).Length > 0) {
                addStates = type.GetCustomAttributes(typeof(AddState), true) as AddState[];
            }
            s_AddStates.Add(type, addStates);
            return addStates;
        }
    }
}