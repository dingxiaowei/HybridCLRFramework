/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The DamageIndicatorMonitor will show a directional arrow of the direction that the character was damaged from.
    /// </summary>
    public class DamageIndicatorMonitor : CharacterMonitor
    {
        /// <summary>
        /// Indicates the direction that the character took damage.
        /// </summary>
        private struct DamageIndicator
        {
            [Tooltip("The GameObject that did the damage.")]
            private Transform m_Attacker;
            [Tooltip("The position that the character was hit.")]
            private Vector3 m_Position;
            [Tooltip("The angle of the indicator.")]
            private float m_Angle;
            [Tooltip("The time that the indicator was shown.")]
            private float m_DisplayTime;
            [Tooltip("A reference to the indicator's GameObject.")]
            private GameObject m_GameObject;
            [Tooltip("A reference to the indicator's rect transform.")]
            private RectTransform m_RectTransform;
            [Tooltip("A reference to the indicator's image.")]
            private Image m_Image;

            public Transform Attacker { get { return m_Attacker; } }
            public Vector3 Position { get { return m_Position; } }
            public float Angle { get { return m_Angle; } set { m_Angle = value; } }
            public float DisplayTime { get { return m_DisplayTime; } set { m_DisplayTime = value; } }
            public GameObject GameObject { get { return m_GameObject; } }
            public RectTransform RectTransform { get { return m_RectTransform; } }
            public Image Image { get { return m_Image; } }

            /// <summary>
            /// Initializes the pooled HitIndicator values.
            /// </summary>
            /// <param name="attacker">The GameObject that did the damage.</param>
            /// <param name="angle">The angle of the indicator.</param>
            /// <param name="image">A reference to the GameObject of the indicator.</param>
            public void Initialize(Transform attacker, Vector3 position, float angle, GameObject gameObject)
            {
                m_Attacker = attacker;
                m_Position = position;
                m_GameObject = gameObject;
                m_RectTransform = gameObject.GetComponent<RectTransform>();
                m_Image = gameObject.GetComponent<Image>(); 
                m_DisplayTime = Time.time;
                m_GameObject.SetActive(true);
            }
        }

        [Tooltip("Should the indicator be shown even if there isn't a force associated with the damage event?")]
        [SerializeField] protected bool m_AlwaysShowIndicator;
        [Tooltip("Should the indicator follow the position changes of the attacker?")]
        [SerializeField] protected bool m_FollowAttacker = true;
        [Tooltip("Prevents a new hit indicator from appearing if the angle is less than this threshold compared to an already displayed indicator.")]
        [SerializeField] protected float m_IndicatorAngleThreshold = 20;
        [Tooltip("The offset of the indicator from the center of the screen.")]
        [SerializeField] protected float m_IndicatorOffset = 50;
        [Tooltip("The amount of time that the indicator should be fully visible for.")]
        [SerializeField] protected float m_IndicatorVisiblityTime = 2;
        [Tooltip("The amount of time it takes for the indicator to fade.")]
        [SerializeField] protected float m_IndicatorFadeTime = 1;

        public bool AlwaysShowIndicator { get { return m_AlwaysShowIndicator; } set { m_AlwaysShowIndicator = value; } }
        public bool FollowAttacker { get { return m_FollowAttacker; } set { m_FollowAttacker = value; } }
        public float IndicatorAngleTreshold { get { return m_IndicatorAngleThreshold; } set { m_IndicatorAngleThreshold = value; } }
        public float IndicatorOffset { get { return m_IndicatorOffset; } set { m_IndicatorOffset = value; } }
        public float IndicatorVisiblityTime { get { return m_IndicatorVisiblityTime; } set { m_IndicatorVisiblityTime = value; } }
        public float IndicatorFadeTime { get { return m_IndicatorFadeTime; } set { m_IndicatorFadeTime = value; } }

        private GameObject m_GameObject;
        private Transform m_CameraTransform;
        private Transform m_CharacterTransform;
        private UltimateCharacterLocomotion m_CharacterLocomotion;

        private int m_ActiveDamageIndicatorCount;
        private DamageIndicator[] m_ActiveDamageIndicators;
        private GameObject[] m_StoredIndicators;
        private int m_DamageIndicatorIndex;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;

            var images = GetComponentsInChildren<Image>(true);
            m_StoredIndicators = new GameObject[images.Length];
            m_ActiveDamageIndicators = new DamageIndicator[m_StoredIndicators.Length];
            for (int i = 0; i < m_StoredIndicators.Length; ++i) {
                m_StoredIndicators[i] = images[i].gameObject;
                m_StoredIndicators[i].SetActive(false);
            }

            m_GameObject.SetActive(false);
        }

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<float, Vector3, Vector3, GameObject, Collider>(m_Character, "OnHealthDamage", OnDamage);
                EventHandler.UnregisterEvent(m_Character, "OnRespawn", OnRespawn);
                m_CameraTransform = null;
            }

            base.OnAttachCharacter(character);

            if (m_Character == null) {
                return;
            }

            // A camera must exist.
            var camera = UnityEngineUtility.FindCamera(m_Character);
            if (camera != null) {
                m_CameraTransform = camera.transform;
            }
            if (m_CameraTransform == null) {
                Debug.LogError("Error: The Damage Indicator Monitor must have a camera attached to the character.");
                return;
            }

            m_CharacterTransform = m_Character.transform;
            m_CharacterLocomotion = m_Character.GetCachedComponent<UltimateCharacterLocomotion>();
            EventHandler.RegisterEvent<float, Vector3, Vector3, GameObject, Collider>(m_Character, "OnHealthDamage", OnDamage);
            EventHandler.RegisterEvent(m_Character, "OnRespawn", OnRespawn);
        }

        /// <summary>
        /// The object has taken damage.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="force">The amount of force applied to the object while taking the damage.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        /// <param name="hitCollider">The Collider that was hit.</param>
        private void OnDamage(float amount, Vector3 position, Vector3 force, GameObject attacker, Collider hitCollider)
        {
            // Don't show a hit indicator if the force is 0 or there is no attacker. This prevents damage such as fall damage from showing the damage indicator.
            if ((!m_AlwaysShowIndicator && force.sqrMagnitude == 0) || attacker == null) {
                return;
            }

            var direction = Vector3.ProjectOnPlane(m_CharacterTransform.position - ((m_FollowAttacker && m_Character != attacker) ? attacker.transform.position : position),
                                                    m_CharacterLocomotion.Up);
            // The hit indicator is shown on a 2D canvas so the y direction should be ignored.
            direction.y = 0;
            direction.Normalize();

            // Determine the angle of the damage position to determine if a new damage indicator should be shown.
            var angle = Vector3.Angle(direction, m_CameraTransform.forward) * Mathf.Sign(Vector3.Dot(direction, m_CameraTransform.right));

            // Do not show a new damage indicator if the angle is less than a threshold compared to the already displayed indicators.
            DamageIndicator damageIndicator;
            for (int i = 0; i < m_ActiveDamageIndicatorCount; ++i) {
                damageIndicator = m_ActiveDamageIndicators[i];
                if (Mathf.Abs(angle - damageIndicator.Angle) < m_IndicatorAngleThreshold) {
                    damageIndicator.DisplayTime = Time.time;
                    m_ActiveDamageIndicators[i] = damageIndicator;
                    return;
                }
            }

            // Add the indicator to the active hit indicators list and enable the component.
            damageIndicator = GenericObjectPool.Get<DamageIndicator>();
            damageIndicator.Initialize(attacker.transform, position, angle, m_StoredIndicators[m_DamageIndicatorIndex]);
            m_ActiveDamageIndicators[m_ActiveDamageIndicatorCount] = damageIndicator;
            m_ActiveDamageIndicatorCount++;
            m_DamageIndicatorIndex = (m_DamageIndicatorIndex + 1) % m_StoredIndicators.Length;

            // Allow the indicators to move/fade.
            m_GameObject.SetActive(true);
        }
        /// <summary>
        /// One or more hit indicators are shown. 
        /// </summary>
        private void Update()
        {
            for (int i = m_ActiveDamageIndicatorCount - 1; i > -1; --i) {
                // The alpha value is determined by the amount of time the damage indicator has been visible. The indicator should be visible for a time of m_IndicatorVisiblityTime
                // with no fading. After m_IndicatorVisiblityTime the indicator should fade for visibilityTime.
                var alpha = (m_IndicatorFadeTime - (Time.time - (m_ActiveDamageIndicators[i].DisplayTime + m_IndicatorVisiblityTime))) / m_IndicatorFadeTime;
                if (alpha <= 0) {
                    m_ActiveDamageIndicators[i].GameObject.SetActive(false);
                    GenericObjectPool.Return(m_ActiveDamageIndicators[i]);
                    m_ActiveDamageIndicatorCount--;
                    // Sort the array so the complete indicators are at the end.
                    for (int j = i; j < m_ActiveDamageIndicatorCount; ++j) {
                        m_ActiveDamageIndicators[j] = m_ActiveDamageIndicators[j + 1];
                    }
                    continue;
                }
                var color = m_ActiveDamageIndicators[i].Image.color;
                color.a = alpha;
                m_ActiveDamageIndicators[i].Image.color = color;

                var direction = Vector3.ProjectOnPlane(m_CharacterTransform.position - ((m_FollowAttacker && m_CharacterTransform != m_ActiveDamageIndicators[i].Attacker) ?
                                                        m_ActiveDamageIndicators[i].Attacker.position : m_ActiveDamageIndicators[i].Position),
                                                        m_CharacterLocomotion.Up);
                // The hit indicator is shown on a 2D canvas so the y direction should be ignored.
                direction.y = 0;
                direction.Normalize();

                var angle = Vector3.Angle(direction, m_CameraTransform.forward) * Mathf.Sign(Vector3.Dot(direction, m_CameraTransform.right));
                m_ActiveDamageIndicators[i].Angle = angle;

                // Face the image in the direction of the angle.
                var rotation = m_ActiveDamageIndicators[i].RectTransform.localEulerAngles;
                rotation.z = -m_ActiveDamageIndicators[i].Angle;
                m_ActiveDamageIndicators[i].RectTransform.localEulerAngles = rotation;

                // Position the indicator relative to the direction.
                var position = m_ActiveDamageIndicators[i].RectTransform.localPosition;
                position.x = -Mathf.Sin(m_ActiveDamageIndicators[i].Angle * Mathf.Deg2Rad) * m_IndicatorOffset;
                position.y = -Mathf.Cos(m_ActiveDamageIndicators[i].Angle * Mathf.Deg2Rad) * m_IndicatorOffset;
                m_ActiveDamageIndicators[i].RectTransform.localPosition = position;
            }

            // The component can be disabled when the damage indicators have disappeared.
            if (m_ActiveDamageIndicatorCount == 0) {
                m_GameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            // No indicators should be shown when the character respawns.
            for (int i = m_ActiveDamageIndicatorCount - 1; i > -1; --i) {
                m_ActiveDamageIndicators[i].GameObject.SetActive(false);
                GenericObjectPool.Return(m_ActiveDamageIndicators[i]);
            }
            m_ActiveDamageIndicatorCount = 0;
            m_GameObject.SetActive(false);
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected override bool CanShowUI()
        {
            return base.CanShowUI() && m_ActiveDamageIndicatorCount > 0;
        }
    }
}