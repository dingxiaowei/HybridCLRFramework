/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking.Game;
#endif
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using Opsive.UltimateCharacterController.Utility;
#if ULTIMATE_CHARACTER_CONTROLLER_VR
    using Opsive.UltimateCharacterController.VR;
#endif
    using UnityEngine;

    /// <summary>
    /// Any item that can be thrown, such as a grenade or baseball. The GameObject that the ThrowableItem attaches to is not the actual object that is thrown - the ThrownObject field
    /// specifies this instead.
    /// </summary>
    public class ThrowableItem : UsableItem
    {
        [Tooltip("The object that is thrown.")]
        [SerializeField] protected GameObject m_ThrownObject;
        [Tooltip("Should the visible object be disabled?")]
        [SerializeField] protected bool m_DisableVisibleObject;
        [Tooltip("Specifies if the item should wait for the OnAnimatorActivateThrowableObject animation event or wait for the specified duration before activating the throwable object.")]
        [SerializeField] protected AnimationEventTrigger m_ActivateThrowableObjectEvent;
        [Tooltip("Should the object be thrown when the stop use method is called?")]
        [SerializeField] protected bool m_ThrowOnStopUse;
        [Tooltip("The starting velocity of the thrown object.")]
        [SerializeField] protected Vector3 m_Velocity = new Vector3(0, 5, 10);
        [Tooltip("The layer that the item should occupy when initially spawned.")]
        [SerializeField] protected int m_StartLayer = LayerManager.IgnoreRaycast;
        [Tooltip("The layer that the thrown object should change to after being thrown.")]
        [SerializeField] protected int m_ThrownLayer = LayerManager.Default;
        [Tooltip("The amount of time after the object has been thrown to change the layer.")]
        [SerializeField] protected float m_LayerChangeDelay = 0.1f;
        [Tooltip("The amount of damage applied to the object hit by the thrown object.")]
        [SerializeField] protected float m_DamageAmount = 10;
        [Tooltip("The layers that the thrown object can collide with.")]
        [SerializeField] protected LayerMask m_ImpactLayers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.Overlay | 1 << LayerManager.UI | 1 << LayerManager.Overlay);
        [Tooltip("The Surface Impact triggered when the object hits another object.")]
        [SerializeField] protected SurfaceImpact m_SurfaceImpact;
        [Tooltip("The amount of force to apply to the object hit.")]
        [SerializeField] protected float m_ImpactForce = 2;
        [Tooltip("The number of frames to add the impact force to.")]
        [SerializeField] protected int m_ImpactForceFrames = 15;
        [Tooltip("The name of the state to activate upon impact.")]
        [SerializeField] protected string m_ImpactStateName;
        [Tooltip("The number of seconds until the impact state is disabled. A value of -1 will require the state to be disabled manually.")]
        [SerializeField] protected float m_ImpactStateDisableTimer = 10;
        [Tooltip("Specifies if the item should wait for the OnAnimatorReequipThrowableItem animation event or wait for the specified duration before requipping.")]
        [SerializeField] protected AnimationEventTrigger m_ReequipEvent = new AnimationEventTrigger(false, 0.5f);
        [Tooltip("The value of the Item Substate Animator parameter when the item is being reequipped.")]
        [SerializeField] protected int m_ReequipItemSubstateParameterValue = 10;
        [Tooltip("Should the item's trajectory be shown when the character aims?")]
        [SerializeField] protected bool m_ShowTrajectoryOnAim;
        [Tooltip("The offset of the trajectory visualization relative to the trajectory transform set on the Throwable Item Properties.")]
        [SerializeField] protected Vector3 m_TrajectoryOffset;

        public GameObject ThrownObject { get { return m_ThrownObject; } set { m_ThrownObject = value; } }
        public bool DisableVisibleObject { get { return m_DisableVisibleObject; } set {
                m_DisableVisibleObject = value;
                if (m_Item != null) {
                    m_Item.SetVisibleObjectActive(m_Item.VisibleObjectActive);
                    EnableObjectMeshRenderers(CanActivateVisibleObject());
                }
            }
        }
        public AnimationEventTrigger ActivateThrowableObjectEvent { get { return m_ActivateThrowableObjectEvent; } set { m_ActivateThrowableObjectEvent = value; } }
        public bool ThrowOnStopUse { get { return m_ThrowOnStopUse; } set { m_ThrowOnStopUse = value; } }
        public int StartLayer { get { return m_StartLayer; }
            set
            {
                m_StartLayer = value;
                if (m_InstantiatedThrownObject != null && !m_Thrown) {
                    m_InstantiatedThrownObject.layer = m_StartLayer;
                }
            }
        }
        public int ThrownLayer { get { return m_ThrownLayer; } set { m_ThrownLayer = value; } }
        public float LayerChangeDelay { get { return m_LayerChangeDelay; } set { m_LayerChangeDelay = value; } }
        public Vector3 Velocity { get { return m_Velocity; } set { m_Velocity = value; } }
        public float DamageAmount { get { return m_DamageAmount; } set { m_DamageAmount = value; } }
        public LayerMask ImpactLayers { get { return m_ImpactLayers; } set { m_ImpactLayers = value; } }
        public SurfaceImpact SurfaceImpact { get { return m_SurfaceImpact; } set { m_SurfaceImpact = value; } }
        public float ImpactForce { get { return m_ImpactForce; } set { m_ImpactForce = value; } }
        public int ImpactForceFrames { get { return m_ImpactForceFrames; } set { m_ImpactForceFrames = value; } }
        public string ImpactStateName { get { return m_ImpactStateName; } set { m_ImpactStateName = value; } }
        public float ImpactStateDisableTimer { get { return m_ImpactStateDisableTimer; } set { m_ImpactStateDisableTimer = value; } }
        public AnimationEventTrigger ReequipEvent { get { return m_ReequipEvent; } set { m_ReequipEvent = value; } }
        public int ReequipItemSubstateParameterValue { get { return m_ReequipItemSubstateParameterValue; } set { m_ReequipItemSubstateParameterValue = value; } }
        public bool ShowTrajectoryOnAim
        {
            get { return m_ShowTrajectoryOnAim; }
            set
            {
                m_ShowTrajectoryOnAim = value;
                if (Application.isPlaying && m_ShowTrajectoryOnAim && m_ThrownObject == null) {
                    Debug.LogError("Error: A TrajectoryObject must be added in order for the trajectory to be shown.");
                }
            }
        }
        public Vector3 TrajectoryOffset { get { return m_TrajectoryOffset; } set { m_TrajectoryOffset = value; } }

        private TrajectoryObject m_TrajectoryObject;
        protected UltimateCharacterLocomotion m_CharacterLocomotion;
        protected Transform m_CharacterTransform;

        private GameObject m_Object;
        private Transform m_ObjectTransform;
        private Renderer[] m_FirstPersonObjectRenderers;
        private Renderer[] m_ThirdPersonObjectRenderers;
        private IThrowableItemPerspectiveProperties m_ThrowableItemPerpectiveProperties;
        private GameObject m_InstantiatedThrownObject;
        protected TrajectoryObject m_InstantiatedTrajectoryObject;
        private RaycastHit m_RaycastHit;
