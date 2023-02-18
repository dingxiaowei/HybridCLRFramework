/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.StateSystem;
    using UnityEngine;

    /// <summary>
    /// The AnimatorMonitor acts as a bridge for the parameters on the Animator component.
    /// If an Animator component is not attached to the character (such as for first person view) then the updates will be forwarded to the item's Animator.
    /// </summary>
    public class AnimatorMonitor : StateBehavior
    {
#if UNITY_EDITOR
        [Tooltip("Should the Animator log any changes to the item parameters?")]
        [SerializeField] protected bool m_LogAbilityParameterChanges;
        [Tooltip("Should the Animator log any changes to the item parameters?")]
        [SerializeField] protected bool m_LogItemParameterChanges;
        [Tooltip("Should the Animator log any events that it sends?")]
        [SerializeField] protected bool m_LogEvents;
#endif
        [Tooltip("The damping time for the Horizontal Movement parameter. The higher the value the slower the parameter value changes.")]
        [SerializeField] protected float m_HorizontalMovementDampingTime = 0.1f;
        [Tooltip("The damping time for the Forward Movement parameter. The higher the value the slower the parameter value changes.")]
        [SerializeField] protected float m_ForwardMovementDampingTime = 0.1f;
        [Tooltip("The damping time for the Pitch parameter. The higher the value the slower the parameter value changes.")]
        [SerializeField] protected float m_PitchDampingTime = 0.1f;
        [Tooltip("The damping time for the Yaw parameter. The higher the value the slower the parameter value changes.")]
        [SerializeField] protected float m_YawDampingTime = 0.1f;

#if UNITY_EDITOR
        public bool LogEvents { get { return m_LogEvents; } }
#endif
        public float HorizontalMovementDampingTime { get { return m_HorizontalMovementDampingTime; } set { m_HorizontalMovementDampingTime = value; } }
        public float ForwardMovementDampingTime { get { return m_ForwardMovementDampingTime; } set { m_ForwardMovementDampingTime = value; } }
        public float PitchDampingTime { get { return m_PitchDampingTime; } set { m_PitchDampingTime = value; } }
        public float YawDampingTime { get { return m_YawDampingTime; } set { m_YawDampingTime = value; } }

        private static int s_HorizontalMovementHash = Animator.StringToHash("HorizontalMovement");
        private static int s_ForwardMovementHash = Animator.StringToHash("ForwardMovement");
        private static int s_PitchHash = Animator.StringToHash("Pitch");
        private static int s_YawHash = Animator.StringToHash("Yaw");
        private static int s_SpeedHash = Animator.StringToHash("Speed");
        private static int s_HeightHash = Animator.StringToHash("Height");
        private static int s_MovingHash = Animator.StringToHash("Moving");
        private static int s_AimingHash = Animator.StringToHash("Aiming");
        private static int s_MovementSetIDHash = Animator.StringToHash("MovementSetID");
        private static int s_AbilityIndexHash = Animator.StringToHash("AbilityIndex");
        private static int s_AbilityChangeHash = Animator.StringToHash("AbilityChange");
        private static int s_AbilityIntDataHash = Animator.StringToHash("AbilityIntData");
        private static int s_AbilityFloatDataHash = Animator.StringToHash("AbilityFloatData");
        private static int[] s_ItemSlotIDHash;
        private static int[] s_ItemSlotStateIndexHash;
        private static int[] s_ItemSlotStateIndexChangeHash;
        private static int[] s_ItemSlotSubstateIndexHash;

        protected GameObject m_GameObject;
        protected Transform m_Transform;
        protected Animator m_Animator;

        private float m_HorizontalMovement;
        private float m_ForwardMovement;
        private float m_Pitch;
        private float m_Yaw;
        private float m_Speed;
        private int m_Height;
        private bool m_Moving;
        private bool m_Aiming;
        private int m_MovementSetID;
        private int m_AbilityIndex;
        private int m_AbilityIntData;
        private float m_AbilityFloatData;
        private bool m_HasItemParameters;
        private int[] m_ItemSlotID;
        private int[] m_ItemSlotStateIndex;
        private int[] m_ItemSlotSubstateIndex;
        private Item[] m_EquippedItems;
        private bool m_EquippedItemsDirty;

        public bool AnimatorEnabled { get { return m_Animator != null && m_Animator.enabled; } }
        public bool FixedUpdateMode {  get { return m_Animator != null && m_Animator.updateMode == AnimatorUpdateMode.AnimatePhysics; } }
        public float HorizontalMovement { get { return m_HorizontalMovement; } }
        public float ForwardMovement { get { return m_ForwardMovement; } }
        public float Pitch { get { return m_Pitch; } }
        public float Yaw { get { return m_Yaw; } }
        public float Speed { get { return m_Speed; } }
        public int Height { get { return m_Height; } }
        public bool Moving { get { return m_Moving; } }
        public bool Aiming { get { return m_Aiming; } }
        public int MovementSetID { get { return m_MovementSetID; } }
        public int AbilityIndex { get { return m_AbilityIndex; } }
        public bool AbilityChange { get { return (m_Animator != null) && m_Animator.GetBool(s_AbilityChangeHash); } }
        public int AbilityIntData { get { return m_AbilityIntData; } }
        public float AbilityFloatData { get { return m_AbilityFloatData; } }
        public bool HasItemParameters { get { return m_HasItemParameters; } }
        public int ParameterSlotCount { get { return m_ItemSlotID.Length; } }
        public int[] ItemSlotID { get { return m_ItemSlotID; } }
        public int[] ItemSlotStateIndex { get { return m_ItemSlotStateIndex; } }
        public int[] ItemSlotSubstateIndex { get { return m_ItemSlotSubstateIndex; } }
        [Snapshot] protected Item[] EquippedItems { get { return m_EquippedItems; } set { m_EquippedItems = value; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_Transform = transform;
            m_Animator = m_GameObject.GetCachedComponent<Animator>();

#if UNITY_EDITOR
            // If the animator doesn't have the required parameters then it's not a valid animator.
            if (m_Animator != null) {
                if (!HasParameter(s_HorizontalMovementHash) || !HasParameter(s_ForwardMovementHash) || !HasParameter(s_AbilityChangeHash)) {
                    Debug.LogError($"Error: The animator {m_Animator.name} is not designed to work with the Ultimate Character Controller. " +
                                   "Ensure the animator has all of the required parameters.");
                    return;
                }
            }
#endif
            InitializeItemParameters();

            EventHandler.RegisterEvent<Item, int>(m_GameObject, "OnAbilityWillEquipItem", OnWillEquipItem);
            EventHandler.RegisterEvent<Item, int>(m_GameObject, "OnAbilityUnequipItemComplete", OnUnequipItem);
            EventHandler.RegisterEvent<Item, int>(m_GameObject, "OnInventoryRemoveItem", OnUnequipItem);
            if (m_Animator != null) {
                EventHandler.RegisterEvent(m_GameObject, "OnCharacterSnapAnimator", SnapAnimator);
                EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterChangeUpdateLocation", OnChangeUpdateLocation);
                EventHandler.RegisterEvent<float>(m_GameObject, "OnCharacterChangeTimeScale", OnChangeTimeScale);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Does the animator have the specified parameter?
        /// </summary>
        /// <param name="parameterHash">The hash of the parameter.</param>
        /// <returns>True if the animator has the specified parameter.</returns>
        private bool HasParameter(int parameterHash)
        {
            for (int i = 0; i < m_Animator.parameterCount; ++i) {
                if (m_Animator.parameters[i].nameHash == parameterHash) {
                    return true;
                }
            }
            return false;
        }
#endif

        /// <summary>
        /// Initializes the item parameters.
        /// </summary>
        public void InitializeItemParameters()
        {
            if (m_HasItemParameters) {
                return;
            }
            // The Animator Controller may not have the item parameters if the character can never equip an item.
            m_HasItemParameters = m_GameObject.GetComponentInChildren<ItemPlacement>() != null;

            var inventory = m_GameObject.GetComponent<InventoryBase>();
            if (inventory != null) {
                var slotCount = inventory.SlotCount;
                m_EquippedItems = new Item[slotCount];

                m_ItemSlotID = new int[slotCount];
                m_ItemSlotStateIndex = new int[slotCount];
                m_ItemSlotSubstateIndex = new int[slotCount];

                if (s_ItemSlotIDHash == null || s_ItemSlotIDHash.Length < slotCount) {
                    s_ItemSlotIDHash = new int[slotCount];
                    s_ItemSlotStateIndexHash = new int[slotCount];
                    s_ItemSlotStateIndexChangeHash = new int[slotCount];
                    s_ItemSlotSubstateIndexHash = new int[slotCount];

                    for (int i = 0; i < slotCount; ++i) {
                        s_ItemSlotIDHash[i] = Animator.StringToHash(string.Format("Slot{0}ItemID", i));
                        s_ItemSlotStateIndexHash[i] = Animator.StringToHash(string.Format("Slot{0}ItemStateIndex", i));
                        s_ItemSlotStateIndexChangeHash[i] = Animator.StringToHash(string.Format("Slot{0}ItemStateIndexChange", i));
                        s_ItemSlotSubstateIndexHash[i] = Animator.StringToHash(string.Format("Slot{0}ItemSubstateIndex", i));
                    }
                }
            }
        }

        /// <summary>
        /// Prepares the Animator parameters for start.
        /// </summary>
        protected virtual void Start()
        {
            SnapAnimator();

            if (m_Animator != null) {
                var characterLocomotion = m_GameObject.GetCachedComponent<UltimateCharacterLocomotion>();
                OnChangeUpdateLocation(characterLocomotion.UpdateLocation == Game.KinematicObjectManager.UpdateLocation.FixedUpdate);
                OnChangeTimeScale(characterLocomotion.TimeScale);
            }
        }

        /// <summary>
        /// Snaps the animator to the default values.
        /// </summary>
        protected virtual void SnapAnimator()
        {
            // A first person view may not use an Animator.
            if (m_Animator != null) {
                // The values should be reset enabled so the animator will snap to the correct animation.
                m_Animator.SetFloat(s_HorizontalMovementHash, m_HorizontalMovement, 0, 0);
                m_Animator.SetFloat(s_ForwardMovementHash, m_ForwardMovement, 0, 0);
                m_Animator.SetFloat(s_PitchHash, m_Pitch, 0, 0);
                m_Animator.SetFloat(s_YawHash, m_Yaw, 0, 0);
                m_Animator.SetFloat(s_SpeedHash, m_Speed, 0, 0);
                m_Animator.SetFloat(s_HeightHash, m_Height, 0, 0);
                m_Animator.SetBool(s_MovingHash, m_Moving);
                m_Animator.SetBool(s_AimingHash, m_Aiming);
                m_Animator.SetInteger(s_MovementSetIDHash, m_MovementSetID);
                m_Animator.SetInteger(s_AbilityIndexHash, m_AbilityIndex);
                m_Animator.SetTrigger(s_AbilityChangeHash);
                m_Animator.SetInteger(s_AbilityIntDataHash, m_AbilityIntData);
                m_Animator.SetFloat(s_AbilityFloatDataHash, m_AbilityFloatData, 0, 0);

                if (m_HasItemParameters) {
                    UpdateItemIDParameters();
                    for (int i = 0; i < m_EquippedItems.Length; ++i) {
                        m_Animator.SetInteger(s_ItemSlotIDHash[i], m_ItemSlotID[i]);
                        m_Animator.SetTrigger(s_ItemSlotStateIndexChangeHash[i]);
                        m_Animator.SetInteger(s_ItemSlotStateIndexHash[i], m_ItemSlotStateIndex[i]);
                        m_Animator.SetInteger(s_ItemSlotSubstateIndexHash[i], m_ItemSlotSubstateIndex[i]);
                    }
                }

                EventHandler.ExecuteEvent(m_GameObject, "OnAnimatorWillSnap");

                // Root motion should not move the character when snapping.
                var position = m_Transform.position;
                var rotation = m_Transform.rotation;

                // Update 0 will force the changes.
                m_Animator.Update(0);
                // Keep updating the Animator until it is no longer in a transition. This will snap the animator to the correct state immediately.
                while (IsInTrasition()) {
                    m_Animator.Update(Time.fixedDeltaTime);
                }
                m_Animator.Update(0);
                // The animator should be positioned at the start of each state.
                for (int i = 0; i < m_Animator.layerCount; ++i) {
                    m_Animator.Play(m_Animator.GetCurrentAnimatorStateInfo(i).fullPathHash, i, 0);
                }
                m_Animator.Update(Time.fixedDeltaTime);
                // Prevent the change parameters from staying triggered when the animator is on the idle state.
                SetAbilityChangeParameter(false);

                m_Transform.position = position;
                m_Transform.rotation = rotation;
            }

            // The item animators should also snap.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    SetItemStateIndexChangeParameter(i, false);
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SnapAnimator();
                    }
                }
            }
            EventHandler.ExecuteEvent(m_GameObject, "OnAnimatorSnapped");
        }

        /// <summary>
        /// Is the Animator Controller currently in a transition?
        /// </summary>
        /// <returns>True if any layer within the Animator Controller is within a transition.</returns>
        private bool IsInTrasition()
        {
            for (int i = 0; i < m_Animator.layerCount; ++i) {
                if (m_Animator.IsInTransition(i)) {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Returns true if the specified layer is in transition.
        /// </summary>
        /// <param name="layerIndex">The layer to determine if it is in transition.</param>
        /// <returns>True if the specified layer is in transition.</returns>
        public bool IsInTransition(int layerIndex)
        {
            if (m_Animator == null) {
                return false;
            }

            return m_Animator.IsInTransition(layerIndex);
        }

        /// <summary>
        /// Sets the Horizontal Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        public void SetHorizontalMovementParameter(float value, float timeScale)
        {
            SetHorizontalMovementParameter(value, timeScale, m_HorizontalMovementDampingTime);
        }

        /// <summary>
        /// Sets the Horizontal Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetHorizontalMovementParameter(float value, float timeScale, float dampingTime)
        {
            var change = m_HorizontalMovement != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_HorizontalMovementHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    m_HorizontalMovement = m_Animator.GetFloat(s_HorizontalMovementHash);
                    if (Mathf.Abs(m_HorizontalMovement) < 0.001f) {
                        m_HorizontalMovement = 0;
                    }
                } else {
                    m_HorizontalMovement = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetHorizontalMovementParameter(value, timeScale, dampingTime);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Forward Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        public void SetForwardMovementParameter(float value, float timeScale)
        {
            SetForwardMovementParameter(value, timeScale, m_ForwardMovementDampingTime);
        }

        /// <summary>
        /// Sets the Forward Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetForwardMovementParameter(float value, float timeScale, float dampingTime)
        {
            var change = m_ForwardMovement != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_ForwardMovementHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    m_ForwardMovement = m_Animator.GetFloat(s_ForwardMovementHash);
                    if (Mathf.Abs(m_ForwardMovement) < 0.001f) {
                        m_ForwardMovement = 0;
                    }
                } else {
                    m_ForwardMovement = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetForwardMovementParameter(value, timeScale, dampingTime);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Pitch parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public void SetPitchParameter(float value, float timeScale)
        {
            SetPitchParameter(value, timeScale, m_PitchDampingTime);
        }

        /// <summary>
        /// Sets the Pitch parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetPitchParameter(float value, float timeScale, float dampingTime)
        {
            var change = m_Pitch != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_PitchHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    m_Pitch = m_Animator.GetFloat(s_PitchHash);
                    if (Mathf.Abs(m_Pitch) < 0.001f) {
                        m_Pitch = 0;
                    }
                } else {
                    m_Pitch = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetPitchParameter(value, timeScale, dampingTime);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Yaw parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <returns>True if the parameter was changed.</returns>
        public void SetYawParameter(float value, float timeScale)
        {
            SetYawParameter(value, timeScale, m_YawDampingTime);
        }

        /// <summary>
        /// Sets the Yaw parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetYawParameter(float value, float timeScale, float dampingTime)
        {
            var change = m_Yaw != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_YawHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    m_Yaw = m_Animator.GetFloat(s_YawHash);
                    if (Mathf.Abs(m_Yaw) < 0.001f) {
                        m_Yaw = 0;
                    }
                } else {
                    m_Yaw = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetYawParameter(value, timeScale, dampingTime);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Speed parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        public void SetSpeedParameter(float value, float timeScale)
        {
            SetSpeedParameter(value, timeScale, 0);
        }

        /// <summary>
        /// Sets the Speed parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetSpeedParameter(float value, float timeScale, float dampingTime)
        {
            var change = m_Speed != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_SpeedHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    m_Speed = m_Animator.GetFloat(s_SpeedHash);
                    if (Mathf.Abs(m_Speed) < 0.001f) {
                        m_Speed = 0;
                    }
                } else {
                    m_Speed = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetSpeedParameter(value, timeScale, dampingTime);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Height parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetHeightParameter(int value)
        {
            var change = m_Height != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_HeightHash, value, 0, 0);
                    m_Height = (int)m_Animator.GetFloat(s_HeightHash);
                    if (Mathf.Abs(m_Height) < 0.001f) {
                        m_Height = 0;
                    }
                } else {
                    m_Height = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetHeightParameter(value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Moving parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetMovingParameter(bool value)
        {
            var change = m_Moving != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetBool(s_MovingHash, value);
                }
                m_Moving = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetMovingParameter(value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Aiming parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAimingParameter(bool value)
        {
            var change = m_Aiming != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetBool(s_AimingHash, value);
                }
                m_Aiming = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetAimingParameter(value);
                    }
                }
            }
            return change;
        }

        /// <summary>
        /// Sets the Movement Set ID parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetMovementSetIDParameter(int value)
        {
            var change = m_MovementSetID != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetInteger(s_MovementSetIDHash, value);
                }
                m_MovementSetID = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetMovementSetIDParameter(value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Ability Index parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAbilityIndexParameter(int value)
        {
            var change = m_AbilityIndex != value;
            if (change) {
#if UNITY_EDITOR
                if (m_LogAbilityParameterChanges) {
                    Debug.Log(Time.frameCount + " Changed AbilityIndex to " + value + ".");
                }
#endif
                if (m_Animator != null) {
                    m_Animator.SetInteger(s_AbilityIndexHash, value);
                    SetAbilityChangeParameter(true);
                }
                m_AbilityIndex = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetAbilityIndexParameter(value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Ability Change parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAbilityChangeParameter(bool value)
        {
            if (m_Animator != null && m_Animator.GetBool(s_AbilityChangeHash) != value) {
                if (value) {
                    m_Animator.SetTrigger(s_AbilityChangeHash);
                } else {
                    m_Animator.ResetTrigger(s_AbilityChangeHash);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the Int Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAbilityIntDataParameter(int value)
        {
            var change = m_AbilityIntData != value;
            if (change) {
#if UNITY_EDITOR
                if (m_LogAbilityParameterChanges) {
                    Debug.Log(Time.frameCount + " Changed AbilityIntData to " + value + ".");
                }
#endif
                if (m_Animator != null) {
                    m_Animator.SetInteger(s_AbilityIntDataHash, value);
                }
                m_AbilityIntData = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetAbilityIntDataParameter(value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Float Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        public void SetAbilityFloatDataParameter(float value, float timeScale)
        {
            SetAbilityFloatDataParameter(value, timeScale, 0);
        }

        /// <summary>
        /// Sets the Float Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAbilityFloatDataParameter(float value, float timeScale, float dampingTime)
        {
            var change = m_AbilityFloatData != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_AbilityFloatDataHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    m_AbilityFloatData = m_Animator.GetFloat(s_AbilityFloatDataHash);
                } else {
                    m_AbilityFloatData = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetAbilityFloatDataParameter(value, timeScale, dampingTime);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Item ID parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        public virtual bool SetItemIDParameter(int slotID, int value)
        {
            var change = m_ItemSlotID[slotID] != value;
            if (change) {
#if UNITY_EDITOR
                if (m_LogItemParameterChanges) {
                    Debug.Log(string.Format("{0} Changed Slot{1}ItemID to {2}.", Time.frameCount, slotID, value));
                }
#endif
                if (m_Animator != null) {
                    m_Animator.SetInteger(s_ItemSlotIDHash[slotID], value);
                    // Even though no state index was changed the trigger should be set to true so the animator can transition to the new item id.
                    SetItemStateIndexChangeParameter(slotID, value != 0);
                }
                m_ItemSlotID[slotID] = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetItemIDParameter(slotID, value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Primary Item State Index parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetItemStateIndexParameter(int slotID, int value)
        {
            var change = m_ItemSlotStateIndex[slotID] != value;
            if (change) {
#if UNITY_EDITOR
                if (m_LogItemParameterChanges) {
                    Debug.Log(string.Format("{0} Changed Slot{1}ItemStateIndex to {2}.", Time.frameCount, slotID, value));
                }
#endif
                if (m_Animator != null) {
                    m_Animator.SetInteger(s_ItemSlotStateIndexHash[slotID], value);
                    SetItemStateIndexChangeParameter(slotID, value != 0);
                }
                m_ItemSlotStateIndex[slotID] = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetItemStateIndexParameter(slotID, value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Item State Index Change parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot of that item that should be set.</param>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetItemStateIndexChangeParameter(int slotID, bool value)
        {
            if (m_Animator != null && m_Animator.GetBool(s_ItemSlotStateIndexChangeHash[slotID]) != value) {
                if (value) {
                    m_Animator.SetTrigger(s_ItemSlotStateIndexChangeHash[slotID]);
                } else {
                    m_Animator.ResetTrigger(s_ItemSlotStateIndexChangeHash[slotID]);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the Item Substate Index parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetItemSubstateIndexParameter(int slotID, int value)
        {
            var change = m_ItemSlotSubstateIndex[slotID] != value;
            if (change) {
#if UNITY_EDITOR
                if (m_LogItemParameterChanges) {
                    Debug.Log(string.Format("{0} Changed Slot{1}ItemSubstateIndex to {2}.", Time.frameCount, slotID, value));
                }
#endif
                if (m_Animator != null) {
                    m_Animator.SetInteger(s_ItemSlotSubstateIndexHash[slotID], value);
                }
                m_ItemSlotSubstateIndex[slotID] = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetItemSubstateIndexParameter(slotID, value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Executes an event on the EventHandler.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        public virtual void ExecuteEvent(string eventName)
        {
#if UNITY_EDITOR
            if (m_LogEvents) {
                Debug.Log("Execute " + eventName);
            }
#endif
            EventHandler.ExecuteEvent(m_GameObject, eventName);
        }

        /// <summary>
        /// The specified item will be equipped.
        /// </summary>
        /// <param name="item">The item that will be equipped.</param>
        /// <param name="slotID">The slot that the item will occupy.</param>
        private void OnWillEquipItem(Item item, int slotID)
        {
            m_EquippedItems[slotID] = item;
            m_EquippedItemsDirty = true;
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="item">The item that was unequipped.</param>
        /// <param name="slotID">The slot that the item was unequipped from.</param>
        private void OnUnequipItem(Item item, int slotID)
        {
            if (item != m_EquippedItems[slotID]) {
                return;
            }

            m_EquippedItems[slotID] = null;
            m_EquippedItemsDirty = true;
        }

        /// <summary>
        /// Updates the ItemID and MovementSetID parameters to the equipped items.
        /// </summary>
        public void UpdateItemIDParameters()
        {
            if (m_EquippedItemsDirty) {
                var movementSetID = 0;
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    var itemID = 0;
                    if (m_EquippedItems[i] != null) {
                        if (m_EquippedItems[i].DominantItem) {
                            movementSetID = m_EquippedItems[i].AnimatorMovementSetID;
                        }
                        itemID = m_EquippedItems[i].AnimatorItemID;
                    }
                    SetItemIDParameter(i, itemID);
                }
                SetMovementSetIDParameter(movementSetID);
                m_EquippedItemsDirty = false;
            }
        }

        /// <summary>
        /// The character has changed between Update and FixedUpdate location.
        /// </summary>
        /// <param name="fixedUpdate">Should the Animator update within the FixedUpdate loop?</param>
        private void OnChangeUpdateLocation(bool fixedUpdate)
        {
            m_Animator.updateMode = fixedUpdate ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal;
        }

        /// <summary>
        /// The character's local timescale has changed.
        /// </summary>
        /// <param name="timeScale">The new timescale.</param>
        private void OnChangeTimeScale(float timeScale)
        {
            m_Animator.speed = timeScale;
        }

        /// <summary>
        /// Enables or disables the Animator.
        /// </summary>
        /// <param name="enable">Should the animator be enabled?</param>
        public void EnableAnimator(bool enable)
        {
            m_Animator.enabled = enable;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<Item, int>(m_GameObject, "OnAbilityWillEquipItem", OnWillEquipItem);
            EventHandler.UnregisterEvent<Item, int>(m_GameObject, "OnAbilityUnequipItemComplete", OnUnequipItem);
            EventHandler.UnregisterEvent<Item, int>(m_GameObject, "OnInventoryRemoveItem", OnUnequipItem);
            if (m_Animator != null) {
                EventHandler.UnregisterEvent(m_GameObject, "OnCharacterSnapAnimator", SnapAnimator);
                EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterChangeUpdateLocation", OnChangeUpdateLocation);
                EventHandler.UnregisterEvent<float>(m_GameObject, "OnCharacterChangeTimeScale", OnChangeTimeScale);
            }
        }

#if UNITY_2019_3_OR_NEWER
        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReset()
        {
            s_ItemSlotIDHash = null;
            s_ItemSlotStateIndexHash = null;
            s_ItemSlotStateIndexChangeHash = null;
            s_ItemSlotSubstateIndexHash = null;
        }
#endif
    }
}