/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Damage
{
    using UnityEngine;

    /// <summary>
    /// The damager interface.
    /// </summary>
    public interface IDamager
    {
        GameObject gameObject { get; }
        int DamageTypeIndex { get; }
    }
}