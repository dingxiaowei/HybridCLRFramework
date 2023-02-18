/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking.Game;
#endif
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using UnityEngine;

    /// <summary>
    /// Spawns a projectile when the cast is performed.
    /// </summary>
    [System.Serializable]
    public class SpawnProjectile : CastAction
    {
        [Tooltip("The projectile that should be spawned.")]
        [SerializeField] protected GameObject m_ProjectilePrefab;
        [Tooltip("The positional offset that the projectile should be spawned.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("The rotational offset that the projectile should be spawned.")]
        [SerializeField] protected Vector3 m_RotationOffset;
        [Tooltip("The speed that the projectile should be initialized to.")]
        [SerializeField] protected float m_Speed = 1;
        [Tooltip("Should the projecitle be parented to the origin?")]
        [SerializeField] protected bool m_ParentToOrigin;

        public GameObject ProjectilePrefab { get { return m_ProjectilePrefab; } set { m_ProjectilePrefab = value; } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public Vector3 RotationOffset { get { return m_RotationOffset; } set { m_RotationOffset = value; } }
        public float Speed { get { return m_Speed; } set { m_Speed = value; } }
        public bool ParentToOrigin { get { return m_ParentToOrigin; } set { m_ParentToOrigin = value; } }

        private UltimateCharacterLocomotion m_CharacterLocomotion;

        /// <summary>
        /// Initializes the CastAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the CastAction belongs to.</param>
        /// <param name="index">The index of the CastAction.</param>
        public override void Initialize(GameObject character, MagicItem magicItem, int index)
        {
            base.Initialize(character, magicItem, index);

            m_CharacterLocomotion = character.GetCachedComponent<UltimateCharacterLocomotion>();
        }

        /// <summary>
        /// Performs the cast.
        /// </summary>
        /// <param name="origin">The location that the cast should spawn from.</param>
        /// <param name="direction">The direction of the cast.</param>
        /// <param name="targetPosition">The target position of the cast.</param>
        public override void Cast(Transform origin, Vector3 direction, Vector3 targetPosition)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // The server will spawn the projectile.
            if (m_MagicItem.NetworkInfo != null) {
                if (m_MagicItem.NetworkInfo.IsLocalPlayer()) {
                    m_MagicItem.NetworkCharacter.MagicCast(m_MagicItem, m_Index, m_CastID, direction, targetPosition);
                }
                if (!m_MagicItem.NetworkInfo.IsServer()) {
                    return;
                }
            }
#endif

            if (m_ProjectilePrefab == null) {
                Debug.LogError("Error: A Projectile Prefab must be specified", m_MagicItem);
                return;
            }

            var position = origin.TransformPoint(m_PositionOffset);
            var obj = ObjectPool.Instantiate(m_ProjectilePrefab, position,
                            Quaternion.LookRotation(direction, m_CharacterLocomotion.Up) * Quaternion.Euler(m_RotationOffset), m_ParentToOrigin ? origin : null);
            var projectile = obj.GetComponent<MagicProjectile>();
            if (projectile != null) {
                projectile.Initialize(direction * m_Speed, Vector3.zero, m_GameObject, m_MagicItem, m_CastID);
            } else {
                Debug.LogWarning($"Warning: The projectile {m_ProjectilePrefab.name} does not have the MagicProjectile component attached.");
            }
            var magicParticle = obj.GetComponent<MagicParticle>();
            if (magicParticle != null) {
                magicParticle.Initialize(m_MagicItem, m_CastID);
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_MagicItem.NetworkInfo != null) {
                NetworkObjectPool.NetworkSpawn(m_ProjectilePrefab, obj, true);
            }
#endif
        }
    }
}