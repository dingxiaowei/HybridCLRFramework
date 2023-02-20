/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Editor.Controls
{
    /// <summary>
    /// Uses Unity's TreeView class to create a single column tree view that does not have any children. The elements can be reordered and searched.
    /// </summary>
    public class FlatTreeView<T> : TreeView where T : TreeModal
    {
        private const string c_GenericDragID = "ModalDragging";

        private TreeModal m_TreeModal;
        private List<TreeViewItem> m_Rows = new List<TreeViewItem>();
        private TreeViewItem m_Root;

        private Action m_TreeChange;

        public TreeModal TreeModal { get { return m_TreeModal; } set { m_TreeModal = value; Reload(); } }
        public Action TreeChange { get { return m_TreeChange; } set { m_TreeChange = value; } }

        /// <summary>
        /// Constructor for FlatTreeView.
        /// </summary>
        /// <param name="state">The TreeView's state.</param>
        /// <param name="modal">The TreeView's data.</param>
        public FlatTreeView(TreeViewState state, TreeModal modal) : base (state)
        {
            showBorder = true;
            m_TreeModal = modal;
            Reload();
        }

        /// <summary>
        /// Creates the root element of the TreeView.
        /// </summary>
        /// <returns>The TreeView's root element.</returns>
        protected override TreeViewItem BuildRoot()
        {
            m_Root = new TreeViewItem(0, -1, "Root");
            return m_Root;
        }

        /// <summary>
        /// Creates all of the TreeView rows.
        /// </summary>
        /// <param name="root">The root of the tree.</param>
        /// <returns>A list of all of the TreeView rows.</returns>
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            m_Rows.Clear();
            if (!string.IsNullOrEmpty(searchString) && root.children != null) {
                // Not all of the rows are shown while searching.
                Search(root, m_Rows);
            } else {
                // Show all of the rows.
                var rowCount = m_TreeModal.GetRowCount();
                for (int i = 0; i < rowCount; ++i) {
                    m_Rows.Add(new TreeViewItem(i, -1));
                }
                SetupParentsAndChildrenFromDepths(root, m_Rows);

                // If the children list was null then the tree hasn't been initialized yet. At this point the tree would have been initialized so
                // perform a serach if necessary while the list is initialized.
                if (!string.IsNullOrEmpty(searchString)) {
                    m_Rows.Clear();
                    Search(root, m_Rows);
                }
            }
            return m_Rows;
        }

        /// <summary>
        /// Searches the tree for the searchString.
        /// </summary>
        /// <param name="root">The root of the tree.</param>
        /// <param name="result">Any found rows.</param>
        private void Search(TreeViewItem root, List<TreeViewItem> result)
        {
            for (int i = 0; i < root.children.Count; ++i) {
                if (m_TreeModal.MatchesSearch(root.children[i].id, searchString)) {
                    result.Add(root.children[i]);
                }
            }
        }

        /// <summary>
        /// Returns a custom height for the row.
        /// </summary>
        /// <param name="row">The row to get the custom height of.</param>
        /// <param name="item">The item to get the custom height of.</param>
        /// <returns>The custom height for the row.</returns>
        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            var height = m_TreeModal.GetRowHeight(item, state);
            // -1 indicates the model doesn't supply the height.
            if (height != -1) {
                return height;
            }
            return base.GetCustomRowHeight(row, item);
        }

        /// <summary>
        /// Draws the row with the specified arguments.
        /// </summary>
        /// <param name="args">The row to draw.</param>
        protected override void RowGUI(RowGUIArgs args)
        {
            var rowRect = args.rowRect;
            rowRect.x = GetContentIndent(args.item);
            m_TreeModal.RowGUI(rowRect, args.item, state);
        }

        /// <summary>
        /// Called when the TreeView changes selection.
        /// </summary>
        /// <param name="selectedIds">The new ids being selected.</param>
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            RefreshCustomRowHeights();
            Repaint();

            // Notify those interested that there was a change - this allows the tree to be serialized.
            if (m_TreeChange != null) {
                m_TreeChange();
            }
        }

        /// <summary>
        /// Can the TreeView have multiple selections?
        /// </summary>
        /// <param name="item">Can this item be part of a multiselection?</param>
        /// <returns>True if the TreeView can have multiple selections.</returns>
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        /// <summary>
        /// Can the row be dragged?
        /// </summary>
        /// <param name="args">The row that is trying to be dragged.</param>
        /// <returns>True if the row can be dragged.</returns>
        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return !hasSearch;
        }

        /// <summary>
        /// Prepares the row for a drag.
        /// </summary>
        /// <param name="args">The row that is being dragged.</param>
        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            var draggedRows = new List<TreeViewItem>();
            var rows = GetRows();
            // Convert the row IDs to row items.
            for (int i = 0; i < rows.Count; ++i) {
                for (int j = 0; j < args.draggedItemIDs.Count; ++j) {
                    if (rows[i].id == args.draggedItemIDs[j]) {
                        draggedRows.Add(rows[i]);
                        break;
                    }
                }
            }
            // Start the drag.
            DragAndDrop.SetGenericData(c_GenericDragID, draggedRows);
            DragAndDrop.objectReferences = new UnityEngine.Object[] { }; // Required for dragging to work.
            DragAndDrop.StartDrag("Drag");
        }

        /// <summary>
        /// The row is being dragged - handle the dragging.
        /// </summary>
        /// <param name="args">The row that is being dragged.</param>
        /// <returns>The status of the drag.</returns>
        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            // Return early if the dragging is occurring from a different window.
            var draggedRows = DragAndDrop.GetGenericData(c_GenericDragID) as List<TreeViewItem>;
            if (draggedRows == null) {
                return DragAndDropVisualMode.None;
            }

            switch (args.dragAndDropPosition) {
                case DragAndDropPosition.UponItem: // Dropping on top of other items is not allowed in a flat tree.
                    return DragAndDropVisualMode.None;
                case DragAndDropPosition.BetweenItems: // The item can be dropped in between other items.
                    {
                        if (args.performDrop) {
                            // Do the drop.
                            OnDropDraggedElementsAtIndex(draggedRows, args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
                        }
                        return DragAndDropVisualMode.Move;
                    }

                case DragAndDropPosition.OutsideItems: // The item will be dropped to the last row if it is outside the tree.
                    {
                        if (args.performDrop) {
                            // Do the drop.
                            OnDropDraggedElementsAtIndex(draggedRows, m_Root.children.Count - 1);
                        }
                        return DragAndDropVisualMode.Move;
                    }
                default:
                    Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
                    return DragAndDropVisualMode.None;
            }
        }

        /// <summary>
        /// The rows specified have been dropped at the insert index.
        /// </summary>
        /// <param name="draggedRows">The rows that have been dropped.</param>
        /// <param name="insertIndex">The index to insert the dropped rows at.</param>
        public virtual void OnDropDraggedElementsAtIndex(List<TreeViewItem> draggedRows, int insertIndex)
        {
            // Convert the rows indicies to row ids.
            var draggedElements = new List<int>();
            for (int i = 0; i < draggedRows.Count; ++i) {
                draggedElements.Add(draggedRows[i].id);
            }

            // Let the model to the drop.
            var insertIDs = m_TreeModal.MoveRows(draggedElements, insertIndex);
            // Update the selection.
            SetSelection(insertIDs, TreeViewSelectionOptions.RevealAndFrame);
            RefreshCustomRowHeights();

            // Notify those interested that the tree has changed.
            if (m_TreeChange != null) {
                m_TreeChange();
            }
        }
    }

    /// <summary>
    /// The TreeModal class acts as the data source for the tree. 
    /// </summary>
    [Serializable]
    public abstract class TreeModal
    {
        protected Action m_BeforeModalChange;
        protected Action m_AfterModalChange;
        public Action BeforeModalChange { get { return m_BeforeModalChange; } set { m_BeforeModalChange = value; } }
        public Action AfterModalChange { get { return m_AfterModalChange; } set { m_AfterModalChange = value; } }

        /// <summary>
        /// Returns the number of rows in the tree.
        /// </summary>
        /// <returns>The number of rows in the tree.</returns>
        public abstract int GetRowCount();

        /// <summary>
        /// Returns the height of the row.
        /// </summary>
        /// <param name="item">The item that occupies the row with the requested height.</param>
        /// <param name="state">The state of the tree.</param>
        /// <returns>The height of the row.</returns>
        public virtual float GetRowHeight(TreeViewItem item, TreeViewState state) { return -1; }

        /// <summary>
        /// Draws the GUI for the row.
        /// </summary>
        /// <param name="rowRect">The rect of the row being drawn.</param>
        /// <param name="item">The item that occupies the row which is being drawn.</param>
        /// <param name="state">The state of the tree.</param>
        public abstract void RowGUI(Rect rowRect, TreeViewItem item, TreeViewState state);

        /// <summary>
        /// Moves the rows to the specified index.
        /// </summary>
        /// <param name="rows">The rows being moved.</param>
        /// <param name="insertIndex">The index to insert the rows at.</param>
        /// <returns>An updated list of row ids.</returns>
        public abstract List<int> MoveRows(List<int> rows, int insertIndex);

        /// <summary>
        /// Does the specified row id match the search?
        /// </summary>
        /// <param name="id">The id of the row.</param>
        /// <param name="searchString">The string value of the search.</param>
        /// <returns>True if the row matches the search string.</returns>
        public virtual bool MatchesSearch(int id, string searchString) { return false; }
    }
}