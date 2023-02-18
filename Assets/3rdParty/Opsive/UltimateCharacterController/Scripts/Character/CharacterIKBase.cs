/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.UltimateCharacterController.StateSystem;
    using System;
    using UnityEngine;

    /// <summary>
    /// Base class for applying IK on the character. Allows other IK solutions to easily be used instead of Unity's IK system.
    /// </summary>
    public abstract class CharacterIKBase : StateBehavior
    {
        // Specifies the limb affected by IK.
        public enum IKGoal
        {
            LeftHand,   // The character's left hand.
            LeftElbow,  // The character's left elbow.
            RightHand,  // The character's right hand.
            RightElbow, // The character's right elbow.
            LeftFoot,   // The character's left foot.
            LeftKnee,   // The character's left knee.
            RightFoot,  // The character's right foot.
            RightKnee,  // The character's right knee.
            Last        // The last entry in the enum - used to detect the number of values.
        }

        // Other objects can modify the final ik position/rotation before it is sent to the IK implementation.
        protected Func<IKGoal, Vector3, Quaternion, Vector3> m_OnUpdateIKPosition;
        protected Func<IKGoal, Quaternion, Vector3, Quaternion> m_OnUpdateIKRotation;
        public Func<IKGoal, Vector3, Quaternion, Vector3> OnUpdateIKPosition { get { return m_OnUpdateIKPosition; } set { m_OnUpdateIKPosition = value; } }
        public Func<IKGoal, Quaternion, Vector3, Quaternion> OnUpdateIKRotation { get { return m_OnUpdateIKRotation; } set { m_OnUpdateIKRotation = value; } }

        /// <summary>
        /// Updates the IK component after the animator has updated.
        /// </summary>
        /// <param name="fixedUpdate">Is the IK being updated within the FixedUpdate loop?</param>
        public abstract void Move(bool fixedUpdate);

        /// <summary>
        /// Specifies the location of the left or right hand IK target and IK hint target.
        /// </summary>
        /// <param name="itemTransform">The transform of the item.</param>
        /// <param name="itemHand">The hand that the item is parented to.</param>
        /// <param name="nonDominantHandTarget">The target of the left or right hand. Can be null.</param>
        /// <param name="nonDominantHandElbowTarget">The target of the left or right elbow. Can be null.</param>
        public abstract void SetItemIKTargets(Transform itemTransform, Transform itemHand, Transform nonDominantHandTarget, Transform nonDominantHandElbowTarget);

        /// <summary>
        /// Specifies the target location of the limb.
        /// </summary>
        /// <param name="target">The target location of the limb.</param>
        /// <param name="ikGoal">The limb affected by the target location.</param>
        /// <param name="duration">The amount of time it takes to reach the goal.</param>
        public abstract void SetAbilityIKTarget(Transform target, IKGoal ikGoal, float duration);
    }
}