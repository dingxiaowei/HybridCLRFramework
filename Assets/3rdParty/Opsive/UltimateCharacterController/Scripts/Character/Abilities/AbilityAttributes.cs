/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// Attribute which allows the type to be added multiple times.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AllowMultipleAbilityTypes : Attribute
    {
        // Intentionally left blank.
    }

    /// <summary>
    /// Attribute which specifies the default input name for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DefaultInputName : Attribute
    {
        private string m_InputName;
        private int m_Index;
        public string InputName { get { return m_InputName; } }
        public int Index { get { return m_Index; } }
        public DefaultInputName(string inputName) { m_InputName = inputName; }
        public DefaultInputName(string inputName, int index) { m_InputName = inputName; m_Index = index; }
    }

    /// <summary>
    /// Attribute which specifies the default start type for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultStartType : Attribute
    {
        private Ability.AbilityStartType m_StartType;
        public Ability.AbilityStartType StartType { get { return m_StartType; } }
        public DefaultStartType(Ability.AbilityStartType startType) { m_StartType = startType; }
    }

    /// <summary>
    /// Attribute which specifies the default stop type for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultStopType : Attribute
    {
        private Ability.AbilityStopType m_StopType;
        public Ability.AbilityStopType StopType { get { return m_StopType; } }
        public DefaultStopType(Ability.AbilityStopType stopType) { m_StopType = stopType; }
    }

    /// <summary>
    /// Attribute which specifies the default Ability Index for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultAbilityIndex : Attribute
    {
        private int m_Value;
        public int Value { get { return m_Value; } }
        public DefaultAbilityIndex(int value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default Ability Int Data for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultAbilityIntData : Attribute
    {
        private int m_Value;
        public int Value { get { return m_Value; } }
        public DefaultAbilityIntData(int value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default item state index for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultItemStateIndex : Attribute
    {
        private int m_Value;
        public int Value { get { return m_Value; } }
        public DefaultItemStateIndex(int value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default State value for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultState : Attribute
    {
        private string m_Value;
        public string Value { get { return m_Value; } }
        public DefaultState(string value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default Allow Positional Input for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultAllowPositionalInput : Attribute
    {
        private bool m_Value;
        public bool Value { get { return m_Value; } }
        public DefaultAllowPositionalInput(bool value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default Allow Rotational Input for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultAllowRotationalInput : Attribute
    {
        private bool m_Value;
        public bool Value { get { return m_Value; } }
        public DefaultAllowRotationalInput(bool value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default Use Gravity value for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultUseGravity : Attribute
    {
        private Ability.AbilityBoolOverride m_Value;
        public Ability.AbilityBoolOverride Value { get { return m_Value; } }
        public DefaultUseGravity(Ability.AbilityBoolOverride value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default Use Root Motion Position value for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultUseRootMotionPosition : Attribute
    {
        private Ability.AbilityBoolOverride m_Value;
        public Ability.AbilityBoolOverride Value { get { return m_Value; } }
        public DefaultUseRootMotionPosition(Ability.AbilityBoolOverride value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default Use Root Motion Rotation value for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultUseRootMotionRotation : Attribute
    {
        private Ability.AbilityBoolOverride m_Value;
        public Ability.AbilityBoolOverride Value { get { return m_Value; } }
        public DefaultUseRootMotionRotation(Ability.AbilityBoolOverride value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default Detect Horizontal Collisions for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultDetectHorizontalCollisions : Attribute
    {
        private Ability.AbilityBoolOverride m_Value;
        public Ability.AbilityBoolOverride Value { get { return m_Value; } }
        public DefaultDetectHorizontalCollisions(Ability.AbilityBoolOverride value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default Detect Vertical Collisions for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultDetectVerticalCollisions : Attribute
    {
        private Ability.AbilityBoolOverride m_Value;
        public Ability.AbilityBoolOverride Value { get { return m_Value; } }
        public DefaultDetectVerticalCollisions(Ability.AbilityBoolOverride value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default Object Detection Mode for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultObjectDetection : Attribute
    {
        private DetectObjectAbilityBase.ObjectDetectionMode m_Value;
        public DetectObjectAbilityBase.ObjectDetectionMode Value { get { return m_Value; } }
        public DefaultObjectDetection(DetectObjectAbilityBase.ObjectDetectionMode value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default Use Look Direction for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultUseLookDirection : Attribute
    {
        private bool m_Value;
        public bool Value { get { return m_Value; } }
        public DefaultUseLookDirection(bool value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default Cast Offset for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultCastOffset : Attribute
    {
        private Vector3 m_Value;
        public Vector3 Value { get { return m_Value; } }
        public DefaultCastOffset(float x, float y, float z) { m_Value = new Vector3(x, y, z); }
    }

    /// <summary>
    /// Attribute which specifies the default Equipped Slots for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultEquippedSlots : Attribute
    {
        private int m_Value;
        public int Value { get { return m_Value; } }
        public DefaultEquippedSlots(int value) { m_Value = value; }
    }

    /// <summary>
    /// Attribute which specifies the default Reequip Slots for the ability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultReequipSlots : Attribute
    {
        private bool m_Value;
        public bool Value { get { return m_Value; } }
        public DefaultReequipSlots(bool value) { m_Value = value; }
    }
}