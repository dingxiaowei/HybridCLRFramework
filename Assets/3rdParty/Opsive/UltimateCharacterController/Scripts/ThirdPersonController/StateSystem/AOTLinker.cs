/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

#if NET_4_6 || UNITY_2018_3_OR_NEWER || UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID || UNITY_WII || UNITY_WIIU || UNITY_SWITCH || UNITY_PS3 || UNITY_PS4 || UNITY_XBOXONE || UNITY_WSA
using UnityEngine;
using System;
using Opsive.UltimateCharacterController.StateSystem;

namespace Opsive.UltimateCharacterController.ThirdPersonController.StateSystem
{
    // See Opsive.UltimateCharacterController.StateSystem.AOTLinker for an explanation of this class.
    public class AOTLinker : MonoBehaviour
    {
        public void Linker()
        {
#pragma warning disable 0219
#if THIRD_PERSON_CONTROLLER
            var objectDeathVisiblityGenericDelegate = new Preset.GenericDelegate<Character.PerspectiveMonitor.ObjectDeathVisiblity>();
            var objectDeathVisiblityFuncDelegate = new Func<Character.PerspectiveMonitor.ObjectDeathVisiblity>(() => { return 0; });
            var objectDeathVisiblityActionDelegate = new Action<Character.PerspectiveMonitor.ObjectDeathVisiblity>((Character.PerspectiveMonitor.ObjectDeathVisiblity value) => { });
#endif
#pragma warning restore 0219
        }
    }
}
#endif