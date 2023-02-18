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
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Uses the physics system to determine the impacted object.
    /// </summary>
    [System.Serializable]
    public class PhysicsCast : CastAction
    {
        /// <summary>
        /// The type of cast to perform.
        /// </summary>
        public enum CastMode
        {
            Raycast,        // Performs a raycast.
            SphereCast,     // Performs a sphere cast.
            OverlapSphere,  // Performs an overlap sphere check.
        }

        [Tooltip("Specifies the type of cast to perform.")]
        [SerializeField] protected CastMode m_Mode;
        [Tooltip("Should the look source be used when determining the cast position? If false the origin will be used.")]
        [SerializeField] protected bool m_UseLookSourcePosition;
        [Tooltip("The offset to add to the physics cast position.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("The distance of the cast.")]
        [SerializeField] protected float m_Distance = 5;
        [Tooltip("The radius of the cast.")]
        [SerializeField] protected float m_Radius = 5;
        [Tooltip("The layers that the cast can detect.")]
        [SerializeField] protected LayerMask m_Layers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.UI | 1 << LayerManager.SubCharacter | 1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect);
        [Tooltip("Specifies if the cast intersect with triggers.")]
        [SerializeField] protected QueryTriggerInteraction m_TriggerInteraction = QueryTriggerInteraction.Ignore;
        [Tooltip("The maximum number of collisions that the cast can detect.")]
        [SerializeField] protected int m_MaxCollisionCount = 50;

        public CastMode Mode { get { return m_Mode; } set { m_Mode = value; } }
        public bool UseLookSourcePosition { get { return m_UseLookSourcePosition; } set { m_UseLookSourcePosition = value; } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public float Distance { get { return m_Distance; } set { m_Distance = value; } }
        public float Radius { get { return m_Radius; } set { m_Radius = value; } }
        public LayerMask Layers { get { return m_Layers; } set { m_Layers = value; } }
        public QueryTriggerInteraction TriggerInteraction { get { return m_TriggerInteraction; } set { m_TriggerInteraction = value; } }

        private Transform m_CharacterTransform;
        private UltimateCharacterLocomotion m_CharacterLocomotion;

        private RaycastHit[] m_HitRaycasts;
        private Collider[] m_HitColliders;

        /// <summary>
        /// Initializes the CastAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the CastAction belongs to.</param>
        /// <param name="index">The index of the CastAction.</param>
        public override void Initialize(GameObject character, MagicItem magicItem, int index)
        {
            base.Initialize(character, magicItem, index);

            m_CharacterTransform = character.transform;
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
            var position = m_UseLookSourcePosition ? m_CharacterLocomotion.LookSource.LookPosition() : origin.position;
            position = MathUtility.TransformPoint(position, m_CharacterTransform.rotation, m_PositionOffset);
            int hitCount;
            // Raycast and Spherecast use RaycastHit[], while OverlapSphere uses Collider[].
            if (m_Mode == CastMode.Raycast || m_Mode == CastMode.SphereCast) {
                if (m_HitRaycasts == null) {
                    m_HitRaycasts = new RaycastHit[m_MaxCollisionCount];
                }
                if (m_Mode == CastMode.Raycast) {
                    hitCount = Physics.RaycastNonAlloc(position, direction, m_HitRaycasts, m_Distance, m_Layers, m_TriggerInteraction);
                } else {
                    hitCount = Physics.SphereCastNonAlloc(position - direction * m_Radius, m_Radius, direction, m_HitRaycasts, m_Distance, m_Layers, m_TriggerInteraction);
                }
                for (int i = 0; i < hitCount; ++i) {
                    m_MagicItem.PerformImpact(m_CastID, m_GameObject, m_HitRaycasts[i].transform.gameObject, m_HitRaycasts[i]);
                }
                return;
            }

            // OverlapSphere.
            if (m_HitColliders == null) {
                m_HitColliders = new Collider[m_MaxCollisionCount];
            }
            hitCount = Physics.OverlapSphereNonAlloc(position, m_Radius, m_HitColliders, m_Layers, m_TriggerInteraction);
            for (int i = 0; i < hitCount; ++i) {
                // The cast cannot hit the current character.
                if (m_HitColliders[i].transform.IsChildOf(m_CharacterTransform)) {
                    continue;
                }

                // The object must be within view.
                var visibleTransform = false;
                Vector3 colliderPosition;
                PivotOffset pivotOffset;
                if ((pivotOffset = m_HitColliders[i].gameObject.GetCachedComponent<PivotOffset>()) != null) {
                    colliderPosition = m_HitColliders[i].transform.TransformPoint(pivotOffset.Offset);
                } else {
                    colliderPosition = m_HitColliders[i].transform.position;
                }
                if (Physics.Linecast(position, colliderPosition, out var raycastHit, m_Layers, m_TriggerInteraction)) {
                    if (raycastHit.transform.IsChildOf(m_HitColliders[i].transform)) {
                        visibleTransform = true;
                    }
                }
                if (!visibleTransform) {
                    continue;
                }

                // The object is valid - perform the impact.
                m_MagicItem.PerformImpact(m_CastID, origin.gameObject, m_HitColliders[i].gameObject, raycastHit);
            }
        }

        /// <summary>
        /// Stops the cast.
        /// </summary>
        public override void Stop()
        {
            if (m_MagicItem != null && m_MagicItem.ImpactActions != null) {
                for (int i = 0; i < m_MagicItem.ImpactActions.Length; ++i) {
                    m_MagicItem.ImpactActions[i].Reset(m_CastID);
                }
            }

            base.Stop();
        }
    }
}