#if ULTIMATE_CHARACTER_CONTROLLER_VR
        private IVRThrowableItem m_VRThrowableItem;
#endif

        private bool m_Aiming;
        protected bool m_Throwing;
        private bool m_Thrown;
        private bool m_Reequipping;
        private bool m_Reequipped;
        private int m_ReequipFrame;
        private bool m_ActivateVisibleObject;
        private ScheduledEventBase m_ReequipEventBase;
        private bool m_NextItemSet;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_TrajectoryObject = GetComponent<TrajectoryObject>();
            m_CharacterLocomotion = m_Character.GetCachedComponent<UltimateCharacterLocomotion>();
            m_CharacterTransform = m_CharacterLocomotion.transform;
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            m_VRThrowableItem = GetComponent<IVRThrowableItem>();
#endif

            if (m_ThrownObject != null && m_TrajectoryObject != null) {
                // The object has to be instantiated for GetComponent to work.
                var instantiatedThrownObject = ObjectPool.Instantiate(m_ThrownObject);
                var trajectoryCollider = instantiatedThrownObject.GetComponent<Collider>();
                if (trajectoryCollider != null) {
                    // Only sphere and capsules are supported.
                    if (trajectoryCollider is SphereCollider) {
                        var trajectorySphereCollider = trajectoryCollider as SphereCollider;
                        var sphereCollider = m_GameObject.AddComponent<SphereCollider>();
                        sphereCollider.center = trajectorySphereCollider.center;
                        sphereCollider.radius = trajectorySphereCollider.radius;
                        sphereCollider.enabled = false;
                    } else if (trajectoryCollider is CapsuleCollider) {
                        var trajectoryCapsuleCollider = trajectoryCollider as CapsuleCollider;
                        var capsuleCollider = m_GameObject.AddComponent<CapsuleCollider>();
                        capsuleCollider.center = trajectoryCapsuleCollider.center;
                        capsuleCollider.radius = trajectoryCapsuleCollider.radius;
                        capsuleCollider.height = trajectoryCapsuleCollider.height;
                        capsuleCollider.direction = trajectoryCapsuleCollider.direction;
                        capsuleCollider.enabled = false;
                    } else {
                        Debug.LogError($"Error: The collider of type {trajectoryCollider.GetType()} is not supported on the trajectory object " + m_ThrownObject.name);
                    }
                    m_GameObject.layer = LayerManager.SubCharacter;
                }
                ObjectPool.Destroy(instantiatedThrownObject);
            }
            m_ThrowableItemPerpectiveProperties = m_ActivePerspectiveProperties as IThrowableItemPerspectiveProperties;

            if (m_ShowTrajectoryOnAim && m_TrajectoryObject == null) {
                Debug.LogError($"Error: A TrajectoryObject must be added to the {m_GameObject.name} GameObject in order for the trajectory to be shown.");
            }

            if (m_ThrownObject == null) {
                Debug.LogError($"Error: A ThrownObject must be assigned to the {m_GameObject.name} GameObject.");
            }

            EventHandler.RegisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
            EventHandler.RegisterEvent(m_Character, "OnAnimatorReequipThrowableItem", ReequipThrowableItem);
        }

        /// <summary>
        /// Initialize the visible object transform.
        /// </summary>
        protected override void Start()
        {
            var perspectiveItem = m_CharacterLocomotion.FirstPersonPerspective ? m_Item.FirstPersonPerspectiveItem : m_Item.ThirdPersonPerspectiveItem;
            m_Object = perspectiveItem.GetVisibleObject();
            m_ObjectTransform = m_Object.transform;
            var firstPersonPerspectiveItem = m_Item.FirstPersonPerspectiveItem;
            if (firstPersonPerspectiveItem != null) {
                var visibleObject = firstPersonPerspectiveItem.GetVisibleObject();
                if (visibleObject != null) {
                    m_FirstPersonObjectRenderers = visibleObject.GetComponentsInChildren<Renderer>(true);
                }
            }
            var thirdPersonPerspectiveItem = m_Item.ThirdPersonPerspectiveItem;
            if (thirdPersonPerspectiveItem != null) {
                var visibleObject = thirdPersonPerspectiveItem.GetVisibleObject();
                if (visibleObject != null) {
                    m_ThirdPersonObjectRenderers = visibleObject.GetComponentsInChildren<Renderer>(true);
                }
            }
            if (m_ThrowableItemPerpectiveProperties == null) {
                m_ThrowableItemPerpectiveProperties = m_ActivePerspectiveProperties as IThrowableItemPerspectiveProperties;

                if (m_ThrowableItemPerpectiveProperties == null) {
                    Debug.LogError($"Error: The First/Third Person Throwable Item Properties component cannot be found for the Item {name}." +
                                   $"Ensure the component exists and the component's Action ID matches the Action ID of the Item ({m_ID}).");
                }
            }
            EnableObjectMeshRenderers(!m_Throwing && CanActivateVisibleObject());
        }

        /// <summary>
        /// Enables or disables the object mesh renderers for the current perspective.
        /// </summary>
        /// <param name="enable">Should the renderers be enabled?</param>
        public void EnableObjectMeshRenderers(bool enable)
        {
            var renderers = m_CharacterLocomotion.FirstPersonPerspective ? m_FirstPersonObjectRenderers : m_ThirdPersonObjectRenderers;
            if (renderers != null) {
                for (int i = 0; i < renderers.Length; ++i) {
                    renderers[i].enabled = enable;
                }
            }
        }

        /// <summary>
        /// Returns the ItemIdentifier which can be used by the item.
        /// </summary>
        /// <returns>The ItemIdentifier which can be used by the item.</returns>
        public override IItemIdentifier GetConsumableItemIdentifier()
        {
            return m_Item.ItemIdentifier;
        }

        /// <summary>
        /// Returns the amout of UsableItemIdentifier which has been consumed by the UsableItem.
        /// </summary>
        /// <returns>The amount consumed of the UsableItemIdentifier.</returns>
        public override int GetConsumableItemIdentifierAmount()
        {
            return -1;
        }

        /// <summary>
        /// The item has been equipped by the character.
        /// </summary>
        public override void Equip()
        {
            base.Equip();

            m_NextItemSet = false;
            EnableObjectMeshRenderers(CanActivateVisibleObject());
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            if (m_VRThrowableItem != null) {
                m_VRThrowableItem.Equip();
            }
#endif
        }

        /// <summary>
        /// Can the visible object be activated? An example of when it shouldn't be activated is when a grenade can be thrown but it is not the primary item
        /// so it shouldn't be thrown until after the throw action has started.
        /// </summary>
        /// <returns>True if the visible object can be activated.</returns>
        public override bool CanActivateVisibleObject()
        {
            return !m_DisableVisibleObject || m_ActivateVisibleObject;
        }

        /// <summary>
        /// Returns the substate index that the item should be in.
        /// </summary>
        /// <returns>the substate index that the item should be in.</returns>
        public override int GetItemSubstateIndex()
        {
            if (m_Reequipping) {
                return m_ReequipItemSubstateParameterValue;
            }
            // When the item is being thrown the index should be positive.
            if (m_Throwing) {
                return Mathf.Max(1, base.GetItemSubstateIndex());
            }
            return -1;
        }
        
        /// <summary>
        /// Updates the trajectory visualization.
        /// </summary>
        private void LateUpdate()
        {
            if (m_Aiming && m_ShowTrajectoryOnAim && !m_Throwing && !m_Reequipping && !m_NextItemSet && m_TrajectoryObject != null) {
                var trajectoryTransform = m_ThrowableItemPerpectiveProperties.TrajectoryLocation != null ? m_ThrowableItemPerpectiveProperties.TrajectoryLocation : m_CharacterTransform;
                var lookDirection = m_LookSource.LookDirection(trajectoryTransform.TransformPoint(m_TrajectoryOffset), false, m_ImpactLayers, true);
                var velocity = MathUtility.TransformDirection(m_Velocity, Quaternion.LookRotation(lookDirection, m_CharacterLocomotion.Up));
                // Prevent the item from being thrown behind the character. This can happen if the character is looking straight up and there is a positive
                // y velocity. Gravity will cause the thrown object to go in the opposite direction.
                if (Vector3.Dot(velocity.normalized, m_CharacterTransform.forward) < 0) {
                    velocity = m_CharacterTransform.up * velocity.magnitude;
                }
                m_TrajectoryObject.SimulateTrajectory(m_Character, trajectoryTransform.TransformPoint(m_TrajectoryOffset), Quaternion.identity,
                                                        velocity + (m_CharacterTransform.forward * m_CharacterLocomotion.LocalVelocity.z), Vector3.zero);
            }
        }

        /// <summary>
        /// The Aim ability has started or stopped.
        /// </summary>
        /// <param name="start">Has the Aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        private void OnAim(bool aim, bool inputStart)
        {
            if (!inputStart) {
                return;
            }
            m_Aiming = aim;

            if (!m_Aiming && m_ShowTrajectoryOnAim && m_TrajectoryObject != null) {
                m_TrajectoryObject.ClearTrajectory();
            }
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="itemAbility">The itemAbility that is trying to use the item.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanUseItem(ItemAbility itemAbility, UseAbilityState abilityState)
        {
            if (!base.CanUseItem(itemAbility, abilityState)) {
                return false;
            }

            // The item can't be used if it is already being used.
            if (abilityState == UseAbilityState.Start && (m_Throwing || m_Reequipping)) {
                return false;
            }

            // The item can't be used if there aren't any items left.
            if (m_Inventory != null && m_Inventory.GetItemIdentifierAmount(m_Item.ItemIdentifier) == 0) {
                return false;
            }

            // The item can't be used if it hasn't been started yet, is reequipping the throwable item, or has been requipped.
            if (abilityState == UseAbilityState.Update && (!m_Throwing || m_Reequipping || m_Reequipped)) {
                return false;
            }

            // Give the item a frame to recover from the reequip.
            if (Time.frameCount == m_ReequipFrame) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="itemAbility">The item ability that is using the item.</param>
        public override void StartItemUse(ItemAbility itemAbility)
        {
            base.StartItemUse(itemAbility);
            
            // An Animator Audio State Set may prevent the item from being used.
            if (!IsItemInUse()) {
                return;
            }

            if (!m_ThrowOnStopUse) {
                StartThrow();
            }

            // Instantiate the object that will actually be thrown.
            var location = m_ThrowableItemPerpectiveProperties.ThrowLocation;
            m_InstantiatedThrownObject = ObjectPool.Instantiate(m_ThrownObject, location.position, location.rotation, m_ObjectTransform.parent);
            m_InstantiatedThrownObject.transform.localScale = location.localScale;
            m_InstantiatedThrownObject.transform.SetLayerRecursively(m_StartLayer);
            m_InstantiatedTrajectoryObject = m_InstantiatedThrownObject.GetCachedComponent<TrajectoryObject>();
            if (m_InstantiatedTrajectoryObject == null) {
                Debug.LogError($"Error: {m_TrajectoryObject.name} must contain the TrajectoryObject component.");
                return;
            }
            if (m_InstantiatedTrajectoryObject is Destructible) {
                (m_InstantiatedTrajectoryObject as Destructible).InitializeDestructibleProperties(m_DamageAmount, m_ImpactForce, m_ImpactForceFrames,
                                                                m_ImpactLayers, m_ImpactStateName, m_ImpactStateDisableTimer, m_SurfaceImpact);
            }
            // The trajectory object will be enabled when the object is thrown.
            m_InstantiatedTrajectoryObject.enabled = false;

            // Hide the object that isn't thrown.
            EnableObjectMeshRenderers(false);

            // The instantiated object may not immediately be visible.
            if (m_DisableVisibleObject) {
                m_InstantiatedThrownObject.SetActive(false);
                m_ActivateVisibleObject = false;
                m_Item.SetVisibleObjectActive(false);

                if (m_ActivateThrowableObjectEvent.WaitForAnimationEvent) {
                    EventHandler.RegisterEvent(m_Character, "OnAnimatorActivateThrowableObject", ActivateThrowableObject);
                } else {
                    Scheduler.ScheduleFixed(m_ActivateThrowableObjectEvent.Duration, ActivateThrowableObject);
                }
            }
        }

        /// <summary>
        /// Activates the throwable object.
        /// </summary>
        private void ActivateThrowableObject()
        {
            m_InstantiatedThrownObject.SetActive(true);
            m_ActivateVisibleObject = true;
            if (!m_Item.IsActive()) {
                m_Item.SetVisibleObjectActive(true);
            }
            EventHandler.UnregisterEvent(m_Character, "OnAnimatorActivateThrowableObject", ActivateThrowableObject);
        }

        /// <summary>
        /// Starts the actual throw.
        /// </summary>
        protected virtual void StartThrow()
        {
            if (m_Throwing) {
                return;
            }

            m_Throwing = true;
            m_Thrown = false;
            m_Reequipping = false;
            m_Reequipped = false;

            if (m_Aiming && m_ShowTrajectoryOnAim && m_TrajectoryObject != null) {
                m_TrajectoryObject.ClearTrajectory();
            }
            m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();
        }

        /// <summary>
        /// Uses the item.
        /// </summary>
        public override void UseItem()
        {
            base.UseItem();

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkCharacter.ThrowItem(this);
            }
#endif
            ThrowItem();
            if (m_Inventory != null) {
                m_Inventory.AdjustItemIdentifierAmount(m_Item.ItemIdentifier, -1);
            }
            m_Thrown = true;
        }

        /// <summary>
        /// Throws the throwable object.
        /// </summary>
        public void ThrowItem()
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // The object has been thrown. If the ItemAction is on the server then that object should be spawned on the network.
            // Non-server actions should disable the mesh renderers so the object can take its place. The mesh renderers will be enabled again in a separate call.
            if (m_NetworkInfo != null) {
                EnableObjectMeshRenderers(false);
                if (!m_NetworkInfo.IsServer()) {
                    ObjectPool.Destroy(m_InstantiatedThrownObject);
                    m_InstantiatedThrownObject = null;
                    return;
                }
            }
#endif

            m_InstantiatedThrownObject.transform.parent = null;
            // The collider was previously disabled. Enable it again when it is thrown.
            var collider = m_InstantiatedThrownObject.GetCachedComponent<Collider>();
            collider.enabled = true;

            // When the item is used the trajectory object should start moving on its own.
            // The throwable item may be on the other side of an object (especially in the case of separate arms for the first person perspective). Perform a linecast
            // to ensure the throwable item doesn't move through any objects.
            if (!m_CharacterLocomotion.ActiveMovementType.UseIndependentLook(false) &&
                            Physics.Linecast(m_CharacterLocomotion.LookSource.LookPosition(), m_InstantiatedTrajectoryObject.transform.position, out m_RaycastHit,
                                                m_ImpactLayers, QueryTriggerInteraction.Ignore)) {
                m_InstantiatedTrajectoryObject.transform.position = m_RaycastHit.point;
            }

            var trajectoryTransform = m_ThrowableItemPerpectiveProperties.TrajectoryLocation != null ? m_ThrowableItemPerpectiveProperties.TrajectoryLocation : m_CharacterTransform;
            var lookDirection = m_LookSource.LookDirection(trajectoryTransform.TransformPoint(m_TrajectoryOffset), false, m_ImpactLayers, true);
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            if (m_VRThrowableItem != null && m_CharacterLocomotion.FirstPersonPerspective) {
                m_Velocity = m_VRThrowableItem.GetVelocity();
            }
#endif
            var velocity = MathUtility.TransformDirection(m_Velocity, Quaternion.LookRotation(lookDirection, m_CharacterLocomotion.Up));
            // Prevent the item from being thrown behind the character. This can happen if the character is looking straight up and there is a positive
            // y velocity. Gravity will cause the thrown object to go in the opposite direction.
            if (Vector3.Dot(velocity.normalized, m_CharacterTransform.forward) < 0 && m_CharacterTransform.InverseTransformDirection(velocity.normalized).y > 0) {
                velocity = m_CharacterTransform.up * velocity.magnitude;
            }
            m_InstantiatedTrajectoryObject.Initialize(velocity + (m_CharacterTransform.forward * m_CharacterLocomotion.LocalVelocity.z), Vector3.zero, m_Character, false);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null) {
                NetworkObjectPool.NetworkSpawn(m_ThrownObject, m_InstantiatedThrownObject, true);
            }
