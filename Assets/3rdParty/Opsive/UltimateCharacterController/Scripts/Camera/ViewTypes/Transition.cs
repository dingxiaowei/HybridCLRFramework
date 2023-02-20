/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Utility;
using Opsive.UltimateCharacterController.Events;

namespace Opsive.UltimateCharacterController.Camera.ViewTypes
{
    /// <summary>
    /// The Transition View Type will transition the camera from one view type to another.
    /// </summary>
    public class Transition : ViewType
    {
        [Tooltip("The amount of time to transition from a third person to first person person. 0 will disable the transition.")]
        [SerializeField] protected float m_FirstToThirdTransitionDuration = 0.1f;
        [Tooltip("The starting offset when transition from a first person perspective to third person.")]
        [SerializeField] protected Vector3 m_StartFirstPersonCameraOffset = new Vector3(0, 0, -0.4f);
        [Tooltip("The amount of time to transition from a third person to first person perspective. 0 will disable the transition.")]
        [SerializeField] protected float m_ThirdToFirstTransitionDuration = 0.1f;
        [Tooltip("The ending offset when transition from a third person perspective  to first person.")]
        [SerializeField] protected Vector3 m_EndFirstPersonCameraOffset = new Vector3(0, 0, -0.4f);
        [Tooltip("The amount of time to transition from a third person to another third person perspective. 0 will disable the transition.")]
        [SerializeField] protected float m_ThirdToThirdTransitionDuration = 0.1f;

        public float FirstToThirdTransitionDuration { get { return m_FirstToThirdTransitionDuration; } set { m_FirstToThirdTransitionDuration = value; } }
        public Vector3 StartFirstPersonCameraOffset { get { return m_StartFirstPersonCameraOffset; } set { m_StartFirstPersonCameraOffset = value; } }
        public float ThirdToFirstTransitionDuration { get { return m_ThirdToFirstTransitionDuration; } set { m_ThirdToFirstTransitionDuration = value; } }
        public Vector3 EndFirstPersonCameraOffset { get { return m_EndFirstPersonCameraOffset; } set { m_EndFirstPersonCameraOffset = value; } }
        public float ThirdToThirdTransitionDuration { get { return m_ThirdToThirdTransitionDuration; } set { m_ThirdToThirdTransitionDuration = value; } }

        private bool m_IsTransitioning;
        private ViewType m_TransitionFrom;
        private ViewType m_TransitionTo;
        private Quaternion m_StartRotation;
        private Vector3 m_StartPosition;
        private float m_TransitionDuration;
        private float m_CurrentTransition;
        private float m_StartTransition;
        private bool m_NotifyTransitionComplete;

        public override float Pitch { get { return 0; } }
        public override float Yaw { get { return 0; } }
        public override Quaternion CharacterRotation { get { return Quaternion.identity; } }
        public override float LookDirectionDistance { get { return m_TransitionTo.LookDirectionDistance; } }
        public override bool FirstPersonPerspective { get { return false; } }

        public bool IsTransitioning { get { return m_IsTransitioning; } }

        /// <summary>
        /// Start the transition if it can be started.
        /// </summary>
        /// <param name="fromViewType">The originating view type.</param>
        /// <param name="toViewType">The destination view type.</param>
        /// <returns>True if the transition can be started.</returns>
        public bool StartTransition(ViewType fromViewType, ViewType toViewType)
        {
            // Allow the transition if the duration is positive.
            var fromFirstPerson = fromViewType.FirstPersonPerspective;
            var toFirstPerson = toViewType.FirstPersonPerspective;
            if (fromFirstPerson && !toFirstPerson) {
                if (m_FirstToThirdTransitionDuration == 0) {
                    return false; // No transition necessary.
                }
                m_TransitionDuration = m_FirstToThirdTransitionDuration;
            } else if (!fromFirstPerson && toFirstPerson) {
                if (m_ThirdToFirstTransitionDuration == 0) {
                    return false; // No transition necessary.
                }
                m_TransitionDuration = m_ThirdToFirstTransitionDuration;
            } else if (!fromFirstPerson && !toFirstPerson) {
                if (m_ThirdToThirdTransitionDuration == 0) {
                    return false; // No transition necessary.
                }
                m_TransitionDuration = m_ThirdToThirdTransitionDuration;
            } else {
                // There is no first to first person transition.
                return false;
            }

            // Setup for a transition.
            m_TransitionFrom = fromViewType;
            m_TransitionTo = toViewType;
            if (fromFirstPerson && !m_IsTransitioning) { // IsTransitioning will be true if a new transition has started before the previous one completed.
                m_StartPosition = m_Transform.TransformPoint(m_StartFirstPersonCameraOffset);
            } else {
                m_StartPosition = m_Transform.position;
            }
            m_StartRotation = m_Transform.rotation;
            m_CurrentTransition = 0f;
            m_StartTransition = Time.time;

            // Execute the camera change event immediately if coming from first person so the character model will be correctly shown in third person.
            if (fromFirstPerson) {
                EventHandler.ExecuteEvent<bool>(m_Character, "OnCameraChangePerspectives", m_TransitionTo.FirstPersonPerspective);
            }
            m_IsTransitioning = true;

            return true;
        }

