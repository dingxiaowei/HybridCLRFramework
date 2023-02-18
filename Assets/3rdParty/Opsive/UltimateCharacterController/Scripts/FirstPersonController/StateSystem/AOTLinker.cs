/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

#if NET_4_6 || UNITY_2018_3_OR_NEWER || UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID || UNITY_WII || UNITY_WIIU || UNITY_SWITCH || UNITY_PS3 || UNITY_PS4 || UNITY_XBOXONE || UNITY_WSA
using UnityEngine;
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
using Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes;
#endif
using Opsive.UltimateCharacterController.StateSystem;
using System;

namespace Opsive.UltimateCharacterController.FirstPersonController.StateSystem
{
    // See Opsive.UltimateCharacterController.StateSystem.AOTLinker for an explanation of this class.
    public class AOTLinker : MonoBehaviour
    {
        public void Linker()
        {
#pragma warning disable 0219
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
            var objectOverlayRenderTypeFirstPersonCameraViewType = new Preset.GenericDelegate<FirstPerson.ObjectOverlayRenderType>();
            var objectOverlayRenderTypeFirstPersonCameraFuncDelegate = new Func<FirstPerson.ObjectOverlayRenderType>(() => { return 0; });
            var objectOverlayRenderTypeFirstPersonCameraActionDelegate = new Action<FirstPerson.ObjectOverlayRenderType>((FirstPerson.ObjectOverlayRenderType value) => { });
#endif
#pragma warning restore 0219
        }
    }
}
#endif