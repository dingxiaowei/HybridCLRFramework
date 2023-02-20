/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Items
{
    /// <summary>
    /// Describes any third person perspective dependent properties for the ShootableWeapon.
    /// </summary>
    public class ThirdPersonShootableWeaponProperties : ThirdPersonWeaponProperties, IShootableWeaponPerspectiveProperties
    {
        [Tooltip("The sensitivity amount for how much the weapon must be looking in the look source direction (-1 is least sensitive and 1 is most).")]
        [Range(-1, 1)] [SerializeField] protected float m_LookSensitivity = 0.97f;
        [Tooltip("The location that the weapon is fired at.")]
        [SerializeField] protected Transform m_FirePointLocation;
        [Tooltip("The transform that the fire point should be attached to.")]
        [SerializeField] protected Transform m_FirePointAttachmentLocation;
        [Tooltip("The ID of the transform that the fire point should be attached to.. This field will be used if the value is not -1 and the attachment is null.")]
        [SerializeField] protected int m_FirePointAttachmentLocationID = -1;
        [Tooltip("The location that the muzzle flash is spawned at.")]
        [SerializeField] protected Transform m_MuzzleFlashLocation;
        [Tooltip("The location that the shell is ejected at.")]
        [SerializeField] protected Transform m_ShellLocation;
        [Tooltip("The location that the smoke is spawned at.")]
        [SerializeField] protected Transform m_SmokeLocation;
        [Tooltip("The location that the tracer is spawned at.")]
        [SerializeField] protected Transform m_TracerLocation;
        [Tooltip("A reference to the weapon's clip that can be reloaded.")]
        [SerializeField] protected Transform m_ReloadableClip;
        [Tooltip("A reference to the reloadable clip attachment transform.")]
        [SerializeField] protected Transform m_ReloadableClipAttachment;
        [Tooltip("The ID of the reloadable clip attachment transform. This field will be used if the value is not -1 and the attachment is null.")]
        [SerializeField] protected int m_ReloadableClipAttachmentID = -1;
        [Tooltip("A reference to the attachment transform that the projectile should be parented to when reloading.")]
        [SerializeField] protected Transform m_ReloadProjectileAttachment;
        [Tooltip("The ID of the attachment transform that hte projectile should be parented to when reloading. This field will be used if the value is not -1 and the attachment is null.")]
        [SerializeField] protected int m_ReloadProjectileAttachmentID = -1;
        [Tooltip("Optionally specify if the weapon has a camera for the scope.")]
        [SerializeField] protected GameObject m_ScopeCamera;

        public float LookSensitivity { get { return m_LookSensitivity; } set { m_LookSensitivity = value; } }
        [NonSerialized] public Transform FirePointLocation { get { return m_FirePointLocation; } set { m_FirePointLocation = value; } }
        [NonSerialized] public Transform MuzzleFlashLocation { get { return m_MuzzleFlashLocation; } set { m_MuzzleFlashLocation = value; } }
        [NonSerialized] public Transform ShellLocation { get { return m_ShellLocation; } set { m_ShellLocation = value; } }
        [NonSerialized] public Transform SmokeLocation { get { return m_SmokeLocation; } set { m_SmokeLocation = value; } }
        [NonSerialized] public Transform TracerLocation { get { return m_TracerLocation; } set { m_TracerLocation = value; } }
        [NonSerialized] public Transform ReloadableClip { get { return m_ReloadableClip; } set { m_ReloadableClip = value; } }
        [NonSerialized] public Transform ReloadableClipAttachment { get { return m_ReloadableClipAttachment; } set { m_ReloadableClipAttachment = value; } }
        [NonSerialized] public int ReloadableClipAttachmentID { get { return m_ReloadableClipAttachmentID; } set { m_ReloadableClipAttachmentID = value; } }
        [NonSerialized] public Transform ReloadProjectileAttachment { get { return m_ReloadProjectileAttachment; } set { m_ReloadProjectileAttachment = value; } }
        [NonSerialized] public int ReloadProjectileAttachmentID { get { return m_ReloadProjectileAttachmentID; } set { m_ReloadProjectileAttachmentID = value; } }
        [NonSerialized] public GameObject ScopeCamera { get { return m_ScopeCamera; } set { m_ScopeCamera = value; } }

        private ILookSource m_LookSource;
        private Transform m_ObjectTransform;

#if UNITY_EDITOR
        private float m_LastLookSensitivity;
        private int m_ConsistantLookSensitivityCount;
#endif

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_ObjectTransform = m_Object.transform;
            // The look source is used to determine if the item is facing in the forward direction.
            m_LookSource = m_CharacterLocomotion.LookSource;

            // The item may be added at runtime while the attachment transform is located on the character.
            if (m_FirePointAttachmentLocationID != -1 || (m_ReloadableClipAttachment == null && m_ReloadableClip != null) || m_ReloadProjectileAttachmentID != -1) {
                var character = GetComponentInParent<UltimateCharacterLocomotion>();
                var objectIdentifiers = character.GetComponentsInChildren<Objects.ObjectIdentifier>();
                if (objectIdentifiers.Length > 0) {
                    for (int i = 0; i < objectIdentifiers.Length; ++i) {
#if FIRST_PERSON_CONTROLLER
                        // The first person attachments should be filtered out.
                        if (objectIdentifiers[i].GetComponentInParent<FirstPersonController.Character.FirstPersonObjects>() != null) {
                            continue;
                        }
#endif
                        if (objectIdentifiers[i].ID == m_FirePointAttachmentLocationID) {
                            m_FirePointAttachmentLocation = objectIdentifiers[i].transform;
                        } else if (objectIdentifiers[i].ID == m_ReloadableClipAttachmentID) {
                            m_ReloadableClipAttachment = objectIdentifiers[i].transform;
                        } else if (objectIdentifiers[i].ID == m_ReloadProjectileAttachmentID) {
                            m_ReloadProjectileAttachment = objectIdentifiers[i].transform;
                        }

                        // If the references are found then the loop can end early.
                        if ((m_ReloadableClip == null || m_ReloadableClipAttachmentID == -1 || m_ReloadableClipAttachment != null) && 
                            (m_FirePointAttachmentLocationID == -1 || m_FirePointAttachmentLocation != null) &&
                            (m_ReloadProjectileAttachmentID == -1 || m_ReloadProjectileAttachment != null)) {
                            break;
                        }
                    }
                    // If no IDs match then log a warning.
                    if (m_FirePointAttachmentLocation == null && m_FirePointAttachmentLocationID != -1) {
                        Debug.LogWarning("Warning: Unable to find the third person fire point attachment ObjectIdentifier with the ID " + m_FirePointAttachmentLocationID + " for item " + name + ".");
                    }
                    if (m_ReloadableClipAttachment == null && m_ReloadableClip != null && m_ReloadableClipAttachmentID != -1) {
                        Debug.LogWarning("Warning: Unable to find the third person reload attachment ObjectIdentifier with the ID " + m_ReloadableClipAttachmentID + " for item " + name + ".");
                    }
                    if (m_ReloadProjectileAttachment == null && m_ReloadProjectileAttachmentID != -1) {
                        Debug.LogWarning("Warning: Unable to find the third person reload projectile ObjectIdentifier with the ID " + m_ReloadProjectileAttachmentID + " for item " + name + ".");
                    }
                }
            }
            if (m_FirePointAttachmentLocation != null) {
                // If the fire point is null then the found attachment transform becomes the firepoint. If it is not null then the parent is set.
                if (m_FirePointLocation == null) {
                    m_FirePointLocation = m_FirePointAttachmentLocation;
                } else {
                    m_FirePointLocation.SetParentOrigin(m_FirePointAttachmentLocation);
                }
            }

            EventHandler.RegisterEvent<ILookSource>(m_Character, "OnCharacterAttachLookSource", OnAttachLookSource);
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            m_LookSource = lookSource;
        }

        /// <summary>
        /// Can the weapon be fired?
        /// </summary>
        /// <param name="fireInLookSourceDirection">Should the weapon fire in the LookSource direction?</param>
        /// <param name="abilityActive">Is the Use ability active?</param>
        /// <returns>True if the item can be fired.</returns>
        public bool CanFire(bool abilityActive, bool fireInLookSourceDirection)
        {
#if UNITY_EDITOR
            if (!abilityActive) {
                m_LastLookSensitivity = -1;
                m_ConsistantLookSensitivityCount = 0;
            }
#endif
            // The object has to be facing in the same general direction as the look source. When the ability is not active the direction shouldn't prevent
            // the ability from starting. This will allow the weapon to move to the correct direction while the ability is active.
            if (abilityActive && fireInLookSourceDirection) {
                var lookSensitivity = Vector3.Dot(m_ObjectTransform.forward, m_LookSource.LookDirection(m_ObjectTransform.position, false, 0, true));
#if UNITY_EDITOR
                // A common cause for the weapon not being able to fire is because of the look sensitivity. Add a check to display a warning if the look sensitivity is blocking the firing.
                if (lookSensitivity <= m_LookSensitivity && m_ConsistantLookSensitivityCount != -1) {
                    if (Mathf.Abs(m_LastLookSensitivity - lookSensitivity) < 0.05f) {
                        m_ConsistantLookSensitivityCount++;
                        if (m_ConsistantLookSensitivityCount > 10) {
                            Debug.LogWarning("Warning: The ShootableWeapon is unable to fire because of the Look Sensitivity on the ShootableWeaponProperties. See this page for more info: " +
                                             "https://opsive.com/support/documentation/ultimate-character-controller/items/actions/usable/shootable-weapon/");
                            m_ConsistantLookSensitivityCount = -1;
                        }
                    } else {
                        m_ConsistantLookSensitivityCount = 0;
                    }
                    m_LastLookSensitivity = lookSensitivity;
                }
#endif
                return lookSensitivity > m_LookSensitivity;
            }
            return true;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<ILookSource>(m_Character, "OnCharacterAttachLookSource", OnAttachLookSource);
        }
    }
}