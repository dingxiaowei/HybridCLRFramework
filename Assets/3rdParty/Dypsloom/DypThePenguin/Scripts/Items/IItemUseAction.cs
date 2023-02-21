/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Item use action.
    /// </summary>
    public interface IItemUseAction
    {
        bool UseInput { get; }
        IItemAction TargetAction { get; }
    }

    /// <summary>
    /// The item use action
    /// </summary>
    [Serializable]
    public class ItemUseAction : IItemUseAction
    {
        [Tooltip("The keycode to use the action.")]
        [SerializeField] protected KeyCode m_KeyCode;
        [Tooltip("The button to sue the action.")]
        [SerializeField] protected string m_Button;
        [Tooltip("The action component.")]
        [SerializeField] protected ItemActionComponent m_ItemActionComponent;

        public bool UseInput => Input.GetKeyDown(m_KeyCode) ||
                                string.IsNullOrWhiteSpace(m_Button) ? false : Input.GetButtonDown(m_Button) ;

        public IItemAction TargetAction => m_ItemActionComponent;

    }
}