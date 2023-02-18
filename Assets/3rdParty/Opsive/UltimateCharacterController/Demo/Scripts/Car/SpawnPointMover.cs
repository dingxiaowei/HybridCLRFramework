/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Car
{
    using UnityEngine;

    /// <summary>
    /// Moves the spawn point to the target location after it has been initialized.
    /// </summary>
    public class SpawnPointMover : MonoBehaviour
    {
        [Tooltip("The location to move the spawn point to.")]
        [SerializeField] protected Transform m_Target;

        /// <summary>
        /// The spawn point has been initialized by the time start is called.
        /// </summary>
        public void Start()
        {
            transform.parent = m_Target;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}
