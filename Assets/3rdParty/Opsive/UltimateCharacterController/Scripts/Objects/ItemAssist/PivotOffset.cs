/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.ItemAssist
{
    using UnityEngine;

    /// <summary>
    /// Specifies an offset for the pivot position.
    /// </summary>
    public class PivotOffset : MonoBehaviour
    {
        [Tooltip("The pivot offset.")]
        [SerializeField] protected Vector3 m_Offset;

        public Vector3 Offset { get { return m_Offset; } set { m_Offset = value; } }
    }
}