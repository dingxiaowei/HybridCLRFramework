/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility
{
    /// <summary>
    /// Static class defining information about the asset.
    /// </summary>
    public static class AssetInfo
    {
        private static string s_Version = "2.1.7";
        public static string Version { get { return s_Version; } }

        public static string Name
        {
            get
            {
#pragma warning disable 0162
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_SHOOTER && ULTIMATE_CHARACTER_CONTROLLER_MELEE
                return "Ultimate Character Controller";
#endif
#if FIRST_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_SHOOTER && ULTIMATE_CHARACTER_CONTROLLER_MELEE
                return "First Person Controller";
#endif
#if THIRD_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_SHOOTER && ULTIMATE_CHARACTER_CONTROLLER_MELEE
                return "Third Person Controller";
#endif
#if FIRST_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
                return "Ultimate First Person Shooter";
#endif
#if FIRST_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_MELEE
                return "Ultimate First Person Melee";
#endif
#if THIRD_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
                return "Ultimate Third Person Shooter";
#endif
#if THIRD_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_MELEE
                return "Ultimate Third Person Melee";
#endif
                return string.Empty;
#pragma warning restore 0162
            }
        }
    }
}