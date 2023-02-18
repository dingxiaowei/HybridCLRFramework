/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character.Abilities.AI;
    using Opsive.UltimateCharacterController.Objects.CharacterAssist;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Moves the character to the specified start location. This ability will be called manually by the controller and should not be started by the user.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    [DefaultAllowPositionalInput(false)]
    [DefaultAllowRotationalInput(false)]
    public class MoveTowards : Ability
    {
        [Tooltip("The multiplier to apply to the input vector. Allows the character to move towards the destination faster.")]
        [SerializeField] protected float m_InputMultiplier = 1;
        [Tooltip("The amount of time it takes that the character has to be stuck before teleporting the character to the start location.")]
        [SerializeField] protected float m_InactiveTimeout = 1;
        [Tooltip("Should the OnEnableGameplayInpt event be sent to disable the input when the ability is active?")]
        [SerializeField] protected bool m_DisableGameplayInput;
        [Tooltip("The location that the Move Towards ability should move towards if the ability is not started by another ability.")]
        [SerializeField] protected MoveTowardsLocation m_IndependentStartLocation;

        public float InputMultiplier { get { return m_InputMultiplier; } set { m_InputMultiplier = value; } }
        public float InactiveTimeout { get { return m_InactiveTimeout; } set { m_InactiveTimeout = value; } }
        public bool DisableGameplayInput { get { return m_DisableGameplayInput; } set { m_DisableGameplayInput = value; } }
        [Shared.Utility.NonSerialized] public MoveTowardsLocation IndependentStartLocation { get { return m_IndependentStartLocation; } set { m_IndependentStartLocation = value; } }

        public override bool IsConcurrent { get { return true; } }
        public override bool ImmediateStartItemVerifier { get { return true; } }

        private MoveTowardsLocation m_MoveTowardsLocation;
        private Ability m_OnArriveAbility;

        private PathfindingMovement m_PathfindingMovement;
        private SpeedChange[] m_SpeedChangeAbilities;

        private float m_MovementMultiplier;
        private Vector3 m_TargetDirection;
        private bool m_Arrived;
        private bool m_PrecisionStartWait;

        private ScheduledEventBase m_ForceStartEvent;

        public MoveTowardsLocation StartLocation { get { return m_MoveTowardsLocation; } }
        public Ability OnArriveAbility { get { return m_OnArriveAbility; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            m_PathfindingMovement = m_CharacterLocomotion.GetAbility<PathfindingMovement>();
            m_SpeedChangeAbilities = m_CharacterLocomotion.GetAbilities<SpeedChange>();
        }

        /// <summary>
        /// Starts moving to the specified start location.
        /// </summary>
        /// <param name="startLocations">The locations the character can move towards. If multiple locations are possible then the closest valid location will be used.</param>
        /// <param name="onArriveAbility">The ability that should be started as soon as the character arrives at the location.</param>
        /// <returns>True if the MoveTowards ability is started.</returns>
        public bool StartMoving(MoveTowardsLocation[] startLocations, Ability onArriveAbility)
        {
            // MoveTowards doesn't need to start if there is no start location.
            if (startLocations == null || startLocations.Length == 0) {
                return false;
            }

            // The arrive ability must exist and be unique. If the ability is already set then StartMoving may have been triggered because the arrive ability
            // should start.
            if (onArriveAbility == null || onArriveAbility == m_OnArriveAbility) {
                return false;
            }

            // No reason to start if the character is already in a valid start location.
            for (int i = 0; i < startLocations.Length; ++i) {
                if (startLocations[i].IsPositionValid(m_Transform.position, m_Transform.rotation, m_CharacterLocomotion.Grounded) && startLocations[i].IsRotationValid(m_Transform.rotation)) {
                    return false;
                }
            }

            // The character needs to move - start the ability.
            m_OnArriveAbility = onArriveAbility;
            if (m_OnArriveAbility.Index < Index) {
                Debug.LogWarning($"Warning: {m_OnArriveAbility.GetType().Name} has a higher priority then the MoveTowards ability. This will cause unintended behavior.");
            }

            m_MoveTowardsLocation = GetClosestStartLocation(startLocations);

            StartAbility();

            // MoveTowards may be starting when all of the inputs are being checked. If it has a lower index then the update loop won't run initially
            // which will prevent the TargetDirection from having a valid value. Run the Update loop immediately so TargetDirection is correct.
            if (Index < onArriveAbility.Index) {
                Update();
            }

            return true;
        }

        /// <summary>
        /// Returns the closest start location out of the possible MoveTowardsLocations.
        /// </summary>
        /// <param name="startLocations">The locations the character can move towards.</param>
        /// <returns>The best location out of the possible MoveTowardsLocations.</returns>
        private MoveTowardsLocation GetClosestStartLocation(MoveTowardsLocation[] startLocations)
        {
            // If only one location is available then it is the closest.
            if (startLocations.Length == 1) {
                return startLocations[0];
            }

            // Multiple locations are available. Choose the closest location.
            MoveTowardsLocation startLocation = null;
            var closestDistance = float.MaxValue;
            float distance;
            for (int i = 0; i < startLocations.Length; ++i) {
                if ((distance = startLocations[i].GetTargetDirection(m_Transform.position, m_Transform.rotation).sqrMagnitude) < closestDistance) {
                    closestDistance = distance;
                    startLocation = startLocations[i];
                }
            }

            return startLocation;
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            if (!base.CanStartAbility()) {
                return false;
            }

            return m_MoveTowardsLocation != null || m_IndependentStartLocation != null;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            if (m_OnArriveAbility != null) {
                m_AllowEquippedSlotsMask = m_OnArriveAbility.AllowEquippedSlotsMask;
                m_OnArriveAbility.AbilityMessageCanStart = false;
            }

            base.AbilityStarted();
            m_Arrived = false;
            if (m_DisableGameplayInput) {
                EventHandler.ExecuteEvent(m_GameObject, "OnEnableGameplayInput", false);
            }

            // The MoveTowardsLocation may already be set by the starting ability within StartMoving.
            if (m_MoveTowardsLocation == null) {
                m_MoveTowardsLocation = m_IndependentStartLocation;
            }
            // The movement speed will depend on the current speed the character is moving.
            m_MovementMultiplier = m_MoveTowardsLocation.MovementMultiplier;
            if (m_SpeedChangeAbilities != null) {
                for (int i = 0; i < m_SpeedChangeAbilities.Length; ++i) {
                    if (m_SpeedChangeAbilities[i].IsActive) {
                        m_MovementMultiplier = m_SpeedChangeAbilities[i].SpeedChangeMultiplier;
                        break;
                    }
                }
            }
            // Use the pathfinding ability if the destination is a valid pathfinding destination.
            if (m_PathfindingMovement != null && m_PathfindingMovement.Index < Index) {
                m_PathfindingMovement.SetDestination(m_MoveTowardsLocation.TargetPosition);
            }

            // Force independent look so the ability will have complete control over the rotation.
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterForceIndependentLook", true);
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            // ItemEquipVerifier and EquipUnequip should never be blocked.
            if (startingAbility is ItemEquipVerifier || startingAbility is Items.EquipUnequip) {
                return false;
            }

            // Block the ability if it has a lower priority (higher index) then the MoveTowards ability. ItemAbilities have a different priority list.
            if (startingAbility.Index > Index || startingAbility is StoredInputAbilityBase) {
                return true;
            }

            // The arrive ability can determine if an ability should be blocked.
            if (m_OnArriveAbility != null) {
                return m_OnArriveAbility.ShouldBlockAbilityStart(startingAbility);
            }
            return false;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            if (activeAbility is StoredInputAbilityBase) {
                return true;
            }

            // The arrive ability can determine if an ability should be stopped.
            if (m_OnArriveAbility != null) {
                return m_OnArriveAbility.ShouldStopActiveAbility(activeAbility);
            }
            return false;
        }

        /// <summary>
        /// Updates the ability.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // The input values should move towards the target.
            var arrived = m_MoveTowardsLocation.IsRotationValid(m_Transform.rotation);
            m_TargetDirection = m_MoveTowardsLocation.GetTargetDirection(m_Transform.position, m_Transform.rotation);
            if (!m_MoveTowardsLocation.IsPositionValid(m_Transform.position, m_Transform.rotation, m_CharacterLocomotion.Grounded)) {
                if (m_PathfindingMovement == null || !m_PathfindingMovement.IsActive) {
                    m_CharacterLocomotion.InputVector = GetInputVector(m_TargetDirection);
                }
                arrived = false;
            } else if (!m_MoveTowardsLocation.PrecisionStart && (m_PathfindingMovement == null || !m_PathfindingMovement.IsActive) && 
                                                            (m_OnArriveAbility == null || m_OnArriveAbility.AllowPositionalInput)) {
                m_CharacterLocomotion.InputVector = m_CharacterLocomotion.RawInputVector;
            }

            if (arrived && !m_Arrived) {
                m_Arrived = true;
                // The character should completely stop moving when they have arrived when using a precision start. Return early to allow the animator
                // to start transitioning to the next frame.
                if (m_MoveTowardsLocation.PrecisionStart) {
                    m_CharacterLocomotion.ResetRotationPosition();
                    m_PrecisionStartWait = true;
                    return;
                }
            }

            // If the character isn't making any progress teleport them to the starting location and start the arrive ability.
            if (!m_Arrived) {
                if (m_CharacterLocomotion.Velocity.sqrMagnitude <= 0.0001f && m_CharacterLocomotion.Torque.eulerAngles.sqrMagnitude <= 0.0001f) {
                    if (m_ForceStartEvent == null) {
                        m_ForceStartEvent = Scheduler.Schedule(m_InactiveTimeout, ImmediateMove);
                    }
                } else if (m_ForceStartEvent != null) {
                    Scheduler.Cancel(m_ForceStartEvent);
                    m_ForceStartEvent = null;
                }
            }

            // Keep the MoveTowards ability active until the character has arrived at the destination and the ItemEquipVerifier ability isn't active.
            // This will prevent the character from sliding when ItemEquipVerifier is active and MoveTowards is not active.
            if (arrived && (m_CharacterLocomotion.ItemEquipVerifierAbility == null || !m_CharacterLocomotion.ItemEquipVerifierAbility.IsActive)) {
                if (!m_MoveTowardsLocation.PrecisionStart || !m_PrecisionStartWait) {
                    StopAbility();
                } else {
                    // After the character is no longer in transition the arrive ability can start. This will ensure the character always starts in the correct location.
                    // For some abilities it doesn't matter if the character is in a precise position and in that case the precision start field can be disabled.
                    if (m_MoveTowardsLocation.PrecisionStart && !m_AnimatorMonitor.IsInTransition(0)) {
                        m_PrecisionStartWait = false;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the rotation that the character should rotate towards.
        /// </summary>
        /// <returns>The rotation that the character should rotate towards.</returns>
        protected virtual Quaternion GetTargetRotation()
        {
            return Quaternion.LookRotation(m_MoveTowardsLocation.TargetRotation * Vector3.forward, m_CharacterLocomotion.Up);
        }

        /// <summary>
        /// Returns the input vector that the character should move with.
        /// </summary>
        /// <param name="direction">The direction that the character should move towards.</param>
        /// <returns>The input vector that the character should move with.</returns>
        protected virtual Vector2 GetInputVector(Vector3 direction)
        {
            var inputVector = Vector2.zero;
            inputVector.x = direction.x;
            inputVector.y = direction.z;
            return inputVector.normalized * m_InputMultiplier * m_MovementMultiplier;
        }

        /// <summary>
        /// Update the controller's rotation values.
        /// </summary>
        public override void UpdateRotation()
        {
            if (m_PathfindingMovement != null && m_PathfindingMovement.IsActive) {
                return;
            }

            var rotation = GetTargetRotation() * Quaternion.Inverse(m_Transform.rotation);
            var deltaRotation = m_CharacterLocomotion.DeltaRotation;
            deltaRotation.y = Mathf.MoveTowards(0, MathUtility.ClampInnerAngle(rotation.eulerAngles.y),
                                                        m_CharacterLocomotion.MotorRotationSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime);
            m_CharacterLocomotion.DeltaRotation = deltaRotation;
        }

        /// <summary>
        /// Ensure the move direction is valid.
        /// </summary>
        public override void ApplyPosition()
        {
            if (m_PathfindingMovement != null && m_PathfindingMovement.IsActive) {
                return;
            }

            // Prevent the character from jittering back and forth to land precisely on the target.
            var moveDirection = m_Transform.InverseTransformDirection(m_CharacterLocomotion.MoveDirection);
            if (Mathf.Abs(moveDirection.x) > Mathf.Abs(m_TargetDirection.x)) {
                moveDirection.x = m_TargetDirection.x;
            }
            if (Mathf.Abs(moveDirection.z) > Mathf.Abs(m_TargetDirection.z)) {
                moveDirection.z = m_TargetDirection.z;
            }
            m_CharacterLocomotion.MoveDirection = m_Transform.TransformDirection(moveDirection);
        }

        /// <summary>
        /// Immediately moves to the target position/rotation.
        /// </summary>
        private void ImmediateMove()
        {
            m_CharacterLocomotion.SetPositionAndRotation(m_MoveTowardsLocation.TargetPosition, m_MoveTowardsLocation.TargetRotation, true, false);
            m_ForceStartEvent = null;
            StopAbility();
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            m_MoveTowardsLocation = null;
            if (force) {
                m_OnArriveAbility = null;
            }
            if (m_ForceStartEvent != null) {
                Scheduler.Cancel(m_ForceStartEvent);
                m_ForceStartEvent = null;
            }
            if (m_DisableGameplayInput) {
                EventHandler.ExecuteEvent(m_GameObject, "OnEnableGameplayInput", true);
            }

            // Reset the force independet look parameter set within StartAbility.
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterForceIndependentLook", false);

            // Start the OnArriveAbility after MoveTowards has stopped to prevent MoveTowards from affecting the arrive ability.
            if (m_OnArriveAbility != null) {
                m_CharacterLocomotion.TryStartAbility(m_OnArriveAbility, true, true);
                m_OnArriveAbility = null;
            }
        }
    }
}