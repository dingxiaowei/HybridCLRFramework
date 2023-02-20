/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Character.MovementTypes;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Character.Effects;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
using Opsive.UltimateCharacterController.Networking;
using Opsive.UltimateCharacterController.Networking.Character;
#endif
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Character
{
    /// <summary>
    /// The UltimateCharacterLocomotion component extends the CharacterLocomotion functionality by handling the following features:
    /// - Movement Types
    /// - Abilities
    /// - Effects
    /// - Animator Knowledge
    /// </summary>
    public class UltimateCharacterLocomotion : CharacterLocomotion
    {
        [Tooltip("The name of the state that should be activated when the character is in a first person perspective.")]
        [SerializeField] protected string m_FirstPersonStateName = "FirstPerson";
        [Tooltip("The name of the state that should be activated when the character is in a third person perspective.")]
        [SerializeField] protected string m_ThirdPersonStateName = "ThirdPerson";
        [Tooltip("The name of the state that should be activated when the character is moving.")]
        [SerializeField] protected string m_MovingStateName = "Moving";
        [Tooltip("The name of the state that should be activated when the character is airborne.")]
        [SerializeField] protected string m_AirborneStateName = "Airborne";
        [Tooltip("The full name of the active movement type.")]
        [SerializeField] protected string m_MovementTypeFullName;
        [Tooltip("The name of the active first person movement type.")]
        [SerializeField] protected string m_FirstPersonMovementTypeFullName;
        [Tooltip("The name of the active third person movement type.")]
        [SerializeField] protected string m_ThirdPersonMovementTypeFullName;
        [Tooltip("Specifies how much to multiply the yaw parameter by when turning in place.")]
        [SerializeField] protected float m_YawMultiplier = 7;
        [Tooltip("The serialization data for the MovementTypes.")]
        [SerializeField] protected Serialization[] m_MovementTypeData;
        [Tooltip("The serialization data for the Abilities.")]
        [SerializeField] protected Serialization[] m_AbilityData;
        [Tooltip("The serialization data for the Item Abilities.")]
        [SerializeField] protected Serialization[] m_ItemAbilityData;
        [Tooltip("The serialization data for the Effects.")]
        [SerializeField] protected Serialization[] m_EffectData;
        [Tooltip("Unity event invoked when an ability has been started or stopped.")]
        [SerializeField] protected UnityMovementTypeBoolEvent m_OnMovementTypeActiveEvent;
        [Tooltip("Unity event invoked when an movement type has been started or stopped.")]
        [SerializeField] protected UnityAbilityBoolEvent m_OnAbilityActiveEvent;
        [Tooltip("Unity event invoked when an item ability has been started or stopped.")]
        [SerializeField] protected UnityItemAbilityBoolEvent m_OnItemAbilityActiveEvent;
        [Tooltip("Unity event invoked when the character has changed grounded state.")]
        [SerializeField] protected UnityBoolEvent m_OnGroundedEvent;
        [Tooltip("Unity event invoked when the character has landed on the ground.")]
        [SerializeField] protected UnityFloatEvent m_OnLandEvent;
        [Tooltip("Unity event invoked when the time scale has changed.")]
        [SerializeField] protected UnityFloatEvent m_OnChangeTimeScaleEvent;
        [Tooltip("Unity event invoked when the moving platforms have changed.")]
        [SerializeField] protected UnityTransformEvent m_OnChangeMovingPlatformsEvent;

        public string FirstPersonStateName { get { return m_FirstPersonStateName; } set { m_FirstPersonStateName = value; } }
        public string ThirdPersonStateName { get { return m_ThirdPersonStateName; } set { m_ThirdPersonStateName = value; } }
        public string MovingStateName { get { return m_MovingStateName; } set { m_MovingStateName = value; } }
        public string AirborneStateName { get { return m_AirborneStateName; } set { m_AirborneStateName = value; } }
        public string MovementTypeFullName { get { return m_MovementTypeFullName; } set { SetMovementType(value); } }
        public string FirstPersonMovementTypeFullName
        {
            get { return m_FirstPersonMovementTypeFullName; }
            set
            {
                if (!string.IsNullOrEmpty(value) && m_FirstPersonMovementTypeFullName != value) {
                    if (Application.isPlaying && m_FirstPersonPerspective) {
                        SetMovementType(value);
                    } else {
                        m_FirstPersonMovementTypeFullName = value;
                    }
                }
            }
        }
        public string ThirdPersonMovementTypeFullName
        {
            get { return m_ThirdPersonMovementTypeFullName; }
            set
            {
                if (!string.IsNullOrEmpty(value) && m_ThirdPersonMovementTypeFullName != value) {
                    if (Application.isPlaying && !m_FirstPersonPerspective) {
                        SetMovementType(value);
                    } else {
                        m_ThirdPersonMovementTypeFullName = value;
                    }
                }
            }
        }
        public float YawMultiplier { get { return m_YawMultiplier; } set { m_YawMultiplier = value; } }
        public float YawAngle { get { return m_YawAngle; } }
        
        public UnityMovementTypeBoolEvent OnMovementTypeActiveEvent { get { return m_OnMovementTypeActiveEvent; } set { m_OnMovementTypeActiveEvent = value; } }
        public UnityAbilityBoolEvent OnAbilityActiveEvent { get { return m_OnAbilityActiveEvent; } set { m_OnAbilityActiveEvent = value; } }
        public UnityItemAbilityBoolEvent OnItemAbilityActiveEvent { get { return m_OnItemAbilityActiveEvent; } set { m_OnItemAbilityActiveEvent = value; } }
        public UnityBoolEvent OnGroundedEvent { get { return m_OnGroundedEvent; } set { m_OnGroundedEvent = value; } }
        public UnityFloatEvent OnLandEvent { get { return m_OnLandEvent; } set { m_OnLandEvent = value; } }
        public UnityFloatEvent OnChangeTimeScaleEvent { get { return m_OnChangeTimeScaleEvent; } set { m_OnChangeTimeScaleEvent = value; } }
        public UnityTransformEvent OnChangeMovingPlatformsEvent { get { return m_OnChangeMovingPlatformsEvent; } set { m_OnChangeMovingPlatformsEvent = value; } }

        private GameObject m_GameObject;
        private Animator m_Animator;
        private int m_KinematicObjectIndex = -1;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private INetworkInfo m_NetworkInfo;
        private INetworkCharacter m_NetworkCharacter;
#endif

        private float m_MaxHeight;
        private Vector3 m_MaxHeightPosition;
        private float m_YawAngle;
        private float m_PrevPlatformYawOffset;
        private Vector2 m_RawInputVector;
        private bool m_Moving;
        private bool m_MovingParameter;
        private ILookSource m_LookSource;
        private MovementType[] m_MovementTypes;
        private Ability[] m_Abilities;
        private Ability[] m_ActiveAbilities;
        private int m_ActiveAbilityCount;
        private bool m_DirtyAbilityParameter;
        private MoveTowards m_MoveTowardsAbility;
        private ItemEquipVerifier m_ItemEquipVerifierAbility;
        private ItemAbility[] m_ItemAbilities;
        private ItemAbility[] m_ActiveItemAbilities;
        private int m_ActiveItemAbilityCount;
        private bool m_DirtyItemAbilityParameter;
        private Effect[] m_Effects;
        private Effect[] m_ActiveEffects;
        private int m_ActiveEffectCount;
        private Dictionary<string, int> m_MovementTypeNameMap = new Dictionary<string, int>();
        private MovementType m_MovementType;
        private bool m_FirstPersonPerspective;
        private bool m_Alive;
        private bool m_Aiming;
        private Vector3 m_AbilityMotor;

        private int[] m_ItemSlotStateIndex;
        private int[] m_ItemSlotSubstateIndex;

        [NonSerialized] public int KinematicObjectIndex { get { return m_KinematicObjectIndex; } set { m_KinematicObjectIndex = value; } }
        public ILookSource LookSource { get { return m_LookSource; } }
        public MovementType[] MovementTypes { get { return m_MovementTypes; }
            set
            {
                m_MovementTypes = value;
                m_MovementTypeNameMap.Clear();
                for (int i = 0; i < m_MovementTypes.Length; ++i) {
                    m_MovementTypeNameMap.Add(m_MovementTypes[i].GetType().FullName, i);
                }
            }
        }
        public Serialization[] MovementTypeData { get { return m_MovementTypeData; } set { m_MovementTypeData = value; } }
        public Ability[] Abilities { get { return m_Abilities; } set { m_Abilities = value;
                if (Application.isPlaying && m_Abilities != null) {
                    if (m_ActiveAbilities == null) {
                        m_ActiveAbilities = new Ability[m_Abilities.Length];
                    } else {
                        System.Array.Resize(ref m_ActiveAbilities, m_Abilities.Length);
                    }

                    // The ability can be added after the character has already been initialized.
                    for (int i = 0; i < m_Abilities.Length; ++i) {
                        if (m_Abilities[i].Index == -1) {
                            m_Abilities[i].Initialize(this, i);
                            m_Abilities[i].Awake();
                        }
                    }
                }
            }
        }
        public Serialization[] AbilityData { get { return m_AbilityData; } set { m_AbilityData = value; } }
        public ItemAbility[] ItemAbilities { get { return m_ItemAbilities; } set { m_ItemAbilities = value;
                if (Application.isPlaying && m_ItemAbilities != null) {
                    if (m_ActiveItemAbilities == null) {
                        m_ActiveItemAbilities = new ItemAbility[m_ItemAbilities.Length];
                    } else {
                        System.Array.Resize(ref m_ActiveItemAbilities, m_ItemAbilities.Length);
                    }

                    // The ability can be added after the character has already been initialized.
                    for (int i = 0; i < m_ItemAbilities.Length; ++i) {
                        if (m_ItemAbilities[i].Index == -1) {
                            m_ItemAbilities[i].Initialize(this, i);
                            m_ItemAbilities[i].Awake();
                        }
                    }
                }
            }
        }
        public Serialization[] ItemAbilityData { get { return m_ItemAbilityData; } set { m_ItemAbilityData = value; } }
        [Snapshot] public Ability[] ActiveAbilities { get { return m_ActiveAbilities; } protected set { m_ActiveAbilities = value; } }
        [Snapshot] public int ActiveAbilityCount { get { return m_ActiveAbilityCount; } protected set { m_ActiveAbilityCount = value; } }
        [Snapshot] public bool DirtyAbilityParameter { get { return m_DirtyAbilityParameter; } protected set { m_DirtyAbilityParameter = value; } }
        [Snapshot] public ItemAbility[] ActiveItemAbilities { get { return m_ActiveItemAbilities; } protected set { m_ActiveItemAbilities = value; } }
        [Snapshot] public int ActiveItemAbilityCount { get { return m_ActiveItemAbilityCount; } protected set { m_ActiveItemAbilityCount = value; } }
        [Snapshot] public bool DirtyItemAbilityParameter { get { return m_DirtyItemAbilityParameter; } protected set { m_DirtyItemAbilityParameter = value; } }
        public Effect[] Effects { get { return m_Effects; } set { m_Effects = value; } }
        public Serialization[] EffectData { get { return m_EffectData; } set { m_EffectData = value; } }
        public MovementType ActiveMovementType { get { return m_MovementType; } }
        public MoveTowards MoveTowardsAbility { get { return m_MoveTowardsAbility; } }
        public ItemEquipVerifier ItemEquipVerifierAbility { get { return m_ItemEquipVerifierAbility; } }
        public override float TimeScale
        {
            get { return base.TimeScale; }
            set
            {
                // Override the TimeScale setter to allow an event to be sent when the time scale changes.
                if (base.TimeScale != value) {
                    if (base.TimeScale == 0 && value != 0) {
                        EventHandler.ExecuteEvent(m_GameObject, "OnEnableGameplayInput", true);
                    } else if (base.TimeScale != 0 && value == 0) {
                        EventHandler.ExecuteEvent(m_GameObject, "OnEnableGameplayInput", false);
                    }
                    EventHandler.ExecuteEvent(m_GameObject, "OnCharacterChangeTimeScale", value);
                    if (m_OnChangeTimeScaleEvent != null) {
                        m_OnChangeTimeScaleEvent.Invoke(value);
                    }
                }
                base.TimeScale = value;
            }
        }

        [NonSerialized] public Vector2 RawInputVector { get { return m_RawInputVector; } set { m_RawInputVector = value; } }
        [NonSerialized] public Vector2 InputVector { get { return m_InputVector; } set { m_InputVector = value; } }
        [NonSerialized] public Vector3 DeltaRotation { get { return m_DeltaRotation; } set { m_DeltaRotation = value; } }
        [NonSerialized] public bool Moving { get { return m_Moving || m_InputVector.sqrMagnitude > 0.001f; } set {
                if (m_Moving != value) {
                    m_Moving = value;
                    EventHandler.ExecuteEvent(m_GameObject, "OnCharacterMoving", m_Moving);
                    if (!string.IsNullOrEmpty(m_MovingStateName)) {
                        StateManager.SetState(m_GameObject, m_MovingStateName, m_Moving);
                    }
                }
            }
        }
        [NonSerialized] public Vector3 AbilityMotor { get { return m_AbilityMotor; } set { m_AbilityMotor = value; } }
        [NonSerialized] public bool FirstPersonPerspective { get { return m_FirstPersonPerspective; } set { m_FirstPersonPerspective = value; } }
        public bool Alive { get { return m_Alive; } }
        public float DeltaTime { get { return m_DeltaTime; } }
        public float FramerateDeltaTime { get { return m_FramerateDeltaTime; } }

        /// <summary>
        /// Cache the component references and initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            m_GameObject = gameObject;
            m_Animator = m_GameObject.GetCachedComponent<Animator>();
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            m_NetworkInfo = m_GameObject.GetCachedComponent<INetworkInfo>();
            m_NetworkCharacter = m_GameObject.GetCachedComponent<INetworkCharacter>();
#endif

            // Create any movement types, abilities, and effects from the serialized data.
            DeserializeMovementTypes();
            DeserializeAbilities();
            DeserializeItemAbilities();
            DeserializeEffects();

            base.Awake();

            // Use the local y position in determining the max height.
            m_MaxHeight = float.NegativeInfinity;
            m_MaxHeightPosition = m_Transform.position;
            var forwardDirection = m_Transform.rotation * m_Transform.forward;
            m_YawAngle = Mathf.Atan2(forwardDirection.x, forwardDirection.z) * Mathf.Rad2Deg;
            m_Alive = true;

            EventHandler.RegisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterChangePerspectives", OnChangePerspectives);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnAimAbilityAim", OnAiming);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_GameObject, "OnRespawn", OnRespawn);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorSnapped", AnimatorSnapped);

            // Call Awake on all of the deserialized objects after the character controller's Awake method is complete.
            if (m_MovementTypes != null) {
                for (int i = 0; i < m_MovementTypes.Length; ++i) {
                    m_MovementTypes[i].Awake();
                }
            }
            if (m_Abilities != null) {
                m_ActiveAbilities = new Ability[m_Abilities.Length];
                for (int i = 0; i < m_Abilities.Length; ++i) {
                    m_Abilities[i].Awake();
                }
            }
            if (m_ItemAbilities != null) {
                m_ActiveItemAbilities = new ItemAbility[m_ItemAbilities.Length];
                for (int i = 0; i < m_ItemAbilities.Length; ++i) {
                    m_ItemAbilities[i].Awake();
                }
            }
            if (m_Effects != null) {
                m_ActiveEffects = new Effect[m_Effects.Length];
                for (int i = 0; i < m_Effects.Length; ++i) {
                    m_Effects[i].Awake();
                }
            }
            // The controller needs to start with a movement type.
            SetMovementType(m_MovementTypeFullName);
            if (m_MovementType != null) {
                m_FirstPersonPerspective = m_MovementType.FirstPersonPerspective;
            }
        }

        /// <summary>
        /// Deserialize the movement types.
        /// </summary>
        /// <returns>Were any movement types removed?</returns>
        public bool DeserializeMovementTypes()
        {
            return DeserializeMovementTypes(false);
        }

        /// <summary>
        /// Deserialize the movement types.
        /// </summary>
        /// <param name="forceDeserialization">Should the movement types be force deserialized?</param>
        /// <returns>Were any movement types removed?</returns>
        public bool DeserializeMovementTypes(bool forceDeserialization)
        {
            // The Movement Types only need to be deserialized once.
            if (m_MovementTypes != null && !forceDeserialization) {
                return false;
            }

            var dirty = false;
            if (m_MovementTypeData != null && m_MovementTypeData.Length > 0) {
                m_MovementTypes = new MovementType[m_MovementTypeData.Length];
                m_MovementTypeNameMap.Clear();
                for (int i = 0; i < m_MovementTypes.Length; ++i) {
                    m_MovementTypes[i] = m_MovementTypeData[i].DeserializeFields(MemberVisibility.Public) as MovementType;
                    if (m_MovementTypes[i] == null) {
                        dirty = true;
                        continue;
                    }
                    m_MovementTypeNameMap.Add(m_MovementTypes[i].GetType().FullName, i);
                    if (Application.isPlaying) {
                        m_MovementTypes[i].Initialize(this);
                    }
                }
            }
            return dirty;
        }

        /// <summary>
        /// Deserialize the abilities.
        /// </summary>
        /// <returns>Were any abilities removed?</returns>
        public bool DeserializeAbilities()
        {
            return DeserializeAbilities(false);
        }

        /// <summary>
        /// Deserialize the abilities.
        /// </summary>
        /// <param name="forceDeserialization">Should the abilities be force deserialized?</param>
        /// <returns>Were any abilities removed?</returns>
        public bool DeserializeAbilities(bool forceDeserialization)
        {
            // The abilities only need to be deserialized once.
            if (m_Abilities != null && !forceDeserialization) {
                return false;
            }

            var dirty = false;
            if (m_AbilityData != null && m_AbilityData.Length > 0) {
                m_Abilities = new Ability[m_AbilityData.Length];
                for (int i = 0; i < m_AbilityData.Length; ++i) {
                    m_Abilities[i] = m_AbilityData[i].DeserializeFields(MemberVisibility.Public) as Ability;
                    if (m_Abilities[i] == null) {
                        dirty = true;
                        continue;
                    }
                    if (Application.isPlaying) {
                        m_Abilities[i].Initialize(this, i);
                    }
                    // The MoveTowards and ItemToggleAbilityBlock abilities are a special type of ability in that it is started by the controller.
                    if (m_Abilities[i] is MoveTowards) {
                        m_MoveTowardsAbility = m_Abilities[i] as MoveTowards;
                    } else if (m_Abilities[i] is ItemEquipVerifier) {
                        m_ItemEquipVerifierAbility = m_Abilities[i] as ItemEquipVerifier;
                    }
                }
            }
            return dirty;
        }

        /// <summary>
        /// Deserialize the item abilities.
        /// </summary>
        /// <returns>Were any item abilities removed?</returns>
        public bool DeserializeItemAbilities()
        {
            return DeserializeItemAbilities(false);
        }

        /// <summary>
        /// Deserialize the item abilities.
        /// </summary>
        /// <param name="forceDeserialization">Should the item abilities be force deserialized?</param>
        /// <returns>Were any item abilities removed?</returns>
        public bool DeserializeItemAbilities(bool forceDeserialization)
        {
            // The Item Abilities only need to be deserialized once.
            if (m_ItemAbilities != null && !forceDeserialization) {
                return false;
            }

            var dirty = false;
            if (m_ItemAbilityData != null && m_ItemAbilityData.Length > 0) {
                m_ItemAbilities = new ItemAbility[m_ItemAbilityData.Length];
                for (int i = 0; i < m_ItemAbilityData.Length; ++i) {
                    m_ItemAbilities[i] = m_ItemAbilityData[i].DeserializeFields(MemberVisibility.Public) as ItemAbility;
                    if (m_ItemAbilities[i] == null) {
                        dirty = true;
                        continue;
                    }
                    if (Application.isPlaying) {
                        m_ItemAbilities[i].Initialize(this, i);
                    }
                }
            }
            return dirty;
        }

        /// <summary>
        /// Deserialize the effects.
        /// </summary>
        /// <returns>Were any effects removed?</returns>
        public bool DeserializeEffects()
        {
            return DeserializeEffects(false);
        }

        /// <summary>
        /// Deserialize the effects.
        /// </summary>
        /// <param name="forceDeserialization">Should the effects be force deserialized?</param>
        /// <returns>Were any effects removed?</returns>
        public bool DeserializeEffects(bool forceDeserialization)
        {
            // The Effects only need to be deserialized once.
            if (m_Effects != null && !forceDeserialization) {
                return false;
            }

            var dirty = false;
            if (m_EffectData != null && m_EffectData.Length > 0) {
                m_Effects = new Effect[m_EffectData.Length];
                for (int i = 0; i < m_EffectData.Length; ++i) {
                    m_Effects[i] = m_EffectData[i].DeserializeFields(MemberVisibility.Public) as Effect;
                    if (m_Effects[i] == null) {
                        dirty = true;
                        continue;
                    }
                    if (Application.isPlaying) {
                        m_Effects[i].Initialize(this, i);
                    }
                }
            }
            return dirty;
        }

        /// <summary>
        /// Returns an array of serialized movement types. Useful for editor scripts when the movement types haven't already been deserialized at runtime.
        /// </summary>
        /// <returns>An array of serialized movement types.</returns>
        public MovementType[] GetSerializedMovementTypes()
        {
            if (m_MovementTypeData != null && m_MovementTypeData.Length > 0 && (m_MovementTypes == null || m_MovementTypes.Length == 0)) { DeserializeMovementTypes(); }
            return m_MovementTypes;
        }

        /// <summary>
        /// Returns an array of serialized abilities. Useful for editor scripts when the abilities haven't already been deserialized at runtime.
        /// </summary>
        /// <returns>An array of serialized abilities.</returns>
        public Ability[] GetSerializedAbilities()
        {
            if (m_AbilityData != null && m_AbilityData.Length > 0 && (m_Abilities == null || m_Abilities.Length == 0)) { DeserializeAbilities(); }
            return m_Abilities;
        }

        /// <summary>
        /// Returns an array of serialized item abilities. Useful for editor scripts when the item abilities haven't already been deserialized at runtime.
        /// </summary>
        /// <returns>An array of serialized item abilities.</returns>
        public ItemAbility[] GetSerializedItemAbilities()
        {
            if (m_ItemAbilityData != null && m_ItemAbilityData.Length > 0 && (m_ItemAbilities == null || m_ItemAbilities.Length == 0)) { DeserializeItemAbilities(); }
            return m_ItemAbilities;
        }

        /// <summary>
        /// Returns an array of serialized effects. Useful for editor scripts when the effects haven't already been deserialized at runtime.
        /// </summary>
        /// <returns>An array of serialized effects.</returns>
        public Effect[] GetSerializedEffects()
        {
            if (m_EffectData != null && m_EffectData.Length > 0 && (m_Effects == null || m_Effects.Length == 0)) { DeserializeEffects(); }
            return m_Effects;
        }

        /// <summary>
        /// Sets the movement type to the object with the specified type which should be set.
        /// </summary>
        /// <param name="typeName">The type name of the MovementType which should be set.</param>
        private void SetMovementType(string typeName)
        {
            SetMovementType(UnityEngineUtility.GetType(typeName));
        }

        /// <summary>
        /// Sets the movement type to the object with the specified type.
        /// </summary>
        /// <param name="typeName">The type of the MovementType which should be set.</param>
        public void SetMovementType(System.Type type)
        {
            if (type == null || (m_MovementType != null && m_MovementType.GetType() == type)) {
                return;
            }

            // The MovementTypes may not be deserialized yet.
            if (m_MovementTypeNameMap.Count == 0) {
                DeserializeMovementTypes();
            }

            int index;
            if (!m_MovementTypeNameMap.TryGetValue(type.FullName, out index)) {
                Debug.LogError("Error: Unable to find the movement type with name " + type.FullName);
                return;
            }

            // Notify the previous movement type that it is no longer active.
            if (m_MovementType != null && Application.isPlaying) {
                m_MovementType.ChangeMovementType(false);

                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterChangeMovementType", m_MovementType, false);

                if (m_OnMovementTypeActiveEvent != null) {
                    m_OnMovementTypeActiveEvent.Invoke(m_MovementType, false);
                }
            }

            m_MovementTypeFullName = type.FullName;
            m_MovementType = m_MovementTypes[index];

            // Notify the current movement type that is now active.
            if (Application.isPlaying) {
                if (m_MovementType.FirstPersonPerspective) {
                    m_FirstPersonMovementTypeFullName = m_MovementTypeFullName;
                } else {
                    m_ThirdPersonMovementTypeFullName = m_MovementTypeFullName;
                }
                m_MovementType.ChangeMovementType(true);

                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterChangeMovementType", m_MovementType, true);

                if (m_OnMovementTypeActiveEvent != null) {
                    m_OnMovementTypeActiveEvent.Invoke(m_MovementType, true);
                }
            }
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            m_LookSource = lookSource;

#if THIRD_PERSON_CONTROLLER
            var hasPerspectiveMonitor = m_GameObject.GetComponent<ThirdPersonController.Character.PerspectiveMonitor>() != null;
#else
            var hasPerspectiveMonitor = false;
#endif
            // If the character doesn't have the PerspectiveMonitor then the perspective depends on the look source.
            if (!hasPerspectiveMonitor) {
                if (lookSource != null) {
                    var cameraController = lookSource as Camera.CameraController;
                    if (cameraController != null) {
                        m_FirstPersonPerspective = cameraController.ActiveViewType.FirstPersonPerspective;
                    } else {
                        m_FirstPersonPerspective = false;
                    }
                }
                EventHandler.ExecuteEvent<bool>(m_GameObject, "OnCharacterChangePerspectives", m_FirstPersonPerspective);
            }
        }

        /// <summary>
        /// The character has been enabled.
        /// </summary>
        protected override void OnEnable()
        {
            // The KinematicObjectManager is responsible for calling the move method.
            m_KinematicObjectIndex = KinematicObjectManager.RegisterCharacter(this);

            // If the previous index is not -1 then the character has already been enabled. Send events so all of the components correctly reset.
            if (m_KinematicObjectIndex != -1) {
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterMoving", false);
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterImmediateTransformChange", true);
            }

            base.OnEnable();
        }

        /// <summary>
        /// Call Start on all of the character's abilities and effects in addition to checking the TimeScale.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // The animator needs to know how many slots there are.
            var inventory = m_GameObject.GetCachedComponent<Inventory.InventoryBase>();
            if (inventory != null) {
                var slotCount = inventory.SlotCount;
                m_ItemSlotStateIndex = new int[slotCount];
                m_ItemSlotSubstateIndex = new int[slotCount];
            }

            // Start the abilities and effects.
            if (m_Abilities != null) {
                for (int i = 0; i < m_Abilities.Length; ++i) {
                    m_Abilities[i].Start();
                }
            }
            if (m_ItemAbilities != null) {
                for (int i = 0; i < m_ItemAbilities.Length; ++i) {
                    m_ItemAbilities[i].Start();
                }
            }
            if (m_Effects != null) {
                for (int i = 0; i < m_Effects.Length; ++i) {
                    m_Effects[i].Start();
                }
            }

            // Do a pass on trying to start any abilities in case they should be started on the first frame.
            UpdateAbilities(m_Abilities);
            UpdateAbilities(m_ItemAbilities);
            UpdateDirtyAbilityAnimatorParameters();
            
            // The character isn't moving at the start.
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterMoving", false);
            // Notify those interested in the time scale isn't set to 1 at the start. The TimeScale property will notify those interested of the change during runtime.
            if (m_TimeScale != 1) {
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterChangeTimeScale", m_TimeScale);
                if (m_OnChangeTimeScaleEvent != null) {
                    m_OnChangeTimeScaleEvent.Invoke(m_TimeScale);
                }
            }
        }

        /// <summary>
        /// Callback from CharacterLocomotion.Move when the UltimateCharacterLocomotion should perform its updates (such as updating the abilities).
        /// </summary>
        protected override void UpdateUltimateLocomotion()
        {
            // The MovementType may change the InputVector.
            m_RawInputVector = m_InputVector;

            // Abilities may disallow input.
            bool allowPositionalInput, allowRotationalInput;
            AbilitiesAllowInput(out allowPositionalInput, out allowRotationalInput);
            if (allowPositionalInput) {
                // Positional input is allowed - use the movement type to determine how the character should move.
                m_InputVector = m_MovementType.GetInputVector(m_InputVector);
            } else {
                m_InputVector = Vector2.zero;
            }
            if (!allowRotationalInput) {
                m_DeltaRotation = Vector3.zero;
            }

            // Start and update the abilities.
            UpdateAbilities(m_Abilities);
            UpdateAbilities(m_ItemAbilities);

            // Update the effects.
            for (int i = 0; i < m_ActiveEffectCount; ++i) {
                m_ActiveEffects[i].Update();
            }

            // The Moving value is based on the input vector. It does not include vertical movement.
            m_MovingParameter = Moving;
            if (m_Moving != (m_InputVector.sqrMagnitude > 0.001f)) {
                Moving = !m_Moving;
            }
        }

        /// <summary>
        /// Do the abilities allow positional and rotational input?
        /// </summary>
        /// <param name="allowPositionalInput">A reference to a bool which indicates if the abilities allow positional input.</param>
        /// <param name="allowRotationalInput">A reference to a bool which indicates if the abilities allow rotational input.</param>
        private void AbilitiesAllowInput(out bool allowPositionalInput, out bool allowRotationalInput)
        {
            allowPositionalInput = allowRotationalInput = true;
            // Check the abilities to see if any disallow input.
            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                if (!m_ActiveAbilities[i].AllowPositionalInput) {
                    allowPositionalInput = false;
                }
                if (!m_ActiveAbilities[i].AllowRotationalInput) {
                    allowRotationalInput = false;
                }
            }

            // If neither the positional or rotational input is allowed then the item abilities don't need to be checked.
            if (!allowPositionalInput && !allowRotationalInput) {
                return;
            }

            // Check the item abilities to see if any disallow input.
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                if (!m_ActiveItemAbilities[i].AllowPositionalInput) {
                    allowPositionalInput = false;
                }
                if (!m_ActiveItemAbilities[i].AllowRotationalInput) {
                    allowRotationalInput = false;
                }
            }
        }

        /// <summary>
        /// Try to start an automatic inactive ability and also try to stop an automatic active ability. The Update or InactiveUpdate will also be called.
        /// </summary>
        /// <param name="abilities">An array of all of the abilities.</param>
        private void UpdateAbilities(Ability[] abilities)
        {
            if (abilities != null) {
                for (int i = 0; i < abilities.Length; ++i) {
                    if (!abilities[i].IsActive) {
                        if (abilities[i].StartType == Ability.AbilityStartType.Automatic) {
                            TryStartAbility(abilities[i]);
                        } else if (!(abilities[i] is ItemAbility) && abilities[i].StartType != Ability.AbilityStartType.Manual && abilities[i].CheckForAbilityMessage) {
                            // The ability message can show if the non-automatic/manual ability can start.
                            abilities[i].AbilityMessageCanStart = abilities[i].Enabled && abilities[i].CanStartAbility();
                        }
                    }
                    if (abilities[i].IsActive && abilities[i].StopType == Ability.AbilityStopType.Automatic) {
                        TryStopAbility(abilities[i]);
                    }
                    if (abilities[i].IsActive) {
                        abilities[i].Update();
                    } else if (abilities[i].Enabled) {
                        abilities[i].InactiveUpdate();
                    }
                }
            }
        }

        /// <summary>
        /// Update the animator parameters.
        /// </summary>
        protected override void UpdateAnimator()
        {
            // In the case of a first person view the character may not have an animator.
            if (m_AnimatorMonitor == null) {
                return;
            }

            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                m_ActiveAbilities[i].UpdateAnimator();
            }
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                m_ActiveItemAbilities[i].UpdateAnimator();
            }

            m_AnimatorMonitor.SetHorizontalMovementParameter(m_InputVector.x, m_TimeScale);
            m_AnimatorMonitor.SetForwardMovementParameter(m_InputVector.y, m_TimeScale);
            if (m_LookSource != null) {
                m_AnimatorMonitor.SetPitchParameter(m_LookSource.Pitch, m_TimeScale, 0);
            }
            m_AnimatorMonitor.SetYawParameter(m_YawAngle * m_YawMultiplier, m_TimeScale);
            m_AnimatorMonitor.SetMovingParameter(m_MovingParameter);

            // The ability parameters should only be updated once each move call.
            UpdateDirtyAbilityAnimatorParameters();
            m_AnimatorMonitor.UpdateItemIDParameters();
        }

        /// <summary>
        /// Updates the position and rotation. This should be done after the animator has updated so the root motion is accurate for the current frame.
        /// </summary>
        protected override void UpdatePositionAndRotation()
        {
            KinematicObjectManager.BeginCharacterMovement(m_KinematicObjectIndex);

            base.UpdatePositionAndRotation();
            LateUpdateUltimateLocomotion();

            KinematicObjectManager.EndCharacterMovement(m_KinematicObjectIndex);
        }

        /// <summary>
        /// Updates the grounded state.
        /// </summary>
        /// <param name="grounded">Is the character grounded?</param>
        /// <param name="eventUpdate">Should the events be sent if the grounded status changes?</param>
        /// <returns>True if the grounded state changed.</returns>
        protected override bool UpdateGroundState(bool grounded, bool sendEvents)
        {
            var groundedStatusChanged = base.UpdateGroundState(grounded, sendEvents);
            if (groundedStatusChanged) {
                // Notify interested objects of the ground change.
                if (sendEvents) {
                    EventHandler.ExecuteEvent<bool>(m_GameObject, "OnCharacterGrounded", m_Grounded);
                    if (m_OnGroundedEvent != null) {
                        m_OnGroundedEvent.Invoke(m_Grounded);
                    }
                }
                if (m_Grounded) {
                    if (sendEvents && m_MaxHeight != float.NegativeInfinity) {
                        var height = m_MaxHeight - m_Transform.InverseTransformDirection(m_Transform.position - m_MaxHeightPosition).y;
                        EventHandler.ExecuteEvent<float>(m_GameObject, "OnCharacterLand", height);
                        if (m_OnLandEvent != null) {
                            m_OnLandEvent.Invoke(height);
                        }
                    }
                } else {
                    m_MaxHeightPosition = m_Transform.position;
                    m_MaxHeight = float.NegativeInfinity;
                }
            } else if (!m_Grounded && UsingGravity) {
                // Save out the max height of the character in the air so the fall height can be calculated.
                var height = m_Transform.InverseTransformDirection(m_Transform.position - m_MaxHeightPosition).y;
                if (height > m_MaxHeight) {
                    m_MaxHeightPosition = m_Transform.position;
                    m_MaxHeight = height;
                }
            }
            // Set the airborne state if the grounded status has changed or no events are being sent. No events will be sent when the grounded status is initially checked.
            if ((groundedStatusChanged || !sendEvents) && !string.IsNullOrEmpty(m_AirborneStateName)) {
                StateManager.SetState(m_GameObject, m_AirborneStateName, !m_Grounded);
            }
            return groundedStatusChanged;
        }

        /// <summary>
        /// Update the rotation forces.
        /// </summary>
        protected override void UpdateRotation()
        {
            // If using root motion rotation the animation will specify the rotation.
            if (UsingRootMotionRotation) {
                m_DeltaRotation = Vector3.zero;
            }

            // Give the abilities a chance to update the rotation.
            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                m_ActiveAbilities[i].UpdateRotation();
            }
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                m_ActiveItemAbilities[i].UpdateRotation();
            }

            base.UpdateRotation();
        }

        /// <summary>
        /// Applies the final rotation to the transform.
        /// </summary>
        protected override void ApplyRotation()
        {
            // Give the abilities a chance to verify the rotation.
            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                m_ActiveAbilities[i].ApplyRotation();
            }
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                m_ActiveItemAbilities[i].ApplyRotation();
            }

            // Save the local yaw angle before the rotation is applied so the animator knows how much the character turned.
            m_YawAngle = 0;
            if (m_Platform == null) {
                if (Mathf.Abs(m_Torque.eulerAngles.y) > 0.1f) {
                    m_YawAngle = MathUtility.ClampInnerAngle(m_Torque.eulerAngles.y);
                }
            } else {
                var platformYawOffset = MathUtility.InverseTransformQuaternion(m_Transform.rotation, m_Platform.rotation).eulerAngles.y;
                if (Mathf.Abs(platformYawOffset - m_PrevPlatformYawOffset) > 0.1f) {
                    m_YawAngle = MathUtility.ClampInnerAngle(platformYawOffset - m_PrevPlatformYawOffset);
                }
                m_PrevPlatformYawOffset = platformYawOffset;
            }

            base.ApplyRotation();
        }

        /// <summary>
        /// When the character changes grounded state the moving platform should also be updated. This
        /// allows the character to always reference the correct moving platform (if one exists at all).
        /// </summary>
        /// <param name="hitTransform">The name of the possible moving platform transform.</param>
        /// <param name="groundUpdate">Is the moving platform update being called from a grounded check?</param>
        /// <returns>True if the platform changed.</returns>
        protected override bool UpdateMovingPlatformTransform(Transform hitTransform, bool groundUpdate)
        {
            var movingPlatformChanged = base.UpdateMovingPlatformTransform(hitTransform, groundUpdate);
            if (movingPlatformChanged) {
                if (m_Platform != null) {
                    m_PrevPlatformYawOffset = MathUtility.InverseTransformQuaternion(m_Transform.rotation, m_Platform.rotation).eulerAngles.y;
                }
                // Notify interested objects of the platform change.
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterChangeMovingPlatforms", m_Platform);
                if (m_OnChangeMovingPlatformsEvent != null) {
                    m_OnChangeMovingPlatformsEvent.Invoke(m_Platform);
                }
            }
            return movingPlatformChanged;
        }

        /// <summary>
        /// Move according to the forces.
        /// </summary>
        protected override void UpdatePosition()
        {
            // Give the abilities a chance to update the movement.
            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                m_ActiveAbilities[i].UpdatePosition();
            }
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                m_ActiveItemAbilities[i].UpdatePosition();
            }

            base.UpdatePosition();
        }

        /// <summary>
        /// Updates the motor forces.
        /// </summary>
        protected override void UpdateMotorThrottle()
        {
            base.UpdateMotorThrottle();

            MotorThrottle += m_AbilityMotor;
        }

        /// <summary>
        /// Applies the final move direction to the transform.
        /// </summary>
        protected override void ApplyPosition()
        {
            // Give the abilities a chance to verify the movement.
            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                m_ActiveAbilities[i].ApplyPosition();
            }
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                m_ActiveItemAbilities[i].ApplyPosition();
            }

            base.ApplyPosition();
        }

        /// <summary>
        /// Updates the ability and item ability parameters if they are dirty.
        /// </summary>
        private void UpdateDirtyAbilityAnimatorParameters()
        {
            if (m_DirtyAbilityParameter) {
                UpdateAbilityAnimatorParameters(true);
            }
            if (m_DirtyItemAbilityParameter) {
                UpdateItemAbilityAnimatorParameters(true);
            }
        }

        /// <summary>
        /// Callback after the animator has updated and the character should perform its post movement updates (such as updating the item abilities).
        /// </summary>
        private void LateUpdateUltimateLocomotion()
        {
            // If the animator is being updated within LateUpdate then the character colliders will be enabled.
            var collisionLayerEnabled = CollisionLayerEnabled;
            EnableColliderCollisionLayer(false);

            // Start and update the item abilities. This is done after the controller has moved so the items will be using the latest character position/rotation (such as a melee item
            // for collision detection).
            LateUpdateActiveAbilities(m_ActiveAbilities, ref m_ActiveAbilityCount);
            LateUpdateActiveAbilities(m_ActiveItemAbilities, ref m_ActiveItemAbilityCount);

            EnableColliderCollisionLayer(collisionLayerEnabled);
        }

        /// <summary>
        /// Calls LateUpdate on the active abilities.
        /// </summary>
        /// <param name="abilities">An array of all of the abilities.</param>
        /// <param name="abilityCount">The number of active abilities.</param>
        private void LateUpdateActiveAbilities(Ability[] abilities, ref int abilityCount)
        {
            if (abilities != null) {
                for (int i = 0; i < abilityCount; ++i) {
                    abilities[i].LateUpdate();
                }
            }
        }

        /// <summary>
        /// Tries to start the specified ability.
        /// </summary>
        /// <param name="ability">The ability to try to start.</param>
        /// <returns>True if the ability was started.</returns>
        public bool TryStartAbility(Ability ability)
        {
            return TryStartAbility(ability, false);
        }

        /// <summary>
        /// Tries to start the specified ability.
        /// </summary>
        /// <param name="ability">The ability to try to start.</param>
        /// <param name="ignorePriority">Should the ability priority be ignored?</param>
        /// <returns>True if the ability was started.</returns>
        public bool TryStartAbility(Ability ability, bool ignorePriority)
        {
            return TryStartAbility(ability, ignorePriority, false);
        }

        /// <summary>
        /// Tries to start the specified ability.
        /// </summary>
        /// <param name="ability">The ability to try to start.</param>
        /// <param name="ignorePriority">Should the ability priority be ignored?</param>
        /// <param name="ignoreCanStartCheck">Should the CanStartAbility check be ignored?</param>
        /// <returns>True if the ability was started.</returns>
        public bool TryStartAbility(Ability ability, bool ignorePriority, bool ignoreCanStartCheck)
        {
            // ItemAbilities have a different startup process than regular abilities.
            if (ability is ItemAbility) {
                return TryStartAbility(ability as ItemAbility, ignoreCanStartCheck);
            }

            // Start the ability if it is not active or can be started multiple times, enabled, and can be started.
            if ((!ability.IsActive || ability.CanReceiveMultipleStarts) && ability.Enabled && (ignoreCanStartCheck || ability.CanStartAbility())) {
                // The ability may already be active if the ability can receive multiple starts. Multiple starts are useful for item abilities that need to be active
                // over a period of time but can be updated with the input start type while active. A good example of this is the Toggle Equip Item ability. When
                // this ability starts it sets an Animator parameter to equip or unequip the item. The ability continues to run while equipping or unequipping the item
                // but it should trigger the reverse of the equip or unequip when another start is triggered.
                int index;
                if (!ability.IsActive) {
                    // The priority can be ignored if the ability should be force started.
                    if (!ignorePriority) {
                        // If the ability is not a concurrent ability then it can only be started if it has a lower index than any other active abilities.
                        if (!ability.IsConcurrent) {
                            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                                var ignoreLocalPriority = m_ActiveAbilities[i].IgnorePriority && ability.IgnorePriority;
                                if (m_ActiveAbilities[i].IsConcurrent) {
                                    // The ability cannot be started if a concurrent ability is active and has a lower index.
                                    if (((!ignoreLocalPriority && m_ActiveAbilities[i].Index < ability.Index) || ignoreLocalPriority) && m_ActiveAbilities[i].ShouldBlockAbilityStart(ability)) {
                                        return false;
                                    }
                                } else {
                                    // The ability cannot be started if another ability is already active and has a lower index or if the active ability says the current ability cannot be started.
                                    if ((m_ActiveAbilities[i].Index < ability.Index && !ignoreLocalPriority) || m_ActiveAbilities[i].ShouldBlockAbilityStart(ability)) {
                                        return false;
                                    } else {
                                        // Stop any abilities that have a higher index to prevent two non-concurrent abilities from running at the same time.
                                        TryStopAbility(m_ActiveAbilities[i], true);
                                    }
                                }
                            }
                        }
                        // The ability cannot be started if the active ability says the current ability cannot be started.
                        for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                            if (m_ActiveAbilities[i].ShouldBlockAbilityStart(ability)) {
                                return false;
                            }
                        }
                        for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                            if (m_ActiveItemAbilities[i].ShouldBlockAbilityStart(ability)) {
                                return false;
                            }
                        }
                    }

                    // The ability can start. Stop any currently active abilities that should not be started because the current ability has started.
                    for (int i = m_ActiveAbilityCount - 1; i >= 0; --i) {
                        if (ability.ShouldStopActiveAbility(m_ActiveAbilities[i])) {
                            TryStopAbility(m_ActiveAbilities[i], true);
                        }
                    }
                    for (int i = m_ActiveItemAbilityCount - 1; i >= 0; --i) {
                        if (ability.ShouldStopActiveAbility(m_ActiveItemAbilities[i])) {
                            TryStopAbility(m_ActiveItemAbilities[i], true);
                        }
                    }

                    // Notify the ability that it will start before the Move Towards or Item Equip Verifier is started. ignoreCanStartCheck will only be
                    // true when the Move Towards or Item Equip Verifier abilities start the original ability.
                    if (!ignoreCanStartCheck && (m_MoveTowardsAbility == null || !m_MoveTowardsAbility.IsActive) &&
                                                (m_ItemEquipVerifierAbility == null || !m_ItemEquipVerifierAbility.IsActive)) {
                        if (!ability.AbilityWillStart()) {
                            return false;
                        }
                    }

                    // The ability may require the character to first move to a specific location before it can start.
                    if (!(ability is MoveTowards) && m_MoveTowardsAbility != null) {
                        // If StartMoving returns true then the MoveTowards ability has started and it will start the original
                        // ability after the character has arrived at the destination.
                        if (m_MoveTowardsAbility.StartMoving(ability.GetStartLocations(), ability)) {
                            return true;
                        }
                    }

                    // The ability may first need to unequip any equipped items before it can start.
                    if (!(ability is ItemEquipVerifier) && m_ItemEquipVerifierAbility != null) {
                        // If TryToggleItem returns true then the ItemEquipVerifier ability has started and it will start the original ability after
                        // the character has finished unequipping the equipped items.
                        if (m_ItemEquipVerifierAbility.TryToggleItem(ability, true) && !ability.ImmediateStartItemVerifier) {
                            return true;
                        }
                    }

                    // Insert in the active abilities array according to priority.
                    index = m_ActiveAbilityCount;
                    for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                        if (m_ActiveAbilities[i].Index > ability.Index) {
                            index = i;
                            break;
                        }
                    }
                    // Make space for the new ability.
                    for (int i = m_ActiveAbilityCount; i > index; --i) {
                        m_ActiveAbilities[i] = m_ActiveAbilities[i - 1];
                        m_ActiveAbilities[i].ActiveIndex = i;
                    }

                    m_ActiveAbilities[index] = ability;
                    m_ActiveAbilityCount++;
                } else {
                    // The ability is already active - start it again for a multiple start.
                    index = ability.ActiveIndex;
                }

                // Execute the event before the ability is started in case the ability is stopped within the start.
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterAbilityActive", ability, true);
                if (m_OnAbilityActiveEvent != null) {
                    m_OnAbilityActiveEvent.Invoke(ability, true);
                }

                ability.StartAbility(index);
                m_DirtyAbilityParameter = true;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to start the specified item ability.
        /// </summary>
        /// <param name="itemAbility">The item ability to try to start.</param>
        /// <returns>True if the ability was started.</returns>
        public bool TryStartAbility(ItemAbility itemAbility)
        {
            return TryStartAbility(itemAbility, false);
        }

        /// <summary>
        /// Tries to start the specified item ability.
        /// </summary>
        /// <param name="itemAbility">The item ability to try to start.</param>
        /// <param name="ignoreCanStartCheck">Should the CanStartAbility check be ignored?</param>
        /// <returns>True if the ability was started.</returns>
        public bool TryStartAbility(ItemAbility itemAbility, bool ignoreCanStartCheck)
        {
            // Start the ability if it is not active or can be started multiple times, enabled, and can be started.
            if ((!itemAbility.IsActive || itemAbility.CanReceiveMultipleStarts) && itemAbility.Enabled && (ignoreCanStartCheck || itemAbility.CanStartAbility())) {
                // The ability cannot be started if the active ability says the current ability cannot be started.
                for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                    if (m_ActiveItemAbilities[i].ShouldBlockAbilityStart(itemAbility)) {
                        return false;
                    }
                }
                for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                    if (m_ActiveAbilities[i].ShouldBlockAbilityStart(itemAbility)) {
                        return false;
                    }
                }

                // The ability can start. Stop any currently active abilities that should not be started because the current ability has started.
                for (int i = m_ActiveItemAbilityCount - 1; i >= 0; --i) {
                    if (itemAbility.ShouldStopActiveAbility(m_ActiveItemAbilities[i])) {
                        TryStopAbility(m_ActiveItemAbilities[i], true);
                    }
                }
                for (int i = m_ActiveAbilityCount - 1; i >= 0; --i) {
                    if (itemAbility.ShouldStopActiveAbility(m_ActiveAbilities[i])) {
                        TryStopAbility(m_ActiveAbilities[i], true);
                    }
                }

                // The ability may already be active if the ability can receive multiple starts. Multiple starts are useful for item abilities that need to be active
                // over a period of time but can be updated with the input start type while active. A good example of this is the Toggle Equip Item ability. When
                // this ability starts it sets an Animator parameter to equip or unequip the item. The ability continues to run while equipping or unequipping the item
                // but it should trigger the reverse of the equip or unequip when another start is triggered.
                int index;
                if (!itemAbility.IsActive) {
                    // Notify the ability that it will start. This method isn't as useful for ItemAbilities because the ability will always be immediately started after this,
                    // but it is added for consistency with the ability system. 
                    if (!itemAbility.AbilityWillStart()) {
                        return false;
                    }

                    // Insert in the active abilities array according to priority.
                    index = m_ActiveItemAbilityCount;
                    for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                        if (m_ActiveItemAbilities[i].Index > itemAbility.Index) {
                            index = i;
                            break;
                        }
                    }
                    // Make space for the new item ability.
                    for (int i = m_ActiveItemAbilityCount; i > index; --i) {
                        m_ActiveItemAbilities[i] = m_ActiveItemAbilities[i - 1];
                        m_ActiveItemAbilities[i].ActiveIndex = i;
                    }

                    m_ActiveItemAbilities[index] = itemAbility;
                    m_ActiveItemAbilityCount++;
                } else {
                    // The ability is already active - start it again for a multiple start.
                    index = itemAbility.ActiveIndex;
                }

                // Execute the event before the ability is started in case the ability is stopped within the start.
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterItemAbilityActive", itemAbility, true);
                if (m_OnItemAbilityActiveEvent != null) {
                    m_OnItemAbilityActiveEvent.Invoke(itemAbility, true);
                }

                itemAbility.StartAbility(index);
                m_DirtyItemAbilityParameter = true;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the ability animator parameters to the ability with the highest priority.
        /// </summary>
        public void UpdateAbilityAnimatorParameters()
        {
            UpdateAbilityAnimatorParameters(false);
        }

        /// <summary>
        /// Sets the ability animator parameters to the ability with the highest priority.
        /// </summary>
        /// <param name="immediateUpdate">Should the parameters be updated immediately?</param>
        public void UpdateAbilityAnimatorParameters(bool immediateUpdate)
        {
            if (m_AnimatorMonitor == null) {
                return;
            }

            // Wait to update until the proper time so the animator synchronizes properly.
            if (!immediateUpdate) {
                m_DirtyAbilityParameter = true;
                return;
            }
            m_DirtyAbilityParameter = false;

            int abilityIndex = 0, intData = 0;
            var floatData = 0f;
            bool concurrentAbilityIndex = false;
            bool setAbilityIndex = true, setStateIndex = true, setAbilityFloatData = true;
            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                if (setAbilityIndex && m_ActiveAbilities[i].AbilityIndexParameter != -1) {
                    abilityIndex = m_ActiveAbilities[i].AbilityIndexParameter;
                    concurrentAbilityIndex= m_ActiveAbilities[i].IsConcurrent;
                    setAbilityIndex = false;
                }
                if (setStateIndex && m_ActiveAbilities[i].AbilityIntData != -1) {
                    intData = m_ActiveAbilities[i].AbilityIntData;
                    setStateIndex = false;
                }
                if (setAbilityFloatData && m_ActiveAbilities[i].AbilityFloatData != -1) {
                    floatData = m_ActiveAbilities[i].AbilityFloatData;
                    setAbilityFloatData = false;
                }
            }
            // A negative ability index indicates that the move towards ability is active.
            if (m_MoveTowardsAbility != null && m_MoveTowardsAbility.IsActive && !concurrentAbilityIndex) {
                abilityIndex *= -1;
            }
            m_AnimatorMonitor.SetAbilityIndexParameter(abilityIndex);
            m_AnimatorMonitor.SetAbilityIntDataParameter(intData);
            m_AnimatorMonitor.SetAbilityFloatDataParameter(floatData, m_TimeScale);
        }

        /// <summary>
        /// Sets the ability animator parameters to the ability with the highest priority.
        /// </summary>
        public void UpdateItemAbilityAnimatorParameters()
        {
            UpdateItemAbilityAnimatorParameters(false);
        }

        /// <summary>
        /// Sets the item animator parameters to the item ability with the highest priority.
        /// </summary>
        /// <param name="immediateUpdate">Should the parameters be updated immediately?</param>
        public void UpdateItemAbilityAnimatorParameters(bool immediateUpdate)
        {
            if (m_AnimatorMonitor == null || m_ItemSlotStateIndex == null) {
                return;
            }

            // Wait to update until the proper time so the animator synchronizes properly.
            if (!immediateUpdate) {
                m_DirtyItemAbilityParameter = true;
                return;
            }
            m_DirtyItemAbilityParameter = false;

            // Reset the values.
            for (int i = 0; i < m_ItemSlotStateIndex.Length; ++i) {
                m_ItemSlotStateIndex[i] = -1;
                m_ItemSlotSubstateIndex[i] = -1;
            }

            // The value can only be assigned if it hasn't already been assigned.
            int value;
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                for (int j = 0; j < m_ItemSlotSubstateIndex.Length; ++j) {
                    if (m_ItemSlotStateIndex[j] == -1 && (value = m_ActiveItemAbilities[i].GetItemStateIndex(j)) != -1) {
                        m_ItemSlotStateIndex[j] = value;
                    }
                    if (m_ItemSlotSubstateIndex[j] == -1 && (value = m_ActiveItemAbilities[i].GetItemSubstateIndex(j)) != -1) {
                        m_ItemSlotSubstateIndex[j] = value;
                    }
                }
            }

            // Assign the values to the animator.
            for (int i = 0; i < m_ItemSlotStateIndex.Length; ++i) {
                if (m_ItemSlotStateIndex[i] == -1) { m_ItemSlotStateIndex[i] = 0; }
                if (m_ItemSlotSubstateIndex[i] == -1) { m_ItemSlotSubstateIndex[i] = 0; }
                m_AnimatorMonitor.SetItemStateIndexParameter(i, m_ItemSlotStateIndex[i]);
                m_AnimatorMonitor.SetItemSubstateIndexParameter(i, m_ItemSlotSubstateIndex[i]);
            }
            m_AnimatorMonitor.SetAimingParameter(m_Aiming);
        }

        /// <summary>
        /// Tries to stop the specified ability.
        /// </summary>
        /// <param name="ability">The ability to try to stop.</param>
        /// <returns>True if the ability was stopped.</returns>
        public bool TryStopAbility(Ability ability)
        {
            return TryStopAbility(ability, false);
        }

        /// <summary>
        /// Tries to stop the specified ability.
        /// </summary>
        /// <param name="ability">The ability to try to stop.</param>
        /// <param name="force">Should the ability be force stopped?</param>
        public bool TryStopAbility(Ability ability, bool force)
        {
            // The ability can't be stopped if it isn't active.
            if (!ability.IsActive) {
                return false;
            }

            ability.WillTryStopAbility();

            // CanStopAbility and CanForceStopAbility can prevent the ability from stopping.
            if ((!force && !ability.CanStopAbility()) || (force && !ability.CanForceStopAbility())) {
                return false;
            }

            // Update the active ability array by removing the stopped ability.
            if (ability is ItemAbility) {
                for (int i = ability.ActiveIndex; i < m_ActiveItemAbilityCount - 1; ++i) {
                    m_ActiveItemAbilities[i] = m_ActiveItemAbilities[i + 1];
                    m_ActiveItemAbilities[i].ActiveIndex = i;
                }
                m_ActiveItemAbilityCount--;
                m_ActiveItemAbilities[m_ActiveItemAbilityCount] = null;

                ability.StopAbility(force, true);

                // Let others know that the ability has stopped.
                var itemAbility = ability as ItemAbility;
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterItemAbilityActive", itemAbility, false);
                if (m_OnItemAbilityActiveEvent != null) {
                    m_OnItemAbilityActiveEvent.Invoke(itemAbility, false);
                }
                m_DirtyItemAbilityParameter = true;
            } else {
                for (int i = ability.ActiveIndex; i < m_ActiveAbilityCount - 1; ++i) {
                    m_ActiveAbilities[i] = m_ActiveAbilities[i + 1];
                    m_ActiveAbilities[i].ActiveIndex = i;
                }
                m_ActiveAbilityCount--;
                m_ActiveAbilities[m_ActiveAbilityCount] = null;

                ability.StopAbility(force, true);

                // Let others know that the ability has stopped.
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterAbilityActive", ability, false);
                if (m_OnAbilityActiveEvent != null) {
                    m_OnAbilityActiveEvent.Invoke(ability, false);
                }
                m_DirtyAbilityParameter = true;

                // After the ability has stopped it may need to equip the unequipped items again.
                if (!(ability is ItemEquipVerifier) && !(ability is MoveTowards) && m_ItemEquipVerifierAbility != null) {
                    m_ItemEquipVerifierAbility.TryToggleItem(ability, false);
                }
            }

            // If the AnimatorMonitor is enabled then the character is dead and the animator parameters should be updated immediately.
            if (m_AnimatorMonitor != null && m_AnimatorMonitor.enabled) {
                UpdateDirtyAbilityAnimatorParameters();
            }

            return true;
        }

        /// <summary>
        /// Returns the ability of type T.
        /// </summary>
        /// <typeparam name="T">The type of ability to return.</typeparam>
        /// <returns>The ability of type T. Can be null.</returns>
        public T GetAbility<T>() where T : Ability
        {
            return GetAbility<T>(-1);
        }

        /// <summary>
        /// Returns the ability of type T with the specified index.
        /// </summary>
        /// <typeparam name="T">The type of ability to return.</typeparam>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The ability of type T with the specified index. Can be null.</returns>
        public T GetAbility<T>(int index) where T : Ability
        {
            var type = typeof(T);
            var allAbilities = (typeof(ItemAbility).IsAssignableFrom(type) ? m_ItemAbilities : m_Abilities);
            if (allAbilities != null) {
                for (int i = 0; i < allAbilities.Length; ++i) {
                    if (type == allAbilities[i].GetType() && (index == -1 || index == allAbilities[i].Index)) {
                        return allAbilities[i] as T;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the abilities of type T.
        /// </summary>
        /// <typeparam name="T">The type of ability to return.</typeparam>
        /// <returns>The abilities of type T. Can be null.</returns>
        public T[] GetAbilities<T>() where T : Ability
        {
            return GetAbilities<T>(-1);
        }

        /// <summary>
        /// Returns the abilities of type T with the specified index.
        /// </summary>
        /// <typeparam name="T">The type of ability to return.</typeparam>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The abilities of type T with the specified index. Can be null.</returns>
        public T[] GetAbilities<T>(int index) where T : Ability
        {
            if (m_Abilities == null) { DeserializeAbilities(); }
            if (m_ItemAbilities == null) { DeserializeItemAbilities(); }

            var allAbilities = (typeof(ItemAbility).IsAssignableFrom(typeof(T)) ? m_ItemAbilities : m_Abilities);
            var count = 0;
            if (allAbilities != null) {
                // Determine the total number of abilities first so only one allocation is made.
                for (int i = 0; i < allAbilities.Length; ++i) {
                    if (allAbilities[i] is T && (index == -1 || index == allAbilities[i].Index)) {
                        count++;
                    }
                }

                if (count > 0) {
                    var abilities = new T[count];
                    count = 0;
                    for (int i = 0; i < allAbilities.Length; ++i) {
                        if (allAbilities[i] is T && (index == -1 || index == allAbilities[i].Index)) {
                            abilities[count] = allAbilities[i] as T;
                            count++;
                            if (count == abilities.Length) {
                                break;
                            }
                        }
                    }
                    return abilities;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the abilities of the specified type with the specified index.
        /// </summary>
        /// <param name="index">The type of ability.</param>
        /// <returns>The abilities of the specified type with the specified index. Can be null.</returns>
        public Ability[] GetAbilities(System.Type type)
        {
            return GetAbilities(type, -1);
        }
        
        /// <summary>
        /// Returns the abilities of the specified type with the specified index.
        /// </summary>
        /// <param name="index">The type of ability.</param>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The abilities of the specified type with the specified index. Can be null.</returns>
        public Ability[] GetAbilities(System.Type type, int index)
        {
            if (type == null) { return null; }
            if (m_Abilities == null) { DeserializeAbilities(); }
            if (m_ItemAbilities == null) { DeserializeItemAbilities(); }

            var allAbilities = (typeof(ItemAbility).IsAssignableFrom(type) ? m_ItemAbilities : m_Abilities);
            var count = 0;
            if (allAbilities != null) {
                // Determine the total number of abilities first so only one allocation is made.
                for (int i = 0; i < allAbilities.Length; ++i) {
                    if (allAbilities[i].GetType().IsAssignableFrom(type) && (index == -1 || index == allAbilities[i].Index)) {
                        count++;
                    }
                }

                if (count > 0) {
                    var abilities = new Ability[count];
                    count = 0;
                    for (int i = 0; i < allAbilities.Length; ++i) {
                        if (allAbilities[i].GetType().IsAssignableFrom(type) && (index == -1 || index == allAbilities[i].Index)) {
                            abilities[count] = allAbilities[i];
                            count++;
                            if (count == abilities.Length) {
                                break;
                            }
                        }
                    }
                    return abilities;
                }
            }

            return null;
        }

        /// <summary>
        /// Is the ability of the specified type active?
        /// </summary>
        /// <typeparam name="T">The type of ability.</typeparam>
        /// <returns>True if the ability is active.</returns>
        public bool IsAbilityTypeActive<T>() where T : Ability
        {
            var isItemAbility = typeof(ItemAbility).IsAssignableFrom(typeof(T));
            var activeAbilities = isItemAbility ? m_ActiveItemAbilities : m_ActiveAbilities;
            var count = isItemAbility ? m_ActiveItemAbilityCount : m_ActiveAbilityCount;
            if (activeAbilities != null) {
                for (int i = 0; i < count; ++i) {
                    if (typeof(T).IsAssignableFrom(activeAbilities[i].GetType())) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to start the specified effect.
        /// </summary>
        /// <param name="effect">The effect to try to start.</param>
        /// <returns>True if the effect was started.</returns>
        public bool TryStartEffect(Effect effect)
        {
            // The effect can't be started if it is already active, isn't enabled, or can't be started.
            if (effect.IsActive || !effect.Enabled || !effect.CanStartEffect()) {
                return false;
            }

            effect.StartEffect(m_ActiveEffectCount);
            m_ActiveEffects[m_ActiveEffectCount] = effect;
            m_ActiveEffectCount++;
            return true;
        }

        /// <summary>
        /// Tries to stop the specified effect.
        /// </summary>
        /// <param name="effect">The effect to try to stop.</param>
        /// <returns>True if the effect was stopped.</returns>
        public bool TryStopEffect(Effect effect)
        {
            // The effect can't be stopped if it isn't active.
            if (!effect.IsActive) {
                return false;
            }

            // Store the active index ahead of time because StopEffect will reset the value.
            var index = effect.ActiveIndex;
            effect.StopEffect(true);

            // Update the active effect array by removing the stopped ability.
            for (int i = index; i < m_ActiveEffectCount - 1; ++i) {
                m_ActiveEffects[i] = m_ActiveEffects[i + 1];
            }
            m_ActiveEffectCount--;
            m_ActiveEffects[m_ActiveEffectCount] = null;
            return true;
        }

        /// <summary>
        /// Returns the effect of type T.
        /// </summary>
        /// <typeparam name="T">The type of effect to return.</typeparam>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The effect of type T. Can be null.</returns>
        public T GetEffect<T>() where T : Effect
        {
            return GetEffect<T>(-1);
        }

        /// <summary>
        /// Returns the effect of type T at the specified index.
        /// </summary>
        /// <typeparam name="T">The type of effect to return.</typeparam>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The effect of type T. Can be null.</returns>
        public T GetEffect<T>(int index) where T : Effect
        {
            if (m_Effects == null) { DeserializeEffects(); }

            if (m_Effects != null) {
                var type = typeof(T);
                for (int i = 0; i < m_Effects.Length; ++i) {
                    if (type == m_Effects[i].GetType() && (index == -1 || index == m_Effects[i].Index)) {
                        return m_Effects[i] as T;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the effect of the specified type.
        /// </summary>
        /// <param name="type">The type of effect to retrieve.</param>
        /// <returns>The effect of the specified type. Can be null.</returns>
        public Effect GetEffect(System.Type type)
        {
            return GetEffect(type, -1);
        }

        /// <summary>
        /// Returns the effect of the specified type at the specified index.
        /// </summary>
        /// <param name="type">The type of effect to retrieve.</param>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The effect of the specified type. Can be null.</returns>
        public Effect GetEffect(System.Type type, int index)
        {
            if (type == null) { return null; }
            if (m_Effects == null) { DeserializeEffects(); }

            if (m_Effects != null) {
                for (int i = 0; i < m_Effects.Length; ++i) {
                    if (type == m_Effects[i].GetType() && (index == -1 || index == m_Effects[i].Index)) {
                        return m_Effects[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Callback from the animator when root motion has updated.
        /// </summary>
        protected override void OnAnimatorMove()
        {
            // If using root motion rotation then the delta position should be retrieved from the animation. If it not being retrieved then the abilities
            // have an option to change the delta position based on the AnimatorMotion ScriptableObject.
            if (UsingRootMotionPosition) {
                m_AnimatorDeltaPosition += m_Animator.deltaPosition;
            } else {
                for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                    m_ActiveAbilities[i].OnAnimatorMove(true);
                }
                for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                    m_ActiveItemAbilities[i].OnAnimatorMove(true);
                }
            }

            // If using root motion rotation then the delta rotation should be retrieved from the animation. If it not being retrieved then the abilities
            // have an option to change the delta rotation based on the AnimatorMotion ScriptableObject.
            if (UsingRootMotionRotation) {
                m_AnimatorDeltaRotation *= m_Animator.deltaRotation;
            } else {
                for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                    m_ActiveAbilities[i].OnAnimatorMove(false);
                }
                for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                    m_ActiveItemAbilities[i].OnAnimatorMove(false);
                }
            }

            base.OnAnimatorMove();
        }

        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            m_FirstPersonPerspective = firstPersonPerspective;
            if (firstPersonPerspective) {
                if (!string.IsNullOrEmpty(m_ThirdPersonStateName)) {
                    StateManager.SetState(m_GameObject, m_ThirdPersonStateName, false);
                }
                if (!string.IsNullOrEmpty(m_FirstPersonStateName)) {
                    StateManager.SetState(m_GameObject, m_FirstPersonStateName, true);
                }
                if (!string.IsNullOrEmpty(m_FirstPersonMovementTypeFullName)) {
                    SetMovementType(m_FirstPersonMovementTypeFullName);
                }
            } else {
                if (!string.IsNullOrEmpty(m_FirstPersonStateName)) {
                    StateManager.SetState(m_GameObject, m_FirstPersonStateName, false);
                }
                if (!string.IsNullOrEmpty(m_ThirdPersonStateName)) {
                    StateManager.SetState(m_GameObject, m_ThirdPersonStateName, true);
                }
                if (!string.IsNullOrEmpty(m_ThirdPersonMovementTypeFullName)) {
                    SetMovementType(m_ThirdPersonMovementTypeFullName);
                }
            }

            if (m_MovementType == null) {
                SetMovementType(m_MovementTypeFullName);
            }
        }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        /// <summary>
        /// Pushes the target Rigidbody in the specified direction.
        /// </summary>
        /// <param name="targetRigidbody">The Rigidbody to push.</param>
        /// <param name="moveDirection">The direction that the character is moving.</param>
        /// <param name="point">The point at which to apply the push force.</param>
        /// <param name="radius">The radius of the pushing collider.</param>
        /// <returns>Was the rigidbody pushed?</returns>
        protected override bool PushRigidbody(Rigidbody targetRigidbody, Vector3 moveDirection, Vector3 point, float radius)
        {
            var pushed = base.PushRigidbody(targetRigidbody, moveDirection, point, radius);
            if (pushed && m_NetworkInfo != null) {
                m_NetworkCharacter.PushRigidbody(targetRigidbody, (moveDirection / m_DeltaTime) * (m_Mass / targetRigidbody.mass) * 0.01f, point);
            }
            return pushed;
        }
#endif

        /// <summary>
        /// Sets the rotation of the character.
        /// </summary>
        /// <param name="rotation">The rotation to set.</param>
        public override void SetRotation(Quaternion rotation)
        {
            SetRotation(rotation, true);
        }

        /// <summary>
        /// Sets the rotation of the character.
        /// </summary>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        public void SetRotation(Quaternion rotation, bool snapAnimator)
        {
            // If the character isn't active then only the transform needs to be set.
            if (m_GameObject == null) {
                transform.rotation = rotation;
                return;
            }

            base.SetRotation(rotation);

            if (snapAnimator) {
                StopAllAbilities(false);
            }

            // If the index is -1 then the character isn't registered with the Kinematic Object Manager.
            if (m_KinematicObjectIndex != -1) {
                KinematicObjectManager.SetCharacterRotation(m_KinematicObjectIndex, rotation);
            }

            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterImmediateTransformChange", snapAnimator);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkCharacter.SetRotation(rotation, snapAnimator);
            }
#endif
        }

        /// <summary>
        /// Sets the position of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        public override void SetPosition(Vector3 position)
        {
            SetPosition(position, true);
        }

        /// <summary>
        /// Sets the position of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        public void SetPosition(Vector3 position, bool snapAnimator)
        {
            // If the character isn't active then only the transform needs to be set.
            if (m_GameObject == null) {
                transform.position = position;
                return;
            }

            m_MaxHeight = float.NegativeInfinity;
            m_MaxHeightPosition = position;
            base.SetPosition(position);
            if (snapAnimator) {
                StopAllAbilities(false);
            }

            // If the index is -1 then the character isn't registered with the Kinematic Object Manager.
            if (m_KinematicObjectIndex != -1) {
                KinematicObjectManager.SetCharacterPosition(m_KinematicObjectIndex, position);
            }

            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterImmediateTransformChange", snapAnimator);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkCharacter.SetPosition(position, snapAnimator);
            }
#endif
        }

        /// <summary>
        /// Resets the rotation and position to their default values.
        /// </summary>
        public override void ResetRotationPosition()
        {
            if (m_GameObject == null) {
                return;
            }

            Moving = false;
            base.ResetRotationPosition();

            // If the index is -1 then the character isn't registered with the Kinematic Object Manager.
            if (m_KinematicObjectIndex == -1) {
                return;
            }

            KinematicObjectManager.SetCharacterRotation(m_KinematicObjectIndex, m_Transform.rotation);
            KinematicObjectManager.SetCharacterPosition(m_KinematicObjectIndex, m_Transform.position);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkCharacter.ResetRotationPosition();
            }
#endif
        }

        /// <summary>
        /// Sets the position and rotation of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="rotation">The rotation to set.</param>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            SetPositionAndRotation(position, rotation, true);
        }

        /// <summary>
        /// Sets the position and rotation of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool snapAnimator)
        {
            if (m_GameObject == null) {
                return;
            }

            m_MaxHeight = float.NegativeInfinity;
            m_MaxHeightPosition = position;
            base.SetRotation(rotation);
            base.SetPosition(position);

            if (snapAnimator) {
                StopAllAbilities(false);
            }

            // If the index is -1 then the character isn't registered with the Kinematic Object Manager.
            if (m_KinematicObjectIndex != -1) {
                KinematicObjectManager.SetCharacterRotation(m_KinematicObjectIndex, rotation);
                KinematicObjectManager.SetCharacterPosition(m_KinematicObjectIndex, position);
            }

            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterImmediateTransformChange", snapAnimator);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkCharacter.SetPositionAndRotation(position, rotation, snapAnimator);
            }
#endif
        }

        /// <summary>
        /// Activates or deactivates the character.
        /// </summary>
        /// <param name="active">Is the character active?</param>
        public void SetActive(bool active)
        {
            SetActive(active, false);
        }

        /// <summary>
        /// Activates or deactivates the character.
        /// </summary>
        /// <param name="active">Is the character active?</param>
        /// <param name="uiEvent">Should the OnShowUI event be executed?</param>
        public void SetActive(bool active, bool uiEvent)
        {
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterActivate", active);
            m_GameObject.SetActive(active);

            if (active) {
                // Do a pass on trying to start any abilities in case they should be started immediately when activated.
                UpdateAbilities(m_Abilities);
                UpdateAbilities(m_ItemAbilities);
                UpdateDirtyAbilityAnimatorParameters();

                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterImmediateTransformChange", true);
            }
            if (uiEvent) {
                EventHandler.ExecuteEvent(m_GameObject, "OnShowUI", active);
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkCharacter.SetActive(active, uiEvent);
            }
#endif
        }

        /// <summary>
        /// Casts a ray using in the specified direction. If the character has multiple colliders added then a ray will be cast from each collider.
        /// A CapsuleCast or SphereCast is used depending on the type of collider that has been added.
        /// </summary>
        /// <param name="direction">The direction to perform the cast.</param>
        /// <param name="layers">The layers to perform the cast on.</param>
        /// <param name="offset">The offset of the cast.</param>
        /// <param name="result">The hit RaycastHit.</param>
        /// <returns>Did the cast hit an object?</returns>
        public bool SingleCast(Vector3 direction, Vector3 offset, int layers, ref RaycastHit result)
        {
            for (int i = 0; i < m_ColliderCount; ++i) {
                // Determine if the collider would intersect any objects.
                if (m_Colliders[i] is CapsuleCollider) {
                    Vector3 startEndCap, endEndCap;
                    var capsuleCollider = m_Colliders[i] as CapsuleCollider;
                    MathUtility.CapsuleColliderEndCaps(capsuleCollider, capsuleCollider.transform.position, capsuleCollider.transform.rotation, out startEndCap, out endEndCap);
                    var radius = capsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(capsuleCollider) - ColliderSpacing;
                    if (Physics.CapsuleCast(startEndCap + offset, endEndCap + offset, radius, direction.normalized, out result, direction.magnitude + ColliderSpacing, layers, QueryTriggerInteraction.Ignore)) {
                        return true;
                    }
                } else { // SphereCollider.
                    var sphereCollider = m_Colliders[i] as SphereCollider;
                    var radius = sphereCollider.radius * MathUtility.ColliderRadiusMultiplier(sphereCollider) - ColliderSpacing;
                    if (Physics.SphereCast(sphereCollider.transform.TransformPoint(sphereCollider.center) + offset, radius, direction.normalized,
                                                                    out result, direction.magnitude + ColliderSpacing, layers, QueryTriggerInteraction.Ignore)) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Casts a ray using in the specified direction. If the character has multiple colliders added then a ray will be cast from each collider.
        /// A CapsuleCast or SphereCast is used depending on the type of collider that has been added. The result is stored in the m_CombinedRaycastHits array.
        /// </summary>
        /// <param name="direction">The direction to perform the cast.</param>
        /// <param name="offset">Any offset to apply to the cast.</param>
        /// <param name="colliderIndexMap">The found raycast hits.</param>
        /// <param name="combinedRaycastHits">A mapping between the raycast hit and collider index.</param>
        /// <returns>The number of objects hit from the cast.</returns>
        public int Cast(Vector3 direction, Vector3 offset, ref RaycastHit[] combinedRaycastHits, ref Dictionary<RaycastHit, int> colliderIndexMap)
        {
            if (m_ColliderCount > 1) {
                if (combinedRaycastHits == null) {
                    combinedRaycastHits = new RaycastHit[m_ColliderCount * m_RaycastHits.Length];
                    colliderIndexMap = new Dictionary<RaycastHit, int>();
                }
                // Clear the index map to start it off fresh.
                colliderIndexMap.Clear();
            }

            var hitCount = 0;
            for (int i = 0; i < m_ColliderCount; ++i) {
                int localHitCount;
                // Determine if the collider would intersect any objects.
                if (m_Colliders[i] is CapsuleCollider) {
                    Vector3 startEndCap, endEndCap;
                    var capsuleCollider = m_Colliders[i] as CapsuleCollider;
                    MathUtility.CapsuleColliderEndCaps(capsuleCollider, capsuleCollider.transform.position + offset, capsuleCollider.transform.rotation, out startEndCap, out endEndCap);
                    var radius = capsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(capsuleCollider) - ColliderSpacing;
                    localHitCount = Physics.CapsuleCastNonAlloc(startEndCap, endEndCap, radius, direction.normalized, m_RaycastHits, direction.magnitude + ColliderSpacing, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                } else { // SphereCollider.
                    var sphereCollider = m_Colliders[i] as SphereCollider;
                    var radius = sphereCollider.radius * MathUtility.ColliderRadiusMultiplier(sphereCollider) - ColliderSpacing;
                    localHitCount = Physics.SphereCastNonAlloc(sphereCollider.transform.TransformPoint(sphereCollider.center) + offset, radius, direction.normalized,
                                                                    m_RaycastHits, direction.magnitude + ColliderSpacing, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                }

                if (localHitCount > 0) {
                    // The mapping needs to be saved if there are multiple colliders.
                    if (m_ColliderCount > 1) {
                        int validHitCount = 0;
                        for (int j = 0; j < localHitCount; ++j) {
                            if (colliderIndexMap.ContainsKey(m_RaycastHits[j])) {
                                continue;
                            }
                            // Ensure the array is large enough.
                            if (hitCount + j >= combinedRaycastHits.Length) {
                                Debug.LogWarning("Warning: The maximum number of collisions has been reached. Consider increasing the CharacterLocomotion MaxCollisionCount value.");
                                continue;
                            }

                            colliderIndexMap.Add(m_RaycastHits[j], i);
                            combinedRaycastHits[hitCount + j] = m_RaycastHits[j];
                            validHitCount += 1;
                        }
                        hitCount += validHitCount;
                    } else {
                        combinedRaycastHits = m_RaycastHits;
                        hitCount += localHitCount;
                    }
                }
            }

            return hitCount;
        }

        /// <summary>
        /// Returns the number of colliders that are overlapping the character's collider.
        /// </summary>
        /// <param name="offset">The offset to apply to the character's collider position.</param>
        /// <returns>The number of objects which overlap the collider.</returns>
        public int OverlapCount(Vector3 offset)
        {
            var count = 0;
            for (int i = 0; i < m_ColliderCount; ++i) {
                count += OverlapCount(m_Colliders[i], offset);
            }
            return count;
        }

        /// <summary>
        /// The character has entered a trigger.
        /// </summary>
        /// <param name="other">The trigger collider that the character entered.</param>
        private void OnTriggerEnter(Collider other)
        {
            // Forward the enter to the abilities.
            if (m_Abilities != null) {
                for (int i = 0; i < m_Abilities.Length; ++i) {
                    if (!m_Abilities[i].Enabled) {
                        continue;
                    }

                    m_Abilities[i].OnTriggerEnter(other);
                }
            }
            if (m_ItemAbilities != null) {
                for (int i = 0; i < m_ItemAbilities.Length; ++i) {
                    if (!m_ItemAbilities[i].Enabled) {
                        continue;
                    }

                    m_ItemAbilities[i].OnTriggerEnter(other);
                }
            }
        }

        /// <summary>
        /// The character has exited a trigger.
        /// </summary>
        /// <param name="other">The trigger collider that the character exited.</param>
        private void OnTriggerExit(Collider other)
        {
            // Forward the exit to the abilities.
            if (m_Abilities != null) {
                for (int i = 0; i < m_Abilities.Length; ++i) {
                    if (!m_Abilities[i].Enabled) {
                        continue;
                    }

                    m_Abilities[i].OnTriggerExit(other);
                }
            }
            if (m_ItemAbilities != null) {
                for (int i = 0; i < m_ItemAbilities.Length; ++i) {
                    if (!m_ItemAbilities[i].Enabled) {
                        continue;
                    }

                    m_ItemAbilities[i].OnTriggerExit(other);
                }
            }
        }

        /// <summary>
        /// The character has started or stopped aiming
        /// </summary>
        /// <param name="aiming">Has the character started to aim?</param>
        private void OnAiming(bool aiming)
        {
            m_Aiming = aiming;
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            m_Alive = false;

            // All of the abilities should stop.
            StopAllAbilities(true);

            // The animator values should reset.
            m_YawAngle = 0;
            m_InputVector = Vector3.zero;
            Moving = false;

            // Remote networked characters will not be registered with the KinematicObjectManager.
            if (m_KinematicObjectIndex == -1) {
                return;
            }

            // The character is no longer moving.
            KinematicObjectManager.SetCharacterMovementInput(m_KinematicObjectIndex, 0, 0);
            KinematicObjectManager.SetCharacterDeltaYawRotation(m_KinematicObjectIndex, 0);
        }

        /// <summary>
        /// Stops all of the active abilities.
        /// </summary>
        /// <param name="fromDeath">Are the abilities being stopped from death callback?</param>
        private void StopAllAbilities(bool fromDeath)
        {
            for (int i = m_ActiveAbilityCount - 1; i >= 0; --i) {
                if (!fromDeath || !m_ActiveAbilities[i].CanStayActivatedOnDeath) {
                    TryStopAbility(m_ActiveAbilities[i], true);
                }
            }
            for (int i = m_ActiveItemAbilityCount - 1; i >= 0; --i) {
                if (!fromDeath || !m_ActiveItemAbilities[i].CanStayActivatedOnDeath) {
                    TryStopAbility(m_ActiveItemAbilities[i], true);
                }
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            m_Alive = true;
            ResetRotationPosition();

            // Do a pass on trying to start any abilities and items to ensure they are in sync.
            UpdateAbilities(m_Abilities);
            UpdateAbilities(m_ItemAbilities);
            if (m_AnimatorMonitor != null) {
                m_AnimatorMonitor.SetPitchParameter(m_LookSource.Pitch, 1, 0);
                m_AnimatorMonitor.SetYawParameter(m_YawAngle, 1, 0);
                m_AnimatorMonitor.SetHorizontalMovementParameter(m_InputVector.x, 1, 0);
                m_AnimatorMonitor.SetForwardMovementParameter(m_InputVector.y, 1, 0);
                m_AnimatorMonitor.SetMovingParameter(m_Moving);
                UpdateDirtyAbilityAnimatorParameters();
            }
        }

        /// <summary>
        /// The character's position or rotation has been teleported.
        /// </summary>
        /// <param name="snapAnimator">Should the animator be snapped?</param>
        private void OnImmediateTransformChange(bool snapAnimator)
        {
            UpdateAbilityAnimatorParameters(true);
            UpdateItemAbilityAnimatorParameters(true);
            // Snap the animator after the abilities have updated.
            if (snapAnimator) {
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterSnapAnimator");
            }
        }

        /// <summary>
        /// The Animator has snapped into position.
        /// </summary>
        private void AnimatorSnapped()
        {
            ResetRotationPosition();
        }

        /// <summary>
        /// The character has been disabled.
        /// </summary>
        private void OnDisable()
        {
            // The KinematicObjectManager is responsible for calling the move method.
            KinematicObjectManager.UnregisterCharacter(m_KinematicObjectIndex);
            m_KinematicObjectIndex = -1;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.UnregisterEvent<bool>(gameObject, "OnCharacterChangePerspectives", OnChangePerspectives);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnAimAbilityAim", OnAiming);
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_GameObject, "OnRespawn", OnRespawn);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorSnapped", AnimatorSnapped);

            // The current movement type is no longer active when the object is destroyed.
            if (m_MovementType != null) {
                m_MovementType.ChangeMovementType(false);
            }

            // Call OnDestroy to notify all of the abilities and effects that the GameObject has been destroyed.
            if (m_Abilities != null) {
                for (int i = 0; i < m_Abilities.Length; ++i) {
                    m_Abilities[i].OnDestroy();
                }
            }
            if (m_Effects != null) {
                for (int i = 0; i < m_Effects.Length; ++i) {
                    m_Effects[i].OnDestroy();
                }
            }
        }
    }
}