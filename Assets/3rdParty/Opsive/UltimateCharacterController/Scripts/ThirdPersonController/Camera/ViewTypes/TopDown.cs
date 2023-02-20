/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Input;
using Opsive.UltimateCharacterController.Utility;
#if ULTIMATE_CHARACTER_CONTROLLER_VR
using Opsive.UltimateCharacterController.VR;
#endif

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// The Top Down View Type allows the camera to be placed in a top down perspective with the character in view.
    /// </summary>
    [RecommendedMovementType(typeof(Character.MovementTypes.TopDown))]
    public class TopDown : ViewType
    {
        [Tooltip("The distance that the character should look ahead.")]
        [SerializeField] protected float m_LookDirectionDistance = 100;
        [Tooltip("The forward axis that the camera should adjust towards.")]
        [SerializeField] protected Vector3 m_ForwardAxis = -Vector3.forward;
        [Tooltip("The up axis that the camera should adjust towards.")]
        [SerializeField] protected Vector3 m_UpAxis = Vector3.up;
        [Tooltip("The speed at which the camera rotates to face the character.")]
        [SerializeField] protected float m_RotationSpeed = 1.5f;
        [Tooltip("The minimum pitch angle (in degrees).")]
        [SerializeField] protected float m_MinPitchLimit = 70;
        [Tooltip("The maximum pitch angle (in degrees).")]
        [SerializeField] protected float m_MaxPitchLimit = 89;
        [Tooltip("The radius of the camera's collision sphere to prevent it from clipping with other objects.")]
        [SerializeField] protected float m_CollisionRadius = 0.05f;
        [Tooltip("The distance to position the camera away from the anchor.")]
        [SerializeField] protected float m_ViewDistance = 10;
        [Tooltip("The number of degrees to adjust if the anchor is obstructed by an object.")]
        [SerializeField] protected float m_ViewStep = 2;
        [Tooltip("The amount of smoothing to apply to the movement. Can be zero.")]
        [SerializeField] protected float m_MoveSmoothing = 0.1f;
        [Tooltip("Should the look direction account for vertical offsets? This is only used when the mouse is visible.")]
        [SerializeField] protected bool m_VerticalLookDirection;

        public Vector3 UpAxis { get { return m_UpAxis; } set { m_UpAxis = value; } }
        public float RotationSpeed { get { return m_RotationSpeed; } set { m_RotationSpeed = value; } }
        public float MinPitchLimit { get { return m_MinPitchLimit; } set { m_MinPitchLimit = value; } }
        public float MaxPitchLimit { get { return m_MaxPitchLimit; } set { m_MaxPitchLimit = value; } }
        public float CollisionRadius { get { return m_CollisionRadius; } set { m_CollisionRadius = value; } }
        public float ViewDistance { get { return m_ViewDistance; } set { m_ViewDistance = value; } }
        public float ViewStep { get { return m_ViewStep; } set { m_ViewStep = value; } }
        public float MoveSmoothing { get { return m_MoveSmoothing; } set { m_MoveSmoothing = value; } }
        public bool VerticalLookDirection { get { return m_VerticalLookDirection; } set { m_VerticalLookDirection = value; } }

        public override float Pitch { get { return 0; } }
        public override float Yaw { get { return 0; } }
        public override Quaternion CharacterRotation { get { return m_CharacterTransform.rotation; } }
        public override bool FirstPersonPerspective { get { return false; } }
        public override float LookDirectionDistance { get { return m_LookDistance; } }
        public override bool RotatePriority { get { return false; } }

        private Ray m_Ray = new Ray();
        private UnityEngine.Camera m_Camera;
        private PlayerInput m_PlayerInput;
        private Plane m_HitPlane = new Plane();
        private RaycastHit m_RaycastHit;
        private ObjectFader m_ObjectFader;

        private Vector3 m_LookDirection;
        private Vector3 m_SmoothPositionVelocity;
        private float m_LookDistance;

#if ULTIMATE_CHARACTER_CONTROLLER_VR
        private bool m_VREnabled;
#endif

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_Camera = m_CameraController.gameObject.GetCachedComponent<UnityEngine.Camera>();
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            VRCameraIdentifier vrCamera;
            if ((vrCamera = m_GameObject.GetComponentInChildren<VRCameraIdentifier>()) != null) {
                // The VR camera will be used as the main camera.
                m_Camera.enabled = false;
                m_Camera = vrCamera.GetComponent<UnityEngine.Camera>();
                m_VREnabled = true;
            }
#endif
            m_ObjectFader = m_CameraController.gameObject.GetComponent<ObjectFader>();
            m_LookDistance = m_LookDirectionDistance;
        }

        /// <summary>
        /// Attaches the view type to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the camera to.</param>
        public override void AttachCharacter(GameObject character)
        {
            base.AttachCharacter(character);

            if (m_Character == null) {
                m_PlayerInput = null;
            } else {
                m_PlayerInput = m_Character.GetCachedComponent<PlayerInput>();
                m_LookDirection = m_CharacterTransform.forward;
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
                m_Ray.direction = GetAnchorPosition() - m_Transform.position;
            }
        }

        /// <summary>
        /// Reset the ViewType's variables.
        /// </summary>
        /// <param name="characterRotation">The rotation of the character.</param>
        public override void Reset(Quaternion characterRotation)
        {
            m_SmoothPositionVelocity = Vector3.zero;
        }

        /// <summary>
        /// Rotates the camera to face the character.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediateUpdate)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            if (m_VREnabled && immediateUpdate) {
                UnityEngine.XR.InputTracking.Recenter();
                Events.EventHandler.ExecuteEvent("OnRecenterTracking");
            }
