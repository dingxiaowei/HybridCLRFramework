/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;
using Opsive.UltimateCharacterController.Input;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// The RPG ViewType uses a control scheme similar to the standard in the RPG genre. The ViewType works with the RPG MovementType to move and rotate the camera.
    /// </summary>
    [UltimateCharacterController.Camera.ViewTypes.RecommendedMovementType(typeof(Character.MovementTypes.RPG))]
    public class RPG : ThirdPerson
    {
        [Tooltip("The dapming of the yaw angle when it snaps back to behind the character as the character moves.")]
        [SerializeField] protected float m_YawSnapDamping = 0.1f;
        [Tooltip("Can the camera move freely with the press of the button specified by the free movement input name? ")]
        [SerializeField] protected bool m_AllowFreeMovement;
        [Tooltip("The name of the camera free movement input mapping.")]
        [SerializeField] protected string m_CameraFreeMovementInputName = "Fire1";

        public float YawSnapDamping { get { return m_YawSnapDamping; } set { m_YawSnapDamping = value; } }
        public bool AllowFreeMovement { get { return m_AllowFreeMovement; } set { m_AllowFreeMovement = value; } }
        public string CameraFreeMovementInputName { get { return m_CameraFreeMovementInputName; } set { m_CameraFreeMovementInputName = value; } }

        private ActiveInputEvent m_StartFreeMovementInputEvent;
        private ActiveInputEvent m_StopFreeMovementInputEvent;

        private bool m_FreeMovement;
        private float m_YawOffset;
        private float m_YawSnapVelocity;
        private bool m_CharacterRotate;
        private bool m_RotateWithCharacter;

        /// <summary>
        /// Attaches the view type to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the camera to.</param>
        public override void AttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<Ability, bool>(m_Character, "OnCharacterAbilityActive", OnAbilityActive);
            }

            base.AttachCharacter(character);

            if (m_Character != null) {
                m_RotateWithCharacter = false;
                EventHandler.RegisterEvent<Ability, bool>(m_Character, "OnCharacterAbilityActive", OnAbilityActive);
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
            base.ChangeViewType(activate, pitch, yaw, characterRotation);

            if (activate) {
                // Work with the handler to listen for any input events.
                if (m_Handler != null) {
                    m_StartFreeMovementInputEvent = ObjectPool.Get<ActiveInputEvent>();
                    m_StartFreeMovementInputEvent.Initialize(ActiveInputEvent.Type.ButtonDown, m_CameraFreeMovementInputName, "OnRPGViewTypeStartFreeMovement");

                    m_StopFreeMovementInputEvent = ObjectPool.Get<ActiveInputEvent>();
                    m_StopFreeMovementInputEvent.Initialize(ActiveInputEvent.Type.ButtonUp, m_CameraFreeMovementInputName, "OnRPGViewTypeStopFreeMovement");

                    m_Handler.RegisterInputEvent(m_StartFreeMovementInputEvent);
                }
                EventHandler.RegisterEvent(m_GameObject, "OnRPGViewTypeStartFreeMovement", OnStartFreeMovement);
                EventHandler.RegisterEvent(m_GameObject, "OnRPGViewTypeStopFreeMovement", OnStopFreeMovement);
                EventHandler.RegisterEvent(m_Character, "OnRPGMovementTypeStartRotate", OnStartCharacterRotate);
                EventHandler.RegisterEvent(m_Character, "OnRPGMovementTypeStopRotate", OnStopCharacterRotate);
            } else {
                // The ViewType no longer needs to listen for input events when to ViewType is no longer active.
                if (m_Handler != null) {
                    if (m_FreeMovement) {
                        m_Handler.UnregisterAbilityInputEvent(m_StopFreeMovementInputEvent);
                    } else {
                        m_Handler.UnregisterAbilityInputEvent(m_StartFreeMovementInputEvent);
                    }

                    ObjectPool.Return(m_StartFreeMovementInputEvent);
                    ObjectPool.Return(m_StopFreeMovementInputEvent);
                }
                EventHandler.UnregisterEvent(m_GameObject, "OnRPGViewTypeStartFreeMovement", OnStartFreeMovement);
                EventHandler.UnregisterEvent(m_GameObject, "OnRPGViewTypeStopFreeMovement", OnStopFreeMovement);
                EventHandler.UnregisterEvent(m_Character, "OnRPGMovementTypeStartRotate", OnStartCharacterRotate);
                EventHandler.UnregisterEvent(m_Character, "OnRPGMovementTypeStopRotate", OnStopCharacterRotate);
            }
        }

        /// <summary>
        /// Rotates the camera according to the horizontal and vertical movement values.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediatePosition">Should the camera be positioned immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediatePosition)
        {
            // The character may be controlling the rotation rather than the camera.
            if (m_RotateWithCharacter) {
                m_CharacterRotation = m_CharacterTransform.rotation;
                horizontalMovement = 0;
            }

            if (m_CharacterRotate) {
                m_Yaw += horizontalMovement * m_CharacterLocomotion.TimeScale * Time.timeScale;
            } else {
                // FreeMovement allows the camera's yaw to freely rotate around the character. This will be true when Fire1 is down. YawSnap will be true
                // when the character starts to move.
                if (m_FreeMovement) {
                    m_YawOffset += horizontalMovement * m_CharacterLocomotion.TimeScale * Time.timeScale;
                } else if (m_CharacterLocomotion.Moving) {
                    m_YawOffset = Mathf.SmoothDamp(m_YawOffset, 0, ref m_YawSnapVelocity, m_YawSnapDamping * m_CharacterLocomotion.TimeScale * Time.timeScale * m_CharacterLocomotion.FramerateDeltaTime);
                }
                var deltaRotation = MathUtility.InverseTransformQuaternion(m_CharacterRotation, m_CharacterTransform.rotation);
                m_Yaw = deltaRotation.eulerAngles.y + m_YawOffset;
            }

            // The camera can only rotate along the pitch if MovementType.RPG.RotateInputName or CameraFreeMovementInputName is down.
            if (!m_FreeMovement && !m_CharacterRotate) {
                verticalMovement = 0;
            }

            return base.Rotate(horizontalMovement, verticalMovement, immediatePosition);
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
            if (!characterLookDirection) {
                return base.LookDirection(lookPosition, characterLookDirection, layerMask, useRecoil);
            }
            return m_CharacterTransform.forward;
        }

        /// <summary>
        /// The ViewType should move freely around the yaw axis.
        /// </summary>
        private void OnStartFreeMovement()
        {
            // Free movement may not be allowed if the character doesn't attack automatically.
            if (!m_AllowFreeMovement) {
                return;
            }

            m_FreeMovement = true;

            // The ViewType doesn't need to listen for the start input anymore and should instead listen for the stop.
            if (m_Handler != null && m_AllowFreeMovement) {
                m_Handler.UnregisterAbilityInputEvent(m_StartFreeMovementInputEvent);
                m_Handler.RegisterInputEvent(m_StopFreeMovementInputEvent);
            }
        }

        /// <summary>
        /// The ViewType should no longer move freely around the yaw axis.
        /// </summary>
        private void OnStopFreeMovement()
        {
            m_FreeMovement = false;

            // The ViewType doesn't need to listen for the stop input anymore and should instead listen for the start.
            if (m_Handler != null) {
                m_Handler.UnregisterAbilityInputEvent(m_StopFreeMovementInputEvent);
                m_Handler.RegisterInputEvent(m_StartFreeMovementInputEvent);
            }
        }

        /// <summary>
        /// The character has started to rotate.
        /// </summary>
        private void OnStartCharacterRotate()
        {
            m_CharacterRotate = true;
            m_YawOffset = 0;
        }

        /// <summary>
        /// The character has stopped rotating.
        /// </summary>
        private void OnStopCharacterRotate()
        {
            m_CharacterRotate = false;
        }

        /// <summary>
        /// The character's ability has been started or stopped.
        /// </summary>
        /// <param name="ability">The ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnAbilityActive(Ability ability, bool active)
        {
            if (!(ability is MoveTowards)) {
                return;
            }

            // Rotate with the camera so the camera will follow the character's rotation when the character is getting into position for Move Towards.
            m_RotateWithCharacter = active;

            // When rotate with character is enabled the CharacterRotation quaternion will update to the character's current rotation so the camera moves with the
            // character rather than the character moving with the camera. Set to yaw to 0 to prevent a snapping when the CharacterRotation quaternion is updated.
            if (m_RotateWithCharacter) {
                m_Yaw = 0;
            }
        }
    }
}