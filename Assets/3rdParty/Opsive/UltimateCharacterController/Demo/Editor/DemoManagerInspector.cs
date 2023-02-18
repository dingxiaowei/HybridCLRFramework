/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Demo
{
    using Opsive.UltimateCharacterController.Demo;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Shows a custom inspector for the DemoManager component.
    /// </summary>
    [CustomEditor(typeof(DemoManager), true)]
    public class DemoManagerInspector : InspectorBase
    {
        private const string c_EditorPrefsSelectedDemoZoneIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Demo.SelectedDemoZoneIndex";
        private string SelectedViewTypeIndexKey { get { return c_EditorPrefsSelectedDemoZoneIndexKey + "." + target.GetType() + "." + target.name; } }

        private ReorderableList m_DemoZonesList;

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            DrawCharacterField();
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
            if (PropertyFromName("m_PerspectiveSelection").objectReferenceValue == null) {
                EditorGUILayout.PropertyField(PropertyFromName("m_DefaultFirstPersonStart"));
            }
#endif
            if (Foldout("Free Roam")) {
                EditorGUI.indentLevel++;
                DrawFreeRoamFields();
                EditorGUI.indentLevel--;
            }
            if (Foldout("UI")) {
                EditorGUI.indentLevel++;
                DrawUIFields();
                EditorGUI.indentLevel--;
            }

            if (m_DemoZonesList == null) {
                m_DemoZonesList = new ReorderableList(serializedObject, PropertyFromName("m_DemoZones"), true, true, true, true);
                m_DemoZonesList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - 12, EditorGUIUtility.singleLineHeight), "Demo Zones");
                };
                m_DemoZonesList.onSelectCallback += (ReorderableList list) =>
                {
                    EditorPrefs.SetInt(SelectedViewTypeIndexKey, list.index);
                };
                if (EditorPrefs.GetInt(SelectedViewTypeIndexKey, -1) != -1) {
                    m_DemoZonesList.index = EditorPrefs.GetInt(SelectedViewTypeIndexKey, -1);
                }
            }

            m_DemoZonesList.DoLayoutList();
            DrawSelectedDemoZone();

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Draws the inspected character field.
        /// </summary>
        protected virtual void DrawCharacterField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(PropertyFromName("m_Character"));
            if (PropertyFromName("m_Character").objectReferenceValue == null) {
                if (GUILayout.Button("Find", GUILayout.Width(50))) {
                    var characterLocomotion = FindObjectOfType<UltimateCharacterController.Character.UltimateCharacterLocomotion>();
                    if (characterLocomotion != null) {
                        PropertyFromName("m_Character").objectReferenceValue = characterLocomotion.gameObject;
                    }
                }
            } else {
                if (GUILayout.Button("Clear", GUILayout.Width(50))) {
                    PropertyFromName("m_Character").objectReferenceValue = null;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the free roam fields.
        /// </summary>
        protected virtual void DrawFreeRoamFields()
        {
            EditorGUILayout.PropertyField(PropertyFromName("m_FreeRoam"));
            EditorGUILayout.PropertyField(PropertyFromName("m_FreeRoamPickupItemDefinitions"));
            EditorGUILayout.PropertyField(PropertyFromName("m_FreeRoamItemDefinitionAmounts"), true);
        }

        /// <summary>
        /// Draws the UI fields.
        /// </summary>
        protected virtual void DrawUIFields()
        {
            EditorGUILayout.PropertyField(PropertyFromName("m_PerspectiveSelection"));
            EditorGUILayout.PropertyField(PropertyFromName("m_TextPanel"));
            EditorGUILayout.PropertyField(PropertyFromName("m_Header"));
            EditorGUILayout.PropertyField(PropertyFromName("m_Description"));
            EditorGUILayout.PropertyField(PropertyFromName("m_Action"));
            EditorGUILayout.PropertyField(PropertyFromName("m_NextZoneArrow"));
            EditorGUILayout.PropertyField(PropertyFromName("m_PreviousZoneArrow"));
            EditorGUILayout.PropertyField(PropertyFromName("m_NoZoneTitle"));
            EditorGUILayout.LabelField("No Zone Description");
            PropertyFromName("m_NoZoneDescription").stringValue = InspectorUtility.DrawEditorWithoutSelectAll(() =>
                            EditorGUILayout.TextArea(PropertyFromName("m_NoZoneDescription").stringValue, InspectorStyles.WordWrapTextArea));
            EditorGUILayout.PropertyField(PropertyFromName("m_AddOnDemoManager"));
        }

        /// <summary>
        /// Draws the fields for the selected demo zone.
        /// </summary>
        private void DrawSelectedDemoZone()
        {
            var demoZonesProperty = PropertyFromName("m_DemoZones");
            if (m_DemoZonesList.index == -1 || m_DemoZonesList.index >= demoZonesProperty.arraySize) {
                return;
            }

            var demoZoneProperty = demoZonesProperty.GetArrayElementAtIndex(m_DemoZonesList.index);
            EditorGUILayout.LabelField(demoZoneProperty.FindPropertyRelative("m_Header").stringValue + " Demo Zone", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(demoZoneProperty.FindPropertyRelative("m_Header"));
            EditorGUILayout.PropertyField(demoZoneProperty.FindPropertyRelative("m_DemoZoneTrigger"));
            EditorGUILayout.PropertyField(demoZoneProperty.FindPropertyRelative("m_EnableObjects"), true);
            EditorGUILayout.PropertyField(demoZoneProperty.FindPropertyRelative("m_ToggleObjects"), true);
            EditorGUILayout.LabelField("Description");
            demoZoneProperty.FindPropertyRelative("m_Description").stringValue = InspectorUtility.DrawEditorWithoutSelectAll(() => 
                            EditorGUILayout.TextArea(demoZoneProperty.FindPropertyRelative("m_Description").stringValue, InspectorStyles.WordWrapTextArea));
            EditorGUILayout.PropertyField(demoZoneProperty.FindPropertyRelative("m_Action"));
        }
    }
}