/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions.Magic
{
    using Opsive.UltimateCharacterController.Editor.Inspectors;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions;
    using UnityEngine;

    /// <summary>
    /// Draws an inspector for the PhysicsCast CastAction.
    /// </summary>
    [InspectorDrawer(typeof(PhysicsCast))]
    public class PhysicsCastInspectorDrawer : InspectorDrawer
    {
        /// <summary>
        /// Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            InspectorUtility.DrawField(target, "m_Mode");
            InspectorUtility.DrawField(target, "m_UseLookSourcePosition");
            InspectorUtility.DrawField(target, "m_PositionOffset");
            var mode = (target as PhysicsCast).Mode;
            if (mode != PhysicsCast.CastMode.OverlapSphere) {
                InspectorUtility.DrawField(target, "m_Distance");
            }
            if (mode != PhysicsCast.CastMode.Raycast) {
                InspectorUtility.DrawField(target, "m_Radius");
            }
            InspectorUtility.DrawField(target, "m_Layers");
            InspectorUtility.DrawField(target, "m_MaxCollisionCount");
            InspectorUtility.DrawField(target, "m_TriggerInteraction");
        }
    }
}