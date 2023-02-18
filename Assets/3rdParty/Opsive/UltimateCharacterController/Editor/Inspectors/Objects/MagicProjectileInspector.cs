/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects
{
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using UnityEditor;

    /// <summary>
    /// Custom inspector for the MagicProjectile component.
    /// </summary>
    [CustomEditor(typeof(MagicProjectile), true)]
    public class MagicProjectileInspector : TrajectoryObjectInspector
    {
        /// <summary>
        /// Draws the inspector fields for the object.
        /// </summary>
        protected override void DrawObjectFields()
        {
            base.DrawObjectFields();

            if (Foldout("Magic Projectile")) {
                EditorGUI.indentLevel++;
                var destroyOnCollisionProperty = PropertyFromName("m_DestroyOnCollision");
                EditorGUILayout.PropertyField(destroyOnCollisionProperty);
                if (destroyOnCollisionProperty.boolValue) {
                    EditorGUILayout.PropertyField(PropertyFromName("m_WaitForParticleStop"));
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
