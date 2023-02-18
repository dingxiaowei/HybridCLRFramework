/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The CrosshairsMonitor will update the UI for the crosshair.
    /// </summary>
    public class CrosshairsMonitor : CharacterMonitor
    {
#if UNITY_EDITOR
        [Tooltip("Draw a debug line to see the direction that the crosshairs is looking (editor only).")]
        [SerializeField] protected bool m_DebugDrawLookRay;
#endif
        [Tooltip("The radius of the crosshair's collision sphere to detect if it is targetting an enemy.")]
        [SerializeField] protected float m_CollisionRadius = 0.05f;
        [Tooltip("The maximum number of colliders that the crosshairs can detect.")]
        [SerializeField] protected int m_MaxCollisionCount = 40;
        [Tooltip("Specifies if the crosshairscan detect triggers.")]
        [SerializeField] protected QueryTriggerInteraction m_TriggerInteraction = QueryTriggerInteraction.Ignore;
        [Tooltip("The crosshairs used when the item doesn't specify a crosshairs.")]
        [SerializeField] protected Sprite m_DefaultSprite;
        [Tooltip("The default color of the crosshairs.")]
        [SerializeField] protected Color m_DefaultColor = Color.white;
        [Tooltip("The color of the crosshairs when a target is in sight.")]
        [SerializeField] protected Color m_TargetColor = Color.red;
        [Tooltip("A reference to the image used for the center crosshairs.")]
        [SerializeField] protected Image m_CenterCrosshairsImage;
        [Tooltip("A reference to the image used for the left crosshairs.")]
        [SerializeField] protected Image m_LeftCrosshairsImage;
        [Tooltip("A reference to the image used for the top crosshairs.")]
        [SerializeField] protected Image m_TopCrosshairsImage;
        [Tooltip("A reference to the image used for the right crosshairs.")]
        [SerializeField] protected Image m_RightCrosshairsImage;
        [Tooltip("A reference to the image used for the bottom crosshairs.")]
        [SerializeField] protected Image m_BottomCrosshairsImage;
        [Tooltip("Should the crosshairs be disabled when the character dies?")]
        [SerializeField] protected bool m_DisableOnDeath = true;

        public float CollisionRadius { get { return m_CollisionRadius; } set { m_CollisionRadius = value; } }
        public QueryTriggerInteraction TriggerInteraction { get { return m_TriggerInteraction; } set { m_TriggerInteraction = value; } }
        public Color DefaultColor { get { return m_DefaultColor; } set { m_DefaultColor = value; } }
        public Color TargetColor { get { return m_TargetColor; } set { m_TargetColor = value; } }
        public bool DisableOnDeath { get { return m_DisableOnDeath; } set { m_DisableOnDeath = value; } }

        private GameObject m_GameObject;
        private UnityEngine.Camera m_Camera;
        private CameraController m_CameraController;
        private AimAssist m_AimAssist;
        private Transform m_CharacterTransform;
        private CharacterLayerManager m_CharacterLayerManager;
        private UltimateCharacterLocomotion m_CharacterLocomotion;

        private RectTransform m_CenterRectTransform;
        private RectTransform m_LeftRectTransform;
        private RectTransform m_TopRectTransform;
        private RectTransform m_RightRectTransform;
        private RectTransform m_BottomRectTransform;

        private RaycastHit[] m_RaycastHits;
        private UnityEngineUtility.RaycastHitComparer m_RaycastHitComparer = new UnityEngineUtility.RaycastHitComparer();
        private Item m_EquippedItem;
        private float m_CurrentCrosshairsSpread;
        private float m_TargetCrosshairsSpread;
        private float m_CrosshairsSpreadVelocity;
        private bool m_Aiming;
        private bool m_EnableImage;
        private int m_EquippedItemCount;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            if (m_CenterCrosshairsImage == null) {
                m_CenterCrosshairsImage = GetComponent<Image>();
            }
            m_CenterCrosshairsImage.sprite = m_DefaultSprite;
            m_CenterRectTransform = m_CenterCrosshairsImage.GetComponent<RectTransform>();
            if ((m_CenterCrosshairsImage.enabled = (m_DefaultSprite != null))) {
                UnityEngineUtility.SizeSprite(m_CenterCrosshairsImage.sprite, m_CenterRectTransform);
            }

            if (m_LeftCrosshairsImage != null) m_LeftRectTransform = m_LeftCrosshairsImage.GetComponent<RectTransform>();
            if (m_TopCrosshairsImage != null) m_TopRectTransform = m_TopCrosshairsImage.GetComponent<RectTransform>();
            if (m_RightCrosshairsImage != null) m_RightRectTransform = m_RightCrosshairsImage.GetComponent<RectTransform>();
            if (m_BottomCrosshairsImage != null) m_BottomRectTransform = m_BottomCrosshairsImage.GetComponent<RectTransform>();

            m_RaycastHits = new RaycastHit[m_MaxCollisionCount];

            // Setup the crosshairs defaults.
            ResetMonitor();
        }

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<Item, int>(m_Character, "OnAbilityWillEquipItem", OnEquipItem);
                EventHandler.UnregisterEvent<Item, bool>(m_Character, "OnItemUpdateDominantItem", OnUpdateDominantItem);
                EventHandler.UnregisterEvent<Item, int>(m_Character, "OnAbilityUnequipItemComplete", OnUnequipItem);
                EventHandler.UnregisterEvent<Item, int>(m_Character, "OnInventoryRemoveItem", OnUnequipItem);
                EventHandler.UnregisterEvent<bool, bool>(m_Character, "OnAddCrosshairsSpread", OnAddCrosshairsSpread);
                EventHandler.UnregisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
                EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
                EventHandler.UnregisterEvent(m_Character, "OnRespawn", OnRespawn);
                ResetMonitor();
            }

            base.OnAttachCharacter(character);

            if (m_Character == null) {
                return;
            }

            m_Camera = UnityEngineUtility.FindCamera(m_Character);
            m_CameraController = m_Camera.gameObject.GetCachedComponent<CameraController>();
            m_CameraController.SetCrosshairs(transform);

            m_AimAssist = m_Camera.GetComponent<AimAssist>();
            m_CharacterTransform = m_Character.transform;
            m_CharacterLayerManager = m_Character.GetCachedComponent<CharacterLayerManager>();
            m_CharacterLocomotion = m_Character.GetCachedComponent<UltimateCharacterLocomotion>();
            m_EnableImage = false;

            EventHandler.RegisterEvent<Item, int>(m_Character, "OnAbilityWillEquipItem", OnEquipItem);
            EventHandler.RegisterEvent<Item, bool>(m_Character, "OnItemUpdateDominantItem", OnUpdateDominantItem);
            EventHandler.RegisterEvent<Item, int>(m_Character, "OnAbilityUnequipItemComplete", OnUnequipItem);
            EventHandler.RegisterEvent<Item, int>(m_Character, "OnInventoryRemoveItem", OnUnequipItem);
            EventHandler.RegisterEvent<bool, bool>(m_Character, "OnAddCrosshairsSpread", OnAddCrosshairsSpread);
            EventHandler.RegisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_Character, "OnRespawn", OnRespawn);

            // An item may already be equipped.
            var inventory = m_Character.GetCachedComponent<Inventory.InventoryBase>();
            if (inventory != null) {
                for (int i = 0; i < inventory.SlotCount; ++i) {
                    var item = inventory.GetActiveItem(i);
                    if (item != null) {
                        OnEquipItem(item, i);
                    }
                }
            }
        }

        /// <summary>
        /// Determine any targets that are within the crosshairs raycast.
        /// </summary>
        private void Update()
        {
            var crosshairsColor = m_DefaultColor;
            var crosshairsRay = m_Camera.ScreenPointToRay(m_CenterRectTransform.position);
            Transform target = null;
            // Prevent the ray between the character and the camera from causing a false collision.
            if (!m_CharacterLocomotion.FirstPersonPerspective) {
                var direction = m_CharacterTransform.InverseTransformPoint(crosshairsRay.origin);
                direction.y = 0;
                crosshairsRay.origin = crosshairsRay.GetPoint(direction.magnitude);
            }
#if UNITY_EDITOR
            // Visualize the direction of the look direction.
            if (m_DebugDrawLookRay) {
                Debug.DrawRay(crosshairsRay.origin, crosshairsRay.direction * m_CameraController.LookDirectionDistance, Color.white);
            }
#endif
            var hitCount = Physics.SphereCastNonAlloc(crosshairsRay, m_CollisionRadius, m_RaycastHits, m_CameraController.LookDirectionDistance, m_CharacterLayerManager.IgnoreInvisibleLayers, m_TriggerInteraction);
#if UNITY_EDITOR
            if (hitCount == m_MaxCollisionCount) {
                Debug.LogWarning("Warning: The crosshairs detected the maximum number of objects. Consider increasing the Max Collision Count on the Crosshairs Monitor.");
            }
#endif
            if (hitCount > 0) {
                for (int i = 0; i < hitCount; ++i) {
                    var closestRaycastHit = QuickSelect.SmallestK(m_RaycastHits, hitCount, i, m_RaycastHitComparer);
                    var closestRaycastHitTransform = closestRaycastHit.transform;
                    // The crosshairs cannot hit the character that is attached to the camera.
                    if (closestRaycastHitTransform.IsChildOf(m_CharacterTransform)) {
                        continue;
                    }

                    if (MathUtility.InLayerMask(closestRaycastHitTransform.gameObject.layer, m_CharacterLayerManager.EnemyLayers)) {
                        target = closestRaycastHitTransform;
                        crosshairsColor = m_TargetColor;
                    }
                    break;
                }
            }
            if (m_AimAssist != null) {
                m_AimAssist.SetTarget(target);
            }

            if (m_EquippedItem != null) {
                m_CurrentCrosshairsSpread = Mathf.SmoothDamp(m_CurrentCrosshairsSpread, m_TargetCrosshairsSpread, ref m_CrosshairsSpreadVelocity,
                                                m_EquippedItem.QuadrantSpreadDamping);
            }
            m_CenterCrosshairsImage.color = crosshairsColor;
            if (m_LeftCrosshairsImage != null && m_LeftCrosshairsImage.enabled) {
                m_LeftCrosshairsImage.color = crosshairsColor;
                PositionSprite(m_LeftRectTransform, -m_EquippedItem.QuadrantOffset - m_CurrentCrosshairsSpread, 0);
            }
            if (m_TopCrosshairsImage != null && m_TopCrosshairsImage.enabled) {
                m_TopCrosshairsImage.color = crosshairsColor;
                PositionSprite(m_TopRectTransform, 0, m_EquippedItem.QuadrantOffset + m_CurrentCrosshairsSpread);
            }
            if (m_RightCrosshairsImage != null && m_RightCrosshairsImage.enabled) {
                m_RightCrosshairsImage.color = crosshairsColor;
                PositionSprite(m_RightRectTransform, m_EquippedItem.QuadrantOffset + m_CurrentCrosshairsSpread, 0);
            }
            if (m_BottomCrosshairsImage != null && m_BottomCrosshairsImage.enabled) {
                m_BottomCrosshairsImage.color = crosshairsColor;
                PositionSprite(m_BottomRectTransform, 0, -m_EquippedItem.QuadrantOffset - m_CurrentCrosshairsSpread);
            }

            var enableImage = !m_Aiming || (m_EquippedItem != null && m_EquippedItem.ShowCrosshairsOnAim);
            if (enableImage != m_EnableImage) {
                m_EnableImage = enableImage;
                EnableCrosshairsImage(enableImage);
            }
        }

        /// <summary>
        /// An item has been equipped.
        /// </summary>
        /// <param name="item">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(Item item, int slotID)
        {
            if (!item.DominantItem) {
                return;
            }

            m_CurrentCrosshairsSpread = m_TargetCrosshairsSpread = 0;
            m_EquippedItem = item;
            m_CenterCrosshairsImage.sprite = m_EquippedItem.CenterCrosshairs != null ? m_EquippedItem.CenterCrosshairs : m_DefaultSprite;
            if (m_CenterCrosshairsImage.sprite != null) {
                m_CenterCrosshairsImage.enabled = !m_Aiming || m_EquippedItem.ShowCrosshairsOnAim;
                UnityEngineUtility.SizeSprite(m_CenterCrosshairsImage.sprite, m_CenterRectTransform);
            } else {
                m_CenterCrosshairsImage.enabled = false;
            }
            if (m_LeftCrosshairsImage != null) {
                m_LeftCrosshairsImage.sprite = m_EquippedItem.LeftCrosshairs;
                if (m_LeftCrosshairsImage.sprite != null) {
                    m_LeftCrosshairsImage.enabled = !m_Aiming || m_EquippedItem.ShowCrosshairsOnAim;
                    PositionSprite(m_LeftRectTransform, -m_EquippedItem.QuadrantOffset, 0);
                    UnityEngineUtility.SizeSprite(m_LeftCrosshairsImage.sprite, m_LeftRectTransform);
                } else {
                    m_LeftCrosshairsImage.enabled = false;
                }
            }
            if (m_TopCrosshairsImage != null) {
                m_TopCrosshairsImage.sprite = m_EquippedItem.TopCrosshairs;
                if (m_TopCrosshairsImage.sprite != null) {
                    m_TopCrosshairsImage.enabled = !m_Aiming || m_EquippedItem.ShowCrosshairsOnAim;
                    PositionSprite(m_TopRectTransform, 0, m_EquippedItem.QuadrantOffset);
                    UnityEngineUtility.SizeSprite(m_TopCrosshairsImage.sprite, m_TopRectTransform);
                } else {
                    m_TopCrosshairsImage.enabled = false;
                }
            }
            if (m_RightCrosshairsImage != null) {
                m_RightCrosshairsImage.sprite = m_EquippedItem.RightCrosshairs;
                if (m_RightCrosshairsImage.sprite != null) {
                    m_RightCrosshairsImage.enabled = !m_Aiming || m_EquippedItem.ShowCrosshairsOnAim;
                    PositionSprite(m_RightRectTransform, m_EquippedItem.QuadrantOffset, 0);
                    UnityEngineUtility.SizeSprite(m_RightCrosshairsImage.sprite, m_RightRectTransform);
                } else {
                    m_RightCrosshairsImage.enabled = false;
                }
            }
            if (m_BottomCrosshairsImage != null) {
                m_BottomCrosshairsImage.sprite = m_EquippedItem.BottomCrosshairs;
                if (m_BottomCrosshairsImage.sprite != null) {
                    m_BottomCrosshairsImage.enabled = !m_Aiming || m_EquippedItem.ShowCrosshairsOnAim;
                    PositionSprite(m_BottomRectTransform, 0, -m_EquippedItem.QuadrantOffset);
                    UnityEngineUtility.SizeSprite(m_BottomCrosshairsImage.sprite, m_BottomRectTransform);
                } else {
                    m_BottomCrosshairsImage.enabled = false;
                }
            }
            m_EquippedItemCount++;
        }

        /// <summary>
        /// The DominantItem field has been updated for the specified item.
        /// </summary>
        /// <param name="item">The Item whose DominantItem field was updated.</param>
        /// <param name="dominantItem">True if the item is now a dominant item.</param>
        private void OnUpdateDominantItem(Item item, bool dominantItem)
        {
            if (item.DominantItem && item.IsActive()) {
                OnEquipItem(item, -1);
            } else if (m_EquippedItem == item) {
                m_EquippedItemCount--;
                if (m_EquippedItemCount == 0) {
                    ResetMonitor();
                }
            }
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="item">The unequipped item.</param>
        /// <param name="slotID">The slot that the item previously occupied.</param>
        private void OnUnequipItem(Item item, int slotID)
        {
            if (item != m_EquippedItem) {
                return;
            }

            m_EquippedItemCount--;
            if (m_EquippedItemCount == 0) {
                ResetMonitor();
            }
        }

        /// <summary>
        /// Resets the monitor back to the default state.
        /// </summary>
        private void ResetMonitor()
        {
            m_EquippedItem = null;
            m_EquippedItemCount = 0;
            m_CenterCrosshairsImage.sprite = m_DefaultSprite;
            m_CenterCrosshairsImage.enabled = m_DefaultSprite != null;
            if (m_LeftCrosshairsImage != null) {
                m_LeftCrosshairsImage.sprite = null;
                m_LeftCrosshairsImage.enabled = false;
            }
            if (m_TopCrosshairsImage != null) {
                m_TopCrosshairsImage.sprite = null;
                m_TopCrosshairsImage.enabled = false;
            }
            if (m_RightCrosshairsImage != null) {
                m_RightCrosshairsImage.sprite = null;
                m_RightCrosshairsImage.enabled = false;
            }
            if (m_BottomCrosshairsImage != null) {
                m_BottomCrosshairsImage.sprite = null;
                m_BottomCrosshairsImage.enabled = false;
            }
        }

        /// <summary>
        /// Positions the sprite according to the specified x and y position.
        /// </summary>
        /// <param name="spriteRectTransform">The transform to position.</param>
        /// <param name="xPosition">The x position of the sprite.</param>
        /// <param name="yPosition">The y position of the sprite.</param>
        private void PositionSprite(RectTransform spriteRectTransform, float xPosition, float yPosition)
        {
            var position = spriteRectTransform.localPosition;
            position.x = xPosition;
            position.y = yPosition;
            spriteRectTransform.localPosition = position;
        }

        /// <summary>
        /// Adds a force to the quadrant recoil spring.
        /// </summary>
        /// <param name="start">Is the spread just starting?</param>
        /// <param name="fromRecoil">Is the spread being added from a recoil?</param>
        private void OnAddCrosshairsSpread(bool start, bool fromRecoil)
        {
            if (m_EquippedItem == null) {
                return;
            }

            if (start) {
                m_CurrentCrosshairsSpread = fromRecoil ? m_EquippedItem.MaxQuadrantSpread : 0;
                m_TargetCrosshairsSpread = fromRecoil ? 0 : m_EquippedItem.MaxQuadrantSpread;
            } else {
                m_TargetCrosshairsSpread = 0;
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
        }

        /// <summary>
        /// Enables or disables the crosshairs image.
        /// </summary>
        /// <param name="enable">Should the crosshairs be enabled?</param>
        private void EnableCrosshairsImage(bool enable)
        {
            m_CenterCrosshairsImage.enabled = enable && m_CenterCrosshairsImage.sprite != null;
            if (m_LeftCrosshairsImage != null) m_LeftCrosshairsImage.enabled = enable && m_LeftCrosshairsImage.sprite != null;
            if (m_TopCrosshairsImage != null) m_TopCrosshairsImage.enabled = enable && m_TopCrosshairsImage.sprite != null;
            if (m_RightCrosshairsImage != null) m_RightCrosshairsImage.enabled = enable && m_RightCrosshairsImage.sprite != null;
            if (m_BottomCrosshairsImage != null) m_BottomCrosshairsImage.enabled = enable && m_BottomCrosshairsImage.sprite != null;
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            if (m_DisableOnDeath) {
                m_GameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            if (m_DisableOnDeath && base.CanShowUI()) {
                m_GameObject.SetActive(true);
            }

            // Force the crosshairs to update so the color will be correct.
            Update();
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected override bool CanShowUI()
        {
            return base.CanShowUI() && (!m_DisableOnDeath || m_CharacterLocomotion.Alive);
        }
    }
}