#endif
            var rotation = Quaternion.LookRotation(-m_Ray.direction, m_UpAxis);
            return immediateUpdate ? rotation : Quaternion.Slerp(m_Transform.rotation, rotation, m_RotationSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * m_CharacterLocomotion.DeltaTime);
        }

        /// <summary>
        /// Moves the camera to face the character.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated position.</returns>
        public override Vector3 Move(bool immediateUpdate)
        {
            var step = 0f;
            m_Ray.origin = GetAnchorPosition();
            var lookRotation = Quaternion.LookRotation(-m_ForwardAxis, m_CharacterLocomotion.Up);
            m_Ray.direction = MathUtility.TransformQuaternion(lookRotation, Quaternion.Euler(90 - m_MinPitchLimit, 0, 0)) * m_UpAxis;
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            // Prevent the character from being obstructed by adjusting the pitch of the camera and testing for an obstruction free path.
            while (Physics.SphereCast(m_Ray, m_CollisionRadius, out m_RaycastHit, m_ViewDistance, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                if (m_ObjectFader != null) {
                    var canFade = true;
                    // If the object can be faded then the view does not need to readjust.
                    var renderers = m_RaycastHit.collider.gameObject.GetCachedComponents<Renderer>();
                    for (int i = 0; i < renderers.Length; ++i) {
                        var materials = renderers[i].materials;
                        for (int j = 0; j < materials.Length; ++j) {
                            if (!m_ObjectFader.CanMaterialFade(materials[j])) {
                                canFade = false;
                                break;
                            }
                        }
                        if (!canFade) {
                            break;
                        }
                    }

                    // If the material will fade then the view does not need to readjust.
                    if (canFade) {
                        break;
                    }
                }

                if (m_MinPitchLimit + step >= m_MaxPitchLimit) {
                    m_Ray.direction = MathUtility.TransformQuaternion(lookRotation, Quaternion.Euler(90 - m_MaxPitchLimit, 0, 0)) * m_UpAxis;
                    break;
                }
                step += m_ViewStep;
                m_Ray.direction = MathUtility.TransformQuaternion(lookRotation, Quaternion.Euler(90 - m_MinPitchLimit - step, 0, 0)) * m_UpAxis;
            }
            m_CharacterLocomotion.EnableColliderCollisionLayer(true);
            var targetPosition = m_Ray.origin + m_Ray.direction * m_ViewDistance;
            return immediateUpdate ? targetPosition : Vector3.SmoothDamp(m_Transform.position, targetPosition, ref m_SmoothPositionVelocity, m_MoveSmoothing);
        }

        /// <summary>
        /// Returns the position of the look source.
        /// </summary>
        public override Vector3 LookPosition()
        {
            return m_CharacterTransform.position;
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>The direction that the character is looking.</returns>
        public override Vector3 LookDirection(bool characterLookDirection)
        {
            return m_CharacterTransform.forward;
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
            // The character should look towards the cursor or Mouse X/Y direction.
            if (m_PlayerInput.IsCursorVisible()) {
                var ray = m_Camera.ScreenPointToRay(m_PlayerInput.GetMousePosition());
                var planeRaycast = true;
                var hitPointValid = false;
                var hitPoint = Vector3.zero;
                if (m_VerticalLookDirection) {
                    // If vertical look direction is enabled then the top down character should be able to aim along the relative y axis. The hit plane should be based
                    // off of the hit object's relative y position instead of the look position. This allows the character to look up/down while ensuring the direction
                    // will move through the mouse position.
                    if (Physics.Raycast(ray, out m_RaycastHit, m_LookDirectionDistance, layerMask, QueryTriggerInteraction.Ignore)) {
                        m_LookDirection = m_RaycastHit.point - lookPosition;
                        m_LookDirection.Normalize();
                        hitPoint = m_RaycastHit.point;
                        planeRaycast = false;
                        hitPointValid = true;
                    }
                }

                if (planeRaycast) {
                    // Cast a ray from the ray position to an invisible plane to determine the direction that the character should look.
                    float distance;
                    m_HitPlane.SetNormalAndPosition(m_CharacterTransform.up, lookPosition);
                    if (m_HitPlane.Raycast(ray, out distance)) {
                        hitPoint = ray.GetPoint(distance);
                        m_LookDirection = (hitPoint - lookPosition).normalized;
                        hitPointValid = true;
                    }
                }

                if (hitPointValid) {
                    // The hit point may be located within the look position. Use the character's forward direction in this case to prevent IK from placing the character
                    // in an impossible position.
                    if (!characterLookDirection && ((m_CharacterTransform.position - hitPoint).sqrMagnitude < (m_CharacterTransform.position - lookPosition).sqrMagnitude * 1.5f ||
                            Vector3.Dot(m_LookDirection, m_CharacterTransform.forward) < 0f)) {
                        var lookDirection = Vector3.Lerp(m_CharacterTransform.forward, m_LookDirection, Vector3.Dot(m_LookDirection, m_CharacterTransform.forward));
                        var verticalLookDirection = m_CharacterTransform.InverseTransformDirection(m_LookDirection).y;
                        var localLookDirection = m_CharacterTransform.InverseTransformDirection(lookDirection);
                        localLookDirection.y = verticalLookDirection;
                        m_LookDirection = m_CharacterTransform.TransformDirection(localLookDirection);
                    }
                    m_LookDistance = m_LookDirection.magnitude;
                    m_LookDirection.Normalize();
                }
            } else {
                // If the cursor isn't visible then get the axis to determine a look rotation. This will be used for controllers and virtual input.
                var direction = Vector3.zero;
                direction.x = m_PlayerInput.GetAxis(m_PlayerInput.HorizontalLookInputName);
                direction.z = -m_PlayerInput.GetAxis(m_PlayerInput.VerticalLookInputName);
                if (direction.sqrMagnitude > 0.1f) {
                    m_LookDirection = Quaternion.LookRotation(direction.normalized, m_CharacterLocomotion.Up) * Vector3.forward;
                }
            }

            return m_LookDirection;
        }
    }
}