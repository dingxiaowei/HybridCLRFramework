/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Motion;
    using Opsive.UltimateCharacterController.StateSystem;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The FirstPerson ViewType allows the camera to be placed in a first person perspective.
    /// </summary>
    public abstract class FirstPerson : ViewType
    {
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
        /// <summary>
        /// Specifies how the overlay objects are rendered.
        /// </summary>
        public enum ObjectOverlayRenderType
        {
            SecondCamera,   // Use a second stacked camera to ensure the overlay objects do no clip with any other objects.
            RenderPipeline, // Use the LWRP/URP render pipeline to ensure the overlay objects do no clip with any other objects.
            None            // No special rendering for the overlay objects.
        }
#endif

        [Tooltip("The distance that the character should look ahead.")]
        [SerializeField] protected float m_LookDirectionDistance = 100;
        
        [Tooltip("The offset between the anchor and the camera.")]
        [SerializeField] protected Vector3 m_LookOffset = new Vector3(0, .1f, 0.27f);
        [Tooltip("Amount to adjust the camera position by when the character is looking down.")]
        [SerializeField] protected Vector3 m_LookDownOffset = new Vector3(0, 0, 0.28f);
        [Tooltip("The culling mask of the camera.")]
        [SerializeField] protected LayerMask m_CullingMask = ~(1 << LayerManager.Overlay);
        [Tooltip("The field of view of the main camera.")]
        [SerializeField] protected float m_FieldOfView = 70f;
        [Tooltip("The damping time of the field of view angle when changed.")]
        [SerializeField] protected float m_FieldOfViewDamping = 0.2f;
        [Tooltip("Specifies the position offset from the camera that the first person objects should render.")]
        [SerializeField] protected Vector3 m_FirstPersonPositionOffset;
        [Tooltip("Specifies the rotation offset from the camera that the first person objects should render.")]
        [SerializeField] protected Vector3 m_FirstPersonRotationOffset;
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
        [Tooltip("Specifies how the overlay objects are rendered.")]
        [SerializeField] protected ObjectOverlayRenderType m_OverlayRenderType = ObjectOverlayRenderType.SecondCamera;
#else
        [Tooltip("Should the first person camera be used?")]
        [SerializeField] protected bool m_UseFirstPersonCamera = true;
#endif
        [Tooltip("A reference to the first person camera.")]
        [SerializeField] protected UnityEngine.Camera m_FirstPersonCamera;
        [Tooltip("The culling mask of the first person objects.")]
        [SerializeField] protected LayerMask m_FirstPersonCullingMask = 1 << LayerManager.Overlay;
        [Tooltip("Should the first person camera's field of view be synchronized with the main camera?")]
        [SerializeField] protected bool m_SynchronizeFieldOfView = true;
        [Tooltip("Specifies the field of view for the first person camera.")]
        [SerializeField] protected float m_FirstPersonFieldOfView = 30f;
        [Tooltip("The damping time of the field of view angle when changed.")]
        [SerializeField] protected float m_FirstPersonFieldOfViewDamping = 0.2f;

        [Tooltip("A vertical limit intended to prevent the camera from intersecting with the character.")]
        [SerializeField] protected float m_PositionLowerVerticalLimit = 0.25f;
        [Tooltip("Determines how much the camera will be pushed down when the player falls onto a surface.")]
        [SerializeField] protected float m_PositionFallImpact = 2f;
        [Tooltip("The number of frames that the fall impact force should be applied.")]
        [SerializeField] protected int m_PositionFallImpactSoftness = 4;
        [Tooltip("Rotates the camera depending on the sideways local velocity of the character, resulting in the camera leaning into or away from its sideways movement direction.")]
        [SerializeField] protected float m_RotationStrafeRoll = 0.01f;
        [Tooltip("Determines how much the camera will roll when the player falls onto a surface.")]
        [SerializeField] protected float m_RotationFallImpact = 0.1f;
        [Tooltip("The number of frames that the fall impact force should be applied.")]
        [SerializeField] protected int m_RotationFallImpactSoftness = 1;

        [Tooltip("The positional spring used for regular movement.")]
        [SerializeField] protected Spring m_PositionSpring = new Spring();
        [Tooltip("The rotational spring used for regular movement.")]
        [SerializeField] protected Spring m_RotationSpring = new Spring();
        [Tooltip("The positional spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryPositionSpring = new Spring();
        [Tooltip("The rotational spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryRotationSpring = new Spring();

        [Tooltip("The minimum pitch angle (in degrees).")]
        [SerializeField] protected float m_MinPitchLimit = -72;
        [Tooltip("The maximum pitch angle (in degrees).")]
        [SerializeField] protected float m_MaxPitchLimit = 72;

        [Tooltip("The rate that the camera changes its position while the character is moving.")]
        [SerializeField] protected Vector3 m_BobPositionalRate = new Vector3(0.0f, 1.4f, 0.0f);
        [Tooltip("The strength of the positional camera bob. Determines how far the camera swings in each respective direction.")]
        [SerializeField] protected Vector3 m_BobPositionalAmplitude = new Vector3(0.0f, 0.35f, 0.0f);
        [Tooltip("The rate that the camera changes its roll rotation value while the character is moving.")]
        [SerializeField] protected float m_BobRollRate = 0.9f;
        [Tooltip("The strength of the roll within the camera bob. Determines how far the camera tilts from left to right.")]
        [SerializeField] protected float m_BobRollAmplitude = 1.7f;
        [Tooltip("This tweaking feature is useful if the bob motion gets out of hand after changing character velocity.")]
        [SerializeField] protected float m_BobInputVelocityScale = 1;
        [Tooltip("A cap on the velocity value from the bob function, preventing the camera from flipping out when the character travels at excessive speeds.")]
        [SerializeField] protected float m_BobMaxInputVelocity = 1000;
        [Tooltip("A trough should only occur when the bob vertical offset is less then the specified value.")]
        [SerializeField] protected float m_BobMinTroughVerticalOffset = -0.01f;
        [Tooltip("The amount of force to add when the bob has reached its lowest point. This can be used to add a shaking effect to the camera to mimick a giant walking.")]
        [SerializeField] protected Vector3 m_BobTroughForce = new Vector3(0.0f, 0.0f, 0.0f);
        [Tooltip("Determines whether the bob should stay in effect only when the character is on the ground.")]
        [SerializeField] protected bool m_BobRequireGroundContact = true;

        [Tooltip("The speed that the camera should shake.")]
        [SerializeField] protected float m_ShakeSpeed = 0.1f;
        [Tooltip("The strength of the shake. Determines how much the camera will tilt.")]
        [SerializeField] protected Vector3 m_ShakeAmplitude = new Vector3(10, 10);

        [Tooltip("Number of head offset values to average together. The head offset value specifies how reactive the camera is to head movements.")]
        [SerializeField] protected int m_SmoothHeadOffsetSteps = 2;
        [Tooltip("The radius of the camera's collision sphere to prevent it from clipping with other objects.")]
        [SerializeField] protected float m_CollisionRadius = 0.05f;
        [Tooltip("Should the camera rotate as the character's head rotates?")]
        [SerializeField] protected bool m_RotateWithHead;
        
        public Vector3 LookOffset { get { return m_LookOffset; }
            set
            {
                m_LookOffset = value;
                if (m_PositionSpring != null && m_CameraController != null) {
                    InitializePositionSpringValue();
                }
            }
        }
        public Vector3 LookDownOffset { get { return m_LookDownOffset; } set { m_LookDownOffset = value; } }
        public LayerMask CullingMask { get { return m_CullingMask; } set {
                if (m_CullingMask != value) {
                    m_CullingMask = value;
                    UpdateFirstPersonCamera(m_CharacterLocomotion.FirstPersonPerspective);
                }
            } }
        public float FieldOfView { get { return m_FieldOfView; } set { m_FieldOfView = value; } }
        public float FieldOfViewDamping { get { return m_FieldOfViewDamping; } set { m_FieldOfViewDamping = value; } }
        public Vector3 FirstPersonPositionOffset { get { return m_FirstPersonPositionOffset; }
            set
            {
                m_FirstPersonPositionOffset = value;
                if (m_FirstPersonCameraTransform != null) {
                    m_FirstPersonCameraTransform.localPosition = m_FirstPersonPositionOffset;
                }
            }
        }
        public Vector3 FirstPersonRotationOffset { get { return m_FirstPersonRotationOffset; }
            set
            {
                m_FirstPersonRotationOffset = value;
                if (m_FirstPersonCameraTransform != null) {
                    m_FirstPersonCameraTransform.localRotation = Quaternion.Euler(m_FirstPersonRotationOffset);
                }
            }
        }
        public override float LookDirectionDistance { get { return m_LookDirectionDistance; } }
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
        [NonSerialized] public ObjectOverlayRenderType OverlayRenderType { get { return m_OverlayRenderType; } set { m_OverlayRenderType = value; } }
#else
        public bool UseFirstPersonCamera { get { return m_UseFirstPersonCamera; }
            set
            {
                m_UseFirstPersonCamera = value;
                if (m_FirstPersonCamera == null || m_CharacterLocomotion == null) {
                    return;
                }
                UpdateFirstPersonCamera(m_CharacterLocomotion.FirstPersonPerspective);
            }
        }
#endif
        [NonSerialized] public UnityEngine.Camera FirstPersonCamera { get { return m_FirstPersonCamera; } set { m_FirstPersonCamera = value; } }
        public LayerMask FirstPersonCullingMask { get { return m_FirstPersonCullingMask; }
            set {
                if (m_FirstPersonCamera != null &&  m_FirstPersonCullingMask != value) {
                    m_FirstPersonCullingMask = value;
                    m_FirstPersonCamera.cullingMask = m_FirstPersonCullingMask;
                }
            }
        }
        public bool SynchronizeFieldOfView { get { return m_SynchronizeFieldOfView; } set { m_SynchronizeFieldOfView = value; } }
        public float FirstPersonFieldOfView { get { return m_FirstPersonFieldOfView; } set { m_FirstPersonFieldOfView = value; } }
        public float FirstPersonFieldOfViewDamping { get { return m_FirstPersonFieldOfViewDamping; } set { m_FirstPersonFieldOfViewDamping = value; } }
        public float PositionLowerVerticalLimit { get { return m_PositionLowerVerticalLimit; }
            set {
                m_PositionLowerVerticalLimit = value;
                if (m_PositionSpring != null && m_CameraController != null) {
                    InitializePositionSpringValue();
                }
            } }
        public float PositionFallImpact { get { return m_PositionFallImpact; } set { m_PositionFallImpact = value; } }
        public int PositionFallImpactSoftness { get { return m_PositionFallImpactSoftness; } set { m_PositionFallImpactSoftness = value; } }
        public float RotationStrafeRoll { get { return m_RotationStrafeRoll; } set { m_RotationStrafeRoll = value; } }
        public float RotationFallImpact { get { return m_RotationFallImpact; } set { m_RotationFallImpact = value; } }
        public int RotationFallImpactSoftness { get { return m_RotationFallImpactSoftness; } set { m_RotationFallImpactSoftness = value; } }
        public Spring PositionSpring { get { return m_PositionSpring; }
            set {
                m_PositionSpring = value;
                if (m_CameraController != null) { m_PositionSpring.Initialize(false, true); InitializePositionSpringValue(); }
            } }
        public Spring RotationSpring { get { return m_RotationSpring; }
            set {
                m_RotationSpring = value;
                if (m_RotationSpring != null) { m_RotationSpring.Initialize(true, true); }
            } }
        public Spring SecondaryPositionSpring { get { return m_SecondaryPositionSpring; }
            set {
                m_SecondaryPositionSpring = value;
                if (m_SecondaryPositionSpring != null) { m_SecondaryPositionSpring.Initialize(false, true); }
            } }
        public Spring SecondaryRotationSpring { get { return m_SecondaryRotationSpring; }
            set {
                m_SecondaryRotationSpring = value;
                if (m_SecondaryRotationSpring != null) { m_SecondaryRotationSpring.Initialize(true, true); }
            } }
        public float MinPitchLimit { get { return m_MinPitchLimit; } set { m_MinPitchLimit = value; } }
        public float MaxPitchLimit { get { return m_MaxPitchLimit; } set { m_MaxPitchLimit = value; } }
        public Vector3 BobPositionalRate { get { return m_BobPositionalRate; } set { m_BobPositionalRate = value; } }
        public Vector3 BobPositionalAmplitude { get { return m_BobPositionalAmplitude; } set { m_BobPositionalAmplitude = value; } }
        public float BobRollRate { get { return m_BobRollRate; } set { m_BobRollRate = value; } }
        public float BobRollAmplitude { get { return m_BobRollAmplitude; } set { m_BobRollAmplitude = value; } }
        public float BobInputVelocityScale { get { return m_BobInputVelocityScale; } set { m_BobInputVelocityScale = value; } }
        public float BobMaxInputVelocity { get { return m_BobMaxInputVelocity; } set { m_BobMaxInputVelocity = value; } }
        public float BobMinTroughVerticalOffset { get { return m_BobMinTroughVerticalOffset; } set { m_BobMinTroughVerticalOffset = value; } }
        public Vector3 BobTroughForce { get { return m_BobTroughForce; } set { m_BobTroughForce = value; } }
        public bool BobRequireGroundContact { get { return m_BobRequireGroundContact; } set { m_BobRequireGroundContact = value; } }
        public float ShakeSpeed { get { return m_ShakeSpeed; } set { m_ShakeSpeed = value; } }
        public Vector3 ShakeAmplitude { get { return m_ShakeAmplitude; } set { m_ShakeAmplitude = value; } }
        public int SmoothHeadOffsetSteps { get { return m_SmoothHeadOffsetSteps; } set { 
                m_SmoothHeadOffsetSteps = value;
                if (value == 0) {
                    m_SmoothHeadBufferCount = 0;
                }
            } }
        public bool RotateWithHead { get { return m_RotateWithHead; } set { m_RotateWithHead = value; } }

        private UnityEngine.Camera m_Camera;
        private Transform m_CrosshairsTransform;
        private AimAssist m_AimAssist;
        private Transform m_FirstPersonCameraTransform;

        protected float m_Pitch;
        protected float m_Yaw;
        private float m_PrevBobSpeed;
        protected Vector3 m_Shake;
        protected Quaternion m_CharacterRotation;
        private Quaternion m_CharacterPlatformRotationOffset = Quaternion.identity;
        private Quaternion m_PlatformRotation = Quaternion.identity;
        private bool m_AppendingZoomState;
        private Vector3 m_CrosshairsLocalPosition;
        private Quaternion m_CrosshairsDeltaRotation;

        private Transform m_CharacterAnchor;
        private Vector3 m_CharacterAnchorOffset;
        private Quaternion m_CharacterAnchorRotation;
        private Vector3[] m_SmoothHeadOffsetBuffer;
        private int m_SmoothHeadBufferIndex;
        private int m_SmoothHeadBufferCount;
        private RaycastHit m_RaycastHit;
        private float m_FieldOfViewChangeTime;
        private float m_FirstPersonFieldOfViewChangeTime;
        private float m_BobVerticalOffset = float.MaxValue;
        private bool m_BobVerticalOffsetDecreasing = false;
        private float m_VerticalOffsetAdjustment;

        private Vector3 m_PrevPositionSpringValue;
        private Vector3 m_PrevPositionSpringVelocity;
        private Vector3 m_PrevRotationSpringValue;
        private Vector3 m_PrevRotationSpringVelocity;
        private Vector3 m_PrevSecondaryPositionSpringValue;
        private Vector3 m_PrevSecondaryPositionSpringVelocity;
        private Vector3 m_PrevSecondaryRotationSpringValue;
        private Vector3 m_PrevSecondaryRotationSpringVelocity;
        private float m_PrevFieldOfViewDamping;
        private float m_PrevFirstPersonFieldOfViewDamping;
        private int m_StateChangeFrame = -1;

        public override float Pitch { get { return m_Pitch; } }
        public override float Yaw { get { return m_Yaw; } }
        public override Quaternion CharacterRotation { get { return m_CharacterRotation; } }
        public override bool FirstPersonPerspective { get { return true; } }
        public Vector3 Shake { get { return m_Shake; } }

        /// <summary>
        /// Initializes the view type to the specified camera controller.
        /// </summary>
        /// <param name="cameraController">The camera controller to initialize the view type to.</param>
        public override void Initialize(CameraController cameraController)
        {
            base.Initialize(cameraController);

            m_Camera = cameraController.gameObject.GetCachedComponent<UnityEngine.Camera>();
            m_AimAssist = m_GameObject.GetCachedComponent<AimAssist>();

            m_Camera.depth = 0;
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
            if (m_OverlayRenderType != ObjectOverlayRenderType.None) {
#else
            if (m_UseFirstPersonCamera) {
#endif
                m_Camera.cullingMask &= m_CullingMask;
            }

            // Setup the overlay camera.
            if (m_FirstPersonCamera != null) {
                if (!UnityEngine.XR.XRSettings.enabled) {
                    m_Camera.fieldOfView = m_FieldOfView;
                    m_FirstPersonCamera.fieldOfView = m_SynchronizeFieldOfView ? m_FieldOfView : m_FirstPersonFieldOfView;
                }
                m_FirstPersonCamera.clearFlags = CameraClearFlags.Depth;
                m_FirstPersonCamera.cullingMask = m_FirstPersonCullingMask;
                m_FirstPersonCamera.depth = m_Camera.depth + 1;
                m_FirstPersonCamera.rect = m_Camera.rect;
                m_FirstPersonCameraTransform = m_FirstPersonCamera.transform;
                m_FirstPersonCameraTransform.parent = m_Transform;
                m_FirstPersonCameraTransform.localPosition = m_FirstPersonPositionOffset;
                m_FirstPersonCameraTransform.localRotation = Quaternion.Euler(m_FirstPersonRotationOffset);
            }

            // Using the buffer is optional.
            if (m_SmoothHeadOffsetSteps > 0) {
                m_SmoothHeadOffsetBuffer = new Vector3[m_SmoothHeadOffsetSteps];
            }

            // Initialize the springs.
            m_PositionSpring.Initialize(false, false);
            m_RotationSpring.Initialize(true, true);
            m_SecondaryPositionSpring.Initialize(false, false);
            m_SecondaryRotationSpring.Initialize(true, true);
            InitializePositionSpringValue();
            m_PositionSpring.Reset();

            // Register for any interested events.
            EventHandler.RegisterEvent(m_GameObject, "OnAnchorOffsetUpdated", InitializePositionSpringValue);
            EventHandler.RegisterEvent<ViewType, bool>(m_GameObject, "OnCameraChangeViewTypes", OnChangeViewType);
        }

        /// <summary>
        /// Initializes the position spring value.
        /// </summary>
        private void InitializePositionSpringValue()
        {
            m_PositionSpring.RestValue = m_CameraController.AnchorOffset + m_LookOffset;
            var value = m_PositionSpring.MinValue;
            value.y = m_PositionSpring.RestValue.y - m_PositionLowerVerticalLimit;
            m_PositionSpring.MinValue = value;
        }

        /// <summary>
        /// Attaches the camera to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the camera to.</param>
        public override void AttachCharacter(GameObject character)
        {
            // Unregister from any events on the previous character.
            if (m_Character != null) {
                EventHandler.UnregisterEvent<Transform>(m_Character, "OnCharacterChangeMovingPlatforms", OnCharacterChangeMovingPlatforms);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnCameraChangePerspectives", UpdateFirstPersonCamera);
                EventHandler.UnregisterEvent<float>(m_Character, "OnCharacterLand", OnCharacterLand);
                EventHandler.UnregisterEvent<float, float, float>(m_Character, "OnCharacterLean", OnCharacterLean);
                EventHandler.UnregisterEvent<float>(m_Character, "OnHeightChangeAdjustHeight", AdjustVerticalOffset);
                m_CharacterAnchor = null;
            }

            base.AttachCharacter(character);

            // Initialize the camera with the new character.
            if (m_Character != null) {
                var characterAnimator = m_Character.GetCachedComponent<Animator>();
                if (characterAnimator != null && m_SmoothHeadOffsetBuffer != null) {
                    m_CharacterAnchor = characterAnimator.GetBoneTransform(HumanBodyBones.Neck);
                    // If the neck doesn't exist then try to get the head.
                    if (m_CharacterAnchor == null) {
                        m_CharacterAnchor = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
                    }
                    if (m_CharacterAnchor != null) {
                        m_CharacterAnchorOffset = m_CharacterTransform.InverseTransformPoint(m_CharacterAnchor.position);
                    }
                } else {
                    EventHandler.RegisterEvent<float>(m_Character, "OnHeightChangeAdjustHeight", AdjustVerticalOffset);
                }
                m_SmoothHeadBufferIndex = -1;
                m_SmoothHeadBufferCount = 0;

                EventHandler.RegisterEvent<Transform>(m_Character, "OnCharacterChangeMovingPlatforms", OnCharacterChangeMovingPlatforms);
                EventHandler.RegisterEvent<bool>(m_Character, "OnCameraChangePerspectives", UpdateFirstPersonCamera);
                EventHandler.RegisterEvent<float>(m_Character, "OnCharacterLand", OnCharacterLand);
                EventHandler.RegisterEvent<float, float, float>(m_Character, "OnCharacterLean", OnCharacterLean);
            }
        }

        /// <summary>
        /// The view type has changed.
        /// </summary>
        /// <param name="activate">Should the current view type be activated?</param>
        /// <param name="pitch">The pitch of the camera (in degrees).</param>
        /// <param name="yaw">The yaw of the camera (in degrees).</param>
        /// <param name="characterRotation">The rotation of the character.</param>
        public override void ChangeViewType(bool activate, float pitch, float yaw, Quaternion characterRotation)
        {
            if (activate) {
                m_Pitch = pitch;
                m_Yaw = yaw;
                m_CharacterRotation = characterRotation;
                if (m_CharacterLocomotion.Platform != null) {
                    UpdatePlatformRotationOffset(m_CharacterLocomotion.Platform);
                }

                UpdateFirstPersonCamera(m_CharacterLocomotion.FirstPersonPerspective);
            }
        }

        /// <summary>
        /// Reset the ViewType's variables.
        /// </summary>
        /// <param name="characterRotation">The rotation of the character.</param>
        public override void Reset(Quaternion characterRotation)
        {
            m_Pitch = 0;
            m_Yaw = 0;
            m_Shake = Vector3.zero;
            m_CharacterRotation = characterRotation;
            m_BobVerticalOffset = float.MaxValue;
            m_BobVerticalOffsetDecreasing = false;
            m_PrevBobSpeed = 0;
            if (m_CharacterLocomotion.Platform != null) {
                UpdatePlatformRotationOffset(m_CharacterLocomotion.Platform);
            }
            if (m_CharacterAnchor != null) {
                m_CharacterAnchorRotation = MathUtility.InverseTransformQuaternion(m_CharacterRotation, m_CharacterAnchor.rotation);
            }

            InitializePositionSpringValue();
            m_PositionSpring.Reset();
            m_RotationSpring.Reset();
            m_SecondaryPositionSpring.Reset();
            m_SecondaryRotationSpring.Reset();

            // The head position should be reset to the current values.
            m_SmoothHeadBufferIndex = -1;
            m_SmoothHeadBufferCount = 0;
            UpdateHeadOffset();
        }

        /// <summary>
        /// Sets the crosshairs to the specified transform.
        /// </summary>
        /// <param name="crosshairs">The transform of the crosshairs.</param>
        public override void SetCrosshairs(Transform crosshairs)
        {
            m_CrosshairsTransform = crosshairs;

            if (m_CrosshairsTransform != null) {
                var screenPoint = RectTransformUtility.WorldToScreenPoint(null, m_CrosshairsTransform.position);
                m_CrosshairsDeltaRotation = Quaternion.LookRotation(m_Camera.ScreenPointToRay(screenPoint).direction, m_Transform.up) * Quaternion.Inverse(m_Transform.rotation);
                m_CrosshairsLocalPosition = m_CrosshairsTransform.localPosition;
            }
        }

        /// <summary>
        /// Returns the delta rotation caused by the crosshairs.
        /// </summary>
        /// <returns>The delta rotation caused by the crosshairs.</returns>
        public override Quaternion GetCrosshairsDeltaRotation()
        {
            if (m_CrosshairsTransform == null) {
                return Quaternion.identity;
            }

            // The crosshairs direction should only be updated when it changes.
            if (m_CrosshairsLocalPosition != m_CrosshairsTransform.localPosition) {
                var screenPoint = RectTransformUtility.WorldToScreenPoint(null, m_CrosshairsTransform.position);
                m_CrosshairsDeltaRotation = Quaternion.LookRotation(m_Camera.ScreenPointToRay(screenPoint).direction, m_Transform.up) * Quaternion.Inverse(m_Transform.rotation);
                m_CrosshairsLocalPosition = m_CrosshairsTransform.localPosition;
            }

            return m_CrosshairsDeltaRotation;
        }

        /// <summary>
        /// The moving platform object has changed.
        /// </summary>
        /// <param name="movingPlatform">The moving platform to set. Can be null.</param>
        private void OnCharacterChangeMovingPlatforms(Transform movingPlatform)
        {
            if (movingPlatform != null) {
                UpdatePlatformRotationOffset(movingPlatform);
            } else {
                m_PlatformRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Updates the Character Platform Rotation Offset variable.
        /// </summary>
        /// <param name="platform">The platform that the character is on top of.</param>
        private void UpdatePlatformRotationOffset(Transform platform)
        {
            m_CharacterPlatformRotationOffset = m_CharacterRotation * Quaternion.Inverse(platform.rotation);
            if (!m_CharacterLocomotion.AlignToGravity) {
                // Only the local y rotation should affect the character's rotation.
                var localPlatformRotationOffset = MathUtility.InverseTransformQuaternion(m_CharacterRotation, m_CharacterPlatformRotationOffset).eulerAngles;
                localPlatformRotationOffset.x = localPlatformRotationOffset.z = 0;
                m_CharacterPlatformRotationOffset = MathUtility.TransformQuaternion(m_CharacterRotation, Quaternion.Euler(localPlatformRotationOffset));
            }
        }

        /// <summary>
        /// Updates the first person camera and culling mask depending on if the first person camera is in use.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        private void UpdateFirstPersonCamera(bool firstPersonPerspective)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
            if (m_OverlayRenderType == ObjectOverlayRenderType.None) {
#else
            if (!m_UseFirstPersonCamera) {
#endif
                return;
            }


            if (firstPersonPerspective) {
                if (m_FirstPersonCamera != null) {
                    m_FirstPersonCamera.gameObject.SetActive(true);
                }
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
                if (m_OverlayRenderType == ObjectOverlayRenderType.RenderPipeline) {
                    m_Camera.cullingMask |= m_FirstPersonCullingMask;
                }
#endif
            } else {
                if (m_FirstPersonCamera != null) {
                    m_FirstPersonCamera.gameObject.SetActive(false);
                }
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
                if (m_OverlayRenderType == ObjectOverlayRenderType.RenderPipeline) {
                    m_Camera.cullingMask &= ~m_FirstPersonCullingMask;
                }
#endif
            }
        }

        /// <summary>
        /// The view type has changed.
        /// </summary>
        /// <param name="viewType">The ViewType that was activated or deactivated.</param>
        /// <param name="activate">Should the current view type be activated?</param>
        private void OnChangeViewType(ViewType viewType, bool activate)
        {
            if (activate || viewType == this) {
                return;
            }

            // Apply the spring/shake values so when switching between first person view types it is a continuous motion.
            if (viewType is FirstPerson) {
                var firstPersonViewType = (viewType as FirstPerson);
                m_PositionSpring.Value = firstPersonViewType.PositionSpring.Value;
                m_PositionSpring.Velocity = firstPersonViewType.PositionSpring.Velocity;
                m_RotationSpring.Value = firstPersonViewType.RotationSpring.Value;
                m_RotationSpring.Velocity = firstPersonViewType.RotationSpring.Velocity;
                m_Shake = firstPersonViewType.Shake;
            }
        }

        /// <summary>
        /// Updates the camera field of view.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        public override void UpdateFieldOfView(bool immediateUpdate)
        {
            if (m_Camera.fieldOfView != m_FieldOfView) {
                var zoom = (immediateUpdate || m_FieldOfViewDamping == 0) ? 1 : ((Time.time - m_FieldOfViewChangeTime) / (m_FieldOfViewDamping / m_CharacterLocomotion.TimeScale));
                m_Camera.fieldOfView = Mathf.SmoothStep(m_Camera.fieldOfView, m_FieldOfView, zoom);
            }
            if (m_FirstPersonCamera != null) {
                if (m_SynchronizeFieldOfView) {
                    m_FirstPersonCamera.fieldOfView = m_Camera.fieldOfView;
                } else if (m_FirstPersonCamera.fieldOfView != m_FirstPersonFieldOfView) {
                    var zoom = immediateUpdate ? 1 : ((Time.time - m_FirstPersonFieldOfViewChangeTime) / (m_FirstPersonFieldOfViewDamping / m_CharacterLocomotion.TimeScale));
                    m_FirstPersonCamera.fieldOfView = Mathf.SmoothStep(m_FirstPersonCamera.fieldOfView, m_FirstPersonFieldOfView, zoom);
                }
            }
        }

        /// <summary>
        /// Rotates the camera according to the horizontal and vertical movement values.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediateUpdate)
        {
            UpdateShakes();

            // Rotate with the moving platform.
            if (m_CharacterLocomotion.Platform != null) {
                m_PlatformRotation = MathUtility.InverseTransformQuaternion(m_CharacterLocomotion.Platform.rotation, m_CharacterPlatformRotationOffset) *
                    Quaternion.Inverse(MathUtility.InverseTransformQuaternion(m_CharacterLocomotion.Platform.rotation, m_CharacterRotation *
                    Quaternion.Inverse(m_CharacterLocomotion.Platform.rotation)));
                if (!m_CharacterLocomotion.AlignToGravity) {
                    // Only the local y rotation should affect the character's rotation.
                    var localPlatformTorque = MathUtility.InverseTransformQuaternion(m_CharacterTransform.rotation, m_PlatformRotation).eulerAngles;
                    localPlatformTorque.x = localPlatformTorque.z = 0;
                    m_PlatformRotation = MathUtility.TransformQuaternion(m_CharacterTransform.rotation, Quaternion.Euler(localPlatformTorque));
                }
                m_CharacterRotation *= m_PlatformRotation;
            }

            // The camera should always stay aligned to the character's up direction.
            if (m_CharacterLocomotion.AlignToGravity) {
                var localRotation = MathUtility.InverseTransformQuaternion(m_CharacterTransform.rotation, m_CharacterRotation).eulerAngles;
                localRotation.x = localRotation.z = 0;
                m_CharacterRotation = MathUtility.TransformQuaternion(m_CharacterTransform.rotation, Quaternion.Euler(localRotation));
            }

            // Remember the offset so the delta can be compared the next update.
            if (m_CharacterLocomotion.Platform != null) {
                UpdatePlatformRotationOffset(m_CharacterLocomotion.Platform);
            }

            // Update the rotation. The pitch may have a limit.
            if (Mathf.Abs(m_MinPitchLimit - m_MaxPitchLimit) < 180) {
                m_Pitch = MathUtility.ClampAngle(m_Pitch, -verticalMovement, m_MinPitchLimit, m_MaxPitchLimit);
            } else {
                m_Pitch -= verticalMovement;
            }

            // Prevent the values from getting too large.
            m_Pitch = MathUtility.ClampInnerAngle(m_Pitch);
            m_Yaw = MathUtility.ClampInnerAngle(m_Yaw);

            // If aim assist has a target then the camera should look in the specified direction.
            if (m_AimAssist != null) {
                m_AimAssist.UpdateBreakForce(Mathf.Abs(horizontalMovement) + Mathf.Abs(verticalMovement));
                if (m_AimAssist.HasTarget()) {
                    var rotation = MathUtility.TransformQuaternion(m_CharacterRotation, Quaternion.Euler(m_Pitch, m_Yaw, 0));
                    var assistRotation = rotation * MathUtility.InverseTransformQuaternion(rotation, m_AimAssist.TargetRotation(rotation));
                    // Set the pitch and yaw so when the target is lost the view type won't snap back to the previous rotation value.
                    var localAssistRotation = MathUtility.InverseTransformQuaternion(m_CharacterRotation, assistRotation).eulerAngles;
                    m_Pitch = MathUtility.ClampInnerAngle(localAssistRotation.x);
                    m_Yaw = MathUtility.ClampInnerAngle(localAssistRotation.y);
                }
            }

            var headRotation = Quaternion.identity;
            if (m_RotateWithHead) {
                headRotation = MathUtility.InverseTransformQuaternion(m_CharacterRotation, m_CharacterAnchor.rotation) * Quaternion.Inverse(m_CharacterAnchorRotation);
                // The camera should only follow the pitch of the head rotation.
                var eulerHeadRotation = headRotation.eulerAngles;
                eulerHeadRotation.y = eulerHeadRotation.z = 0;
                headRotation = Quaternion.Euler(eulerHeadRotation);
            }

            // Return the rotation.
            return MathUtility.TransformQuaternion(m_CharacterRotation, Quaternion.Euler(m_Pitch, m_Yaw, 0)) *
                                                        Quaternion.Euler(m_RotationSpring.Value) *
                                                        Quaternion.Euler(m_SecondaryRotationSpring.Value) * headRotation;
        }

        /// <summary>
        /// Updates the procedular shaking of the camera.
        /// </summary>
        private void UpdateShakes()
        {
            if (m_ShakeSpeed == 0) {
                return;
            }

            // Subtract shake from the last frame or the camera will drift.
            m_Yaw -= m_Shake.x;
            m_Pitch -= m_Shake.y;

            // Apply the new shake.
            m_Shake = Vector3.Scale(SmoothRandom.GetVector3Centered(m_ShakeSpeed), m_ShakeAmplitude);
            m_Yaw += m_Shake.x;
            m_Pitch += m_Shake.y;
            m_RotationSpring.AddForce(Vector3.forward * m_Shake.z * m_CharacterLocomotion.TimeScale * Time.timeScale);
        }

        /// <summary>
        /// Moves the camera according to the current pitch and yaw values.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated position.</returns>
        public override Vector3 Move(bool immediateUpdate)
        {
            UpdateSway();

            UpdateBob();

            UpdateHeadOffset();

            var targetPosition = GetTargetPosition();

            // Adjust the camera position to prevent the body from clipping with the camera's spring-based motions.
            if (m_MaxPitchLimit != 0) {
                var lookDown = Mathf.Max(0, m_Pitch / m_MaxPitchLimit);
                lookDown = Mathf.SmoothStep(0, 1, lookDown);
                targetPosition += m_Transform.TransformDirection(m_LookDownOffset * lookDown);
            }

            // Ensure there aren't any objects obstructing the distance between the anchor offset and the target position.
            var collisionLayerEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            var offset = m_PositionSpring.Value + GetHeadOffset();
            offset.x = offset.z = 0;
            var startPosition = GetAnchorTransformPoint(offset);
            var direction = targetPosition - startPosition;
            if (Physics.SphereCast(startPosition, m_CollisionRadius, direction.normalized, out m_RaycastHit, direction.magnitude + m_Camera.nearClipPlane, 
                            m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                // Move the camera in if an object obstructed the view.
                targetPosition -= direction.normalized * ((direction.magnitude + m_Camera.nearClipPlane) - m_RaycastHit.distance);
            }
            m_CharacterLocomotion.EnableColliderCollisionLayer(collisionLayerEnabled);

            return targetPosition;
        }

        /// <summary>
        /// Returns the target position accounting for the springs and head offset.
        /// </summary>
        /// <returns>The target position accounting for the springs and head offset.</returns>
        public Vector3 GetTargetPosition()
        {
            var targetPosition = GetAnchorTransformPoint(m_PositionSpring.Value + m_SecondaryPositionSpring.Value + Vector3.up * m_VerticalOffsetAdjustment);

            // The camera should move with the head offset of the character.
            targetPosition += GetHeadOffset();

            return targetPosition;
        }

        /// <summary>
        /// Updates the head offset to match the current animator head position.
        /// </summary>
        private void UpdateHeadOffset()
        {
            // Allow the camera to move with the character's neck/head so the body doesn't clip the camera.
            if (m_CharacterAnchor != null && m_SmoothHeadOffsetBuffer != null && m_SmoothHeadOffsetSteps > 0) {
                var offset = m_CharacterTransform.InverseTransformPoint(m_CharacterAnchor.position) - m_CharacterAnchorOffset;
                // Allow the offset to settle before storing the difference.
                if (offset.sqrMagnitude > 0) {
                    // Add the current offset to the buffer.
                    m_SmoothHeadBufferIndex = (m_SmoothHeadBufferIndex + 1) % m_SmoothHeadOffsetBuffer.Length;
                    m_SmoothHeadOffsetBuffer[m_SmoothHeadBufferIndex] = offset;
                    if (m_SmoothHeadBufferCount <= m_SmoothHeadBufferIndex) {
                        m_SmoothHeadBufferCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the smoothed head offset.
        /// </summary>
        /// <returns>The smoothed head offset</returns>
        private Vector3 GetHeadOffset()
        {
            if (m_SmoothHeadBufferCount > 0) {
                // Find the average.
                var total = Vector3.zero;
                for (int i = 0; i < m_SmoothHeadBufferCount; ++i) {
                    var index = m_SmoothHeadBufferIndex - i;
                    if (index < 0) { index = m_SmoothHeadBufferCount + m_SmoothHeadBufferIndex - i; }
                    total += m_SmoothHeadOffsetBuffer[index];
                }
                return m_CharacterTransform.TransformDirection(total / m_SmoothHeadBufferCount);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Applies a sway force on the camera in response to character controller motion.
        /// </summary>
        private void UpdateSway()
        {
            var localVelocity = m_Transform.InverseTransformDirection(m_CharacterLocomotion.LocomotionVelocity * 0.016f) * m_CharacterLocomotion.TimeScale * Time.timeScale;
            m_RotationSpring.AddForce(Vector3.forward * localVelocity.x * m_RotationStrafeRoll);
        }

        /// <summary>
        /// Updates the first person bob.
        /// </summary>
        private void UpdateBob()
        {
            if ((m_BobPositionalRate == Vector3.zero || m_BobPositionalAmplitude == Vector3.zero) && (m_BobRollRate == 0 || m_BobRollAmplitude == 0)) {
                return;
            }

            var bobSpeed = ((m_BobRequireGroundContact && !m_CharacterLocomotion.Grounded) ? 0 : m_CharacterLocomotion.LocomotionVelocity.sqrMagnitude);

            // Scale and limit the input velocity.
            bobSpeed = Mathf.Min(bobSpeed * m_BobInputVelocityScale, m_BobMaxInputVelocity);

            // Reduce the number of decimals to avoid floating point imprecision issues.
            bobSpeed = Mathf.Round(bobSpeed * 1000) / 1000;

            // If the bob speed is zero then fade out the last speed value. It is important to clamp the speed to the 
            // last bob speed value because a preset may have changed since the last last bob.
            if (bobSpeed == 0) {
                bobSpeed = Mathf.Min((m_PrevBobSpeed * 0.93f), m_BobMaxInputVelocity);
            }

            // Update the positional and roll bob value.
            var currentPositionalBobAmplitude = (bobSpeed * (m_BobPositionalAmplitude * -0.0001f));
            var currentRollBobAmplitude = (bobSpeed * (m_BobRollAmplitude * -0.0001f));
            Vector3 currentBobOffsetValue;
            currentBobOffsetValue.x = Mathf.Cos(m_BobPositionalRate.x * m_CharacterLocomotion.TimeScale * Time.time * 10) * currentPositionalBobAmplitude.x;
            currentBobOffsetValue.y = Mathf.Cos(m_BobPositionalRate.y * m_CharacterLocomotion.TimeScale * Time.time * 10) * currentPositionalBobAmplitude.y;
            currentBobOffsetValue.z = Mathf.Cos(m_BobPositionalRate.z * m_CharacterLocomotion.TimeScale * Time.time * 10) * currentPositionalBobAmplitude.z;
            var currentBobRollValue = Mathf.Cos(m_BobRollRate * m_CharacterLocomotion.TimeScale * Time.time * 10) * currentRollBobAmplitude;

            // Add the bob value to the positional and rotational spring.
            m_PositionSpring.AddForce(currentBobOffsetValue);
            m_RotationSpring.AddForce(Vector3.forward * currentBobRollValue);
            m_PrevBobSpeed = bobSpeed;

            // Detect if the bob was previously decreasing and is now moving back up. This will indicate that the bob was at the lowest position.
            if (currentBobOffsetValue.y < m_BobMinTroughVerticalOffset && m_BobVerticalOffset < currentBobOffsetValue.y && m_BobVerticalOffsetDecreasing) {
                m_SecondaryPositionSpring.AddForce(m_BobTroughForce);
            }
            m_BobVerticalOffsetDecreasing = currentBobOffsetValue.y < m_BobVerticalOffset;
            m_BobVerticalOffset = currentBobOffsetValue.y;
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>The direction that the character is looking.</returns>
        public override Vector3 LookDirection(bool characterLookDirection)
        {
            var crosshairsDeltaRotation = characterLookDirection ? Quaternion.identity : GetCrosshairsDeltaRotation();
            var shake = characterLookDirection ? Quaternion.Inverse(Quaternion.Euler(m_Shake.y, m_Shake.x, 0)) : Quaternion.identity;
            var platformRotation = characterLookDirection ? Quaternion.Inverse(m_PlatformRotation) : Quaternion.identity;
            return (m_Transform.rotation * crosshairsDeltaRotation * shake * platformRotation) * Vector3.forward;
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="lookPosition">The position that the character is looking from.</param>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <param name="layerMask">The LayerMask value of the objects that the look direction can hit.</param>
        /// <param name="useRecoil">Should recoil be included in the look direction?</param>
        /// <returns>The direction that the character is looking.</returns>
        public override Vector3 LookDirection(Vector3 lookPosition, bool characterLookDirection, int layerMask, bool useRecoil)
        {
            var collisionLayerEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);

            // If a crosshairs is specified then the character should look at the crosshairs. Do not use the crosshairs delta for character look directions to prevent
            // the character's rotation from being affected by the crosshairs.
            var crosshairsDeltaRotation = characterLookDirection ? Quaternion.identity : GetCrosshairsDeltaRotation();
            var shake = characterLookDirection ? Quaternion.Inverse(Quaternion.Euler(m_Shake.y, m_Shake.x, 0)) : Quaternion.identity;
            var platformRotation = characterLookDirection ? Quaternion.Inverse(m_PlatformRotation) : Quaternion.identity;

            // Cast a ray from the camera point in the forward direction. The look direction is then the vector from the look position to the hit point.
            RaycastHit hit;
            Vector3 hitPoint;
            var rotation = (useRecoil ? m_Transform.rotation : MathUtility.TransformQuaternion(m_CharacterRotation, Quaternion.Euler(m_Pitch, m_Yaw, 0))) * 
                                                crosshairsDeltaRotation * shake * platformRotation;
            if (Physics.Raycast(m_Transform.position, rotation * Vector3.forward, out hit, m_LookDirectionDistance, layerMask, QueryTriggerInteraction.Ignore)) {
                hitPoint = hit.point;
            } else {
                Vector3 position;
                if (useRecoil) {
                    position = GetAnchorTransformPoint(m_PositionSpring.Value + m_SecondaryPositionSpring.Value);
                } else {
                    position = lookPosition;
                }
                var lookDirection = Vector3.zero;
                lookDirection.Set(0, 0, m_LookDirectionDistance);
                hitPoint = MathUtility.TransformPoint(position, rotation, lookDirection);
            }

            m_CharacterLocomotion.EnableColliderCollisionLayer(collisionLayerEnabled);
            return (hitPoint - lookPosition).normalized;
        }
        
        /// <summary>
        /// Adds a positional force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public override void AddPositionalForce(Vector3 force)
        {
            m_PositionSpring.AddForce(force);
        }

        /// <summary>
        /// Adds a rotational force to the ViewType. The secondary position spring is more stiff compared to the primary positional spring.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public override void AddRotationalForce(Vector3 force)
        {
            m_RotationSpring.AddForce(force);
        }

        /// <summary>
        /// Adds a secondary positional force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="restAccumulation">The percent of the force to accumulate to the rest value.</param>
        public override void AddSecondaryPositionalForce(Vector3 force, float restAccumulation)
        {
            if (restAccumulation > 0 && (m_AimAssist == null || !m_AimAssist.HasTarget())) {
                m_SecondaryPositionSpring.RestValue += force * restAccumulation;
            }
            m_SecondaryPositionSpring.AddForce(force);
        }

        /// <summary>
        /// Adds a delayed rotational force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="restAccumulation">The percent of the force to accumulate to the rest value.</param>
        public override void AddSecondaryRotationalForce(Vector3 force, float restAccumulation)
        {
            if (restAccumulation > 0 && (m_AimAssist == null || !m_AimAssist.HasTarget())) {
                m_Pitch += force.x * restAccumulation;
                m_Yaw += force.y * restAccumulation;
                var springRest = m_SecondaryRotationSpring.RestValue;
                springRest.z += force.z * restAccumulation;
                m_SecondaryRotationSpring.RestValue = springRest;
            }
            m_SecondaryRotationSpring.AddForce(force);
        }

        /// <summary>
        /// The character has landed on the ground.
        /// </summary>
        /// <param name="height">The height of the fall.</param>
        private void OnCharacterLand(float height)
        {
            var positionImpact = height * m_PositionFallImpact;
            var rotationImpact = height * m_RotationFallImpact;

            // Apply impact to camera position spring.
            m_PositionSpring.AddForce(positionImpact * -Vector3.up, m_PositionFallImpactSoftness);

            // Apply impact to camera rotation spring. Randomize the rotation upon landing.
            var roll = Random.value > 0.5f ? (rotationImpact * 2) : -(rotationImpact * 2);
            m_RotationSpring.AddForce(Vector3.forward * roll, m_RotationFallImpactSoftness);
        }

        /// <summary>
        /// The character has started to lean.
        /// </summary>
        /// <param name="distance">The distance that the character is leaning.</param>
        /// <param name="tilt">The amount of tilt to apply to the lean.</param>
        /// <param name="itemTiltMultiplier">The multiplier to apply to the tilt of an item.</param>
        private void OnCharacterLean(float distance, float tilt, float itemTiltMultiplier)
        {
            m_PositionSpring.RestValue = m_CameraController.AnchorOffset + m_LookOffset + distance * Vector3.right;
            m_RotationSpring.RestValue = tilt * Vector3.forward;
        }

        /// <summary>
        /// Adjusts the vertical offset by the given amount.
        /// </summary>
        /// <param name="amount">The amount to adjust the vertical offset height by.</param>
        private void AdjustVerticalOffset(float amount)
        {
            m_VerticalOffsetAdjustment += amount;
        }

        /// <summary>
        /// Callback when the StateManager will change the active state on the current object.
        /// </summary>
        public override void StateWillChange()
        {
            // Remember the interal spring values so they can be restored if a new spring is applied during the state change.
            m_PrevPositionSpringValue = m_PositionSpring.Value;
            m_PrevPositionSpringVelocity = m_PositionSpring.Velocity;
            m_PrevRotationSpringValue = m_RotationSpring.Value;
            m_PrevRotationSpringVelocity = m_RotationSpring.Velocity;
            m_PrevSecondaryPositionSpringValue = m_SecondaryPositionSpring.Value;
            m_PrevSecondaryPositionSpringVelocity = m_SecondaryPositionSpring.Velocity;
            m_PrevSecondaryRotationSpringValue = m_SecondaryRotationSpring.Value;
            m_PrevSecondaryRotationSpringVelocity = m_SecondaryRotationSpring.Velocity;
            // Multiple state changes can occur within the same frame. Only remember the first damping value.
            if (m_StateChangeFrame != Time.frameCount) {
                m_PrevFieldOfViewDamping = m_FieldOfViewDamping;
                m_PrevFirstPersonFieldOfViewDamping = m_FirstPersonFieldOfViewDamping;
            }
            m_StateChangeFrame = Time.frameCount;
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            // Append the zoom state name so the combination of state names will be called, such as "CrouchZoom".
            if (!string.IsNullOrEmpty(m_CameraController.ZoomState) && !m_AppendingZoomState) {
                m_AppendingZoomState = true;
                for (int i = 0; i < m_States.Length; ++i) {
                    StateManager.SetState(m_GameObject, m_States[i].Name + m_CameraController.ZoomState, m_States[i].Active && m_CameraController.ZoomInput);
                }
                m_AppendingZoomState = false;
            }

            if (m_Camera.fieldOfView != m_FieldOfView) {
                m_FieldOfViewChangeTime = Time.time;
                // The field of view should get a head start if the damping was previously 0. This will allow the field of view to move back to the original value even
                // when the state is no longer active.
                if (m_CameraController.ActiveViewType == this && m_PrevFieldOfViewDamping == 0) {
                    m_Camera.fieldOfView = (m_Camera.fieldOfView + m_FieldOfView) * 0.5f;
                    if (m_FirstPersonCamera != null && m_SynchronizeFieldOfView) {
                        m_FirstPersonCamera.fieldOfView = m_Camera.fieldOfView;
                    }
                }
            }
            if (m_FirstPersonCamera != null && m_FirstPersonCamera.fieldOfView != m_FirstPersonFieldOfView) {
                m_FirstPersonFieldOfViewChangeTime = Time.time;
                // The field of view should get a head start if the damping was previously 0. This will allow the field of view to move back to the original value even
                // when the state is no longer active.
                if (m_CameraController.ActiveViewType == this && m_PrevFirstPersonFieldOfViewDamping == 0) {
                    m_FirstPersonCamera.fieldOfView = (m_FirstPersonCamera.fieldOfView + m_FirstPersonFieldOfView) * 0.5f;
                }
            }

            m_PositionSpring.Value = m_PrevPositionSpringValue;
            m_PositionSpring.Velocity = m_PrevPositionSpringVelocity;
            m_RotationSpring.Value = m_PrevRotationSpringValue;
            m_RotationSpring.Velocity = m_PrevRotationSpringVelocity;
            m_SecondaryPositionSpring.Value = m_PrevSecondaryPositionSpringValue;
            m_SecondaryPositionSpring.Velocity = m_PrevSecondaryPositionSpringVelocity;
            m_SecondaryRotationSpring.Value = m_PrevSecondaryRotationSpringValue;
            m_SecondaryRotationSpring.Velocity = m_PrevSecondaryRotationSpringVelocity;
        }

        /// <summary>
        /// The camera has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            m_PositionSpring.Destroy();
            m_RotationSpring.Destroy();
            m_SecondaryPositionSpring.Destroy();
            m_SecondaryRotationSpring.Destroy();

            EventHandler.UnregisterEvent(m_GameObject, "OnAnchorOffsetUpdated", InitializePositionSpringValue);
            EventHandler.UnregisterEvent<ViewType, bool>(m_GameObject, "OnCameraChangeViewTypes", OnChangeViewType);
        }
    }
}
 