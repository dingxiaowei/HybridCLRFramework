/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties;
    using Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions;
    using Opsive.UltimateCharacterController.Items.Actions.Magic.ImpactActions;
    using Opsive.UltimateCharacterController.Items.Actions.Magic.BeginEndActions;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking;
    using Opsive.UltimateCharacterController.Networking.Character;
#endif
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The Magic Item uses Cast Actions to perform different magical actions when used. Impact Actions will then perform an action from the resulting cast impact.
    /// </summary>
    public class MagicItem : UsableItem
    {
        /// <summary>
        /// Specifies the direction of the cast.
        /// </summary>
        public enum CastDirection
        {
            None,       // The cast has no movement.
            Forward,    // The cast should move in the forward direction.
            Target,     // The cast should move towards a target.
            Indicate    // The cast should move towards an indicated position.
        }

        /// <summary>
        /// Specifies how often the magic item does its cast.
        /// </summary>
        public enum CastUseType
        {
            Single,     // The cast should occur once per use.
            Continuous  // The cast should occur every use update.
        }

        /// <summary>
        /// Specifies if the cast should interrupted.
        /// </summary>
        public enum CastInterruptSource
        {
            Movement = 1,   // The cast should be interrupted when the character moves.
            Damage = 2      // The cast should be interrupted when the character takes damage.
        }

        [Tooltip("Is the character required to be on the ground?")]
        [SerializeField] protected bool m_RequireGrounded = true;
        [Tooltip("The direction of the cast.")]
        [SerializeField] protected CastDirection m_Direction = CastDirection.Forward;
        [Tooltip("Should the look source be used when determining the cast direction?")]
        [SerializeField] protected bool m_UseLookSource = true;
        [Tooltip("The maximum distance of the movement cast direction.")]
        [SerializeField] protected float m_MaxDistance = 100;
        [Tooltip("The radius of the movement cast direction.")]
        [SerializeField] protected float m_Radius = 0.1f;
        [Tooltip("The layers that the movement directions can collide with.")]
        [SerializeField] protected LayerMask m_DetectLayers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.UI | 1 << LayerManager.SubCharacter | 1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect);
        [Tooltip("The maximum angle that the target object can be compared to the character's forward direction.")]
        [SerializeField] protected float m_MaxAngle = 30;
        [Tooltip("The maximum number of colliders that can be detected by the target cast.")]
        [SerializeField] protected int m_MaxCollisionCount = 100;
        [Tooltip("The number of objects that a single cast should cast.")]
        [SerializeField] protected int m_TargetCount = 1;
        [Tooltip("The transform used to indicate the surface. Can be null.")]
        [SerializeField] protected Transform m_SurfaceIndicator;
        [Tooltip("The offset when positioning the surface indicator.")]
        [SerializeField] protected Vector3 m_SurfaceIndicatorOffset = new Vector3(0, 0.1f, 0);
        [Tooltip("Specifies how often the cast is used.")]
        [SerializeField] protected CastUseType m_UseType;
        [Tooltip("The minimum duration of the continuous use type. If a value of -1 is set then the item will be stopped when the stop is requested.")]
        [SerializeField] protected float m_MinContinuousUseDuration = 1;
        [Tooltip("Should the continuous use type cast every update?")]
        [SerializeField] protected bool m_ContinuousCast;
        [Tooltip("The amount of the ItemIdentifier that should be used each cast.")]
        [SerializeField] protected int m_UseAmount;
        [Tooltip("Specifies when the cast should be interrupted.")]
        [SerializeField] protected CastInterruptSource m_InterruptSource;
        [Tooltip("The SurfaceImpact of the cast.")]
        [SerializeField] protected SurfaceImpact m_SurfaceImpact;
        [Tooltip("The value to add to the Item Substate Index when the item can stop.")]
        [SerializeField] protected int m_CanStopSubstateIndexAddition = 10;
        [Tooltip("The serialization data for the BeginActions.")]
        [SerializeField] [ForceSerialized] protected Serialization[] m_BeginActionData;
        [Tooltip("The serialization data for the CastActions.")]
        [SerializeField] [ForceSerialized] protected Serialization[] m_CastActionData;
        [Tooltip("The serialization data for the MagicImpactActions.")]
        [SerializeField] [ForceSerialized] protected Serialization[] m_ImpactActionData;
        [Tooltip("The serialization data for the EndActions.")]
        [SerializeField] [ForceSerialized] protected Serialization[] m_EndActionData;

        public bool RequireGrounded { get { return m_RequireGrounded; } set { m_RequireGrounded = value; } }
        public CastDirection Direction { get { return m_Direction; } set { m_Direction = value; } }
        public bool UseLookSource { get { return m_UseLookSource; } set { m_UseLookSource = value; } }
        public float MaxDistance { get { return m_MaxDistance; } set { m_MaxDistance = value; } }
        public float Radius { get { return m_Radius; } set { m_Radius = value; } }
        public LayerMask DetectLayers { get { return m_DetectLayers; } set { m_DetectLayers = value; } }
        public float MaxAngle { get { return m_MaxAngle; } set { m_MaxAngle = value; } }
        public int TargetCount { get { return m_TargetCount; } set { m_TargetCount = value; } }
        public Transform SurfaceIndicator { get { return m_SurfaceIndicator; } }
        public Vector3 SurfaceIndicatorOffset { get { return m_SurfaceIndicatorOffset; }
            set {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                // The local surface indicator should not show for remote players.
                if (m_NetworkInfo != null && !m_NetworkInfo.IsLocalPlayer()) {
                    return;
                }
#endif
                m_SurfaceIndicatorOffset = value; 
            }
        }
        public CastUseType UseType { get { return m_UseType; } set { m_UseType = value; } }
        public float MinContinuousUseDuration { get { return m_MinContinuousUseDuration; } set { m_MinContinuousUseDuration = value; } }
        public bool ContinuousCast { get { return m_ContinuousCast; } set { m_ContinuousCast = value; } }
        public int UseAmount { get { return m_UseAmount; } set { m_UseAmount = value; } }
        public CastInterruptSource InterruptSource { get { return m_InterruptSource; } set { m_InterruptSource = value; } }
        public SurfaceImpact SurfaceImpact { get { return m_SurfaceImpact; } set { m_SurfaceImpact = value; } }
        public int CanStopSubstateIndexAddition { get { return m_CanStopSubstateIndexAddition; } set { m_CanStopSubstateIndexAddition = value; } }
        public Serialization[] BeginActionData {
            get { return m_BeginActionData; }
            set {
                m_BeginActionData = value;
                if (!Application.isPlaying) {
                    DeserializeBeginActions(false);
                }
            }
        }
        public Serialization[] CastActionData {
            get { return m_CastActionData; }
            set {
                m_CastActionData = value;
                if (!Application.isPlaying) {
                    DeserializeCastActions(false);
                }
            }
        }
        public Serialization[] ImpactActionData {
            get { return m_ImpactActionData; }
            set {
                m_ImpactActionData = value;
                if (!Application.isPlaying) {
                    DeserializeImpactActions(false);
                }
            }
        }
        public Serialization[] EndActionData {
            get { return m_EndActionData; }
            set {
                m_EndActionData = value;
                if (!Application.isPlaying) {
                    DeserializeEndActions(false);
                }
            }
        }

        private Transform m_CharacterTransform;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private CharacterLayerManager m_CharacterLayerManager;
        private IMagicItemPerspectiveProperties m_MagicItemPerspectiveProperties;
        private BeginEndAction[] m_BeginActions;
        private CastAction[] m_CastActions;
        private ImpactAction[] m_ImpactActions;
        private BeginEndAction[] m_EndActions;
        private ItemAbility m_StartAbility;

        private Collider[] m_TargetColliders;
        private float[] m_TargetAngles;
        private float m_StartUseTime;
        private float m_StartCastTime;

        private Vector3 m_CastDirection;
        private Vector3 m_CastPosition;
        private Vector3 m_CastNormal;
        private bool m_Used;
        private uint m_CastID;
        private bool[] m_CastActionUsed;
        private bool[] m_CastActionCasted;

        private bool m_InterruptImpact;
        private bool m_StopRequested;
        private bool m_Stopping;
        private bool m_ForceStop;

        public GameObject Character { get { return m_Character; } }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        public INetworkInfo NetworkInfo { get { return m_NetworkInfo; } }
        public INetworkCharacter NetworkCharacter { get { return m_NetworkCharacter; } }
