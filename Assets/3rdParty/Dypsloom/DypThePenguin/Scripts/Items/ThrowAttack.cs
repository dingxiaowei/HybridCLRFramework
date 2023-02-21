/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    using Dypsloom.DypThePenguin.Scripts.Character;
    using Dypsloom.DypThePenguin.Scripts.Damage;
    using Dypsloom.Shared.Utility;
    using UnityEngine;

    /// <summary>
    /// The throw attack
    /// </summary>
    public class ThrowAttack : ItemActionComponent
    {
        [Tooltip("Cooldown between each throw.")]
        [SerializeField] protected float m_Cooldown;
        [Tooltip("The projectile prefab.")]
        [SerializeField] protected GameObject m_ProjectilePrefab;

        /// <summary>
        /// Use the item object.
        /// </summary>
        /// <param name="item">The item object.</param>
        /// <param name="itemUser">The item user.</param>
        public override void Use(IItem item, IItemUser itemUser)
        {
            m_NextUseTime = Time.time + m_Cooldown;
            
            itemUser.Character.CharacterAnimator.ItemAction(CharacterAnimator.SnowBallAnimID,CharacterAnimator.ThrowSnowballAnimID);

            var characterTransform = itemUser.Character.transform;
            var projectileSpawnPoint = itemUser.Character.ProjectilesSpawnPoint;
            
            var projectileSpawnPosition = projectileSpawnPoint?.position ?? characterTransform.position;

            var projectileRotation = projectileSpawnPoint?.rotation ?? characterTransform.rotation;
            
            var projectileObject = PoolManager.Instantiate(m_ProjectilePrefab, 
                projectileSpawnPosition, projectileRotation);

            var projectile = projectileObject.GetComponent<Projectile>();
            if (projectile != null) {
                projectile.SetItemUser(itemUser);
            }

            itemUser.Character.Inventory.Remove(item, 1);
        }
    }
}