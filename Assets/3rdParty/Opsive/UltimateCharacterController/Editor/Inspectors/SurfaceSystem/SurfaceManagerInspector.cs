/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.SurfaceSystem;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.SurfaceSystem
{
    /// <summary>
    /// Shows a custom inspector for the Surface Manager.
    /// </summary>
    [CustomEditor(typeof(SurfaceManager))]
    public class SurfaceManagerInspector : InspectorBase
    {
        private const string c_EditorPrefsSelectedObjectSurfaceIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SurfaceManager.SelectedObjectSurfaceIndex";
        private string SelectedObjectSurfaceIndexKey { get { return c_EditorPrefsSelectedObjectSurfaceIndexKey + "." + target.GetType() + "." + target.name; } }

        private SurfaceManager m_SurfaceManager;
        private ReorderableList m_ObjectSurfacesList;

        /// <summary>
        /// Initialize the surface manager reference.
        /// </summary>
        private void OnEnable()
        {
            m_SurfaceManager = target as SurfaceManager;
        }

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            if (m_ObjectSurfacesList == null) {
                var objectSurfacesProperty = PropertyFromName("m_ObjectSurfaces");
                m_ObjectSurfacesList = new ReorderableList(serializedObject, objectSurfacesProperty, true, false, true, true);
                m_ObjectSurfacesList.drawHeaderCallback = OnObjectSurfaceListDrawHeader;
                m_ObjectSurfacesList.drawElementCallback = OnObjectSurfaceElementDraw;
                m_ObjectSurfacesList.onSelectCallback = OnObjectSurfaceSelect;
                if (EditorPrefs.GetInt(SelectedObjectSurfaceIndexKey, -1) != -1) {
                    m_ObjectSurfacesList.index = EditorPrefs.GetInt(SelectedObjectSurfaceIndexKey, -1);
                }
            }
            m_ObjectSurfacesList.DoLayoutList();

            if (m_SurfaceManager.ObjectSurfaces != null && m_ObjectSurfacesList.index != -1 && m_ObjectSurfacesList.index < m_SurfaceManager.ObjectSurfaces.Length) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(InspectorUtility.IndentWidth);
                EditorGUILayout.BeginVertical();
                var objectSurface = m_SurfaceManager.ObjectSurfaces[m_ObjectSurfacesList.index];
                objectSurface.SurfaceType = EditorGUILayout.ObjectField("Surface Type", objectSurface.SurfaceType, typeof(SurfaceType), true) as SurfaceType;

                if (objectSurface.UVTextures == null) {
                    objectSurface.UVTextures = new UVTexture[0];
                }
                if (objectSurface.UVTextures.Length > 0) {
                    var columnCount = Mathf.Max(1, Mathf.RoundToInt(EditorGUIUtility.currentViewWidth / 160f) - 1);
                    var columnIndex = 0;
                    for (int i = 0; i < objectSurface.UVTextures.Length; ++i) {
                        if (columnIndex % columnCount == 0) {
                            GUILayout.Space(10);
                            GUILayout.BeginHorizontal();
                        }

                        // Draw the UVTexture.
                        EditorGUILayout.BeginHorizontal();
                        objectSurface.UVTextures[i].Texture = EditorGUILayout.ObjectField(objectSurface.UVTextures[i].Texture, typeof(Texture), false, GUILayout.Width(75), GUILayout.Height(75)) as Texture;
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("UV");
                        objectSurface.UVTextures[i].UV = EditorGUILayout.RectField(objectSurface.UVTextures[i].UV, GUILayout.Width(95));
                        GUILayout.Space(3);
                        if (GUILayout.Button("Remove", GUILayout.Width(95))) {
                            var uvTextureList = new List<UVTexture>(objectSurface.UVTextures);
                            uvTextureList.RemoveAt(i);
                            objectSurface.UVTextures = uvTextureList.ToArray();
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();

                        // Allow multiple textures to be drawn per row.
                        var guiRect = GUILayoutUtility.GetLastRect();
                        GUILayout.Space(-guiRect.width - Screen.width);
                        columnIndex = (columnIndex + 1) % columnCount;
                        if (columnIndex % columnCount == 0) {
                            GUILayout.EndHorizontal();
                        }
                    }
                    if (columnIndex != 0) {
                        GUILayout.EndHorizontal();
                    }
                } else {
                    GUILayout.Space(10);
                    GUILayout.Label("(No Textures Added)");
                    GUILayout.Space(-5);
                }

                GUILayout.Space(15);
                if (GUILayout.Button("Add Texture", GUILayout.Width(140))) {
                    var uvTextures = objectSurface.UVTextures;
                    System.Array.Resize(ref uvTextures, uvTextures.Length + 1);
                    var uvTexture = uvTextures[uvTextures.Length - 1];
                    uvTexture.UV = new Rect(0, 0, 1, 1);
                    uvTextures[uvTextures.Length - 1] = uvTexture;
                    objectSurface.UVTextures = uvTextures;
                }
                GUILayout.Space(5);

                // ObjectSurface is a struct so it's passed by value and needs to be reassigned.
                m_SurfaceManager.ObjectSurfaces[m_ObjectSurfacesList.index] = objectSurface;
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (Foldout("Fallbacks")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_FallbackSurfaceImpact"), true);
                EditorGUILayout.PropertyField(PropertyFromName("m_FallbackSurfaceType"), true);
                EditorGUILayout.PropertyField(PropertyFromName("m_FallbackAllowDecals"), true);
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Draws the header for the ObjectSurface list.
        /// </summary>
        private void OnObjectSurfaceListDrawHeader(Rect rect)
        {
            GUI.Label(rect, "Object Surfaces");
        }

        /// <summary>
        /// Draws the ObjectSurface ReordableList element.
        /// </summary>
        private void OnObjectSurfaceElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The index may be out of range if the component was copied.
            if (index >= m_SurfaceManager.ObjectSurfaces.Length) {
                m_ObjectSurfacesList.index = -1;
                EditorPrefs.SetInt(SelectedObjectSurfaceIndexKey, m_ObjectSurfacesList.index);
                return;
            }

            var objectSurface = m_SurfaceManager.ObjectSurfaces[index];
            if (objectSurface.SurfaceType == null) {
                EditorGUI.LabelField(rect, "(No Surface Type)");
            } else {
                EditorGUI.LabelField(rect, objectSurface.SurfaceType.name);
            }
        }

        /// <summary>
        /// A new element has been selected within the list.
        /// </summary>
        private void OnObjectSurfaceSelect(ReorderableList list)
        {
            EditorPrefs.SetInt(SelectedObjectSurfaceIndexKey, list.index);
        }
    }
}