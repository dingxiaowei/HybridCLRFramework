/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.MovementTypes;
using Opsive.UltimateCharacterController.Input;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Character.MovementTypes
{
    /// <summary>
    /// The TopDown MovementType can move the character relative to a top down camera.
    /// </summary>
    public class TopDown : MovementType
    {
        [Tooltip("Should the character move relative to the camera's direction?")]
        [SerializeField] protected bool m_RelativeCameraMovement = true;
        [Tooltip("Should the character look in the direction of the movement?")]
        [SerializeField] protected bool m_LookInMoveDirection;

        public bool RelativeCameraMovement { get { return m_RelativeCameraMovement; } set { m_RelativeCameraMovement = value; } }
        public bool LookInMoveDirection { get { return m_LookInMoveDirection; } set { m_LookInMoveDirection = value; } }

        private PlayerInput m_PlayerInput;
        private UnityEngine.Camera m_Camera;
        private Plane m_HitPlane = new Plane();

        public override bool FirstPersonPerspective { get { return false; } }

        /// <summary>
        /// Initializes the MovementType.
        /// </summary>
        /// <param name="characterLocomotion">The reference to the character locomotion component.</param>
        public override void Initialize(UltimateCharacterLocomotion characterLocomotion)
        {
            base.Initialize(characterLocomotion);

            m_PlayerInput = characterLocomotion.gameObject.GetCachedComponent<PlayerInput>();
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        protected override void OnAttachLookSource(ILookSource lookSource)
        {
            base.OnAttachLookSource(lookSource);

            if (lookSource != null) {
                m_Camera = lookSource.GameObject.GetCachedComponent<UnityEngine.Camera>();
            } else {
                m_Camera = null;
            }
        }

        /// <summary>
        /// Returns the delta yaw rotation of the character.
        /// </summary>
        /// <param name="characterHorizontalMovement">The character's horizontal movement.</param>
        /// <param name="characterForwardMovement">The character's forward movement.</param>
        /// <param name="cameraHorizontalMovement">The camera's horizontal movement.</param>
        /// <param name="cameraVerticalMovement">The camera's vertical movement.</param>
        /// <returns>The delta yaw rotation of the character.</returns>
        public override float GetDeltaYawRotation(float characterHorizontalMovement, float characterForwardMovement, float cameraHorizontalMovement, float cameraVerticalMovement)
        {
            if (m_LookInMoveDirection) {
                if (characterHorizontalMovement != 0 || characterForwardMovement != 0) {
                    var inputVector = Vector3.zero;
                    inputVector.Set(characterHorizontalMovement, 0, characterForwardMovement);

                    var lookRotation = Quaternion.LookRotation(m_LookSource.Transform.rotation * inputVector.normalized);
                    return MathUtility.ClampInnerAngle(MathUtility.InverseTransformQuaternion(m_Transform.rotation, lookRotation).eulerAngles.y);
                }
            } else {
                // The character should look towards the cursor or Mouse X/Y direction.
                if (m_PlayerInput.IsCursorVisible()) {
                    // Cast a ray from the mouse position to an invisible plane to determine the direction that the character should look.
                    float distance;
                    var ray = m_Camera.ScreenPointToRay(m_PlayerInput.GetMousePosition());
                    var characterCenter = m_Transform.position + (m_CharacterLocomotion.Up * m_CharacterLocomotion.Height / 2);
                    m_HitPlane.SetNormalAndPosition(m_CharacterLocomotion.Up, characterCenter);
                    if (m_HitPlane.Raycast(ray, out distance)) {
                        var rotation = Quaternion.LookRotation((ray.GetPoint(distance) - characterCenter).normalized, m_CharacterLocomotion.Up);
                        return MathUtility.ClampInnerAngle(MathUtility.InverseTransformQuaternion(m_Transform.rotation, rotation).eulerAngles.y);
                    }
                } else {
                    // If the mouse hasn't moved then get the axis to determine a look rotation. This will be used for controllers and virtual input.
                    var direction = Vector3.zero;
                    direction.x = m_PlayerInput.GetAxis(m_PlayerInput.HorizontalLookInputName);
                    direction.z = -m_PlayerInput.GetAxis(m_PlayerInput.VerticalLookInputName);
                    if (direction.sqrMagnitude > 0.1f) {
                        var rotation = Quaternion.LookRotation(direction.normalized, m_CharacterLocomotion.Up);
                        return MathUtility.ClampInnerAngle(MathUtility.InverseTransformQuaternion(m_Transform.rotation, rotation).eulerAngles.y);
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the controller's input vector relative to the movement type.
        /// </summary>
        /// <param name="inputVector">The current input vector.</param>
        /// <returns>The updated input vector.</returns>
        public override Vector2 GetInputVector(Vector2 inputVector)
        {
            if (!m_RelativeCameraMovement) {
                return inputVector;
            }

            var rotation = m_Transform.rotation;
            // The camera may not exist (in the case of an AI agent) but if it does move relative to the camera position.
            if (m_LookSource != null) {
                var localEuler = MathUtility.InverseTransformQuaternion(m_LookSource.Transform.rotation, Quaternion.LookRotation(Vector3.forward, m_CharacterLocomotion.Up)).eulerAngles;
                localEuler.x = localEuler.z = 0;
                rotation *= MathUtility.TransformQuaternion(Quaternion.Euler(localEuler), Quaternion.LookRotation(Vector3.forward, m_CharacterLocomotion.Up));
            }
            // Convert to a local input vector. Vector3s are required for the correct calculation.
            var localInputVector = Vector3.zero;
            localInputVector.Set(inputVector.x, 0, inputVector.y);
            localInputVector = Quaternion.Inverse(rotation) * localInputVector;

            // Store the max input vector value so it can be normalized before being returned.
            var maxInputVectorValue = Mathf.Max(Mathf.Abs(inputVector.x), Mathf.Abs(inputVector.y));
            inputVector.x = localInputVector.x;
            inputVector.y = localInputVector.z;
            // Normalize the input vector to prevent the diagonals from moving faster.
            inputVector = inputVector.normalized * maxInputVectorValue;

            return inputVector;
        }

        /// <summary>
        /// Can the character look independently of the transform rotation?
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>True if the character should look independently of the transform rotation.</returns>
        public override bool UseIndependentLook(bool characterLookDirection)
        {
            if (m_LookInMoveDirection || base.UseIndependentLook(characterLookDirection)) {
                return true;
            }
            return !characterLookDirection;
        }
    }
}