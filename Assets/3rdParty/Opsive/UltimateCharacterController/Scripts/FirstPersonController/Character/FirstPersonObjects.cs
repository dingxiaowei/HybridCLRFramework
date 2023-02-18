/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Character
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.StateSystem;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.FirstPersonController.Character.Identifiers;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Manages the location of the first person objects while in first or third person view.
    /// </summary>
    public class FirstPersonObjects : StateBehavior
    {
        [Tooltip("The minimum pitch angle (in degrees).")]
        [SerializeField] protected float m_MinPitchLimit = -90;
        [Tooltip("The maximum pitch angle (in degrees).")]
        [SerializeField] protected float m_MaxPitchLimit = 90;
        [Tooltip("Should the object's pitch be locked to the character's rotation?")]
        [SerializeField] protected bool m_LockPitch;
        [Tooltip("The minimum yaw angle (in degrees).")]
        [SerializeField] protected float m_MinYawLimit = -180;
        [Tooltip("The maximum yaw angle (in degrees).")]
        [SerializeField] protected float m_MaxYawLimit = 180;
        [Tooltip("Should the object's yaw be locked to the character's rotation?")]
        [SerializeField] protected bool m_LockYaw;
        [Tooltip("Should the object rotate with a change in crosshairs rotation?")]
        [SerializeField] protected bool m_RotateWithCrosshairs = true;
        [Tooltip("The speed at which the object rotates towards the target position.")]
        [SerializeField] protected float m_RotationSpeed = 15;
        [Tooltip("Should the objects be positioned according to the target position of the camera ignorning the look offset?")]
        [SerializeField] protected bool m_IgnorePositionalLookOffset;
        [Tooltip("If ignoring the look offset, specifies the offset from the target position that the first person objects should move towards.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("If ignoring the look offset, specifies the speed that the first person objects should move towards the target position.")]
        [SerializeField] protected float m_MoveSpeed;

        public float MinPitchLimit
        {
            get { return m_MinPitchLimit; }
            set
            {
                m_MinPitchLimit = value;
                if (Application.isPlaying) {
                    if (m_LockPitch) { UpdateLockedPitchAngle(); }
                    enabled = IsActive();
                }
            }
        }
        public float MaxPitchLimit
        {
            get { return m_MaxPitchLimit; }
            set
            {
                m_MaxPitchLimit = value;
                if (Application.isPlaying) {
                    if (m_LockPitch) { UpdateLockedPitchAngle(); }
                    enabled = IsActive();
                }
            }
        }
        public bool LockPitch { get { return m_LockPitch; }
            set
            {
                m_LockPitch = value;
                if (Application.isPlaying) {
                    if (m_LockPitch) { UpdateLockedPitchAngle(); }
                    enabled = IsActive();
                }
            }
        }
        public float MinYawLimit
        {
            get { return m_MinYawLimit; }
            set
            {
                m_MinYawLimit = value;
                if (Application.isPlaying) {
                    if (m_LockYaw) { UpdateLockedYawAngle(); }
                    enabled = IsActive();
                }
            }
        }
        public float MaxYawLimit
        {
            get { return m_MaxYawLimit; }
            set
            {
                m_MaxYawLimit = value;
                if (Application.isPlaying) {
                    if (m_LockYaw) { UpdateLockedYawAngle(); }
                    enabled = IsActive();
                }
            }
        }
        public bool LockYaw
        {
            get { return m_LockYaw; }
            set
            {
                m_LockYaw = value;
                if (Application.isPlaying) {
                    if (m_LockYaw) { UpdateLockedYawAngle(); }
                    enabled = IsActive();
                }
            }
        }
        public bool RotateWithCrosshairs
        {
            get { return m_RotateWithCrosshairs; }
            set
            {
                m_RotateWithCrosshairs = value;
                if (Application.isPlaying) {
                    enabled = IsActive();
                }
            }
        }
        public float RotationSpeed { get { return m_RotationSpeed; } set { m_RotationSpeed = value; } }
        public bool IgnorePositionalLookOffset { get { return m_IgnorePositionalLookOffset; } set { m_IgnorePositionalLookOffset = value; if (Application.isPlaying) { enabled = IsActive(); } } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public float MoveSpeed { get { return m_MoveSpeed; } set { m_MoveSpeed = value; } }

        private Transform m_Transform;
        private GameObject m_GameObject;
        private Transform m_CharacterTransform;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private CameraController m_CameraController;
        private GameObject m_Character;
        private Transform m_CameraTransform;
        private GameObject[] m_FirstPersonBaseObjects;
        private HashSet<GameObject> m_ShouldActivateObject = new HashSet<GameObject>();
        private Dictionary<Item, GameObject[]> m_ItemBaseObjectMap = new Dictionary<Item, GameObject[]>();
        private Item[] m_EquippedItems;

        private float m_Pitch;
        private float m_Yaw;
        public GameObject Character { get { return m_Character; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_Transform = transform;
            m_CharacterLocomotion = gameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            m_CharacterTransform = m_CharacterLocomotion.transform;
            m_Character = m_CharacterTransform.gameObject;
            var baseObjects = GetComponentsInChildren<FirstPersonBaseObject>();
            var count = 0;
            m_FirstPersonBaseObjects = new GameObject[baseObjects.Length];
            for (int i = 0; i < baseObjects.Length; ++i) {
                if (baseObjects[i].AlwaysActive) {
                    continue;
                }
                m_FirstPersonBaseObjects[count] = baseObjects[i].gameObject;
                m_FirstPersonBaseObjects[count].SetActive(false);
                count++;
            }
            if (count != baseObjects.Length) {
                System.Array.Resize(ref m_FirstPersonBaseObjects, count);
            }
            var inventory = m_Character.GetCachedComponent<Inventory.InventoryBase>();
            m_EquippedItems = new Item[inventory.SlotCount];

            EventHandler.RegisterEvent<CameraController>(m_Character, "OnCharacterAttachCamera", OnAttachCamera);
            EventHandler.RegisterEvent<Item>(m_Character, "OnInventoryAddItem", OnAddItem);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_Character, "OnRespawn", OnRespawn);
            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterActivate", OnActivate);

            enabled = false;
        }

        /// <summary>
        /// The character has been attached to the camera. Initialze the camera-related values.
        /// </summary>
        /// <param name="cameraGameObject">The camera controller attached to the character. Can be null.</param>
        private void OnAttachCamera(CameraController cameraController)
        {
            m_CameraController = cameraController;
            m_Transform.parent = (m_CameraController != null ? m_CameraController.Transform : m_CharacterTransform);
            m_Transform.localPosition = Vector3.zero;
            m_Transform.localRotation = Quaternion.identity;
            m_Pitch = m_Yaw = 0;
            m_CameraTransform = (m_CameraController != null ? m_CameraController.Transform : null);
            enabled = IsActive();
        }

        /// <summary>
        /// Is the component active?
        /// </summary>
        /// <returns>True if the component is active.</returns>
        private bool IsActive()
        {
            // The component should be active if any values can update the rotation.
            return m_CameraTransform != null && (Mathf.Abs(m_MinPitchLimit - m_MaxPitchLimit) < 180 || m_LockPitch || 
                Mathf.Abs(m_MinYawLimit - m_MaxYawLimit) < 360 || m_LockYaw || m_RotateWithCrosshairs || m_IgnorePositionalLookOffset || 
                m_Transform.localPosition != Vector3.zero);
        }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        /// <summary>
        /// Disables the GameObject if the character is remote.
        /// </summary>
        public void Start()
        {
            // Remote players should never see the first person objects.
            var networkInfo = m_Character.GetComponentInParent<Networking.INetworkInfo>();
            if (networkInfo != null && !networkInfo.IsLocalPlayer()) {
                m_GameObject.SetActive(false);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterActivate", OnActivate);
            }
        }
#endif

        /// <summary>
        /// Updates the internal pitch angle while ensuring it is within the pitch limits.
        /// </summary>
        private void UpdateLockedPitchAngle()
        {
            var localRotation = MathUtility.InverseTransformQuaternion(m_CharacterTransform.rotation, m_CameraTransform.rotation).eulerAngles;
            if (Mathf.Abs(m_MinPitchLimit - m_MaxPitchLimit) < 180) {
                m_Pitch = MathUtility.ClampAngle(localRotation.x, m_MinPitchLimit, m_MaxPitchLimit);
            } else {
                m_Pitch = localRotation.x;
            }
        }

        /// <summary>
        /// Updates the internal yaw angle while ensuring it is within the yaw limits.
        /// </summary>
        private void UpdateLockedYawAngle()
        {
            var localRotation = MathUtility.InverseTransformQuaternion(m_CharacterTransform.rotation, m_CameraTransform.rotation).eulerAngles;
            if (Mathf.Abs(m_MinYawLimit - m_MaxYawLimit) < 360) {
                m_Yaw = MathUtility.ClampAngle(localRotation.y, m_MinYawLimit, m_MaxYawLimit);
            } else {
                m_Yaw = localRotation.y;
            }
        }

        /// <summary>
        /// Adjusts the location of the transform according to the enabled toggles.
        /// </summary>
        private void LateUpdate()
        {
            var localRotation = MathUtility.InverseTransformQuaternion(m_CharacterTransform.rotation, m_CameraTransform.rotation).eulerAngles;
            if (m_LockPitch) {
                localRotation.x = m_Pitch;
            } else if (Mathf.Abs(m_MinPitchLimit - m_MaxPitchLimit) < 180) {
                localRotation.x = MathUtility.ClampAngle(localRotation.x, m_MinPitchLimit, m_MaxPitchLimit);
            }
            if (m_LockYaw) {
                localRotation.y = m_Yaw;
            } else if (Mathf.Abs(m_MinYawLimit - m_MaxYawLimit) < 360) {
                localRotation.y = MathUtility.ClampAngle(localRotation.y, m_MinYawLimit, m_MaxYawLimit);
            }
            var rotation = MathUtility.TransformQuaternion(m_CharacterTransform.rotation, Quaternion.Euler(localRotation));
            if (m_RotateWithCrosshairs) {
                rotation = m_CameraController.GetCrosshairsDeltaRotation() * rotation;
            }
            m_Transform.rotation = Quaternion.Slerp(m_Transform.rotation, rotation, m_RotationSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime);

            if (m_IgnorePositionalLookOffset && m_CameraController.ActiveViewType is FirstPersonController.Camera.ViewTypes.FirstPerson) {
                var firstPersonViewType = m_CameraController.ActiveViewType as FirstPersonController.Camera.ViewTypes.FirstPerson;
                var targetPosition = firstPersonViewType.GetTargetPosition() + m_CharacterTransform.TransformDirection(m_PositionOffset);
                m_Transform.position = Vector3.MoveTowards(m_Transform.position, targetPosition, m_MoveSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime);
            } else if (m_Transform.localPosition != Vector3.zero) {
                m_Transform.localPosition = Vector3.MoveTowards(m_Transform.localPosition, Vector3.zero, m_MoveSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime);
            }
        }

        /// <summary>
        /// The inventory has added the specified item.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        private void OnAddItem(Item item)
        {
            if (m_ItemBaseObjectMap.ContainsKey(item)) {
                return;
            }

            var firstPersonPerspective = item.GetComponent<Items.FirstPersonPerspectiveItem>();
            if (firstPersonPerspective != null && firstPersonPerspective.Object != null) {
                // If the item contains a first person object then the item will have a parent FirstPersonBaseObject. This object should be enabled/disabled depending on
                // the item active status.
                var firstPersonBaseObject = firstPersonPerspective.Object.transform.GetComponentInParentIncludeInactive<FirstPersonBaseObject>();
                // A base object may not exist in VR.
                if (firstPersonBaseObject == null) {
                    return;
                }

                var baseObjects = new GameObject[firstPersonPerspective.AdditionalControlObjects.Length + 1];
                baseObjects[0] = firstPersonBaseObject.gameObject;
                for (int i = 0; i < firstPersonPerspective.AdditionalControlObjects.Length; ++i) {
                    baseObjects[i + 1] = firstPersonPerspective.AdditionalControlObjects[i];
                }

                m_ItemBaseObjectMap.Add(item, baseObjects);
            }
        }

        /// <summary>
        /// The specified item will be equipped.
        /// </summary>
        public void StartEquip(Item item, int slotID)
        {
            m_EquippedItems[slotID] = item;

            CheckActiveBaseObjects();
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        public void UnequipItem(Item item, int slotID)
        {
            if (item != m_EquippedItems[slotID]) {
                return;
            }

            m_EquippedItems[slotID] = null;

            CheckActiveBaseObjects();
        }

        /// <summary>
        /// Loops through the base objects determining if it should be active.
        /// </summary>
        private void CheckActiveBaseObjects()
        {
            // Loop through the equipped items to determine which base objects should be activated.
            // Once the loop is complete do the activation/deactivation based on the equipped items.
            m_ShouldActivateObject.Clear();
            
            for (int i = 0; i < m_EquippedItems.Length; ++i) {
                if (m_EquippedItems[i] != null) {
                    if (!m_ItemBaseObjectMap.TryGetValue(m_EquippedItems[i], out var baseObjects)) {
                        Debug.LogError($"Error: Unable to find the base object for item {m_EquippedItems[i].name}. Ensure the item specifies a base object under the First Person Perspective Item component.");
                        continue;
                    }
                    for (int j = 0; j < baseObjects.Length; ++j) {
                        m_ShouldActivateObject.Add(baseObjects[j]);
                    }
                }
            }

            for (int i = 0; i < m_FirstPersonBaseObjects.Length; ++i) {
                m_FirstPersonBaseObjects[i].SetActive(m_ShouldActivateObject.Contains(m_FirstPersonBaseObjects[i]));
            }
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            enabled = false;
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            enabled = m_CameraTransform != null && (m_LockPitch || m_LockYaw || m_RotateWithCrosshairs);
        }

        /// <summary>
        /// The character has been activated or deactivated.
        /// </summary>
        /// <param name="activate">Was the character activated?</param>
        private void OnActivate(bool activate)
        {
            m_GameObject.SetActive(activate);
        }

        /// <summary>
        /// The GameObject was destroyed. Unregister for any registered events.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<CameraController>(m_Character, "OnCharacterAttachCamera", OnAttachCamera);
            EventHandler.UnregisterEvent<Item>(m_Character, "OnInventoryAddItem", OnAddItem);
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_Character, "OnRespawn", OnRespawn);
            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterActivate", OnActivate);
        }
    }
}