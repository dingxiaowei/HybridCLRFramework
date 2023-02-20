/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Items.Actions;

namespace Opsive.UltimateCharacterController.Objects.ItemAssist
{
    /// <summary>
    /// The ShieldCollider component specifies the object that acts as a collider for the shield.
    /// </summary>
    public class ShieldCollider : MonoBehaviour
    {
        [Tooltip("A reference to the Shield item action.")]
        [HideInInspector] [SerializeField] protected Shield m_Shield;
        [Tooltip("Is the collider attached to a Shield used for the first person perspective?")]
        [HideInInspector] [SerializeField] protected bool m_FirstPersonPerspective;

        [Utility.NonSerialized] public Shield Shield { get { return m_Shield; } set { m_Shield = value; } }
        [Utility.NonSerialized] public bool FirstPersonPerspective { set { m_FirstPersonPerspective = value; } }

        private Collider m_Collider;
        private GameObject m_Character;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Collider = GetComponent<Collider>();
            m_Collider.enabled = false;

            m_Character = m_Shield.gameObject.GetComponentInParent<Character.UltimateCharacterLocomotion>().gameObject;
            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }

        /// <summary>
        /// The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="inFirstPerson">Is the camera in a first person view?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            // The collider should only be enabled for the corresponding perspective.
            m_Collider.enabled = m_FirstPersonPerspective == firstPersonPerspective;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }
    }
}