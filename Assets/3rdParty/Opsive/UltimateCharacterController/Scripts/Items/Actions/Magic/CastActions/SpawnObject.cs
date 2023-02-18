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
    using Opsive.UltimateCharacterController.Networking.Objects;
#endif
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Spawns an object when the cast is performed.
    /// </summary>
    [System.Serializable]
    public class SpawnObject : CastAction, IMagicObjectAction
    {
        [Tooltip("The object that should be spawned.")]
        [SerializeField] protected GameObject m_Object;
        [Tooltip("The positional offset that the object should be spawned.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("The rotational offset that the object should be spawned.")]
        [SerializeField] protected Vector3 m_RotationOffset;
        [Tooltip("Should the object be parented to the origin?")]
        [SerializeField] protected bool m_ParentToOrigin;

        public GameObject Object { get { return m_Object; } set { m_Object = value; } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public Vector3 RotationOffset { get { return m_RotationOffset; } set { m_RotationOffset = value; } }
        public bool ParentToOrigin { get { return m_ParentToOrigin; } set { m_ParentToOrigin = value; } }

        private Transform m_Transform;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private GameObject m_SpawnedObject;

        public GameObject SpawnedGameObject { set { m_SpawnedObject = value; } }

        /// <summary>
        /// Initializes the CastAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the CastAction belongs to.</param>
        /// <param name="index">The index of the CastAction.</param>
        public override void Initialize(GameObject character, MagicItem magicItem, int index)
        {
            base.Initialize(character, magicItem, index);

            m_Transform = character.transform;
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
            if (m_Object == null) {
                Debug.LogError("Error: An Object must be specified.", m_MagicItem);
                return;
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // The local player will spawn the object if the object is a networked magic object.
            if (m_MagicItem.NetworkInfo != null && !m_MagicItem.NetworkInfo.IsLocalPlayer()) {
                if (m_Object.GetComponent<INetworkMagicObject>() != null) {
                    return;
                }
            }
#endif

            var position = MathUtility.TransformPoint(origin.position, m_Transform.rotation, m_PositionOffset);
            if (targetPosition != position) {
                direction = (targetPosition - position).normalized;
            }
            m_SpawnedObject = ObjectPool.Instantiate(m_Object, position, 
                            Quaternion.LookRotation(direction, m_CharacterLocomotion.Up) * Quaternion.Euler(m_RotationOffset), m_ParentToOrigin ? origin : null);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_MagicItem.NetworkInfo != null && m_MagicItem.NetworkInfo.IsLocalPlayer()) {
                var networkMagicObject = m_SpawnedObject.GetComponent<INetworkMagicObject>();
                if (networkMagicObject != null) {
                    networkMagicObject.Instantiate(m_GameObject, m_MagicItem, m_Index, m_CastID);
                }

                NetworkObjectPool.NetworkSpawn(m_Object, m_SpawnedObject, false);
            }
#endif
        }

        /// <summary>
        /// Stops the cast.
        /// </summary>
        public override void Stop()
        {
            if (m_SpawnedObject != null) {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                if (NetworkObjectPool.IsNetworkActive()) {
                    // The object may have already been destroyed over the network.
                    if (!m_GameObject.activeSelf) {
                        return;
                    }
                    NetworkObjectPool.Destroy(m_SpawnedObject);
                    return;
                }
#endif
                ObjectPool.Destroy(m_SpawnedObject);
                m_SpawnedObject = null;
            }

            base.Stop();
        }

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public override void OnChangePerspectives(Transform origin)
        {
            if (m_SpawnedObject == null || m_SpawnedObject.transform.parent == origin) {
                return;
            }

            var spawnedTransform = m_SpawnedObject.transform;
            var localRotation = spawnedTransform.localRotation;
            var localScale = spawnedTransform.localScale;
            spawnedTransform.parent = origin;
            spawnedTransform.position = MathUtility.TransformPoint(origin.position, m_Transform.rotation, m_PositionOffset);
            spawnedTransform.localRotation = localRotation;
            spawnedTransform.localScale = localScale;
        }
    }
}