/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.MovementTypes;
using Opsive.UltimateCharacterController.Input;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Character.MovementTypes
{
    /// <summary>
    /// The RPG MovementType uses a control scheme similar to the standard in the RPG genre. The MovementType works with the RPG ViewType to move and rotate the character.
    /// </summary>
    public class RPG : MovementType
    {
        [Tooltip("The name of the rotate input mapping.")]
        [SerializeField] protected string m_RotateInputName = "Fire3";
        [Tooltip("The name of the turn input mapping.")]
        [SerializeField] protected string m_TurnInputName = "Horizontal";
        [Tooltip("The amount to multiply the turn value by.")]
        [SerializeField] protected float m_TurnMultiplier = 1.5f;
        [Tooltip("The name of the auto move input mapping.")]
        [SerializeField] protected string m_AutoMoveInputName = "Action";

        public string RotateInputName { get { return m_RotateInputName; } set { m_RotateInputName = value; } }
        public string TurnInputName { get { return m_TurnInputName; } set { m_TurnInputName = value; } }
        public float TurnMultiplier { get { return m_TurnValue; } set { m_TurnValue = value; } }
        public string AutoMoveInputName { get { return m_AutoMoveInputName; } set { m_AutoMoveInputName = value; } }

        private UltimateCharacterLocomotionHandler m_Handler;
        private ActiveInputEvent m_StartRotateInputEvent;
        private ActiveInputEvent m_StopRotateInputEvent;
        private ActiveInputEvent m_TurnInputEvent;
        private ActiveInputEvent m_AutoMoveInputEvent;

        private bool m_Rotate;
        private float m_TurnValue;
        private bool m_AutoMove;
        private bool m_MovementTypeActive;

        public override bool FirstPersonPerspective { get { return false; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_Handler = m_GameObject.GetCachedComponent<UltimateCharacterLocomotionHandler>();

            // Work with the handler to listen for any input events.
            if (m_Handler != null) {
                m_StartRotateInputEvent = ObjectPool.Get<ActiveInputEvent>();
                m_StartRotateInputEvent.Initialize(ActiveInputEvent.Type.ButtonDown, m_RotateInputName, "OnRPGMovementTypeStartRotate");

                m_StopRotateInputEvent = ObjectPool.Get<ActiveInputEvent>();
                m_StopRotateInputEvent.Initialize(ActiveInputEvent.Type.ButtonUp, m_RotateInputName, "OnRPGMovementTypeStopRotate");

                m_TurnInputEvent = ObjectPool.Get<ActiveInputEvent>();
                m_TurnInputEvent.Initialize(ActiveInputEvent.Type.Axis, m_TurnInputName, "OnRPGMovementTypeTurn");

                m_AutoMoveInputEvent = ObjectPool.Get<ActiveInputEvent>();
                m_AutoMoveInputEvent.Initialize(ActiveInputEvent.Type.ButtonDown, m_AutoMoveInputName, "OnRPGMovementTypeAutoMove");

                m_Handler.RegisterInputEvent(m_StartRotateInputEvent);
                m_Handler.RegisterInputEvent(m_TurnInputEvent);
                m_Handler.RegisterInputEvent(m_AutoMoveInputEvent);
            }
            EventHandler.RegisterEvent(m_GameObject, "OnRPGMovementTypeStartRotate", OnStartRotate);
            EventHandler.RegisterEvent(m_GameObject, "OnRPGMovementTypeStopRotate", OnStopRotate);
            EventHandler.RegisterEvent<float>(m_GameObject, "OnRPGMovementTypeTurn", OnTurn);
            EventHandler.RegisterEvent(m_GameObject, "OnRPGMovementTypeAutoMove", OnToggleAutoMove);
        }

        /// <summary>
        /// The movement type has changed.
        /// </summary>
        /// <param name="activate">Should the current movement type be activated?</param>
        public override void ChangeMovementType(bool activate)
        {
            m_MovementTypeActive = activate;
        }

        /// <summary>
        /// Starts rotating the character along the relative y axis.
        /// </summary>
        private void OnStartRotate()
        {
            m_Rotate = true;

            // The handler only needs to listen to the start input event once.
            if (m_Handler != null) {
                m_Handler.UnregisterInputEvent(m_StartRotateInputEvent);
                m_Handler.RegisterInputEvent(m_StopRotateInputEvent);
            }
        }

        /// <summary>
        /// Stops rotating the character along the relative y axis.
        /// </summary>
        private void OnStopRotate()
        {
            m_Rotate = false;

            // The handler only needs to listen to the stop input event once.
            if (m_Handler != null) {
                m_Handler.UnregisterInputEvent(m_StopRotateInputEvent);
                m_Handler.RegisterInputEvent(m_StartRotateInputEvent);
            }
        }

        /// <summary>
        /// The character axis has turned.
        /// </summary>
        /// <param name="turnValue">The value of the turn axis.</param>
        private void OnTurn(float turnValue)
        {
            m_TurnValue = turnValue * m_TurnMultiplier;
        }

        /// <summary>
        /// Toggles the character from automatically moving in the forward direction.
        /// </summary>
        private void OnToggleAutoMove()
        {
            if (!m_MovementTypeActive) {
                return;
            }

            m_AutoMove = !m_AutoMove;
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
            var turnAmount = m_TurnValue;
            if (m_Rotate) {
                turnAmount += MathUtility.InverseTransformQuaternion(m_Transform.rotation, m_LookSource.Transform.rotation).eulerAngles.y;
            }
            var rotation = Quaternion.AngleAxis(turnAmount, m_CharacterLocomotion.Up) * m_Transform.rotation;
            return MathUtility.ClampInnerAngle(MathUtility.InverseTransformQuaternion(m_Transform.rotation, rotation).eulerAngles.y);
        }

        /// <summary>
        /// Gets the controller's input vector relative to the movement type.
        /// </summary>
        /// <param name="inputVector">The current input vector.</param>
        /// <returns>The updated input vector.</returns>
        public override Vector2 GetInputVector(Vector2 inputVector)
        {
            // AutoMove will automatically move the character in the forward direction.
            if (m_AutoMove) {
                inputVector.y = 1;

                // The raw input should also be updated so the movement abilities can correctly track the auto move.
                var rawInputVector = m_CharacterLocomotion.RawInputVector;
                rawInputVector.y = 1;
                m_CharacterLocomotion.RawInputVector = rawInputVector;
            }
            return inputVector;
        }

        /// <summary>
        /// Can the character look independently of the transform rotation?
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>True if the character should look independently of the transform rotation.</returns>
        public override bool UseIndependentLook(bool characterLookDirection)
        {
            return true;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            if (m_Handler != null) {
                if (m_Rotate) {
                    m_Handler.UnregisterInputEvent(m_StopRotateInputEvent);
                } else {
                    m_Handler.UnregisterInputEvent(m_StartRotateInputEvent);
                }

                m_Handler.UnregisterInputEvent(m_TurnInputEvent);
                m_Handler.UnregisterInputEvent(m_AutoMoveInputEvent);

                ObjectPool.Return(m_StartRotateInputEvent);
                ObjectPool.Return(m_StopRotateInputEvent);
                ObjectPool.Return(m_TurnInputEvent);
                ObjectPool.Return(m_AutoMoveInputEvent);
            }
            EventHandler.UnregisterEvent(m_GameObject, "OnRPGMovementTypeStartRotate", OnStartRotate);
            EventHandler.UnregisterEvent(m_GameObject, "OnRPGMovementTypeStopRotate", OnStopRotate);
            EventHandler.UnregisterEvent<float>(m_GameObject, "OnRPGMovementTypeTurn", OnTurn);
            EventHandler.UnregisterEvent(m_GameObject, "OnRPGMovementTypeAutoMove", OnToggleAutoMove);
        }
    }
}