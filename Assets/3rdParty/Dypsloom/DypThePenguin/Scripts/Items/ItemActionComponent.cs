/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    using UnityEngine;

    /// <summary>
    /// An abstract class used to create actions for item objects.
    /// </summary>
    public abstract class ItemActionComponent : MonoBehaviour, IItemAction
    {
        protected float m_NextUseTime;

        /// <summary>
        /// Can the item object be used.
        /// </summary>
        public virtual bool CanUse => Time.time >= m_NextUseTime;

        /// <summary>
        /// Use the item object.
        /// </summary>
        /// <param name="item">The item object.</param>
        /// <param name="itemUser">The item user.</param>
        public abstract void Use(IItem item, IItemUser itemUser);
    }
}