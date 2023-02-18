/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions;
    using Opsive.UltimateCharacterController.Items.Actions.Magic.ImpactActions;
    using Opsive.UltimateCharacterController.Items.Actions.Magic.BeginEndActions;
    using Opsive.UltimateCharacterController.StateSystem;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Shows a custom inspector for the MagicItem component.
    /// </summary>
    [CustomEditor(typeof(MagicItem), true)]
    public class MagicItemInspector : UsableItemInspector
    {
        private const string c_EditorPrefsSelectedBeginActionIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedBeginActionIndex";
        private const string c_EditorPrefsSelectedBeginActionStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Magic.SelectedBeginActionStateIndex";
        private const string c_EditorPrefsSelectedCastActionIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedCastActionIndex";
        private const string c_EditorPrefsSelectedCastActionStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Magic.SelectedCastActionStateIndex";
        private const string c_EditorPrefsSelectedImpactActionIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedImpactActionIndex";
        private const string c_EditorPrefsSelectedImpactActionStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Magic.SelectedImpactActionStateIndex";
        private const string c_EditorPrefsSelectedEndActionIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedEndActionIndex";
        private const string c_EditorPrefsSelectedEndActionStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Magic.SelectedEndActionStateIndex";
        private string SelectedBeginActionIndexKey { get { return c_EditorPrefsSelectedBeginActionIndexKey + "." + target.GetType() + "." + target.name; } }
        private string SelectedCastActionIndexKey { get { return c_EditorPrefsSelectedCastActionIndexKey + "." + target.GetType() + "." + target.name; } }
        private string SelectedImpactActionIndexKey { get { return c_EditorPrefsSelectedImpactActionIndexKey + "." + target.GetType() + "." + target.name; } }
        private string SelectedEndActionIndexKey { get { return c_EditorPrefsSelectedEndActionIndexKey + "." + target.GetType() + "." + target.name; } }

        private MagicItem m_MagicItem;

        private ReorderableList m_ReorderableBeginActionList;
        private ReorderableList m_ReorderableBeginActionStateList;
        private ReorderableList m_ReorderableCastActionList;
        private ReorderableList m_ReorderableCastActionStateList;
        private ReorderableList m_ReorderableImpactActionList;
        private ReorderableList m_ReorderableImpactActionStateList;
        private ReorderableList m_ReorderableEndActionList;
        private ReorderableList m_ReorderableEndActionStateList;

        /// <summary>
        /// Initialize any starting values.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_MagicItem = target as MagicItem;
            m_MagicItem.DeserializeBeginActions(true);
            m_MagicItem.DeserializeCastActions(true);
            m_MagicItem.DeserializeImpactActions(true);
        }

        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                if (Foldout("Magic")) {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(PropertyFromName("m_RequireGrounded"));
                    var directionProperty = PropertyFromName("m_Direction");
                    EditorGUILayout.PropertyField(directionProperty);
                    if (directionProperty.enumValueIndex != (int)MagicItem.CastDirection.None) {
                        EditorGUI.indentLevel++;
                        if (directionProperty.enumValueIndex != (int)MagicItem.CastDirection.Target) {
                            EditorGUILayout.PropertyField(PropertyFromName("m_UseLookSource"));
                        }
                        EditorGUILayout.PropertyField(PropertyFromName("m_MaxDistance"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_Radius"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_DetectLayers"));
                        if (directionProperty.enumValueIndex == (int)MagicItem.CastDirection.Target) {
                            EditorGUILayout.PropertyField(PropertyFromName("m_MaxAngle"));
                            EditorGUILayout.PropertyField(PropertyFromName("m_MaxCollisionCount"));
                            EditorGUILayout.PropertyField(PropertyFromName("m_TargetCount"));
                        } else if (directionProperty.enumValueIndex == (int)MagicItem.CastDirection.Indicate) {
                            EditorGUILayout.PropertyField(PropertyFromName("m_SurfaceIndicator"));
                            EditorGUILayout.PropertyField(PropertyFromName("m_SurfaceIndicatorOffset"));
                        }
                        EditorGUI.indentLevel--;
                    }
                    var useTypeProperty = PropertyFromName("m_UseType");
                    EditorGUILayout.PropertyField(useTypeProperty);
                    if (useTypeProperty.enumValueIndex == (int)MagicItem.CastUseType.Continuous) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_MinContinuousUseDuration"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_ContinuousCast"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.PropertyField(PropertyFromName("m_UseAmount"));

                    // Interrupt Source should use a MaskField.
                    var castInterruptSource = (int)InspectorUtility.GetFieldValue<MagicItem.CastInterruptSource>(target, "m_InterruptSource");
                    var castInterruptSourceString = System.Enum.GetNames(typeof(MagicItem.CastInterruptSource));
                    var value = EditorGUILayout.MaskField(new GUIContent("Interrupt Source", InspectorUtility.GetFieldTooltip(target, "m_InterruptSource")), castInterruptSource, castInterruptSourceString);
                    if (value != castInterruptSource) {
                        InspectorUtility.SetFieldValue(target, "m_InterruptSource", value);
                    }

                    EditorGUILayout.PropertyField(PropertyFromName("m_SurfaceImpact"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_CanStopSubstateIndexAddition"));
                    if (Foldout("Begin Actions")) {
                        EditorGUI.indentLevel++;
                        ReorderableListSerializationHelper.DrawReorderableList(ref m_ReorderableBeginActionList, this, m_MagicItem.BeginActions, "m_BeginActionData",
                                                                        OnBeginActionListDrawHeader, OnBeginActionListDraw, OnBeginActionListReorder, OnBeginActionListAdd, OnBeginActionListRemove, OnBeginActionListSelect,
                                                                        DrawSelectedBeginAction, SelectedBeginActionIndexKey, false, true);
                        EditorGUI.indentLevel--;
                    }
                    if (Foldout("Cast Actions")) {
                        EditorGUI.indentLevel++;
                        ReorderableListSerializationHelper.DrawReorderableList(ref m_ReorderableCastActionList, this, m_MagicItem.CastActions, "m_CastActionData",
                                                                        OnCastActionListDrawHeader, OnCastActionListDraw, OnCastActionListReorder, OnCastActionListAdd, OnCastActionListRemove, OnCastActionListSelect,
                                                                        DrawSelectedCastAction, SelectedCastActionIndexKey, false, true);
                        EditorGUI.indentLevel--;
                    }
                    if (Foldout("Impact Actions")) {
                        EditorGUI.indentLevel++;
                        ReorderableListSerializationHelper.DrawReorderableList(ref m_ReorderableImpactActionList, this, m_MagicItem.ImpactActions, "m_ImpactActionData",
                                                                        OnImpactActionListDrawHeader, OnImpactActionListDraw, OnImpactActionListReorder, OnImpactActionListAdd, OnImpactActionListRemove, OnImpactActionListSelect,
                                                                        DrawSelectedImpactAction, SelectedImpactActionIndexKey, false, true);
                        EditorGUI.indentLevel--;
                    }
                    if (Foldout("End Actions")) {
                        EditorGUI.indentLevel++;
                        ReorderableListSerializationHelper.DrawReorderableList(ref m_ReorderableEndActionList, this, m_MagicItem.EndActions, "m_EndActionData",
                                                                        OnEndActionListDrawHeader, OnEndActionListDraw, OnEndActionListReorder, OnEndActionListAdd, OnEndActionListRemove, OnEndActionListSelect,
                                                                        DrawSelectedEndAction, SelectedEndActionIndexKey, false, true);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }

        /// <summary>
        /// Draws the header for the begin action list.
        /// </summary>
        private void OnBeginActionListDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Begin Actions");
        }

        /// <summary>
        /// Draws the header for the cast action list.
        /// </summary>
        private void OnCastActionListDrawHeader(Rect rect)
        {
            var activeRect = rect;
            activeRect.x += 13;
            activeRect.width -= 33;
            EditorGUI.LabelField(activeRect, "Cast Actions");

            activeRect.x += activeRect.width - 32;
            activeRect.width = 50;
            EditorGUI.LabelField(activeRect, new GUIContent("Delay", "The delay to start the cast after the item has been used."));
        }

        /// <summary>
        /// Draws the header for the impact action list.
        /// </summary>
        private void OnImpactActionListDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Impact Actions");
        }

        /// <summary>
        /// Draws the header for the end action list.
        /// </summary>
        private void OnEndActionListDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "End Actions");
        }

        /// <summary>
        /// Draws all of the begin actions.
        /// </summary>
        private void OnBeginActionListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The index may be out of range if the component was copied.
            if (index >= m_MagicItem.BeginActions.Length) {
                m_ReorderableBeginActionList.index = -1;
                EditorPrefs.SetInt(SelectedBeginActionIndexKey, m_ReorderableBeginActionList.index);
                return;
            }

            var beginAction = m_MagicItem.BeginActions[index];
            if (beginAction == null) {
                SerializeBeginActions();
                return;
            }

            EditorGUI.LabelField(rect, InspectorUtility.DisplayTypeName(beginAction.GetType(), true));
        }

        /// <summary>
        /// Draws all of the cast actions.
        /// </summary>
        private void OnCastActionListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The index may be out of range if the component was copied.
            if (index >= m_MagicItem.CastActions.Length) {
                m_ReorderableCastActionList.index = -1;
                EditorPrefs.SetInt(SelectedCastActionIndexKey, m_ReorderableCastActionList.index);
                return;
            }

            var castAction = m_MagicItem.CastActions[index];
            if (castAction == null) {
                SerializeCastActions();
                return;
            }

            // Reduce the rect width so the delay field can be added.
            var activeRect = rect;
            activeRect.width -= 30;
            EditorGUI.LabelField(activeRect, InspectorUtility.DisplayTypeName(castAction.GetType(), true));

            // Draw the delay field and serialize if there is a change.
            EditorGUI.BeginChangeCheck();
            activeRect = rect;
            activeRect.x += activeRect.width - 50;
            activeRect.y += 1;
            activeRect.width = 33;
            activeRect.height = 17f;
            castAction.Delay = EditorGUI.FloatField(activeRect, castAction.Delay);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeCastActions();
            }
        }

        /// <summary>
        /// Draws all of the impact actions.
        /// </summary>
        private void OnImpactActionListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The index may be out of range if the component was copied.
            if (index >= m_MagicItem.ImpactActions.Length) {
                m_ReorderableImpactActionList.index = -1;
                EditorPrefs.SetInt(SelectedImpactActionIndexKey, m_ReorderableImpactActionList.index);
                return;
            }

            var castAction = m_MagicItem.ImpactActions[index];
            if (castAction == null) {
                SerializeImpactActions();
                return;
            }

            EditorGUI.LabelField(rect, InspectorUtility.DisplayTypeName(castAction.GetType(), true));
        }

        /// <summary>
        /// Draws all of the end actions.
        /// </summary>
        private void OnEndActionListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The index may be out of range if the component was copied.
            if (index >= m_MagicItem.EndActions.Length) {
                m_ReorderableEndActionList.index = -1;
                EditorPrefs.SetInt(SelectedEndActionIndexKey, m_ReorderableEndActionList.index);
                return;
            }

            var endAction = m_MagicItem.EndActions[index];
            if (endAction == null) {
                SerializeEndActions();
                return;
            }

            EditorGUI.LabelField(rect, InspectorUtility.DisplayTypeName(endAction.GetType(), true));
        }

        /// <summary>
        /// The begin action list has been reordered.
        /// </summary>
        private void OnBeginActionListReorder(ReorderableList list)
        {
            // Deserialize the actions so the array will be correct. The list operates on the BeginActionData array.
            m_MagicItem.DeserializeBeginActions(true);

            // Update the selected index.
            EditorPrefs.SetInt(SelectedBeginActionIndexKey, m_ReorderableBeginActionList.index);
        }

        /// <summary>
        /// The cast action list has been reordered.
        /// </summary>
        private void OnCastActionListReorder(ReorderableList list)
        {
            // Deserialize the actions so the array will be correct. The list operates on the CastActionData array.
            m_MagicItem.DeserializeCastActions(true);

            // Update the selected index.
            EditorPrefs.SetInt(SelectedCastActionIndexKey, m_ReorderableCastActionList.index);
        }

        /// <summary>
        /// The impact action list has been reordered.
        /// </summary>
        private void OnImpactActionListReorder(ReorderableList list)
        {
            // Deserialize the actions so the array will be correct. The list operates on the ImpactActionData array.
            m_MagicItem.DeserializeImpactActions(true);

            // Update the selected index.
            EditorPrefs.SetInt(SelectedImpactActionIndexKey, m_ReorderableImpactActionList.index);
        }

        /// <summary>
        /// The end action list has been reordered.
        /// </summary>
        private void OnEndActionListReorder(ReorderableList list)
        {
            // Deserialize the actions so the array will be correct. The list operates on the EndActionData array.
            m_MagicItem.DeserializeEndActions(true);

            // Update the selected index.
            EditorPrefs.SetInt(SelectedEndActionIndexKey, m_ReorderableEndActionList.index);
        }

        /// <summary>
        /// Adds a new begin action element to the list.
        /// </summary>
        private void OnBeginActionListAdd(ReorderableList list)
        {
            ReorderableListSerializationHelper.AddObjectType(typeof(BeginEndAction), true, m_MagicItem.BeginActions, AddBeginAction);
        }

        /// <summary>
        /// Adds a new cast action element to the list.
        /// </summary>
        private void OnCastActionListAdd(ReorderableList list)
        {
            ReorderableListSerializationHelper.AddObjectType(typeof(CastAction), true, m_MagicItem.CastActions, AddCastAction);
        }

        /// <summary>
        /// Adds a new impact action element to the list.
        /// </summary>
        private void OnImpactActionListAdd(ReorderableList list)
        {
            ReorderableListSerializationHelper.AddObjectType(typeof(ImpactAction), true, m_MagicItem.ImpactActions, AddImpactAction);
        }

        /// <summary>
        /// Adds a new end action element to the list.
        /// </summary>
        private void OnEndActionListAdd(ReorderableList list)
        {
            ReorderableListSerializationHelper.AddObjectType(typeof(BeginEndAction), true, m_MagicItem.EndActions, AddEndAction);
        }

        /// <summary>
        /// Adds the begin action with the specified type.
        /// </summary>
        private void AddBeginAction(object obj)
        {
            m_MagicItem.DeserializeBeginActions(false);
            var beginActions = m_MagicItem.BeginActions;
            if (beginActions == null) {
                beginActions = new BeginEndAction[1];
            } else {
                Array.Resize(ref beginActions, beginActions.Length + 1);
            }
            beginActions[beginActions.Length - 1] = Activator.CreateInstance((Type)obj) as BeginEndAction;
            m_MagicItem.BeginActions = beginActions;
            SerializeBeginActions();

            // Select the newly added action.
            m_ReorderableBeginActionList.index = m_MagicItem.BeginActions.Length - 1;
            EditorPrefs.SetInt(SelectedBeginActionIndexKey, m_ReorderableBeginActionList.index);
        }

        /// <summary>
        /// Adds the cast action with the specified type.
        /// </summary>
        private void AddCastAction(object obj)
        {
            m_MagicItem.DeserializeCastActions(false);
            var castActions = m_MagicItem.CastActions;
            if (castActions == null) {
                castActions = new CastAction[1];
            } else {
                Array.Resize(ref castActions, castActions.Length + 1);
            }
            castActions[castActions.Length - 1] = Activator.CreateInstance((Type)obj) as CastAction;
            m_MagicItem.CastActions = castActions;
            SerializeCastActions();

            // Select the newly added action.
            m_ReorderableCastActionList.index = m_MagicItem.CastActions.Length - 1;
            EditorPrefs.SetInt(SelectedCastActionIndexKey, m_ReorderableCastActionList.index);
        }

        /// <summary>
        /// Adds the impact action with the specified type.
        /// </summary>
        private void AddImpactAction(object obj)
        {
            m_MagicItem.DeserializeImpactActions(false);
            var impactActions = m_MagicItem.ImpactActions;
            if (impactActions == null) {
                impactActions = new ImpactAction[1];
            } else {
                Array.Resize(ref impactActions, impactActions.Length + 1);
            }
            impactActions[impactActions.Length - 1] = Activator.CreateInstance((Type)obj) as ImpactAction;
            m_MagicItem.ImpactActions = impactActions;
            SerializeImpactActions();

            // Select the newly added action.
            m_ReorderableImpactActionList.index = m_MagicItem.ImpactActions.Length - 1;
            EditorPrefs.SetInt(SelectedImpactActionIndexKey, m_ReorderableImpactActionList.index);
        }

        /// <summary>
        /// Adds the end action with the specified type.
        /// </summary>
        private void AddEndAction(object obj)
        {
            m_MagicItem.DeserializeEndActions(false);
            var endActions = m_MagicItem.EndActions;
            if (endActions == null) {
                endActions = new BeginEndAction[1];
            } else {
                Array.Resize(ref endActions, endActions.Length + 1);
            }
            endActions[endActions.Length - 1] = Activator.CreateInstance((Type)obj) as BeginEndAction;
            m_MagicItem.EndActions = endActions;
            SerializeEndActions();

            // Select the newly added action.
            m_ReorderableEndActionList.index = m_MagicItem.EndActions.Length - 1;
            EditorPrefs.SetInt(SelectedEndActionIndexKey, m_ReorderableEndActionList.index);
        }

        /// <summary>
        /// Remove the begin action at the list index.
        /// </summary>
        private void OnBeginActionListRemove(ReorderableList list)
        {
            InspectorUtility.RecordUndoDirtyObject(target, "Change Value");

            var beginActions = new List<BeginEndAction>(m_MagicItem.BeginActions);
            beginActions.RemoveAt(list.index);
            m_MagicItem.BeginActions = beginActions.ToArray();
            SerializeBeginActions();

            // Update the index to point to no longer point to the now deleted action.
            list.index = list.index - 1;
            if (list.index == -1 && beginActions.Count > 0) {
                list.index = 0;
            }
            EditorPrefs.SetInt(SelectedBeginActionIndexKey, list.index);
        }

        /// <summary>
        /// Remove the cast action at the list index.
        /// </summary>
        private void OnCastActionListRemove(ReorderableList list)
        {
            InspectorUtility.RecordUndoDirtyObject(target, "Change Value");

            var castActions = new List<CastAction>(m_MagicItem.CastActions);
            castActions.RemoveAt(list.index);
            m_MagicItem.CastActions = castActions.ToArray();
            SerializeCastActions();

            // Update the index to point to no longer point to the now deleted action.
            list.index = list.index - 1;
            if (list.index == -1 && castActions.Count > 0) {
                list.index = 0;
            }
            EditorPrefs.SetInt(SelectedCastActionIndexKey, list.index);
        }

        /// <summary>
        /// Remove the impact action at the list index.
        /// </summary>
        private void OnImpactActionListRemove(ReorderableList list)
        {
            InspectorUtility.RecordUndoDirtyObject(target, "Change Value");

            var impactActions = new List<ImpactAction>(m_MagicItem.ImpactActions);
            impactActions.RemoveAt(list.index);
            m_MagicItem.ImpactActions = impactActions.ToArray();
            SerializeImpactActions();

            // Update the index to point to no longer point to the now deleted action.
            list.index = list.index - 1;
            if (list.index == -1 && impactActions.Count > 0) {
                list.index = 0;
            }
            EditorPrefs.SetInt(SelectedImpactActionIndexKey, list.index);
        }

        /// <summary>
        /// Remove the end action at the list index.
        /// </summary>
        private void OnEndActionListRemove(ReorderableList list)
        {
            InspectorUtility.RecordUndoDirtyObject(target, "Change Value");

            var endActions = new List<BeginEndAction>(m_MagicItem.EndActions);
            endActions.RemoveAt(list.index);
            m_MagicItem.EndActions = endActions.ToArray();
            SerializeEndActions();

            // Update the index to point to no longer point to the now deleted action.
            list.index = list.index - 1;
            if (list.index == -1 && endActions.Count > 0) {
                list.index = 0;
            }
            EditorPrefs.SetInt(SelectedEndActionIndexKey, list.index);
        }

        /// <summary>
        /// A new element has been selected within the list.
        /// </summary>
        private void OnBeginActionListSelect(ReorderableList list)
        {
            EditorPrefs.SetInt(SelectedBeginActionIndexKey, list.index);
            // The begin action state list should start out fresh so a reference doesn't have to be cached for each action.
            m_ReorderableBeginActionStateList = null;
        }

        /// <summary>
        /// A new element has been selected within the list.
        /// </summary>
        private void OnCastActionListSelect(ReorderableList list)
        {
            EditorPrefs.SetInt(SelectedCastActionIndexKey, list.index);
            // The cast action state list should start out fresh so a reference doesn't have to be cached for each action.
            m_ReorderableCastActionStateList = null;
        }

        /// <summary>
        /// A new element has been selected within the list.
        /// </summary>
        private void OnImpactActionListSelect(ReorderableList list)
        {
            EditorPrefs.SetInt(SelectedImpactActionIndexKey, list.index);
            // The cast action state list should start out fresh so a reference doesn't have to be cached for each action.
            m_ReorderableImpactActionStateList = null;
        }

        /// <summary>
        /// A new element has been selected within the list.
        /// </summary>
        private void OnEndActionListSelect(ReorderableList list)
        {
            EditorPrefs.SetInt(SelectedEndActionIndexKey, list.index);
            // The end action state list should start out fresh so a reference doesn't have to be cached for each action.
            m_ReorderableEndActionStateList = null;
        }

        /// <summary>
        /// Draws the specified begin action.
        /// </summary>
        private void DrawSelectedBeginAction(int index)
        {
            var beginAction = m_MagicItem.BeginActions[index];
            InspectorUtility.DrawObject(beginAction, true, false, target, true, SerializeBeginActions);

            var selectedBeginAction = beginAction as BeginEndAction;
            if (InspectorUtility.Foldout(selectedBeginAction, new GUIContent("States"), false)) {
                // The BeginAction class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the state list. When the reorderable list is drawn
                // the object will be used so it's like the dummy object never existed.
                var gameObject = new GameObject();
                var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                stateIndexHelper.StateIndexData = new int[selectedBeginAction.States.Length];
                for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                    stateIndexHelper.StateIndexData[i] = i;
                }
                var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                m_ReorderableBeginActionStateList = StateInspector.DrawStates(m_ReorderableBeginActionStateList, serializedObject, stateIndexSerializedObject.FindProperty("m_StateIndexData"),
                                                            GetSelectedBeginActionStateIndexKey(selectedBeginAction), OnBeginActionStateListDraw, OnBeginActionStateListAdd, OnBeginActionStateListReorder,
                                                            OnBeginActionStateListRemove);
                DestroyImmediate(gameObject);
            }

            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeBeginActions();
            }
        }

        /// <summary>
        /// Draws the specified cast action.
        /// </summary>
        private void DrawSelectedCastAction(int index)
        {
            var castAction = m_MagicItem.CastActions[index];
            InspectorUtility.DrawObject(castAction, true, false, target, true, SerializeCastActions);

            var selectedCastAction = castAction as CastAction;
            if (InspectorUtility.Foldout(selectedCastAction, new GUIContent("States"), false)) {
                // The CastAction class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the state list. When the reorderable list is drawn
                // the object will be used so it's like the dummy object never existed.
                var gameObject = new GameObject();
                var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                stateIndexHelper.StateIndexData = new int[selectedCastAction.States.Length];
                for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                    stateIndexHelper.StateIndexData[i] = i;
                }
                var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                m_ReorderableCastActionStateList = StateInspector.DrawStates(m_ReorderableCastActionStateList, serializedObject, stateIndexSerializedObject.FindProperty("m_StateIndexData"),
                                                            GetSelectedCastActionStateIndexKey(selectedCastAction), OnCastActionStateListDraw, OnCastActionStateListAdd, OnCastActionStateListReorder,
                                                            OnCastActionStateListRemove);
                DestroyImmediate(gameObject);
            }

            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeCastActions();
            }
        }

        /// <summary>
        /// Draws the specified impact action.
        /// </summary>
        private void DrawSelectedImpactAction(int index)
        {
            var impactAction = m_MagicItem.ImpactActions[index];
            InspectorUtility.DrawObject(impactAction, true, false, target, true, SerializeImpactActions);

            var selectedImpactAction = impactAction as ImpactAction;
            if (InspectorUtility.Foldout(selectedImpactAction, new GUIContent("States"), false)) {
                // The ImpactAction class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the state list. When the reorderable list is drawn
                // the object will be used so it's like the dummy object never existed.
                var gameObject = new GameObject();
                var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                stateIndexHelper.StateIndexData = new int[selectedImpactAction.States.Length];
                for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                    stateIndexHelper.StateIndexData[i] = i;
                }
                var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                m_ReorderableImpactActionStateList = StateInspector.DrawStates(m_ReorderableImpactActionStateList, serializedObject, stateIndexSerializedObject.FindProperty("m_StateIndexData"),
                                                            GetSelectedImpactActionStateIndexKey(selectedImpactAction), OnImpactActionStateListDraw, OnImpactActionStateListAdd, OnImpactActionStateListReorder,
                                                            OnImpactActionStateListRemove);
                DestroyImmediate(gameObject);
            }

            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeImpactActions();
            }
        }

        /// <summary>
        /// Draws the specified end action.
        /// </summary>
        private void DrawSelectedEndAction(int index)
        {
            var endAction = m_MagicItem.EndActions[index];
            InspectorUtility.DrawObject(endAction, true, false, target, true, SerializeEndActions);

            var selectedEndAction = endAction as BeginEndAction;
            if (InspectorUtility.Foldout(selectedEndAction, new GUIContent("States"), false)) {
                // The EndAction class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the state list. When the reorderable list is drawn
                // the object will be used so it's like the dummy object never existed.
                var gameObject = new GameObject();
                var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                stateIndexHelper.StateIndexData = new int[selectedEndAction.States.Length];
                for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                    stateIndexHelper.StateIndexData[i] = i;
                }
                var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                m_ReorderableEndActionStateList = StateInspector.DrawStates(m_ReorderableEndActionStateList, serializedObject, stateIndexSerializedObject.FindProperty("m_StateIndexData"),
                                                            GetSelectedEndActionStateIndexKey(selectedEndAction), OnEndActionStateListDraw, OnEndActionStateListAdd, OnEndActionStateListReorder,
                                                            OnEndActionStateListRemove);
                DestroyImmediate(gameObject);
            }

            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeEndActions();
            }
        }

        /// <summary>
        /// Serializes the begin actions.
        /// </summary>
        private void SerializeBeginActions()
        {
            var beginActions = m_MagicItem.BeginActions == null ? new List<BeginEndAction>() : new List<BeginEndAction>(m_MagicItem.BeginActions);
            m_MagicItem.BeginActionData = Shared.Utility.Serialization.Serialize<BeginEndAction>(beginActions);
            m_MagicItem.BeginActions = beginActions.ToArray();
            InspectorUtility.SetDirty(target);
        }

        /// <summary>
        /// Serializes the cast actions.
        /// </summary>
        private void SerializeCastActions()
        {
            var castActions = m_MagicItem.CastActions == null ? new List<CastAction>() : new List<CastAction>(m_MagicItem.CastActions);
            m_MagicItem.CastActionData = Shared.Utility.Serialization.Serialize<CastAction>(castActions);
            m_MagicItem.CastActions = castActions.ToArray();
            InspectorUtility.SetDirty(target);
        }

        /// <summary>
        /// Serializes the impact actions.
        /// </summary>
        private void SerializeImpactActions()
        {
            var impactActions = m_MagicItem.ImpactActions == null ? new List<ImpactAction>() : new List<ImpactAction>(m_MagicItem.ImpactActions);
            m_MagicItem.ImpactActionData = Shared.Utility.Serialization.Serialize<ImpactAction>(impactActions);
            m_MagicItem.ImpactActions = impactActions.ToArray();
            InspectorUtility.SetDirty(target);
        }

        /// <summary>
        /// Serializes the end actions.
        /// </summary>
        private void SerializeEndActions()
        {
            var endActions = m_MagicItem.EndActions == null ? new List<BeginEndAction>() : new List<BeginEndAction>(m_MagicItem.EndActions);
            m_MagicItem.EndActionData = Shared.Utility.Serialization.Serialize<BeginEndAction>(endActions);
            m_MagicItem.EndActions = endActions.ToArray();
            InspectorUtility.SetDirty(target);
        }

        /// <summary>
        /// Returns the state index key for the specified begin action.
        /// </summary>
        private string GetSelectedBeginActionStateIndexKey(BeginEndAction beginAction)
        {
            return c_EditorPrefsSelectedBeginActionStateIndexKey + "." + target.GetType() + "." + target.name + "." + beginAction.GetType();
        }

        /// <summary>
        /// Returns the state index key for the specified cast action.
        /// </summary>
        private string GetSelectedCastActionStateIndexKey(CastAction castAction)
        {
            return c_EditorPrefsSelectedCastActionStateIndexKey + "." + target.GetType() + "." + target.name + "." + castAction.GetType();
        }

        /// <summary>
        /// Returns the state index key for the specified cast action.
        /// </summary>
        private string GetSelectedImpactActionStateIndexKey(ImpactAction impactAction)
        {
            return c_EditorPrefsSelectedImpactActionStateIndexKey + "." + target.GetType() + "." + target.name + "." + impactAction.GetType();
        }

        /// <summary>
        /// Returns the state index key for the specified end action.
        /// </summary>
        private string GetSelectedEndActionStateIndexKey(BeginEndAction endAction)
        {
            return c_EditorPrefsSelectedEndActionStateIndexKey + "." + target.GetType() + "." + target.name + "." + endAction.GetType();
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnBeginActionStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_ReorderableBeginActionStateList == null) {
                return;
            }

            EditorGUI.BeginChangeCheck();
            var beginAction = m_MagicItem.BeginActions[EditorPrefs.GetInt(SelectedBeginActionIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= m_MagicItem.BeginActions[EditorPrefs.GetInt(SelectedBeginActionIndexKey)].States.Length) {
                m_ReorderableBeginActionStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedBeginActionStateIndexKey(beginAction), m_ReorderableBeginActionStateList.index);
                return;
            }

            StateInspector.OnStateListDraw(beginAction, beginAction.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeBeginActions();

                StateInspector.UpdateDefaultStateValues(beginAction.States);
            }
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnCastActionStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_ReorderableCastActionStateList == null) {
                return;
            }

            EditorGUI.BeginChangeCheck();
            var castAction = m_MagicItem.CastActions[EditorPrefs.GetInt(SelectedCastActionIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= m_MagicItem.CastActions[EditorPrefs.GetInt(SelectedCastActionIndexKey)].States.Length) {
                m_ReorderableCastActionStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedCastActionStateIndexKey(castAction), m_ReorderableCastActionStateList.index);
                return;
            }

            StateInspector.OnStateListDraw(castAction, castAction.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeCastActions();

                StateInspector.UpdateDefaultStateValues(castAction.States);
            }
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnImpactActionStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_ReorderableImpactActionStateList == null) {
                return;
            }

            EditorGUI.BeginChangeCheck();
            var impactAction = m_MagicItem.ImpactActions[EditorPrefs.GetInt(SelectedImpactActionIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= m_MagicItem.ImpactActions[EditorPrefs.GetInt(SelectedImpactActionIndexKey)].States.Length) {
                m_ReorderableImpactActionStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedImpactActionStateIndexKey(impactAction), m_ReorderableImpactActionStateList.index);
                return;
            }

            StateInspector.OnStateListDraw(impactAction, impactAction.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeImpactActions();

                StateInspector.UpdateDefaultStateValues(impactAction.States);
            }
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnEndActionStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_ReorderableEndActionStateList == null) {
                return;
            }

            EditorGUI.BeginChangeCheck();
            var endAction = m_MagicItem.EndActions[EditorPrefs.GetInt(SelectedEndActionIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= m_MagicItem.EndActions[EditorPrefs.GetInt(SelectedEndActionIndexKey)].States.Length) {
                m_ReorderableEndActionStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedEndActionStateIndexKey(endAction), m_ReorderableEndActionStateList.index);
                return;
            }

            StateInspector.OnStateListDraw(endAction, endAction.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeEndActions();

                StateInspector.UpdateDefaultStateValues(endAction.States);
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnBeginActionStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingBeginActionPreset, CreateBeginActionPreset);
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnCastActionStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingCastActionPreset, CreateCastActionPreset);
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnImpactActionStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingImpactActionPreset, CreateImpactActionPreset);
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnEndActionStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingEndActionPreset, CreateEndActionPreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingBeginActionPreset()
        {
            var beginAction = m_MagicItem.BeginActions[EditorPrefs.GetInt(SelectedBeginActionIndexKey)];
            var states = StateInspector.AddExistingPreset(beginAction.GetType(), beginAction.States, m_ReorderableBeginActionStateList, GetSelectedBeginActionStateIndexKey(beginAction));
            if (beginAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableBeginActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                beginAction.States = states;
                SerializeBeginActions();
            }
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingCastActionPreset()
        {
            var castAction = m_MagicItem.CastActions[EditorPrefs.GetInt(SelectedCastActionIndexKey)];
            var states = StateInspector.AddExistingPreset(castAction.GetType(), castAction.States, m_ReorderableCastActionStateList, GetSelectedCastActionStateIndexKey(castAction));
            if (castAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableCastActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                castAction.States = states;
                SerializeCastActions();
            }
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingImpactActionPreset()
        {
            var impactAction = m_MagicItem.ImpactActions[EditorPrefs.GetInt(SelectedImpactActionIndexKey)];
            var states = StateInspector.AddExistingPreset(impactAction.GetType(), impactAction.States, m_ReorderableImpactActionStateList, GetSelectedImpactActionStateIndexKey(impactAction));
            if (impactAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableImpactActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                impactAction.States = states;
                SerializeImpactActions();
            }
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingEndActionPreset()
        {
            var endAction = m_MagicItem.EndActions[EditorPrefs.GetInt(SelectedEndActionIndexKey)];
            var states = StateInspector.AddExistingPreset(endAction.GetType(), endAction.States, m_ReorderableEndActionStateList, GetSelectedEndActionStateIndexKey(endAction));
            if (endAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableEndActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                endAction.States = states;
                SerializeEndActions();
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateBeginActionPreset()
        {
            var beginAction = m_MagicItem.BeginActions[EditorPrefs.GetInt(SelectedBeginActionIndexKey)];
            var states = StateInspector.CreatePreset(beginAction, beginAction.States, m_ReorderableBeginActionStateList, GetSelectedBeginActionStateIndexKey(beginAction));
            if (beginAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableBeginActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                beginAction.States = states;
                SerializeBeginActions();
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateCastActionPreset()
        {
            var castAction = m_MagicItem.CastActions[EditorPrefs.GetInt(SelectedCastActionIndexKey)];
            var states = StateInspector.CreatePreset(castAction, castAction.States, m_ReorderableCastActionStateList, GetSelectedCastActionStateIndexKey(castAction));
            if (castAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableCastActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                castAction.States = states;
                SerializeCastActions();
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateImpactActionPreset()
        {
            var impactAction = m_MagicItem.ImpactActions[EditorPrefs.GetInt(SelectedImpactActionIndexKey)];
            var states = StateInspector.CreatePreset(impactAction, impactAction.States, m_ReorderableImpactActionStateList, GetSelectedImpactActionStateIndexKey(impactAction));
            if (impactAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableImpactActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                impactAction.States = states;
                SerializeImpactActions();
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateEndActionPreset()
        {
            var endAction = m_MagicItem.EndActions[EditorPrefs.GetInt(SelectedEndActionIndexKey)];
            var states = StateInspector.CreatePreset(endAction, endAction.States, m_ReorderableEndActionStateList, GetSelectedEndActionStateIndexKey(endAction));
            if (endAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableEndActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                endAction.States = states;
                SerializeEndActions();
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnBeginActionStateListReorder(ReorderableList list)
        {
            var beginAction = m_MagicItem.BeginActions[EditorPrefs.GetInt(SelectedBeginActionIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[beginAction.States.Length];
            Array.Copy(beginAction.States, copiedStates, beginAction.States.Length);
            for (int i = 0; i < beginAction.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    beginAction.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(beginAction.States);
            if (beginAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableBeginActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                beginAction.States = states;
                SerializeBeginActions();
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnCastActionStateListReorder(ReorderableList list)
        {
            var castAction = m_MagicItem.CastActions[EditorPrefs.GetInt(SelectedCastActionIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[castAction.States.Length];
            Array.Copy(castAction.States, copiedStates, castAction.States.Length);
            for (int i = 0; i < castAction.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    castAction.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(castAction.States);
            if (castAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableCastActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                castAction.States = states;
                SerializeCastActions();
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnImpactActionStateListReorder(ReorderableList list)
        {
            var impactAction = m_MagicItem.ImpactActions[EditorPrefs.GetInt(SelectedImpactActionIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[impactAction.States.Length];
            Array.Copy(impactAction.States, copiedStates, impactAction.States.Length);
            for (int i = 0; i < impactAction.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    impactAction.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(impactAction.States);
            if (impactAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableImpactActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                impactAction.States = states;
                SerializeImpactActions();
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnEndActionStateListReorder(ReorderableList list)
        {
            var endAction = m_MagicItem.EndActions[EditorPrefs.GetInt(SelectedEndActionIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[endAction.States.Length];
            Array.Copy(endAction.States, copiedStates, endAction.States.Length);
            for (int i = 0; i < endAction.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    endAction.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(endAction.States);
            if (endAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableEndActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                endAction.States = states;
                SerializeEndActions();
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnBeginActionStateListRemove(ReorderableList list)
        {
            var beginAction = m_MagicItem.BeginActions[EditorPrefs.GetInt(SelectedBeginActionIndexKey)];
            var states = StateInspector.OnStateListRemove(beginAction.States, GetSelectedBeginActionStateIndexKey(beginAction), list);
            if (beginAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableBeginActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                beginAction.States = states;
                SerializeBeginActions();
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnCastActionStateListRemove(ReorderableList list)
        {
            var castAction = m_MagicItem.CastActions[EditorPrefs.GetInt(SelectedCastActionIndexKey)];
            var states = StateInspector.OnStateListRemove(castAction.States, GetSelectedCastActionStateIndexKey(castAction), list);
            if (castAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableCastActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                castAction.States = states;
                SerializeCastActions();
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnImpactActionStateListRemove(ReorderableList list)
        {
            var impactAction = m_MagicItem.ImpactActions[EditorPrefs.GetInt(SelectedImpactActionIndexKey)];
            var states = StateInspector.OnStateListRemove(impactAction.States, GetSelectedImpactActionStateIndexKey(impactAction), list);
            if (impactAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableImpactActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                impactAction.States = states;
                SerializeImpactActions();
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnEndActionStateListRemove(ReorderableList list)
        {
            var endAction = m_MagicItem.EndActions[EditorPrefs.GetInt(SelectedEndActionIndexKey)];
            var states = StateInspector.OnStateListRemove(endAction.States, GetSelectedEndActionStateIndexKey(endAction), list);
            if (endAction.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableEndActionStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                endAction.States = states;
                SerializeEndActions();
            }
        }
    }
}