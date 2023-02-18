/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Character.Identifiers
{
    using UnityEngine;

    /// <summary>
    /// Identifier component which identifies the first person base object.
    /// </summary>
    public class FirstPersonBaseObject : MonoBehaviour
    {
        [Tooltip("The unique ID of the first person base object.")]
        [SerializeField] protected int m_ID;
        [Tooltip("Should the base object always stay active? This is useful for first person VR.")]
        [SerializeField] protected bool m_AlwaysActive;

        [Opsive.Shared.Utility.NonSerialized] public int ID { get { return m_ID; } set { m_ID = value; } }
        [Opsive.Shared.Utility.NonSerialized] public bool AlwaysActive { get { return m_AlwaysActive; } set { m_AlwaysActive = value; } }
    }
}