/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

#if UNITY_EDITOR
using UnityEngine;

namespace Opsive.UltimateCharacterController.StateSystem
{
    /// <summary>
    /// Inspector helper class for the UltimateCharacterControllerInspector to be able to display states within the ReorderableList.
    /// </summary>
    public class StateInspectorHelper : MonoBehaviour
    {
        [SerializeField] private int[] m_StateIndexData;
        public int[] StateIndexData { get { return m_StateIndexData; } set { m_StateIndexData = value; } }
    }
}
#endif