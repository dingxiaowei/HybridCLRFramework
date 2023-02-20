/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;
using Opsive.UltimateCharacterController.Utility.Builders;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    /// <summary>
    /// The ItemManager will draw any item properties
    /// </summary>
    [OrderedEditorItem("Item", 4)]
    public class ItemManager : Manager
    {
        private string[] m_ToolbarStrings = { "New Item", "Existing Item" };
        private enum ThirdPersonHumanoidParentHand { Left, Right }

        [SerializeField] private bool m_DrawNewItems = true;

        // New Item.
        [SerializeField] private string m_Name;
        [SerializeField] private ItemType m_ItemType;
        [SerializeField] private int m_AnimatorItemID;
        [SerializeField] private GameObject m_Character;
        [SerializeField] private int m_SlotID;
        [SerializeField] private bool m_AddFirstPersonPerspective = true;
        [SerializeField] private GameObject m_FirstPersonObject;
        [SerializeField] private RuntimeAnimatorController m_FirstPersonObjectAnimatorController = null;
        [SerializeField] private GameObject m_FirstPersonVisibleItem;
        [SerializeField] private RuntimeAnimatorController m_FirstPersonVisibleItemAnimatorController = null;
        [SerializeField] private GameObject m_FirstPersonParent;
        [SerializeField] private bool m_AddThirdPersonPerspective = true;
        [SerializeField] private GameObject m_ThirdPersonObject;
        [SerializeField] private RuntimeAnimatorController m_ThirdPersonObjectAnimatorController;
        [SerializeField] private ThirdPersonHumanoidParentHand m_ThirdHumanoidParentHand = ThirdPersonHumanoidParentHand.Right;
        [SerializeField] private GameObject m_ThirdPersonParent;
        [SerializeField] private ItemBuilder.ActionType m_ActionType;
        [SerializeField] private ItemType m_ActionItemType;
        [SerializeField] private bool m_AddToDefaultLoadout = true;
        [SerializeField] private StateConfiguration m_AddStateConfiguration;
        [SerializeField] private int m_AddProfileIndex;
        [SerializeField] private string m_AddProfileName;

        // Existing Item.
        [SerializeField] private Item m_Item;
        [SerializeField] private ItemBuilder.ActionType m_AddActionType;
        [SerializeField] private ItemType m_ExistingAddActionItemType;
        [SerializeField] private int m_RemoveActionTypeIndex;
        [SerializeField] private GameObject m_ExistingFirstPersonObject;
        [SerializeField] private RuntimeAnimatorController m_ExistingFirstPersonObjectAnimatorController;
        [SerializeField] private GameObject m_ExistingFirstPersonVisibleItem;
        [SerializeField] private GameObject m_ExistingFirstPersonParent;
        [SerializeField] private RuntimeAnimatorController m_ExistingFirstPersonVisibleItemAnimatorController;
        [SerializeField] private GameObject m_ExistingThirdPersonObject;
        [SerializeField] private RuntimeAnimatorController m_ExistingThirdPersonObjectAnimatorController;
        [SerializeField] private ThirdPersonHumanoidParentHand m_ExistingThirdHumanoidParentHand = ThirdPersonHumanoidParentHand.Right;
        [SerializeField] private GameObject m_ExistingThirdPersonParent;
        [SerializeField] private StateConfiguration m_ExistingStateConfiguration;
        [SerializeField] private int m_ExistingProfileIndex;

        private ItemSlot m_FirstPersonItemSlot = null;
        private ItemSlot m_ThirdPersonItemSlot = null;
        private ItemSlot m_ExistingFirstPersonItemSlot = null;
        private ItemSlot m_ExistingThirdPersonItemSlot = null;

        private Material m_InvisibleShadowCaster;

        /// <summary>
        /// Initialize the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

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
        /// Draws the ItemManager.
        /// </summary>
        public override void OnGUI()
        {
            var toolbarSelection = GUILayout.Toolbar(m_DrawNewItems ? 0 : 1, m_ToolbarStrings, EditorStyles.toolbarButton);
            m_DrawNewItems = toolbarSelection == 0;
            GUILayout.Space(10);

            if (m_DrawNewItems) {
                GUILayout.Label("New Item", InspectorStyles.CenterBoldLabel);
                DrawNewItem();
            } else {
                GUILayout.Label("Existing Item", InspectorStyles.CenterBoldLabel);
                DrawExistingItem();
            }
        }

        /// <summary>
        /// Draws the UI for new item.
        /// </summary>
        private void DrawNewItem()
        {
            var canBuild = true;
            m_Name = EditorGUILayout.TextField("Name", m_Name);
            if (string.IsNullOrEmpty(m_Name)) {
                canBuild = false;
                EditorGUILayout.HelpBox("The item must have a name.", MessageType.Error);
            }
            m_ItemType = EditorGUILayout.ObjectField("Item Type", m_ItemType, typeof(ItemType), false) as ItemType;
            if (canBuild && m_ItemType == null) {
                canBuild = false;
                EditorGUILayout.HelpBox("The item must specify an ItemType.", MessageType.Error);
            }

            var character = EditorGUILayout.ObjectField("Character", m_Character, typeof(GameObject), true) as GameObject;
            var characterUpdate = false;
            if (character != m_Character) {
                m_Character = character;
                characterUpdate = true;

#if FIRST_PERSON_CONTROLLER
                // Try to assign the first person objects if they exist.
                if (m_Character != null) {
                    var firstPersonObjects = m_Character.GetComponentInChildren<UltimateCharacterController.FirstPersonController.Character.FirstPersonObjects>();
                    if (firstPersonObjects != null) {
                        var firstPersonBaseObject = firstPersonObjects.GetComponentInChildren<UltimateCharacterController.FirstPersonController.Character.Identifiers.FirstPersonBaseObject>();
                        if (firstPersonBaseObject != null) {
                            m_FirstPersonObject = firstPersonBaseObject.gameObject;
                        }
                    }
                    m_AddThirdPersonPerspective = m_Character.GetComponent<Animator>() != null;
                }
#endif
            }

            if (m_Character == null) {
                m_SlotID = EditorGUILayout.IntField("Slot ID", m_SlotID);
            } else {
                if (EditorUtility.IsPersistent(m_Character)) {
                    if (canBuild) {
                        EditorGUILayout.HelpBox("The character must be located within the scene.", MessageType.Error);
                        canBuild = false;
                    }
                } else {
                    // The attach to object must be a character already created.
                    if (m_Character.GetComponentInChildren<ItemPlacement>() == null) {
                        if (canBuild) {
                            EditorGUILayout.HelpBox("The character must be an already created character.", MessageType.Error);
                            canBuild = false;
                        }
                    } else {
                        if (m_ItemType != null & m_Character.GetComponent<InventoryBase>() != null) {
                            // The item can automatically be added to the default loadout if the inventory component exists.
                            EditorGUI.indentLevel++;
                            m_AddToDefaultLoadout = EditorGUILayout.Toggle("Add to Default Loadout", m_AddToDefaultLoadout);
                            EditorGUI.indentLevel--;
                        } else {
                            m_AddToDefaultLoadout = false;
                        }
                    }
                }
            }
            m_AnimatorItemID = EditorGUILayout.IntField("Animator Item ID", m_AnimatorItemID);

#if FIRST_PERSON_CONTROLLER
            GUILayout.Space(5);
            GUILayout.Label("First Person", InspectorStyles.BoldLabel);
            GUI.enabled = m_FirstPersonObject == null && m_FirstPersonVisibleItem == null;
            m_AddFirstPersonPerspective = EditorGUILayout.Toggle("Add First Person Item", m_AddFirstPersonPerspective);
            GUI.enabled = m_AddFirstPersonPerspective;
            var firstPersonSuccess = DrawFirstPersonObject(m_Character, ref m_FirstPersonObject, ref m_FirstPersonObjectAnimatorController, ref m_FirstPersonVisibleItem, 
                                                                ref m_FirstPersonParent, ref m_FirstPersonItemSlot, ref m_FirstPersonVisibleItemAnimatorController,
                                                                m_ThirdPersonItemSlot != null ? m_ThirdPersonItemSlot.ID : 0, characterUpdate, canBuild && m_AddFirstPersonPerspective);
            GUI.enabled = true;
            if (m_AddFirstPersonPerspective && !firstPersonSuccess) {
                canBuild = false;
            }
#endif
            if (m_Character == null || (m_Character != null && m_Character.GetComponent<Animator>() != null)) {
                GUILayout.Space(10);
                GUILayout.Label("Third Person (including AI and multiplayer)", InspectorStyles.BoldLabel);
                GUI.enabled = m_ThirdPersonObject == null;
                m_AddThirdPersonPerspective = EditorGUILayout.Toggle("Add Third Person Item", m_AddThirdPersonPerspective);
                GUI.enabled = m_AddThirdPersonPerspective;
                var thirdPersonSuccess = DrawThirdPersonObject(m_Character, ref m_ThirdPersonObject, ref m_ThirdHumanoidParentHand, ref m_ThirdPersonParent, ref m_ThirdPersonItemSlot,
                                                                        ref m_ThirdPersonObjectAnimatorController, m_FirstPersonItemSlot != null ? m_FirstPersonItemSlot.ID : 0, characterUpdate, canBuild && m_AddThirdPersonPerspective);
                GUI.enabled = true;
                if (m_AddThirdPersonPerspective && !thirdPersonSuccess) {
                    canBuild = false;
                }
            }

            if (!m_AddFirstPersonPerspective && !m_AddThirdPersonPerspective) {
                if (canBuild) {
                    EditorGUILayout.HelpBox("At least one perspective must be added.", MessageType.Error);
                    canBuild = false;
                }
            }

            GUILayout.Space(15);
            m_ActionType = (ItemBuilder.ActionType)EditorGUILayout.EnumPopup("Action Type", m_ActionType);

#if !ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            if (m_ActionType == ItemBuilder.ActionType.ShootableWeapon && canBuild) {
                EditorGUILayout.HelpBox("The shooter controller is necessary in order to create shootable weapons.", MessageType.Error);
                canBuild = false;
            }
#endif
#if !ULTIMATE_CHARACTER_CONTROLLER_MELEE
            if (m_ActionType == ItemBuilder.ActionType.MeleeWeapon && canBuild) {
                EditorGUILayout.HelpBox("The melee controller is necessary in order to create melee weapons.", MessageType.Error);
                canBuild = false;
            }
#endif
#if FIRST_PERSON_CONTROLLER
            // The slot IDs must match.
            if (m_FirstPersonItemSlot != null && m_ThirdPersonItemSlot != null && m_FirstPersonItemSlot.ID != m_ThirdPersonItemSlot.ID && canBuild) {
                canBuild = false;
                EditorGUILayout.HelpBox("The first and third person ItemSlots must use the same ID.", MessageType.Error);
            }
#endif

            if (canBuild && (m_ActionType != ItemBuilder.ActionType.Shield)) {
                EditorGUI.indentLevel++;
                m_ActionItemType = EditorGUILayout.ObjectField("Consumable Item Type", m_ActionItemType, typeof(ItemType), false) as ItemType;
                EditorGUI.indentLevel--;
            }

            // Setup profiles.
            GUILayout.Space(5);
            var updatedStateConfiguration = EditorGUILayout.ObjectField("State Configuration", m_AddStateConfiguration, typeof(StateConfiguration), false) as StateConfiguration;
            if (updatedStateConfiguration != m_AddStateConfiguration) {
                if (updatedStateConfiguration != null) {
                    EditorPrefs.SetString(ManagerUtility.LastStateConfigurationGUIDString, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(updatedStateConfiguration)));
                } else {
                    EditorPrefs.SetString(ManagerUtility.LastStateConfigurationGUIDString, string.Empty);
                }
                m_AddStateConfiguration = updatedStateConfiguration;
            }

            if (m_AddStateConfiguration != null) {
                EditorGUI.indentLevel++;
                var profiles = m_AddStateConfiguration.GetProfilesForGameObject(null, StateConfiguration.Profile.ProfileType.Item);
                if (profiles.Count > 0) {
                    // The item can be added without any profiles.
                    profiles.Insert(0, "(None)");
                    m_AddProfileIndex = EditorGUILayout.Popup("Profile", m_AddProfileIndex, profiles.ToArray());
                    m_AddProfileName = profiles[m_AddProfileIndex];
                }
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(5);
            GUI.enabled = canBuild;
            if (GUILayout.Button("Build Item")) {
                var item = ItemBuilder.BuildItem(m_Name, m_ItemType, m_AnimatorItemID, m_Character, m_SlotID, m_AddToDefaultLoadout, m_AddFirstPersonPerspective, m_FirstPersonObject, m_FirstPersonObjectAnimatorController,
                    m_FirstPersonVisibleItem, m_FirstPersonItemSlot, m_FirstPersonVisibleItemAnimatorController, m_AddThirdPersonPerspective, m_ThirdPersonObject, m_ThirdPersonItemSlot, m_ThirdPersonObjectAnimatorController,
                    m_InvisibleShadowCaster, m_ActionType, m_ActionItemType);
                // Setup any profiles on the item.
                if (m_AddStateConfiguration != null) {
                    if (m_AddProfileIndex > 0) {
                        m_AddStateConfiguration.AddStatesToGameObject(m_AddProfileName, item.gameObject);
                        InspectorUtility.SetDirty(item.gameObject);
                    }
                }
                
                // If the character is null then a prefab will be created.
                if (m_Character == null) {
                    var path = EditorUtility.SaveFilePanel("Save Item", "Assets", m_Name + ".prefab", "prefab");
                    if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                        var relativePath = path.Replace(Application.dataPath, "");
#if UNITY_2018_3_OR_NEWER
                        PrefabUtility.SaveAsPrefabAsset(item, "Assets" + relativePath);
#else
                        PrefabUtility.CreatePrefab("Assets" + relativePath, item);
#endif
                        Object.DestroyImmediate(item, true);
                    }
                }

                // Remove the original objects if they are in the scene - this will prevent duplicate objects from existing.
                if (m_FirstPersonVisibleItem != null && !EditorUtility.IsPersistent(m_FirstPersonVisibleItem) &&
                    (m_Character == null || !m_FirstPersonVisibleItem.transform.IsChildOf(m_Character.transform))) {
                    Object.DestroyImmediate(m_FirstPersonVisibleItem, true);
                    m_FirstPersonVisibleItem = null;
                }
                if (m_FirstPersonObject != null && !EditorUtility.IsPersistent(m_FirstPersonObject) && 
                    (m_Character == null || !m_FirstPersonObject.transform.IsChildOf(m_Character.transform))) {
                    Object.DestroyImmediate(m_FirstPersonObject, true);
                    m_FirstPersonObject = null;
                }
                if (m_ThirdPersonObject != null && !EditorUtility.IsPersistent(m_ThirdPersonObject) &&
                    (m_Character == null || !m_ThirdPersonObject.transform.IsChildOf(m_Character.transform))) {
                    Object.DestroyImmediate(m_ThirdPersonObject, true);
                    m_ThirdPersonObject = null;
                }

                // Select the newly added item.
                Selection.activeGameObject = item.gameObject;
            }
            GUI.enabled = true;
        }

