// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Ultimate Character Controller/Demo/ShieldBubble"
{
	Properties
	{
		_DistortionAmount("Distortion Amount", Range( 0 , 1)) = 0.1
		_TextureSample1("Texture Sample 1", 2D) = "bump" {}
		_DepthFade("DepthFade", Float) = 0.75
		_Opacity("Opacity", Range( 0 , 1)) = 0.5
		_FadeColor("FadeColor", Color) = (0,0,0,0)
		_TimeScale("Time Scale", Float) = 0.2
		_DistortionColor("DistortionColor", Color) = (0,0,0,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		GrabPass{ }
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf Standard alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float3 worldPos;
			float3 viewDir;
			float3 worldNormal;
			half ASEVFace : VFACE;
		};

		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _DistortionAmount;
		uniform sampler2D _TextureSample1;
		uniform float _TimeScale;
		uniform float4 _DistortionColor;
		uniform float4 _FadeColor;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _DepthFade;
		uniform float _Opacity;


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float mulTime70 = _Time.y * _TimeScale;
			float cos73 = cos( mulTime70 );
			float sin73 = sin( mulTime70 );
			float2 rotator73 = mul( i.uv_texcoord - float2( 0.5,0.5 ) , float2x2( cos73 , -sin73 , sin73 , cos73 )) + float2( 0.5,0.5 );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float4 screenColor78 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( float4( UnpackScaleNormal( tex2D( _TextureSample1, rotator73 ), _DistortionAmount ) , 0.0 ) + ase_grabScreenPosNorm ).xy);
			o.Albedo = ( float4( (screenColor78).rgb , 0.0 ) * _DistortionColor ).rgb;
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth9 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos )));
			float distanceDepth9 = abs( ( screenDepth9 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFade ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV1 = dot( ase_worldNormal, i.viewDir );
			float fresnelNode1 = ( 0.0 + 1.15 * pow( 1.0 - fresnelNdotV1, 2.8 ) );
			float4 switchResult65 = (((i.ASEVFace>0)?(( fresnelNode1 * _FadeColor )):(float4( 0,0,0,0 ))));
			o.Emission = ( ( _FadeColor * ( 1.0 - saturate( distanceDepth9 ) ) ) + switchResult65 ).rgb;
			o.Alpha = _Opacity;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16900
267;262;1269;828;2179.027;1086.33;3.1413;True;True
Node;AmplifyShaderEditor.CommentaryNode;68;-1129.4,-849.4218;Float;False;1820.48;460.2398;;13;81;79;78;77;76;75;74;73;72;71;70;69;83;Distortion;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-1079.4,-543.4218;Float;False;Property;_TimeScale;Time Scale;5;0;Create;True;0;0;False;0;0.2;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;72;-967.3995,-799.4218;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;71;-919.3995,-671.4218;Float;False;Constant;_Vector0;Vector 0;-1;0;Create;True;0;0;False;0;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;85;-1115.655,-315.3779;Float;False;1052.848;330.9797;;5;15;16;61;9;10;Intersection Fading;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;70;-919.3995,-543.4218;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;84;-1144.358,87.49419;Float;False;1138.397;490.3369;;5;66;65;4;1;64;Rim Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-1079.896,-181.2764;Float;False;Property;_DepthFade;DepthFade;2;0;Create;True;0;0;False;0;0.75;0.75;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-695.3995,-591.4218;Float;False;Property;_DistortionAmount;Distortion Amount;0;0;Create;True;0;0;False;0;0.1;0.011;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;73;-695.3995,-719.4218;Float;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GrabScreenPosition;75;-343.3995,-575.4218;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;76;-407.3995,-783.4218;Float;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;None;302951faffe230848aa0d3df7bb70faa;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;64;-1028.218,364.6009;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DepthFade;9;-839.0717,-221.4737;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;77;-87.39868,-687.4218;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;61;-554.0582,-215.1548;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-1388.261,-30.19072;Float;False;Property;_FadeColor;FadeColor;4;0;Create;True;0;0;False;0;0,0,0,0;0,1,0.9921569,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;1;-739.9854,286.3983;Float;True;Standard;TangentNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1.15;False;3;FLOAT;2.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;78;58.51733,-762.0266;Float;False;Global;_GrabScreen0;Grab Screen 0;2;0;Create;True;0;0;False;0;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;16;-388.5641,-216.364;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-501.3401,177.0547;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SwitchByFaceNode;65;-234.417,328.2799;Float;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-235.9531,-225.5644;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;79;273.5635,-751.5845;Float;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;83;212.5284,-618.2986;Float;False;Property;_DistortionColor;DistortionColor;6;0;Create;True;0;0;False;0;0,0,0,1;0,1,0.0323627,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;513.5764,-680.6417;Float;True;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;66;-193.6022,204.1944;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-153.0929,623.1542;Float;False;Property;_Opacity;Opacity;3;0;Create;True;0;0;False;0;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;802.9553,-258.8602;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Ultimate Character Controller/Demo/ShieldBubble;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;1;0,0,0,0;VertexScale;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;0;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;70;0;69;0
WireConnection;73;0;72;0
WireConnection;73;1;71;0
WireConnection;73;2;70;0
WireConnection;76;1;73;0
WireConnection;76;5;74;0
WireConnection;9;0;10;0
WireConnection;77;0;76;0
WireConnection;77;1;75;0
WireConnection;61;0;9;0
WireConnection;1;4;64;0
WireConnection;78;0;77;0
WireConnection;16;0;61;0
WireConnection;4;0;1;0
WireConnection;4;1;13;0
WireConnection;65;0;4;0
WireConnection;15;0;13;0
WireConnection;15;1;16;0
WireConnection;79;0;78;0
WireConnection;81;0;79;0
WireConnection;81;1;83;0
WireConnection;66;0;15;0
WireConnection;66;1;65;0
WireConnection;0;0;81;0
WireConnection;0;2;66;0
WireConnection;0;9;3;0
ASEEND*/
//CHKSM=D008FAC91B02A1BF517501F1AB9E7D3024F46DA0