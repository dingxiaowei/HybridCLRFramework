/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.ImpactActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using UnityEngine;

    /// <summary>
    /// Spawns a SurfaceEffect upon impact. The SurfaceImpact object is specified on the MagicItem.
    /// </summary>
    public class SpawnSurfaceEffect : ImpactAction
    {
        private UltimateCharacterLocomotion m_CharacterLocomotion;

        /// <summary>
        /// Initializes the ImpactAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the ImpactAction belongs to.</param>
        /// <param name="index">The index of the ImpactAction.</param>
        public override void Initialize(GameObject character, MagicItem magicItem, int index)
        {
            base.Initialize(character, magicItem, index);

            m_CharacterLocomotion = character.GetCachedComponent<UltimateCharacterLocomotion>();
        }

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="source">The object that caused the cast.</param>
        /// <param name="target">The object that was hit by the cast.</param>
        /// <param name="hit">The raycast that caused the impact.</param>
        protected override void ImpactInternal(uint castID, GameObject source, GameObject target, RaycastHit hit)
        {
            SurfaceManager.SpawnEffect(hit, m_MagicItem.SurfaceImpact, m_CharacterLocomotion.Up, m_CharacterLocomotion.TimeScale, m_CharacterLocomotion.gameObject);
        }
    }
}