/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Input;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.FirstPersonController.Character.Abilities
{
    /// <summary>
    /// The Lean ability allows the character to lean the camera to the left or the right of the character. This allows the character to peak
    /// without exposing their body. An optional collider can be used as a hitpoint and to detect any collisions.
    /// </summary>
    [DefaultStartType(AbilityStartType.Axis)]
    [DefaultStopType(AbilityStopType.Axis)]
    [DefaultInputName("Lean")]
    public class Lean : Ability
    {
        private const string c_LeanEventName = "OnCharacterLean";

        [Tooltip("The distance that the camera should lean.")]
        [SerializeField] protected float m_Distance = 0.7f;
        [Tooltip("The amount of tilt to apply with the lean (in degress).")]
        [SerializeField] protected float m_Tilt = 7;
        [Tooltip("A tilt multiplier applied to the items.")]
        [SerializeField] protected float m_ItemTiltMultiplier = 2;
        [Tooltip("An optional collider that can be used for collision detection and hit points.")]
        [SerializeField] protected Collider m_Collider;
        [Tooltip("Optionally modify the distance that the collider leans.")]
        [Range(0, 1)] [SerializeField] protected float m_ColliderOffsetMultiplier = 0.75f;
        [Tooltip("The maximum number of collisions that can be detected by the collider.")]
        [SerializeField] protected int m_MaxCollisionCount = 5;

        public float Distance { get { return m_Distance; } set { m_Distance = value; } }
        public float Tilt { get { return m_Tilt; } set { m_Tilt = value; } }
        public float ItemTiltMultiplier { get { return m_ItemTiltMultiplier; } set { m_ItemTiltMultiplier = value; } }
        [NonSerialized] public Collider Collider { get { return m_Collider; } set { m_Collider = value; } }
        public float ColliderOffsetMultiplier { get { return m_ColliderOffsetMultiplier; } set { m_ColliderOffsetMultiplier = value; } }

        private UltimateCharacterLocomotionHandler m_Handler;
        private ActiveInputEvent m_LeanInput;

        private GameObject m_ColliderGameObject;
        private Transform m_ColliderTransform;
        private Collider[] m_OverlapColliders;
        private float m_HitDistance;

        private float m_AxisValue;

        public override bool IsConcurrent { get { return true; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_Handler = m_GameObject.GetCachedComponent<UltimateCharacterLocomotionHandler>();
            if (m_Collider != null) {
                if (!(m_Collider is CapsuleCollider) && !(m_Collider is SphereCollider)) {
                    Debug.LogError("Error: Only Capsule and Sphere Colliders are supported by the Lean ability.");
                    m_Collider = null;
                    return;
                }

                m_OverlapColliders = new Collider[m_MaxCollisionCount];
                m_ColliderGameObject = m_Collider.gameObject;
                m_ColliderTransform = m_Collider.transform;
                m_ColliderGameObject.SetActive(false);
            }
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterChangePerspectives", OnChangePerspectives);
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            // If a handler exists then the ability is interested in updates when the axis value changes. This for example allows the lean to switch 
            // between the left and right lean without having to stop and start again.
            if (m_Handler != null) {
                m_LeanInput = ObjectPool.Get<ActiveInputEvent>();
                m_LeanInput.Initialize(ActiveInputEvent.Type.Axis, InputNames[InputIndex], "OnLeanInputUpdate");
                m_Handler.RegisterInputEvent(m_LeanInput);
            }
            EventHandler.RegisterEvent<float>(m_GameObject, "OnLeanInputUpdate", OnInputUpdate);

            base.AbilityStarted();

            // The collider should be activated when the ability starts. The collider detects when the character would be clipping with a wall
            // and also allows the character to be shot at while leaning.
            if (m_ColliderGameObject != null) {
                m_ColliderGameObject.SetActive(true);
            }

            // Start leaning.
            m_AxisValue = InputAxisValue;
            UpdateLean(true);
        }

        /// <summary>
        /// As the character is moving the lean should update to ensure the collider doesn't clip with any objects.
        /// </summary>
        public override void Update()
        {
            UpdateLean(false);
        }

        /// <summary>
        /// Updates the lean value. Will first ensure the collider doesn't clip with any other objects.
        /// </summary>
        /// <param name="forceUpdate">Should the lean values be forced to update?</param>
        private void UpdateLean(bool forceUpdate)
        {
            var update = forceUpdate;
            // If a collider exists then the lean should not clip any walls. Note that the collider doesn't actually move - it stays at the maximum lean
            // distance so ComputePenetration can detect how much to retract the lean in order to prevent any clipping.
            if (m_Collider != null) {
                var collisionLayerEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
                m_CharacterLocomotion.EnableColliderCollisionLayer(false);
                int hitCount;
                if (m_Collider is CapsuleCollider) {
                    Vector3 startEndCap, endEndCap;
                    var capsuleCollider = m_Collider as CapsuleCollider;
                    MathUtility.CapsuleColliderEndCaps(capsuleCollider, m_ColliderTransform.TransformPoint(capsuleCollider.center), m_ColliderTransform.rotation, out startEndCap, out endEndCap);
                    hitCount = Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, capsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(capsuleCollider), 
                                                m_OverlapColliders, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                } else { // SphereCollider.
                    var sphereCollider = m_Collider as SphereCollider;
                    hitCount = Physics.OverlapSphereNonAlloc(m_ColliderTransform.TransformPoint(sphereCollider.center), sphereCollider.radius * 
                                                MathUtility.ColliderRadiusMultiplier(sphereCollider),
                                                m_OverlapColliders, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                }

                if (hitCount > 0) {
                    Vector3 direction;
                    float distance;
                    var offset = Vector3.zero;
                    // Determine the offset required to resolve the collision. Note that for multiple hit colliders this will not always be resolved on the first iteration
                    // but it doesn't need to be perfect for a lean.
                    for (int i = 0; i < hitCount; ++i) {
                        if (Physics.ComputePenetration(m_Collider, m_ColliderTransform.position, m_ColliderTransform.rotation,
                            m_OverlapColliders[i], m_OverlapColliders[i].transform.position, m_OverlapColliders[i].transform.rotation, out direction, out distance)) {
                            offset += direction.normalized * (distance + m_CharacterLocomotion.ColliderSpacing);
                        }
                    }
                    // Determing if there is any horizontal collision. If a collision exists then the lean should be updated to prevent any clipping.
                    var hitDistance = m_Transform.InverseTransformDirection(offset).x;
                    if (m_HitDistance != hitDistance) {
                        m_HitDistance = hitDistance;
                        update = true;
                    }
                } else if (m_HitDistance > 0) {
                    // The collider was previously overlapping an object but it is not anymore. Update lean.
                    m_HitDistance = 0;
                    update = true;
                }
                m_CharacterLocomotion.EnableColliderCollisionLayer(collisionLayerEnabled);
            }

            // Update the lean if the ability is just starting or stopping, there is an axis value change, or there is a collision.
            if (update) {
                float distance, tilt;
                if (m_AxisValue == 0) {
                    distance = tilt = 0;
                } else {
                    distance = m_Distance * -Mathf.Sign(m_AxisValue);
                    tilt = m_Tilt * Mathf.Sign(m_AxisValue);
                }

                // The collider should always be at the maximum value to allow for a stable ComputePenetration value.
                if (m_ColliderTransform != null) {
                    var localPosition = m_ColliderTransform.localPosition;
                    localPosition.x = distance * m_ColliderOffsetMultiplier;
                    m_ColliderTransform.localPosition = localPosition;
                }

                // Prevent any clipping.
                if (Mathf.Abs(m_HitDistance) > 0) {
                    var percent = 1 - Mathf.Abs(m_HitDistance) / m_Distance;
                    distance *= percent;
                    tilt *= percent;
                }
                // Notify those interested of the distance and tilt value.
                EventHandler.ExecuteEvent(m_GameObject, c_LeanEventName, distance, tilt, m_ItemTiltMultiplier);
            }
        }

        /// <summary>
        /// The AbilityInputEvent has updated the axis value.
        /// </summary>
        /// <param name="value">The updated axis value.</param>
        private void OnInputUpdate(float value)
        {
            if (m_AxisValue != value) {
                m_AxisValue = value;
                UpdateLean(true);
            }
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            // Update one last time with an axis value of 0 to return to the starting position.
            m_AxisValue = 0;
            UpdateLean(true);

            // The collider is no longer needed.
            if (m_ColliderGameObject != null) {
                m_ColliderGameObject.SetActive(false);
            }

            if (m_Handler != null) {
                m_Handler.UnregisterInputEvent(m_LeanInput);
                ObjectPool.Return(m_LeanInput);
            }
            EventHandler.UnregisterEvent<float>(m_GameObject, "OnLeanInputUpdate", OnInputUpdate);
        }

        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            // Lean does not work in third person mode.
            Enabled = firstPersonPerspective;
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnLeanInputUpdate", OnChangePerspectives);
        }
    }
}