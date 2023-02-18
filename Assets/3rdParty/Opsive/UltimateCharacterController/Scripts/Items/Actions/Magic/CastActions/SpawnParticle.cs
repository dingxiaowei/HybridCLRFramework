/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking.Game;
    using Opsive.UltimateCharacterController.Networking.Objects;
#endif
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Spawns a particle when the cast is performed.
    /// </summary>
    [System.Serializable]
    public class SpawnParticle : CastAction, IMagicObjectAction
    {
        [Tooltip("The particle prefab that should be spawned.")]
        [SerializeField] protected GameObject m_ParticlePrefab;
        [Tooltip("The positional offset that the particle should be spawned.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("The rotational offset that the particle should be spawned.")]
        [SerializeField] protected Vector3 m_RotationOffset;
        [Tooltip("Should the particle be parented to the origin?")]
        [SerializeField] protected bool m_ParentToOrigin;
        [Tooltip("Should the directional vector be projected onto the character's normal plane?")]
        [SerializeField] protected bool m_ProjectDirectionOnPlane;
        [Tooltip("Should the particle's parent be cleared when the cast stops?")]
        [SerializeField] protected bool m_ClearParentOnStop;
        [Tooltip("Should the particle's Length Scale be set?")]
        [SerializeField] protected bool m_SetRendererLengthScale;
        [Tooltip("Additional value to add to the particle's Length Scale.")]
        [SerializeField] protected float m_AdditionalLength = 0.1f;
        [Tooltip("The layer that the particle should occupy.")]
        [SerializeField] protected int m_ParticleLayer = LayerManager.IgnoreRaycast;
        [Tooltip("The duration of the alpha fade in.")]
        [SerializeField] protected float m_FadeInDuration;
        [Tooltip("The duration of the alpha fade out.")]
        [SerializeField] protected float m_FadeOutDuration;
        [Tooltip("The name of the material's color property.")]
        [SerializeField] protected string m_MaterialColorName = "_TintColor";
        [Tooltip("The delta step of the alpha fade.")]
        [SerializeField] protected float m_FadeStep = 0.05f;

        public GameObject ParticlePrefab { get { return m_ParticlePrefab; } set { if (m_ParticlePrefab != value) { m_ParticlePrefab = value; m_Renderers = null; } } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public Vector3 RotationOffset { get { return m_RotationOffset; } set { m_RotationOffset = value; } }
        public bool ParentToOrigin { get { return m_ParentToOrigin; } set { m_ParentToOrigin = value; } }
        public bool ProjectDirectionOnPlane { get { return m_ProjectDirectionOnPlane; } set { m_ProjectDirectionOnPlane = value; } }
        public bool ClearParentOnStop { get { return m_ClearParentOnStop; } set { m_ClearParentOnStop = value; } }
        public bool SetRendererLengthScale { get { return m_SetRendererLengthScale; } set { m_SetRendererLengthScale = value; } }
        public float AdditionalLength { get { return m_AdditionalLength; } set { m_AdditionalLength = value; } }
        public int ParticleLayer { get { return m_ParticleLayer; }
            set {
                m_ParticleLayer = value;
                if (m_ParticleTransform == null) {
                    return;
                }
                m_ParticleTransform.SetLayerRecursively(m_ParticleLayer);
            }
        }
        public float FadeInDuration { get { return m_FadeInDuration; } set { m_FadeInDuration = value; } }
        public float FadeOutDuration { get { return m_FadeOutDuration; } set { m_FadeOutDuration = value; } }
        public float FadeStep { get { return m_FadeStep; } set { m_FadeStep = value; } }

        private Transform m_Transform;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private Transform m_ParticleTransform;
        private ParticleSystem m_ParticleSystem;
        private ParticleSystemRenderer[] m_Renderers;
        private ScheduledEventBase m_FadeEvent;

        private int m_MaterialColorID;
        private bool m_Active;

        public GameObject SpawnedGameObject { 
            set {
                if (m_FadeEvent != null) {
                    Scheduler.Cancel(m_FadeEvent);
                    m_FadeEvent = null;
                    SetRendererAlpha(0);
                }
                m_ParticleTransform = value.transform;
                m_ParticleSystem = value.GetCachedComponent<ParticleSystem>();
                m_Renderers = m_ParticleSystem.GetComponentsInChildren<ParticleSystemRenderer>();
                StartMaterialFade(value);
                m_Active = true;
            } 
        }

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
            m_MaterialColorID = Shader.PropertyToID(m_MaterialColorName);
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
            if (m_MagicItem.NetworkInfo != null && m_MagicItem.NetworkInfo.IsLocalPlayer()) {
                m_MagicItem.NetworkCharacter.MagicCast(m_MagicItem, m_Index, m_CastID, direction, targetPosition);
            }
#endif

            var position = MathUtility.TransformPoint(origin.position, m_Transform.rotation, m_PositionOffset);
            if (targetPosition != position) {
                direction = (targetPosition - position).normalized;
            }
            if (m_ProjectDirectionOnPlane) {
                direction = Vector3.ProjectOnPlane(direction, m_CharacterLocomotion.Up);
            }
            // The direction can't be 0.
            if (direction.sqrMagnitude == 0) {
                direction = m_CharacterLocomotion.transform.forward;
            }
            var rotation = Quaternion.LookRotation(direction, m_CharacterLocomotion.Up) * Quaternion.Euler(m_RotationOffset);

            // If the cast is currently active then the particle should be reused.
            if (m_Active) {
                if (m_SetRendererLengthScale) {
                    SetRendererLength(origin.position, targetPosition);
                }
                if (!m_ParentToOrigin) {
                    m_ParticleTransform.position = position;
                }
                m_ParticleTransform.rotation = rotation;
                return;
            }

            if (m_ParticlePrefab == null) {
                Debug.LogError("Error: A Particle Prefab must be specified.", m_MagicItem);
                return;
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // The local player will spawn the particle if the object is a networked magic object.
            if (m_MagicItem.NetworkInfo != null && !m_MagicItem.NetworkInfo.IsLocalPlayer()) {
                if (m_ParticlePrefab.GetComponent<INetworkMagicObject>() != null) {
                    return;
                }
            }
#endif

            if (m_FadeEvent != null) {
                Scheduler.Cancel(m_FadeEvent);
                m_FadeEvent = null;
                SetRendererAlpha(0);
            }
            var obj = ObjectPool.Instantiate(m_ParticlePrefab, position, rotation, m_ParentToOrigin ? origin : null);
            m_ParticleTransform = obj.transform;
            m_ParticleTransform.SetLayerRecursively(m_ParticleLayer);
            m_ParticleSystem = obj.GetCachedComponent<ParticleSystem>();

            if (m_ParticleSystem == null) {
                Debug.LogError($"Error: A Particle System must be specified on the particle {m_ParticlePrefab}.", m_MagicItem);
                return;
            }

            m_ParticleSystem.Clear(true);
            m_Renderers = null;
            if (m_SetRendererLengthScale) {
                SetRendererLength(origin.position, targetPosition);
            }
            StartMaterialFade(obj);

            // The MagicParticle can determine the impacts.
            var magicParticle = obj.GetComponent<MagicParticle>();
            if (magicParticle != null) {
                magicParticle.Initialize(m_MagicItem, m_CastID);
            }
            m_Active = true;

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_MagicItem.NetworkInfo != null && m_MagicItem.NetworkInfo.IsLocalPlayer()) {
                var networkMagicObject = obj.GetComponent<INetworkMagicObject>();
                if (networkMagicObject != null) {
                    networkMagicObject.Instantiate(m_GameObject, m_MagicItem, m_Index, m_CastID);
                }

                NetworkObjectPool.NetworkSpawn(m_ParticlePrefab, obj, false);
            }
#endif
        }

        /// <summary>
        /// Sets the length of the renderer.
        /// </summary>
        /// <param name="position">The position that the particle is spawned from.</param>
        /// <param name="targetPosition">The target position of the cast.</param>
        private void SetRendererLength(Vector3 position, Vector3 targetPosition)
        {
            if (m_Renderers == null) {
                m_Renderers = m_ParticleSystem.GetComponentsInChildren<ParticleSystemRenderer>();
            }
            for (int i = 0; i < m_Renderers.Length; ++i) {
                m_Renderers[i].lengthScale = (position - targetPosition).magnitude + m_AdditionalLength;
            }
        }

        /// <summary>
        /// Starts to fade the particle materials.
        /// </summary>
        /// <param name="particle">The GameObject that the particle belongs to.</param>
        private void StartMaterialFade(GameObject particle)
        {
            // Optionally fade the particle into the world.
            if (m_FadeInDuration > 0) {
                if (m_Renderers == null) {
                    m_Renderers = particle.GetComponentsInChildren<ParticleSystemRenderer>();
                }
                SetRendererAlpha(0);
                var interval = m_FadeInDuration / (1 / m_FadeStep);
                m_FadeEvent = Scheduler.Schedule(interval, FadeMaterials, interval, 1f);
            }
        }

        /// <summary>
        /// Sets the alpha of the renderers.
        /// </summary>
        /// <param name="alpha">The alpha that should be set.</param>
        private void SetRendererAlpha(float alpha)
        {
            for (int i = 0; i < m_Renderers.Length; ++i) {
                if (!m_Renderers[i].material.HasProperty(m_MaterialColorID)) {
                    continue;
                }
                var color = m_Renderers[i].material.GetColor(m_MaterialColorID);
                color.a = alpha;
                m_Renderers[i].material.SetColor(m_MaterialColorID, color);
            }
        }

        /// <summary>
        /// Fades all of the materials which belong to the renderers.
        /// </summary>
        /// <param name="interval">The time interval which updates the fade.</param>
        /// <param name="targetAlpha">The target alpha value.</param>
        private void FadeMaterials(float interval, float targetAlpha)
        {
            var arrived = true;
            for (int i = 0; i < m_Renderers.Length; ++i) {
                if (!m_Renderers[i].material.HasProperty(m_MaterialColorID)) {
                    continue;
                }
                var color = m_Renderers[i].material.GetColor(m_MaterialColorID);
                color.a = Mathf.MoveTowards(color.a, targetAlpha, m_FadeStep);
                m_Renderers[i].material.SetColor(m_MaterialColorID, color);

                // Schedule the method again if the material isn't at the desired fade value.
                if (color.a != targetAlpha) {
                    arrived = false;
                }
            }
            if (!arrived) {
                m_FadeEvent = Scheduler.Schedule(interval, FadeMaterials, interval, targetAlpha);
            }
        }

        /// <summary>
        /// The cast will be stopped. Start any cleanup.
        /// </summary>
        public override void WillStop()
        {
            if (m_ClearParentOnStop) {
                m_ParticleSystem.transform.parent = null;
            }
        }

        /// <summary>
        /// Stops the cast.
        /// </summary>
        public override void Stop()
        {
            m_ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            // Optionally fade the particle out of the world.
            if (m_FadeOutDuration > 0) {
                if (m_FadeEvent != null) {
                    Scheduler.Cancel(m_FadeEvent);
                    m_FadeEvent = null;
                }
                if (m_Renderers == null) {
                    m_Renderers = m_ParticleSystem.GetComponentsInChildren<ParticleSystemRenderer>();
                }
                var interval = m_FadeOutDuration / (1 / m_FadeStep);
                // Reset the alpha if the renderers have no fade in duration.
                if (m_FadeInDuration == 0) {
                    SetRendererAlpha(1);
                }
                m_FadeEvent = Scheduler.Schedule(interval, FadeMaterials, interval, 0f);
            }

            m_Active = false;
            base.Stop();
        }

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public override void OnChangePerspectives(Transform origin)
        {
            if (!m_Active || m_ParticleSystem.transform.parent == origin) {
                return;
            }

            var spawnedTransform = m_ParticleSystem.transform;
            var localRotation = spawnedTransform.localRotation;
            var localScale = spawnedTransform.localScale;
            spawnedTransform.parent = origin;
            spawnedTransform.position = MathUtility.TransformPoint(origin.position, m_Transform.rotation, m_PositionOffset);
            spawnedTransform.localRotation = localRotation;
            spawnedTransform.localScale = localScale;
        }
    }
}