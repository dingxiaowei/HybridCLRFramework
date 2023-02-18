/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Game
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// The KinematicObjectManager acts as a central organizer for determining when the characters, cameras, and kinematic objects should update. The update order that the objects
    /// are updated matter and the KinematicObjectManager ensures the objects are updated in the correct order to allow for smooth movement.
    /// </summary>
    public class KinematicObjectManager : MonoBehaviour
    {
        /// <summary>
        /// Specifies the location that the object should be updated.
        /// </summary>
        public enum UpdateLocation
        {
            Update,         // The object will be updated within Unity's Update loop.
            FixedUpdate,    // The object will be updated within Unity's FixedUpdate loop.
        }

        /// <summary>
        /// A small storage class used for storing the fixed and smooth location. This component will also move the interpolate the objects during the Update loop.
        /// </summary>
        private class SmoothFixedLocation
        {
            private Transform m_Transform;

            private Vector3 m_FixedPosition;
            private Quaternion m_FixedRotation;
            private Vector3 m_SmoothPosition;
            private Quaternion m_SmoothRotation;

            public Transform Transform { get { return m_Transform; } }

            /// <summary>
            /// Initializes the object.
            /// </summary>
            /// <param name="transform">The transform that is being managed by the KinematicObjectManager.</param>
            public void Initialize(Transform transform)
            {
                m_Transform = transform;

                m_FixedPosition = m_SmoothPosition = m_Transform.position;
                m_FixedRotation = m_SmoothRotation = m_Transform.rotation;
            }

            /// <summary>
            /// The object is moved within FixedUpdate while the camera is moved within Update. This would normally cause jitters but a separate smooth variable
            /// ensures the object stays in synchronize with the Update loop.
            /// </summary>
            /// <param name="interpAmount">The amount to interpolate between the smooth and fixed position.</param>
            public virtual void SmoothMove(float interpAmount)
            {
                m_Transform.position = Vector3.Lerp(m_SmoothPosition, m_FixedPosition, interpAmount);
                m_Transform.rotation = Quaternion.Slerp(m_SmoothRotation, m_FixedRotation, interpAmount);
            }

            /// <summary>
            /// Restores the location back to the fixed location. This will be performed immediately before the object is moved within FixedUpdate.
            /// </summary>
            public virtual void RestoreFixedLocation()
            {
                m_Transform.position = m_SmoothPosition = m_FixedPosition;
                m_Transform.rotation = m_SmoothRotation = m_FixedRotation;
            }

            /// <summary>
            /// Assigns the fixed location. This will be performed immediately after the object is moved within FixedUpdate.
            /// </summary>
            public virtual void AssignFixedLocation()
            {
                m_FixedPosition = m_Transform.position;
                m_FixedRotation = m_Transform.rotation;
            }

            /// <summary>
            /// Immediately set the object's position.
            /// </summary>
            /// <param name="position">The position of the object.</param>
            public virtual void SetPosition(Vector3 position)
            {
                m_Transform.position = m_FixedPosition = m_SmoothPosition = position;
            }

            /// <summary>
            /// Immediately set the object's rotation.
            /// </summary>
            /// <param name="position">The rotation of the object.</param>
            public virtual void SetRotation(Quaternion rotation)
            {
                m_Transform.rotation = m_FixedRotation = m_SmoothRotation = rotation;
            }
        }

        /// <summary>
        /// Extends the SmoothFixedLocation class for characters.
        /// </summary>
        private class KinematicCharacter : SmoothFixedLocation
        {
            private UltimateCharacterLocomotion m_CharacterLocomotion;
            private UltimateCharacterLocomotionHandler m_CharacterHandler;
            private CharacterIKBase m_CharacterIK;
            private SmoothFixedLocation[] m_SmoothedBones;
            private CameraController m_AttachedCamera;
            private float m_HorizontalMovement;
            private float m_ForwardMovement;
            private float m_DeltaYawRotation;
            private ScheduledEventBase m_CompleteInitEvent;

            public UltimateCharacterLocomotion CharacterLocomotion { get { return m_CharacterLocomotion; } }
            public CameraController AttachedCamera { get { return m_AttachedCamera; } }
            public CharacterIKBase CharacterIK { get { return m_CharacterIK; } }
            public float HorizontalMovement { set { m_HorizontalMovement = value; } }
            public float ForwardMovement { set { m_ForwardMovement = value; } }
            public float DeltaYawRotation { set { m_DeltaYawRotation = value; } }

            /// <summary>
            /// Initializes the object.
            /// </summary>
            /// <param name="characterLocomotion">The character that is being managed by the KinematicObjectManager.</param>
            public void Initialize(UltimateCharacterLocomotion characterLocomotion)
            {
                m_CharacterLocomotion = characterLocomotion;

                OnAttachLookSource(m_CharacterLocomotion.LookSource);
                EventHandler.RegisterEvent<ILookSource>(m_CharacterLocomotion.gameObject, "OnCharacterAttachLookSource", OnAttachLookSource);

                // The class is pooled so reset any variables.
                m_HorizontalMovement = m_ForwardMovement = m_DeltaYawRotation = 0;
                Initialize(characterLocomotion.transform);

                m_CharacterHandler = m_CharacterLocomotion.GetComponent<UltimateCharacterLocomotionHandler>();
                m_CharacterIK = m_CharacterLocomotion.GetComponent<CharacterIKBase>();

                // Wait a moment before finishing with the initialization. This allows the character to be created at runtime.
                m_CompleteInitEvent = Scheduler.ScheduleFixed(Time.fixedDeltaTime / 2, () => {
                    if (m_CharacterHandler == null) {
                        m_CharacterHandler = m_CharacterLocomotion.GetComponent<UltimateCharacterLocomotionHandler>();
                    }
                    if (m_CharacterIK == null) {
                        m_CharacterIK = m_CharacterLocomotion.GetComponent<CharacterIKBase>();
                    }

                    var smoothedBones = m_CharacterLocomotion.SmoothedBones;
                    if (smoothedBones != null && smoothedBones.Length > 0) {
                        var validBones = 0;
                        for (int i = 0; i < smoothedBones.Length; ++i) {
                            if (smoothedBones[i] != null) {
                                validBones++;
                            }
                        }
                        if (validBones > 0) {
                            m_SmoothedBones = new SmoothFixedLocation[validBones];
                            var index = 0;
                            for (int i = 0; i < smoothedBones.Length; ++i) {
                                if (smoothedBones[i] == null) {
                                    continue;
                                }
                                m_SmoothedBones[index] = GenericObjectPool.Get<SmoothFixedLocation>();
                                m_SmoothedBones[index].Initialize(smoothedBones[i]);
                                index++;
                            }
                        }
                    }
                    m_CompleteInitEvent = null;
                });
            }

            /// <summary>
            /// A new ILookSource object has been attached to the character.
            /// </summary>
            /// <param name="lookSource">The ILookSource object attached to the character.</param>
            private void OnAttachLookSource(ILookSource lookSource)
            {
                if (lookSource != null) {
                    m_AttachedCamera = lookSource.GameObject.GetCachedComponent<CameraController>();
                } else {
                    m_AttachedCamera = null;
                }
            }
            
            /// <summary>
            /// The object is moved within FixedUpdate while the camera is moved within Update. This would normally cause jitters but a separate smooth variable
            /// ensures the object stays in synchronize with the Update loop.
            /// </summary>
            public override void SmoothMove(float interpAmount)
            {
                if (m_CharacterLocomotion.ManualMove) {
                    return;
                }

                base.SmoothMove(interpAmount);

                if (m_SmoothedBones != null) {
                    for (int i = 0; i < m_SmoothedBones.Length; ++i) {
                        m_SmoothedBones[i].SmoothMove(interpAmount);
                    }
                }
            }

            /// <summary>
            /// Moves the character according to the input variables.
            /// </summary>
            /// <param name="manualMove">Is the character being moved manually?</param>
            public void Move(bool manualMove)
            {
                if (m_CharacterLocomotion.ManualMove != manualMove) {
                    return;
                }
                 
                if (m_CharacterHandler != null) {
                    m_DeltaYawRotation = m_CharacterHandler.GetDeltaYawRotation();
                }
                m_CharacterLocomotion.Move(m_HorizontalMovement, m_ForwardMovement, m_DeltaYawRotation);
            }

            /// <summary>
            /// Restores the location back to the fixed location. This will be performed immediately before the object is moved within FixedUpdate.
            /// </summary>
            public override void RestoreFixedLocation()
            {
                base.RestoreFixedLocation();

                if (m_SmoothedBones != null) {
                    for (int i = 0; i < m_SmoothedBones.Length; ++i) {
                        m_SmoothedBones[i].RestoreFixedLocation();
                    }
                }
            }

            /// <summary>
            /// Assigns the fixed location. This will be performed immediately after the object is moved within FixedUpdate.
            /// </summary>
            /// <param name="assignSmoothedBones">Should the character's smoothed bones be assigned?</param>
            public void AssignFixedLocation(bool assignSmoothedBones)
            {
                if (assignSmoothedBones) {
                    if (m_SmoothedBones != null) {
                        for (int i = 0; i < m_SmoothedBones.Length; ++i) {
                            m_SmoothedBones[i].AssignFixedLocation();
                        }
                    }
                } else {
                    base.AssignFixedLocation();
                }
            }

            /// <summary>
            /// Immediately set the object's position.
            /// </summary>
            /// <param name="position">The position of the object.</param>
            public override void SetPosition(Vector3 position)
            {
                base.SetPosition(position);

                // The character's position has been set. Reset the bone location so they will snap into place.
                if (m_SmoothedBones != null) {
                    for (int i = 0; i < m_SmoothedBones.Length; ++i) {
                        m_SmoothedBones[i].SetPosition(m_SmoothedBones[i].Transform.position);
                    }
                }
            }

            /// <summary>
            /// Immediately set the object's rotation.
            /// </summary>
            /// <param name="rotation">The rotation of the object.</param>
            public override void SetRotation(Quaternion rotation)
            {
                base.SetRotation(rotation);

                // The character's rotation has been set. Reset the bone location so they will snap into place.
                if (m_SmoothedBones != null) {
                    for (int i = 0; i < m_SmoothedBones.Length; ++i) {
                        m_SmoothedBones[i].SetRotation(m_SmoothedBones[i].Transform.rotation);
                    }
                }
            }

            /// <summary>
            /// Stops managing the character.
            /// </summary>
            public void UnregisterCharacter()
            {
                if (m_CompleteInitEvent != null) {
                    Scheduler.Cancel(m_CompleteInitEvent);
                    m_CompleteInitEvent = null;
                }

                if (m_SmoothedBones != null) {
                    for (int i = 0; i < m_SmoothedBones.Length; ++i) {
                        GenericObjectPool.Return(m_SmoothedBones[i]);
                    }
                    m_SmoothedBones = null;
                }
                EventHandler.UnregisterEvent<ILookSource>(m_CharacterLocomotion.gameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            }
        }

        /// <summary>
        /// Extends the SmoothFixedLocation class for kinematic objects.
        /// </summary>
        private class KinematicObject : SmoothFixedLocation
        {
            private IKinematicObject m_KinematicObject;

            public IKinematicObject IKinematicObject { get { return m_KinematicObject; } }

            /// <summary>
            /// Initializes the object.
            /// </summary>
            /// <param name="kinematicObject">The kinematic object that is being managed by the KinematicObjectManager.</param>
            public void Initialize(IKinematicObject kinematicObject)
            {
                m_KinematicObject = kinematicObject;
                Initialize(m_KinematicObject.transform);
            }

            /// <summary>
            /// Moves the kinematic object.
            /// </summary>
            /// <param name="applyFixedLocation">Should the fixed location be applied?</param>
            public void Move(bool applyFixedLocation)
            {
                if (applyFixedLocation) {
                    RestoreFixedLocation();
                }
                m_KinematicObject.Move();
                if (applyFixedLocation) {
                    AssignFixedLocation();
                }
            }
        }

        /// <summary>
        /// Moves and rotates the camera.
        /// </summary>
        private class KinematicCamera : SmoothFixedLocation
        {
            private CameraController m_CameraController;
            private UltimateCharacterLocomotion m_CharacterLocomotion;
            private Vector2 m_LookVector;

            public CameraController CameraController { get { return m_CameraController; } }
            public UltimateCharacterLocomotion CharacterLocomotion { get { return m_CharacterLocomotion; } }
            public Vector2 LookVector { set { m_LookVector = value; } }

            /// <summary>
            /// Initializes the object.
            /// </summary>
            /// <param name="cameraController">The camera controller that is being managed by the KinematicObjectManager.</param>
            public void Initialize(CameraController cameraController)
            {
                Initialize(cameraController.transform);

                m_CameraController = cameraController;
                OnAttachCharacter(m_CameraController.Character);
                EventHandler.RegisterEvent<GameObject>(m_CameraController.gameObject, "OnCameraAttachCharacter", OnAttachCharacter);
            }

            /// <summary>
            /// Attaches the camera to the specified character.
            /// </summary>
            /// <param name="character">The character to attach the camera to.</param>
            private void OnAttachCharacter(GameObject character)
            {
                if (character != null) {
                    m_CharacterLocomotion = character.GetCachedComponent<UltimateCharacterLocomotion>();
                } else {
                    m_CharacterLocomotion = null;
                }
            }

            /// <summary>
            /// Rotates the camera.
            /// </summary>
            public void Rotate()
            {
                RestoreFixedLocation();

                m_CameraController.Rotate(m_LookVector.x, m_LookVector.y);
            }

            /// <summary>
            /// Calls the Move method of the CameraController.
            /// </summary>
            public void Move()
            {
                m_CameraController.Move(m_LookVector.x, m_LookVector.y);

                AssignFixedLocation();
            }

            /// <summary>
            /// Stops managing the camera.
            /// </summary>
            public void UnregisterCamera()
            {
                EventHandler.UnregisterEvent<GameObject>(m_CameraController.gameObject, "OnCameraAttachCharacter", OnAttachCharacter);
            }
        }

        private static KinematicObjectManager s_Instance;
        private static KinematicObjectManager Instance
        {
            get
            {
                if (!s_Initialized) {
                    s_Instance = new GameObject("Kinematic Object Manager").AddComponent<KinematicObjectManager>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        [Tooltip("The number of starting characters. For best performance this value should be the maximum number of characters that can be active within the scene.")]
        [SerializeField] protected int m_StartCharacterCount = 1;
        [Tooltip("The number of starting cameras. For best performance this value should be the maximum number of cameras that can be active within the scene.")]
        [SerializeField] protected int m_StartCameraCount = 1;
        [UnityEngine.Serialization.FormerlySerializedAs("m_StartDeterministicObjectCount")]
        [Tooltip("The number of starting kinematic objects. For best performance this value should be the maximum number of kinematic objects that can be active within the scene.")]
        [SerializeField] protected int m_StartKinematicObjectCount;
        [Tooltip("Should the Auto Sync Transforms be enabled? See this page for more info: https://docs.unity3d.com/ScriptReference/Physics-autoSyncTransforms.html.")]
        [SerializeField] protected bool m_AutoSyncTransforms;

        public int StartCharacterCount { get { return m_StartCharacterCount; } }
        public int StartCameraCount { get { return m_StartCameraCount; } }
        public int StartKinematicObjectCount { get { return m_StartKinematicObjectCount; } }

        private KinematicCharacter[] m_Characters;
        private KinematicCamera[] m_Cameras;
        private KinematicObject[] m_KinematicObjects;
        private int m_CharacterCount;
        private int m_CameraCount;
        private int m_KinematicObjectCount;
        private float m_FixedTime;
        private bool m_FixedUpdate;

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null) {
                s_Instance = this;
                s_Initialized = true;
                SceneManager.sceneUnloaded -= SceneUnloaded;

                // Characters and cameras may be marked DontDestroyOnLoad. Reregister any active objects.
                var characterLocomotions = FindObjectsOfType<UltimateCharacterLocomotion>();
                for (int i = 0; i < characterLocomotions.Length; ++i) {
                    if (characterLocomotions[i].KinematicObjectIndex == -1) {
                        continue;
                    }
                    // The character is active.
                    characterLocomotions[i].KinematicObjectIndex = RegisterCharacter(characterLocomotions[i]);
                }
                var cameras = FindObjectsOfType<CameraController>();
                for (int i = 0; i < cameras.Length; ++i) {
                    if (cameras[i].KinematicObjectIndex == -1) {
                        continue;
                    }
                    // The camera is active.
                    cameras[i].KinematicObjectIndex = RegisterCamera(cameras[i]);
                }
            }
        }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Characters = new KinematicCharacter[m_StartCharacterCount];
            m_Cameras = new KinematicCamera[m_StartCameraCount];
            m_KinematicObjects = new KinematicObject[m_StartKinematicObjectCount];

            Physics.autoSyncTransforms = m_AutoSyncTransforms;
        }

        /// <summary>
        /// Registers the character to be managed by the KinematicObjectManager.
        /// </summary>
        /// <param name="characterLocomotion">The character that should be managed by the KinematicObjectManager.</param>
        /// <returns>The index of the character registered.</returns>
        public static int RegisterCharacter(UltimateCharacterLocomotion characterLocomotion)
        {
            return Instance.RegisterCharacterInternal(characterLocomotion);
        }

        /// <summary>
        /// Internal method which registers the character to be managed by the KinematicObjectManager.
        /// </summary>
        /// <param name="characterLocomotion">The character that should be managed by the KinematicObjectManager.</param>
        /// <returns>The index of the character registered.</returns>
        private int RegisterCharacterInternal(UltimateCharacterLocomotion characterLocomotion)
        {
            if (m_CharacterCount == m_Characters.Length) {
                System.Array.Resize(ref m_Characters, m_Characters.Length + 1);
                Debug.LogWarning($"Characters array resized. For best performance increase the size of the Start Character Count variable " +
                                 $"within the Kinematic Object Manager to a value of at least {(m_CharacterCount + 1)}.");
            }
            m_Characters[m_CharacterCount] = GenericObjectPool.Get<KinematicCharacter>();
            m_Characters[m_CharacterCount].Initialize(characterLocomotion);
            m_CharacterCount++;
            return m_CharacterCount - 1;
        }

        /// <summary>
        /// Registers the camera to be managed by the KinematicObjectManager.
        /// </summary>
        /// <param name="cameraController">The camera that should be managed by the KinematicObjectManager.</param>
        /// <returns>The index of the camera registered.</returns>
        public static int RegisterCamera(CameraController cameraController)
        {
            return Instance.RegisterCameraInternal(cameraController);
        }

        /// <summary>
        /// Intenral method which registers the camera to be managed by the KinematicObjectManager.
        /// </summary>
        /// <param name="cameraController">The camera that should be managed by the KinematicObjectManager.</param>
        /// <returns>The index of the camera registered.</returns>
        private int RegisterCameraInternal(CameraController cameraController)
        {
            if (m_CameraCount == m_Cameras.Length) {
                System.Array.Resize(ref m_Cameras, m_Cameras.Length + 1);
                Debug.LogWarning($"Cameras array resized. For best performance increase the size of the Start Camera Count variable " +
                                 $"within the Kinematic Object Manager to a value of at least {(m_CameraCount + 1)}.");
            }
            m_Cameras[m_CameraCount] = GenericObjectPool.Get<KinematicCamera>();
            m_Cameras[m_CameraCount].Initialize(cameraController);
            m_CameraCount++;
            return m_CameraCount - 1;
        }

        /// <summary>
        /// Registers the kinematic object that should be managed by the KinematicObjectManager.
        /// </summary>
        /// <param name="kinematicObject">The kinematic object that should be managed by the KinematicObjectManager.</param>
        /// <returns>The index of the kinematci object registered.</returns>
        public static int RegisterKinematicObject(IKinematicObject kinematicObject)
        {
            return Instance.RegisterKinematicObjectInternal(kinematicObject);
        }

        /// <summary>
        /// Internal method which registers the kinematic object that should be managed by the KinematicObjectManager.
        /// </summary>
        /// <param name="kinematicObject">The kinematic object that should be managed by the KinematicObjectManager.</param>
        /// <returns>The index of the kinematci object registered.</returns>
        private int RegisterKinematicObjectInternal(IKinematicObject kinematicObject)
        {
            if (m_KinematicObjectCount == m_KinematicObjects.Length) {
                System.Array.Resize(ref m_KinematicObjects, m_KinematicObjects.Length + 1);
                Debug.LogWarning($"Kinematic objects array resized. For best performance increase the size of the Start Kinematic Object Count variable " +
                                 $"within the Kinematic Object Manager to a value of at least {(m_KinematicObjectCount + 1)}.");
            }
            m_KinematicObjects[m_KinematicObjectCount] = GenericObjectPool.Get<KinematicObject>();
            m_KinematicObjects[m_KinematicObjectCount].Initialize(kinematicObject);
            m_KinematicObjectCount++;
            return m_KinematicObjectCount - 1;
        }

        /// <summary>
        /// Sets the yaw rotation of the character.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="yawRotation">The yaw rotation that the character should rotate towards.</param>
        public static void SetCharacterDeltaYawRotation(int characterIndex, float yawRotation)
        {
            Instance.SetCharacterDeltaYawRotationInternal(characterIndex, yawRotation);
        }

        /// <summary>
        /// Internal method which sets the yaw rotation of the character.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="yawRotation">The yaw rotation that the character should rotate towards.</param>
        private void SetCharacterDeltaYawRotationInternal(int characterIndex, float yawRotation)
        {
            m_Characters[characterIndex].DeltaYawRotation = yawRotation;
        }

        /// <summary>
        /// Sets the horizontal and forward input values of the character.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        public static void SetCharacterMovementInput(int characterIndex, float horizontalMovement, float forwardMovement)
        {
            Instance.SetCharacterMovementInputInternal(characterIndex, horizontalMovement, forwardMovement);
        }

        /// <summary>
        /// Internal method which sets the horizontal and forward input values of the character.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        private void SetCharacterMovementInputInternal(int characterIndex, float horizontalMovement, float forwardMovement)
        {
            m_Characters[characterIndex].HorizontalMovement = horizontalMovement;
            m_Characters[characterIndex].ForwardMovement = forwardMovement;
        }

        /// <summary>
        /// Immediately sets the character's position.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="position">The position of the object.</param>
        public static void SetCharacterPosition(int characterIndex, Vector3 position)
        {
            Instance.SetCharacterPositionInternal(characterIndex, position);
        }

        /// <summary>
        /// Internal method which immediately sets the character's position.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="position">The position of the object.</param>
        private void SetCharacterPositionInternal(int characterIndex, Vector3 position)
        {
            m_Characters[characterIndex].SetPosition(position);

            if (!m_AutoSyncTransforms) {
                Physics.SyncTransforms();
            }
        }

        /// <summary>
        /// Immediately sets the character's rotation.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="position">The position of the object.</param>
        public static void SetCharacterRotation(int characterIndex, Quaternion rotation)
        {
            Instance.SetCharacterRotationInternal(characterIndex, rotation);
        }

        /// <summary>
        /// Internal method which immediately sets the character's rotation.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="position">The position of the object.</param>
        private void SetCharacterRotationInternal(int characterIndex, Quaternion rotation)
        {
            m_Characters[characterIndex].SetRotation(rotation);

            if (!m_AutoSyncTransforms) {
                Physics.SyncTransforms();
            }
        }

        /// <summary>
        /// Sets the look vector of the camera.
        /// </summary>
        /// <param name="cameraIndex">The index of the camera within the cameras array.</param>
        /// <param name="lookVector">The look vector of the camera.</param>
        public static void SetCameraLookVector(int cameraIndex, Vector2 lookVector)
        {
            Instance.SetCameraLookVectorInternal(cameraIndex, lookVector);
        }

        /// <summary>
        /// Internal method which sets the look vector of the camera.
        /// </summary>
        /// <param name="cameraIndex">The index of the camera within the cameras array.</param>
        /// <param name="lookVector">The look vector of the camera.</param>
        private void SetCameraLookVectorInternal(int cameraIndex, Vector2 lookVector)
        {
            m_Cameras[cameraIndex].LookVector = lookVector;
        }

        /// <summary>
        /// Sets the position of the camera.
        /// </summary>
        /// <param name="cameraIndex">The index of the camera within the cameras array.</param>
        /// <param name="position">The position of the camera.</param>
        public static void SetCameraPosition(int cameraIndex, Vector3 position)
        {
            Instance.SetCameraPositionInternal(cameraIndex, position);
        }

        /// <summary>
        /// Internal method which sets the position of the camera.
        /// </summary>
        /// <param name="cameraIndex">The index of the camera within the cameras array.</param>
        /// <param name="position">The position of the camera.</param>
        private void SetCameraPositionInternal(int cameraIndex, Vector3 position)
        {
            m_Cameras[cameraIndex].SetPosition(position);
        }

        /// <summary>
        /// Sets the rotation of the camera.
        /// </summary>
        /// <param name="cameraIndex">The index of the camera within the cameras array.</param>
        /// <param name="rotation">The rotation of the camera.</param>
        public static void SetCameraRotation(int cameraIndex, Quaternion rotation)
        {
            Instance.SetCameraRotationInternal(cameraIndex, rotation);
        }

        /// <summary>
        /// Internal method which sets the rotation of the camera.
        /// </summary>
        /// <param name="cameraIndex">The index of the camera within the cameras array.</param>
        /// <param name="rotation">The rotation of the camera.</param>
        private void SetCameraRotationInternal(int cameraIndex, Quaternion rotation)
        {
            m_Cameras[cameraIndex].SetRotation(rotation);
        }

        /// <summary>
        /// Immediately sets the kinematic object's position.
        /// </summary>
        /// <param name="kinematicObjectIndex">The index of the kinematic object within the array.</param>
        /// <param name="position">The position of the object.</param>
        public static void SetKinematicObjectPosition(int kinematicObjectIndex, Vector3 position)
        {
            Instance.SetKinematicObjectPositionInternal(kinematicObjectIndex, position);
        }

        /// <summary>
        /// Internal method which immediately sets the kinematic object's position.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="position">The position of the object.</param>
        private void SetKinematicObjectPositionInternal(int kinematicObjectIndex, Vector3 position)
        {
            m_KinematicObjects[kinematicObjectIndex].SetPosition(position);

            if (!m_AutoSyncTransforms) {
                Physics.SyncTransforms();
            }
        }

        /// <summary>
        /// Immediately sets the kinematic object's rotation.
        /// </summary>
        /// <param name="kinematicObjectIndex">The index of the character within the characters array.</param>
        /// <param name="position">The position of the object.</param>
        public static void SetKinematicObjectRotation(int kinematicObjectIndex, Quaternion rotation)
        {
            Instance.SetKinematicObjectRotationInternal(kinematicObjectIndex, rotation);
        }

        /// <summary>
        /// Internal method which immediately sets the kinematic object's rotation.
        /// </summary>
        /// <param name="kinematicObjectIndex">The index of the kinematic object within the array.</param>
        /// <param name="position">The position of the object.</param>
        private void SetKinematicObjectRotationInternal(int kinematicObjectIndex, Quaternion rotation)
        {
            m_KinematicObjects[kinematicObjectIndex].SetRotation(rotation);

            if (!m_AutoSyncTransforms) {
                Physics.SyncTransforms();
            }
        }

        /// <summary>
        /// Smoothly moves the objects.
        /// </summary>
        private void Update()
        {
            var interpAmount = (Time.time - m_FixedTime) / Time.fixedDeltaTime;
            for (int i = 0; i < m_KinematicObjectCount; ++i) {
                if (m_KinematicObjects[i].IKinematicObject.UpdateLocation == UpdateLocation.Update) {
                    m_KinematicObjects[i].Move(false);
                    m_KinematicObjects[i].AssignFixedLocation();
                } else {
                    m_KinematicObjects[i].SmoothMove(interpAmount);
                }
            }
            // Sync the transforms for IK.
            if (m_KinematicObjectCount > 0 && !m_AutoSyncTransforms) {
                Physics.SyncTransforms();
            }
            for (int i = 0; i < m_CameraCount; ++i) {
                if (m_Cameras[i].CharacterLocomotion.UpdateLocation == UpdateLocation.Update) {
                    continue;
                }

                m_Cameras[i].SmoothMove(interpAmount);
            }
            for (int i = 0; i < m_CharacterCount; ++i) {
                if (m_Characters[i].CharacterLocomotion.UpdateLocation == UpdateLocation.Update) {
                    m_Characters[i].Move(false);
                    m_Characters[i].AssignFixedLocation(false);
                    m_Characters[i].AssignFixedLocation(true);
                } else {
                if (m_Characters[i].CharacterIK != null && m_Characters[i].CharacterIK.enabled) {
                    m_Characters[i].CharacterIK.Move(false);
                }
                    // Update the smoothed bone fixed location after the IK pass has executed. The animator is updated
                    // during the physics loop so the smooth bone locations only need to be assigned after the FixedUpdate
                    // loop has run.
                    if (m_FixedUpdate) {
                        m_Characters[i].AssignFixedLocation(true);
                    }
                    m_Characters[i].SmoothMove(interpAmount);
                }
            }
            m_FixedUpdate = false;
        }

        /// <summary>
        /// Moves the kinematic objects and characters.
        /// </summary>
        private void FixedUpdate()
        {
            // The kinematic object and cameras should be moved first so the characters receive the most recent changes.
            for (int i = 0; i < m_KinematicObjectCount; ++i) {
                if (m_KinematicObjects[i].IKinematicObject.UpdateLocation == UpdateLocation.Update) {
                    continue;
                }
                m_KinematicObjects[i].Move(true);
            }

            for (int i = 0; i < m_CharacterCount; ++i) {
                if (m_Characters[i].CharacterLocomotion.UpdateLocation == UpdateLocation.Update) {
                    continue;
                }
                if (!m_AutoSyncTransforms) {
                    Physics.SyncTransforms();
                }
                m_Characters[i].Move(false);
                if (m_Characters[i].CharacterIK != null && m_Characters[i].CharacterIK.enabled) {
                    m_Characters[i].CharacterIK.Move(true);
                }
                // If FixedUpdate is called multiple times before Update then the framerate is low.
                // Update the position immediately to prevent jittering.
                if (m_FixedUpdate) {
                    m_Characters[i].AssignFixedLocation(true);
                }
            }

            // Remember the time so SmoothMove can determine how much interpolation is necessary.
            m_FixedTime = Time.time;
            m_FixedUpdate = true;
        }

        /// <summary>
        /// Updates the IK component.
        /// </summary>
        private void LateUpdate()
        {
            for (int i = 0; i < m_CharacterCount; ++i) {
                if (m_Characters[i].CharacterIK != null && m_Characters[i].CharacterIK.enabled) {
                    m_Characters[i].CharacterIK.Move(false);
                }
            }
        }

        /// <summary>
        /// Moves the character.
        /// </summary>
        /// <param name="characterIndex">The index of the character.</param>
        public static void CharacterMove(int characterIndex)
        {
            Instance.CharacterMoveInternal(characterIndex);
        }

        /// <summary>
        /// Internal method which moves the character.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        public void CharacterMoveInternal(int characterIndex)
        {
            if (characterIndex < 0) {
                return;
            }

            if (!m_AutoSyncTransforms) {
                Physics.SyncTransforms();
            }

            m_Characters[characterIndex].Move(true);
        }

        /// <summary>
        /// Indicates that the character has started moving.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        public static void BeginCharacterMovement(int characterIndex)
        {
            Instance.BeginCharacterMovementInternal(characterIndex);
        }

        /// <summary>
        /// Internal method which indicates that the character has started moving.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        private void BeginCharacterMovementInternal(int characterIndex)
        {
            if (characterIndex < 0) {
                return;
            }

            if (!m_AutoSyncTransforms) {
                Physics.SyncTransforms();
            }

            if (m_Characters[characterIndex].CharacterLocomotion.UpdateLocation == UpdateLocation.FixedUpdate) {
                m_Characters[characterIndex].RestoreFixedLocation();
            }

            // If the character has a camera attached the camera should first be rotated.
            int cameraIndex;
            if (m_Characters[characterIndex].AttachedCamera != null && (cameraIndex = m_Characters[characterIndex].AttachedCamera.KinematicObjectIndex) >= 0) {
                m_Cameras[cameraIndex].Rotate();
            }
        }

        /// <summary>
        /// Indicates that the character has stopped moving.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        public static void EndCharacterMovement(int characterIndex)
        {
            Instance.EndCharacterMovementInternal(characterIndex);
        }

        /// <summary>
        /// Internal method which indicates that the character has stopped moving.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        private void EndCharacterMovementInternal(int characterIndex)
        {
            if (characterIndex < 0) {
                return;
            }

            if (m_Characters[characterIndex].CharacterLocomotion.UpdateLocation == UpdateLocation.FixedUpdate) {
                m_Characters[characterIndex].AssignFixedLocation();
            }

            // After the character has updated the camera should update one more time to account for the new character position.
            int cameraIndex;
            if (m_Characters[characterIndex].AttachedCamera != null && (cameraIndex = m_Characters[characterIndex].AttachedCamera.KinematicObjectIndex) >= 0) {
                m_Cameras[cameraIndex].Move();
            }
        }

        /// <summary>
        /// Stops managing the object at the specified index.
        /// </summary>
        /// <param name="characterIndex">The index of the object within the manager array.</param>
        public static void UnregisterCharacter(int characterIndex)
        {
            Instance.UnregisterCharacterInternal(characterIndex);
        }

        /// <summary>
        /// Internal method which stops managing the character at the specified index.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        private void UnregisterCharacterInternal(int characterIndex)
        {
            if (characterIndex < 0) {
                return;
            }

            m_Characters[characterIndex].UnregisterCharacter();
            GenericObjectPool.Return(m_Characters[characterIndex]);
            // Keep the array packed by shifting all of the subsequent elements over by one.
            for (int i = characterIndex + 1; i < m_CharacterCount; ++i) {
                m_Characters[i - 1] = m_Characters[i];
                m_Characters[i - 1].CharacterLocomotion.KinematicObjectIndex = i - 1;
            }
            m_CharacterCount--;
        }

        /// <summary>
        /// Stops managing the camera at the specified index.
        /// </summary>
        /// <param name="characterIndex">The index of the camera within the cameras array.</param>
        public static void UnregisterCamera(int cameraIndex)
        {
            Instance.UnregisterCameraInternal(cameraIndex);
        }

        /// <summary>
        /// Internal method which stops managing the camera at the specified index.
        /// </summary>
        /// <param name="characterIndex">The index of the camera within the cameras array.</param>
        private void UnregisterCameraInternal(int cameraIndex)
        {
            if (cameraIndex < 0) {
                return;
            }

            m_Cameras[cameraIndex].UnregisterCamera();
            GenericObjectPool.Return(m_Cameras[cameraIndex]);
            // Keep the array packed by shifting all of the subsequent elements over by one.
            for (int i = cameraIndex + 1; i < m_CameraCount; ++i) {
                m_Cameras[i - 1] = m_Cameras[i];
                m_Cameras[i - 1].CameraController.KinematicObjectIndex = i - 1;
            }
            m_CameraCount--;
        }

        /// <summary>
        /// Stops managing the kinematic object at the specified index.
        /// </summary>
        /// <param name="kinematicObjectIndex">The index of the kinematic object within the characters array.</param>
        public static void UnregisterKinematicObject(int kinematicObjectIndex)
        {
            Instance.UnregisterKinematicObjectInternal(kinematicObjectIndex);
        }

        /// <summary>
        /// Internal method which stops managing the kinematic object at the specified index.
        /// </summary>
        /// <param name="kinematicObjectIndex">The index of the kinematic object within the characters array.</param>
        private void UnregisterKinematicObjectInternal(int kinematicObjectIndex)
        {
            if (kinematicObjectIndex < 0) {
                return;
            }

            GenericObjectPool.Return(m_KinematicObjects[kinematicObjectIndex]);
            // Keep the array packed by shifting all of the subsequent elements over by one.
            for (int i = kinematicObjectIndex + 1; i < m_KinematicObjectCount; ++i) {
                m_KinematicObjects[i - 1] = m_KinematicObjects[i];
                m_KinematicObjects[i - 1].IKinematicObject.KinematicObjectIndex = i - 1;
            }
            m_KinematicObjectCount--;
        }

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded(Scene scene)
        {
            s_Initialized = false;
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;

            UnityEngineUtility.ClearCache();
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

#if UNITY_2019_3_OR_NEWER
        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReset()
        {
            s_Initialized = false;
            s_Instance = null;
        }
#endif
    }
}