#endif

        public IMagicItemPerspectiveProperties MagicItemPerspectiveProperties { get { return m_MagicItemPerspectiveProperties; } }
        public BeginEndAction[] BeginActions {
            get {
                if (!Application.isPlaying && m_BeginActions == null) { DeserializeBeginActions(false); }
                return m_BeginActions;
            }
            set { m_BeginActions = value; }
        }
        public CastAction[] CastActions {
            get {
                if (!Application.isPlaying && m_CastActions == null) { DeserializeCastActions(false); }
                return m_CastActions;
            }
            set { m_CastActions = value; }
        }
        public ImpactAction[] ImpactActions {
            get {
                if (!Application.isPlaying && m_ImpactActions == null) { DeserializeImpactActions(false); }
                return m_ImpactActions;
            }
            set { m_ImpactActions = value; }
        }
        public BeginEndAction[] EndActions {
            get {
                if (!Application.isPlaying && m_EndActions == null) { DeserializeEndActions(false); }
                return m_EndActions;
            }
            set { m_EndActions = value; }
        }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_CharacterTransform = m_Character.transform;
            m_CharacterLocomotion = m_Character.GetCachedComponent<UltimateCharacterLocomotion>();
            m_CharacterLayerManager = m_Character.GetCachedComponent<CharacterLayerManager>();

            m_MagicItemPerspectiveProperties = m_ActivePerspectiveProperties as IMagicItemPerspectiveProperties;

            DeserializeBeginActions(false);
            DeserializeCastActions(false);
            DeserializeImpactActions(false);
            DeserializeEndActions(false);

            if (m_CastActions != null) {
                m_CastActionUsed = new bool[m_CastActions.Length];
                m_CastActionCasted = new bool[m_CastActions.Length];
            }
            if (m_SurfaceIndicator != null) {
                m_SurfaceIndicator.gameObject.SetActive(false);
            }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // The local surface indicator should not show for remote players.
            if (m_NetworkInfo != null && !m_NetworkInfo.IsLocalPlayer()) {
                m_SurfaceIndicator = null;
            }