#endif
            // Optionally change the layer after the object has been thrown. This allows the object to change from the first person Overlay layer
            // to the Default layer after it has cleared the character's hands.
            if (m_StartLayer != m_ThrownLayer) {
                Scheduler.ScheduleFixed(m_LayerChangeDelay, ChangeThrownLayer, m_InstantiatedThrownObject);
            }
        }

        /// <summary>
        /// Changes the thrown object to the thrown layer.
        /// </summary>
        /// <param name="thrownObject">The object that was thrown.</param>
        private void ChangeThrownLayer(GameObject thrownObject)
        {
            thrownObject.transform.SetLayerRecursively(m_ThrownLayer);
        }

        /// <summary>
        /// The item has been used.
        /// </summary>
        public override void ItemUseComplete()
        {
            base.ItemUseComplete();
            m_Throwing = false;
            m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();

            // The item can be reequipped with an animation event or timer. The item should not be reequipped if it is disabled - the starting throw animation will
            // reequip a disabled item.
            if (!m_DisableVisibleObject && m_Inventory.GetItemIdentifierAmount(m_Item.ItemIdentifier) > 0) {
                m_Reequipping = true;
                if (!m_ReequipEvent.WaitForAnimationEvent) {
                    m_ReequipEventBase = Scheduler.ScheduleFixed(m_ReequipEvent.Duration, ReequipThrowableItem);
                }
            }
        }

        /// <summary>
        /// Tries to stop the item use.
        /// </summary>
        public override void TryStopItemUse()
        {
            if (m_ThrowOnStopUse && !m_Thrown) {
                StartThrow();
            }
        }

        /// <summary>
        /// Can the item use be stopped?
        /// </summary>
        /// <returns>True if the item use can be stopped.</returns>
        public override bool CanStopItemUse()
        {
            // The item can't be stopped until the object has been thrown and is not reequipping the object.
            if (!m_Thrown || m_Reequipping) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Stops the item use.
        /// </summary>
        public override void StopItemUse()
        {
            base.StopItemUse();

            // If there are no items remaining then the next item should be equipped. Wait until the item use is stopped so the use animation will complete.
            if (m_Inventory != null && m_Inventory.GetItemIdentifierAmount(m_Item.ItemIdentifier) == 0) {
                EventHandler.ExecuteEvent(m_Character, "OnNextItemSet", m_Item, true);
                m_NextItemSet = true;
            }

            m_Throwing = m_Thrown = false;
            m_Reequipping = m_Reequipped = false;
            m_ActivateVisibleObject = false;
            m_InstantiatedThrownObject = null;
            if (m_DisableVisibleObject) {
                m_Item.SetVisibleObjectActive(false);
            }
        }

        /// <summary>
        /// The ThrowableItem has been reequipped.
        /// </summary>
        private void ReequipThrowableItem()
        {
            if (!m_Reequipping) {
                return;
            }

            Scheduler.Cancel(m_ReequipEventBase);
            m_ReequipEventBase = null;
            m_Reequipping = false;
            m_Reequipped = true;
            m_ReequipFrame = Time.frameCount;

            // The item shouldn't be reequipped if it is out of ammo.
            if (m_Inventory != null && m_Inventory.GetItemIdentifierAmount(m_Item.ItemIdentifier) == 0) {
                return;
            }

            if (!m_DisableVisibleObject) {
                EnableObjectMeshRenderers(true);
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                    m_NetworkCharacter.EnableThrowableObjectMeshRenderers(this);
                }
#endif
            }
        }

        /// <summary>
        /// The item has been unequipped by the character.
        /// </summary>
        public override void Unequip()
        {
            base.Unequip();

            if (m_ShowTrajectoryOnAim && m_TrajectoryObject != null) {
                m_TrajectoryObject.ClearTrajectory();
            }
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            if (m_VRThrowableItem != null) {
                m_VRThrowableItem.Unequip();
            }
#endif
        }

        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        protected override void OnChangePerspectives(bool firstPersonPerspective)
        {
            base.OnChangePerspectives(firstPersonPerspective);

            // A new object is used for each perspective.
            var perspectiveItem = firstPersonPerspective ? m_Item.FirstPersonPerspectiveItem : m_Item.ThirdPersonPerspectiveItem;
            m_Object = perspectiveItem.GetVisibleObject();
            m_ObjectTransform = m_Object.transform;
            m_ThrowableItemPerpectiveProperties = m_ActivePerspectiveProperties as IThrowableItemPerspectiveProperties;

            // OnChangePerspective will be called whether or not the throwable item is equipped. Only set the mesh renderer status if the item is equipped.
            if (m_Item.IsActive()) {
                // If the object has already been thrown then the mesh renderer should be disabled.
                if (m_InstantiatedThrownObject != null) {
                    EnableObjectMeshRenderers(m_InstantiatedThrownObject.activeSelf);
                } else {
                    EnableObjectMeshRenderers(!m_Thrown);
                }
                if (m_Throwing && !m_Thrown) {
                    // Setup the thrown object if the item is in the process of being thrown.
                    var thrownObjectTransform = m_InstantiatedThrownObject.transform;
                    thrownObjectTransform.parent = m_ObjectTransform.parent;
                    var location = m_ThrowableItemPerpectiveProperties.ThrowLocation;
                    thrownObjectTransform.position = location.position;
                    thrownObjectTransform.rotation = location.rotation;
                    thrownObjectTransform.localScale = location.localScale;
                    m_InstantiatedThrownObject.transform.SetLayerRecursively(m_StartLayer);
                }
            }
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
            EventHandler.UnregisterEvent(m_Character, "OnAnimatorReequipThrowableItem", ReequipThrowableItem);
        }
    }
}