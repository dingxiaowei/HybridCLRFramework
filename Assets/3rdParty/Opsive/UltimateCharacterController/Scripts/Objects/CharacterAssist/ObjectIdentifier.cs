/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects
{
    using UnityEngine;

    /// <summary>
    /// Represents a unique identifier for the object that this component is attached to, used by the Detect Object Ability Base ability.
    /// </summary>
    public class ObjectIdentifier : MonoBehaviour
    {
        [Tooltip("The value of the identifier.")]
        [SerializeField] protected uint m_ID;

        public uint ID { get { return m_ID; } set { m_ID = value; } }
    }
}