/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Camera
{
    /// <summary>
    /// Specifies an offset to apply to the object targeted by aim assist.
    /// </summary>
    public class AimAssistOffset : MonoBehaviour
    {
        [Tooltip("The amount of offset to apply to the object.")]
        [SerializeField] protected Vector3 m_Offset;

        public Vector3 Offset { get { return m_Offset; } }
    }
}