/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------


namespace Opsive.UltimateCharacterController.Editor.Utility
{
    using System.Collections.Generic;
    using UnityEditor;

    /// <summary>
    /// Updates the Unity input manager with the correct button bindings.
    /// </summary>
    public class UnityInputBuilder
    {
        /// <summary>
        /// The elements axis type within the InputManager.
        /// </summary>
        public enum AxisType
        {
            KeyMouseButton, Mouse, Joystick
        }
        /// <summary>
        /// The element's axis number within the InputManager.
        /// </summary>
        public enum AxisNumber
        {
            X, Y, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Eleven, Twelve, Thirteen, Fourteen, Fifteen, Sixteen, Seventeen, Eighteen, Nineteen, Twenty
        }

        private static Dictionary<string, int> s_FoundAxes;
        public static Dictionary<string, int> FoundAxes { get { return s_FoundAxes; } set { s_FoundAxes = value; } }

        /// <summary>
        /// Update the Input Manager to add all of the correct controls.
        /// </summary>
        public static void UpdateInputManager()
        {
            var serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            var axesProperty = serializedObject.FindProperty("m_Axes");

            // Unity defined axis:
            AddInputAxis(axesProperty, "Horizontal", "left", "right", "a", "d", 1000, 0.001f, 3, true, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Vertical", "down", "up", "s", "w", 1000, 0.001f, 3, true, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Fire1", "", "left ctrl", "", "mouse 0", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Fire2", "", "", "", "mouse 1", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Fire3", "", "left shift", "", "mouse 2", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Jump", "", "space", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Mouse X", "", "", "", "", 0, 0, 0.1f, false, false, AxisType.Mouse, AxisNumber.X);
            AddInputAxis(axesProperty, "Mouse Y", "", "", "", "", 0, 0, 0.1f, false, false, AxisType.Mouse, AxisNumber.Y);
            AddInputAxis(axesProperty, "Mouse ScrollWheel", "", "", "", "", 0, 0, 0.1f, false, false, AxisType.Mouse, AxisNumber.Three);
            AddInputAxis(axesProperty, "Horizontal", "", "", "", "", 1000, 0.19f, 1, false, false, AxisType.Joystick, AxisNumber.X);
            AddInputAxis(axesProperty, "Vertical", "", "", "", "", 1000, 0.19f, 1, false, true, AxisType.Joystick, AxisNumber.Y);
            AddInputAxis(axesProperty, "Fire1", "", "", "", "", 1000, 0.001f, 1000, false, false, AxisType.Joystick, AxisNumber.Ten);
            AddInputAxis(axesProperty, "Fire2", "", "", "", "", 1000, 0.001f, 1000, false, false, AxisType.Joystick, AxisNumber.Nine);
            AddInputAxis(axesProperty, "Fire3", "", "joystick button 2", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Jump", "", "joystick button 0", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Mouse X", "", "", "", "", 0, 0.19f, 1, false, false, AxisType.Joystick, AxisNumber.Four);
            AddInputAxis(axesProperty, "Mouse Y", "", "", "", "", 0, 0.19f, 1, false, true, AxisType.Joystick, AxisNumber.Five);

            // New axis:
            AddInputAxis(axesProperty, "Alt Horizontal", "q", "e", "", "", 1000, 0.19f, 3, true, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Change Speeds", "", "left shift", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Crouch", "", "c", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Crouch", "", "joystick button 9", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Toggle Perspective", "", "v", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Toggle Perspective", "", "joystick button 8", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Toggle Item Equip", "", "t", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Next Item", "", "e", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Next Item", "", "joystick button 3", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Previous Item", "", "q", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip First Item", "", "1", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Second Item", "", "2", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Third Item", "", "3", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Fourth Item", "", "4", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Fifth Item", "", "5", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Sixth Item", "", "6", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Seventh Item", "", "7", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Eighth Item", "", "8", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Ninth Item", "", "9", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Equip Tenth Item", "", "0", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Reload", "", "r", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Reload", "", "joystick button 2", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Drop", "", "y", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Grenade", "", "g", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Grenade", "", "joystick button 5", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Lean", "x", "z", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "SecondaryUse", "", "b", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "SecondaryUse", "", "joystick button 4", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Action", "", "f", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);
            AddInputAxis(axesProperty, "Action", "", "joystick button 1", "", "", 1000, 0.001f, 1000, false, false, AxisType.KeyMouseButton, AxisNumber.X);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Adds a new axis to the InputManager.
        /// </summary>
        /// <param name="axesProperty">The array of all of the axes.</param>
        /// <param name="name">The name of the new axis.</param>
        /// <param name="negativeButton">The name of the negative button of the new axis.</param>
        /// <param name="positiveButton">The name of the positive button of the new axis.</param>
        /// <param name="altNegativeButton">The name of the alternative negative button of the new axis.</param>
        /// <param name="altPositiveButton">The name of the alternative positive button of the new axis.</param>
        /// <param name="sensitivity">The sensitivity of the new axis.</param>
        /// <param name="gravity">The gravity of the new axis.</param>
        /// <param name="dead">The dead value of the new axis.</param>
        /// <param name="snap">Does the new axis snap?</param>
        /// <param name="axisType">The type of axis to add.</param>
        public static void AddInputAxis(SerializedProperty axesProperty, string name, string negativeButton, string positiveButton,
                                string altNegativeButton, string altPositiveButton, float gravity, float dead, float sensitivity, bool snap, bool invert, AxisType axisType, AxisNumber axisNumber)
        {
            var property = FindAxisProperty(axesProperty, name);
            property.FindPropertyRelative("m_Name").stringValue = name;
            property.FindPropertyRelative("negativeButton").stringValue = negativeButton;
            property.FindPropertyRelative("positiveButton").stringValue = positiveButton;
            property.FindPropertyRelative("altNegativeButton").stringValue = altNegativeButton;
            property.FindPropertyRelative("altPositiveButton").stringValue = altPositiveButton;
            property.FindPropertyRelative("gravity").floatValue = gravity;
            property.FindPropertyRelative("dead").floatValue = dead;
            property.FindPropertyRelative("sensitivity").floatValue = sensitivity;
            property.FindPropertyRelative("snap").boolValue = snap;
            property.FindPropertyRelative("invert").boolValue = invert;
            property.FindPropertyRelative("type").intValue = (int)axisType;
            property.FindPropertyRelative("axis").intValue = (int)axisNumber;
        }

        /// <summary>
        /// Searches for a property with the given name and axis type within the axes property array. If no property is found then a new one will be created.
        /// </summary>
        /// <param name="axesProperty">The array to search through.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns></returns>
        private static SerializedProperty FindAxisProperty(SerializedProperty axesProperty, string name)
        {
            SerializedProperty foundProperty = null;
            // As new axes are being added make sure a previous axis is not overwritten because the name matches.
            var existingCount = 0;
            if (s_FoundAxes == null) {
                s_FoundAxes = new Dictionary<string, int>();
            }
            s_FoundAxes.TryGetValue(name, out existingCount);
            var localCount = 0;
            for (int i = 0; i < axesProperty.arraySize; ++i) {
                var property = axesProperty.GetArrayElementAtIndex(i);
                if (property.FindPropertyRelative("m_Name").stringValue.Equals(name)) {
                    if (localCount == existingCount) {
                        foundProperty = property;
                        break;
                    }
                    localCount++;
                }
            }
            if (existingCount == 0) {
                s_FoundAxes.Add(name, 1);
            } else {
                s_FoundAxes[name] = existingCount + 1;
            }

            // If no property was found then create a new one.
            if (foundProperty == null) {
                axesProperty.InsertArrayElementAtIndex(axesProperty.arraySize);
                foundProperty = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
            }

            return foundProperty;
        }
    }
}