/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEditor;
using Opsive.UltimateCharacterController.Objects;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects
{
    /// <summary>
    /// Custom inspector for the Grenade component.
    /// </summary>
    [CustomEditor(typeof(Grenade), true)]
    public class GrenadeInspector : DestructibleInspector
    {
        /// <summary>
        /// Draws the inspector fields for the object.
        /// </summary>
        protected override void DrawObjectFields()
        {
            base.DrawObjectFields();

            if (Foldout("Grenade")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_Lifespan"));
                EditorGUILayout.PropertyField(PropertyFromName("m_Pin"));
                EditorGUI.indentLevel--;
            }
        }
    }
}
