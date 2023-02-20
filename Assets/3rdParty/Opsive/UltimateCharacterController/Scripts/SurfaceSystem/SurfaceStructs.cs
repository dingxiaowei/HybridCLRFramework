/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.SurfaceSystem
{
    /// <summary>
    /// A ImpactEffect pairs the SurfaceImpact with a SurfaceEffect.
    /// </summary>
    [System.Serializable]
    public struct ImpactEffect
    {
#pragma warning disable 0649
        [Tooltip("The SurfaceImpact which triggers the SurfaceEffect.")]
        [SerializeField] private SurfaceImpact m_SurfaceImpact;
        [Tooltip("The SurfaceEffect to spawn when triggered by the SurfaceImpact.")]
        [SerializeField] private SurfaceEffect m_SurfaceEffect;
#pragma warning restore 0649

        public SurfaceImpact SurfaceImpact { get { return m_SurfaceImpact; } }
        public SurfaceEffect SurfaceEffect { get { return m_SurfaceEffect; } }
    }

    /// <summary>
    /// Maps a texture to a set of UV coordinates.
    /// </summary>
    [System.Serializable]
    public struct UVTexture
    {
        [Tooltip("The texture to map the UV coordinates to.")]
        [SerializeField] private Texture m_Texture;
        [Tooltip("The UV coordinates of the texture.")]
        [SerializeField] private Rect m_UV;

        public Texture Texture { get { return m_Texture; } set { m_Texture = value; } }
        public Rect UV { get { return m_UV; } set { m_UV = value; } }
    }

    /// <summary>
    /// Represets a default surface listed within the SurfaceManager.
    /// </summary>
    [System.Serializable]
    public struct ObjectSurface
    {
        [Tooltip("The type of surface represented.")]
        [SerializeField] private SurfaceType m_SurfaceType;
        [Tooltip("The textures which go along with the specified SurfaceType.")]
        [SerializeField] private UVTexture[] m_UVTextures;

        public SurfaceType SurfaceType { get { return m_SurfaceType; } set { m_SurfaceType = value; } }
        public UVTexture[] UVTextures { get { return m_UVTextures; } set { m_UVTextures = value; } }
    }
}