/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.SurfaceSystem
{
    /// <summary>
    /// The SurfaceType is the main surface concept used for spawning objects based on impact.
    /// </summary>
    public class SurfaceType : ScriptableObject
    {
        [Tooltip("The SurfaceImpactEffects array maps the SurfaceImpact object with the SurfaceEffect object.")]
        [SerializeField] protected ImpactEffect[] m_ImpactEffects;
        [Tooltip("Does the SurfaceType allow footprints?")]
        [SerializeField] protected bool m_AllowFootprints = true;

        public ImpactEffect[] ImpactEffects { get { return m_ImpactEffects; } }
        public bool AllowFootprints { get { return m_AllowFootprints; } }
    }
}