#if FIRST_PERSON_CONTROLLER
        /// <summary>
        /// Draws the controls for the first person object fields.
        /// </summary>
        /// <param name="character">The character that has the item (can be null).</param>
        /// <param name="firstPersonObject">A reference to the first person object.</param>
        /// <param name="firstPersonObjectAnimatorController">A reference to the animator controller added to the first person object. Can be null.</param>
        /// <param name="firstPersonVisibleItem">A reference to the first person visible item.</param>
        /// <param name="firstPersonParent">A reference to the first person parent.</param>
        /// <param name="firstPersonItemSlot">The ItemSlot on the parent GameObject.</param>
        /// <param name="firstPersonVisibleItemAnimatorController">A reference to the animator controller added to the first person visible item. Can be null.</param>
        /// <param name="defaultItemSlotIndex">The index of the default item slot.</param>
        /// <param name="characterUpdate">Was the character field updated?</param>
        /// <param name="showError">Should the error be shown (if any)?</param>
        /// <returns>Was the objects drawn successfully?</returns>
        private bool DrawFirstPersonObject(GameObject character, ref GameObject firstPersonObject, ref RuntimeAnimatorController firstPersonObjectAnimatorController, ref GameObject firstPersonVisibleItem, 
                                            ref GameObject firstPersonParent, ref ItemSlot firstPersonItemSlot, ref RuntimeAnimatorController firstPersonVisibleItemAnimatorController, 
                                            int defaultItemSlotIndex, bool characterUpdate, bool showError)
        {
            var success = true;
            firstPersonObject = EditorGUILayout.ObjectField("First Person Base", firstPersonObject, typeof(GameObject), true) as GameObject;
            if (character != null && firstPersonObject == null) {
                success = false;
                if (showError) {
                    EditorGUILayout.HelpBox("A first person base object is required.", MessageType.Error);
                }
            } else if (firstPersonObject != null) {
                if (EditorUtility.IsPersistent(firstPersonObject)) {
                    success = false;
                    if (showError) {
                        EditorGUILayout.HelpBox("Please drag your first person base object into the scene. The Item Manager cannot add components to prefabs.", MessageType.Error);
                    }
                } else {
                    Animator animator;
                    if ((animator = firstPersonObject.GetComponent<Animator>()) == null || animator.runtimeAnimatorController == null) {
                        EditorGUI.indentLevel++;
                        firstPersonObjectAnimatorController = EditorGUILayout.ObjectField("Animator Controller", firstPersonObjectAnimatorController, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
                        EditorGUI.indentLevel--;
                    }
                }
            }
            var visibleItem = EditorGUILayout.ObjectField("First Person Visible Item", firstPersonVisibleItem, typeof(GameObject), true) as GameObject;
            // Preselect the parent if the first person object is not null.
            if ((visibleItem != firstPersonVisibleItem || characterUpdate) && visibleItem != null && firstPersonObject != null) {
                var itemSlots = firstPersonObject.GetComponentsInChildren<ItemSlot>();
                if (itemSlots.Length > 0) {
                    var itemSlot = itemSlots[0];
                    for (int i = 1; i < itemSlots.Length; ++i) {
                        if (itemSlots[i].ID == defaultItemSlotIndex) {
                            itemSlot = itemSlots[i];
                            break;
                        }
                    }
                    firstPersonParent = itemSlot.gameObject;
                }
            }
            firstPersonVisibleItem = visibleItem;
            if ((character != null && (firstPersonObject == null && firstPersonVisibleItem != null)) ||
                (firstPersonObject != null && firstPersonVisibleItem != null && !firstPersonVisibleItem.transform.IsChildOf(firstPersonObject.transform))) {
                EditorGUI.indentLevel++;
                var invalidItemSlot = false;
                EditorGUILayout.BeginHorizontal();
                firstPersonParent = EditorGUILayout.ObjectField("Item Parent", firstPersonParent, typeof(GameObject), true) as GameObject;
                if (firstPersonParent == null) {
                    invalidItemSlot = true;
                } else {
                    // The First Person Parent should be a child of the FirstPersonObjects component.
                    if ((firstPersonObject == null && firstPersonParent.GetComponentInParent<UltimateCharacterController.FirstPersonController.Character.FirstPersonObjects>() == null) || 
                        firstPersonObject != null && !firstPersonParent.transform.IsChildOf(firstPersonObject.transform)) {
                        invalidItemSlot = true;
                    } else if ((firstPersonItemSlot = firstPersonParent.GetComponent<ItemSlot>()) == null) {
                        // Allow for some leeway if there is only one child ItemSlot component.
                        var itemSlots = firstPersonParent.GetComponentsInChildren<ItemSlot>();
                        if (itemSlots.Length == 1) {
                            firstPersonParent = itemSlots[0].gameObject;
                        } else {
                            invalidItemSlot = true;
                            // Allow the ItemSlot to be added.
                            if (GUILayout.Button("Add ItemSlot", GUILayout.Width(150))) {
                                firstPersonParent = AddItemSlot(character != null ? character : firstPersonObject, firstPersonParent.transform, true);
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (invalidItemSlot) {
                    success = false;
                    if (showError) {
                        EditorGUILayout.HelpBox("The first person Item Parent field does not specify a valid ItemSlot GameObject.", MessageType.Error);
                    }
                }
                EditorGUI.indentLevel--;
            }
            if (firstPersonVisibleItem != null) {
                EditorGUI.indentLevel++;
                firstPersonVisibleItemAnimatorController = EditorGUILayout.ObjectField("Animator Controller", firstPersonVisibleItemAnimatorController, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
                EditorGUI.indentLevel--;
            }
            return success;
        }
#endif

        /// <summary>
        /// Draws the controls for the third person object fields.
        /// </summary>
        /// <param name="character">The character that has the item (can be null).</param>
        /// <param name="thirdPersonObject">A reference to the third person object.</param>
        /// <param name="parentHand">A reference to the third person hand.</param>
        /// <param name="thirdPersonParent">A reference to the third person parent.</param>
        /// <param name="thirdPersonItemSlot">The ItemSlot on the parent GameObject.</param>
        /// <param name="defaultItemSlotIndex">The index of the default item slot.</param>
        /// <param name="characterUpdate">Was the character field updated?</param>
        /// <param name="showError">Should the error be shown (if any)?</param>
        /// <returns>Was the objects drawn successfully?</returns>
        private bool DrawThirdPersonObject(GameObject character, ref GameObject thirdPersonObject, ref ThirdPersonHumanoidParentHand parentHand, ref GameObject thirdPersonParent, 
                                                ref ItemSlot thirdPersonItemSlot, ref RuntimeAnimatorController thirdPersonObjectAnimatorController, 
                                                int defaultItemSlotIndex, bool characterUpdate, bool showError)
        {
            var success = true;
            var prevThirdPersonObject = thirdPersonObject;
            thirdPersonObject = EditorGUILayout.ObjectField("Third Person Visible Item", thirdPersonObject, typeof(GameObject), true) as GameObject;
            if (thirdPersonObject != null && character != null) {
                EditorGUI.indentLevel++;
                var invalidItemSlot = false;
                EditorGUILayout.BeginHorizontal();
                var animator = character.GetComponent<Animator>();

                // Setup the default ItemSlot to be the same ID as the first person perspective.
                if (prevThirdPersonObject != thirdPersonObject || characterUpdate) {
                    var itemSlots = character.GetComponentsInChildren<ItemSlot>();
                    for (int i = 0; i < itemSlots.Length; ++i) {
#if FIRST_PERSON_CONTROLLER
                        if (itemSlots[i].GetComponentInParent<UltimateCharacterController.FirstPersonController.Character.Identifiers.FirstPersonBaseObject>() != null) {
                            continue;
                        }
#endif
                        if (itemSlots[i].ID == defaultItemSlotIndex) {
                            thirdPersonItemSlot = itemSlots[i];
                            thirdPersonParent = thirdPersonItemSlot.gameObject;

                            if (animator != null && animator.GetBoneTransform(HumanBodyBones.Head) != null) {
                                if (thirdPersonParent.transform.IsChildOf(animator.GetBoneTransform(HumanBodyBones.RightHand))) {
                                    parentHand = ThirdPersonHumanoidParentHand.Right;
                                } else {
                                    parentHand = ThirdPersonHumanoidParentHand.Left;
                                }
                            }
                            break;
                        }
                    }
                }

                // Show a dropdown for the humanoid characters.
                if (animator != null && animator.GetBoneTransform(HumanBodyBones.Head) != null) {
                    var hand = (ThirdPersonHumanoidParentHand)EditorGUILayout.EnumPopup("Hand", parentHand);
                    if (thirdPersonParent == null || hand != parentHand) {
                        parentHand = hand;
                        var handTransform = animator.GetBoneTransform(hand == ThirdPersonHumanoidParentHand.Right ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand);
                        var itemSlot = handTransform.GetComponentInChildren<ItemSlot>();
                        if (itemSlot != null) {
                            thirdPersonParent = itemSlot.gameObject;
                        } else {
                            thirdPersonParent = null;
                        }
                    }
                } else {
                    thirdPersonParent = EditorGUILayout.ObjectField("Item Parent", thirdPersonParent, typeof(GameObject), true) as GameObject;
                }
                if (thirdPersonParent == null) {
                    invalidItemSlot = true;
                } else {
#if FIRST_PERSON_CONTROLLER
                    // The Third Person Parent should not be a child of the FirstPersonObjects component.
                    if (thirdPersonParent.GetComponentInParent<UltimateCharacterController.FirstPersonController.Character.FirstPersonObjects>() != null) {
                        invalidItemSlot = true;
                    } else {
#endif
                        if ((thirdPersonItemSlot = thirdPersonParent.GetComponent<ItemSlot>()) == null) {
                            // Allow for some leeway if there is only one child ItemSlot component.
                            var itemSlots = thirdPersonParent.GetComponentsInChildren<ItemSlot>();
                            if (itemSlots.Length == 1) {
                                thirdPersonParent = itemSlots[0].gameObject;
                            } else {
                                success = false;
                                // Allow the ItemSlot to be added.
                                if (GUILayout.Button("Add ItemSlot", GUILayout.Width(150))) {
                                    thirdPersonParent = AddItemSlot(character, thirdPersonParent.transform, false);
                                }
                            }
                        }
#if FIRST_PERSON_CONTROLLER
                    }
#endif
                }
                EditorGUILayout.EndHorizontal();
                if (invalidItemSlot) {
                    success = false;
                    if (showError) {
                        EditorGUILayout.HelpBox("The third person Item Parent field does not specify a valid ItemSlot GameObject.", MessageType.Error);
                    }
                }
                EditorGUI.indentLevel--;
            }
            if (thirdPersonObject != null) {
                EditorGUI.indentLevel++;
                thirdPersonObjectAnimatorController = EditorGUILayout.ObjectField("Animator Controller", thirdPersonObjectAnimatorController, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
                EditorGUI.indentLevel--;
            }
            return success;
        }

        /// <summary>
        /// Adds an ItemSlot child GameObject to the specified parent.
        /// </summary>
        /// <param name="baseParent">The object that is adding the item slot.</param>
        /// <param name="itemParent">The object to add the ItemSlot to.</param>
        /// <param name="firstPerson">Should a first person ItemSlot be added?</param>
        /// <returns>The added the ItemSlot GameObject (can be null).</returns>
        private GameObject AddItemSlot(GameObject baseParent, Transform itemParent, bool firstPerson)
        {
            // The new ItemSlot's ID should be unique.
            var allItemSlots = baseParent.GetComponentsInChildren<ItemSlot>();
            var maxID = -1;
#if FIRST_PERSON_CONTROLLER
            var firstPersonObjects = baseParent.GetComponentInChildren<UltimateCharacterController.FirstPersonController.Character.FirstPersonObjects>();
#endif
            for (int i = 0; i < allItemSlots.Length; ++i) {
#if FIRST_PERSON_CONTROLLER
                // The ItemSlot must match the perspective.
                if (firstPersonObjects != null && (allItemSlots[i].transform.IsChildOf(firstPersonObjects.transform) != firstPerson)){
                    continue;
                }
#endif
                if (allItemSlots[i].ID > maxID) {
                    maxID = allItemSlots[i].ID;
                }
            }
            // Setup the new ItemSlot.
            var itemSlotGameObject = new GameObject("Items", new System.Type[] { typeof(ItemSlot) });
            itemSlotGameObject.transform.SetParentOrigin(itemParent);
            var itemSlot = itemSlotGameObject.GetComponent<ItemSlot>();
            // The new ID should be one greater than the previous max ID.
            itemSlot.ID = maxID + 1;
            return itemSlotGameObject;
        }

        /// <summary>
        /// Draws the UI for existing item.
        /// </summary>
        private void DrawExistingItem()
        {
            EditorGUILayout.BeginHorizontal();
            m_Item = EditorGUILayout.ObjectField("Item", m_Item, typeof(Item), true) as Item;
            GUI.enabled = m_Item != null;
            if (GUILayout.Button("Remove", GUILayout.Width(80))) {
#if FIRST_PERSON_CONTROLLER
                var firstPersonVisibleItemObject = m_Item.GetComponent<UltimateCharacterController.FirstPersonController.Items.FirstPersonPerspectiveItem>();
                if (firstPersonVisibleItemObject != null) {
                    ItemBuilder.RemoveFirstPersonObject(firstPersonVisibleItemObject);
                }
#endif
                var thirdPersonVisibleItemObject = m_Item.GetComponent<UltimateCharacterController.ThirdPersonController.Items.ThirdPersonPerspectiveItem>();
                if (thirdPersonVisibleItemObject != null) {
                    ItemBuilder.RemoveThirdPersonObject(thirdPersonVisibleItemObject);
                }
                // The ItemType should also be removed from the Inventory/ItemSetManager.
                var inventory = m_Item.GetComponentInParent<InventoryBase>();
                if (inventory != null) {
                    var defaultLoadout = new System.Collections.Generic.List<ItemTypeCount>(inventory.DefaultLoadout);
                    for (int i = defaultLoadout.Count - 1; i > -1; --i) {
                        if (defaultLoadout[i].ItemType == m_Item.ItemType) {
                            defaultLoadout.RemoveAt(i);
                            break;
                        }
                    }
                    inventory.DefaultLoadout = defaultLoadout.ToArray();
                    EditorUtility.SetDirty(inventory);

                    var itemSetManager = inventory.GetComponent<ItemSetManager>();
                    if (itemSetManager != null && m_Item.ItemType.CategoryIndices != null) {
                        for (int i = 0; i < m_Item.ItemType.CategoryIndices.Length; ++i) {
                            var category = itemSetManager.CategoryItemSets[i];
                            for (int j = category.ItemSetList.Count - 1; j > -1; --j) {
                                if (category.ItemSetList[j].Slots[m_Item.SlotID] == m_Item.ItemType) {
                                    category.ItemSetList.RemoveAt(j);
                                }
                            }
                        }
                        EditorUtility.SetDirty(itemSetManager);
                    }
                }

                Undo.DestroyObjectImmediate(m_Item.gameObject);
                m_Item = null;
            }
            GUI.enabled = m_Item != null;
            EditorGUILayout.EndHorizontal();

            // Actions can be removed.
            if (m_Item != null) {
                var actions = m_Item.GetComponents<ItemAction>();
                if (actions.Length > 0) {
                    var actionStrings = new string[actions.Length];
                    for (int i = 0; i < actions.Length; ++i) {
                        actionStrings[i] = InspectorUtility.DisplayTypeName(actions[i].GetType(), false);
                        if (actions.Length > 1) {
                            actionStrings[i] += " (ID " + actions[i].ID + ")";
                        }
                    }
                    EditorGUILayout.BeginHorizontal();
                    m_RemoveActionTypeIndex = EditorGUILayout.Popup("Remove Action", m_RemoveActionTypeIndex, actionStrings);
                    if (GUILayout.Button("Remove", GUILayout.Width(80))) {
                        ItemBuilder.RemoveAction(actions[m_RemoveActionTypeIndex]);
                        m_RemoveActionTypeIndex = 0;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            // Actions can be added.
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            m_AddActionType = (ItemBuilder.ActionType)EditorGUILayout.EnumPopup("Add Action", m_AddActionType);
            var canBuild = true;

#if !ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            if (m_AddActionType == ItemBuilder.ActionType.ShootableWeapon) {
                EditorGUILayout.HelpBox("The shooter controller is necessary in order to create melee weapons.", MessageType.Error);
                canBuild = false;
            }
#endif
#if !ULTIMATE_CHARACTER_CONTROLLER_MELEE
            if (m_AddActionType == ItemBuilder.ActionType.MeleeWeapon) {
                EditorGUILayout.HelpBox("The melee controller is necessary in order to create melee weapons.", MessageType.Error);
                canBuild = false;
            }
#endif

            if (canBuild && (m_AddActionType != ItemBuilder.ActionType.Shield)) {
                EditorGUI.indentLevel++;
                m_ExistingAddActionItemType = EditorGUILayout.ObjectField("Consumable Item Type", m_ExistingAddActionItemType, typeof(ItemType), false) as ItemType;
                EditorGUI.indentLevel--;
            }

            if (GUILayout.Button("Add", GUILayout.Width(80))) {
                ItemBuilder.AddAction(m_Item.gameObject, m_AddActionType, m_ExistingAddActionItemType);
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = m_Item != null && canBuild;

#if FIRST_PERSON_CONTROLLER
            GUILayout.Space(5);
            // The first person objects can be added or removed.
            EditorGUILayout.LabelField("First Person", InspectorStyles.BoldLabel);
            EditorGUI.indentLevel++;
            FirstPersonController.Items.FirstPersonPerspectiveItem firstPersonVisibleItem = null;
            if (m_Item != null) {
                firstPersonVisibleItem = m_Item.GetComponent<FirstPersonController.Items.FirstPersonPerspectiveItem>();
                GUI.enabled = firstPersonVisibleItem == null;
                if (firstPersonVisibleItem != null) {
                    m_ExistingFirstPersonObject = firstPersonVisibleItem.Object;
                    m_ExistingFirstPersonVisibleItem = firstPersonVisibleItem.VisibleItem;
                    if (m_ExistingFirstPersonVisibleItem != null) {
                        var firstPersonVisibleItemAnimator = firstPersonVisibleItem.VisibleItem.GetComponent<Animator>();
                        if (firstPersonVisibleItemAnimator != null) {
                            m_ExistingFirstPersonVisibleItemAnimatorController = firstPersonVisibleItemAnimator.runtimeAnimatorController;
                        } else {
                            m_ExistingFirstPersonVisibleItemAnimatorController = null;
                        }
                    } else {
                        m_ExistingFirstPersonVisibleItemAnimatorController = null;
                    }
                }
                var character = m_Item.GetComponentInParent<Character.UltimateCharacterLocomotion>();
                DrawFirstPersonObject(character != null ? character.gameObject : null, ref m_ExistingFirstPersonObject, ref m_ExistingFirstPersonObjectAnimatorController, 
                                                                ref m_ExistingFirstPersonVisibleItem, ref m_ExistingFirstPersonParent, ref m_ExistingFirstPersonItemSlot,
                                                                ref m_ExistingFirstPersonVisibleItemAnimatorController, 
                                                                m_ExistingThirdPersonItemSlot != null ? m_ExistingThirdPersonItemSlot.ID : 0, false, true);
                GUI.enabled = true;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(InspectorUtility.IndentWidth);
            GUI.enabled = m_Item != null && firstPersonVisibleItem == null;
            if (GUILayout.Button("Add")) {
                var character = m_Item.GetComponentInParent<Character.UltimateCharacterLocomotion>();
                ItemBuilder.AddFirstPersonObject(character.gameObject, m_Item.name, m_Item.gameObject, ref m_ExistingFirstPersonObject, m_ExistingFirstPersonObjectAnimatorController,
                                                    ref m_ExistingFirstPersonVisibleItem, m_ExistingFirstPersonItemSlot, m_ExistingFirstPersonVisibleItemAnimatorController);
            }

            GUI.enabled = m_Item != null && firstPersonVisibleItem != null;
            if (GUILayout.Button("Remove")) {
                ItemBuilder.RemoveFirstPersonObject(firstPersonVisibleItem);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
#endif

            // The third person objects can be added or removed.
            GUI.enabled = m_Item != null;
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Third Person", InspectorStyles.BoldLabel);
            EditorGUI.indentLevel++;
            ThirdPersonController.Items.ThirdPersonPerspectiveItem thirdPersonVisibleItem = null;
            if (m_Item != null) {
                thirdPersonVisibleItem = m_Item.GetComponent<ThirdPersonController.Items.ThirdPersonPerspectiveItem>();
                GUI.enabled = thirdPersonVisibleItem == null;
                if (thirdPersonVisibleItem != null) {
                    m_ExistingThirdPersonObject = thirdPersonVisibleItem.Object;
                    if (m_ExistingThirdPersonObject != null) {
                        var thirdPersonAnimator = thirdPersonVisibleItem.Object.GetComponent<Animator>();
                        if (thirdPersonAnimator != null) {
                            m_ExistingThirdPersonObjectAnimatorController = thirdPersonAnimator.runtimeAnimatorController;
                        } else {
                            m_ExistingThirdPersonObjectAnimatorController = null;
                        }
                    } else {
                        m_ExistingThirdPersonObjectAnimatorController = null;
                    }
                }
                var character = m_Item.GetComponentInParent<Character.UltimateCharacterLocomotion>();
                if (character == null || (character != null && character.GetComponent<Animator>() != null)) {
                    DrawThirdPersonObject(character != null ? character.gameObject : null, ref m_ExistingThirdPersonObject, ref m_ExistingThirdHumanoidParentHand, ref m_ExistingThirdPersonParent,
                                                                ref m_ExistingThirdPersonItemSlot, ref m_ExistingThirdPersonObjectAnimatorController,
                                                                m_ExistingFirstPersonItemSlot != null ? m_ExistingFirstPersonItemSlot.ID : 0, false, true);
                }
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(InspectorUtility.IndentWidth);
            GUI.enabled = m_Item != null && thirdPersonVisibleItem == null;
            if (GUILayout.Button("Add")) {
                var character = m_Item.GetComponentInParent<Character.UltimateCharacterLocomotion>();
                ItemBuilder.AddThirdPersonObject(character.gameObject, m_Item.name, m_Item.gameObject, ref m_ExistingThirdPersonObject, m_ExistingThirdPersonItemSlot, m_ExistingThirdPersonObjectAnimatorController, m_InvisibleShadowCaster, false);
            }
            GUI.enabled = m_Item != null && thirdPersonVisibleItem != null;
            if (GUILayout.Button("Remove")) {
                ItemBuilder.RemoveThirdPersonObject(thirdPersonVisibleItem);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            GUI.enabled = m_Item != null;

            // Setup profiles.
            GUILayout.Space(5);
            EditorGUILayout.LabelField("State Profile", InspectorStyles.BoldLabel);

            EditorGUI.indentLevel++;
            var updatedStateConfiguration = EditorGUILayout.ObjectField("State Configuration", m_ExistingStateConfiguration, typeof(StateConfiguration), false) as StateConfiguration;
            if (updatedStateConfiguration != m_ExistingStateConfiguration) {
                if (updatedStateConfiguration != null) {
                    EditorPrefs.SetString(ManagerUtility.LastStateConfigurationGUIDString, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(updatedStateConfiguration)));
                } else {
                    EditorPrefs.SetString(ManagerUtility.LastStateConfigurationGUIDString, string.Empty);
                }
                m_ExistingStateConfiguration = updatedStateConfiguration;
            }
            if (m_ExistingStateConfiguration != null) {
                var profiles = m_ExistingStateConfiguration.GetProfilesForGameObject(m_Item == null ? null : m_Item.gameObject, StateConfiguration.Profile.ProfileType.Item);
                EditorGUILayout.BeginHorizontal();
                var canSetup = true;
                if (profiles.Count == 0) {
                    canSetup = false;
                    profiles.Add("(None)");
                }
                m_ExistingProfileIndex = EditorGUILayout.Popup("Profile", m_ExistingProfileIndex, profiles.ToArray());
                GUI.enabled = m_Item != null && canSetup;
                if (GUILayout.Button("Apply")) {
                    m_ExistingStateConfiguration.AddStatesToGameObject(profiles[m_ExistingProfileIndex], m_Item.gameObject);
                    InspectorUtility.SetDirty(m_Item.gameObject);
                }
                GUI.enabled = m_Item != null;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }
    }
}