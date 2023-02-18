/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Controls the animation for the door.
    /// </summary>
    public class Door : MonoBehaviour
    {
        private const string c_ColorText = "_Color";
        private const string c_EmissionColor = "_EmissionColor";

        [Tooltip("The LayerMask of the character.")]
        [SerializeField] protected LayerMask m_LayerMask = 1 << LayerManager.Character;
        [Tooltip("Is the door locked?")]
        [SerializeField] protected bool m_Locked = false;
        [Tooltip("Should the door be permanently locked? A permanently locked door cannot be opened by the DemoManager.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_PermantlyLocked")]
        [SerializeField] protected bool m_PermanentlyLocked;
        [Tooltip("Should the door be opened at the start?")]
        [SerializeField] protected bool m_OpenAtStart;
        [Tooltip("Should the door close when the character leaves the trigger?")]
        [SerializeField] protected bool m_CloseOnTriggerExit = true;
        [Tooltip("The material that can change colors when the door is locked or unlcoked.")]
        [SerializeField] protected Material m_StatusMaterial;
        [Tooltip("The color of the closed door.")]
        [SerializeField] protected Color m_LockedColor = Color.red;
        [Tooltip("The color of the opened door.")]
        [SerializeField] protected Color m_UnlockedColor = Color.green;
        [Tooltip("The AudioClip that should play when the door is opened.")]
        [SerializeField] protected AudioClip m_OpenAudioClip;
        [Tooltip("The AudioClip that should play when the door is closed.")]
        [SerializeField] protected AudioClip m_CloseAudioClip;

        public bool Locked { get { return m_Locked; }
            set {
                if (m_Locked == value) {
                    return;
                }
                m_Locked = value;
                UpdateDoorStatus();
                if (m_Locked && m_Open) {
                    OpenClose(false, true, true);
                } else if (!m_Locked && m_CharacterCount > 0) {
                    m_Open = false;
                    OpenClose(true, true, true);
                }
            }
        }
        public bool PermanentlyLocked { get { return m_PermanentlyLocked; } set { m_PermanentlyLocked = value; } }
        public bool CloseOnTriggerExit { get { return m_CloseOnTriggerExit; } set { m_CloseOnTriggerExit = value; } }

        private static int s_OpenHash = Animator.StringToHash("Open");

        private Animator m_Animator;
        private AudioSource m_AudioSource;

        private bool m_ManagerOpen;
        private bool m_Open;
        private int m_CharacterCount;
        private Material[] m_StatusMaterials;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_AudioSource = GetComponent<AudioSource>();

            var demoManager = FindObjectOfType<DemoManager>();
            if (demoManager != null) {
                demoManager.RegisterDoor(this);
            }

            // Cache the light materials so they can be changed.
            var renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; ++i) {
                var materials = renderers[i].sharedMaterials;
                for (int j = 0; j < materials.Length; ++j) {
                    // The shared material will allow for a valid comparison. The instance material should not be compared.
                    if (materials[j] != m_StatusMaterial) {
                        continue;
                    }

                    if (m_StatusMaterials == null) {
                        m_StatusMaterials = new Material[1];
                    } else {
                        System.Array.Resize(ref m_StatusMaterials, m_StatusMaterials.Length + 1);
                    }

                    // Cache the instance material so each status light operates independently.
                    m_StatusMaterials[m_StatusMaterials.Length - 1] = renderers[i].materials[j];
                }
            }

            // Permantly locked doors cannot be opened.
            if (m_PermanentlyLocked) {
                m_Locked = true;
            }
            if (m_OpenAtStart) {
                OpenClose(true, true, false);
            }
            UpdateDoorStatus();
        }

        /// <summary>
        /// Updates the door status material.
        /// </summary>
        private void UpdateDoorStatus()
        {
            if (m_StatusMaterials != null) {
                for (int i = 0; i < m_StatusMaterials.Length; ++i) {
                    var locked = m_Locked || m_PermanentlyLocked;
                    m_StatusMaterials[i].SetColor(c_ColorText, locked ? m_LockedColor : m_UnlockedColor);
                    m_StatusMaterials[i].SetColor(c_EmissionColor, locked ? m_LockedColor : m_UnlockedColor);
                }
            }
        }

        /// <summary>
        /// Closes the door.
        /// </summary>
        public void Close()
        {
            OpenClose(false, false, false);
        }

        /// <summary>
        /// Opens the door.
        /// </summary>
        public void Open()
        {
            OpenClose(true, false, false);
        }

        /// <summary>
        /// Opens or closes the door.
        /// </summary>
        /// <param name="open">Should the door be opened?</param>
        /// <param name="fromManager">Is the door being opened or closed from the DemoManager?</param>
        /// <param name="playAudio">Should the door open/close audio be played?</param>
        public void OpenClose(bool open, bool fromManager, bool playAudio)
        {
            // Permanently locked doors cannot be opened.
            if (m_PermanentlyLocked && open) {
                return;
            }

            // Don't close the door if the manager opened the door.
            if (!open && !fromManager && m_ManagerOpen) {
                return;
            }

            // The door can't open if it is already open.
            if (m_Open == open) {
                return;
            }

            if (open) {
                // The door can't open if it's locked.
                if (m_Locked) {
                    // The manager can unlock the door.
                    if (fromManager) {
                        m_Locked = false;
                    } else {
                        return;
                    }
                }
            }

            m_Open = open;

            m_Animator.SetBool(s_OpenHash, open);
            if (playAudio && m_AudioSource != null) {
                if (open && m_OpenAudioClip != null) {
                    m_AudioSource.clip = m_OpenAudioClip;
                    m_AudioSource.Play();
                } else if (!open && m_CloseAudioClip != null) {
                    m_AudioSource.clip = m_CloseAudioClip;
                    m_AudioSource.Play();
                }
            }

            // The manager will override the trigger settings.
            if (fromManager) {
                m_ManagerOpen = open;
            }
        }

        /// <summary>
        /// Open the door if the character enters the trigger.
        /// </summary>
        /// <param name="other">The collider which entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask) || other.gameObject.GetCachedParentComponent<UltimateCharacterLocomotion>() == null) {
                return;
            }

            m_CharacterCount++;
            OpenClose(true, false, true);
        }

        /// <summary>
        /// Close the door if the character leaves the trigger.
        /// </summary>
        /// <param name="other">The collider which left the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask) || other.gameObject.GetCachedParentComponent<UltimateCharacterLocomotion>() == null) {
                return;
            }

            // The door should only close when all characters are no longer within the trigger.
            m_CharacterCount--;
            if (m_CharacterCount == 0 && m_Animator.GetBool(s_OpenHash) && m_CloseOnTriggerExit) {
                OpenClose(false, false, true);
            }
        }
    }
}