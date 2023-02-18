/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Editor.Utility;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.StateSystem;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.Utility.Builders;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws the Character Builder settings within the window.
    /// </summary>
    [Serializable]
    [OrderedEditorItem("Character", 2)]
    public class CharacterManager : Manager
    {
        /// <summary>
        /// Specifies the perspective that the character can change into.
        /// </summary>
        private enum Perspective
        {
            First,  // The character can only be in first person perspective.
            Third,  // The character can only be in third person perspective.
            Both,   // The character can be in first or third person perspective.
            None    // Default value.
        }
        /// <summary>
        /// Specifies if the rig is a humanoid or generic. Humanoids allow for animation retargetting whereas generic characters do not.
        /// </summary>
        private enum ModelType { Humanoid, Generic }

        private string[] m_ToolbarStrings = { "New Character", "Existing Character" };
        [SerializeField] private bool m_DrawNewCharacter = true;

        [SerializeField] private GameObject m_AddCharacter;
        [SerializeField] private Perspective m_AddPerspective = Perspective.None;
        [SerializeField] private string m_FirstPersonMovementType;
        [SerializeField] private string m_ThirdPersonMovementType;
        [SerializeField] private bool m_StartFirstPersonPerspective = true;
        [SerializeField] private bool m_AddAnimator = true;
        [SerializeField] private ModelType m_AddModelType;
        [SerializeField] private RuntimeAnimatorController m_AddAnimatorController;
        [SerializeField] private GameObject[] m_AddFirstPersonArms;
        [SerializeField] private RuntimeAnimatorController[] m_AddFirstPersonArmsAnimatorController;
        [SerializeField] private GameObject[] m_AddFirstPersonHiddenObjects;
        [SerializeField] private bool m_AddStandardAbilities = true;
        [SerializeField] private bool m_AddAIAgent = false;
        [SerializeField] private bool m_AddNavMeshAgent = false;
        [SerializeField] private bool m_AddItems = true;
        [SerializeField] private Inventory.ItemCollection m_AddItemCollection;
        [SerializeField] private bool m_AddHealth = true;
        [SerializeField] private bool m_AddUnityIK = true;
        [SerializeField] private bool m_AddFootEffects = true;
        [SerializeField] private bool m_AddRagdoll = true;
        [SerializeField] private StateConfiguration m_AddStateConfiguration;
        [SerializeField] private int m_AddProfileIndex;
        [SerializeField] private string m_AddProfileName;

        [SerializeField] private GameObject m_ExistingCharacter;
        [SerializeField] private Perspective m_ExistingPerspective;
        [SerializeField] private bool m_ExistingAnimator = true;
        [SerializeField] private ModelType m_ExistingModelType;
        [SerializeField] private RuntimeAnimatorController m_ExistingAnimatorController;
        [SerializeField] private GameObject[] m_OriginalExistingFirstPersonArms;
        [SerializeField] private GameObject[] m_ExistingFirstPersonArms;
        [SerializeField] private RuntimeAnimatorController[] m_ExistingFirstPersonArmsAnimatorController;
        [SerializeField] private GameObject[] m_OriginalExistingFirstPersonHiddenObjects;
        [SerializeField] private GameObject[] m_ExistingFirstPersonHiddenObjects;
        [SerializeField] private bool m_ExistingAIAgent = false;
        [SerializeField] private bool m_ExistingNavMeshAgent = true;
        [SerializeField] private bool m_ExistingItems = true;
        [SerializeField] private Inventory.ItemCollection m_ExistingItemCollection;
        [SerializeField] private bool m_ExistingHealth = true;
        [SerializeField] private bool m_ExistingUnityIK = true;
        [SerializeField] private bool m_ExistingFootEffects = true;
        [SerializeField] private StateConfiguration m_ExistingStateConfiguration;
        [SerializeField] private int m_ExistingProfileIndex;
        [SerializeField] private string m_ExistingProfileName;

        private List<Type> m_FirstPersonMovementTypes = new List<Type>();
        private string[] m_FirstPersonMovementTypeStrings;
        private List<Type> m_ThirdPersonMovementTypes = new List<Type>();
        private string[] m_ThirdPersonMovementTypeStrings;
        private string[] m_PerspectiveNames = { "First", "Third", "Both" };

        private Material m_InvisibleShadowCaster;

        /// <summary>
        /// Initialize the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            // Set the default perspective based on what asset is installed.
            if (m_AddPerspective == Perspective.None) {
#if FIRST_PERSON_CONTROLLER
                m_AddPerspective = Perspective.First;
#elif THIRD_PERSON_CONTROLLER
                m_AddPerspective = Perspective.Third;
#endif
            }

            // Get a list of the available movement types.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must derive from MovementType.
                    if (!typeof(Character.MovementTypes.MovementType).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    if (assemblyTypes[j].FullName.Contains("FirstPersonController")) {
                        m_FirstPersonMovementTypes.Add(assemblyTypes[j]);
                    } else if (assemblyTypes[j].FullName.Contains("ThirdPersonController")) {
                        m_ThirdPersonMovementTypes.Add(assemblyTypes[j]);
                    }
                }
            }

            // Create an array of display names for the popup.
            if (m_FirstPersonMovementTypes.Count > 0) {
                m_FirstPersonMovementTypeStrings = new string[m_FirstPersonMovementTypes.Count];
                for (int i = 0; i < m_FirstPersonMovementTypes.Count; ++i) {
                    m_FirstPersonMovementTypeStrings[i] = InspectorUtility.DisplayTypeName(m_FirstPersonMovementTypes[i], true);
                }
            }
            if (m_ThirdPersonMovementTypes.Count > 0) {
                m_ThirdPersonMovementTypeStrings = new string[m_ThirdPersonMovementTypes.Count];
                for (int i = 0; i < m_ThirdPersonMovementTypes.Count; ++i) {
                    m_ThirdPersonMovementTypeStrings[i] = InspectorUtility.DisplayTypeName(m_ThirdPersonMovementTypes[i], true);
                }
            }

            // Look for the ItemCollection within the scene if it isn't already populated.
            if (m_AddItemCollection == null || m_ExistingItemCollection == null) {
                m_AddItemCollection = m_ExistingItemCollection = ManagerUtility.FindItemCollection(m_MainManagerWindow);
            }

            // Find the state configuration.
            var stateConfiguration = ManagerUtility.FindStateConfiguration(m_MainManagerWindow);
            if (stateConfiguration != null) {
                if (m_AddStateConfiguration == null) {
                    m_AddStateConfiguration = stateConfiguration;
                }
                if (m_ExistingStateConfiguration == null) {
                    m_ExistingStateConfiguration = stateConfiguration;
                }
            }

            m_InvisibleShadowCaster = ManagerUtility.FindInvisibleShadowCaster(m_MainManagerWindow);
        }

        /// <summary>
        /// Draws the Manager.
        /// </summary>
        public override void OnGUI()
        {
            var toolbarSelection = GUILayout.Toolbar(m_DrawNewCharacter ? 0 : 1, m_ToolbarStrings, EditorStyles.toolbarButton);
            m_DrawNewCharacter = toolbarSelection == 0;
            GUILayout.Space(10);

            if (m_DrawNewCharacter) {
                DrawNewCharacter();
            } else {
                DrawExistingCharacter();
            }
        }

        /// <summary>
        /// Draws the controls for a new character.
        /// </summary>
        private void DrawNewCharacter()
        {
            DrawPerspective(ref m_AddPerspective);
            if (m_AddPerspective == Perspective.Both) {
                m_StartFirstPersonPerspective = EditorGUILayout.Popup("Start Perspective", m_StartFirstPersonPerspective ? 0 : 1, new string[] { "First Person", "Third Person" }) == 0;
            } else {
                m_StartFirstPersonPerspective = (m_AddPerspective == Perspective.First);
            }

            // Show the available first person MovementTypes.
            if (m_AddPerspective == Perspective.First || m_AddPerspective == Perspective.Both) {
                var selectedMovementType = -1;
                for (int i = 0; i < m_FirstPersonMovementTypes.Count; ++i) {
                    if (m_FirstPersonMovementTypes[i].FullName == m_FirstPersonMovementType) {
                        selectedMovementType = i;
                        break;
                    }
                }
                var movementType = selectedMovementType == -1 ? 0 : selectedMovementType;
                selectedMovementType = EditorGUILayout.Popup("First Person Movement", movementType, m_FirstPersonMovementTypeStrings);
                if (movementType != selectedMovementType || string.IsNullOrEmpty(m_FirstPersonMovementType)) {
                    m_FirstPersonMovementType = m_FirstPersonMovementTypes[selectedMovementType].FullName;
                }
                if (m_AddPerspective != Perspective.Both) {
                    m_ThirdPersonMovementType = string.Empty;
                }
            }
            // Show the available third person MovementTypes.
            if (m_AddPerspective == Perspective.Third || m_AddPerspective == Perspective.Both) {
                var selectedMovementType = -1;
                for (int i = 0; i < m_ThirdPersonMovementTypes.Count; ++i) {
                    if (m_ThirdPersonMovementTypes[i].FullName == m_ThirdPersonMovementType) {
                        selectedMovementType = i;
                        break;
                    }
                }
                var movementType = selectedMovementType == -1 ? 0 : selectedMovementType;
                selectedMovementType = EditorGUILayout.Popup("Third Person Movement", movementType, m_ThirdPersonMovementTypeStrings);
                if (movementType != selectedMovementType || string.IsNullOrEmpty(m_ThirdPersonMovementType)) {
                    m_ThirdPersonMovementType = m_ThirdPersonMovementTypes[selectedMovementType].FullName;
                }
                if (m_AddPerspective != Perspective.Both) {
                    m_FirstPersonMovementType = string.Empty;
                }
            }

            var character = EditorGUILayout.ObjectField("Character", m_AddCharacter, typeof(GameObject), true) as GameObject;
            if (character != m_AddCharacter) {
                m_AddCharacter = character;

                if (IsValidHumanoid(m_AddCharacter)) {
                    m_AddModelType = ModelType.Humanoid;
                } else {
                    m_AddModelType = ModelType.Generic;
                }

                if (m_AddModelType == ModelType.Humanoid) {
                    m_AddAnimatorController = ManagerUtility.FindAnimatorController(m_MainManagerWindow);
                }
            }

            // An animator is required for third person and AI agents.
            var validCharacter = CheckValidCharacter(m_AddCharacter, true);
            GUI.enabled = validCharacter && m_AddPerspective == Perspective.First;
            var prevAddAnimator = m_AddAnimator;
            var canBuild = DrawAnimator(m_AddCharacter, m_AddPerspective != Perspective.First, validCharacter, ref m_AddAnimator, ref m_AddModelType, ref m_AddAnimatorController);
            if (prevAddAnimator && !m_AddAnimator) {
                m_AddUnityIK = m_AddFootEffects = m_AddRagdoll = false;
            }
            GUI.enabled = validCharacter && canBuild;

            if (m_AddPerspective == Perspective.First || m_AddPerspective == Perspective.Both) {
                GUILayout.Space(7);
                DrawFirstPersonArms(ref m_AddFirstPersonArms, ref m_AddFirstPersonArmsAnimatorController);
                if (m_AddCharacter != null) {
                    GUILayout.Space(5);
                    DrawFirstPersonHiddenObjects(ref m_AddFirstPersonHiddenObjects);
                }
                GUILayout.Space(7);
            }

            GUILayout.Space(5);
            // Reduce clutter by having an advanced foldout.
            if (InspectorUtility.Foldout(this, "Advanced")) {
                EditorGUI.indentLevel += 2;
                // Abilities are added and removed through the inspector after the character has been built.
                m_AddStandardAbilities = EditorGUILayout.Toggle("Standard Abilities", m_AddStandardAbilities);
                DrawAdvancedComponents(m_AddAnimator, ref m_AddAIAgent, ref m_AddNavMeshAgent, ref m_AddItems, ref m_AddItemCollection, ref m_AddHealth, ref m_AddUnityIK, 
                                        ref m_AddFootEffects, true, canBuild, m_AddCharacter);
                DrawProfileFields(ref m_AddStateConfiguration, ref m_AddProfileIndex, ref m_AddProfileName, null);
                if (m_AddItems && m_AddItemCollection == null) {
                    canBuild = false;
                }
                if (m_AddUnityIK && !IsValidHumanoid(character)) {
                    canBuild = false;
                }
                if (m_AddFootEffects && !m_AddAnimator) {
                    canBuild = false;
                }
                if (m_AddRagdoll && !IsValidHumanoid(character)) {
                    canBuild = false;
                }
                EditorGUI.indentLevel -= 2;
            }

            // If the character hasn't been built yet then it should be created.
            GUI.enabled = validCharacter && canBuild;
            GUILayout.Space(5);
            if (GUILayout.Button("Build Character")) {
                // The first person perspective allows for null characters.
                if (m_AddCharacter == null) {
                    m_AddCharacter = new GameObject("First Person Character");
                }
                var origCharacter = m_AddCharacter;
                if (EditorUtility.IsPersistent(m_AddCharacter)) {
                    var name = m_AddCharacter.name;
                    m_AddCharacter = GameObject.Instantiate(m_AddCharacter) as GameObject;
                    m_AddCharacter.name = name;
                }

                CharacterBuilder.BuildCharacter(m_AddCharacter, m_AddAnimator, m_AddAnimatorController, m_FirstPersonMovementType, m_ThirdPersonMovementType, m_StartFirstPersonPerspective, 
                                                    m_AddFirstPersonHiddenObjects, m_InvisibleShadowCaster, m_AddAIAgent);
                CharacterBuilder.BuildCharacterComponents(m_AddCharacter, m_AddAIAgent, m_AddItems, m_AddItemCollection, !string.IsNullOrEmpty(m_FirstPersonMovementType) || (m_AddFirstPersonArms != null && m_AddFirstPersonArms.Length > 1),
                    m_AddHealth, m_AddUnityIK, m_AddFootEffects, m_AddStandardAbilities, m_AddNavMeshAgent);
#if FIRST_PERSON_CONTROLLER
                if (m_AddFirstPersonArms != null) {
                    var firstPersonObjects = m_AddCharacter.GetComponentInChildren<FirstPersonController.Character.FirstPersonObjects>();
                    if (firstPersonObjects != null) {
                        for (int i = 0; i < m_AddFirstPersonArms.Length; ++i) {
                            if (m_AddFirstPersonArms[i] == null) {
                                continue;
                            }

                            if (EditorUtility.IsPersistent(m_AddFirstPersonArms[i])) {
                                var name = m_AddFirstPersonArms[i].name;
                                m_AddFirstPersonArms[i] = GameObject.Instantiate(m_AddFirstPersonArms[i]) as GameObject;
                                m_AddFirstPersonArms[i].name = name;
                            }

                            ItemBuilder.AddFirstPersonArms(m_AddCharacter, m_AddFirstPersonArms[i], m_AddFirstPersonArmsAnimatorController[i]);
                            m_AddFirstPersonArms[i].transform.SetParentOrigin(firstPersonObjects.transform);
                        }
                    }
                }
#endif
                if (m_AddRagdoll) {
                    // Add the ragdoll ability and open Unity's ragdoll builder.
                    var characterLocomotion = m_AddCharacter.GetComponent<UltimateCharacterLocomotion>();
                    AbilityBuilder.AddAbility(characterLocomotion, typeof(Character.Abilities.Ragdoll));
                    Inspectors.Character.Abilities.RagdollInspectorDrawer.AddRagdollColliders(characterLocomotion.gameObject);
                }
                if (m_AddAnimator) {
                    // Ensure the Animator Controller has the required parameters.
                    AnimatorBuilder.AddParameters((UnityEditor.Animations.AnimatorController)m_AddAnimatorController);
                }
                if (m_AddStateConfiguration != null) {
                    if (m_AddProfileIndex > 0) {
                        m_AddStateConfiguration.AddStatesToGameObject(m_AddProfileName, m_AddCharacter);
                        InspectorUtility.SetDirty(m_AddCharacter);
                    }
                }
                m_AddFirstPersonHiddenObjects = null;
                Selection.activeObject = m_AddCharacter;
                m_AddCharacter = origCharacter;
            }
            GUI.enabled = true;
        }

        /// <summary>
        /// Draws the perspective controls.
        /// </summary>
        private void DrawPerspective(ref Perspective perspective)
        {
            var selectedPerspective = (Perspective)EditorGUILayout.Popup("Perspective", (int)perspective, m_PerspectiveNames);
            var canSwitchPerspective = true;
            // Determine if the selected perspective is supported.
#if !FIRST_PERSON_CONTROLLER
            if (selectedPerspective == Perspective.First || selectedPerspective == Perspective.Both) {
                Debug.LogError("Unable to select the first person perspective. If you'd like to create a first person character ensure the Firts Person Controller is imported.");
                canSwitchPerspective = false;
            }
#endif
#if !THIRD_PERSON_CONTROLLER
            if (selectedPerspective == Perspective.Third || selectedPerspective == Perspective.Both) {
                Debug.LogError("Unable to select the third person perspective. If you'd like to create a third person character ensure the Third Person Controller is imported.");
                canSwitchPerspective = false;
            }
#endif
            if (canSwitchPerspective && selectedPerspective != perspective) {
                perspective = selectedPerspective;
            }
        }

        /// <summary>
        /// Is the character a valid character?
        /// </summary>
        /// <param name="character">The character to check against.</param>
        /// <param name="buildCharacter">Is the character a new character?</param>
        /// <returns>True if the character is a valid character.</returns>
        private bool CheckValidCharacter(GameObject character, bool buildCharacter)
        {
            // A character is required in order to add or remove components from it.
            if (character == null) {
                // The first person perspective doesn't need a predefined Character GameObject.
                if (buildCharacter && m_AddPerspective == Perspective.First) {
                    return true;
                }
                EditorGUILayout.HelpBox("Select the GameObject which will be used as the character. This object will have the majority of the components added to it.",
                                    MessageType.Error);
                return false;
            }

            var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
            if (buildCharacter && characterLocomotion != null) {
                EditorGUILayout.HelpBox("The character has already been created. Use the \"Existing Character\" tab to modify the character components.", MessageType.Error);
                return false;
            }

            if (!buildCharacter && characterLocomotion == null) {
                EditorGUILayout.HelpBox("The character hasn't been created. Use the \"New Character\" tab to build the character.", MessageType.Error);
                return false;
            }
            
            if (EditorUtility.IsPersistent(character)) {
                EditorGUILayout.HelpBox("Please drag your character into the scene. The Character Manager cannot add components to prefabs.", MessageType.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Draws the animator controls.
        /// </summary>
        private bool DrawAnimator(GameObject character, bool forceAdd, bool enableGUI, ref bool animator, ref ModelType modelType, ref RuntimeAnimatorController animatorController)
        {
            animator = EditorGUILayout.Toggle("Animator", animator || forceAdd);
            GUI.enabled = enableGUI;
            if (animator) {
                if (character == null) {
                    if (enableGUI) {
                        EditorGUILayout.HelpBox("A rigged character must be selected in order for an Animator to be added.", MessageType.Error);
                    }
                    return false;
                }
                EditorGUI.indentLevel++;
                var selectedModelType = (ModelType)EditorGUILayout.EnumPopup("Model Type", modelType);
                if (selectedModelType != modelType) {
                    modelType = selectedModelType;
                    if (modelType == ModelType.Humanoid) {
                        // Humanoids support retargetting so can use the demo controller.
                        animatorController = ManagerUtility.FindAnimatorController(m_MainManagerWindow);
                    } else {
                        // Generic characters require a custom animator controller.
                        animatorController = null;
                    }
                }
                if (modelType == ModelType.Humanoid && !IsValidHumanoid(character)) {
                    animatorController = null;
                    if (character != null) {
                        EditorGUILayout.HelpBox("The specified character is not a humanoid.\nHumanoid characters should have the avatar is set to humanoid and have a head bone assigned.", MessageType.Error);
                    }
                }
                // The non-default animator controller can be selected.
                animatorController = EditorGUILayout.ObjectField("Animator Controller", animatorController, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;

                EditorGUI.indentLevel--;
            }
            return !animator || animatorController != null;
        }

        /// <summary>
        /// Is the character a valid humanoid?
        /// </summary>
        /// <param name="character">The character GameObject to check against.</param>
        /// <returns>True if the character is a valid humanoid.</returns>
        private bool IsValidHumanoid(GameObject character)
        {
            if (character == null) {
                return false;
            }
            var spawnedCharacter = false;
            // The character has to be spawned in order to be able to detect if it is a Humanoid.
            if (AssetDatabase.GetAssetPath(character).Length > 0) {
                character = GameObject.Instantiate(character) as GameObject;
                spawnedCharacter = true;
            }
            var animator = character.GetComponent<Animator>();
            var hasAnimator = animator != null;
            if (!hasAnimator) {
                animator = character.AddComponent<Animator>();
            }
            // A human will have a head.
            var isHumanoid = animator.GetBoneTransform(HumanBodyBones.Head) != null;
            // GetBoneTransform sometimes returns a false negative.
            if (!isHumanoid) {
                isHumanoid = animator.isHuman;
            }
            // Clean up.
            if (!hasAnimator) {
                UnityEngine.Object.DestroyImmediate(animator, true);
            }
            if (spawnedCharacter) {
                UnityEngine.Object.DestroyImmediate(character, true);
            }
            return isHumanoid;
        }

        /// <summary>
        /// Draws the controls for the objects that will act as the first person arms.
        /// </summary>
        private void DrawFirstPersonArms(ref GameObject[] firstPersonArms, ref RuntimeAnimatorController[] firstPersonArmsAnimator)
        {
            if (firstPersonArms == null || firstPersonArms.Length == 0) {
                firstPersonArms = new GameObject[1];
                firstPersonArmsAnimator = new RuntimeAnimatorController[1];
            }

            for (int i = 0; i < firstPersonArms.Length; ++i) {
                GUILayout.BeginHorizontal();
                var guiContent = new GUIContent(i == 0 ? "First Person Arms" : " ", "First person objects that are the base for the items. The second field contains a reference to the Animator Controller that the arms should use.");
                firstPersonArms[i] = EditorGUILayout.ObjectField(guiContent, firstPersonArms[i], typeof(GameObject), true) as GameObject;
                if (firstPersonArms[i] != null) {
                    firstPersonArmsAnimator[i] = EditorGUILayout.ObjectField(firstPersonArmsAnimator[i], typeof(RuntimeAnimatorController), true, GUILayout.MaxWidth(180)) as RuntimeAnimatorController;
                }
                if (i == firstPersonArms.Length - 1 && firstPersonArms[i] != null) {
                    Array.Resize(ref firstPersonArms, firstPersonArms.Length + 1);
                    Array.Resize(ref firstPersonArmsAnimator, firstPersonArmsAnimator.Length + 1);
                    break;
                }
                if (i != firstPersonArms.Length - 1 && GUILayout.Button(InspectorStyles.RemoveIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                    var arms = new List<GameObject>(firstPersonArms);
                    arms.RemoveAt(i);
                    firstPersonArms = arms.ToArray();
                    var animators = new List<RuntimeAnimatorController>(firstPersonArmsAnimator);
                    animators.RemoveAt(i);
                    firstPersonArmsAnimator = animators.ToArray();
                    break;
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draws the controls for the objects that will be hidden while in first person view.
        /// </summary>
        private void DrawFirstPersonHiddenObjects(ref GameObject[] firstPersonObjects)
        {
            if (firstPersonObjects == null || firstPersonObjects.Length == 0) {
                firstPersonObjects = new GameObject[1];
            }

            for (int i = 0; i < firstPersonObjects.Length; ++i) {
                GUILayout.BeginHorizontal();
                var guiContent = new GUIContent(i == 0 ? "Third Person Objects" : " ", "Third person objects that should be hidden when the first person view is active (character arms and head).");
                firstPersonObjects[i] = EditorGUILayout.ObjectField(guiContent, firstPersonObjects[i], typeof(GameObject), true) as GameObject;
                if (i == firstPersonObjects.Length - 1 && firstPersonObjects[i] != null) {
                    Array.Resize(ref firstPersonObjects, firstPersonObjects.Length + 1);
                    break;
                }
                if (i != firstPersonObjects.Length - 1 && GUILayout.Button(InspectorStyles.RemoveIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                    var list = new List<GameObject>(firstPersonObjects);
                    list.RemoveAt(i);
                    firstPersonObjects = list.ToArray();
                    break;
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draws the controls for the advanced character components.
        /// </summary>
        private void DrawAdvancedComponents(bool addAnimator, ref bool aiAgent, ref bool navMeshAgent, ref bool items, ref Inventory.ItemCollection itemCollection, ref bool health,ref bool unityIK,
                                                ref bool footEffects, bool drawRagdoll, bool showError, GameObject character)
        {
            aiAgent = EditorGUILayout.Toggle("AI Agent", aiAgent);
            if (aiAgent) {
                EditorGUI.indentLevel++;
                navMeshAgent = EditorGUILayout.Toggle("NavMeshAgent", navMeshAgent);
                EditorGUI.indentLevel--;
            }
            items = EditorGUILayout.Toggle("Items", items);
            if (items) {
                EditorGUI.indentLevel++;
                itemCollection = EditorGUILayout.ObjectField("Item Collection", itemCollection, typeof(Inventory.ItemCollection), false) as Inventory.ItemCollection;
                if (itemCollection == null) {
                    EditorGUILayout.HelpBox("An ItemCollection needs to be specified for the character to be created.", MessageType.Error);
                }
                EditorGUI.indentLevel--;
            }
            health = EditorGUILayout.Toggle("Health", health);
            unityIK = EditorGUILayout.Toggle("Unity IK", unityIK);
            if (unityIK && !IsValidHumanoid(character) && showError) {
                EditorGUILayout.HelpBox("The Unity IK component requires a humanoid character with an Animator.", MessageType.Error);
            }
            footEffects = EditorGUILayout.Toggle("Foot Effects", footEffects);
            if (footEffects && !addAnimator && showError) {
                EditorGUILayout.HelpBox("The foot effects component requires an Animator.", MessageType.Error);
            }
            if (drawRagdoll) {
                m_AddRagdoll = EditorGUILayout.Toggle("Ragdoll", m_AddRagdoll);
                if (m_AddRagdoll && !IsValidHumanoid(character) && showError) {
                    EditorGUILayout.HelpBox("Unity's ragdoll system requires a humanoid character with an Animator.", MessageType.Error);
                }
            }
        }

        /// <summary>
        /// Draws the fields related to the profile.
        /// </summary>
        private void DrawProfileFields(ref StateConfiguration stateConfiguration, ref int profileIndex, ref string profileName, GameObject character)
        {
            var updatedStateConfiguration = EditorGUILayout.ObjectField("State Configuration", stateConfiguration, typeof(StateConfiguration), false) as StateConfiguration;
            if (updatedStateConfiguration != stateConfiguration) {
                if (updatedStateConfiguration != null) {
                    EditorPrefs.SetString(ManagerUtility.LastStateConfigurationGUIDString, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(updatedStateConfiguration)));
                } else {
                    EditorPrefs.SetString(ManagerUtility.LastStateConfigurationGUIDString, string.Empty);
                }
                stateConfiguration = updatedStateConfiguration;
            }

            EditorGUI.indentLevel++;
            if (stateConfiguration != null) {
                var profiles = stateConfiguration.GetProfilesForGameObject(character, StateConfiguration.Profile.ProfileType.Character);
                // The character can be added without any profiles.
                profiles.Insert(0, "(None)");
                profileIndex = EditorGUILayout.Popup("Profile", profileIndex, profiles.ToArray());
                profileName = profiles[profileIndex];
            }
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draws the controls for an existing character.
        /// </summary>
        private void DrawExistingCharacter()
        {
            var character = EditorGUILayout.ObjectField("Character", m_ExistingCharacter, typeof(GameObject), true) as GameObject;

            if (character != m_ExistingCharacter) {
                m_ExistingCharacter = character;

                if (m_ExistingCharacter != null) {
                    m_ExistingAIAgent = IsAIAgent(m_ExistingCharacter);
                    if (m_ExistingAIAgent) {
                        m_ExistingNavMeshAgent = HasNavMeshAgent(m_ExistingCharacter);
                    } else {
                        m_ExistingNavMeshAgent = false;
                    }
                    if (!HasEssentials(m_ExistingCharacter, m_ExistingPerspective == Perspective.Both)) {
                        EditorGUILayout.HelpBox("Error: The character doesn't contain all of the essential components. Press \"Add\" to add the missing components.", MessageType.Error);
                        if (GUILayout.Button("Add")) {
                            CharacterBuilder.AddEssentials(m_ExistingCharacter, m_ExistingAnimator, m_ExistingAnimatorController, m_ExistingPerspective == Perspective.Both, m_InvisibleShadowCaster, m_ExistingAIAgent);
                        }
                    }
                    m_ExistingPerspective = CurrentPerspective(m_ExistingCharacter);
                    m_ExistingAnimator = HasAnimator(m_ExistingCharacter);
                    if (m_ExistingAnimator) {
                        var animator = m_ExistingCharacter.GetComponent<Animator>();
                        m_ExistingAnimatorController = animator.runtimeAnimatorController;
                        m_ExistingModelType = IsValidHumanoid(m_ExistingCharacter) ? ModelType.Humanoid : ModelType.Generic;
                    }
#if FIRST_PERSON_CONTROLLER
                    // Find any existing first person arms.
                    var firstPersonBaseObjects = m_ExistingCharacter.GetComponentsInChildren<FirstPersonController.Character.Identifiers.FirstPersonBaseObject>();
                    var existingFirstPersonArms = new List<GameObject>();
                    m_ExistingFirstPersonArmsAnimatorController = new RuntimeAnimatorController[firstPersonBaseObjects.Length];
                    for (int i = 0; i < firstPersonBaseObjects.Length; ++i) {
                        existingFirstPersonArms.Add(firstPersonBaseObjects[i].gameObject);
                        var animator = firstPersonBaseObjects[i].GetComponent<Animator>();
                        if (animator != null) {
                            m_ExistingFirstPersonArmsAnimatorController[i] = animator.runtimeAnimatorController;
                        }
                    }
                    m_OriginalExistingFirstPersonArms = m_ExistingFirstPersonArms = existingFirstPersonArms.ToArray();
#endif

                    // Search for any existing first person hidden objects.
                    var renderers = m_ExistingCharacter.GetComponentsInChildren<Renderer>();
                    var existingFirstPersonObjects = new List<GameObject>();
                    for (int i = 0; i < renderers.Length; ++i) {
                        if (renderers[i].GetComponent<Character.Identifiers.ThirdPersonObject>() != null) {
                            existingFirstPersonObjects.Add(renderers[i].gameObject);
                            continue;
                        }

                        var addGameObject = false;
                        var materials = renderers[i].sharedMaterials;
                        for (int j = 0; j < materials.Length; ++j) {
                            if (materials[j] == m_InvisibleShadowCaster) {
                                addGameObject = true;
                                break;
                            }
                        }
                        if (addGameObject) {
                            existingFirstPersonObjects.Add(renderers[i].gameObject);
                        }
                    }
                    m_OriginalExistingFirstPersonHiddenObjects = m_ExistingFirstPersonHiddenObjects = existingFirstPersonObjects.ToArray();

                    m_ExistingItems = HasItems(m_ExistingCharacter, m_ExistingAIAgent, m_ExistingPerspective != Perspective.Third);
                    if (m_ExistingItems) {
                        var itemSetManager = m_ExistingCharacter.GetComponent<Inventory.ItemSetManager>();
                        if (itemSetManager != null) {
                            m_ExistingItemCollection = itemSetManager.ItemCollection;
                        }
                    }
                    m_ExistingHealth = HasHealth(m_ExistingCharacter);
                    m_ExistingUnityIK = HasUnityIK(m_ExistingCharacter);
                    m_ExistingFootEffects = HasFootEffects(m_ExistingCharacter);
                } else {
                    m_OriginalExistingFirstPersonArms = m_ExistingFirstPersonArms = m_OriginalExistingFirstPersonHiddenObjects = m_ExistingFirstPersonHiddenObjects = null;
                }
            }

            var validCharacter = CheckValidCharacter(m_ExistingCharacter, false);
            GUI.enabled = validCharacter;

            DrawPerspective(ref m_ExistingPerspective);

            GUI.enabled = validCharacter && m_ExistingPerspective == Perspective.First;
            DrawAnimator(m_ExistingCharacter, m_ExistingPerspective != Perspective.First, validCharacter, ref m_ExistingAnimator, ref m_ExistingModelType, ref m_ExistingAnimatorController);
            GUI.enabled = validCharacter;

            if (m_ExistingPerspective == Perspective.First || m_ExistingPerspective == Perspective.Both) {
                GUILayout.Space(7);
                DrawFirstPersonArms(ref m_ExistingFirstPersonArms, ref m_ExistingFirstPersonArmsAnimatorController);
                if (m_ExistingCharacter != null) {
                    GUILayout.Space(5);
                    DrawFirstPersonHiddenObjects(ref m_ExistingFirstPersonHiddenObjects);
                }
                GUILayout.Space(7);
            }

            DrawAdvancedComponents(m_ExistingAnimator, ref m_ExistingAIAgent, ref m_ExistingNavMeshAgent, ref m_ExistingItems, ref m_ExistingItemCollection, 
                                    ref m_ExistingHealth, ref m_ExistingUnityIK, ref m_ExistingFootEffects, false, validCharacter, m_ExistingCharacter);
            DrawProfileFields(ref m_ExistingStateConfiguration, ref m_ExistingProfileIndex, ref m_ExistingProfileName, m_ExistingCharacter);

            GUILayout.Space(5);
            if (GUILayout.Button("Update Character")) {
                if (m_ExistingPerspective != CurrentPerspective(m_ExistingCharacter)) {
#if THIRD_PERSON_CONTROLLER
                    // If the perspective was switched from/to the both perspective then the perspective monitor needs to be added or removed.
                    var perspectiveMonitor = m_ExistingCharacter.GetComponent<UltimateCharacterController.ThirdPersonController.Character.PerspectiveMonitor>();
                    if (m_ExistingPerspective == Perspective.Both) {
                        if (perspectiveMonitor == null) {
                            m_ExistingCharacter.AddComponent<UltimateCharacterController.ThirdPersonController.Character.PerspectiveMonitor>();
                        }
                    } else if (perspectiveMonitor != null) {
                        UnityEngine.Object.DestroyImmediate(perspectiveMonitor, true);
                    }
#endif
#if FIRST_PERSON_CONTROLLER
                    // The First Person Objects component should also be added/removed if the character supports items.
                    if (HasItems(m_ExistingCharacter, m_ExistingAIAgent, false)) {
                        var firstPersonObjects = m_ExistingCharacter.GetComponentInChildren<UltimateCharacterController.FirstPersonController.Character.FirstPersonObjects>();
                        if (m_ExistingPerspective != Perspective.Third && firstPersonObjects == null) {
                            var firstPersonObjectsGameObject = new GameObject("First Person Objects");
                            firstPersonObjectsGameObject.transform.parent = character.transform;
                            firstPersonObjectsGameObject.AddComponent<UltimateCharacterController.FirstPersonController.Character.FirstPersonObjects>();
                        } else if (m_ExistingPerspective == Perspective.Third && firstPersonObjects != null) {
                            UnityEngine.Object.DestroyImmediate(firstPersonObjects, true);
                        }
                    }
#endif
                }
                if (m_ExistingAnimator != HasAnimator(m_ExistingCharacter)) {
                    if (m_ExistingAnimator) {
                        CharacterBuilder.AddAnimator(m_ExistingCharacter, m_ExistingAnimatorController, m_ExistingAIAgent);
                    } else {
                        CharacterBuilder.RemoveAnimator(m_ExistingCharacter);
                    }
                } else if (m_ExistingAnimator) {
                    // The animator controller may have changed.
                    var animator = m_ExistingCharacter.GetComponent<Animator>();
                    if (animator != null && animator.runtimeAnimatorController != m_ExistingAnimatorController) {
                        animator.runtimeAnimatorController = m_ExistingAnimatorController;
                    }
                }

#if FIRST_PERSON_CONTROLLER
                // The arms may have changed.
                var firstPersonArmsChange = (m_OriginalExistingFirstPersonArms != null && m_OriginalExistingFirstPersonArms.Length != (m_ExistingFirstPersonArms.Length - 1)) ||
                                                (m_OriginalExistingFirstPersonArms == null && m_ExistingFirstPersonArms.Length > 1);
                if (m_OriginalExistingFirstPersonArms != null && !firstPersonArmsChange) {
                    for (int i = 0; i < m_OriginalExistingFirstPersonArms.Length; ++i) {
                        var containsObject = false;
                        for (int j = 0; j < m_ExistingFirstPersonArms.Length; ++j) {
                            if (m_OriginalExistingFirstPersonArms[i] == m_ExistingFirstPersonArms[j]) {
                                containsObject = true;
                                break;
                            }
                        }
                        if (!containsObject) {
                            firstPersonArmsChange = true;
                            break;
                        }
                    }

                }
                if (firstPersonArmsChange) {
                    // Remove the original arms who are no longer used.
                    if (m_OriginalExistingFirstPersonArms != null) {
                        for (int i = 0; i < m_OriginalExistingFirstPersonArms.Length; ++i) {
                            if (m_OriginalExistingFirstPersonArms[i] == null) {
                                continue;
                            }
                            var remove = true;
                            for (int j = 0; j < m_ExistingFirstPersonArms.Length; ++j) {
                                if (m_OriginalExistingFirstPersonArms[i] == m_ExistingFirstPersonArms[j]) {
                                    remove = false;
                                    break;
                                }
                            }
                            if (remove) {
                                UnityEngine.Object.DestroyImmediate(m_OriginalExistingFirstPersonArms[i], true);
                            }
                        }
                    }

                    // Add the new objects.
                    var firstPersonObjects = m_ExistingCharacter.GetComponentInChildren<FirstPersonController.Character.FirstPersonObjects>();
                    for (int i = 0; i < m_ExistingFirstPersonArms.Length; ++i) {
                        if (m_ExistingFirstPersonArms[i] == null) {
                            continue;
                        }
                        var add = true;
                        for (int j = 0; j < m_OriginalExistingFirstPersonArms.Length; ++j) {
                            if (m_ExistingFirstPersonArms[i] == m_OriginalExistingFirstPersonArms[j]) {
                                add = false;
                                break;
                            }
                        }

                        if (add) {
                            if (EditorUtility.IsPersistent(m_ExistingFirstPersonArms[i])) {
                                var name = m_ExistingFirstPersonArms[i].name;
                                m_ExistingFirstPersonArms[i] = GameObject.Instantiate(m_ExistingFirstPersonArms[i]) as GameObject;
                                m_ExistingFirstPersonArms[i].name = name;
                            }

                            ItemBuilder.AddFirstPersonArms(m_ExistingCharacter, m_ExistingFirstPersonArms[i], m_ExistingFirstPersonArmsAnimatorController[i]);
                            m_ExistingFirstPersonArms[i].transform.SetParentOrigin(firstPersonObjects.transform);
                        }
                    }
                }
#endif

                // Determine if the hidden objects have changed.
                var firstPersonHiddenObjectChange = (m_OriginalExistingFirstPersonHiddenObjects != null && m_ExistingFirstPersonHiddenObjects.Length != (m_ExistingFirstPersonHiddenObjects.Length - 1)) ||
                                                (m_OriginalExistingFirstPersonHiddenObjects == null && m_ExistingFirstPersonHiddenObjects.Length > 1);
                if (m_OriginalExistingFirstPersonHiddenObjects != null && !firstPersonHiddenObjectChange) {
                    for (int i = 0; i < m_OriginalExistingFirstPersonHiddenObjects.Length; ++i) {
                        var containsObject = false;
                        for (int j = 0; j < m_ExistingFirstPersonHiddenObjects.Length; ++j) {
                            if (m_OriginalExistingFirstPersonHiddenObjects[i] == m_ExistingFirstPersonHiddenObjects[j]) {
                                containsObject = true;
                                break;
                            }
                        }
                        if (!containsObject) {
                            firstPersonHiddenObjectChange = true;
                            break;
                        }
                    }
                }

                if (firstPersonHiddenObjectChange) {
                    // Remove all of the original hidden objects before adding them back again.
                    if (m_OriginalExistingFirstPersonHiddenObjects != null) {
                        for (int i = 0; i < m_OriginalExistingFirstPersonHiddenObjects.Length; ++i) {
                            if (m_OriginalExistingFirstPersonHiddenObjects[i] == null) {
                                continue;
                            }

                            Character.Identifiers.ThirdPersonObject thirdPersonObject;
                            if ((thirdPersonObject = m_OriginalExistingFirstPersonHiddenObjects[i].GetComponent<Character.Identifiers.ThirdPersonObject>())) {
                                UnityEngine.Object.DestroyImmediate(thirdPersonObject, true);
                                continue;
                            }

                            var renderers = m_OriginalExistingFirstPersonHiddenObjects[i].GetComponents<Renderer>();
                            for (int j = 0; j < renderers.Length; ++j) {
                                var materials = renderers[j].sharedMaterials;
                                for (int k = 0; k < materials.Length; ++k) {
                                    if (materials[k] == m_InvisibleShadowCaster) {
                                        materials[k] = null;
                                    }
                                }
                                renderers[j].sharedMaterials = materials;
                            }
                        }
                    }

                    var addThirdPersonObject = false;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                    var networkInfo = m_ExistingCharacter.GetComponent<Networking.INetworkInfo>();
                    if (networkInfo != null) {
                        addThirdPersonObject = true;
                    }
#endif
                    if (!addThirdPersonObject) {
                        var characterLocomotion = m_ExistingCharacter.GetComponent<UltimateCharacterLocomotion>();
                        characterLocomotion.DeserializeMovementTypes();
                        var movementTypes = characterLocomotion.MovementTypes;
                        for (int i = 0; i < movementTypes.Length; ++i) {
                            if (movementTypes[i].GetType().FullName.Contains("ThirdPerson")) {
                                addThirdPersonObject = true;
                                break;
                            }
                        }
                    }

                    // All of the original hidden objects have been removed. Add the new objects.
                    for (int i = 0; i < m_ExistingFirstPersonHiddenObjects.Length; ++i) {
                        if (m_ExistingFirstPersonHiddenObjects[i] == null) {
                            continue;
                        }

                        if (!addThirdPersonObject) {
                            var renderers = m_ExistingFirstPersonHiddenObjects[i].GetComponents<Renderer>();
                            for (int j = 0; j < renderers.Length; ++j) {
                                var materials = renderers[j].sharedMaterials;
                                for (int k = 0; k < materials.Length; ++k) {
                                    materials[k] = m_InvisibleShadowCaster;
                                }
                                renderers[j].sharedMaterials = materials;
                            }
                        } else {
                            // The PerspectiveMonitor component is responsible for switching out the material.
                            m_ExistingFirstPersonHiddenObjects[i].AddComponent<Character.Identifiers.ThirdPersonObject>();
                        }
                    }
                    InspectorUtility.SetDirty(m_ExistingCharacter);
                    m_OriginalExistingFirstPersonHiddenObjects = m_ExistingFirstPersonHiddenObjects;
                    // An empty element will occupy the last existing slot.
                    if (m_OriginalExistingFirstPersonHiddenObjects != null && m_OriginalExistingFirstPersonHiddenObjects.Length > 0) {
                        Array.Resize(ref m_OriginalExistingFirstPersonHiddenObjects, m_OriginalExistingFirstPersonHiddenObjects.Length - 1);
                    }
                }
                if (m_ExistingAIAgent != IsAIAgent(m_ExistingCharacter)) {
                    if (m_ExistingAIAgent) {
                        CharacterBuilder.AddAIAgent(m_ExistingCharacter);
                    } else {
                        CharacterBuilder.RemoveAIAgent(m_ExistingCharacter);
                    }
                }
                if (m_ExistingNavMeshAgent != HasNavMeshAgent(m_ExistingCharacter)) {
                    if (m_ExistingNavMeshAgent) {
                        var characterLocomotion = m_ExistingCharacter.GetComponent<UltimateCharacterLocomotion>();
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
                        var navMeshAgent = m_ExistingCharacter.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        if (navMeshAgent != null) {
                            navMeshAgent.stoppingDistance = 0.1f;
                        }
                    } else {
                        AbilityBuilder.RemoveAbility<Character.Abilities.AI.NavMeshAgentMovement>(m_ExistingCharacter.GetComponent<UltimateCharacterLocomotion>());
                        var navMeshAgent = m_ExistingCharacter.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        if (navMeshAgent != null) {
                            GameObject.DestroyImmediate(navMeshAgent, true);
                        }
                    }
                }
                if (m_ExistingItems != HasItems(m_ExistingCharacter, m_ExistingAIAgent, m_ExistingPerspective != Perspective.Third)) {
                    if (m_ExistingItems) {
                        CharacterBuilder.AddItemSupport(m_ExistingCharacter, m_ExistingItemCollection, m_ExistingAIAgent, m_ExistingPerspective != Perspective.Third);
                    } else {
                        CharacterBuilder.RemoveItemSupport(m_ExistingCharacter);
                    }
                }
                if (m_ExistingHealth != HasHealth(m_ExistingCharacter)) {
                    if (m_ExistingHealth) {
                        CharacterBuilder.AddHealth(m_ExistingCharacter);
                    } else {
                        CharacterBuilder.RemoveHealth(m_ExistingCharacter);
                    }
                }
                if (m_ExistingUnityIK != HasUnityIK(m_ExistingCharacter)) {
                    if (m_ExistingUnityIK) {
                        if (IsValidHumanoid(m_ExistingCharacter)) {
                            CharacterBuilder.AddUnityIK(m_ExistingCharacter);
                        }
                    } else {
                        CharacterBuilder.RemoveUnityIK(m_ExistingCharacter);
                    }
                }
                if (m_ExistingFootEffects != HasFootEffects(m_ExistingCharacter)) {
                    if (m_ExistingFootEffects) {
                        CharacterBuilder.AddFootEffects(m_ExistingCharacter);
                    } else {
                        CharacterBuilder.RemoveFootEffects(m_ExistingCharacter);
                    }
                }
                if (m_ExistingStateConfiguration != null && m_ExistingProfileIndex > 0) {
                    m_ExistingStateConfiguration.AddStatesToGameObject(m_ExistingProfileName, m_ExistingCharacter);
                    InspectorUtility.SetDirty(m_ExistingCharacter);
                }
            }
            GUI.enabled = true;
        }

        /// <summary>
        /// Does the character have the essential components?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character has the essential components.</returns>
        private bool HasEssentials(GameObject character, bool usePerspectiveMonitor)
        {
#if THIRD_PERSON_CONTROLLER
            if (usePerspectiveMonitor && character.GetComponent<UltimateCharacterController.ThirdPersonController.Character.PerspectiveMonitor>() == null) {
                return false;
            }
#endif
            return character.GetComponent<Rigidbody>() != null && character.GetComponent<UltimateCharacterLocomotion>() != null &&
                    character.GetComponent<UltimateCharacterLocomotionHandler>() && character.GetComponent<CharacterLayerManager>() != null;
        }

        /// <summary>
        /// Retrieves the current perspective of the character.
        /// </summary>
        /// <param name="character">The character to determine the perspective of.</param>
        /// <returns>The character's perspective.</returns>
        private Perspective CurrentPerspective(GameObject character)
        {
            var hasBothComponents = false;
#if THIRD_PERSON_CONTROLLER
            hasBothComponents = m_ExistingCharacter.GetComponentInChildren<UltimateCharacterController.ThirdPersonController.Character.PerspectiveMonitor>() != null;
#endif
            if (hasBothComponents) {
                // If the character has the perspective monitor then it can switch perspectives.
                return Perspective.Both;
            } else {
                if (!m_ExistingAnimator) {
                    // If the character doesn't have an animator then it has to be in first person.
                    return Perspective.First;
                } else {
                    // Use the movement types to determine the perspective.
                    var perspective = Perspective.Third;
                    var characterLocomotion = m_ExistingCharacter.GetComponent<UltimateCharacterLocomotion>();
                    if (characterLocomotion != null) {
                        characterLocomotion.DeserializeMovementTypes();
                        var movementTypes = characterLocomotion.MovementTypes;
                        for (int i = 0; i < movementTypes.Length; ++i) {
                            if (movementTypes[i].GetType().Namespace.Contains("FirstPersonController")) {
                                perspective = Perspective.First;
                                break;
                            }
                        }
                    }
                    return perspective;
                }
            }
        }

        /// <summary>
        /// Does the character have an animator?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character has an animator.</returns>
        private bool HasAnimator(GameObject character)
        {
            return character.GetComponent<Animator>() && character.GetComponent<AnimatorMonitor>();
        }

        /// <summary>
        /// Is the character an AI agent?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character is an AI agent.</returns>
        private bool IsAIAgent(GameObject character)
        {
            return character.GetComponent<LocalLookSource>() && !character.GetComponent<UltimateCharacterLocomotionHandler>() && !character.GetComponent<ItemHandler>();
        }

        /// <summary>
        /// Does the character have the NavMeshAgent components?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character has the NavMeshAgent components.</returns>
        private bool HasNavMeshAgent(GameObject character)
        {
            return character.GetComponent<UnityEngine.AI.NavMeshAgent>() != null && character.GetComponent<UltimateCharacterLocomotion>().GetAbility<Character.Abilities.AI.NavMeshAgentMovement>() != null;
        }

        /// <summary>
        /// Does the character have the components required for items?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <param name="aiAgent">Is the character an AI agent?</param>
        /// <param name="checkFirstPersonObjects">Should the FirstPersonObjects component be checked?</param>
        /// <returns>True if the character has the components required for items.</returns>
        private bool HasItems(GameObject character, bool aiAgent, bool checkFirstPersonObjects)
        {
            if ((!aiAgent && character.GetComponent<ItemHandler>() == null) || character.GetComponentInChildren<Items.ItemPlacement>() == null || character.GetComponent<AnimatorMonitor>() == null) {
                return false;
            }

#if FIRST_PERSON_CONTROLLER
            if (checkFirstPersonObjects && character.GetComponentInChildren<UltimateCharacterController.FirstPersonController.Character.FirstPersonObjects>() == null) {
                return false;
            }
#endif

            var animator = character.GetComponent<Animator>();
            if (animator != null) {
                var head = animator.GetBoneTransform(HumanBodyBones.Head);
                if (head != null) {
                    var leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                    var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    if (leftHand != null && rightHand != null) {
                        if (leftHand.GetComponentInChildren<Items.ItemSlot>() == null || rightHand.GetComponentInChildren<Items.ItemSlot>() == null) {
                            return false;
                        }
                    }
                }
            }

            return character.GetComponent<Inventory.Inventory>() && character.GetComponent<Inventory.ItemSetManager>();
        }

        /// <summary>
        /// Does the character have the health components?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character has the health components.</returns>
        private bool HasHealth(GameObject character)
        {
            return character.GetComponent<Traits.CharacterHealth>() && character.GetComponent<Traits.AttributeManager>() && character.GetComponent<Traits.CharacterRespawner>();
        }

        /// <summary>
        /// Does the character have the CharacterIK component?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character has the CharacterIK component.</returns>
        private bool HasUnityIK(GameObject character)
        {
            return character.GetComponent<CharacterIK>();
        }

        /// <summary>
        /// Does the character have the CharacterFootEffects component?
        /// </summary>
        /// <param name="character">The GameObject of the character to determine if it has the required components.</param>
        /// <returns>True if the character has the CharacterFootEffects component.</returns>
        private bool HasFootEffects(GameObject character)
        {
            return character.GetComponent<CharacterFootEffects>();
        }
    }
}