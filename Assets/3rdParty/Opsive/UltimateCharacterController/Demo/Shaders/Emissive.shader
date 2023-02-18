// Surface shader that will display an overlay texture.
Shader "Ultimate Character Controller/Demo/Emissive" {
  Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Emissive (RGB)", 2D) = "white" {}
    [HDR]_EmissiveColor ("Emissive Color", Color) = (1,1,1,1)
    _EmissiveSpeed("Emissive Speed", Float) = 0.0
  }
  SubShader {
    Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
    LOD 200
    
    CGPROGRAM
    // Physically based Standard lighting model, and enable shadows on all light types
    #pragma surface surf Standard fullforwardshadows 

    // Use shader model 3.0 target, to get nicer looking lighting
    #pragma target 3.0

    sampler2D _MainTex;

    struct Input {
      float2 uv_MainTex;
    };

    fixed4 _Color;
    fixed4 _EmissiveColor;
    fixed _EmissiveSpeed;

    UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_INSTANCING_BUFFER_END(Props)

    void surf (Input IN, inout SurfaceOutputStandard o) {
            o.Albedo = _Color.rgb;
            float2 emissiveUV = float2(IN.uv_MainTex.x +( (_Time.y - floor(_Time.y)) * _EmissiveSpeed), IN.uv_MainTex.y);
            fixed4 emissive = tex2D(_MainTex, emissiveUV) * _EmissiveColor;
            o.Emission = emissive.rgb;
    }
    ENDCG
  }
  FallBack "Diffuse"
}