/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Camera
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Events;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.StateSystem;
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Base class for the first and third person camera controllers. 
    /// </summary>
    public class CameraController : StateBehavior, ILookSource
    {
        [Tooltip("Should the character be initialized on awake?")]
        [SerializeField] protected bool m_InitCharacterOnAwake = true;
        [Tooltip("The character that the camera should follow.")]
        [SerializeField] protected GameObject m_Character;
        [Tooltip("The transform of the object to attach the camera relative to.")]
        [SerializeField] protected Transform m_Anchor;
        [Tooltip("Should the anchor be assigned automatically based on the humanoid bone?")]
        [SerializeField] protected bool m_AutoAnchor;
        [Tooltip("The bone that the anchor will be assigned to if AutoAnchor is enabled.")]
        [SerializeField] protected HumanBodyBones m_AutoAnchorBone = HumanBodyBones.Head;
        [Tooltip("The offset between the anchor and the camera.")]
        [SerializeField] protected Vector3 m_AnchorOffset = new Vector3(0, 1.8f, 0);
        [Tooltip("The name of the active ViewType.")]
        [SerializeField] protected string m_ViewTypeFullName;
        [Tooltip("The name of the active first person ViewType.")]
        [SerializeField] protected string m_FirstPersonViewTypeFullName;
        [Tooltip("The name of the active third person ViewType.")]
        [SerializeField] protected string m_ThirdPersonViewTypeFullName;
        [Tooltip("The serialization data for the ViewTypes.")]
        [SerializeField] protected Serialization[] m_ViewTypeData;
        [Tooltip("Can the camera change perspectives?")]
        [SerializeField] protected bool m_CanChangePerspectives = true;
        [Tooltip("Can the camera zoom?")]
        [SerializeField] protected bool m_CanZoom = true;
        [Tooltip("The state that should be activated when zoomed.")]
        [SerializeField] protected string m_ZoomState = "Zoom";
        [Tooltip("Should the ItemIdentifier name be appened to the name of the state name?")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_StateAppendItemIdentifierName")] // 2.2.
        [HideInInspector] [SerializeField] protected bool m_StateAppendItemIdentifierName;
        [Tooltip("Unity event invoked when an view type has been started or stopped.")]
        [SerializeField] protected UnityViewTypeBoolEvent m_OnChangeViewTypesEvent;
        [Tooltip("Unity event invoked when the camera changes perspectives.")]
        [SerializeField] protected UnityBoolEvent m_OnChangePerspectivesEvent;
        [Tooltip("Unity event invoked when the camera changes zoom states.")]
        [SerializeField] protected UnityBoolEvent m_OnZoomEvent;

        [NonSerialized] public bool InitCharacterOnAwake { get { return m_InitCharacterOnAwake; } set { m_InitCharacterOnAwake = value; } }
        [NonSerialized] public GameObject Character { get { return m_Character; } set { InitializeCharacter(value); } }
        [NonSerialized] public Transform Anchor { get { return m_Anchor; } set { m_Anchor = value; if (m_Character != null) { InitializeAnchor(); } } }
        public Vector3 AnchorOffset { get { return m_AnchorOffset; } set { m_AnchorOffset = value; if (Application.isPlaying) { EventHandler.ExecuteEvent(m_GameObject, "OnAnchorOffsetUpdated"); } } }
        public bool CanChangePerspectives { get { return m_CanChangePerspectives; } set { m_CanChangePerspectives = value; } }
        public bool CanZoom { get { return m_CanZoom; } set { m_CanZoom = value; if (m_CanZoom && m_ZoomInput && !m_Zoom) { SetZoom(true); } else if (!m_CanZoom && m_Zoom) { SetZoom(false); } } }
        public string ZoomState { get { return m_ZoomState; } set { m_ZoomState = value; } }
        public UnityViewTypeBoolEvent OnChangeViewTypesEvent { get { return m_OnChangeViewTypesEvent; } set { m_OnChangeViewTypesEvent = value; } }
        public UnityBoolEvent OnChangePerspectivesEvent { get { return m_OnChangePerspectivesEvent; } set { m_OnChangePerspectivesEvent = value; } }
        public UnityBoolEvent OnZoomEvent { get { return m_OnZoomEvent; } set { m_OnZoomEvent = value; } }

        private int m_KinematicObjectIndex = -1;
        private ViewType[] m_ViewTypes;
        private ViewType m_ViewType;
        private Dictionary<string, int> m_ViewTypeNameMap = new Dictionary<string, int>();
        private ViewType m_FirstPersonViewType;
        private ViewType m_ThirdPersonViewType;
        private Transition m_Transitioner;

        private bool m_ZoomInput;
        private bool m_Zoom;

        private GameObject m_GameObject;
        private Transform m_Transform;
        private Transform m_CharacterTransform;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private InventoryBase m_CharacterInventory;

        [NonSerialized] public int KinematicObjectIndex { get { return m_KinematicObjectIndex; } set { m_KinematicObjectIndex = value; } }
        public bool ZoomInput { get { return m_ZoomInput; } }
        public GameObject GameObject { get { return m_ViewType.GameObject; } }
        public Transform Transform { get { return m_ViewType.Transform; } }
        public GameObject CameraGameObject { get { return m_ViewType.CameraGameObject; } }
        public Transform CameraTransform { get { return m_ViewType.CameraTransform; } }

        public ViewType[] ViewTypes { get { return m_ViewTypes; }
            set
            {
                m_ViewTypes = value;
                m_ViewTypeNameMap.Clear();
                if (m_ViewTypes != null) {
                    for (int i = 0; i < m_ViewTypes.Length; ++i) {
                        if (m_ViewTypes[i] == null) {
                            continue;
                        }
                        m_ViewTypeNameMap.Add(m_ViewTypes[i].GetType().FullName, i);
                    }
                }
            }
        }
        public Serialization[] ViewTypeData { get { return m_ViewTypeData; } set { m_ViewTypeData = value; } }
        [NonSerialized] public string ViewTypeFullName { get { return m_ViewTypeFullName; } set { SetViewType(value); } }
        public ViewType ActiveViewType { get { return m_ViewType; } }
        public string FirstPersonViewTypeFullName { get { return m_FirstPersonViewTypeFullName; }
            set
            {
                m_FirstPersonViewTypeFullName = value;
                if (Application.isPlaying) {
                    if (m_ViewType != null && m_ViewType.FirstPersonPerspective) {
                        SetViewType(m_FirstPersonViewTypeFullName);
                    } else {
                        // The first person type should always match the name.
                        int index;
                        var type = UnityEngineUtility.GetType(m_FirstPersonViewTypeFullName);
                        if (type != null && m_ViewTypeNameMap.TryGetValue(type.FullName, out index)) {
                            m_FirstPersonViewType = m_ViewTypes[index];
                        }
                    }
                }
            }
        }
        public string ThirdPersonViewTypeFullName { get { return m_ThirdPersonViewTypeFullName; }
            set
            {
                m_ThirdPersonViewTypeFullName = value;
                if (Application.isPlaying) {
                    if (m_ViewType != null && !m_ViewType.FirstPersonPerspective) {
                        SetViewType(m_ThirdPersonViewTypeFullName);
                    } else {
                        // The third person type should always match the name.
                        int index;
                        var type = UnityEngineUtility.GetType(m_ThirdPersonViewTypeFullName);
                        if (type != null && m_ViewTypeNameMap.TryGetValue(type.FullName, out index)) {
                            m_ThirdPersonViewType = m_ViewTypes[index];
                        }
                    }
                }
            }
        }

        public float LookDirectionDistance { get { return m_ViewType.LookDirectionDistance; } }
        public float Pitch { get { return m_ViewType.Pitch; } }

        /// <summary>
        /// Initialize the camera controller.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_Transform = transform;
            m_ViewType = null; // The ViewType may have been assigned during fast enter playmode.

            // Create the view types from the serialized data.
            DeserializeViewTypes(true);

            // Initialize the first and third person view types if they haven't been initialized yet.
            if (m_FirstPersonViewType == null && !string.IsNullOrEmpty(m_FirstPersonViewTypeFullName)) {
                int index;
                if (m_ViewTypeNameMap.TryGetValue(m_FirstPersonViewTypeFullName, out index)) {
                    m_FirstPersonViewType = m_ViewTypes[index];
                }
            }
            if (m_ThirdPersonViewType == null && !string.IsNullOrEmpty(m_ThirdPersonViewTypeFullName)) {
                int index;
                if (m_ViewTypeNameMap.TryGetValue(m_ThirdPersonViewTypeFullName, out index)) {
                    m_ThirdPersonViewType = m_ViewTypes[index];
                }
            }

            // Call Awake on all of the deserialized view types after the camera controller's Awake method is complete.
            if (m_ViewTypes != null) {
                for (int i = 0; i < m_ViewTypes.Length; ++i) {
                    m_ViewTypes[i].Awake();
                }
            }

            if (m_InitCharacterOnAwake) {
                if (m_Character == null) {
                    Debug.LogWarning("Warning: No character has been assigned to the Camera Controller. It will automatically be assigned to the GameObject with the Player tag.");
                    m_Character = GameObject.FindGameObjectWithTag("Player");
                    if (m_Character == null) {
                        Debug.LogError("Error: Unable to find the character with the Player tag. The camera will be disabled.");
                    }
                }
            } else {
                m_Character = null;
            }

            // The items need to know if they are in a first person perspective within Awake.
            if (m_Character != null) {
                var characterLocomotion = m_Character.GetCachedComponent<UltimateCharacterLocomotion>();
                if (characterLocomotion == null) {
                    Debug.LogError("Error: the character " + m_Character + " doesn't have the Ultimate Character Locomotion component.");
                    m_Character = null;
                    return;
                }
                characterLocomotion.FirstPersonPerspective = m_ViewType.FirstPersonPerspective;
            }
        }

        /// <summary>
        /// Deserialize the view types.
        /// </summary>
        /// <returns>Were any view types removed?</returns>
        public bool DeserializeViewTypes()
        {
            return DeserializeViewTypes(false);
        }

        /// <summary>
        /// Deserialize the view types.
        /// </summary>
        /// <param name="forceDeserialization">Should the view types be force deserialized?</param>
        /// <returns>Were any view types removed?</returns>
        public bool DeserializeViewTypes(bool forceDeserialization)
        {
            // The View Types only need to be deserialized once.
            if (m_ViewTypes != null && !forceDeserialization) {
                return false;
            }

            var dirty = false;
            if (m_ViewTypeData != null && m_ViewTypeData.Length > 0) {
                m_ViewTypes = new ViewType[m_ViewTypeData.Length];
                m_ViewTypeNameMap.Clear();
                for (int i = 0; i < m_ViewTypeData.Length; ++i) {
                    m_ViewTypes[i] = m_ViewTypeData[i].DeserializeFields(MemberVisibility.Public) as ViewType;
                    if (m_ViewTypes[i] == null) {
                        dirty = true;
                        continue;
                    }
                    // The transitioning view type is saved separately.
                    if (m_ViewTypes[i] is Transition) {
                        m_Transitioner = m_ViewTypes[i] as Transition;
                    } else {
                        m_ViewTypeNameMap.Add(m_ViewTypes[i].GetType().FullName, i);
                    }
                    if (Application.isPlaying) {
                        m_ViewTypes[i].Initialize(this);
                    }
                }
            }
            if (UnityEngineUtility.GetType(m_ViewTypeFullName) != null) {
                SetViewType(m_ViewTypeFullName);
            } else {
                SetViewType(m_ViewTypes[0].GetType(), false);
            }
            return dirty;
        }

        /// <summary>
        /// Sets the view type to the object with the specified type.
        /// </summary>
        /// <param name="typeName">The type name of the ViewType which should be set.</param>
        private void SetViewType(string typeName)
        {
            SetViewType(UnityEngineUtility.GetType(typeName), false);
        }

        /// <summary>
        /// Sets the view type to the object with the specified type.
        /// </summary>
        /// <param name="type">The type of the ViewType which should be set.</param>
        /// <param name="immediateTransition">Should the ViewType be transitioned immediately?</param>
        public void SetViewType(System.Type type, bool immediateTransition)
        {
            if ((m_ViewType != null && m_ViewType.GetType() == type) || type == null) {
                return;
            }

            // The ViewTypes may not be deserialized yet.
            if (m_ViewTypeNameMap.Count == 0) {
                DeserializeViewTypes();
            }

            int index;
            if (!m_ViewTypeNameMap.TryGetValue(type.FullName, out index)) {
                Debug.LogError("Error: Unable to find the view type with name " + type.FullName);
                return;
            }

            float pitch = 0f, yaw = 0f;
            var characterRotation = Quaternion.identity;
            // ViewType will be null on startup.
            if (m_ViewType != null && m_Character != null && Application.isPlaying) {
                pitch = m_ViewType.Pitch;
                yaw = m_ViewType.Yaw;
                characterRotation = m_ViewType.CharacterRotation;
                m_ViewType.ChangeViewType(false, 0, yaw, characterRotation);

                EventHandler.ExecuteEvent(m_GameObject, "OnCameraChangeViewTypes", m_ViewType, false);
                if (m_OnChangeViewTypesEvent != null) {
                    m_OnChangeViewTypesEvent.Invoke(m_ViewType, false);
                }
            }

            var originalViewType = m_ViewType;
            m_ViewTypeFullName = type.FullName;
            m_ViewType = m_ViewTypes[index];

            // Keep the first/third person view type updated to be able to switch back to the last active type.
            if (m_ViewType.FirstPersonPerspective) {
                m_FirstPersonViewTypeFullName = m_ViewTypeFullName;
                m_FirstPersonViewType = m_ViewType;
            } else {
                m_ThirdPersonViewTypeFullName = m_ViewTypeFullName;
                m_ThirdPersonViewType = m_ViewType;
            }

            // If the original view type is not null then the view type has been changed at runtime. Transition to that new view type.
            if (originalViewType != null && m_Character != null && Application.isPlaying) {
                m_ViewType.ChangeViewType(true, pitch, yaw, characterRotation);

                EventHandler.ExecuteEvent(m_GameObject, "OnChangeViewType", m_ViewType, true);
                if (originalViewType.FirstPersonPerspective != m_ViewType.FirstPersonPerspective) {
                    EventHandler.ExecuteEvent<bool>(m_Character, "OnCameraWillChangePerspectives", m_ViewType.FirstPersonPerspective);
                }

                // Use the transitioner if it exists.
                if (!immediateTransition && m_Transitioner != null) {
                    // StartTransition will return success if the transition is started.
                    if (m_Transitioner.StartTransition(originalViewType, m_ViewType)) {
                        return;
                    } else if (m_Transitioner.IsTransitioning) {
                        m_Transitioner.StopTransition(false);
                    }
                } else {
                    // If there isn't a transitioner then immediately move to the target position.
                    if (m_ViewType.RotatePriority) {
                        KinematicObjectManager.SetCameraRotation(m_KinematicObjectIndex, m_ViewType.Rotate(0, 0, true));
                        KinematicObjectManager.SetCameraPosition(m_KinematicObjectIndex, m_ViewType.Move(true));
                    } else {
                        KinematicObjectManager.SetCameraPosition(m_KinematicObjectIndex, m_ViewType.Move(true));
                        KinematicObjectManager.SetCameraRotation(m_KinematicObjectIndex, m_ViewType.Rotate(0, 0, true));
                    }
                }

                // Execute the perspective event if the transitioner does not exist or is not active. The transitioner will execute this event when it finishes.
                if (originalViewType.FirstPersonPerspective != m_ViewType.FirstPersonPerspective) {
                    EventHandler.ExecuteEvent<bool>(m_Character, "OnCameraChangePerspectives", m_ViewType.FirstPersonPerspective);

                    if (m_OnChangePerspectivesEvent != null) {
                        m_OnChangePerspectivesEvent.Invoke(m_ViewType.FirstPersonPerspective);
                    }
                }
            }
        }

        /// <summary>
        /// Returns an array of serialized view types.
        /// </summary>
        /// <returns>An array of serialized abilities.</returns>
        public ViewType[] GetSerializedViewTypes()
        {
            if (m_ViewTypeData != null && m_ViewTypeData.Length > 0 && (m_ViewTypes == null || m_ViewTypes.Length == 0)) { DeserializeViewTypes(); }
            return m_ViewTypes;
        }

        /// <summary>
        /// The camera has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The KinematicObjectManager is responsible for calling the move method.
            m_KinematicObjectIndex = KinematicObjectManager.RegisterCamera(this);
        }

        /// <summary>
        /// Initialize the character.
        /// </summary>
        private void Start()
        {
            if (m_Character != null) {
                if (m_InitCharacterOnAwake) {
                    // Set m_Character to null to prevent InitializeCharacter from thinking that it previously had a character that it needs to cleanup after.
                    var character = m_Character;
                    InitializeCharacter(null);
                    InitializeCharacter(character);
                }
            } else {
                enabled = false;
            }
        }

        /// <summary>
        /// Initialize the camera to follow the character.
        /// </summary>
        /// <param name="character">The character to initialize. Can be null.</param>
        private void InitializeCharacter(GameObject character)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif

            if (character == m_Character) {
                return;
            }

            // If the character is not null then the previous character should be notified that there is no longer a camera attached.
            if (m_Character != null) {
                EventHandler.ExecuteEvent<CameraController>(m_Character, "OnCharacterAttachCamera", null);
                EventHandler.ExecuteEvent<ILookSource>(m_Character, "OnCharacterAttachLookSource", null);
                EventHandler.UnregisterEvent<Vector3>(m_Character, "OnCameraPositionalForce", AddPositionalForce);
                EventHandler.UnregisterEvent<Vector3>(m_Character, "OnCameraRotationalForce", AddRotationalForce);
                EventHandler.UnregisterEvent<Vector3, Vector3, float>(m_Character, "OnAddSecondaryCameraForce", OnAddSecondaryForce);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
                EventHandler.UnregisterEvent(m_Character, "OnAnimatorSnapped", PositionImmediately);
                EventHandler.UnregisterEvent<Ability, bool>(m_Character, "OnCharacterAbilityActive", OnAbilityActive);
                EventHandler.UnregisterEvent<ItemAbility, bool>(m_Character, "OnCharacterItemAbilityActive", OnItemAbilityActive);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnCameraChangePerspectives", OnChangePerspectives);
                StateManager.LinkGameObjects(m_Character, m_GameObject, false);
            }

            // The character should no longer be using the zoom state if the camera is no longer attached.
            if (m_Zoom && m_Character != null) {
                SetZoom(false);
            }

            // Set the character values.
            if (enabled = (character != null)) {
                m_Character = character;
                m_CharacterTransform = m_Character.transform;
                m_CharacterLocomotion = character.GetCachedComponent<UltimateCharacterLocomotion>();
                m_CharacterInventory = character.GetCachedComponent<InventoryBase>();

                InitializeAnchor();
            } else {
                m_Character = null;
                m_CharacterTransform = null;
                m_CharacterLocomotion = null;
                m_CharacterInventory = null;
            }

            // Notify the view types of the character that is being attached.
            for (int i = 0; i < m_ViewTypes.Length; ++i) {
                m_ViewTypes[i].AttachCharacter(m_Character);
            }

            if (m_Character != null) {
                m_ViewType.ChangeViewType(true, 0, 0, m_CharacterTransform.rotation);
                if (m_ViewType.RotatePriority) {
                    KinematicObjectManager.SetCameraRotation(m_KinematicObjectIndex, m_ViewType.Rotate(0, 0, true));
                    KinematicObjectManager.SetCameraPosition(m_KinematicObjectIndex, m_ViewType.Move(true));
                } else {
                    KinematicObjectManager.SetCameraPosition(m_KinematicObjectIndex, m_ViewType.Move(true));
                    KinematicObjectManager.SetCameraRotation(m_KinematicObjectIndex, m_ViewType.Rotate(0, 0, true));
                }
                m_ViewType.UpdateFieldOfView(true);

                // Registered for any interested events.
                EventHandler.RegisterEvent<Vector3>(m_Character, "OnCameraPositionalForce", AddPositionalForce);
                EventHandler.RegisterEvent<Vector3>(m_Character, "OnCameraRotationalForce", AddRotationalForce);
                EventHandler.RegisterEvent<Vector3, Vector3, float>(m_Character, "OnAddSecondaryCameraForce", OnAddSecondaryForce);
                EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
                EventHandler.RegisterEvent(m_Character, "OnAnimatorSnapped", PositionImmediately);
                EventHandler.RegisterEvent<Ability, bool>(m_Character, "OnCharacterAbilityActive", OnAbilityActive);
                EventHandler.RegisterEvent<ItemAbility, bool>(m_Character, "OnCharacterItemAbilityActive", OnItemAbilityActive);
                EventHandler.RegisterEvent<bool>(m_Character, "OnCameraChangePerspectives", OnChangePerspectives);

                // Notify the camera components of the attached character.
                EventHandler.ExecuteEvent<GameObject>(m_GameObject, "OnCameraAttachCharacter", character);

                // Notify the character of the attached camera.
                EventHandler.ExecuteEvent<CameraController>(m_Character, "OnCharacterAttachCamera", this);
                EventHandler.ExecuteEvent<ILookSource>(m_Character, "OnCharacterAttachLookSource", this);
                EventHandler.ExecuteEvent<bool>(m_Character, "OnCameraWillChangePerspectives", m_ViewType.FirstPersonPerspective);
                EventHandler.ExecuteEvent<bool>(m_Character, "OnCameraChangePerspectives", m_ViewType.FirstPersonPerspective);

                StateManager.LinkGameObjects(m_Character, m_GameObject, true);

#if UNITY_EDITOR
                // Show a warning if the movement type isn't what is recommended for the current view type. Only show this when the character is initially attached
                // because a mismatch is most likely when initially setting up the character.
                var recommendedMovementTypes = m_ViewType.GetType().GetCustomAttributes(typeof(RecommendedMovementType), true);
                if (recommendedMovementTypes != null && recommendedMovementTypes.Length > 0) {
                    var movementType = m_CharacterLocomotion.ActiveMovementType;
                    var isRecommendedMovementType = false;
                    for (int i = 0; i < recommendedMovementTypes.Length; ++i) {
                        var recommendedMovementType = recommendedMovementTypes[0] as RecommendedMovementType;
                        if (recommendedMovementType.Type.IsInstanceOfType(movementType)) {
                            isRecommendedMovementType = true;
                            break;
                        }
                    }
                    if (!isRecommendedMovementType) {
                        Debug.LogWarning($"Warning: The {UnityEngineUtility.GetFriendlyName(movementType.GetType())} MovementType is active while the ViewType " +
                                         $"recommends using {UnityEngineUtility.GetFriendlyName((recommendedMovementTypes[0] as RecommendedMovementType).Type)}.");
                    }

                }
#endif
            } else {
                // Notify the camera components of the attached character.
                EventHandler.ExecuteEvent<GameObject>(m_GameObject, "OnCameraAttachCharacter", character);
            }
        }

        /// <summary>
        /// Initialize the anchor Transform.
        /// </summary>
        private void InitializeAnchor()
        {
            // Assign the anchor to the bone transform if auto anchor is enabled. Otherwise use the character's Transform.
            Transform anchor = null;
            if (m_AutoAnchor && (anchor = m_Character.GetComponent<Animator>().GetBoneTransform(m_AutoAnchorBone)) != null) {
                m_Anchor = anchor;
            } else if (m_Anchor != null && !m_Anchor.IsChildOf(m_Character.transform)) {
                m_Anchor = null;
            }

            if (m_Anchor == null) {
                m_Anchor = m_CharacterTransform;
            }
        }

        /// <summary>
        /// Rotates the camera according to the horizontal and vertical movement values.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        public void Rotate(float horizontalMovement, float verticalMovement)
        {
            if (m_CharacterLocomotion.TimeScale == 0 || Time.fixedDeltaTime == 0) {
                return;
            }

            // If a transition is active then move the transition rather then the active view type. The transitioner will move the view type.
            if (m_Transitioner != null && m_Transitioner.IsTransitioning) {
                m_Transform.rotation = m_Transitioner.Rotate(horizontalMovement, verticalMovement, false);
            } else if (m_ViewType.RotatePriority) {
                m_Transform.rotation = m_ViewType.Rotate(horizontalMovement, verticalMovement, false);
            }
        }

        /// <summary>
        /// Moves the camera.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        public void Move(float horizontalMovement, float verticalMovement)
        {
            if (m_CharacterLocomotion.TimeScale == 0 || Time.fixedDeltaTime == 0) {
                return;
            }

            m_ViewType.UpdateFieldOfView(false);

            // If a transition is active then move the transition rather then the active view type. The transitioner will move the view type.
            if (m_Transitioner != null && m_Transitioner.IsTransitioning) {
                m_Transform.position = m_Transitioner.Move(false);
                return;
            }

            if (m_ViewType.RotatePriority) {
                m_Transform.position = m_ViewType.Move(false);
            } else {
                m_Transform.position = m_ViewType.Move(false);
                m_Transform.rotation = m_ViewType.Rotate(horizontalMovement, verticalMovement, false);
            }
        }

        /// <summary>
        /// Adds a positional force to all of the ViewTypes.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public void AddPositionalForce(Vector3 force)
        {
            for (int i = 0; i < m_ViewTypes.Length; ++i) {
                m_ViewTypes[i].AddPositionalForce(force);
            }
        }

        /// <summary>
        /// Adds a rotational force to all of the ViewTypes.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public void AddRotationalForce(Vector3 force)
        {
            for (int i = 0; i < m_ViewTypes.Length; ++i) {
                m_ViewTypes[i].AddRotationalForce(force);
            }
        }

        /// <summary>
        /// Adds a secondary positional and rotational force to the ViewTypes.
        /// </summary>
        /// <param name="positionalForce">The positional force to add.</param>
        /// <param name="rotationalForce">The rotational force to add.</param>
        /// <param name="restAccumulation">The percent of the force to accumulate to the rest value.</param>
        private void OnAddSecondaryForce(Vector3 positionalForce, Vector3 rotationalForce, float restAccumulation)
        {
            for (int i = 0; i < m_ViewTypes.Length; ++i) {
                m_ViewTypes[i].AddSecondaryPositionalForce(positionalForce, restAccumulation);
                m_ViewTypes[i].AddSecondaryRotationalForce(rotationalForce, restAccumulation);
            }
        }

        /// <summary>
        /// Adds a secondary positional force to all of the ViewTypes.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="restAccumulation">The percent of the force to accumulate to the rest value.</param>
        public void AddSecondaryPositionalForce(Vector3 force, float restAccumulation)
        {
            for (int i = 0; i < m_ViewTypes.Length; ++i) {
                m_ViewTypes[i].AddSecondaryPositionalForce(force, restAccumulation);
            }
        }

        /// <summary>
        /// Adds a secondary rotational force to all of the ViewTypes.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="restAccumulation">The percent of the force to accumulate to the rest value.</param>
        public void AddSecondaryRotationalForce(Vector3 force, float restAccumulation)
        {
            for (int i = 0; i < m_ViewTypes.Length; ++i) {
                m_ViewTypes[i].AddSecondaryRotationalForce(force, restAccumulation);
            }
        }

        /// <summary>
        /// Tries to start or stop the camera zoom. The camera may not be able to start if an ability doesn't allow it.
        /// </summary>
        /// <param name="zoom">Should the camera zoom?</param>
        public void TryZoom(bool zoom)
        {
            if (m_Character == null || !m_Character.activeInHierarchy) {
                return;
            }

            // The zoom state may not be able to start. Remember the input so when the game state changes (different ability, item, etc) zoom can try to activate again.
            m_ZoomInput = zoom;

            if (m_ZoomInput) {
                // The camera may not allow zooming.
                if (!m_CanZoom) {
                    SetZoom(false);
                    return;
                }

                // The ViewType may not allow zoomig.
                if (!m_ViewType.CanZoom()) {
                    SetZoom(false);
                    return;
                }

                // The item may not allow zooming.
                if (m_CharacterInventory != null) {
                    for (int i = 0; i < m_CharacterInventory.SlotCount; ++i) {
                        var item = m_CharacterInventory.GetActiveItem(i);
                        if (item != null && !item.CanCameraZoom()) {
                            SetZoom(false);
                            return;
                        }
                    }
                }

                // The character abilities disallow zoom.
                if (m_CharacterLocomotion.ActiveAbilityCount > 0) {
                    for (int i = 0; i < m_CharacterLocomotion.ActiveAbilityCount; ++i) {
                        if (!m_CharacterLocomotion.ActiveAbilities[i].CanCameraZoom()) {
                            SetZoom(false);
                            return;
                        }
                    }
                }
                if (m_CharacterLocomotion.ActiveItemAbilityCount > 0) {
                    for (int i = 0; i < m_CharacterLocomotion.ActiveItemAbilityCount; ++i) {
                        if (!m_CharacterLocomotion.ActiveItemAbilities[i].CanCameraZoom()) {
                            SetZoom(false);
                            return;
                        }
                    }
                }
            }

            // The camera can zoom or unzoom.
            SetZoom(zoom);
        }

        /// <summary>
        /// Sets the zoom state.
        /// </summary>
        /// <param name="zoom">Should the camera zoon?</param>
        private void SetZoom(bool zoom)
        {
            if (zoom == m_Zoom) {
                return;
            }

            m_Zoom = zoom;
            if (!string.IsNullOrEmpty(m_ZoomState)) {
                StateManager.SetState(m_GameObject, m_ZoomState, m_Zoom);
                StateManager.SetState(m_Character, m_ZoomState, m_Zoom);

                if (m_CharacterInventory != null && m_StateAppendItemIdentifierName) {
                    for (int i = 0; i < m_CharacterInventory.SlotCount; ++i) {
                        var item = m_CharacterInventory.GetActiveItem(i);
                        if (item != null && item.IsActive()) {
                            var itemStateName = m_ZoomState + item.ItemDefinition.name;
                            StateManager.SetState(m_GameObject, itemStateName, m_Zoom);
                            StateManager.SetState(m_Character, itemStateName, m_Zoom);
                        }
                    }
                }
            }

            EventHandler.ExecuteEvent<bool>(m_Character, "OnCameraZoom", m_Zoom);
            if (m_OnZoomEvent != null) {
                m_OnZoomEvent.Invoke(m_Zoom);
            }
        }

        /// <summary>
        /// Sets the crosshairs to the specified transform for all of the view types.
        /// </summary>
        /// <param name="crosshairs">The transform of the crosshairs.</param>
        public void SetCrosshairs(Transform crosshairs)
        {
            for (int i = 0; i < m_ViewTypes.Length; ++i) {
                m_ViewTypes[i].SetCrosshairs(crosshairs);
            }
        }

        /// <summary>
        /// Returns the delta rotation caused by the crosshairs.
        /// </summary>
        /// <param name="localSpace">Should the delta rotation be returned in local space?</param>
        /// <returns>The delta rotation caused by the crosshairs.</returns>
        public Quaternion GetCrosshairsDeltaRotation() { return m_ViewType.GetCrosshairsDeltaRotation(); }

