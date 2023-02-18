/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Utility
{
    using Opsive.UltimateCharacterController.Traits;
    using Opsive.UltimateCharacterController.Motion;
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Utility class for the Ultimate Character Controller inspectors.
    /// </summary>
    public static class InspectorUtility
    {
        private const int c_IndentWidth = 15;
        public static int IndentWidth { get { return c_IndentWidth; } }
        private const string c_EditorPrefsFoldoutKey = "Opsive.UltimateCharacterController.Editor.Foldout.";

        private static Dictionary<string, string> s_CamelCaseSplit = new Dictionary<string, string>();
        private static Regex s_CamelCaseRegex = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

        private static Dictionary<string, FieldInfo> s_FieldNameMap = new Dictionary<string, FieldInfo>();
        private static Dictionary<object, bool> s_FoldoutValueMap = new Dictionary<object, bool>();

        public static Dictionary<Type, string[]> s_TypeNameMap = new Dictionary<Type, string[]>();
        public static Dictionary<Type, int[]> s_TypeIndexMap = new Dictionary<Type, int[]>();
        public static Dictionary<Type, Type[]> s_TypeTypeMap = new Dictionary<Type, Type[]>();

        /// <summary>
        /// Places a space before each capital letter in a word.
        /// </summary>
        public static string SplitCamelCase(string s)
        {
            if (s.Equals(""))
                return s;
            if (s_CamelCaseSplit.ContainsKey(s)) {
                return s_CamelCaseSplit[s];
            }

            var origString = s;
            // Remove the "m_" and '_' prefix.
            if (s.Length > 2 && s.Substring(0, 2).CompareTo("m_") == 0) {
                s = s.Substring(2);
            } else if (s.Length > 1 && s[0].CompareTo('_') == 0) {
                s = s.Substring(1);
            }
            s = s_CamelCaseRegex.Replace(s, " ");
            s = s.Replace("_", " ");
            s = (char.ToUpper(s[0]) + s.Substring(1)).Trim();
            s_CamelCaseSplit.Add(origString, s);
            return s;
        }

        /// <summary>
        /// Returns a string which shows the namespace with a more friendly prefix.
        /// </summary>
        public static string DisplayTypeName(Type type, bool friendlyNamespacePrefix)
        {
            var name = SplitCamelCase(type.Name);
            // Show a friendly version of the full path.
            if (friendlyNamespacePrefix) {
                name = UltimateCharacterController.Utility.UnityEngineUtility.GetFriendlyName(type.FullName, name);
            }
            return name;
        }

        /// <summary>
        /// Draws the inspector for the AnimationEventTrigger class.
        /// </summary>
        /// <param name="obj">The object that is being drawn.</param>
        /// <param name="name">The name of the drawn field.</param>
        /// <param name="animationEventTriggerProperty">The property of the AnimationEventTrigger.</param>
        public static void DrawAnimationEventTrigger(object obj, string name, SerializedProperty animationEventTriggerProperty)
        {
            if (Foldout(obj, new GUIContent(name, GetFieldTooltip(obj, animationEventTriggerProperty.name)))) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(animationEventTriggerProperty.FindPropertyRelative("m_WaitForAnimationEvent"));
                EditorGUILayout.PropertyField(animationEventTriggerProperty.FindPropertyRelative("m_Duration"));
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Draws a EditorGUI foldout, saving the foldout expand/collapse bool within a EditorPref.
        /// </summary>
        /// <param name="obj">The object that is being drawn beneath the foldout.</param>
        /// <param name="name">The name of the foldout.</param>
        /// <returns>True if the foldout is expanded.</returns>
        public static bool Foldout(object obj, string name)
        {
            return Foldout(obj, new GUIContent(name), true, string.Empty);
        }

        /// <summary>
        /// Draws a EditorGUI foldout, saving the foldout expand/collapse bool within a EditorPref.
        /// </summary>
        /// <param name="obj">The object that is being drawn beneath the foldout.</param>
        /// <param name="guiContent">The GUIContent of the foldout.</param>
        /// <returns>True if the foldout is expanded.</returns>
        public static bool Foldout(object obj, GUIContent guiContent)
        {
            return Foldout(obj, guiContent, true, string.Empty);
        }

        /// <summary>
        /// Draws a EditorGUI foldout, saving the foldout expand/collapse bool within a EditorPref.
        /// </summary>
        /// <param name="obj">The object that is being drawn beneath the foldout.</param>
        /// <param name="guiContent">The GUIContent of the foldout.</param>
        /// <param name="defaultExpanded">The default value if the foldout is expanded.</param>
        /// <returns>True if the foldout is expanded.</returns>
        public static bool Foldout(object obj, GUIContent guiContent, bool defaultExpanded)
        {
            return Foldout(obj, guiContent, defaultExpanded, string.Empty);
        }

        /// <summary>
        /// Draws a EditorGUI foldout, saving the foldout expand/collapse bool within a EditorPref.
        /// </summary>
        /// <param name="obj">The object that is being drawn beneath the foldout.</param>
        /// <param name="guiContent">The GUIContent of the foldout.</param>
        /// <param name="defaultExpanded">The default value if the foldout is expanded.</param>
        /// <param name="identifyingString">A string that can be used to help identify the foldout key.</param>
        /// <returns>True if the foldout is expanded.</returns>
        public static bool Foldout(object obj, GUIContent guiContent, bool defaultExpanded, string identifyingString)
        {
            if (obj == null) {
                return false;
            }
            var key = c_EditorPrefsFoldoutKey + "." + obj.GetType() + (obj is MonoBehaviour ? ("." + (obj as MonoBehaviour).name) : string.Empty) + "." + identifyingString + "." + guiContent.text;
            bool prevFoldout;
            if (!s_FoldoutValueMap.TryGetValue(key, out prevFoldout)) {
                prevFoldout = EditorPrefs.GetBool(key, defaultExpanded);
                s_FoldoutValueMap.Add(key, prevFoldout);
            }
            var foldout = EditorGUILayout.Foldout(prevFoldout, guiContent);
            if (foldout != prevFoldout) {
                EditorPrefs.SetBool(key, foldout);
                s_FoldoutValueMap[key] = foldout;
            }
            return foldout;
        }

        /// <summary>
        /// Draws a slider which has a min and max label beside it.
        /// </summary>
        /// <param name="minValue">The current minimum value.</param>
        /// <param name="maxValue">The current maximum value.</param>
        /// <param name="minLimit">The minimum value that can be selected.</param>
        /// <param name="maxLimit">The maximum value that can be selected.</param>
        /// <param name="guiContent">The guiContent of the slider.</param>
        public static void MinMaxSlider(ref float minValue, ref float maxValue, float minLimit, float maxLimit, GUIContent guiContent)
        {
            EditorGUILayout.BeginHorizontal();
            minValue = EditorGUILayout.FloatField(guiContent, minValue);
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
            maxValue = EditorGUILayout.FloatField(maxValue);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Prevents the label from being too far away from the text.
        /// </summary>
        /// <param name="rect">The rectangle of the textfield.</param>
        /// <param name="label">The textfield label.</param>
        /// <param name="text">The textfield value.</param>
        /// <param name="widthAddition">Any additional width to separate the label and the control.</param>
        /// <returns>The new text.</returns>
        public static string ClampTextField(Rect rect, string label, string text, int widthAddition)
        {
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(label));
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = textDimensions.x + widthAddition;
            text = EditorGUI.TextField(rect, label, text);
            EditorGUIUtility.labelWidth = prevLabelWidth;
            return text;
        }

        /// <summary>
        /// Prevents the label from being too far away from the int field.
        /// </summary>
        /// <param name="rect">The rectangle of the textfield.</param>
        /// <param name="label">The textfield label.</param>
        /// <param name="value">The int value.</param>
        /// <param name="widthAddition">Any additional width to separate the label and the control.</param>
        /// <returns>The new value.</returns>
        public static int ClampIntField(Rect rect, string label, int value, int widthAddition)
        {
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(label));
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = textDimensions.x + widthAddition;
            value = EditorGUI.IntField(rect, label, value);
            EditorGUIUtility.labelWidth = prevLabelWidth;
            return value;
        }

        /// <summary>
        /// Prevents the label from being too far away from the float field.
        /// </summary>
        /// <param name="rect">The rectangle of the textfield.</param>
        /// <param name="label">The textfield label.</param>
        /// <param name="value">The float value.</param>
        /// <param name="widthAddition">Any additional width to separate the label and the control.</param>
        /// <returns>The new value.</returns>
        public static float ClampFloatField(Rect rect, string label, float value, int widthAddition)
        {
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(label));
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = textDimensions.x + widthAddition;
            value = EditorGUI.FloatField(rect, label, value);
            EditorGUIUtility.labelWidth = prevLabelWidth;
            return value;
        }

        /// <summary>
        /// Prevents the label from being too far away from the mask.
        /// </summary>
        /// <param name="rect">The rectangle of the mask.</param>
        /// <param name="label">The mask label.</param>
        /// <param name="mask">The mask selection.</param>
        /// <param name="values">The mask string values.</param>
        /// <param name="widthAddition">Any additional width to separate the label and the control.</param>
        /// <returns>The new value.</returns>
        public static int ClampMaskField(Rect rect, string label, int mask, string[] values, int widthAddition)
        {
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(label));
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = textDimensions.x + widthAddition;
            mask = EditorGUI.MaskField(rect, label, mask, values);
            EditorGUIUtility.labelWidth = prevLabelWidth;
            return mask;
        }

        /// <summary>
        /// Prevents the label from being too far away from the popup.
        /// </summary>
        /// <param name="rect">The rectangle of the mask.</param>
        /// <param name="label">The mask label.</param>
        /// <param name="index">The popup selection.</param>
        /// <param name="values">The mask string values.</param>
        /// <param name="widthAddition">Any additional width to separate the label and the control.</param>
        /// <returns>The new value.</returns>
        public static int ClampPopupField(Rect rect, string label, int index, string[] values, int widthAddition)
        {
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(label));
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = textDimensions.x + widthAddition;
            index = EditorGUI.Popup(rect, label, index, values);
            EditorGUIUtility.labelWidth = prevLabelWidth;
            return index;
        }

        /// <summary>
        /// Draws a popup with the names for the specified type.
        /// </summary>
        /// <param name="type">The type of object to draw the popup of.</param>
        /// <param name="value">The current value of the popup.</param>
        /// <param name="label">The lable for the popup.</param>
        /// <param name="allowNone">Can an empty value be selected?</param>
        /// <returns>The selected popup value.</returns>
        public static string DrawTypePopup(Type type, string value, string label, bool allowNone)
        {
            Type[] types;
            string[] names;
            int[] indicies;
            if (!s_TypeNameMap.TryGetValue(type, out names)) {
                // The type hasn't been found yet. Find and cache the values.
                var typesList = new List<Type>();
                var nameList = new List<string>();
                var indiciesList = new List<int>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; ++i) {
                    var assemblyTypes = assemblies[i].GetTypes();
                    for (int j = 0; j < assemblyTypes.Length; ++j) {
                        if (type.IsAssignableFrom(assemblyTypes[j]) && !assemblyTypes[j].IsAbstract) {
                            typesList.Add(assemblyTypes[j]);
                            nameList.Add(DisplayTypeName(assemblyTypes[j], true));
                            indiciesList.Add(indiciesList.Count);
                        }
                    }
                }

                types = typesList.ToArray();
                names = nameList.ToArray();
                indicies = indiciesList.ToArray();
                Array.Sort(names, indicies);

                s_TypeTypeMap.Add(type, types);
                s_TypeNameMap.Add(type, names);
                s_TypeIndexMap.Add(type, indicies);
            } else {
                types = s_TypeTypeMap[type];
                indicies = s_TypeIndexMap[type];
            }

            // Find the index of the type if it has already been specified.
            var index = 0;
            if (!string.IsNullOrEmpty(value)) {
                for (int i = 0; i < types.Length; ++i) {
                    if (types[indicies[i]].FullName.Equals(value)) {
                        index = i + (allowNone ? 1 : 0);
                        break;
                    }
                }
            }

            if (allowNone) {
                var nameList = new List<string>(names);
                nameList.Insert(0, "(none)");
                names = nameList.ToArray();
            }
            // Gets a new type.
            index = EditorGUILayout.Popup(label, index, names);
            if (allowNone && index == 0) {
                value = string.Empty;
            } else {
                if (allowNone && index > 0) {
                    index--; // Remove the (none).
                }
                value = types[indicies[index]].FullName;
            }
            return value;
        }

        /// <summary>
        /// Synchronizes the number of elements within the state array with the number of elements within the property array.
        /// </summary>
        public static void SynchronizePropertyCount(UltimateCharacterController.StateSystem.State[] states, SerializedProperty serializedProperty)
        {
            var serializedCount = serializedProperty.arraySize;
            if (serializedCount > states.Length) {
                for (int i = serializedCount - 1; i >= states.Length; --i) {
                    serializedProperty.DeleteArrayElementAtIndex(i);
                }
            } else if (serializedCount < states.Length) {
                for (int i = serializedCount; i < states.Length; ++i) {
                    serializedProperty.InsertArrayElementAtIndex(i);
                    serializedProperty.GetArrayElementAtIndex(i).intValue = i;
                }
            }
        }

        /// <summary>
        /// Draws the object.
        /// </summary>
        public static void DrawObject(object obj, bool drawHeader, bool friendlyNamespacePrefix, UnityEngine.Object target, bool drawNoFieldsNotice, Action changeCallback)
        {
            if (obj == null) {
                return;
            }

            if (drawHeader) {
                EditorGUILayout.LabelField(DisplayTypeName(obj.GetType(), friendlyNamespacePrefix), EditorStyles.boldLabel);
            }

            EditorGUI.BeginChangeCheck();
            var inspectorDrawer = InspectorDrawerUtility.InspectorDrawerForType(obj.GetType());
            if (inspectorDrawer != null) {
                inspectorDrawer.OnInspectorGUI(obj, target);
            } else {
                ObjectInspector.DrawFields(obj, drawNoFieldsNotice);
            }

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                if (changeCallback != null) {
                    changeCallback();
                }
            }
        }

        /// <summary>
        /// Draws a field belonging to the object with the specified field name.
        /// </summary>
        /// <param name="obj">The object being drawn.</param>
        /// <param name="name">The name of the field.</param>
        public static void DrawField(object obj, string name)
        {
            var field = GetField(obj, name);
            if (field != null) {
                try {
                    var prevValue = field.GetValue(obj);
                    var value = ObjectInspector.DrawObject(new GUIContent(SplitCamelCase(name), GetFieldTooltip(field)), field.FieldType, prevValue, name, 0, null, null, field, true);
                    if (prevValue != value && GUI.changed) {
                        field.SetValue(obj, value);
                    }
                } catch (Exception /*e*/) { }
            } else {
                Debug.LogError("Error: Unable to find a field with name " + name + " on object " + obj);
            }
        }

        /// <summary>
        /// Draws a slider belonging to the object with the specified field name.
        /// </summary>
        /// <param name="obj">The object being drawn.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="minValue">The minimum slider value.</param>
        /// <param name="maxValue">The maximum slider value.</param>
        public static void DrawFieldSlider(object obj, string name, float minValue, float maxValue)
        {
            var sliderValue = GetFieldValue<float>(obj, name);
            var value = EditorGUILayout.Slider(new GUIContent(InspectorUtility.SplitCamelCase(name), GetFieldTooltip(obj, name)), sliderValue, minValue, maxValue);
            if (sliderValue != value) {
                SetFieldValue(obj, name, value);
            }
        }

        /// <summary>
        /// Draws an int slider belonging to the object with the specified field name.
        /// </summary>
        /// <param name="obj">The object being drawn.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="minValue">The minimum slider value.</param>
        /// <param name="maxValue">The maximum slider value.</param>
        public static void DrawFieldIntSlider(object obj, string name, int minValue, int maxValue)
        {
            var sliderValue = GetFieldValue<int>(obj, name);
            var value = EditorGUILayout.IntSlider(new GUIContent(InspectorUtility.SplitCamelCase(name), GetFieldTooltip(obj, name)), sliderValue, minValue, maxValue);
            if (sliderValue != value) {
                SetFieldValue(obj, name, value);
            }
        }

        /// <summary>
        /// Draws the fields for the spring object.
        /// </summary>
        /// <param name="owner">The object which contains the spring.</param>
        /// <param name="foldoutName">The name of the springs foldout.</param>
        /// <param name="fieldName">The name of the spring's field.</param>
        public static void DrawSpring(object owner, string foldoutName, string fieldName)
        {
            var spring = GetFieldValue<Spring>(owner, fieldName);
            if (Foldout(owner, new GUIContent(foldoutName, GetFieldTooltip(GetField(owner, fieldName))))) {
                EditorGUI.indentLevel++;
                DrawFieldSlider(spring, "m_Stiffness", 0, 1);
                DrawFieldSlider(spring, "m_Damping", 0, 1);
                DrawField(spring, "m_VelocityFadeInLength");
                DrawField(spring, "m_MaxSoftForceFrames");
                DrawField(spring, "m_MinVelocity");
                DrawField(spring, "m_MaxVelocity");
                DrawField(spring, "m_MinValue");
                DrawField(spring, "m_MaxValue");
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Draws the Attribute fields.
        /// </summary>
        /// <param name="attributeManager">The AttributeManager that the target uses.</param>
        /// <param name="attributeName">The name of the selected attribute.</param>
        /// <param name="fieldName">The name of the field that is being drawn.</param>
        /// <returns>The name of the attribute.</returns>
        public static string DrawAttribute(AttributeManager attributeManager, string attributeName, string fieldName)
        {
            if (attributeManager != null) {
                var attributeNames = new string[attributeManager.Attributes.Length + 1];
                attributeNames[0] = "(None)";
                var attributeIndex = 0;
                for (int i = 0; i < attributeManager.Attributes.Length; ++i) {
                    attributeNames[i + 1] = attributeManager.Attributes[i].Name;
                    if (attributeName == attributeNames[i + 1]) {
                        attributeIndex = i + 1;
                    }
                }
                var selectedAttributeIndex = EditorGUILayout.Popup(fieldName, attributeIndex, attributeNames);
                if (attributeIndex != selectedAttributeIndex) {
                    attributeName = (selectedAttributeIndex == 0 ? string.Empty : attributeManager.Attributes[selectedAttributeIndex - 1].Name);
                }
            } else {
                attributeName = EditorGUILayout.TextField(fieldName, attributeName);
            }
            return attributeName;
        }

        /// <summary>
        /// Draws the AttributeModifier fields.
        /// </summary>
        /// <param name="attributeManager">The AttributeManager that the target uses.</param>
        /// <param name="attributeModifier">The AttributeModifier that should be drawn.</param>
        /// <param name="fieldName">The name of the attribute field.</param>
        public static void DrawAttributeModifier(AttributeManager attributeManager, AttributeModifier attributeModifier, string fieldName)
        {
            var attributeName = DrawAttribute(attributeManager, attributeModifier.AttributeName, fieldName);
            if (attributeName != attributeModifier.AttributeName) {
                attributeModifier.AttributeName = attributeName;
                GUI.changed = true;
            }
            if (!string.IsNullOrEmpty(attributeModifier.AttributeName)) {
                EditorGUI.indentLevel++;
                attributeModifier.ValueChange = EditorGUILayout.FloatField("Value Change", attributeModifier.ValueChange);
                attributeModifier.ChangeUpdateValue = EditorGUILayout.Toggle(new GUIContent("Change Update Value", "Should a new update value be set?"), attributeModifier.ChangeUpdateValue);
                if (attributeModifier.ChangeUpdateValue) {
                    attributeModifier.AutoUpdateValueType = (UltimateCharacterController.Traits.Attribute.AutoUpdateValue)EditorGUILayout.EnumPopup("Auto Update Type", attributeModifier.AutoUpdateValueType);
                    if (attributeModifier.AutoUpdateValueType != UltimateCharacterController.Traits.Attribute.AutoUpdateValue.None) {
                        attributeModifier.AutoUpdateStartDelay = EditorGUILayout.FloatField("Start Delay", attributeModifier.AutoUpdateStartDelay);
                        attributeModifier.AutoUpdateInterval = EditorGUILayout.FloatField("Interval", attributeModifier.AutoUpdateInterval);
                        attributeModifier.AutoUpdateAmount = EditorGUILayout.FloatField("Amount", attributeModifier.AutoUpdateAmount);
                        attributeModifier.AutoUpdateDuration = EditorGUILayout.FloatField("Duration", attributeModifier.AutoUpdateDuration);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Returns the Tooltip attribute for the given field, if it exists.
        /// </summary>
        /// <param name="field">The field to get the Tooltip attribute of.</param>
        /// <returns>The Tooltip for the given field. If it does not exist then null is returned.</returns>
        public static TooltipAttribute GetTooltipAttribute(FieldInfo field)
        {
            var tooltipAttributes = field.GetCustomAttributes(typeof(TooltipAttribute), false) as TooltipAttribute[];
            if (tooltipAttributes.Length > 0) {
                return tooltipAttributes[0];
            }
            return null;
        }

        /// <summary>
        /// Returns the field belonging to the object with the specified field name.
        /// </summary>
        /// <param name="obj">The object being retrieved.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The field belongning to the object with the specified field name. Can be null.</returns>
        public static FieldInfo GetField(object obj, string name)
        {
            FieldInfo field;
            if (!s_FieldNameMap.TryGetValue(name, out field)) {
                field = obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                s_FieldNameMap.Add(name, field);
            }
            // The type may not match if the same field name exists within multiple objects.
            if (field != null && obj != null && field.DeclaringType != obj.GetType()) {
                field = obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                s_FieldNameMap[name] = field;
            }
            return field;
        }

        /// <summary>
        /// Returns the tooltip attached to the specified field.
        /// </summary>
        /// <param name="field">The field whose tooltip should be retrieved.</param>
        /// <returns>The found tooltip. Can be null.</returns>
        public static string GetFieldTooltip(FieldInfo field)
        {
            var attributes = field.GetCustomAttributes(typeof(TooltipAttribute), false) as TooltipAttribute[];
            if (attributes != null && attributes.Length > 0) {
                return attributes[0].tooltip;
            }
            return System.String.Empty;
        }

        /// <summary>
        /// Returns the field value.
        /// </summary>
        /// <param name="obj">The object being retrieved.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        public static T GetFieldValue<T>(object obj, string name)
        {
            var field = GetField(obj, name);
            if (field != null) {
                return (T)field.GetValue(obj);
            }
            return default(T);
        }

        /// <summary>
        /// Returns the tooltip attached to the specified field name.
        /// </summary>
        /// <param name="obj">The object whose tooltip is being retrieved.</param>
        /// <param name="name">The field name whose tooltip should be retrieved.</param>
        /// <returns>The found tooltip. Can be null.</returns>
        public static string GetFieldTooltip(object obj, string name)
        {
            var field = GetField(obj, name);
            if (field != null) {
                return InspectorUtility.GetFieldTooltip(field);
            }
            return System.String.Empty;
        }

        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="obj">The object being retrieved.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value that the field should be set to.</param>
        public static void SetFieldValue(object obj, string name, object value)
        {
            var field = GetField(obj, name);
            if (field != null) {
                field.SetValue(obj, value);
            }
        }

        /// <summary>
        /// Draws the UnityEvent property field with the correct indentation.
        /// </summary>
        /// <param name="property">The UnityEvent property.</param>
        public static void UnityEventPropertyField(SerializedProperty property)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * c_IndentWidth);
            EditorGUILayout.PropertyField(property);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Returns a highlight color for wireframe that contrasts well against the solid color.
        /// </summary>
        /// <param name="color">The color to contract against.</param>
        /// <returns>A highlight color for wireframe that contrasts well against the solid color.</returns>
        public static Color GetContrastColor(Color color)
        {
            var max = 0f;
            if ((color.r > max)) max = color.r;
            if ((color.g > max)) max = color.g;
            if ((color.b > max)) max = color.b;

            return new Color(((color.r == max) ? color.r : color.r * 0.75f), ((color.g == max) ? color.g : color.g * 0.75f),
                             ((color.b == max) ? color.b : color.b * 0.75f), Mathf.Clamp01(color.a * 2));
        }

        /// <summary>
        /// Unity will immediately select all the text in a textfield and you can't clear the selection. This workaround will allow the text to be
        /// selected again. This solution is based off of the post at: 
        /// https://stackoverflow.com/questions/44097608/how-can-i-stop-immediate-gui-from-selecting-all-text-on-click.
        /// </summary>
        /// <param name="guiCall">The editor gui method that should be called.</param>
        /// <returns>The value of the gui call.</returns>
        public static T DrawEditorWithoutSelectAll<T>(System.Func<T> guiCall)
        {
            var preventSelection = (Event.current.type == EventType.MouseDown);
            var oldCursorColor = GUI.skin.settings.cursorColor;

            if (preventSelection) {
                GUI.skin.settings.cursorColor = new Color(0, 0, 0, 0);
            }

            var value = guiCall();

            if (preventSelection) {
                GUI.skin.settings.cursorColor = oldCursorColor;
            }

            return value;
        }

        /// <summary>
        /// Records the object to the Undo manager and sets the dirty state.
        /// </summary>
        /// <param name="obj">The object that has been changed.</param>
        /// <param name="undoName">The name of the operation that can be undone.</param>
        public static void RecordUndoDirtyObject(UnityEngine.Object obj, string undoName)
        {
            if (obj == null || Application.isPlaying) {
                return;
            }

            Undo.RecordObject(obj, undoName);

            // The object should also be marked dirty so the changes persist.
            SetDirty(obj);
        }

        /// <summary>
        /// Flags the object as dirty.
        /// </summary>
        /// <param name="obj">The object that was changed.</param>
        public static void SetDirty(UnityEngine.Object obj)
        {
            if (obj == null || Application.isPlaying) {
                return;
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
            new SerializedObject(obj).ApplyModifiedProperties();
            if (obj is Component) {
                EditorSceneManager.MarkSceneDirty((obj as Component).gameObject.scene);
            } else if (obj is GameObject) {
                EditorSceneManager.MarkSceneDirty((obj as GameObject).scene);
            } else if (!EditorUtility.IsPersistent(obj)) {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            } else {
                EditorUtility.SetDirty(obj);
            }
        }

        /// <summary>
        /// Returns the active path that the save file window should start at.
        /// </summary>
        /// <returns>The name of the path to save the file in.</returns>
        public static string GetSaveFilePath()
        {
            var selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(selectedPath)) {
                selectedPath = "Assets";
            }

            return selectedPath;
        }
    }
}