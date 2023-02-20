/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.UltimateCharacterController.Input;

namespace Opsive.UltimateCharacterController.Character.Abilities.Starters
{
    /// <summary>
    /// The AbilityStarter allows a custom object to decide when the ability should start.
    /// </summary>
    [System.Serializable]
    public abstract class AbilityStarter
    {
        protected Ability m_Ability;

        /// <summary>
        /// Initializes the starter to the specified ability.
        /// </summary>
        /// <param name="ability">The ability that owns the starter.</param>
        public virtual void Initialize(Ability ability) { m_Ability = ability; }

        /// <summary>
        /// Can the starter start the ability?
        /// </summary>
        /// <param name="playerInput">A reference to the input component.</param>
        /// <returns>True if the starter can start the ability.</returns>
        public abstract bool CanInputStartAbility(PlayerInput playerInput);

        /// <summary>
        /// The ability has started.
        /// </summary>
        public virtual void AbilityStarted() { }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        public virtual void AbilityStopped() { }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        public virtual void OnDestroy() { }
    }
}