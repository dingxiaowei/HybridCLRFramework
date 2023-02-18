/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.AI
{
    using UnityEngine;

    /// <summary>
    /// Base class for moving the character with a pathfinding implementation.
    /// </summary>
    public abstract class PathfindingMovement : Ability
    {
        public override bool IsConcurrent { get { return true; } }

        /// <summary>
        /// Returns the desired input vector value. This will be used by the Ultimate Character Locomotion componnet.
        /// </summary>
        public abstract Vector2 InputVector { get; }
        /// <summary>
        /// Returns the desired rotation value. This will be used by the Ultimate Character Locomotion component.
        /// </summary>
        public abstract Vector3 DeltaRotation { get; }

        /// <summary>
        /// Sets the destination of the pathfinding agent.
        /// </summary>
        /// <param name="target">The position to move towards.</param>
        /// <returns>True if the destination was set.</returns>
        public abstract bool SetDestination(Vector3 target);

        /// <summary>
        /// Updates the character's input values.
        /// </summary>
        public override void Update()
        {
            m_CharacterLocomotion.InputVector = InputVector;
        }

        /// <summary>
        /// Updates the character's rotation values.
        /// </summary>
        public override void UpdateRotation()
        {
            m_CharacterLocomotion.DeltaRotation = DeltaRotation;
        }
    }
}