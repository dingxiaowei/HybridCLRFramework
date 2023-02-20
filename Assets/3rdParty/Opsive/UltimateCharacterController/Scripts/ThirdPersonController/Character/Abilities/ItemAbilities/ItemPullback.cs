/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Character.Abilities.Items
{
    /// <summary>
    /// Pulls back the item if the character gets too close to a wall. This will prevent the item from clipping with the wall.
    /// </summary>
    [DefaultStopType(AbilityStopType.Automatic)]
    public class ItemPullback : ItemAbility
    {
        [Tooltip("The collider used to detect when the character is near an object and should pull back the items.")]
        [SerializeField] protected Collider m_Collider;
        [Tooltip("The layers that the collider can collide with.")]
        [SerializeField] protected LayerMask m_CollisionLayers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.TransparentFX | 1 << LayerManager.SubCharacter |
                                                                1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect | 1 << LayerManager.Water);
        [Tooltip("The maximum number of collisions that should be detected by the collider.")]
        [SerializeField] protected int m_MaxCollisionCount = 5;

        public Collider Collider { get { return m_Collider; } set { m_Collider = value; } }
        public LayerMask CollisionLayers { get { return m_CollisionLayers; } set { m_CollisionLayers = value; } }

        private Transform m_ColliderTransform;
        private Collider[] m_HitColliders;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            if (m_Collider == null || (!(m_Collider is CapsuleCollider) && !(m_Collider is SphereCollider))) {
                Debug.LogError("Error: Only Capsule and Sphere Colliders are supported by the Item Pullback ability.");
                m_Collider = null;
                Enabled = false;
                return;
            }

            m_HitColliders = new Collider[m_MaxCollisionCount];
            m_ColliderTransform = m_Collider.transform;
            m_Collider.gameObject.SetActive(false);

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCameraWillChangePerspectives", OnChangePerspectives);
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            return HasCollision();
        }

        /// <summary>
        /// Is there a collision between the item pullback collider and another object?
        /// </summary>
        /// <returns>True if there is a collision with the item pullback collider.</returns>
        private bool HasCollision()
        {
            int hitCount;
            if (m_Collider is CapsuleCollider) {
                var capsuleCollider = m_Collider as CapsuleCollider;
                if (capsuleCollider.radius == 0) {
                    return false;
                }
                Vector3 startEndCap, endEndCap;
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, m_ColliderTransform.position, m_ColliderTransform.rotation, out startEndCap, out endEndCap);
                hitCount = Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, capsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(capsuleCollider), m_HitColliders, m_CollisionLayers, QueryTriggerInteraction.Ignore);
            } else { // SphereCollider.
                var sphereCollider = m_Collider as SphereCollider;
                if (sphereCollider.radius == 0) {
                    return false;
                }
                hitCount = Physics.OverlapSphereNonAlloc(m_ColliderTransform.position, sphereCollider.radius * MathUtility.ColliderRadiusMultiplier(sphereCollider), m_HitColliders, m_CollisionLayers, QueryTriggerInteraction.Ignore);
            }

            for (int i = 0; i < hitCount; ++i) {
                // Objects which are children of the character aren't considered a collision.
                if (m_HitColliders[i].transform.IsChildOf(m_Transform)) {
                    continue;
                }

                // Projectiles shouldn't prevent the pullback ability.
                if (m_HitColliders[i].gameObject.GetCachedComponent<Objects.Projectile>() != null) {
                    continue;
                }

                // It only takes one object for the ability to be in a collision state.
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            // The ability should block the use ability if a shootable weapon is trying to be started. This will prevent the weapon from trying to shoot through a wall.
            if (startingAbility is Use) {
                var useAbility = startingAbility as Use;
                return useAbility.UsesItemActionType(typeof(UltimateCharacterController.Items.Actions.ShootableWeapon));
            }
            if (startingAbility is Reload && startingAbility.InputIndex != -1) {
                return true;
            }
#endif

            return false;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            if (activeAbility is Reload) {
                return true;
            }
#endif

            return false;
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            return !HasCollision();
        }

        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            // Item Pullback does not work in first person mode.
            Enabled = !firstPersonPerspective;
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCameraWillChangePerspectives", OnChangePerspectives);
        }
    }
}