#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
        /// <summary>
        /// Toggle between a first and third person perspective.
        /// </summary>
        public void TogglePerspective()
        {
            // The camera can only change perspectives if it is allowed and both the first and third person view types exist.
            if (!m_CanChangePerspectives || m_ThirdPersonViewType == null || m_FirstPersonViewType == null) {
                return;
            }

            SetPerspective(!m_ViewType.FirstPersonPerspective);
        }
#endif

        /// <summary>
        /// Sets the ViewType to a third or first person perspective.
        /// </summary>
        /// <param name="firstPersonPerspective">True if the perspective should be switched to a first person perspective.</param>
        public void SetPerspective(bool firstPersonPerspective)
        {
            SetPerspective(firstPersonPerspective, false);
        }

        /// <summary>
        /// Sets the ViewType to a third or first person perspective.
        /// </summary>
        /// <param name="firstPersonPerspective">True if the perspective should be switched to a first person perspective.</param>
        /// <param name="immediateTransition">Should the ViewType be transitioned immediately?</param>
        public void SetPerspective(bool firstPersonPerspective, bool immediateTransition)
        {
            var viewType = (firstPersonPerspective ? m_FirstPersonViewType : m_ThirdPersonViewType);
            if (viewType != null) {
                SetViewType(viewType.GetType(), immediateTransition);
            }
        }

        /// <summary>
        /// Returns the view type of type T.
        /// </summary>
        /// <typeparam name="T">The type of view type to return.</typeparam>
        /// <returns>The view type of type T. Can be null.</returns>
        public T GetViewType<T>() where T : ViewType
        {
            var type = typeof(T);
            if (m_ViewTypes != null) {
                for (int i = 0; i < m_ViewTypes.Length; ++i) {
                    if (type == m_ViewTypes[i].GetType()) {
                        return m_ViewTypes[i] as T;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// The character's ability has been started or stopped.
        /// </summary>
        /// <param name="ability">The ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnAbilityActive(Ability ability, bool active)
        {
            // When an ability starts or stops it can prevent the camera from zooming.
            TryZoom(m_ZoomInput);
        }

        /// <summary>
        /// The character's item ability has been started or stopped.
        /// </summary>
        /// <param name="itemAbility">The item ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnItemAbilityActive(ItemAbility itemAbility, bool active)
        {
            // When an ability starts or stops it can prevent the camera from zooming.
            TryZoom(m_ZoomInput);
        }

        /// <summary>
        /// The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the camera in a first person perspective?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            // The new perspective may not allow zooming.
            if (m_ZoomInput) {
                TryZoom(m_ZoomInput);
            }
        }

        /// <summary>
        /// Returns the position of the look source.
        /// </summary>
        /// <returns>The position of the look source.</returns>
        public Vector3 LookPosition()
        {
            if (m_Transitioner != null && m_Transitioner.IsTransitioning) {
                return m_Transitioner.LookPosition();
            }
            return m_ViewType.LookPosition();
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>The direction that the character is looking.</returns>
        public Vector3 LookDirection(bool characterLookDirection)
        {
            if (m_Transitioner != null && m_Transitioner.IsTransitioning) {
                return m_Transitioner.LookDirection(characterLookDirection);
            }
            return m_ViewType.LookDirection(characterLookDirection);
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="lookPosition">The position that the character is looking from.</param>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <param name="layerMask">The LayerMask value of the objects that the look direction can hit.</param>
        /// <param name="useRecoil">Should recoil be included in the look direction?</param>
        /// <returns>The direction that the character is looking.</returns>
        public Vector3 LookDirection(Vector3 lookPosition, bool characterLookDirection, int layerMask, bool useRecoil)
        {
            if (m_Transitioner != null && m_Transitioner.IsTransitioning) {
                return m_Transitioner.LookDirection(lookPosition, characterLookDirection, layerMask, useRecoil);
            }
            return m_ViewType.LookDirection(lookPosition, characterLookDirection, layerMask, useRecoil);
        }

        /// <summary>
        /// Positions the camera immediately.
        /// </summary>
        public void PositionImmediately()
        {
            PositionImmediately(true);
        }

        /// <summary>
        /// Positions the camera immediately.
        /// </summary>
        /// <param name="resetViewTypes">Should the view types variables be reset?</param>
        public void PositionImmediately(bool resetViewTypes)
        {
            // If the camera is being positioned immediately then there is no use for the transitioner.
            if (m_Transitioner != null && m_Transitioner.IsTransitioning) {
                m_Transitioner.StopTransition(false);
            }

            if (resetViewTypes) {
                // Reset the view type's variables.
                for (int i = 0; i < m_ViewTypes.Length; ++i) {
                    m_ViewTypes[i].Reset(m_CharacterTransform.rotation);
                }
            }

            m_ViewType.UpdateFieldOfView(true);

            if (m_ViewType.RotatePriority) {
                KinematicObjectManager.SetCameraRotation(m_KinematicObjectIndex, m_ViewType.Rotate(0, 0, true));
                KinematicObjectManager.SetCameraPosition(m_KinematicObjectIndex, m_ViewType.Move(true));
            } else {
                KinematicObjectManager.SetCameraPosition(m_KinematicObjectIndex, m_ViewType.Move(true));
                KinematicObjectManager.SetCameraRotation(m_KinematicObjectIndex, m_ViewType.Rotate(0, 0, true));
            }
        }

        /// <summary>
        /// The character's position or rotation has been teleported.
        /// </summary>
        /// <param name="snapAnimator">Should the animator be snapped?</param>
        private void OnImmediateTransformChange(bool snapAnimator)
        {
            PositionImmediately(snapAnimator);
        }

        /// <summary>
        /// The camera has been disabled.
        /// </summary>
        private void OnDisable()
        {
            KinematicObjectManager.UnregisterCamera(m_KinematicObjectIndex);
            m_KinematicObjectIndex = -1;
        }

        /// <summary>
        /// The camera has been destroyed. Unregister for an registered events.
        /// </summary>
        private void OnDestroy()
        {
            InitializeCharacter(null);

            for (int i = 0; i < m_ViewTypes.Length; ++i) {
                m_ViewTypes[i].OnDestroy();
            }
        }
    }
}