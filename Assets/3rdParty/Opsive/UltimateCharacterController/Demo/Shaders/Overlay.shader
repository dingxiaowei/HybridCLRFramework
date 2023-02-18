// Surface shader that will display an overlay texture.
Shader "Ultimate Character Controller/Demo/Overlay" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_OverlayTex ("Overlay (RGBA)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _OverlayTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_OverlayTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from adding the main texture to an overlay texture. The color then allows the result to be tinted.
			fixed4 mainTex = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 overlayTex = tex2D(_OverlayTex, IN.uv_OverlayTex);
			o.Albedo = ((mainTex.rgb * (1 - overlayTex.a)) + (overlayTex.rgb * overlayTex.a)) * _Color.rgb;
			o.Alpha = _Color.a;

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
