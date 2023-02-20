/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Input;
using Opsive.UltimateCharacterController.Motion;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;
#if ULTIMATE_CHARACTER_CONTROLLER_VR
using Opsive.UltimateCharacterController.VR;
#endif

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// The Third Person View Type will orbit around the character while always having the character in view.
    /// </summary>
    public abstract class ThirdPerson : ViewType
    {
        [Tooltip("The distance that the character should look ahead.")]
        [SerializeField] protected float m_LookDirectionDistance = 100;
        [Tooltip("The offset between the anchor and the camera.")]
        [SerializeField] protected Vector3 m_LookOffset = new Vector3(0.5f, 0, -2.5f);
        [Tooltip("The amount of smoothing to apply to the look offset. Can be zero.")]
        [SerializeField] protected float m_LookOffsetSmoothing = 0.05f;
        [Tooltip("The forward axis that the camera should adjust towards.")]
        [SerializeField] protected Vector3 m_ForwardAxis = Vector3.forward;
        [Tooltip("The field of view of the main camera.")]
        [SerializeField] protected float m_FieldOfView = 70f;
        [Tooltip("The damping time of the field of view angle when changed.")]
        [SerializeField] protected float m_FieldOfViewDamping = 0.25f;
        [Tooltip("The radius of the camera's collision sphere to prevent it from clipping with other objects.")]
        [SerializeField] protected float m_CollisionRadius = 0.05f;
        [Tooltip("The offset from the anchor position when determining if there is a collision.")]
        [SerializeField] protected Vector3 m_CollisionAnchorOffset;
        [Tooltip("The amount of smoothing to apply to the position. Can be zero.")]
        [SerializeField] protected float m_PositionSmoothing = 0.02f;
        [Tooltip("The amount of smoothing to apply to the position when an object is obstructing the target position. Can be zero.")]
        [SerializeField] protected float m_ObstructionPositionSmoothing = 0.04f;
        [Tooltip("The minimum pitch angle (in degrees).")]
        [SerializeField] protected float m_MinPitchLimit = -72;
        [Tooltip("The maximum pitch angle (in degrees).")]
        [SerializeField] protected float m_MaxPitchLimit = 72;
        [Tooltip("The positional spring used for regular movement.")]
        [SerializeField] protected Spring m_PositionSpring;
        [Tooltip("The rotational spring used for regular movement.")]
        [SerializeField] protected Spring m_RotationSpring;
        [Tooltip("The positional spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryPositionSpring;
        [Tooltip("The rotational spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryRotationSpring;
        [Tooltip("The name of the step zoom input mapping.")]
        [SerializeField] protected string m_StepZoomInputName = "Mouse ScrollWheel";
        [Tooltip("Specifies how quickly the camera zooms when step zooming.")]
        [SerializeField] protected float m_StepZoomSensitivity;
        [Tooltip("The minimum distance that the step zoom can zoom.")]
        [SerializeField] protected float m_MinStepZoom;
        [Tooltip("The maximum distance that the step zoom can zoom.")]
        [SerializeField] protected float m_MaxStepZoom = 1;

        public override float LookDirectionDistance { get { return m_LookDirectionDistance; } }
        public Vector3 ForwardAxis { get { return m_ForwardAxis; } set { m_ForwardAxis = value; } }
        public Vector3 LookOffset { get { return m_LookOffset; } set { m_LookOffset = value; } }
        public float LookOffsetSmoothing { get { return m_LookOffsetSmoothing; } set { m_LookOffsetSmoothing = value; } }
        public float FieldOfView { get { return m_FieldOfView; } set { m_FieldOfView = value; } }
        public float FieldOfViewDamping { get { return m_FieldOfViewDamping; } set { m_FieldOfViewDamping = value; } }
        public float CollisionRadius { get { return m_CollisionRadius; } set { m_CollisionRadius = value; } }
        public Vector3 CollisionAnchorOffset { get { return m_CollisionAnchorOffset; } set { m_CollisionAnchorOffset = value; } }
        public float PositionSmoothing { get { return m_PositionSmoothing; } set { m_PositionSmoothing = value; } }
        public float ObstructionPositionSmoothing { get { return m_ObstructionPositionSmoothing; } set { m_ObstructionPositionSmoothing = value; } }
        public float MinPitchLimit { get { return m_MinPitchLimit; } set { m_MinPitchLimit = value; } }
        public float MaxPitchLimit { get { return m_MaxPitchLimit; } set { m_MaxPitchLimit = value; } }
        public Spring PositionSpring { get { return m_PositionSpring; }
            set {
                m_PositionSpring = value;
                if (m_PositionSpring != null) { m_PositionSpring.Initialize(false, true); }
            }
        }
        public Spring RotationSpring { get { return m_RotationSpring; }
            set
            {
                m_RotationSpring = value;
                if (m_RotationSpring != null) { m_RotationSpring.Initialize(true, true); }
            }
        }
        public Spring SecondaryPositionSpring { get { return m_SecondaryPositionSpring; }
            set
            {
                m_SecondaryPositionSpring = value;
                if (m_SecondaryPositionSpring != null) { m_SecondaryPositionSpring.Initialize(false, true); }
            }
        }
        public Spring SecondaryRotationSpring { get { return m_SecondaryRotationSpring; }
            set
            {
                m_SecondaryRotationSpring = value;
                if (m_SecondaryRotationSpring != null) { m_SecondaryRotationSpring.Initialize(true, true); }
            }
        }
        public string StepZoomInputName { get { return m_StepZoomInputName; } set { m_StepZoomInputName = value; } }
        public float StepZoomSensitivity { get { return m_StepZoomSensitivity; } set { m_StepZoomSensitivity = value; } }
        public float MinStepZoom { get { return m_MinStepZoom; } set { m_MinStepZoom = value; } }
        public float MaxStepZoom { get { return m_MaxStepZoom; } set { m_MaxStepZoom = value; } }

        private UnityEngine.Camera m_Camera;
        private Transform m_CrosshairsTransform;
        private AimAssist m_AimAssist;
        protected float m_Pitch;
        protected float m_Yaw;
        protected Quaternion m_CharacterRotation;
        private Quaternion m_CharacterPlatformRotationOffset = Quaternion.identity;
        private Quaternion m_PlatformRotation = Quaternion.identity;
        private bool m_AppendingZoomState;
        private Vector3 m_CrosshairsLocalPosition;
        private Quaternion m_CrosshairsDeltaRotation;

        private Vector3 m_CurrentLookOffset;
        private RaycastHit m_RaycastHit;
        private Vector3 m_SmoothPositionVelocity;
        private Vector3 m_ObstructionSmoothPositionVelocity;
        private Vector3 m_SmoothLookOffsetVelocity;
        private float m_FieldOfViewChangeTime;

        protected CameraControllerHandler m_Handler;
        private ActiveInputEvent m_StepZoomInputEvent;
        private float m_StepZoom;

        private Vector3 m_PrevPositionSpringValue;
        private Vector3 m_PrevPositionSpringVelocity;
        private Vector3 m_PrevRotationSpringValue;
        private Vector3 m_PrevRotationSpringVelocity;
        private float m_PrevFieldOfViewDamping;
        private float m_PrevPositionSmoothing;
        private int m_StateChangeFrame = -1;
#if ULTIMATE_CHARACTER_CONTROLLER_VR
        private bool m_VREnabled;
#endif

        public override float Pitch { get { return m_Pitch; } }
        public override float Yaw { get { return m_Yaw; } }
        public override Quaternion CharacterRotation { get { return m_CharacterRotation; } }
        public override bool FirstPersonPerspective { get { return false; } }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_Camera = m_GameObject.GetCachedComponent<UnityEngine.Camera>();
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            VRCameraIdentifier vrCamera;
            if ((vrCamera = m_GameObject.GetComponentInChildren<VRCameraIdentifier>()) != null) {
                // The VR camera will be used as the main camera.
                m_Camera.enabled = false;
                m_Camera = vrCamera.GetComponent<UnityEngine.Camera>();
                m_VREnabled = true;
            }
#endif
            m_AimAssist = m_GameObject.GetCachedComponent<AimAssist>();
            m_Handler = m_GameObject.GetCachedComponent<CameraControllerHandler>();
            m_CurrentLookOffset = m_LookOffset;

            // Initialize the springs.
            m_PositionSpring.Initialize(false, false);
            m_RotationSpring.Initialize(true, true);
            m_SecondaryPositionSpring.Initialize(false, false);
            m_SecondaryRotationSpring.Initialize(true, true);
        }

        /// <summary>
        /// Attaches the view type to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the camera to.</param>
        public override void AttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<Transform>(m_Character, "OnCharacterChangeMovingPlatforms", OnCharacterChangeMovingPlatforms);
            }

            base.AttachCharacter(character);

            if (m_Character != null) {
                EventHandler.RegisterEvent<Transform>(m_Character, "OnCharacterChangeMovingPlatforms", OnCharacterChangeMovingPlatforms);
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
                if (m_Camera.fieldOfView != m_FieldOfView) {
                    m_FieldOfViewChangeTime = Time.time + m_FieldOfViewDamping / m_CharacterLocomotion.TimeScale;
                }
                if (m_StepZoomSensitivity > 0) {
                    if (m_Handler != null) {
                        m_StepZoomInputEvent = ObjectPool.Get<ActiveInputEvent>();
                        m_StepZoomInputEvent.Initialize(ActiveInputEvent.Type.Axis, m_StepZoomInputName, "OnThirdPersonViewTypeStepZoom");
                        m_Handler.RegisterInputEvent(m_StepZoomInputEvent);
                    }
                    EventHandler.RegisterEvent<float>(m_GameObject, "OnThirdPersonViewTypeStepZoom", OnStepZoom);
                }
            } else {
                if (m_StepZoomSensitivity > 0) {
                    if (m_Handler != null) {
                        m_StepZoomInputEvent = ObjectPool.Get<ActiveInputEvent>();
                        m_Handler.UnregisterAbilityInputEvent(m_StepZoomInputEvent);
                        ObjectPool.Return(m_StepZoomInputEvent);
                    }
                    EventHandler.UnregisterEvent<float>(m_GameObject, "OnThirdPersonViewTypeStepZoom", OnStepZoom);
                }
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
            m_CharacterRotation = characterRotation;
            if (m_CharacterLocomotion.Platform != null) {
                UpdatePlatformRotationOffset(m_CharacterLocomotion.Platform);
            }

            m_PositionSpring.Reset();
            m_RotationSpring.Reset();
            m_SecondaryPositionSpring.Reset();
            m_SecondaryRotationSpring.Reset();
            m_ObstructionSmoothPositionVelocity = m_SmoothPositionVelocity = Vector3.zero;
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
        /// The character's moving platform object has changed.
        /// </summary>
        /// <param name="movingPlatform">The moving platform to set. Can be null.</param>
        private void OnCharacterChangeMovingPlatforms(Transform movingPlatform)
        {
            if (movingPlatform != null) {
                UpdatePlatformRotationOffset(movingPlatform);
            } else {
                m_PlatformRotation = Quaternion.identity;
                m_CharacterPlatformRotationOffset = Quaternion.identity;
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
        /// Updates the camera field of view.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        public override void UpdateFieldOfView(bool immediateUpdate)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            if (m_VREnabled) {
                return;
            }
#endif
            if (m_Camera.fieldOfView != m_FieldOfView) {
                var zoom = (immediateUpdate || m_FieldOfViewDamping == 0) ? 1 : ((Time.time - m_FieldOfViewChangeTime) / (m_FieldOfViewDamping / m_CharacterLocomotion.TimeScale));
                m_Camera.fieldOfView = Mathf.SmoothStep(m_Camera.fieldOfView, m_FieldOfView, zoom);
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
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            if (m_VREnabled && immediateUpdate) {
                m_CharacterRotation = m_CharacterTransform.rotation;
                UnityEngine.XR.InputTracking.Recenter();
                EventHandler.ExecuteEvent("OnRecenterTracking");
            }
#endif

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

            // Keep the same relative rotation with the character if the character changes their up direction.
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

            // Return the rotation.
            return MathUtility.TransformQuaternion(m_CharacterRotation, Quaternion.Euler(m_Pitch, m_Yaw, 0)) * Quaternion.LookRotation(m_ForwardAxis) * Quaternion.Euler(m_RotationSpring.Value) * Quaternion.Euler(m_SecondaryRotationSpring.Value);
        }

        /// <summary>
        /// Moves the camera according to the current pitch and yaw values.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated position.</returns>
        public override Vector3 Move(bool immediateUpdate)
        {
            // Prevent obstruction from other objects. Check for obstruction against character player position rather than the look position because the character should always be visible. It doesn't
            // matter as much if the look position isn't directly visible.
            var anchorPosition = GetAnchorPosition();
            m_CurrentLookOffset = immediateUpdate ? m_LookOffset : Vector3.SmoothDamp(m_CurrentLookOffset, m_LookOffset, ref m_SmoothLookOffsetVelocity, m_LookOffsetSmoothing);
            var lookPosition = anchorPosition + (m_CurrentLookOffset.x * m_Transform.right) + (m_CurrentLookOffset.y * m_CharacterTransform.up) + ((m_CurrentLookOffset.z + m_StepZoom) * m_Transform.forward);

            // The position spring is already smoothed so it doesn't need to be included in SmoothDamp.
            lookPosition += m_Transform.TransformDirection(m_PositionSpring.Value + m_SecondaryPositionSpring.Value);
            // Keep the look position above water.
            if (Physics.Linecast(m_CharacterTransform.position, m_CharacterTransform.position + m_CharacterTransform.up * m_CharacterLocomotion.Height, out m_RaycastHit, 1 << LayerManager.Water)) {
                if (lookPosition.y < m_RaycastHit.point.y) {
                    lookPosition.y = m_RaycastHit.point.y;
                }
            }

            // Smoothly move into position.
            Vector3 targetPosition;
            if (immediateUpdate) {
                targetPosition = lookPosition;
            } else {
                targetPosition = Vector3.SmoothDamp(m_Transform.position, lookPosition, ref m_SmoothPositionVelocity, m_PositionSmoothing);
            }

            var collisionLayerEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            var direction = lookPosition - (anchorPosition + m_CollisionAnchorOffset);
            // Fire a sphere to prevent the camera from colliding with other objects.
            if (Physics.SphereCast(anchorPosition + m_CollisionAnchorOffset - direction.normalized * m_CollisionRadius, m_CollisionRadius, direction.normalized, out m_RaycastHit, direction.magnitude, 
                                m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                // Move the camera in if the character isn't in view.
                targetPosition = m_RaycastHit.point + m_RaycastHit.normal * m_CollisionRadius;
                if (!immediateUpdate) {
                    targetPosition = Vector3.SmoothDamp(m_Transform.position, targetPosition, ref m_ObstructionSmoothPositionVelocity, m_ObstructionPositionSmoothing);
                }

                // Keep a constant height if there is nothing getting in the way of that position.
                var localDirection = m_CharacterTransform.TransformDirection(direction);
                if (localDirection.y > 0) {
                    // Account for local y values.
                    var constantHeightPosition = MathUtility.InverseTransformPoint(m_CharacterTransform.position, m_CharacterRotation, targetPosition);
                    constantHeightPosition.y = MathUtility.InverseTransformPoint(m_CharacterTransform.position, m_CharacterRotation, lookPosition).y;
                    constantHeightPosition = MathUtility.TransformPoint(m_CharacterTransform.position, m_CharacterRotation, constantHeightPosition);
                    direction = constantHeightPosition - (anchorPosition + m_CollisionAnchorOffset);
                    if (!Physics.SphereCast(anchorPosition + m_CollisionAnchorOffset - direction.normalized * m_CollisionRadius, m_CollisionRadius, direction.normalized, 
                            out m_RaycastHit, direction.magnitude - m_CollisionRadius, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                        targetPosition = constantHeightPosition;
                    }
                }
            }
            m_CharacterLocomotion.EnableColliderCollisionLayer(collisionLayerEnabled);

            // Prevent the camera from clipping with the character.
            Collider containsCollider;
            if ((containsCollider = m_CharacterLocomotion.BoundsCountains(targetPosition)) != null) {
                targetPosition = containsCollider.ClosestPointOnBounds(targetPosition);
            }

            // The target position should never be lower than the character's position. This may happen if the camera is trying to be positioned below water.
            var localTargetPosition = m_CharacterTransform.InverseTransformPoint(targetPosition);
            if (localTargetPosition.y < 0) {
                localTargetPosition.y = 0;
                targetPosition = m_CharacterTransform.TransformPoint(localTargetPosition);
            }

            return targetPosition;
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>The direction that the character is looking.</returns>
        public override Vector3 LookDirection(bool characterLookDirection)
        {
            var crosshairsDeltaRotation = characterLookDirection ? Quaternion.identity : GetCrosshairsDeltaRotation();
            var platformRotation = characterLookDirection ? m_PlatformRotation : Quaternion.identity;
            return (m_Transform.rotation * crosshairsDeltaRotation * Quaternion.Inverse(platformRotation)) * Vector3.forward;
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
            var platformRotation = characterLookDirection ? m_PlatformRotation : Quaternion.identity;

            // Cast a ray from the camera point in the forward direction. The look direction is then the vector from the look position to the hit point.
            RaycastHit hit;
            Vector3 hitPoint;
            var rotation = (useRecoil ? m_Transform.rotation : 
                                MathUtility.TransformQuaternion(m_CharacterRotation, Quaternion.Euler(m_Pitch, m_Yaw, 0)) * Quaternion.LookRotation(m_ForwardAxis)) *
                                crosshairsDeltaRotation * Quaternion.Inverse(platformRotation);
            if (Physics.Raycast(m_Transform.position, rotation * Vector3.forward, out hit, m_LookDirectionDistance, layerMask, QueryTriggerInteraction.Ignore)) {
                hitPoint = hit.point;
            } else {
                var offset = Vector3.zero;
                offset.Set(0, 0, m_LookDirectionDistance);
                hitPoint = MathUtility.TransformPoint(m_Transform.position, rotation, offset);
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
        /// Adds a secondary force to the ViewType.
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
        /// The camera should zoom in or out.
        /// </summary>
        /// <param name="amount">The amount to zoom.</param>
        private void OnStepZoom(float amount)
        {
            m_StepZoom = Mathf.Clamp(m_StepZoom + m_StepZoomSensitivity * amount * Time.deltaTime, m_MinStepZoom, m_MaxStepZoom);
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
            // Multiple state changes can occur within the same frame. Only remember the first damping value.
            if (m_StateChangeFrame != Time.frameCount) {
                m_PrevFieldOfViewDamping = m_FieldOfViewDamping;
                m_PrevPositionSmoothing = m_PositionSmoothing;
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

            if (m_Camera.fieldOfView != m_FieldOfView
#if ULTIMATE_CHARACTER_CONTROLLER_VR
                && !m_VREnabled
#endif
                ) {
                m_FieldOfViewChangeTime = Time.time;
                if (m_CameraController.ActiveViewType == this) {
                    // The field of view and location should get a head start if the damping was previously 0. This will allow the field of view and location
                    // to move back to the original value when the state is no longer active.
                    if (m_PrevFieldOfViewDamping == 0) {
                        m_Camera.fieldOfView = (m_Camera.fieldOfView + m_FieldOfView) * 0.5f;
                    }

                    if (m_PositionSmoothing == 0 || (m_PrevPositionSmoothing == 0 && m_PositionSmoothing != m_PrevPositionSmoothing)) {
                        KinematicObjectManager.SetCameraPosition(m_CameraController.KinematicObjectIndex, (m_Transform.position + Move(true)) * 0.5f);
                    }
                }
            }

            m_PositionSpring.Value = m_PrevPositionSpringValue;
            m_PositionSpring.Velocity = m_PrevPositionSpringVelocity;
            m_RotationSpring.Value = m_PrevRotationSpringValue;
            m_RotationSpring.Velocity = m_PrevRotationSpringVelocity;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            m_PositionSpring.Destroy();
            m_RotationSpring.Destroy();
            m_SecondaryPositionSpring.Destroy();
            m_SecondaryRotationSpring.Destroy();
        }
    }
}