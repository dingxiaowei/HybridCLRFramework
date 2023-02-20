/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// Plays a full body animation in response to a melee counter attack.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    [DefaultStopType(AbilityStopType.Manual)]
    [DefaultUseRootMotionPosition(AbilityBoolOverride.True)]
    [DefaultAbilityIndex(13)]
    public class MeleeCounterAttackResponse : Ability
    {
        [Tooltip("Specifies if the ability should stop when the OnAnimatorMeleeCounterAttackResponseComplete event is received or wait the specified amount of time before ending the ability.")]
        [SerializeField] protected AnimationEventTrigger m_StopEvent = new AnimationEventTrigger(true, 0.2f);

        public AnimationEventTrigger StopEvent { get { return m_StopEvent; } set { m_StopEvent = value; } }

        private int m_ResponseID;

        public override int AbilityIntData { get { return m_ResponseID; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorMeleeCounterAttackResponseComplete", OnComplete);
        }

        /// <summary>
        /// The character has been counter attacked. Play a response animation.
        /// </summary>
        /// <param name="id">The ID of the counter attack.</param>
        public void StartResponse(int id)
        {
            m_ResponseID = id;
            StartAbility();
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            if (!m_StopEvent.WaitForAnimationEvent) {
                Scheduler.ScheduleFixed(m_StopEvent.Duration, OnComplete);
            }
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            return true;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            if (activeAbility is Items.Use) {
                return true;
            }
            return base.ShouldStopActiveAbility(activeAbility);
        }

        /// <summary>
        /// The animation is done playing - stop the ability.
        /// </summary>
        private void OnComplete()
        {
            StopAbility();
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorMeleeCounterAttackResponseComplete", OnComplete);
        }
    }
}