/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.BeginEndActions
{
    using Opsive.UltimateCharacterController.StateSystem;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Start End Actions will perform an action when the Magic Item use is beginning or is ending.
    /// </summary>
    [System.Serializable]
    [UnityEngine.Scripting.Preserve]
    [AllowDuplicateTypes]
    public abstract class BeginEndAction : StateObject
    {
        protected MagicItem m_MagicItem;
        protected bool m_BeginAction;
        protected int m_Index;

        /// <summary>
        /// Initializes the BeginEndAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the BeginEndAction belongs to.</param>
        /// <param name="startAction">True if the action is a begin action.</param>
        /// <param name="index">The index of the BeginEndAction.</param>
        public virtual void Initialize(GameObject character, MagicItem magicItem, bool beginAction, int index)
        {
            base.Initialize(character);

            m_MagicItem = magicItem;
            m_BeginAction = beginAction;
            m_Index = index;
        }

        /// <summary>
        /// The action has started.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public virtual void Start(Transform origin) { }

        /// <summary>
        /// Updates the action.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// The action has stopped.
        /// </summary>
        public virtual void Stop() { }

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public virtual void OnChangePerspectives(Transform origin) { }

        /// <summary>
        /// The action has been destroyed.
        /// </summary>
        public virtual void OnDestroy() { }
    }
}