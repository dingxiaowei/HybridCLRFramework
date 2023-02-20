/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Items.Actions;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Items
{
    /// <summary>
    /// Describes any third person perspective dependent properties for the GrenadeItem.
    /// </summary>
    public class ThirdPersonGrenadeItemProperties : ThirdPersonThrowableItemProperties, IGrenadeItemPerspectiveProperties
    {
        [Tooltip("A reference to the pin attachment transform.")]
        [SerializeField] protected Transform m_PinAttachmentLocation;
        [Tooltip("The ID of the pin attachment transform. This field will be used if the value is not -1 and the location is null.")]
        [SerializeField] protected int m_PinAttachmentLocationID = -1;

        [Utility.NonSerialized] public Transform PinAttachmentLocation { get { return m_PinAttachmentLocation; } set { m_PinAttachmentLocation = value; } }
        [Utility.NonSerialized] public int PinAttachmentLocationID { get { return m_PinAttachmentLocationID; } set { m_PinAttachmentLocationID = value; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // If the ID is not -1 then find the transform that it will attach to.
            if (m_PinAttachmentLocation == null && m_PinAttachmentLocationID != -1) {
                var character = GetComponentInParent<UltimateCharacterLocomotion>();
                var objectIdentifiers = character.gameObject.GetComponentsInChildren<Objects.ObjectIdentifier>();
                if (objectIdentifiers.Length > 0) {
                    for (int i = 0; i < objectIdentifiers.Length; ++i) {
#if FIRST_PERSON_CONTROLLER
                        // The first person attachments should be filtered out.
                        if (objectIdentifiers[i].GetComponentInParent<FirstPersonController.Character.FirstPersonObjects>() != null) {
                            continue;
                        }
#endif
                        if (objectIdentifiers[i].ID == m_PinAttachmentLocationID) {
                            m_PinAttachmentLocation = objectIdentifiers[i].transform;
                            break;
                        }
                    }
                    // If no IDs match then log a warning and assign the first transform.
                    if (m_PinAttachmentLocation == null) {
                        Debug.LogWarning("Warning: Unable to find the third person pin attachment ObjectIdentifier with the ID " + m_PinAttachmentLocationID + " for item " + name + ".");
                        m_PinAttachmentLocation = objectIdentifiers[0].transform;
                    }
                }
            }
        }
    }
}