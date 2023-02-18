/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Items
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Motion;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.FirstPersonController.Character;
    using Opsive.UltimateCharacterController.FirstPersonController.Character.Identifiers;
    using UnityEngine;

    /// <summary>
    /// Component which represents the item object actually rendererd.
    /// </summary>
    public class FirstPersonPerspectiveItem : PerspectiveItem
    {
        [Tooltip("The ID of the First Person Base Object that the item should be spawned under.")]
        [SerializeField] protected int m_FirstPersonBaseObjectID;
        [Tooltip("The GameObject of the visible item.")]
        [SerializeField] protected GameObject m_VisibleItem;
#if ULTIMATE_CHARACTER_CONTROLLER_VR
        [Tooltip("Should the pivot be positioned as the immediate parent of the VisibleItem? This is useful for VR.")]
        [SerializeField] protected bool m_VRHandParent;
#endif
        [Tooltip("Any additional objects that the item should control the location of. This is useful for dual wielding where the item should update the location of another base object.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_AdditionalObjects")]
        [SerializeField] protected GameObject[] m_AdditionalControlObjects;
        [Tooltip("Any additional object IDs that the item should control the location of. This is useful for dual wielding where the item should update the location of another base object.")]
        [SerializeField] protected int[] m_AdditionalControlObjectBaseIDs;
        [Tooltip("The positional spring used for regular movement (bob, sway, etc).")]
        [SerializeField] protected Spring m_PositionSpring = new Spring();
        [Tooltip("An offset relative to the parent pivot Transform. This position is where the item \"wants to be\". It will try to go back to this position after any forces are applied.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("An offset relative to the parent pivot Transform. This is the desired position when the item is moving off screen (in the case of unequipping).")]
        [SerializeField] protected Vector3 m_PositionExitOffset;
        [Tooltip("Determines how much the item will be pushed down when the player falls onto a surface.")]
        [SerializeField] protected float m_PositionFallImpact = 0.2f;
        [Tooltip("The number of frames over which to even out each the fall impact force.")]
        [SerializeField] protected int m_PositionFallImpactSoftness = 10;
        [Tooltip("Makes the weapon pull backward while falling.")]
        [SerializeField] protected float m_PositionFallRetract = 2;
        [Tooltip("Sliding moves the item in different directions depending on the character movement direction. " +
                 "X slides the item sideways when strafing. " +
                 "Y slides the item down when strafing. " +
                 "Z slides the item forward or backward when walking.")]
        [SerializeField] protected Vector3 m_PositionMoveSlide = new Vector3(0.5f, 1, 0.5f);
        [Tooltip("Sliding which moves the item in different directions depending on the moving paltform's velocity.")]
        [SerializeField] protected Vector3 m_PositionPlatformSlide = new Vector3(0.5f, 0.5f, 0);
        [Tooltip("A tweak parameter that can be used if the spring motion goes out of hand after changing character velocity. Use this slider to scale back the spring motion without having to adjust lots of values.")]
        [SerializeField] protected float m_PositionInputVelocityScale = 1;
        [Tooltip("A cap on the velocity value being fed into the item swaying method, preventing the item from flipping out when the character travels at excessive speeds " +
                 "(such as when affected by a jump pad or a speed boost). This also affects vertical sway.")]
        [SerializeField] protected float m_PositionMaxInputVelocity = 25;
        
        [Tooltip("The rotational spring used for the regular movement (bob, sway, etc).")]
        [SerializeField] protected Spring m_RotationSpring = new Spring(0.5f, 0.2f);
        [Tooltip("An offset relative to the parent pivot Transform. This rotation is where the item \"wants to be\". It will try to go back to this rotation after any forces are applied.")]
        [SerializeField] protected Vector3 m_RotationOffset;
        [Tooltip("An offset relative to the parent pivot Transform. This is the target rotation when the item is moving off screen (in the case of unequipping).")]
        [SerializeField] protected Vector3 m_RotationExitOffset = new Vector3(30, 0, 0);
        [Tooltip("Determines how much the item will roll when the player falls onto a surface.")]
        [SerializeField] protected float m_RotationFallImpact = 10f;
        [Tooltip("The number of frames over which to even out each the fall impact force.")]
        [SerializeField] protected int m_RotationFallImpactSoftness = 5;
        [Tooltip("This setting determines how much the item sways (rotates) in reaction to input movements. Horizontal and vertical movements will sway the weapon spring " +
                 "around the Y and X vectors, respectively. Rotation around the Z vector is hooked to horizontal movement, which is very useful for swaying long melee items " +
                 "such as swords or clubs.")]
        [SerializeField] protected Vector3 m_RotationLookSway = new Vector3(20, 60, 0);
        [Tooltip("Rotation strafe sway rotates the item in different directions depending on character movement direction. " +
                 "X rotates the item up when strafing (it can only rotate up). " +
                 "Y rotates the item sideways when strafing. " +
                 "Z twists the item around the forward vector when strafing.")]
        [SerializeField] protected Vector3 m_RotationStrafeSway = new Vector3(0, 1, 0);
        [Tooltip("This setting rotates the item in response to vertical motion (e.g. falling or walking in stairs). Rotations will have opposing direction when falling versus rising. " +
                 "However, the weapon will only rotate around the Z axis while moving downwards / falling.")]
        [SerializeField] protected Vector3 m_RotationVerticalSway = new Vector3(-200, 0, 0);
        [Tooltip("Sliding which rotates the item in different directions depending on the moving paltform's velocity.")]
        [SerializeField] protected Vector3 m_RotationPlatformSway = new Vector3(0, 0, 0);
        [Tooltip("This parameter reduces the effect of item vertical sway when moving on the ground. At a value of 1 the item behaves as if the player behaves normally. " +
                 "A value of 0 will disable vertical sway altogether when the character is grounded.")]
        [SerializeField] protected float m_RotationGroundSwayMultiplier = 0.5f;
        [Tooltip("A tweak parameter that can be used to temporarily alter the impact of input motion on the item rotation spring, for example in a special player state.")]
	    [SerializeField] protected float m_RotationInputVelocityScale = 1;
        [Tooltip("A cap on the velocity value being fed into the item swaying method, preventing the item from flipping out when extreme mouse sensitivities are being used.")]
	    [SerializeField] protected float m_RotationMaxInputVelocity = 15;
        
        [Tooltip("The positional spring used for the item pivot Transform.")]
        [SerializeField] protected Spring m_PivotPositionSpring = new Spring();
        [Tooltip("An offset relative to the parent First Person Object Transform.")]
        [SerializeField] protected Vector3 m_PivotPositionOffset;
        [Tooltip("The rotational spring used for the item pivot Transform.")]
        [SerializeField] protected Spring m_PivotRotationSpring = new Spring(0.05f, 0.2f);
        [Tooltip("An offset relative to the parent First Person Object Transform.")]
        [SerializeField] protected Vector3 m_PivotRotationOffset;

        [Tooltip("Determines the shaking speed of the item.")]
        [SerializeField] protected float m_ShakeSpeed = 0.15f;
        [Tooltip("The strength of the angular item shake around the X, Y and Z vectors.")]
        [SerializeField] protected Vector3 m_ShakeAmplitude = new Vector3(4, 2, 1);

        [Tooltip("The rate that the item changes its position while the character is moving. Tip: y should be (x * 2) for a nice classic curve of motion.")]
        [SerializeField] protected Vector3 m_BobPositionalRate = new Vector3(0.7f, 1.4f, 0.0f);
        [Tooltip("The strength of the positional item bob. Determines how far the item swings in each respective direction. Tip: make x and y negative to invert the curve.")]
        [SerializeField] protected Vector3 m_BobPositionalAmplitude = new Vector3(0.7f, 2.1f, 0.0f);
        [Tooltip("The rate that the item changes its rotation value while the character is moving.")]
        [SerializeField] protected Vector3 m_BobRotationalRate = new Vector3(0.7f, 0, 0);
        [Tooltip("The strength of the rotation within the item bob. Determines how far the item tilts in each respective direction.")]
        [SerializeField] protected Vector3 m_BobRotationalAmplitude = new Vector3(400, 0, 0);
        [Tooltip("A tweak parameter that can be used if the bob motion goes out of hand after changing player velocity.")]
        [SerializeField] protected float m_BobInputVelocityScale = 1;
        [Tooltip("A cap on the velocity value being fed into the bob function, preventing the item from excessive movement when the character travels at high speeds.")]
        [SerializeField] protected float m_BobMaxInputVelocity = 100;
        [Tooltip("Determines whether the bob should stay in effect only when the character is on the ground.")]
        [SerializeField] protected bool m_BobRequireGroundContact = true;
        
        [Tooltip("Sets the minimum squared controller velocity at which footstep impacts will occur on the item. The system will be disabled if is zero (default).")]
        [SerializeField] protected float m_StepMinVelocity = 5;
        [Tooltip("The number of frames over which to even out each footstep impact. A higher number will make the footstep softer (more like regular bob). A lower number will be more 'snappy'.")]
        [SerializeField] protected int m_StepSoftness = 4;
        [Tooltip("A vector relative to the item determining the amount of force it will receive upon each footstep. Note that this parameter is very sensitive. A typical value is less than 0.01.")]
        [SerializeField] protected Vector3 m_StepPositionForce = new Vector3(0, -0.0012f, 0);
        [Tooltip("Determines the amount of angular force the item will receive upon each footstep. Note that this parameter is very sensitive. A typical value is less than 0.01.")]
        [SerializeField] protected Vector3 m_StepRotationForce = new Vector3(0, 0, -0.0012f);
        [Tooltip("Scales the impact force. It can be used for tweaking the overall force when you are satisfied with the axes' internal relations.")]
        [SerializeField] protected float m_StepForceScale = 1;
        [Tooltip("Simulates shifting weight to the left or right foot, by alternating the positional footstep force every other step. Use this to reduce or enhance the effect of limping.")]
        [SerializeField] protected float m_StepPositionBalance;
        [Tooltip("Simulates shifting weight to the left or right foot, by alternating the angular footstep force every other step. Use this to reduce or enhance the effect of limping.")]
        [SerializeField] protected float m_StepRotationBalance;

        public int FirstPersonBaseObjectID { get { return m_FirstPersonBaseObjectID; } set { m_FirstPersonBaseObjectID = value; } }
        [NonSerialized] public GameObject VisibleItem { get { return m_VisibleItem; } set { m_VisibleItem = value; } }
#if ULTIMATE_CHARACTER_CONTROLLER_VR
        [NonSerialized] public bool VRHandParent { get { return m_VRHandParent; } set { m_VRHandParent = value; } }
#endif
        public GameObject[] AdditionalControlObjects { get { return m_AdditionalControlObjects; } }
        public Spring PositionSpring { get { return m_PositionSpring; }
            set {
                m_PositionSpring = value;
                if (Application.isPlaying && m_PositionSpring != null) { m_PositionSpring.Initialize(false, true); RefreshSprings(); }
            }
        }
        public Vector3 PositionOffset
        { get { return m_PositionOffset; }
            set {
                m_PositionOffset = value;
                if (Application.isPlaying && m_PositionSpring != null) { RefreshSprings(); }
            }
        }
        public Vector3 PositionExitOffset { get { return m_PositionExitOffset; }
            set {
                m_PositionExitOffset = value;
                if (Application.isPlaying && m_PositionSpring != null) { RefreshSprings(); }
            }
        }
        public float PositionFallImpact { get { return m_PositionFallImpact; } set { m_PositionFallImpact = value; } }
        public float PositionFallRetract { get { return m_PositionFallRetract; } set { m_PositionFallRetract = value; } }
        public Vector3 PositionMoveSlide { get { return m_PositionMoveSlide; } set { m_PositionMoveSlide = value; } }
        public Vector3 PositionPlatformSlide { get { return m_PositionPlatformSlide; } set { m_PositionPlatformSlide = value; } }
        public float PositionInputVelocityScale { get { return m_PositionInputVelocityScale; } set { m_PositionInputVelocityScale = value; } }
        public float PositionMaxInputVelocity { get { return m_PositionMaxInputVelocity; } set { m_PositionMaxInputVelocity = value; } }
        public Vector3 RotationOffset { get { return m_RotationOffset; } set { m_RotationOffset = value; if (Application.isPlaying && m_RotationSpring != null) { RefreshSprings(); } } }
        public Vector3 RotationExitOffset { get { return m_RotationExitOffset; } set { m_RotationExitOffset = value; if (Application.isPlaying && m_RotationSpring != null) { RefreshSprings(); } } }
        public Spring RotationSpring { get { return m_RotationSpring; }
            set {
                m_RotationSpring = value;
                if (Application.isPlaying && m_RotationSpring != null) { m_RotationSpring.Initialize(true, true); RefreshSprings(); }
            }
        }
        public float RotationFallImpact { get { return m_RotationFallImpact; } set { m_RotationFallImpact = value; } }
        public int RotationFallImpactSoftness { get { return m_RotationFallImpactSoftness; } set { m_RotationFallImpactSoftness = value; } }
        public Vector3 RotationLookSway { get { return m_RotationLookSway; } set { m_RotationLookSway = value; } }
        public Vector3 RotationStrafeSway { get { return m_RotationStrafeSway; } set { m_RotationStrafeSway = value; } }
        public Vector3 RotationVerticalSway { get { return m_RotationVerticalSway; } set { m_RotationVerticalSway = value; } }
        public Vector3 RotationPlatformSway { get { return m_RotationPlatformSway; } set { m_RotationPlatformSway = value; } }
        public float RotationGroundSwayMultiplier { get { return m_RotationGroundSwayMultiplier; } set { m_RotationGroundSwayMultiplier = value; } }
        public float RotationInputVelocityScale { get { return m_RotationInputVelocityScale; } set { m_RotationInputVelocityScale = value; } }
        public float RotationMaxInputVelocity { get { return m_RotationMaxInputVelocity; } set { m_RotationMaxInputVelocity = value; } }
        public Spring PivotPositionSpring { get { return m_PivotPositionSpring; }
            set {
                m_PivotPositionSpring = value;
                if (Application.isPlaying && m_PivotPositionSpring != null) { m_PivotPositionSpring.Initialize(false, true); RefreshSprings(); }
            }
        }
        public Vector3 PivotPositionOffset
        { get { return m_PivotPositionOffset; }
            set {
                m_PivotPositionOffset = value;
                if (Application.isPlaying && m_PivotPositionSpring != null) { RefreshSprings(); }
            }
        }
        public Spring PivotRotationSpring { get { return m_PivotRotationSpring; }
            set {
                m_PivotRotationSpring = value;
                if (Application.isPlaying && m_PivotRotationSpring != null) { m_PivotRotationSpring.Initialize(true, true); RefreshSprings(); }
            }
        }
        public Vector3 PivotRotationOffset
        { get { return m_PivotRotationOffset; }
            set {
                m_PivotRotationOffset = value;
                if (m_PivotRotationSpring != null) { m_PivotRotationSpring.RestValue = m_PivotRotationOffset; }
            }
        }
        public float ShakeSpeed { get { return m_ShakeSpeed; } set { m_ShakeSpeed = value; } }
        public Vector3 ShakeAmplitude { get { return m_ShakeAmplitude; } set { m_ShakeAmplitude = value; } }
        public Vector3 BobPositionalRate { get { return m_BobPositionalRate; } set { m_BobPositionalRate = value; } }
        public Vector3 BobPositionalAmplitude { get { return m_BobPositionalAmplitude; } set { m_BobPositionalAmplitude = value; } }
        public Vector3 BobRotationalRate { get { return m_BobRotationalRate; } set { m_BobRotationalRate = value; } }
        public Vector3 BobRotationalAmplitude { get { return m_BobRotationalAmplitude; } set { m_BobRotationalAmplitude = value; } }
        public float BobInputVelocityScale { get { return m_BobInputVelocityScale; } set { m_BobInputVelocityScale = value; } }
        public float BobMaxInputVelocity { get { return m_BobMaxInputVelocity; } set { m_BobMaxInputVelocity = value; } }
        public bool BobRequireGroundContact { get { return m_BobRequireGroundContact; } set { m_BobRequireGroundContact = value; } }
        public float StepMinVelocity { get { return m_StepMinVelocity; } set { m_StepMinVelocity = value; } }
        public int StepSoftness { get { return m_StepSoftness; } set { m_StepSoftness = value; } }
        public Vector3 StepPositionForce { get { return m_StepPositionForce; } set { m_StepPositionForce = value; } }
        public Vector3 StepRotationForce { get { return m_StepRotationForce; } set { m_StepRotationForce = value; } }
        public float StepForceScale { get { return m_StepForceScale; } set { m_StepForceScale = value; } }
        public float StepPositionBalance { get { return m_StepPositionBalance; } set { m_StepPositionBalance = value; } }
        public float StepRotationBalance { get { return m_StepRotationBalance; } set { m_StepRotationBalance = value; } }

        private UltimateCharacterLocomotion m_CharacterLocomotion;
        protected Transform m_CharacterTransform;
        private Transform m_ObjectTransform;
        private Transform m_PivotTransform;
        private FirstPersonObjects m_FirstPersonObjects;
        private Transform[] m_AdditionalControlObjectsTransform;

        private bool m_IndependentItem = true;
        private Vector2 m_InputVector;
        private Vector3 m_PositionForce;
        private Vector3 m_RotationForce;
        private Vector3 m_PrevPlatformMovement;
        private Quaternion m_PrevPlatformRotationMovement = Quaternion.identity;
        private Vector3 m_CurrentBobPositionalValue;
        private float m_PrevBobSpeed;
        private float m_PrevUpBob;
        private bool m_BobWasElevating;
        private bool m_UseSpringExitOffset;
        private bool m_Unequipping;
        private float m_LeanTilt;

        private Vector3 m_PrevPositionSpringValue;
        private Vector3 m_PrevPositionSpringVelocity;
        private Vector3 m_PrevRotationSpringValue;
        private Vector3 m_PrevRotationSpringVelocity;
        private Vector3 m_PrevPivotPositionSpringValue;
        private Vector3 m_PrevPivotPositionSpringVelocity;
        private Vector3 m_PrevPivotRotationSpringValue;
        private Vector3 m_PrevPivotRotationSpringVelocity;

        public Transform PivotTransform { get { return m_PivotTransform; } }
        public override bool FirstPersonItem { get { return true; } }
        public Item Item { get { return m_Item; } }

        /// <summary>
        /// Initialize the perspective item.
        /// </summary>
        /// <param name="character">The character GameObject that the item is parented to.</param>
        /// <returns>True if the item was initialized successfully.</returns>
        public override bool Initialize(GameObject character)
        {
            m_Item = gameObject.GetCachedComponent<Item>();
            m_CharacterLocomotion = character.GetCachedComponent<UltimateCharacterLocomotion>();
            m_CharacterTransform = character.transform;

            // The object can be retrieved dynamically for first person objects.
            FirstPersonObjects firstPersonObjects = null;
            if (m_Object == null) {
                // If the object doesn't have a FirstPersonObjects parent then the object is a completely new arm model.
                firstPersonObjects = GetFirstPersonObjects(character);
                // The character may not have a first person perspective setup.
                if (firstPersonObjects == null) {
                    return false;
                }
                var objTransform = firstPersonObjects.transform;

                // A First Person Base Object ID can specified if there are multiple FirstPersonBaseObjects and the item should be spawned under a particular
                // ItemID within that FirstPersonBaseObject.
                var firstPersonObject = objTransform.GetComponentsInChildren<FirstPersonBaseObject>(true);
                for (int i = 0; i < firstPersonObject.Length; ++i) {
                    if (firstPersonObject[i].ID == m_FirstPersonBaseObjectID) {
                        objTransform = firstPersonObject[i].transform;
                        break;
                    }
                }
                m_Object = objTransform.gameObject;

                // The VisibleItem should be positioned under the object.
                var localScale = m_VisibleItem.transform.localScale;
                var parent = GetSpawnParent(character, m_Item.SlotID, true);
                m_VisibleItem.transform.parent = parent;
                m_VisibleItem.transform.localScale = localScale;
                m_VisibleItem.transform.localPosition = m_LocalSpawnPosition;
                m_VisibleItem.transform.localRotation = Quaternion.Euler(m_LocalSpawnRotation);
            } else if (m_Object.GetComponentInParent<FirstPersonObjects>() == null) {
                // If the object doesn't have a FirstPersonObjects parent then the object is a completely new arm model.
                firstPersonObjects = GetFirstPersonObjects(character);
                if (firstPersonObjects == null) {
                    Debug.LogError("Error: Unable to find the parent FirstPersonObjects component.");
                    return false;
                }
                m_Object.transform.SetParentOrigin(firstPersonObjects.transform);
            } else if (m_VisibleItem != null && !m_VisibleItem.transform.IsChildOf(m_Object.transform)) {
                // The object is being initialized after being destroyed.
                var localScale = m_VisibleItem.transform.localScale;
                var parent = GetSpawnParent(character, m_Item.SlotID, true);
                m_VisibleItem.transform.parent = parent;
                m_VisibleItem.transform.localScale = localScale;
                m_VisibleItem.transform.localPosition = m_LocalSpawnPosition;
                m_VisibleItem.transform.localRotation = Quaternion.Euler(m_LocalSpawnRotation);
            }
            
            if (!base.Initialize(character)) {
                return false;
            }

            // The object needs to have a pivot transform.
            m_ObjectTransform = m_Object.transform;
            var pivotParent = m_ObjectTransform.parent;
            var pivotChild = m_ObjectTransform;
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            if (!m_VRHandParent) {
#endif
                // Manually search for the parent so the child can be remembered.
                while (pivotParent != null) {
                    if (pivotParent.GetComponent<FirstPersonObjects>() != null) {
                        break;
                    }
                    pivotChild = pivotParent;
                    pivotParent = pivotParent.parent;
                }
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            } else if (m_VisibleItem != null) {
                pivotParent = m_VisibleItem.transform.parent;
                pivotChild = m_VisibleItem.transform;
            }
#endif
            // The child will already have a pivot if multiple items have been added to the first person hands.
            if (pivotChild.GetComponent<FirstPersonObjectPivot>() == null) {
                // Create the pivot.
                m_PivotTransform = new GameObject(pivotChild.name + " Pivot", typeof(FirstPersonObjectPivot)).transform;
                m_PivotTransform.parent = pivotParent;
                m_PivotTransform.localPosition = Vector3.zero;
                m_PivotTransform.localRotation = Quaternion.identity;
                pivotChild.parent = m_PivotTransform;
            } else {
                m_PivotTransform = pivotChild;
            }
            m_FirstPersonObjects = m_ObjectTransform.GetComponentInParent<FirstPersonObjects>();

            // The control objects can be loaded from IDs.
            if (m_AdditionalControlObjectBaseIDs.Length > 0 && m_AdditionalControlObjects.Length == 0) {
                if (firstPersonObjects == null) {
                    firstPersonObjects = GetFirstPersonObjects(character);
                }
                var controlObjects = firstPersonObjects.GetComponentsInChildren<FirstPersonBaseObject>(true);
                m_AdditionalControlObjects = new GameObject[m_AdditionalControlObjectBaseIDs.Length];
                for (int i = 0; i < m_AdditionalControlObjectBaseIDs.Length; ++i) {
                    for (int j = 0; j < controlObjects.Length; ++j) {
                        if (m_AdditionalControlObjectBaseIDs[i] == controlObjects[j].ID) {
                            m_AdditionalControlObjects[i] = controlObjects[j].gameObject;
                            break;
                        }
                    }
                    if (m_AdditionalControlObjects[i] == null) {
                        Debug.LogError($"Error: Unable to find the control object with ID {m_AdditionalControlObjectBaseIDs[i]}.");
                    }
                }
            }
            m_AdditionalControlObjectsTransform = new Transform[m_AdditionalControlObjects.Length];
            for (int i = 0; i < m_AdditionalControlObjects.Length; ++i) {
                m_AdditionalControlObjectsTransform[i] = m_AdditionalControlObjects[i].transform;
            }

            // Prep the springs for first run.
            RefreshSprings();

            // Initialize the springs.
            m_PositionSpring.Initialize(false, true);
            m_RotationSpring.Initialize(true, true);
            m_PivotPositionSpring.Initialize(false, true);
            m_PivotRotationSpring.Initialize(true, true);

            // Register for interested events.
            EventHandler.RegisterEvent<float>(m_Character, "OnCharacterLand", OnCharacterLand);
            EventHandler.RegisterEvent<float>(m_Character, "OnCharacterChangeTimeScale", OnChangeTimeScale);
            EventHandler.RegisterEvent<int, Vector3, Vector3, bool>(m_Character, "OnAddSecondaryForce", OnAddPivotForce);
            EventHandler.RegisterEvent<float, float, float>(m_Character, "OnCharacterLean", OnCharacterLean);
            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);

            return true;
        }

        /// <summary>
        /// Returns the character's FirstPersonObjects component.
        /// </summary>
        /// <param name="character">The character that the objects belong to.</param>
        /// <returns>The character's FirstPersonObject's component.</returns>
        private FirstPersonObjects GetFirstPersonObjects(GameObject character)
        {
            var firstPersonObjects = character.GetComponentInChildren<FirstPersonObjects>(true);
            if (firstPersonObjects == null) {
                var camera = UnityEngineUtility.FindCamera(character);
                if (camera != null) {
                    return camera.GetComponentInChildren<FirstPersonObjects>(true);
                }
            }
            return firstPersonObjects;
        }

        /// <summary>
        /// Returns the parent that the VisibleItem object should spawn at.
        /// </summary>
        /// <param name="character">The character that the item should spawn under.</param>
        /// <param name="slotID">The character slot that the VisibleItem object should spawn under.</param>
        /// <param name="parentToItemSlotID">Should the object be parented to the item slot ID?</param>
        /// <returns>The parent that the VisibleItem object should spawn at.</returns>
        protected override Transform GetSpawnParent(GameObject character, int slotID, bool parentToItemSlotID)
        {
            var parent = m_Object.transform;
            if (parentToItemSlotID) {
                // If the ItemSlot component exists then the object should spawn as a child object. This will occur when the hands have already been setup and every weapon should use
                // the same hand model (similar to the third person item workflow).
                var itemSlots = parent.GetComponentsInChildren<ItemSlot>(true);
                for (int i = 0; i < itemSlots.Length; ++i) {
                    if (itemSlots[i].ID == slotID) {
                        parent = itemSlots[i].transform;
                        break;
                    }
                }
            }

            return parent;
        }

        /// <summary>
        /// Starts the perspective item. Will be called after the item has been started.
        /// </summary>
        public override void ItemStarted()
        {
            base.ItemStarted();

            // Independent items will have separate parent objects.
            var allItems = m_Character.GetComponentsInChildren<Item>(true);
            for (int i = 0; i < allItems.Length; ++i) {
                // Independent items are only a concept for first person objects.
                if (m_Item == allItems[i] || allItems[i].FirstPersonPerspectiveItem == null) {
                    continue;
                }

                // Independent items will not share the objects. If the object is shared then the item is not independent and no more interations are necessary.
                if (allItems[i].FirstPersonPerspectiveItem.Object == m_Object) {
                    m_IndependentItem = false;
                    break;
                }
            }

            if (m_IndependentItem) {
                // If the item is independent now it may not be in the future when another item is picked up.
                EventHandler.RegisterEvent<Item>(m_Character, "OnInventoryAddItem", OnAddItem);
            }
            EventHandler.ExecuteEvent(m_Character, "OnFirstPersonPerspectiveActivate", this, IsActive());
        }

        /// <summary>
        /// Is the VisibleItem active?
        /// </summary>
        /// <returns>True if the VisibleItem is active.</param>
        public override bool IsActive()
        {
            var active = true;
            if (m_IndependentItem
#if ULTIMATE_CHARACTER_CONTROLLER_VR
                && !m_VRHandParent
#endif
                ) {
                active = base.IsActive();
            } else if (m_VisibleItem == null) {
                return m_Item.VisibleObjectActive;
            }
            if (m_VisibleItem != null) {
                active = active && m_VisibleItem.activeSelf;
            }

            return active;
        }

        /// <summary>
        /// Activates or deactivates the VisibleItem.
        /// </summary>
        /// <param name="active">Should the VisibleItem be activated?</param>
        public override void SetActive(bool active)
        {
            // An independent item will have its own pivot and object which has its own animator. If the item is not independent
            // then the pivot/object are shared with other items so only the visible object should be disabled.
            if (m_IndependentItem
#if ULTIMATE_CHARACTER_CONTROLLER_VR
                && !m_VRHandParent
#endif
                ) {
                base.SetActive(active);

                m_PrevPlatformMovement = m_CharacterLocomotion.PlatformMovement;
                m_PrevPlatformRotationMovement = m_CharacterLocomotion.PlatformTorque;
            }
            if (m_VisibleItem != null) {
                m_VisibleItem.SetActive(active);
            }
        }

        /// <summary>
        /// Returns the current VisibleItem object.
        /// </summary>
        /// <returns>The current VisibleItem object.</returns>
        public override GameObject GetVisibleObject()
        {
            if (m_VisibleItem == null) {
                return base.GetVisibleObject();
            }
            return m_VisibleItem;
        }

        /// <summary>
        /// Moves the item according to the horizontal and vertical movement, as well as the character velocity.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        public override void Move(float horizontalMovement, float verticalMovement)
        {
            if (m_CharacterLocomotion.TimeScale == 0 || Time.fixedDeltaTime == 0) {
                return;
            }

            // The same object/pivot may be specified across multiple active items. Only update the movement once to prevent one item from overriding another item.
            if (UnityEngineUtility.HasUpdatedObject(m_Object)) {
                return;
            }
            // The object hasn't been updated this frame. The updated object set will be cleared by the ItemManager after the frame is complete.
            UnityEngineUtility.AddUpdatedObject(m_Object);

            m_InputVector.x = horizontalMovement;
            m_InputVector.y = verticalMovement;
            // Limit rotation velocity to protect against extreme input sensitivity.
            m_InputVector *= m_RotationInputVelocityScale;
            m_InputVector = Vector2.Min(m_InputVector, Vector2.one * m_RotationMaxInputVelocity);
            m_InputVector = Vector2.Max(m_InputVector, Vector2.one * -m_RotationMaxInputVelocity);

            m_PositionForce = m_RotationForce = Vector3.zero;

            // Apply a sway based on the input vector and character velocity.
            UpdateSway();

            // Apply a bob based on the character velocity.
            UpdateBob();

            // Step according to the character velocity.
            UpdateStep();

            // Adds smooth random movement.
            UpdateShake();

            // Apply the resulting position and rotation changes.
            ApplyMovement();
        }

        /// <summary>
        /// Updates the positional and rotational forces when the camera or character moves.
        /// </summary>
        private void UpdateSway()
        {
            Vector3 rotationForce = Vector3.zero, positionForce = Vector3.zero;

            // Sway the item rotation based off of the input vector.
            rotationForce.Set(m_InputVector.y * (m_RotationLookSway.x * 0.025f), m_InputVector.x * (m_RotationLookSway.y * -0.025f), m_InputVector.magnitude * (m_RotationLookSway.z * -0.025f));

            // Determine a sway velocity based off of the character's local velocity. 
            // This will allow the item to change positions and rotations based off of the movement direction while protecting against extreme speeds.
            var swayVelocity = m_CharacterLocomotion.LocalLocomotionVelocity * m_PositionInputVelocityScale;
            swayVelocity = Vector3.Min(swayVelocity, Vector3.one * m_PositionMaxInputVelocity);
            swayVelocity = Vector3.Max(swayVelocity, Vector3.one * -m_PositionMaxInputVelocity);
            swayVelocity *= m_CharacterLocomotion.TimeScale * Time.timeScale;

            // Rotate the item while changing vertical positions. The item will only rotate around the local z vector while moving down.
            var verticalSway = m_RotationVerticalSway * -swayVelocity.y * 0.005f;
            verticalSway.z *= Random.value > 0.5f ? 1 : -1;
            // Optionally reduce the amount of vertical sway while on the ground.
            if (m_CharacterLocomotion.Grounded) {
                verticalSway *= m_RotationGroundSwayMultiplier;
            }
            verticalSway.z = Mathf.Max(0, verticalSway.z);
            rotationForce += verticalSway;

            // Rotate the item while strafing.
            // m_RotationStrafeSway.x will rotate up when strafing (it can't rotate down).
            // m_RotationStrafeSway.y will rotate sideways when strafing.
            // m_RotationStrafeSway.z will twist weapon around the forward vector when strafing.
            var strafeValue = Vector3.zero;
            strafeValue.Set(-Mathf.Abs(swayVelocity.x * (m_RotationStrafeSway.x * 0.16f)), -(swayVelocity.x * (m_RotationStrafeSway.y * 0.16f)), swayVelocity.x * (m_RotationStrafeSway.z * 0.16f));
            rotationForce += strafeValue;

            // Slide the item position while strafing.
            // m_PositionMoveSlide.x will slide sideways when strafing.
            // m_PositionMoveSlide.y will slide down when strafing (it can't slide up).
            // m_PositionMoveSlide.z will slide forward or backward when walking.
            strafeValue.Set(-swayVelocity.x * (m_PositionMoveSlide.x * 0.0016f), -swayVelocity.x * (m_PositionMoveSlide.y * 0.0016f), -swayVelocity.z * (m_PositionMoveSlide.z * 0.0016f));
            positionForce += strafeValue;

            // Move the item towards the camera while moving.
            positionForce += Vector3.forward * -Mathf.Abs(swayVelocity.y * m_PositionFallRetract * 0.000025f);

            // Move/rotate with the moving platform.
            var platformPositionalDiff = m_CharacterTransform.InverseTransformDirection(m_PrevPlatformMovement - m_CharacterLocomotion.PlatformMovement);
            strafeValue.Set(platformPositionalDiff.x * m_PositionPlatformSlide.x * 0.16f, platformPositionalDiff.y * m_PositionPlatformSlide.y * 0.16f, platformPositionalDiff.z * m_PositionPlatformSlide.z * 0.16f);
            positionForce += strafeValue;
            var platformEulerDiff = MathUtility.InverseTransformQuaternion(m_CharacterTransform.rotation, (m_CharacterLocomotion.PlatformTorque * Quaternion.Inverse(m_PrevPlatformRotationMovement))).eulerAngles;
            strafeValue.Set(platformEulerDiff.x * m_RotationPlatformSway.x * 0.16f, platformEulerDiff.y * m_RotationPlatformSway.y * 0.16f, platformEulerDiff.z * m_RotationPlatformSway.z * 0.16f);
            rotationForce += strafeValue;

            m_PrevPlatformMovement = m_CharacterLocomotion.PlatformMovement;
            m_PrevPlatformRotationMovement = m_CharacterLocomotion.PlatformTorque;

            // Add the final forces.
            m_RotationForce += rotationForce;
            m_PositionForce += positionForce;
        }

        /// <summary>
        /// Bobbing is the sinusoidal motion of the item hooked into the to character controller velocity.
        /// </summary>
        private void UpdateBob()
        {
            if ((m_BobPositionalRate == Vector3.zero || m_BobPositionalAmplitude == Vector3.zero) && (m_BobRotationalRate == Vector3.zero|| m_BobRotationalAmplitude == Vector3.zero)) {
                return;
            }

            var bobSpeed = ((m_BobRequireGroundContact && !m_CharacterLocomotion.Grounded) ? 0 : m_CharacterLocomotion.LocomotionVelocity.sqrMagnitude);

            // Scale and limit the input velocity.
            bobSpeed = Mathf.Min(bobSpeed * m_BobInputVelocityScale, m_BobMaxInputVelocity);

            // Reduce the number of decimals to avoid floating point imprecision issues.
            bobSpeed = Mathf.Round(bobSpeed * 1000f) / 1000f;

            // If the bob speed is zero then fade out the last speed value. It is important to clamp the speed to the 
            // last bob speed value because a preset may have changed since the last last bob.
            if (bobSpeed == 0) {
                bobSpeed = Mathf.Min((m_PrevBobSpeed * 0.93f), m_BobMaxInputVelocity);
            }

            // Update the positional and rotational bob value.
            var currentPositionalBobAmplitude = (bobSpeed * (m_BobPositionalAmplitude * -0.0001f));
            m_CurrentBobPositionalValue.x = Mathf.Cos(m_BobPositionalRate.x * 10.0f * m_CharacterLocomotion.TimeScale * Time.time) * currentPositionalBobAmplitude.x;
            m_CurrentBobPositionalValue.y = Mathf.Cos(m_BobPositionalRate.y * 10.0f * m_CharacterLocomotion.TimeScale * Time.time) * currentPositionalBobAmplitude.y;
            m_CurrentBobPositionalValue.z = Mathf.Cos(m_BobPositionalRate.z * 10.0f * m_CharacterLocomotion.TimeScale * Time.time) * currentPositionalBobAmplitude.z;

            var currentRotationalBobValue = (bobSpeed * (m_BobRotationalAmplitude * -0.0001f));
            currentRotationalBobValue.x = Mathf.Cos(m_BobRotationalRate.x * 10.0f * m_CharacterLocomotion.TimeScale * Time.time) * currentRotationalBobValue.x;
            currentRotationalBobValue.y = Mathf.Cos(m_BobRotationalRate.y * 10.0f * m_CharacterLocomotion.TimeScale * Time.time) * currentRotationalBobValue.y;
            currentRotationalBobValue.z = Mathf.Cos(m_BobRotationalRate.z * 10.0f * m_CharacterLocomotion.TimeScale * Time.time) * currentRotationalBobValue.z;

            // Add the bob value to the positional and rotational spring.
            m_PositionForce += m_CurrentBobPositionalValue;
            m_RotationForce += currentRotationalBobValue;
            m_PrevBobSpeed = bobSpeed;
        }

        /// <summary>
        /// The step feature applies force to the item springs in order to simulate a fine footstep impact in sync with the item bob. 
        /// A footstep force is triggered every time the vertical weapon bob reaches its bottom value.
        /// </summary>
        private void UpdateStep()
        {
            if (m_StepMinVelocity <= 0 || (m_BobRequireGroundContact && !m_CharacterLocomotion.Grounded) || m_CharacterLocomotion.LocomotionVelocity.sqrMagnitude < m_StepMinVelocity) {
                return;
            }

            var elevating = m_PrevUpBob < m_CurrentBobPositionalValue.y;
            m_PrevUpBob = m_CurrentBobPositionalValue.y;

            // Add a soft down force to the item if the bob is dipping.
            if (elevating && !m_BobWasElevating) {
                // Apply a soft footstep force on the item's position and rotation springs, and multiply the footstep force depending on the current foot.
                // Tip: This can be used to reduce or enhance the effect of limping.
                Vector3 positionForce, rotationForce;
                if (Mathf.Cos(m_CharacterLocomotion.TimeScale * Time.time * (m_BobPositionalRate.x * 5)) > 0) {
                    positionForce = m_StepPositionForce - (m_StepPositionForce * m_StepPositionBalance);
                    rotationForce = m_StepRotationForce - (m_StepPositionForce * m_StepRotationBalance);
                } else {
                    positionForce = m_StepPositionForce + (m_StepPositionForce * m_StepPositionBalance);
                    rotationForce = Vector3.Scale(m_StepRotationForce - (m_StepPositionForce * m_StepRotationBalance),
                                                    -Vector3.one + (Vector3.right * 2)); // Invert the y and z rotation.
                }

                m_PivotPositionSpring.AddForce(positionForce * m_StepForceScale * m_CharacterLocomotion.TimeScale * Time.timeScale, m_StepSoftness);
                m_PivotRotationSpring.AddForce(rotationForce * m_StepForceScale * m_CharacterLocomotion.TimeScale * Time.timeScale, m_StepSoftness);
            }

            m_BobWasElevating = elevating;
        }

        /// <summary>
        /// This is procedural item rotation shaking, intended as a purely aesthetic motion to breathe life into the item. 
        /// Super-useful for idle motions, but can also be used to rattle the item from wind turbulence when skydiving!
        /// </summary>
        private void UpdateShake()
        {
            if (m_ShakeSpeed == 0) {
                return;
            }

            var shake = Vector3.Scale(SmoothRandom.GetVector3Centered(m_ShakeSpeed * m_CharacterLocomotion.TimeScale), m_ShakeAmplitude);
            m_RotationForce += shake;
        }

        /// <summary>
        /// Applies the position and rotation springs to the Transform.
        /// </summary>
        private void ApplyMovement()
        {
            m_PivotPositionSpring.AddForce(m_PositionForce);
            m_PivotRotationSpring.AddForce(m_RotationForce);

#if ULTIMATE_CHARACTER_CONTROLLER_VR
            if (m_VRHandParent) {
                // The ObjectTransform will be controlled by the HandHandler.
                m_PivotTransform.localPosition = m_PositionSpring.Value;
                m_PivotTransform.localEulerAngles = m_RotationSpring.Value;
            } else {
#endif
                m_ObjectTransform.localPosition = m_PositionSpring.Value;
                m_ObjectTransform.localEulerAngles = m_RotationSpring.Value;
                m_PivotTransform.localPosition = m_PivotPositionSpring.Value;
                m_PivotTransform.localEulerAngles = m_PivotRotationSpring.Value;
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            }
#endif

            for (int i = 0; i < m_AdditionalControlObjectsTransform.Length; ++i) {
                m_AdditionalControlObjectsTransform[i].position = m_ObjectTransform.position;
                m_AdditionalControlObjectsTransform[i].rotation = m_ObjectTransform.rotation;
            }
        }

        /// <summary>
        /// Adds a pivot positional and rotational force to the VisibleItem.
        /// </summary>
        /// <param name="slotID">The Slot ID that is adding the secondary force.</param>
        /// <param name="positionalForce">The positional force to add.</param>
        /// <param name="rotationalForce">The rotational force to add.</param>
        /// <param name="globalForce">Is the force applied to the entire character?</param>
        private void OnAddPivotForce(int slotID, Vector3 positionalForce, Vector3 rotationalForce, bool globalForce)
        {
            // The pivot force may be added for another item.
            if (slotID != -1 && slotID != m_Item.SlotID) {
                return;
            }

            m_PivotPositionSpring.AddForce(positionalForce);
            m_PivotRotationSpring.AddForce(rotationalForce);
        }

        /// <summary>
        /// The character has started to lean.
        /// </summary>
        /// <param name="distance">The distance that the character is leaning.</param>
        /// <param name="tilt">The amount of tilt to apply to the lean.</param>
        /// <param name="itemTiltMultiplier">The multiplier to apply to the tilt of an item.</param>
        private void OnCharacterLean(float distance, float tilt, float itemTiltMultiplier)
        {
            m_LeanTilt = tilt * itemTiltMultiplier;

            RefreshSprings();
        }

        /// <summary>
        /// Update the spring values.
        /// </summary>
        private void RefreshSprings()
        {
            if (!Application.isPlaying) {
                return;
            }
            // Set the rest state.
            m_PositionSpring.RestValue = m_UseSpringExitOffset ? m_PositionExitOffset : m_PositionOffset;
            m_RotationSpring.RestValue = m_UseSpringExitOffset ? m_RotationExitOffset : m_RotationOffset;
            m_PivotPositionSpring.RestValue = m_PivotPositionOffset;
            m_PivotRotationSpring.RestValue = (m_LeanTilt * Vector3.forward) + m_PivotRotationOffset;
        }

        /// <summary>
        /// Immediately snaps all of the springs to their resting state.
        /// </summary>
        /// <param name="fromEquip">Are the springs snapping from being equipped?</param>
        private void SnapSprings(bool fromEquip)
        {
            m_PositionSpring.RestValue = m_UseSpringExitOffset ? m_PositionExitOffset : m_PositionOffset;
            m_RotationSpring.RestValue = m_UseSpringExitOffset ? m_RotationExitOffset : m_RotationOffset;
            m_PositionSpring.Reset();
            m_RotationSpring.Reset();
            m_PivotPositionSpring.Reset();
            m_PivotRotationSpring.Reset();

            m_PrevUpBob = 0;
            m_PrevBobSpeed = 0;
            m_BobWasElevating = false;

            // The item shouldn't modify the transform if isn't active or the transform has already been updated.
            if ((!fromEquip && (UnityEngineUtility.HasUpdatedObject(m_Object) || !IsActive())) || !m_Item.DominantItem) {
                return;
            }
            // The object hasn't been updated this frame. The updated object set will be cleared after the frame is complete.
            UnityEngineUtility.AddUpdatedObject(m_Object, true);

            // Apply the snapped spring value.
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            if (m_VRHandParent) {
                m_PivotTransform.localPosition = m_PositionSpring.Value;
                m_PivotTransform.localEulerAngles = m_RotationSpring.Value;
            } else {
#endif
                m_ObjectTransform.localPosition = m_PositionSpring.Value;
                m_ObjectTransform.localEulerAngles = m_RotationSpring.Value;
                m_PivotTransform.localPosition = m_PivotPositionSpring.Value;
                m_PivotTransform.localEulerAngles = m_PivotRotationSpring.Value;
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            }
#endif
        }

        /// <summary>
        /// The item has started to be equipped.
        /// </summary>
        /// <param name="immediateEquip">Is the item being equipped immediately? Immediate equips will occur from the default loadout or quickly switching to the item.</param>
        public override void StartEquip(bool immediateEquip)
        {
            // The FirstPersonObjects component should first be aware of the equip so it can correctly activate the base object.
            m_FirstPersonObjects.StartEquip(m_Item, m_Item.SlotID);

            // Non-dominant items cannot control spring location.
            if (!m_Item.DominantItem) {
                return;
            }

            // If the item isn't in the middle of being unequipped then snap the item to the exit position. This allows the weapon to always be raised from the same location.
            if (!m_Unequipping && !immediateEquip) {
                m_UseSpringExitOffset = true;
                SnapSprings(true);
            }
            m_UseSpringExitOffset = false;
            m_Unequipping = false;
            if (immediateEquip) {
                SnapSprings(true);
            } else {
                RefreshSprings();
            }
        }
        
        /// <summary>
        /// The item has started to be unequipped.
        /// </summary>
        public override void StartUnequip()
        {
            // Non-dominant items cannot control spring location.
            if (!m_Item.DominantItem) {
                return;
            }

            // Move towards the exit offset.
            m_Unequipping = true;
            m_UseSpringExitOffset = true;
            RefreshSprings();
        }

        /// <summary>
        /// The item has been unequipped.
        /// </summary>
        public override void Unequip()
        {
            // The FirstPersonObjects component should first be aware of the unequip so it can correctly deactivate the base object.
            m_FirstPersonObjects.UnequipItem(m_Item, m_Item.SlotID);

            // Non-dominant items cannot control spring location.
            if (!m_Item.DominantItem) {
                return;
            }

            // Prepare the item for when it is equipped again.
            m_Unequipping = false;
            m_UseSpringExitOffset = false;
            RefreshSprings();
        }

        /// <summary>
        /// The inventory has added the specified item.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        private void OnAddItem(Item item)
        {
            if (m_Item == item || item.FirstPersonPerspectiveItem == null) {
                return;
            }

            if (m_Object == item.FirstPersonPerspectiveItem.Object) {
                // Once the item is no longer independent there is no going back to being independent so the item can unregister from the add item event.
                m_IndependentItem = false;
                EventHandler.UnregisterEvent<Item>(m_Character, "OnInventoryAddItem", OnAddItem);
            }
        }

        /// <summary>
        /// The item has been removed.
        /// </summary>        /// <param name="destroy">Should the object be destroyed?</param>
        public override void Remove()
        {
            if (m_IndependentItem
#if ULTIMATE_CHARACTER_CONTROLLER_VR
                && !m_VRHandParent
#endif
                ) {
                base.Remove();
            } else if (m_VisibleItem != null) {
                m_VisibleItem.SetActive(false);
            }

            if (m_Item.DominantItem) {
                m_UseSpringExitOffset = false;
                SnapSprings(false);
            }
        }

        /// <summary>
        /// The character has landed on the ground.
        /// </summary>
        /// <param name="height">The height of the fall.</param>
        private void OnCharacterLand(float height)
        {
            var positionImpact = height * m_PositionFallImpact;
            var rotationImpact = height * m_RotationFallImpact;

            // Apply impact to the item position spring.
            m_PivotPositionSpring.AddForce(Vector3.down * positionImpact, m_PositionFallImpactSoftness);
            // Apply impact to the item rotation spring. Randomize the rotation upon landing.
            m_PivotRotationSpring.AddForce(Vector3.right * rotationImpact, m_RotationFallImpactSoftness);
        }

        /// <summary>
        /// The character's position or rotation has been teleported.
        /// </summary>
        /// <param name="snapAnimator">Should the animator be snapped?</param>
        private void OnImmediateTransformChange(bool snapAnimator)
        {
            SnapSprings(false);
        }

        /// <summary>
        /// The character's local timescale has changed.
        /// </summary>
        /// <param name="timeScale">The new timescale.</param>
        private void OnChangeTimeScale(float timeScale)
        {
            m_PositionSpring.TimeScale = timeScale;
            m_RotationSpring.TimeScale = timeScale;

            m_PivotPositionSpring.TimeScale = timeScale;
            m_PivotRotationSpring.TimeScale = timeScale;
        }

        /// <summary>
        /// Callback when the StateManager will change the active state on the current object.
        /// </summary>
        public override void StateWillChange()
        {
            // Remember the interal spring values so they can be restored if a new spring is applied during the state change.
            m_PrevPositionSpringValue = m_PositionSpring.Value;
            m_PrevPositionSpringVelocity = m_PositionSpring.Velocity;
            m_PrevRotationSpringValue = m_RotationSpring.Value;
            m_PrevRotationSpringVelocity = m_RotationSpring.Velocity;
            m_PrevPivotPositionSpringValue = m_PivotPositionSpring.Value;
            m_PrevPivotPositionSpringVelocity = m_PivotPositionSpring.Velocity;
            m_PrevPivotRotationSpringValue = m_PivotRotationSpring.Value;
            m_PrevPivotRotationSpringVelocity = m_PivotRotationSpring.Velocity;
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            m_PositionSpring.Value = m_PrevPositionSpringValue;
            m_PositionSpring.Velocity = m_PrevPositionSpringVelocity;
            m_RotationSpring.Value = m_PrevRotationSpringValue;
            m_RotationSpring.Velocity = m_PrevRotationSpringVelocity;
            m_PivotPositionSpring.Value = m_PrevPivotPositionSpringValue;
            m_PivotPositionSpring.Velocity = m_PrevPivotPositionSpringVelocity;
            m_PivotRotationSpring.Value = m_PrevPivotRotationSpringValue;
            m_PivotRotationSpring.Velocity = m_PrevPivotRotationSpringVelocity;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (m_Character == null) {
                return;
            }

            m_PositionSpring.Destroy();
            m_RotationSpring.Destroy();
            m_PivotPositionSpring.Destroy();
            m_PivotRotationSpring.Destroy();

            EventHandler.UnregisterEvent<float>(m_Character, "OnCharacterLand", OnCharacterLand);
            EventHandler.UnregisterEvent<float>(m_Character, "OnCharacterChangeTimeScale", OnChangeTimeScale);
            EventHandler.UnregisterEvent<int, Vector3, Vector3, bool>(m_Character, "OnAddSecondaryForce", OnAddPivotForce);
            EventHandler.UnregisterEvent<float, float, float>(m_Character, "OnCharacterLean", OnCharacterLean);
            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
            if (m_IndependentItem) {
                EventHandler.UnregisterEvent<Item>(m_Character, "OnInventoryAddItem", OnAddItem);
            }
        }
    }
}