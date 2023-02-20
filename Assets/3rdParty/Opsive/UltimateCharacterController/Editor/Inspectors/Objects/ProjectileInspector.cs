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
    /// Custom inspector for the Projectile component.
    /// </summary>
    [CustomEditor(typeof(Projectile), true)]
    public class ProjectileInspector : DestructibleInspector
    {
        /// <summary>
        /// Draws the inspector fields for the object.
        /// </summary>
        protected override void DrawObjectFields()
        {
            base.DrawObjectFields();

            if (Foldout("Projectile")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_Lifespan"));
                EditorGUI.indentLevel--;
            }
        }
    }
}