        /// <summary>
        /// Rotates the camera to face the character.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediatePosition">Should the camera be positioned immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediatePosition)
        {
            // Notify those interested when the transition is complete.
            if (m_NotifyTransitionComplete) {
                StopTransition(true);
                m_NotifyTransitionComplete = false;
                return m_TransitionTo.Rotate(horizontalMovement, verticalMovement, true);
            }

            // Lerp the transition value within Rotate because Rotate is called before Move.
            m_CurrentTransition = Mathf.Lerp(0, 1, (Time.time - m_StartTransition) / (m_TransitionDuration / m_CharacterLocomotion.TimeScale));

            // A lerp is already being performed so the destination view type doesn't need to do any interpolation - immediate position should be set to true.
            return Quaternion.Slerp(m_StartRotation, m_TransitionTo.Rotate(horizontalMovement, verticalMovement, true), m_CurrentTransition);
        }

        /// <summary>
        /// Moves the camera to face the character.
        /// </summary>
        /// <param name="immediatePosition">Should the camera be positioned immediately?</param>
        /// <returns>The updated position.</returns>
        public override Vector3 Move(bool immediatePosition)
        {
            // A lerp is already being performed so the destination view type doesn't need to do any interpolation - immediate position should be set to true.
            var targetPosition = m_TransitionTo.Move(true);
            if (m_TransitionTo.FirstPersonPerspective) {
                targetPosition = MathUtility.TransformPoint(targetPosition, m_CameraController.Anchor.rotation, m_EndFirstPersonCameraOffset);
            }

            // The transition should be stopped within rotate (which is called within FixedUpdate) so the interested objects receive the notification in the correct order.
            // The Move method needs to be run before the transition can complete though.
            if (m_CurrentTransition == 1) {
                m_NotifyTransitionComplete = true;
                return targetPosition;
            }

            var t = (immediatePosition ? 1 : m_CurrentTransition);
            targetPosition.x = Mathf.SmoothStep(m_StartPosition.x, targetPosition.x, t);
            targetPosition.y = Mathf.SmoothStep(m_StartPosition.y, targetPosition.y, t);
            targetPosition.z = Mathf.SmoothStep(m_StartPosition.z, targetPosition.z, t);
            return targetPosition;
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
            // Don't use the first person ViewType for the look direction while in a transition. The transition will be at other locations
            // other than the character's head position so a first person look direction will return an invalid direction.
            if (m_TransitionTo.FirstPersonPerspective) {
                return m_TransitionFrom.LookDirection(lookPosition, characterLookDirection, layerMask, useRecoil);
            }
            return m_TransitionTo.LookDirection(lookPosition, characterLookDirection, layerMask, useRecoil);
        }

        /// <summary>
        /// Stops the transition.
        /// </summary>
        /// <param name="success">Was the transition a success?</param>
        public void StopTransition(bool success)
        {
            // The transition may not be started.
            if (!m_IsTransitioning) {
                return;
            }

            m_IsTransitioning = false;
            // The first person perspective will only execute the event after the camera has arrived. Third person will execute the event immediately.
            if (success && m_TransitionTo.FirstPersonPerspective) {
                EventHandler.ExecuteEvent<bool>(m_Character, "OnCameraChangePerspectives", m_TransitionTo.FirstPersonPerspective);
            }
        }
    }
}