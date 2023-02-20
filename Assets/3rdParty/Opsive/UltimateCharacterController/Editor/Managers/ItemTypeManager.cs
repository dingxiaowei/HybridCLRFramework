/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Editor.Controls;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    /// <summary>
    /// The ItemTypeManager will draw any ItemType properties
    /// </summary>
    [Serializable]
    [OrderedEditorItem("Item Types", 3)]
    public class ItemTypeManager : Manager
    {
        private static GUIStyle s_TreeRowHeaderGUIStyle;
        public static GUIStyle TreeRowHeaderGUIStyle
        {
            get
            {
                if (s_TreeRowHeaderGUIStyle == null) {
                    s_TreeRowHeaderGUIStyle = new GUIStyle("RL Header");
                    // The header background image should stretch with the size of the rect.
                    s_TreeRowHeaderGUIStyle.fixedHeight = 0;
                    s_TreeRowHeaderGUIStyle.stretchHeight = true;
                }
                return s_TreeRowHeaderGUIStyle;
            }
        }
        private static GUIStyle s_TreeRowBackgroundGUIStyle;
        public static GUIStyle TreeRowBackgroundGUIStyle
        {
            get
            {
                if (s_TreeRowBackgroundGUIStyle == null) {
                    s_TreeRowBackgroundGUIStyle = new GUIStyle("RL Background");
                }
                return s_TreeRowBackgroundGUIStyle;
            }
        }

        private string[] m_ToolbarStrings = { "Item Types", "Categories"};
        [SerializeField] private ItemCollection m_ItemCollection;
        [SerializeField] private bool m_DrawItemType = true;
        [SerializeField] private string m_CategoryName;
        [SerializeField] private string m_ItemTypeName;
        [SerializeField] private TreeViewState m_CategoryTreeViewState;
        [SerializeField] private TreeViewState m_ItemTypeTreeViewState;
        private FlatTreeView<CategoryCollectionModal> m_CategoryTreeView;
        private FlatTreeView<ItemTypeCollectionModal> m_ItemTypeTreeView;
        private SearchField m_CategorySearchField;
        private SearchField m_ItemTypeSearchField;

        /// <summary>
        /// Default ItemTypeManager constructor.
        /// </summary>
        public ItemTypeManager()
        {
            // Create the category TreeView.
            m_CategoryTreeViewState = new TreeViewState();
            var itemTypeModal = new ItemTypeCollectionModal();
            var categoryModal = new CategoryCollectionModal();
            categoryModal.BeforeModalChange += OnTreeWillChange;
            categoryModal.AfterModalChange += OnTreeChangeReload;
            m_CategoryTreeView = new FlatTreeView<CategoryCollectionModal>(m_CategoryTreeViewState, categoryModal);
            m_CategoryTreeView.TreeChange += OnTreeChange;
            m_CategorySearchField = new SearchField();
            m_CategorySearchField.downOrUpArrowKeyPressed += m_CategoryTreeView.SetFocusAndEnsureSelectedItem;

            // Create the ItemType TreeView.
            m_ItemTypeTreeViewState = new TreeViewState();
            itemTypeModal.BeforeModalChange += OnTreeWillChange;
            itemTypeModal.AfterModalChange += OnTreeChangeReload;
            m_ItemTypeTreeView = new FlatTreeView<ItemTypeCollectionModal>(m_ItemTypeTreeViewState, itemTypeModal);
            m_ItemTypeTreeView.TreeChange += OnTreeChange;
            m_ItemTypeSearchField = new SearchField();
            m_ItemTypeSearchField.downOrUpArrowKeyPressed += m_ItemTypeTreeView.SetFocusAndEnsureSelectedItem;

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        /// <summary>
        /// Unsubscribe from any events when the object is destroyed.
        /// </summary>
        ~ItemTypeManager()
        {
            m_CategoryTreeView.TreeChange -= OnTreeChange;
            m_ItemTypeTreeView.TreeChange -= OnTreeChange;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        /// <summary>
        /// Initialize the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            // Look for the ItemCollection within the scene if it isn't already populated.
            if (m_ItemCollection == null) {
                m_ItemCollection = ManagerUtility.FindItemCollection(m_MainManagerWindow);
            }

            // The ItemCollection may have been serialized.
            if (m_ItemCollection != null) {
                // The CategoryState would have been reconstructed after deserialization so update the state within the tree.
                m_CategoryTreeView.state.expandedIDs = m_CategoryTreeViewState.expandedIDs;
                m_CategoryTreeView.state.lastClickedID = m_CategoryTreeViewState.lastClickedID;
                m_CategoryTreeView.state.searchString = m_CategoryTreeViewState.searchString;
                m_CategoryTreeView.state.selectedIDs = m_CategoryTreeViewState.selectedIDs;
                m_CategoryTreeViewState = m_CategoryTreeView.state;

                // Update the tree with the new collection.
                (m_CategoryTreeView.TreeModal as CategoryCollectionModal).ItemCollection = m_ItemCollection;
                m_CategoryTreeView.Reload();

                // The TreeViewState would have been reconstructed after deserialization so update the state within the tree.
                m_ItemTypeTreeView.state.expandedIDs = m_ItemTypeTreeViewState.expandedIDs;
                m_ItemTypeTreeView.state.lastClickedID = m_ItemTypeTreeViewState.lastClickedID;
                m_ItemTypeTreeView.state.searchString = m_ItemTypeTreeViewState.searchString;
                m_ItemTypeTreeView.state.selectedIDs = m_ItemTypeTreeViewState.selectedIDs;
                m_ItemTypeTreeViewState = m_ItemTypeTreeView.state;

                // Update the tree with the new collection.
                (m_ItemTypeTreeView.TreeModal as ItemTypeCollectionModal).ItemCollection = m_ItemCollection;
                m_ItemTypeTreeView.Reload();
            }
        }

        /// <summary>
        /// Draws the ItemTypeManager.
        /// </summary>
        public override void OnGUI()
        {
            var toolbarSelection = GUILayout.Toolbar(m_DrawItemType ? 0 : 1, m_ToolbarStrings, EditorStyles.toolbarButton);
            m_DrawItemType = toolbarSelection == 0;
            GUILayout.Space(10);

            if (m_DrawItemType) {
                GUILayout.Label("Item Types", InspectorStyles.CenterBoldLabel);
            } else {
                GUILayout.Label("Categories", InspectorStyles.CenterBoldLabel);
            }

            EditorGUILayout.BeginHorizontal();
            var itemCollection = EditorGUILayout.ObjectField("Item Collection", m_ItemCollection, typeof(ItemCollection), false) as ItemCollection;
            if (GUILayout.Button("Create", GUILayout.MaxWidth(100))) {
                var path = EditorUtility.SaveFilePanel("Save Item Collection", "Assets", "ItemCollection.asset", "asset");
                if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                    itemCollection = ScriptableObject.CreateInstance<ItemCollection>();
                    itemCollection.Categories = new ItemCollection.Category[] { new ItemCollection.Category(new System.Random(Guid.NewGuid().GetHashCode()).Next(), "Items") };

                    // Save the collection.
                    path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.CreateAsset(itemCollection, path);
                    AssetDatabase.ImportAsset(path);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (itemCollection != m_ItemCollection) {
                m_ItemCollection = itemCollection;
                if (m_ItemCollection != null) {
                    EditorPrefs.SetString(ManagerUtility.LastItemCollectionGUIDString, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(itemCollection)));
                    (m_CategoryTreeView.TreeModal as CategoryCollectionModal).ItemCollection = m_ItemCollection;
                    (m_ItemTypeTreeView.TreeModal as ItemTypeCollectionModal).ItemCollection = m_ItemCollection;
                } else {
                    EditorPrefs.SetString(ManagerUtility.LastItemCollectionGUIDString, string.Empty);
                    (m_CategoryTreeView.TreeModal as CategoryCollectionModal).ItemCollection = null;
                    (m_ItemTypeTreeView.TreeModal as ItemTypeCollectionModal).ItemCollection = null;
                }
                m_CategoryTreeView.Reload();
                m_ItemTypeTreeView.Reload();
            }

            // ItemCollection must be populated in order to create Categories/ItemTypes.
            if (m_ItemCollection == null) {
                EditorGUILayout.HelpBox("An ItemCollection must be selected. Use the \"Create\" button to create a new collection.", MessageType.Error);
                return;
            }

            if (m_DrawItemType) {
                DrawItemTypes();
            } else {
                DrawCategories();
            }
        }

        /// <summary>
        /// Draws the Categories editor.
        /// </summary>
        private void DrawCategories()
        {
            var categories = m_ItemCollection.Categories;
            if (categories == null) {
                // At least one category needs to exist.
                var category = new ItemCollection.Category((new System.Random()).Next(), "Items");
                categories = m_ItemCollection.Categories = new ItemCollection.Category[] { category };
            }

            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("CategoryName");
            m_CategoryName = EditorGUILayout.TextField("Name", m_CategoryName);
            GUI.enabled = !string.IsNullOrEmpty(m_CategoryName) && (m_CategoryTreeView.TreeModal as CategoryCollectionModal).IsUniqueName(m_CategoryName);
            if (GUILayout.Button("Add", GUILayout.Width(100)) || (Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "CategoryName")) {
                // Create the new Category.
                var category = new ItemCollection.Category();
                category.Name = m_CategoryName;
                category.ID = (m_CategoryTreeView.TreeModal as CategoryCollectionModal).GetUniqueCategoryID();
                // Add the Category to the ItemCollection.
                Array.Resize(ref categories, categories.Length + 1);
                categories[categories.Length - 1] = category;
                m_ItemCollection.Categories = categories;

                // Reset.
                EditorUtility.SetDirty(m_ItemCollection);
                m_CategoryName = string.Empty;
                GUI.FocusControl("");
                m_CategoryTreeView.Reload();
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
            GUILayout.Space(5);

            if (categories != null) {
                var guiRect = GUILayoutUtility.GetLastRect();
                var height = m_MainManagerWindow.position.height - guiRect.yMax - 21;
                m_CategoryTreeView.searchString = m_CategorySearchField.OnGUI(new Rect(0, guiRect.yMax, m_MainManagerWindow.position.width - m_MainManagerWindow.MenuWidth - 2, 20), m_CategoryTreeView.searchString);
                m_CategoryTreeView.OnGUI(new Rect(0, guiRect.yMax + 20, m_MainManagerWindow.position.width - m_MainManagerWindow.MenuWidth - 1, height));
                // OnGUI doesn't update the GUILayout rect so add a blank space to account for it.
                GUILayout.Space(height + 10);
            }
        }

        /// <summary>
        /// Draws the ItemTypes editor.
        /// </summary>
        private void DrawItemTypes()
        {
            var itemTypes = m_ItemCollection.ItemTypes;

            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("ItemTypeName");
            m_ItemTypeName = EditorGUILayout.TextField("Name", m_ItemTypeName);
            GUI.enabled = !string.IsNullOrEmpty(m_ItemTypeName) && (m_ItemTypeTreeView.TreeModal as ItemTypeCollectionModal).IsUniqueName(m_ItemTypeName);
            if (GUILayout.Button("Add", GUILayout.Width(100)) || (Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "ItemTypeName")) {
                // Create the new ItemType.
                var itemType = ScriptableObject.CreateInstance<ItemType>();
                itemType.name = m_ItemTypeName;

                // Add the ItemType to the ItemCollection.
                Array.Resize(ref itemTypes, itemTypes != null ? itemTypes.Length + 1 : 1);
                itemType.ID = itemTypes.Length - 1;
                itemTypes[itemTypes.Length - 1] = itemType;
                m_ItemCollection.ItemTypes = itemTypes;
                AssetDatabase.AddObjectToAsset(itemType, m_ItemCollection);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_ItemCollection));

                // Select the newly added item.
                m_ItemTypeTreeView.SetSelection(new List<int>() { itemType.ID }, TreeViewSelectionOptions.FireSelectionChanged);

                // Reset.
                EditorUtility.SetDirty(m_ItemCollection);
                m_ItemTypeName = string.Empty;
                GUI.FocusControl("");
                m_ItemTypeTreeView.Reload();
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
            GUILayout.Space(5);

            if (itemTypes != null && itemTypes.Length > 0) {
                var guiRect = GUILayoutUtility.GetLastRect();
                var height = m_MainManagerWindow.position.height - guiRect.yMax - 21;
                m_ItemTypeTreeView.searchString = m_ItemTypeSearchField.OnGUI(new Rect(0, guiRect.yMax, m_MainManagerWindow.position.width - m_MainManagerWindow.MenuWidth - 2, 20), m_ItemTypeTreeView.searchString);
                m_ItemTypeTreeView.OnGUI(new Rect(0, guiRect.yMax + 20, m_MainManagerWindow.position.width - m_MainManagerWindow.MenuWidth - 1, height));
                // OnGUI doesn't update the GUILayout rect so add a blank space to account for it.
                GUILayout.Space(height + 10);
            }
        }

        /// <summary>
        /// Save the current state of the tree before it is changed for the undo manager.
        /// </summary>
        private void OnTreeWillChange()
        {
            Undo.RegisterCompleteObjectUndo(m_ItemCollection, "Change Tree");
        }

        /// <summary>
        /// The tree has changed - mark for serialization.
        /// </summary>
        private void OnTreeChange()
        {
            // Marking the GUI as changed will reserialize the managers.
            GUI.changed = true;
        }

        /// <summary>
        /// The tree has changed - mark for serialization and reload the tree.
        /// </summary>
        private void OnTreeChangeReload()
        {
            // Marking the GUI as changed will reserialize the managers.
            GUI.changed = true;
            m_CategoryTreeView.Reload();
            m_ItemTypeTreeView.Reload();
        }

        /// <summary>
        /// Reload the TreeView with an undo redo.
        /// </summary>
        private void OnUndoRedo()
        {
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_ItemCollection));
            m_CategoryTreeView.Reload();
            m_ItemTypeTreeView.Reload();
        }
    }

    /// <summary>
    /// The CategoryCollectionModal inherits TreeModal to provide a tree modal for the category class.
    /// </summary>
    public class CategoryCollectionModal : TreeModal
    {
        // Specifies the height of the row.
        private const float c_RowHeight = 30;
        // Specifies the height of the selected row.
        private const float c_SelectedRowHeight = 65;

        private ItemCollection m_ItemCollection;

        public ItemCollection ItemCollection { get { return m_ItemCollection; } set { m_ItemCollection = value; } }

        /// <summary>
        /// Returns the number of rows in the tree.
        /// </summary>
        /// <returns>The number of rows in the tree.</returns>
        public override int GetRowCount()
        {
            if (m_ItemCollection == null || m_ItemCollection.Categories == null) {
                return 0;
            }
            return m_ItemCollection.Categories.Length;
        }

        /// <summary>
        /// Returns the height of the row.
        /// </summary>
        /// <param name="item">The item that occupies the row with the requested height.</param>
        /// <param name="state">The state of the tree.</param>
        /// <returns>The height of the row.</returns>
        public override float GetRowHeight(TreeViewItem item, TreeViewState state)
        {
            return IsSelected(item, state) ? c_SelectedRowHeight : c_RowHeight;
        }

        /// <summary>
        /// Draws the GUI for the row.
        /// </summary>
        /// <param name="rowRect">The rect of the row being drawn.</param>
        /// <param name="item">The item that occupies the row which is being drawn.</param>
        /// <param name="state">The state of the tree.</param>
        public override void RowGUI(Rect rowRect, TreeViewItem item, TreeViewState state)
        {
            var isSelected = IsSelected(item, state);

            // Leave some spacing on the sides of the row.
            rowRect.xMax -= 2;
            rowRect.yMin += 2;
            rowRect.yMax -= 2;

            // Draw the background.
            DrawBackground(rowRect, isSelected);

            // Draw the header.
            DrawHeader(rowRect, item.id);

            EditorGUI.BeginChangeCheck();

            // Draws the category controls.
            if (DrawControls(rowRect, item.id)) {
                if (EditorGUI.EndChangeCheck()) {
                    // Serialize the changes.
                    EditorUtility.SetDirty(m_ItemCollection);
                    if (m_AfterModalChange != null) {
                        m_AfterModalChange();
                    }
                }
                return;
            }

            // If the category is selected then draw the details.
            if (isSelected) {
                DrawDetails(rowRect, item.id);
            }

            if (EditorGUI.EndChangeCheck()) {
                // Serialize the changes.
                EditorUtility.SetDirty(m_ItemCollection);
                if (m_AfterModalChange != null) {
                    m_AfterModalChange();
                }
            }
        }

        /// <summary>
        /// Draws the background of the Category.
        /// </summary>
        /// <param name="rowRect">The rect to draw the background in.</param>
        /// <param name="isSelected">Is the current Category selected?</param>
        private void DrawBackground(Rect rowRect, bool isSelected)
        {
            if (Event.current.type != EventType.Repaint) {
                return;
            }

            var rect = rowRect;
            // If the row is selected then clamp the header background to the row height to prevent it from taking up the entire height.
            if (isSelected) {
                rect.height = c_RowHeight;
            }
            ItemTypeManager.TreeRowHeaderGUIStyle.Draw(rect, false, false, false, false);

            if (isSelected) {
                rect.y += rect.height;
                rect.height = rowRect.height - rect.height;
                ItemTypeManager.TreeRowBackgroundGUIStyle.Draw(rect, false, false, false, false);
            }
        }

        /// <summary>
        /// Draws the header of the Category.
        /// </summary>
        /// <param name="rowRect">The rect of the Category row.</param>
        /// <param name="id">The id of the Category to draw the header of.</param>
        private void DrawHeader(Rect rowRect, int id)
        {
            var rect = rowRect;
            rect.x += 4;
            rect.y += 4;
            GUI.Label(rect, m_ItemCollection.Categories[id].Name);
        }

        /// <summary>
        /// Draws the identify, duplicate and delete buttons for the category.
        /// </summary>
        /// <param name="rowRect">The rect of the category row.</param>
        /// <param name="id">The id of the category to draw the controls of.</param>
        /// <returns>True if the controls changed the ItemCollection.</returns>
        private bool DrawControls(Rect rowRect, int id)
        {
            var duplicateRect = rowRect;
            duplicateRect.x = duplicateRect.width - 44;
            duplicateRect.width = 20;
            duplicateRect.y += 4;
            duplicateRect.height = 16;
            if (GUI.Button(duplicateRect, InspectorStyles.DuplicateIcon, InspectorStyles.NoPaddingButtonStyle)) {
                // Generate a unique name for the category.
                var categories = m_ItemCollection.Categories;
                var category = categories[id];
                var index = 1;
                string name;
                do {
                    name = category.Name + " (" + index + ")";
                    index++;
                } while (!IsUniqueName(name));

                var clonedCategory = new ItemCollection.Category(GetUniqueCategoryID(), name);

                // Add the Category to the ItemCollection.
                Array.Resize(ref categories, categories.Length + 1);
                categories[categories.Length - 1] = clonedCategory;
                m_ItemCollection.Categories = categories;
                EditorUtility.SetDirty(m_ItemCollection);
                return true;
            }

            var deleteRect = rowRect;
            deleteRect.x = deleteRect.width - 20;
            deleteRect.width = 18;
            deleteRect.y += 4;
            deleteRect.height = 16;
            GUI.enabled = m_ItemCollection.Categories.Length > 1;
            if (GUI.Button(deleteRect, InspectorStyles.DeleteIcon, InspectorStyles.NoPaddingButtonStyle)) {
                // The category can't be deleted if other ItemTypes depend on it.
                var canRemove = true;
                for (int i = 0; i < m_ItemCollection.ItemTypes.Length; ++i) {
                    if (m_ItemCollection.ItemTypes[i].CategoryIDMatch(m_ItemCollection.Categories[id].ID)) {
                        EditorUtility.DisplayDialog("Unable to Delete", "Unable to delete the category: the ItemType " + m_ItemCollection.ItemTypes[i].name + " uses this category", "OK");
                        canRemove = false;
                        break;
                    }
                }

                if (canRemove) {
                    if (m_BeforeModalChange != null) {
                        m_BeforeModalChange();
                    }

                    // Remove the ItemType.
                    var categories = new List<ItemCollection.Category>(m_ItemCollection.Categories);
                    categories.RemoveAt(id);
                    m_ItemCollection.Categories = categories.ToArray();
                    EditorUtility.SetDirty(m_ItemCollection);
                }
                GUI.enabled = true;
                return true;
            }
            GUI.enabled = true;
            return false;
        }

        /// <summary>
        /// Returns a unique ID for the category.
        /// </summary>
        /// <returns>The unique ID of the category.</returns>
        public int GetUniqueCategoryID()
        {
            var categories = m_ItemCollection.Categories;
            var random = new System.Random(Guid.NewGuid().GetHashCode());
            bool generateID;
            int categoryID;
            do {
                categoryID = random.Next();
                generateID = false;
                if (categoryID == 0) {
                    generateID = true;
                } else {
                    for (int i = 0; i < categories.Length; ++i) {
                        if (categories[i].ID == categoryID) {
                            generateID = true;
                            break;
                        }
                    }
                }
            } while (generateID);
            return categoryID;
        }

        /// <summary>
        /// Draws the details of the category.
        /// </summary>
        /// <param name="rowRect">The rect of the category row.</param>
        /// <param name="id">The id of the category to draw the details of.</param>
        private void DrawDetails(Rect rowRect, int id)
        {
            var rect = rowRect;
            rect.x += 4;
            rect.y += c_RowHeight;
            InspectorUtility.RecordUndoDirtyObject(m_ItemCollection, "Category Change");

            var category = m_ItemCollection.Categories[id];

            // Name property.
            var nameRect = rect;
            nameRect.y += 6;
            nameRect.height = 16;
            nameRect.width -= 16;
            var name = InspectorUtility.DrawEditorWithoutSelectAll(() => InspectorUtility.ClampTextField(nameRect, "Name", category.Name, 22));
            // The name must be unique.
            if (name != category.Name) {
                if (IsUniqueName(name)) {
                    category.Name = name;
                }
            }
        }

        /// <summary>
        /// Is the category name unique?
        /// </summary>
        /// <param name="name">The name of the category.</param>
        /// <returns>True if the category name is unique.</returns>
        public bool IsUniqueName(string name)
        {
            if (m_ItemCollection.Categories == null) {
                return true;
            }
            for (int i = 0; i < m_ItemCollection.Categories.Length; ++i) {
                if (m_ItemCollection.Categories[i].Name.ToLower().CompareTo(name.ToLower()) == 0) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Moves the rows to the specified index.
        /// </summary>
        /// <param name="rows">The rows being moved.</param>
        /// <param name="insertIndex">The index to insert the rows at.</param>
        /// <returns>An updated list of row ids.</returns>
        public override List<int> MoveRows(List<int> rows, int insertIndex)
        {
            if (m_BeforeModalChange != null) {
                m_BeforeModalChange();
            }

            var insertIDs = new List<int>();
            var categories = m_ItemCollection.Categories;
            // Move the rows in the array. This method will shift each rows without needing to allocate a new array for each move.
            for (int i = 0; i < rows.Count; ++i) {
                // Shift the array rows up to make space for the moved rows.
                if (insertIndex < rows[i]) {
                    var insertElement = categories[rows[i]];
                    for (int j = rows[i]; j > insertIndex + i; --j) {
                        categories[j] = categories[j - 1];
                    }
                    categories[insertIndex + i] = insertElement;
                } else {
                    // Shift the array rows down to make space for the moved rows.
                    insertIndex--;
                    var insertElement = categories[rows[i] - i];
                    for (int j = rows[i] - i; j < insertIndex + i; ++j) {
                        categories[j] = categories[j + 1];
                    }
                    categories[insertIndex + i] = insertElement;
                }
                insertIDs.Add(insertIndex + i);
            }
            return insertIDs;
        }

        /// <summary>
        /// Does the specified row id match the search?
        /// </summary>
        /// <param name="id">The id of the row.</param>
        /// <param name="searchString">The string value of the search.</param>
        /// <returns>True if the row matches the search string.</returns>
        public override bool MatchesSearch(int id, string searchString)
        {
            return m_ItemCollection.Categories[id].Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Is the specified item selected?
        /// </summary>
        /// <param name="item">The item to test against.</param>
        /// <param name="state">The state of the TreeView.</param>
        /// <returns>True if the item is selected.</returns>
        private bool IsSelected(TreeViewItem item, TreeViewState state)
        {
            if (state.selectedIDs.Count > 0) {
                if (item.id == state.selectedIDs[0]) { // Only one row can be selected at a time.
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// The ItemTypeCollectionModal inherits TreeModal to provide a tree modal for the ItemType class.
    /// </summary>
    public class ItemTypeCollectionModal : TreeModal
    {
        // Specifies the height of the row.
        private const float c_RowHeight = 30;
        // Specifies the height of the selected row.
        private const float c_SelectedRowHeight = 190;

        private ItemCollection m_ItemCollection;

        public ItemCollection ItemCollection { get { return m_ItemCollection; } set { m_ItemCollection = value; } }

        /// <summary>
        /// Returns the number of rows in the tree.
        /// </summary>
        /// <returns>The number of rows in the tree.</returns>
        public override int GetRowCount()
        {
            if (m_ItemCollection == null || m_ItemCollection.ItemTypes == null) {
                return 0;
            }
            return m_ItemCollection.ItemTypes.Length;
        }

        /// <summary>
        /// Returns the height of the row.
        /// </summary>
        /// <param name="item">The item that occupies the row with the requested height.</param>
        /// <param name="state">The state of the tree.</param>
        /// <returns>The height of the row.</returns>
        public override float GetRowHeight(TreeViewItem item, TreeViewState state)
        {
            var selected = IsSelected(item, state);
            var height = selected ? c_SelectedRowHeight : c_RowHeight;
            // Extra spacing needs to be added for each referenced dropped ItemType.
            if (selected) {
                var itemType = m_ItemCollection.ItemTypes[item.id];
                if (itemType.DroppedItemTypes != null) {
                    height += itemType.DroppedItemTypes.Length * 20;
                }
            }

            return height;
        }

        /// <summary>
        /// Draws the GUI for the row.
        /// </summary>
        /// <param name="rowRect">The rect of the row being drawn.</param>
        /// <param name="item">The item that occupies the row which is being drawn.</param>
        /// <param name="state">The state of the tree.</param>
        public override void RowGUI(Rect rowRect, TreeViewItem item, TreeViewState state)
        {
            var isSelected = IsSelected(item, state);

            // Leave some spacing on the sides of the row.
            rowRect.xMax -= 2;
            rowRect.yMin += 2;
            rowRect.yMax -= 2;

            // Draw the background.
            DrawBackground(rowRect, isSelected);

            // Draw the header.
            DrawHeader(rowRect, item.id);

            EditorGUI.BeginChangeCheck();

            // Draws the ItemType controls.
            if (DrawControls(rowRect, item.id)) {
                if (EditorGUI.EndChangeCheck()) {
                    // Serialize the changes.
                    EditorUtility.SetDirty(m_ItemCollection);
                    if (m_AfterModalChange != null) {
                        m_AfterModalChange();
                    }
                }
                return;
            }

            // If the ItemType is selected then draw the details.
            if (isSelected) {
                DrawDetails(rowRect, item.id);
            }

            if (EditorGUI.EndChangeCheck()) {
                // Serialize the changes.
                EditorUtility.SetDirty(m_ItemCollection);
                if (m_AfterModalChange != null) {
                    m_AfterModalChange();
                }
            }
        }

        /// <summary>
        /// Draws the background of the ItemType.
        /// </summary>
        /// <param name="rowRect">The rect to draw the background in.</param>
        /// <param name="isSelected">Is the current ItemType selected?</param>
        private void DrawBackground(Rect rowRect, bool isSelected)
        {
            if (Event.current.type != EventType.Repaint) {
                return;
            }

            var rect = rowRect;
            // If the row is selected then clamp the header background to the row height to prevent it from taking up the entire height.
            if (isSelected) {
                rect.height = c_RowHeight;
            }
            ItemTypeManager.TreeRowHeaderGUIStyle.Draw(rect, false, false, false, false);

            if (isSelected) {
                rect.y += rect.height;
                rect.height = rowRect.height - rect.height;
                ItemTypeManager.TreeRowBackgroundGUIStyle.Draw(rect, false, false, false, false);
            }
        }

        /// <summary>
        /// Draws the header of the ItemType.
        /// </summary>
        /// <param name="rowRect">The rect of the ItemType row.</param>
        /// <param name="id">The id of the ItemType to draw the header of.</param>
        private void DrawHeader(Rect rowRect, int id)
        {
            var rect = rowRect;
            rect.x += 4;
            rect.y += 4;
            GUI.Label(rect, m_ItemCollection.ItemTypes[id].name);
        }

        /// <summary>
        /// Draws the identify, duplicate and delete buttons for the ItemType.
        /// </summary>
        /// <param name="rowRect">The rect of the ItemType row.</param>
        /// <param name="id">The id of the ItemType to draw the controls of.</param>
        /// <returns>True if the controls changed the ItemCollection.</returns>
        private bool DrawControls(Rect rowRect, int id)
        {
            var identifyRect = rowRect;
            identifyRect.x = identifyRect.width - 68;
            identifyRect.width = 20;
            identifyRect.y += 4;
            identifyRect.height = 16;
            if (GUI.Button(identifyRect, InspectorStyles.InfoIcon, InspectorStyles.NoPaddingButtonStyle)) {
                Selection.activeObject = m_ItemCollection.ItemTypes[id];
                EditorGUIUtility.PingObject(Selection.activeObject);
            }

            var duplicateRect = rowRect;
            duplicateRect.x = duplicateRect.width - 44;
            duplicateRect.width = 20;
            duplicateRect.y += 4;
            duplicateRect.height = 16;
            if (GUI.Button(duplicateRect, InspectorStyles.DuplicateIcon, InspectorStyles.NoPaddingButtonStyle)) {
                var itemType = m_ItemCollection.ItemTypes[id];
                var clonedItemType = UnityEngine.Object.Instantiate(itemType);
                // Generate a unique name for the item.
                var index = 1;
                string name;
                do {
                    name = itemType.name + " (" + index + ")";
                    index++;
                } while (!IsUniqueName(name));
                clonedItemType.name = name;

                // Add the ItemType to the ItemCollection.
                var itemTypes = m_ItemCollection.ItemTypes;
                Array.Resize(ref itemTypes, itemTypes.Length + 1);
                clonedItemType.ID = itemTypes.Length - 1;
                itemTypes[itemTypes.Length - 1] = clonedItemType;
                m_ItemCollection.ItemTypes = itemTypes;
                AssetDatabase.AddObjectToAsset(clonedItemType, m_ItemCollection);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_ItemCollection));
                EditorUtility.SetDirty(m_ItemCollection);
                return true;
            }

            var deleteRect = rowRect;
            deleteRect.x = deleteRect.width - 20;
            deleteRect.width = 18;
            deleteRect.y += 4;
            deleteRect.height = 16;
            if (GUI.Button(deleteRect, InspectorStyles.DeleteIcon, InspectorStyles.NoPaddingButtonStyle)) {
                if (m_BeforeModalChange != null) {
                    m_BeforeModalChange();
                }

                // Remove the ItemType.
                var itemTypes = new List<ItemType>(m_ItemCollection.ItemTypes);
                Undo.DestroyObjectImmediate(m_ItemCollection.ItemTypes[id]);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_ItemCollection));
                itemTypes.RemoveAt(id);
                m_ItemCollection.ItemTypes = itemTypes.ToArray();
                EditorUtility.SetDirty(m_ItemCollection);

                // Update all of the ItemIDs.
                for (int i = 0; i < itemTypes.Count; ++i) {
                    m_ItemCollection.ItemTypes[i].ID = i;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws the details of the ItemType.
        /// </summary>
        /// <param name="rowRect">The rect of the ItemType row.</param>
        /// <param name="id">The id of the ItemType to draw the details of.</param>
        private void DrawDetails(Rect rowRect, int id)
        {
            var rect = rowRect;
            rect.x += 4;
            rect.y += c_RowHeight;

            var itemType = m_ItemCollection.ItemTypes[id];
            InspectorUtility.RecordUndoDirtyObject(itemType, "ItemType Change");

            // Name and description properties.
            var nameRect = rect;
            nameRect.y += 4;
            nameRect.width -= 12;
            nameRect.height = 16;

            // Prevent the label from being far away from the text.
            var name = InspectorUtility.DrawEditorWithoutSelectAll(() => InspectorUtility.ClampTextField(nameRect, "Name", itemType.name, 2));
            if (itemType.name != name) {
                if (IsUniqueName(name)) {
                    itemType.name = name;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_ItemCollection));
                }
            }

            // Allow for a description of the ItemType.
            var descriptionRect = nameRect;
            descriptionRect.y += descriptionRect.height + 4;
            EditorGUI.LabelField(descriptionRect, "Description");
            descriptionRect.x += 2;
            descriptionRect.y += descriptionRect.height + 2;
            descriptionRect.width -= 2;
            descriptionRect.height = 42;
            itemType.Description = InspectorUtility.DrawEditorWithoutSelectAll(() => EditorGUI.TextArea(descriptionRect, itemType.Description, InspectorStyles.WordWrapTextArea));

            // The ItemType must belong to a category.
            var categoryRect = rect;
            categoryRect.y = descriptionRect.yMax + 8;
            categoryRect.height = 16;
            categoryRect.width -= 12;

            var selectedMask = 0;
            var categoryNames = new string[m_ItemCollection.Categories.Length];
            for (int i = 0; i < categoryNames.Length; ++i) {
                categoryNames[i] = m_ItemCollection.Categories[i].Name;
                if (itemType.CategoryIDMatch(m_ItemCollection.Categories[i].ID)) {
                    selectedMask |= 1 << i;
                }
            }
            var categoryMask = InspectorUtility.ClampMaskField(categoryRect, "Category", selectedMask, categoryNames, 55);
            if (categoryMask != selectedMask) {
                var selectedIDs = new List<int>();
                for (int i = 0; i < categoryNames.Length; ++i) {
                    if ((categoryMask & (1 << i)) == (1 << i)) {
                        selectedIDs.Add(m_ItemCollection.Categories[i].ID);
                    }
                }
                itemType.CategoryIDs = selectedIDs.ToArray();
            }

            // The ItemType can set a max capacity to restrict the item amount.
            var capacityRect = categoryRect;
            capacityRect.y = capacityRect.yMax + 4;
            itemType.Capacity = InspectorUtility.DrawEditorWithoutSelectAll(() => InspectorUtility.ClampFloatField(capacityRect, "Capacity", itemType.Capacity, 59));

            // The ItemType can drop other ItemTypes. Allow for the selection here.
            var nameItemTypeMap = new Dictionary<string, ItemType>();
            for (int i = 0; i < m_ItemCollection.ItemTypes.Length; ++i) {
                if (m_ItemCollection.ItemTypes[i] == itemType) {
                    continue;
                }
                nameItemTypeMap.Add(m_ItemCollection.ItemTypes[i].name, m_ItemCollection.ItemTypes[i]);
            }

            // Draw all of the previously selected ItemTypes.
            var dropItemTypesRect = capacityRect;
            dropItemTypesRect.width -= 23;
            var dropItemTypesButtonRect = capacityRect;
            dropItemTypesButtonRect.x = dropItemTypesRect.xMax + 4;
            dropItemTypesButtonRect.width = 18;
            if (itemType.DroppedItemTypes != null) {
                for (int i = 0; i < itemType.DroppedItemTypes.Length; ++i) {
                    dropItemTypesButtonRect.y = dropItemTypesRect.y = dropItemTypesRect.yMax + 4;
                    if (i == 1) {
                        dropItemTypesRect.x += 112;
                        dropItemTypesRect.width -= 112;
                    }
                    DrawDropItemTypePopup(dropItemTypesRect, itemType, nameItemTypeMap, itemType.DroppedItemTypes[i], i == 0);

                    if (GUI.Button(dropItemTypesButtonRect, InspectorStyles.RemoveIcon, InspectorStyles.NoPaddingButtonStyle)) {
                        var droppedItemTypesList = new List<ItemType>(itemType.DroppedItemTypes);
                        droppedItemTypesList.RemoveAt(i);
                        itemType.DroppedItemTypes = droppedItemTypesList.ToArray();
                    }
                }
            }

            // A new ItemType should be able to be added.
            dropItemTypesRect.y = dropItemTypesRect.yMax + 4;
            if (itemType.DroppedItemTypes != null && itemType.DroppedItemTypes.Length == 1) {
                dropItemTypesRect.x += 112;
                dropItemTypesRect.width -= 112;
            }
            dropItemTypesRect.width += 23;
            DrawDropItemTypePopup(dropItemTypesRect, itemType, nameItemTypeMap, null, (itemType.DroppedItemTypes == null || itemType.DroppedItemTypes.Length == 0));
        }

        /// <summary>
        /// Is the item type name unique?
        /// </summary>
        /// <param name="name">The name of the item type.</param>
        /// <returns>True if the item type is unique.</returns>
        public bool IsUniqueName(string name)
        {
            if (m_ItemCollection.ItemTypes == null) {
                return true;
            }
            for (int i = 0; i < m_ItemCollection.ItemTypes.Length; ++i) {
                if (m_ItemCollection.ItemTypes[i].name.ToLower().CompareTo(name.ToLower())== 0) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Drops the Popup for the reference of the drop ItemType.
        /// </summary>
        private void DrawDropItemTypePopup(Rect rect, ItemType itemType, Dictionary<string, ItemType> nameItemTypeMap, ItemType droppedItemType, bool showLabel)
        {
            var allItemTypeNames = new List<string>();
            allItemTypeNames.Add("(Add)");
            var prevSelected = 0;
            for (int i = 0; i < m_ItemCollection.ItemTypes.Length; ++i) {
                // The ItemType can always drop itself.
                if (m_ItemCollection.ItemTypes[i] == itemType) {
                    continue;
                }

                if (m_ItemCollection.ItemTypes[i] == droppedItemType) {
                    prevSelected = allItemTypeNames.Count;
                } else {
                    // Don't show any ItemTypes that are already included.
                    var containsItemType = false;
                    if (itemType.DroppedItemTypes != null) {
                        for (int j = 0; j < itemType.DroppedItemTypes.Length; ++j) {
                            if (m_ItemCollection.ItemTypes[i] == itemType.DroppedItemTypes[j]) {
                                containsItemType = true;
                                break;
                            }
                        }
                    }
                    if (containsItemType) {
                        continue;
                    }
                }

                allItemTypeNames.Add(m_ItemCollection.ItemTypes[i].name);
            }

            // Determine the index of the popup.
            if (droppedItemType != null) {
                for (int i = 1; i < allItemTypeNames.Count; ++i) {
                    if (allItemTypeNames[i] == droppedItemType.name) {
                        prevSelected = i;
                        break;
                    }
                }
            }

            var selected = InspectorUtility.ClampPopupField(rect, showLabel ? "Drop ItemTypes" : string.Empty, prevSelected, allItemTypeNames.ToArray(), 15);
            // A new selection has been made.
            if (prevSelected != selected) {
                var droppedItemTypes = itemType.DroppedItemTypes;
                if (droppedItemTypes == null) {
                    droppedItemTypes = new ItemType[0];
                }

                var droppedItemTypesList = new List<ItemType>(droppedItemTypes);
                // Remove the old.
                if (prevSelected != 0) {
                    droppedItemTypesList.Remove(nameItemTypeMap[allItemTypeNames[prevSelected]]);
                }
                // Add the new.
                if (selected != 0) {
                    droppedItemTypesList.Add(nameItemTypeMap[allItemTypeNames[selected]]);
                }
                itemType.DroppedItemTypes = droppedItemTypesList.ToArray();
            }

            // Remove the name from the list so tat ItemType name is no longer drawn.
            if (selected != 0) {
                allItemTypeNames.Remove(allItemTypeNames[selected]);
            }
        }

        /// <summary>
        /// Moves the rows to the specified index.
        /// </summary>
        /// <param name="rows">The rows being moved.</param>
        /// <param name="insertIndex">The index to insert the rows at.</param>
        /// <returns>An updated list of row ids.</returns>
        public override List<int> MoveRows(List<int> rows, int insertIndex)
        {
            if (m_BeforeModalChange != null) {
                m_BeforeModalChange();
            }

            var insertIDs = new List<int>();
            var itemTypes = m_ItemCollection.ItemTypes;
            // Move the rows in the array. This method will shift each rows without needing to allocate a new array for each move.
            for (int i = 0; i < rows.Count; ++i) {
                // Shift the array rows up to make space for the moved rows.
                if (insertIndex < rows[i]) {
                    var insertElement = itemTypes[rows[i]];
                    for (int j = rows[i]; j > insertIndex + i; --j) {
                        itemTypes[j] = itemTypes[j - 1];
                        itemTypes[j].ID = j;
                    }
                    itemTypes[insertIndex + i] = insertElement;
                    itemTypes[insertIndex + i].ID = insertIndex + i;
                } else {
                    // Shift the array rows down to make space for the moved rows.
                    insertIndex--;
                    var insertElement = itemTypes[rows[i] - i];
                    for (int j = rows[i] - i; j < insertIndex + i; ++j) {
                        itemTypes[j] = itemTypes[j + 1];
                        itemTypes[j].ID = j;
                    }
                    itemTypes[insertIndex + i] = insertElement;
                    itemTypes[insertIndex + i].ID = insertIndex + i;
                }
                insertIDs.Add(insertIndex + i);
            }
            return insertIDs;
        }

        /// <summary>
        /// Does the specified row id match the search?
        /// </summary>
        /// <param name="id">The id of the row.</param>
        /// <param name="searchString">The string value of the search.</param>
        /// <returns>True if the row matches the search string.</returns>
        public override bool MatchesSearch(int id, string searchString)
        {
            return m_ItemCollection.ItemTypes[id].name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Is the specified item selected?
        /// </summary>
        /// <param name="item">The item to test against.</param>
        /// <param name="state">The state of the TreeView.</param>
        /// <returns>True if the item is selected.</returns>
        private bool IsSelected(TreeViewItem item, TreeViewState state)
        {
            if (state.selectedIDs.Count > 0) {
                if (item.id == state.selectedIDs[0]) { // Only one row can be selected at a time.
                    return true;
                }
            }
            return false;
        }
    }
}