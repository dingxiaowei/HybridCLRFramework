/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Items.Actions;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Items
{
    /// <summary>
    /// Describes any third person perspective dependent properties for the MeleeWeapon.
    /// </summary>
    public class ThirdPersonMeleeWeaponProperties : ThirdPersonWeaponProperties, IMeleeWeaponPerspectiveProperties
    {
        [Tooltip("An array of hitboxes that the MeleeWeapon detects collisions with.")]
        [SerializeField] protected MeleeWeapon.MeleeHitbox[] m_Hitboxes;
        [Tooltip("The location that the melee weapon trail is spawned at.")]
        [SerializeField] protected Transform m_TrailLocation;

        public MeleeWeapon.MeleeHitbox[] Hitboxes { get { return m_Hitboxes; } set { m_Hitboxes = value; } }
        public Transform TrailLocation { get { return m_TrailLocation; } set { m_TrailLocation = value; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < m_Hitboxes.Length; ++i) {
                m_Hitboxes[i].Initialize(m_CharacterTransform);
            }
        }
    }
}