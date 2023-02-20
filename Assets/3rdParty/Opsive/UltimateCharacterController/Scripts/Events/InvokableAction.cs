/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using System;

namespace Opsive.UltimateCharacterController.Events
{
    /// <summary>
    /// Base class which allows for the Action event system to be pooled.
    /// </summary>
    internal abstract class InvokableActionBase
    {
#if UNITY_EDITOR
        public abstract Delegate GetDelegate();
#endif
    }

    /// <summary>
    /// Adds a layer to the Action event system to allow actions to be pooled.
    /// </summary>
    internal class InvokableAction : InvokableActionBase
    {
        private event Action m_Action;

        /// <summary>
        /// Initializes the action to the specificed function.
        /// </summary>
        /// <param name="action">The function to initialize the action to.</param>
        public void Initialize(Action action)
        {
            m_Action = action;
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        public void Invoke()
        {
            m_Action();
        }

        /// <summary>
        /// Does the inputted action match the object that the InvokeableAction represents?
        /// </summary>
        /// <param name="action">The action to test against.</param>
        /// <returns>True if the actions match.</returns>
        public bool IsAction(Action action)
        {
            return m_Action == action;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Provides an accessor to the action for error checking. Only needed in editor mode.
        /// </summary>
        /// <returns>The action that the InvokableAction object represents.</returns>
        public override Delegate GetDelegate() { return m_Action; }
#endif
    }

    /// <summary>
    /// Adds a layer to the Action event system to allow actions to be pooled.
    /// </summary>
    internal class InvokableAction<T1> : InvokableActionBase
    {
        private event Action<T1> m_Action;

        /// <summary>
        /// Initializes the action to the specificed function.
        /// </summary>
        /// <param name="action">The function to initialize the action to.</param>
        public void Initialize(Action<T1> action)
        {
            m_Action = action;
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="arg1">The first parameter.</param>
        public void Invoke(T1 arg1)
        {
            m_Action(arg1);
        }

        /// <summary>
        /// Does the inputted action match the object that the InvokeableAction represents?
        /// </summary>
        /// <param name="action">The action to test against.</param>
        /// <returns>True if the actions match.</returns>
        public bool IsAction(Action<T1> action)
        {
            return m_Action == action;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Provides an accessor to the action for error checking. Only needed in editor mode.
        /// </summary>
        /// <returns>The action that the InvokableAction object represents.</returns>
        public override Delegate GetDelegate() { return m_Action; }
#endif
    }

    /// <summary>
    /// Adds a layer to the Action event system to allow actions to be pooled.
    /// </summary>
    internal class InvokableAction<T1, T2> : InvokableActionBase
    {
        private event Action<T1, T2> m_Action;

        /// <summary>
        /// Initializes the action to the specificed function.
        /// </summary>
        /// <param name="action">The function to initialize the action to.</param>
        public void Initialize(Action<T1, T2> action)
        {
            m_Action = action;
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        public void Invoke(T1 arg1, T2 arg2)
        {
            m_Action(arg1, arg2);
        }

        /// <summary>
        /// Does the inputted action match the object that the InvokeableAction represents?
        /// </summary>
        /// <param name="action">The action to test against.</param>
        /// <returns>True if the actions match.</returns>
        public bool IsAction(Action<T1, T2> action)
        {
            return m_Action == action;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Provides an accessor to the action for error checking. Only needed in editor mode.
        /// </summary>
        /// <returns>The action that the InvokableAction object represents.</returns>
        public override Delegate GetDelegate() { return m_Action; }
#endif
    }

    /// <summary>
    /// Adds a layer to the Action event system to allow actions to be pooled.
    /// </summary>
    internal class InvokableAction<T1, T2, T3> : InvokableActionBase
    {
        private event Action<T1, T2, T3> m_Action;

        /// <summary>
        /// Initializes the action to the specificed function.
        /// </summary>
        /// <param name="action">The function to initialize the action to.</param>
        public void Initialize(Action<T1, T2, T3> action)
        {
            m_Action = action;
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        public void Invoke(T1 arg1, T2 arg2, T3 arg3)
        {
            m_Action(arg1, arg2, arg3);
        }

        /// <summary>
        /// Does the inputted action match the object that the InvokeableAction represents?
        /// </summary>
        /// <param name="action">The action to test against.</param>
        /// <returns>True if the actions match.</returns>
        public bool IsAction(Action<T1, T2, T3> action)
        {
            return m_Action == action;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Provides an accessor to the action for error checking. Only needed in editor mode.
        /// </summary>
        /// <returns>The action that the InvokableAction object represents.</returns>
        public override Delegate GetDelegate() { return m_Action; }
#endif
    }

    /// <summary>
    /// Adds a layer to the Action event system to allow actions to be pooled.
    /// </summary>
    internal class InvokableAction<T1, T2, T3, T4> : InvokableActionBase
    {
        private event Action<T1, T2, T3, T4> m_Action;

        /// <summary>
        /// Initializes the action to the specificed function.
        /// </summary>
        /// <param name="action">The function to initialize the action to.</param>
        public void Initialize(Action<T1, T2, T3, T4> action)
        {
            m_Action = action;
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        /// <param name="arg4">The fourth parameter.</param>
        public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            m_Action(arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// Does the inputted action match the object that the InvokeableAction represents?
        /// </summary>
        /// <param name="action">The action to test against.</param>
        /// <returns>True if the actions match.</returns>
        public bool IsAction(Action<T1, T2, T3, T4> action)
        {
            return m_Action == action;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Provides an accessor to the action for error checking. Only needed in editor mode.
        /// </summary>
        /// <returns>The action that the InvokableAction object represents.</returns>
        public override Delegate GetDelegate() { return m_Action; }
#endif
    }

    /// <summary>
    /// Allows for an action with five parameters.
    /// </summary>
    public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    /// <summary>
    /// Adds a layer to the Action event system to allow actions to be pooled.
    /// </summary>
    internal class InvokableAction<T1, T2, T3, T4, T5> : InvokableActionBase
    {
        private event Action<T1, T2, T3, T4, T5> m_Action;

        /// <summary>
        /// Initializes the action to the specificed function.
        /// </summary>
        /// <param name="action">The function to initialize the action to.</param>
        public void Initialize(Action<T1, T2, T3, T4, T5> action)
        {
            m_Action = action;
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        /// <param name="arg4">The fourth parameter.</param>
        /// <param name="arg5">The fifth parameter.</param>
        public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            m_Action(arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// Does the inputted action match the object that the InvokeableAction represents?
        /// </summary>
        /// <param name="action">The action to test against.</param>
        /// <returns>True if the actions match.</returns>
        public bool IsAction(Action<T1, T2, T3, T4, T5> action)
        {
            return m_Action == action;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Provides an accessor to the action for error checking. Only needed in editor mode.
        /// </summary>
        /// <returns>The action that the InvokableAction object represents.</returns>
        public override Delegate GetDelegate() { return m_Action; }
#endif
    }

    /// <summary>
    /// Allows for an action with six parameters.
    /// </summary>
    public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    /// <summary>
    /// Adds a layer to the Action event system to allow actions to be pooled.
    /// </summary>
    internal class InvokableAction<T1, T2, T3, T4, T5, T6> : InvokableActionBase
    {
        private event Action<T1, T2, T3, T4, T5, T6> m_Action;

        /// <summary>
        /// Initializes the action to the specificed function.
        /// </summary>
        /// <param name="action">The function to initialize the action to.</param>
        public void Initialize(Action<T1, T2, T3, T4, T5, T6> action)
        {
            m_Action = action;
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        /// <param name="arg4">The fourth parameter.</param>
        /// <param name="arg5">The fifth parameter.</param>
        /// <param name="arg6">The sixth parameter.</param>
        public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            m_Action(arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// Does the inputted action match the object that the InvokeableAction represents?
        /// </summary>
        /// <param name="action">The action to test against.</param>
        /// <returns>True if the actions match.</returns>
        public bool IsAction(Action<T1, T2, T3, T4, T5, T6> action)
        {
            return m_Action == action;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Provides an accessor to the action for error checking. Only needed in editor mode.
        /// </summary>
        /// <returns>The action that the InvokableAction object represents.</returns>
        public override Delegate GetDelegate() { return m_Action; }
#endif
    }
}
