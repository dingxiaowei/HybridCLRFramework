/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Objects
{
    /// <summary>
    /// Specifies the number of forward faces the object has, used by the Detect Object Ability Base ability.
    /// </summary>
    public class ObjectForwardFaces : MonoBehaviour
    {
        [Tooltip("The number of forward facing sides the object has.")]
        [SerializeField] protected int m_ForwardFaceCount = 1;

        public int ForwardFaceCount { get { return m_ForwardFaceCount; } set { m_ForwardFaceCount = value; } }
    }
}