/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    /// Switch the camera to a first or third person perspective.
    /// </summary>
    public class PerspectiveSwitchZone : MonoBehaviour
    {
        [Tooltip("Should the camera switch to a first person perspective?")]
        [SerializeField] protected bool m_FirstPersonPerspective;

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            var cameraController = UnityEngineUtility.FindCamera(characterLocomotion.gameObject).GetComponent<Camera.CameraController>();
            cameraController.SetPerspective(m_FirstPersonPerspective);
        }
    }
}