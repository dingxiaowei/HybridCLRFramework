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
    /// The Generic ability allows for new animations without having to explicitly code a new ability. The ability will end after a specified duration or 
    /// the OnAnimatorGenericAbilityComplete event is sent.
    /// </summary>
    [AllowMultipleAbilityTypes]
    [DefaultAbilityIndex(10000)]
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Action")]
    public class Generic : Ability
    {
        [Tooltip("Specifies if the ability should stop when the OnAnimatorGenericAbilityComplete event is received or wait the specified amount of time before ending the ability.")]
        [SerializeField] protected AnimationEventTrigger m_StopEvent = new AnimationEventTrigger(false, 0.5f);

        public AnimationEventTrigger StopEvent { get { return m_StopEvent; } set { m_StopEvent = value; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorGenericAbilityComplete", OnComplete);
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

            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorGenericAbilityComplete", OnComplete);
        }
    }
}