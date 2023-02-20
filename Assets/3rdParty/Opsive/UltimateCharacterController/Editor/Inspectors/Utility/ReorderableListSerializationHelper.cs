/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using System;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Utility
{
    /// <summary>
    /// Helper class for managing the ReoderableList.
    /// </summary>
    public static class ReorderableListSerializationHelper
    {
        private static Dictionary<Type, List<Type>> s_ObjectTypes = new Dictionary<Type, List<Type>>();
        private static Dictionary<Type, bool> s_CanAddMultipleTypes = new Dictionary<Type, bool>();

        /// <summary>
        /// The object has been enabled again.
        /// </summary>
        public static void OnEnable()
        {
            // Start with a fresh cache.
            s_ObjectTypes.Clear();
            s_CanAddMultipleTypes.Clear();
            InspectorDrawerUtility.OnEnable();
        }

        /// <summary>
        /// Draws the ReorderableList.
        /// </summary>
        public static void DrawReorderableList(ref ReorderableList reorderableList, InspectorBase inspector, Array drawnObject, string serializedData,  
                                                ReorderableList.HeaderCallbackDelegate drawHeaderCallback, ReorderableList.ElementCallbackDelegate drawElementCallback, 
                                                ReorderableList.ReorderCallbackDelegate reorderCallback, ReorderableList.AddCallbackDelegate addCallback, 
                                                ReorderableList.RemoveCallbackDelegate removeCallback, ReorderableList.SelectCallbackDelegate selectCallback,
                                                Action<int> drawSelectedElementCallback, string key, bool requireOne, bool indentList)
        {
            // Initialize the reorder list on first run.
            if (reorderableList == null) {
                var data = inspector.PropertyFromName(inspector.serializedObject, serializedData);
                reorderableList = new ReorderableList(inspector.serializedObject, data, (reorderCallback != null), true, !Application.isPlaying, 
                                                    !Application.isPlaying && (!requireOne || (drawnObject != null && drawnObject.Length > 1)));
                reorderableList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Name");
                };
                if (drawHeaderCallback != null) {
                    reorderableList.drawHeaderCallback = drawHeaderCallback;
                }
                reorderableList.drawElementCallback = drawElementCallback;
                if (reorderCallback != null) {
                    reorderableList.onReorderCallback = reorderCallback;
                }
                reorderableList.onAddCallback = addCallback;
                reorderableList.onRemoveCallback = removeCallback;
                reorderableList.onSelectCallback = selectCallback;
                if (EditorPrefs.GetInt(key, -1) != -1) {
                    reorderableList.index = EditorPrefs.GetInt(key, -1);
                }
            }

            var listRect = GUILayoutUtility.GetRect(0, reorderableList.GetHeight());
            // Indent the list so it lines up with the rest of the content.
            if (indentList) {
                listRect.x += InspectorUtility.IndentWidth;
                listRect.xMax -= InspectorUtility.IndentWidth;
            }
            reorderableList.DoList(listRect);
            if (reorderableList != null && reorderableList.index != -1) {
                if (drawnObject != null && reorderableList.index < drawnObject.Length) {
                    EditorGUI.indentLevel++;
                    drawSelectedElementCallback(reorderableList.index);
                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>
        /// Draws all of the elements.
        /// </summary>
        public static void OnListDraw(Array objects, Rect rect, int index, bool friendlyNamespacePrefix)
        {
            var obj = objects.GetValue(index);
            EditorGUI.LabelField(rect, InspectorUtility.DisplayTypeName(obj.GetType(), friendlyNamespacePrefix));
        }

        /// <summary>
        /// Shows the add menu for the specified type
        /// </summary>
        public static void AddObjectType(Type type, bool friendlyNamespacePrefix, Array existingTypes, GenericMenu.MenuFunction2 addCallback)
        {
            List<Type> typeList;
            if (!s_ObjectTypes.TryGetValue(type, out typeList)) {
                // Search through all of the assemblies to find any types that derive from specified type.
                typeList = new List<Type>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; ++i) {
                    var assemblyTypes = assemblies[i].GetTypes();
                    for (int j = 0; j < assemblyTypes.Length; ++j) {
                        // Must derive from specified type.
                        if (!type.IsAssignableFrom(assemblyTypes[j])) {
                            continue;
                        }

                        // Ignore abstract classes.
                        if (assemblyTypes[j].IsAbstract) {
                            continue;
                        }

                        // Ability types should not show ItemAbilities.
                        if (type == typeof(Ability) && typeof(ItemAbility).IsAssignableFrom(assemblyTypes[j])) {
                            continue;
                        }

                        typeList.Add(assemblyTypes[j]);
                    }
                }
                s_ObjectTypes.Add(type, typeList);
            }

            var addMenu = new GenericMenu();
            for (int i = 0; i < typeList.Count; ++i) {
                // Do not show already added types.
                var addType = true;
                if (!CanAddMultipleTypes(typeList[i]) && existingTypes != null) {
                    for (int j = 0; j < existingTypes.Length; ++j) {
                        if (existingTypes.GetValue(j).GetType() == typeList[i]) {
                            addType = false;
                            break;
                        }
                    }
                }
                if (!addType) {
                    continue;
                }
                
                addMenu.AddItem(new GUIContent(InspectorUtility.DisplayTypeName(typeList[i], friendlyNamespacePrefix)), false, addCallback, typeList[i]);
            }

            addMenu.ShowAsContext();
        }

        /// <summary>
        /// Returns true if multiple of the same type can be added.
        /// </summary>
        /// <param name="abilityType">The type of object.</param>
        /// <returns>True if multiple of the same type can be added.</returns>
        private static bool CanAddMultipleTypes(Type type)
        {
            // Multiple effects can always be added.
            if (typeof(UltimateCharacterController.Character.Effects.Effect).IsAssignableFrom(type)) {
                return true;
            }

            bool multipleTypes;
            if (s_CanAddMultipleTypes.TryGetValue(type, out multipleTypes)) {
                return multipleTypes;
            }

            multipleTypes = type.GetCustomAttributes(typeof(AllowMultipleAbilityTypes), true).Length > 0;
            s_CanAddMultipleTypes.Add(type, multipleTypes);
            return multipleTypes;
        }
    }
}