#endif
        }

        /// <summary>
        /// Deserialize the start actions.
        /// </summary>
        /// <param name="forceDeserialization">Should the actions be force deserialized?</param>
        public void DeserializeBeginActions(bool forceDeserialization)
        {
            // The begin actions only need to be deserialized once.
            if (m_BeginActions != null && !forceDeserialization) {
                return;
            }

            if (m_BeginActionData != null) {
                m_BeginActions = new BeginEndAction[m_BeginActionData.Length];
                for (int i = 0; i < m_BeginActions.Length; ++i) {
                    m_BeginActions[i] = m_BeginActionData[i].DeserializeFields(MemberVisibility.Public) as BeginEndAction;
                    if (Application.isPlaying) {
                        m_BeginActions[i].Initialize(m_Character, this, true, i);
                    }
                }
            }
        }

        /// <summary>
        /// Deserialize the cast actions.
        /// </summary>
        /// <param name="forceDeserialization">Should the actions be force deserialized?</param>
        public void DeserializeCastActions(bool forceDeserialization)
        {
            // The cast actions only need to be deserialized once.
            if (m_CastActions != null && !forceDeserialization) {
                return;
            }

            if (m_CastActionData != null) {
                m_CastActions = new CastAction[m_CastActionData.Length];
                for (int i = 0; i < m_CastActions.Length; ++i) {
                    m_CastActions[i] = m_CastActionData[i].DeserializeFields(MemberVisibility.Public) as CastAction;
                    if (Application.isPlaying) {
                        m_CastActions[i].Initialize(m_Character, this, i);
                    }
                }
            }
        }

        /// <summary>
        /// Deserialize the impact actions.
        /// </summary>
        /// <param name="forceDeserialization">Should the actions be force deserialized?</param>
        public void DeserializeImpactActions(bool forceDeserialization)
        {
            // The impact actions only need to be deserialized once.
            if (m_ImpactActions != null && !forceDeserialization) {
                return;
            }

            if (m_ImpactActionData != null) {
                m_ImpactActions = new ImpactAction[m_ImpactActionData.Length];
                for (int i = 0; i < m_ImpactActions.Length; ++i) {
                    m_ImpactActions[i] = m_ImpactActionData[i].DeserializeFields(MemberVisibility.Public) as ImpactAction;
                    if (Application.isPlaying) {
                        m_ImpactActions[i].Initialize(m_Character, this, i);
                    }
                }
            }
        }

        /// <summary>
        /// Deserialize the end actions.
        /// </summary>
        /// <param name="forceDeserialization">Should the actions be force deserialized?</param>
        public void DeserializeEndActions(bool forceDeserialization)
        {
            // The end actions only need to be deserialized once.
            if (m_EndActions != null && !forceDeserialization) {
                return;
            }

            if (m_EndActionData != null) {
                m_EndActions = new BeginEndAction[m_EndActionData.Length];
                for (int i = 0; i < m_EndActions.Length; ++i) {
                    m_EndActions[i] = m_EndActionData[i].DeserializeFields(MemberVisibility.Public) as BeginEndAction;
                    if (Application.isPlaying) {
                        m_EndActions[i].Initialize(m_Character, this, false, i);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes any values that require on other components to first initialize.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (m_CastActions != null) {
                // Awake is called when the item is ready to go (within start).
                for (int i = 0; i < m_CastActions.Length; ++i) {
                    m_CastActions[i].Awake();
                }
            }

            if (m_MagicItemPerspectiveProperties == null) {
                m_MagicItemPerspectiveProperties = m_ActivePerspectiveProperties as IMagicItemPerspectiveProperties;

                if (m_MagicItemPerspectiveProperties == null) {
                    Debug.LogError($"Error: The First/Third Person Magic Item Properties component cannot be found for the Item {name}." +
                                   $"Ensure the component exists and the component's Action ID matches the Action ID of the Item ({m_ID}).");
                }
            }
        }

        /// <summary>
        /// The item has been equipped by the character.
        /// </summary>
        public override void Equip()
        {
            base.Equip();

            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterMoving", OnMoving);
            EventHandler.RegisterEvent<Ability, bool>(m_Character, "OnCharacterAbilityActive", OnAbilityActive);
            EventHandler.RegisterEvent<float, Vector3, Vector3, GameObject, Collider>(m_Character, "OnHealthDamage", OnDamage);
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
            if (abilityState == UseAbilityState.Update) {
                // Don't allow the item to continue to be reused if it can no longer be used.
                if (m_Used && CanStopItemUse()) {
                    return false;
                }
            } else if (abilityState == UseAbilityState.Start) {
                // Certain items require the character to be grounded.
                if (m_RequireGrounded && !m_CharacterLocomotion.Grounded) {
                    return false;
                }
                // The item can't start if it is in the process of being stopped.
                if (m_Stopping) {
                    return false;
                }
                // If the cast isn't valid then the item shouldn't start.
                if (m_Direction == CastDirection.Target) {
                    DetermineTargetColliders();
                }
                if (!DetermineCastValues(0, ref m_CastDirection, ref m_CastPosition, ref m_CastNormal)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Can the ability be started?
        /// </summary>
        /// <param name="ability">The ability that is trying to start.</param>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility(Ability ability) 
        {
            if (!m_StartAbility.AllowPositionalInput && ability is StoredInputAbilityBase) {
                return false;
            }
            if (!(ability is Jump)) {
                return true;
            }
            return !m_RequireGrounded;
        }

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="itemAbility">The item ability that is using the item.</param>
        public override void StartItemUse(ItemAbility itemAbility)
        {
            base.StartItemUse(itemAbility);

            m_StartUseTime = Time.time;
            m_StartCastTime = 0;
            m_StartAbility = itemAbility;

            // The Begin Actions allows the effect to play any starting effect.
            StartStopBeginEndActions(true, true, false);

            // Reset the cast action used time.
            if (m_CastActionUsed != null) {
                for (int i = 0; i < m_CastActionUsed.Length; ++i) {
                    m_CastActionUsed[i] = m_CastActionCasted[i] = false;
                }
            }
        }

        /// <summary>
        /// Starts or stops the begin or end actions.
        /// </summary>
        /// <param name="beginActions">Should the begin actions be started?</param>
        /// <param name="start">Should the actions be started?</param>
        /// <param name="networkEvent">Should the event be sent over the network?</param>
        public void StartStopBeginEndActions(bool beginActions, bool start, bool networkEvent)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (networkEvent && m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkCharacter.StartStopBeginEndMagicActions(this, beginActions, start);
            }
#endif

            var actions = beginActions ? m_BeginActions : m_EndActions;
            if (actions != null) {
                for (int i = 0; i < actions.Length; ++i) {
                    if (start) {
                        actions[i].Start(m_MagicItemPerspectiveProperties.OriginLocation);
                    } else {
                        actions[i].Stop();
                    }
                }
            }
        }

        /// <summary>
        /// Uses the item.
        /// </summary>
        public override void UseItem()
        {
            base.UseItem();

            // If the item hasn't been used yet the begin actions stop.
            if (m_StartCastTime == 0) {
                m_StartCastTime = Time.time;
                StartStopBeginEndActions(true, false, true);
            }

            if (m_CastActions != null) {
                // Only cast the actions that haven't been casted yet.
                var actionsCasted = !m_ContinuousCast;
                if (!m_ContinuousCast) {
                    for (int i = 0; i < m_CastActionUsed.Length; ++i) {
                        if (!m_CastActionUsed[i]) {
                            actionsCasted = false;
                            break;
                        }
                    }
                }

                if (!actionsCasted) {
                    var useCount = m_Direction == CastDirection.Target ? m_TargetCount : 1;
                    for (int i = 0; i < useCount; ++i) {
                        // The values should be updated for the current cast.
                        if (m_Used || i > 0) {
                            if (!DetermineCastValues(i, ref m_CastDirection, ref m_CastPosition, ref m_CastNormal)) {
                                continue;
                            }
                        }

                        for (int j = 0; j < m_CastActions.Length; ++j) {
                            if (!m_ContinuousCast && m_CastActionUsed[j]) {
                                continue;
                            }

                            // The action may not need to cast if it's not time yet.
                            if (m_StartCastTime + m_CastActions[j].Delay > Time.time) {
                                continue;
                            }

                            if (m_CastActions[j].CastID == 0) {
                                m_CastActions[j].CastID = ++m_CastID;
                            }

                            m_CastActions[j].Cast(m_MagicItemPerspectiveProperties.OriginLocation, m_CastDirection, m_CastPosition);
                            m_CastActionCasted[j] = !m_ContinuousCast;
                        }
                    }

                    // Synchronize the used array with the casted array. This will allow the same action to be performed for multiple targets.
                    if (!m_ContinuousCast) {
                        for (int i = 0; i < m_CastActionUsed.Length; ++i) {
                            m_CastActionUsed[i] = m_CastActionCasted[i];
                        }
                    }
                }
            }
            if (!m_Used) {
                // The item isn't done being used until all actions have been used.
                if (!m_ContinuousCast) {
                    for (int i = 0; i < m_CastActionUsed.Length; ++i) {
                        if (!m_CastActionUsed[i]) {
                            return;
                        }
                    }
                }

                // If the item was just used the end actions should start.
                if (m_UseType == CastUseType.Single) {
                    StartStopBeginEndActions(false, true, true);
                }
                m_Inventory.AdjustItemIdentifierAmount(m_Item.ItemIdentifier, m_UseAmount);
                m_Used = true;
            }
        }

        /// <summary>
        /// Determines the colliders that are hit by the target direction.
        /// </summary>
        private void DetermineTargetColliders()
        {
            if (m_TargetColliders == null) {
                m_TargetColliders = new Collider[m_MaxCollisionCount];
                m_TargetAngles = new float[m_MaxCollisionCount];
                // Initialize to the max value.
                for (int i = 0; i < m_TargetAngles.Length; ++i) {
                    m_TargetAngles[i] = int.MaxValue;
                }
            }
            var hitCount = Physics.OverlapSphereNonAlloc(m_CharacterTransform.position, m_MaxDistance, m_TargetColliders, m_DetectLayers, QueryTriggerInteraction.Ignore);
            if (hitCount == 0) {
                return;
            }

#if UNITY_EDITOR
            if (hitCount == m_TargetColliders.Length) {
                Debug.LogWarning("Warning: The hit count is equal to the max collider array size. This will cause objects to be missed. Consider increasing the max collision count size.");
            }
#endif
            for (int i = 0; i < hitCount; ++i) {
                if (m_TargetColliders[i].transform.IsChildOf(m_CharacterTransform)) {
                    m_TargetAngles[i] = int.MaxValue;
                    continue;
                }

                // The target object needs to be within the field of view of the current object
                var direction = m_TargetColliders[i].transform.position - m_CharacterTransform.position;
                var angle = Vector3.Angle(direction, transform.forward);
                if (angle < m_MaxAngle * 0.5f) {
                    // The target must be within sight.
                    var hitTransform = false;
                    if (Physics.Linecast(m_CharacterTransform.position, m_TargetColliders[i].transform.position, out var raycastHit, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                        if (raycastHit.transform.IsChildOf(m_TargetColliders[i].transform) || raycastHit.transform.IsChildOf(m_CharacterTransform)) {
                            hitTransform = true;
                        }
                    }

                    // Find the target that is most in front of the character.
                    m_TargetAngles[i] = hitTransform ? angle : int.MaxValue;
                } else {
                    m_TargetAngles[i] = int.MaxValue;
                }
            }

            // Sort by the angle. Return the min angle.
            System.Array.Sort(m_TargetAngles, m_TargetColliders);
        }

        /// <summary>
        /// Determines the values of the cast.
        /// </summary>
        /// <param name="index">The index of the target position to retrieve.</param>
        /// <param name="direction">A reference to the target direction.</param>
        /// <param name="position">A reference to the target position.</param>
        /// <param name="normal">A reference to the target normal.</param>
        /// <returns>True if the cast is valid.</returns>
        private bool DetermineCastValues(int index, ref Vector3 direction, ref Vector3 position, ref Vector3 normal)
        {
            if (m_Direction == CastDirection.Forward || m_Direction == CastDirection.Indicate) {
                Vector3 castPosition;
                if (m_Direction == CastDirection.Forward) {
                    direction = m_UseLookSource ? m_LookSource.LookDirection(false) : m_CharacterTransform.forward;
                    castPosition = m_UseLookSource ? m_LookSource.LookPosition() : m_CharacterTransform.position;
                } else { // Indicate.
                    direction = m_LookSource.LookDirection(false);
                    castPosition = m_LookSource.LookPosition();
                }

                m_CharacterLocomotion.EnableColliderCollisionLayer(false);
                var validCast = Physics.SphereCast(castPosition - direction * m_Radius, m_Radius, direction, out var raycastHit, m_MaxDistance, m_DetectLayers, QueryTriggerInteraction.Ignore);
                if (validCast) {
                    // The Cast Actions may indicate that the position is invalid.
                    if (m_CastActions != null) {
                        for (int i = 0; i < m_CastActions.Length; ++i) {
                            if (!m_CastActions[i].IsValidTargetPosition(raycastHit.point, raycastHit.normal)) {
                                return false;
                            }
                        }
                    }

                    position = raycastHit.point;
                    normal = raycastHit.normal;
                }
                m_CharacterLocomotion.EnableColliderCollisionLayer(true);
                return validCast;
            } else if (m_Direction == CastDirection.Target) {
                if (index >= m_TargetAngles.Length || m_TargetAngles[index] == int.MaxValue) {
                    return false;
                }
                var targetTransform = m_TargetColliders[index].transform;
                PivotOffset pivotOffset;
                if ((pivotOffset = targetTransform.gameObject.GetCachedComponent<PivotOffset>()) != null) {
                    position = targetTransform.TransformPoint(pivotOffset.Offset);
                } else {
                    position = targetTransform.position;
                }
                direction = (position - m_MagicItemPerspectiveProperties.OriginLocation.position).normalized;
                normal = targetTransform.up;
                return true;
            }

            // None direction.
            direction = m_CharacterTransform.forward;
            position = m_CharacterTransform.position;
            normal = m_CharacterTransform.up;
            return true;
        }

        /// <summary>
        /// Allows the item to update while it is being used.
        /// </summary>
        public override void UseItemUpdate()
        {
            base.UseItemUpdate();

            var beginEndActions = (m_Used ? m_EndActions : m_BeginActions);
            if (beginEndActions != null) {
                for (int i = 0; i < beginEndActions.Length; ++i) {
                    beginEndActions[i].Update();
                }
            }
        }

        /// <summary>
        /// Is the item waiting to be used? This will return true if the item is waiting to be charged or pulled back.
        /// </summary>
        /// <returns>Returns true if the item is waiting to be used.</returns>
        public override bool IsItemUsePending()
        {
            return !CanStopItemUse();
        }

        /// <summary>
        /// Returns the substate index that the item should be in.
        /// </summary>
        /// <returns>the substate index that the item should be in.</returns>
        public override int GetItemSubstateIndex()
        {
            return base.GetItemSubstateIndex() + (CanStopItemUse() ? m_CanStopSubstateIndexAddition : 0);
        }

        /// <summary>
        /// The item has been used.
        /// </summary>
        public override void ItemUseComplete()
        {
            base.ItemUseComplete();

            for (int i = 0; i < m_CastActions.Length; ++i) {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                    m_NetworkCharacter.StopMagicCast(this, i, m_CastActions[i].CastID);
                }
#endif
                m_CastActions[i].Stop();
            }
        }

        /// <summary>
        /// Draws an indicator when the direction is indicate.
        /// </summary>
        public void LateUpdate()
        {
            var direction = Vector3.zero;
            var position = Vector3.zero;
            var normal = Vector3.zero;
            if (m_Direction != CastDirection.Indicate || !DetermineCastValues(-1, ref direction, ref position, ref normal)) {
                if (m_SurfaceIndicator != null && m_SurfaceIndicator.gameObject.activeSelf) {
                    m_SurfaceIndicator.gameObject.SetActive(false);
                }
                return;
            }

            // The position is valid. Show an optional indicator.
            if (m_SurfaceIndicator != null) {
                m_SurfaceIndicator.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(direction, normal));
                m_SurfaceIndicator.position = position + m_SurfaceIndicator.TransformDirection(m_SurfaceIndicatorOffset);
                if (!m_SurfaceIndicator.gameObject.activeSelf) {
                    m_SurfaceIndicator.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// A cast has caused a collision. Perform the impact actions.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="source">The object that caused the cast.</param>
        /// <param name="target">The object that was hit by the cast.</param>
        /// <param name="hit">The raycast that caused the impact.</param>
        public void PerformImpact(uint castID, GameObject source, GameObject target, RaycastHit hit)
        {
            if (!IsValidCollisionObject(target)) {
                return;
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkCharacter.MagicImpact(this, castID, source, target, hit.point, hit.normal);
            }
#endif

            m_InterruptImpact = false;
            if (m_ImpactActions != null) {
                for (int i = 0; i < m_ImpactActions.Length; ++i) {
                    // Stop calling interrupt if InteruptImpact was triggered.
                    if (m_InterruptImpact) {
                        break;
                    }
                    m_ImpactActions[i].Impact(castID, source, target, hit);
                }
            }

            // Execute an event to allow interested objects know about the collision.
            EventHandler.ExecuteEvent(target, "OnMagicCastCollision", hit, m_SurfaceImpact);
        }

        /// <summary>
        /// Returns true if the object can be collided with.
        /// </summary>
        /// <param name="other">The object that may be able to be collided with.</param>
        /// <returns>True if the object can be collided with.</returns>
        private bool IsValidCollisionObject(GameObject other)
        {
            if (m_Direction == CastDirection.Target) {
                return MathUtility.InLayerMask(other.layer, m_DetectLayers);
            }
            return true;
        }

        /// <summary>
        /// The impact actions should be interrupted. Consider the case where the first action deducts health and the second action plays a particle effect.
        /// The particle effect should not be played if the object does not have the Health component.
        /// </summary>
        public void InterruptImpact()
        {
            m_InterruptImpact = true;
        }

        /// <summary>
        /// Tries to stop the item use.
        /// </summary>
        public override void TryStopItemUse()
        {
            base.TryStopItemUse();

            // The end actions aren't called until the continuous use item stops.
            m_StopRequested = true;
            if (!m_Stopping && CanStopItemUse()) {
                m_Stopping = true;
                m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();
                if (m_CastActions != null) {
                    for (int i = 0; i < m_CastActions.Length; ++i) {
                        m_CastActions[i].WillStop();
                    }
                }

                if (m_UseType == CastUseType.Continuous) {
                    StartStopBeginEndActions(false, true, true);
                }
            }
        }

        /// <summary>
        /// Can the item use be stopped?
        /// </summary>
        /// <returns>True if the item use can be stopped.</returns>
        public override bool CanStopItemUse()
        {
            // The item can always stop when the attributes are no longer valid.
            if ((m_UseAttribute != null && !m_UseAttribute.IsValid(-m_UseAttributeAmount)) || 
                (m_CharacterUseAttribute != null && !m_CharacterUseAttribute.IsValid(-m_CharacterUseAttributeAmount))) {
                return true;
            }
            return m_UseType == CastUseType.Single || 
                (m_StopRequested && (m_MinContinuousUseDuration == -1 || (m_MinContinuousUseDuration > 0 && m_StartUseTime + m_MinContinuousUseDuration < Time.time)));
        }

        /// <summary>
        /// Stops the item use.
        /// </summary>
        public override void StopItemUse()
        {
            base.StopItemUse();
            // If Force Stop is true then the cast was interrupted. Reset the objects.
            if (m_ForceStop) {
                ItemUseComplete();
                StartStopBeginEndActions(true, false, false);
            }

            m_Used = false;
            m_Stopping = m_StopRequested = false;
            m_ForceStop = false;
            StartStopBeginEndActions(false, false, false);
        }

        /// <summary>
        /// The item has started to be unequipped by the character.
        /// </summary>
        public override void StartUnequip()
        {
            base.StartUnequip();

            if (m_SurfaceIndicator != null && m_SurfaceIndicator.gameObject.activeSelf) {
                m_SurfaceIndicator.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The item has been unequipped by the character.
        /// </summary>
        public override void Unequip()
        {
            base.Unequip();

            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterMoving", OnMoving);
            EventHandler.UnregisterEvent<Ability, bool>(m_Character, "OnCharacterAbilityActive", OnAbilityActive);
            EventHandler.UnregisterEvent<float, Vector3, Vector3, GameObject, Collider>(m_Character, "OnHealthDamage", OnDamage);
        }

        /// <summary>
        /// The character has started to or stopped moving.
        /// </summary>
        /// <param name="moving">Is the character moving?</param>
        private void OnMoving(bool moving)
        {
            // Stop the item if the character starts to move and the cast should be interrupted on movement.
            if (moving && (m_InterruptSource & CastInterruptSource.Movement) != 0 && IsItemInUse()) {
                m_ForceStop = true;
                m_StartAbility.StopAbility(true);
            }
        }

        /// <summary>
        /// The character's ability has been started or stopped.
        /// </summary>
        /// <param name="ability">The ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnAbilityActive(Ability ability, bool active)
        {
            if (!active || (!(ability is Jump) && !(ability is Fall))) {
                return;
            }

            // Stop the item if the character starts to jump or fall and the cast should be interrupted on movement.
            if ((m_InterruptSource & CastInterruptSource.Movement) != 0 && IsItemInUse()) {
                m_ForceStop = true;
                m_StartAbility.StopAbility(true);
            }
        }

        /// <summary>
        /// The character has taken damage.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="force">The amount of force applied to the object while taking the damage.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        /// <param name="hitCollider">The Collider that was hit.</param>
        private void OnDamage(float amount, Vector3 position, Vector3 force, GameObject attacker, Collider hitCollider)
        {
            // The item can stop the use ability when the character takes damage.
            if ((m_InterruptSource & CastInterruptSource.Damage) != 0 && IsItemInUse()) {
                m_ForceStop = true;
                m_StartAbility.StopAbility(true);
            }
        }

        /// <summary>
        /// The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        protected override void OnChangePerspectives(bool firstPersonPerspective)
        {
            base.OnChangePerspectives(firstPersonPerspective);

            var targetMagicItemPerspectiveProperties = m_ActivePerspectiveProperties as IMagicItemPerspectiveProperties;
            // The OriginLocation cannot be null.
            if (targetMagicItemPerspectiveProperties.OriginLocation == null) {
                Debug.LogError($"Error: The OriginLocation is null on the {name} MagicItemPerspectiveProperties.");
                return;
            }

            m_MagicItemPerspectiveProperties = targetMagicItemPerspectiveProperties;

            if (m_BeginActions != null) {
                for (int i = 0; i < m_BeginActions.Length; ++i) {
                    m_BeginActions[i].OnChangePerspectives(m_MagicItemPerspectiveProperties.OriginLocation);
                }
            }
            if (m_CastActions != null) {
                for (int i = 0; i < m_CastActions.Length; ++i) {
                    m_CastActions[i].OnChangePerspectives(m_MagicItemPerspectiveProperties.OriginLocation);
                }
            }
            if (m_EndActions != null) {
                for (int i = 0; i < m_EndActions.Length; ++i) {
                    m_EndActions[i].OnChangePerspectives(m_MagicItemPerspectiveProperties.OriginLocation);
                }
            }
        }

        /// <summary>
        /// The item has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_BeginActions != null) {
                for (int i = 0; i < m_BeginActions.Length; ++i) {
                    m_BeginActions[i].OnDestroy();
                }
            }
            if (m_CastActions != null) {
                for (int i = 0; i < m_CastActions.Length; ++i) {
                    m_CastActions[i].OnDestroy();
                }
            }
            if (m_ImpactActions != null) {
                for (int i = 0; i < m_ImpactActions.Length; ++i) {
                    m_ImpactActions[i].OnDestroy();
                }
            }
            if (m_EndActions != null) {
                for (int i = 0; i < m_EndActions.Length; ++i) {
                    m_EndActions[i].OnDestroy();
                }
            }
        }
    }
}