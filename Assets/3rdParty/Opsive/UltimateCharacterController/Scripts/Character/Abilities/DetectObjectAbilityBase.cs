/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// An abstract class for any ability that needs another object to start (such as picking an object up, vaulting, climbing, interacting, etc).
    /// </summary>
    public abstract class DetectObjectAbilityBase : Ability
    {
        /// <summary>
        /// Specifies how to detect the object.
        /// </summary>
        public enum ObjectDetectionMode
        {
            Trigger = 1,        // Use a trigger to detect if the character is near an object.
            Charactercast = 2,  // Use the character colliders to do a cast in order to detect if the character is near an object.
            Raycast = 4,        // Use a raycast to detect if the character is near an object.
            Spherecast = 8,     // Use a spherecast to detect if the character is near an object.
            Customcast = 16     // The ability will perform its own custom cast.
        }

        [Tooltip("Mask which specifies how the ability should detect other objects.")]
        [HideInInspector] [SerializeField] protected ObjectDetectionMode m_ObjectDetection = ObjectDetectionMode.Charactercast;
        [Tooltip("The LayerMask of the object or trigger that should be detected.")]
        [HideInInspector] [SerializeField] protected LayerMask m_DetectLayers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.UI | 1 << LayerManager.SubCharacter | 1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect);
        [Tooltip("Should the detection method use the look source position? If false the character position will be used.")]
        [HideInInspector] [SerializeField] protected bool m_UseLookPosition;
        [Tooltip("Should the detection method use the look source direction? If false the character direction will be used.")]
        [HideInInspector] [SerializeField] protected bool m_UseLookDirection = true;
        [Tooltip("The maximum angle that the character can be relative to the forward direction of the object.")]
        [Range(0, 360)] [HideInInspector] [SerializeField] protected float m_AngleThreshold = 360;
        [Tooltip("The unique ID value of the Object Identifier component. A value of -1 indicates that this ID should not be used.")]
        [HideInInspector] [SerializeField] protected int m_ObjectID = -1;
        [Tooltip("The distance of the cast. Used if the Object Detection Mode uses anything other then a trigger detection mode.")]
        [HideInInspector] [SerializeField] protected float m_CastDistance = 1;
        [Tooltip("The number of frames that should elapse before another cast is performed. A value of 0 will allow the cast to occur every frame.")]
        [HideInInspector] [SerializeField] protected int m_CastFrameInterval = 0;
        [Tooltip("The offset to applied to the cast.")]
        [HideInInspector] [SerializeField] protected Vector3 m_CastOffset = new Vector3(0, 1, 0);
        [Tooltip("Specifies if the cast should interact with triggers.")]
        [HideInInspector] [SerializeField] protected QueryTriggerInteraction m_TriggerInteraction = QueryTriggerInteraction.Ignore;
        [Tooltip("The radius of the spherecast.")]
        [HideInInspector] [SerializeField] protected float m_SpherecastRadius = 0.5f;
        [Tooltip("The maximum number of valid triggers that the ability can detect.")]
        [HideInInspector] [SerializeField] protected int m_MaxTriggerObjectCount = 1;

        public ObjectDetectionMode ObjectDetection { get { return m_ObjectDetection; }
            set {
                m_ObjectDetection = value;
                if ((m_ObjectDetection & ObjectDetectionMode.Trigger) != 0 && m_DetectedTriggerObjects == null) {
                    m_DetectedTriggerObjects = new GameObject[m_MaxTriggerObjectCount];
                }
            }
        }
        public LayerMask DetectLayers { get { return m_DetectLayers; } set { m_DetectLayers = value; } }
        public float DetectAngleThreshold { get { return m_AngleThreshold; } set { m_AngleThreshold = value; } }
        public int ObjectID { get { return m_ObjectID; } set { m_ObjectID = value; } }
        public bool UseLookPosition { get { return m_UseLookPosition; } set { m_UseLookPosition = value; } }
        public bool UseLookDirection { get { return m_UseLookDirection; } set { m_UseLookDirection = value; } }
        public float CastDistance { get { return m_CastDistance; } set { m_CastDistance = value; } }
        public Vector3 CastOffset { get { return m_CastOffset; } set { m_CastOffset = value; } }
        public QueryTriggerInteraction TriggerInteraction { get { return m_TriggerInteraction; } set { m_TriggerInteraction = value; } }
        public float SpherecastRadius { get { return m_SpherecastRadius; } set { m_SpherecastRadius = value; } }

        protected ILookSource m_LookSource;
        protected Transform m_LookSourceTransform;
        protected RaycastHit m_RaycastResult;
        protected GameObject[] m_DetectedTriggerObjects;
        protected int m_DetectedTriggerObjectsCount;
        protected GameObject m_DetectedObject;
        private int m_LastCastFrame;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            m_LastCastFrame = -m_CastFrameInterval;
            // The look source may have already been assigned if the ability was added to the character after the look source was assigned.
            m_LookSource = m_CharacterLocomotion.LookSource;
            if (m_LookSource != null) {
                m_LookSourceTransform = m_LookSource.GameObject.transform;
            }

            if ((m_ObjectDetection & ObjectDetectionMode.Trigger) != 0) {
                m_DetectedTriggerObjects = new GameObject[m_MaxTriggerObjectCount];
            }
            EventHandler.RegisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            m_LookSource = lookSource;
            if (m_LookSource != null) {
                m_LookSourceTransform = m_LookSource.GameObject.transform;
            } else {
                m_LookSourceTransform = null;
            }
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            // The ability may not detect any objects through a trigger or cast.
            if (m_ObjectDetection == 0) {
                return false;
            }

            // The ability can start if using a trigger.
            if ((m_ObjectDetection & ObjectDetectionMode.Trigger) != 0 && m_DetectedTriggerObjectsCount > 0) {
                for (int i = 0; i < m_DetectedTriggerObjectsCount; ++i) {
                    if (ValidateObject(m_DetectedTriggerObjects[i], null)) {
                        m_DetectedObject = m_DetectedTriggerObjects[i];
                        return true;
                    } else if (!m_DetectedTriggerObjects[i].activeInHierarchy) { // The OnTriggerExit callback doesn't occur when the object is deactivated. 
                        if (TriggerExit(m_DetectedTriggerObjects[i])) {
                            i--; // Subtract one so the newly replaced object will be evaluated.
                        }
                    }
                }
            }

            // No more work is necessary if no casts are necessary.
            if (m_ObjectDetection == ObjectDetectionMode.Trigger || (m_UseLookDirection && m_LookSource == null)) {
                return false;
            }

            // Don't perform the cast if the number of casts are being culled.
            if (m_LastCastFrame + m_CastFrameInterval > Time.frameCount) {
                return m_DetectedObject != null;
            }
            m_LastCastFrame = Time.frameCount;

            // The ability may have its own custom cast that should be performed.
            if (m_ObjectDetection == ObjectDetectionMode.Customcast) {
                return true;
            }

            // Use the colliders on the character to detect if the character is near the object.
            var castTransform = m_UseLookPosition ? m_LookSourceTransform : m_Transform;
            var castDirection = m_UseLookDirection ? m_LookSource.LookDirection(true) : m_Transform.forward;
            if ((m_ObjectDetection & ObjectDetectionMode.Charactercast) != 0) {
                if (m_CharacterLocomotion.SingleCast(castDirection * m_CastDistance, castTransform.TransformDirection(m_CastOffset), m_DetectLayers, ref m_RaycastResult)) {
                    var hitObject = m_RaycastResult.collider.gameObject;
                    if (ValidateObject(hitObject, m_RaycastResult)) {
                        m_DetectedObject = hitObject;
                        return true;
                    }
                }
            }

            // Use a raycast to detect if the character is near the object.
            if ((m_ObjectDetection & ObjectDetectionMode.Raycast) != 0) {
                if (Physics.Raycast(castTransform.TransformPoint(m_CastOffset), castDirection, out m_RaycastResult, m_CastDistance, m_DetectLayers, m_TriggerInteraction)) {
                    var hitObject = m_RaycastResult.collider.gameObject;
                    if (ValidateObject(hitObject, m_RaycastResult)) {
                        m_DetectedObject = hitObject;
                        return true;
                    }
                }
            }

            // Use a spherecast to detect if the character is near the object.
            if ((m_ObjectDetection & ObjectDetectionMode.Spherecast) != 0) {
                if (Physics.SphereCast(castTransform.TransformPoint(m_CastOffset) - castTransform.forward * m_SpherecastRadius, m_SpherecastRadius, castDirection, out m_RaycastResult,
                                        m_CastDistance, m_DetectLayers, m_TriggerInteraction)) {
                    var hitObject = m_RaycastResult.collider.gameObject;
                    if (ValidateObject(hitObject, m_RaycastResult)) {
                        m_DetectedObject = hitObject;
                        return true;
                    }
                }
            }

            // The cast did not detect an object.
            m_DetectedObject = null;
            return false;
        }

        /// <summary>
        /// The character has entered a trigger.
        /// </summary>
        /// <param name="other">The trigger collider that the character entered.</param>
        public override void OnTriggerEnter(Collider other)
        {
            // The object may not be detected with a trigger.
            if ((m_ObjectDetection & ObjectDetectionMode.Trigger) == 0) {
                return;
            }

            // The object has to use the correct mask.
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_DetectLayers)) {
                return;
            }

            // Ensure the detected object isn't duplicated within the list.
            for (int i = 0; i < m_DetectedTriggerObjectsCount; ++i) {
                if (m_DetectedTriggerObjects[i] == other.gameObject) {
                    return;
                }
            }

            if (ValidateObject(other.gameObject, null)) {
                if (m_DetectedTriggerObjects.Length == m_DetectedTriggerObjectsCount) {
                    Debug.LogError($"Error: The maximum number of trigger objects need to be increased on the {GetType().Name} ability.");
                    return;
                }
                m_DetectedTriggerObjects[m_DetectedTriggerObjectsCount] = other.gameObject;
                m_DetectedTriggerObjectsCount++;
            }
        }

        /// <summary>
        /// The character has exited a trigger.
        /// </summary>
        /// <param name="other">The trigger collider that the character exited.</param>
        public override void OnTriggerExit(Collider other)
        {
            // The object may not be detected with a trigger.
            if ((m_ObjectDetection & ObjectDetectionMode.Trigger) == 0) {
                return;
            }

            TriggerExit(other.gameObject);
        }

        /// <summary>
        /// The character has exited a trigger.
        /// </summary>
        /// <param name="other">The GameObject that the character exited.</param>
        /// <returns>Returns true if the entered object leaves the trigger.</returns>
        protected virtual bool TriggerExit(GameObject other)
        {
            for (int i = 0; i < m_DetectedTriggerObjectsCount; ++i) {
                if (other == m_DetectedTriggerObjects[i]) {
                    m_DetectedTriggerObjects[i] = null;
                    // Ensure there is not a gap in the trigger object elements.
                    for (int j = i; j < m_DetectedTriggerObjectsCount - 1; ++j) {
                        m_DetectedTriggerObjects[j] = m_DetectedTriggerObjects[j + 1];
                    }
                    m_DetectedTriggerObjectsCount--;
                    // The detected object should be assigned to the oldest trigger object. This value may be null.
                    m_DetectedObject = m_DetectedTriggerObjects[0];
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Validates the object to ensure it is valid for the current ability.
        /// </summary>
        /// <param name="obj">The object being validated.</param>
        /// <param name="raycastHit">The raycast hit of the detected object. Will be null for trigger detections.</param>
        /// <returns>True if the object is valid. The object may not be valid if it doesn't have an ability-specific component attached.</returns>
        protected virtual bool ValidateObject(GameObject obj, RaycastHit? raycastHit)
        {
            if (obj == null || !obj.activeInHierarchy) {
                return false;
            }

            // If an object id is specified then the object must have the Object Identifier component attached with the specified ID.
            if (m_ObjectID != -1) {
                var objectIdentifiers = obj.GetCachedParentComponents<Objects.ObjectIdentifier>();
                if (objectIdentifiers == null) {
                    return false;
                }
                var hasID = false;
                for (int i = 0; i < objectIdentifiers.Length; ++i) {
                    if (objectIdentifiers[i].ID == m_ObjectID) {
                        hasID = true;
                        break;
                    }
                }
                if (!hasID) {
                    return false;
                }
            }

            // The object has to be within the specified angle.
            if (raycastHit.HasValue) {
                var castDirection = m_UseLookDirection ? m_LookSource.LookDirection(true) : m_Transform.forward;
                float angle;
                var objectFaces = obj.GetCachedParentComponent<Objects.ObjectForwardFaces>();
                if (objectFaces != null) {
                    // If an object has multiple faces then the ability can start from multiple directions. It should not start from any angle so don't use the raycast normal.
                    var roundedAngle = 360 / objectFaces.ForwardFaceCount;
                    angle = Quaternion.Angle(Quaternion.LookRotation(castDirection, m_CharacterLocomotion.Up), Quaternion.LookRotation(-obj.transform.forward, m_CharacterLocomotion.Up));
                    angle = Mathf.Abs(MathUtility.ClampInnerAngle(angle - (roundedAngle * Mathf.RoundToInt(angle / roundedAngle))));
                } else {
                    // The object doesn't have the ObjectFaces component. Use the actual angle value.
                    angle = Quaternion.Angle(Quaternion.LookRotation(castDirection, m_CharacterLocomotion.Up), Quaternion.LookRotation(-raycastHit.Value.normal, m_CharacterLocomotion.Up));
                }

                if (angle <= m_AngleThreshold) {
                    return true;
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            AbilityStopped(force, false);
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        /// <param name="triggerExit">Should any triggers be exited before the ability is stopped?</param>
        protected void AbilityStopped(bool force, bool triggerExit)
        {
            // Ensure the OnTriggerExit is triggered when the ability stops.
            if (triggerExit && m_DetectedObject != null && (m_ObjectDetection & ObjectDetectionMode.Trigger) != 0) {
                TriggerExit(m_DetectedObject);
            }

            base.AbilityStopped(force);
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
        }
    }
}