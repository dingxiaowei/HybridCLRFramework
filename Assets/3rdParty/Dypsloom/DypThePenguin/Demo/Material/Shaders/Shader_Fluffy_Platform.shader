// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Shader_Fluffy_Platform"
{
	Properties
	{
		_Albedo("Albedo", Color) = (0,0,0,0)
		[HDR]_BlendColor("Blend Color", Color) = (1,0,0,0)
		[HDR]_BlendColor2("Blend Color 2", Color) = (0.08960724,1,0,0)
		_TopColor("Top Color", Color) = (0,0,0,0)
		_BlendTransitionRatio("Blend Transition Ratio", Float) = 3.69
		_BlendTransitionOffset("Blend Transition Offset", Float) = -0.55
		_SummedBlend("SummedBlend", Vector) = (0,0,0,0)
		_NoiseColor("Noise Color", Color) = (1,1,1,1)
		_ColorNoiseValue("Color Noise Value", Float) = 0.22
		_ColorNoiseScale("Color Noise Scale", Float) = 6.89
		_DeformationScale("Deformation Scale", Float) = 0
		_YDeformation("Y Deformation", Float) = 0
		_DeformationNoiseValue("Deformation Noise Value", Float) = 0.47
		_DeformationNoiseScale("Deformation Noise Scale", Float) = 1.09
		_TimeOffset("Time Offset", Vector) = (0.21,0.12,0.15,0)
		_YLerpConstant("Y Lerp Constant", Float) = 0
		_FresnelColor("Fresnel Color", Color) = (0.7028302,0.965398,1,0)
		_Bias("Bias", Float) = 0
		_Power("Power", Float) = 0
		_Scale("Scale", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float3 _TimeOffset;
		uniform float _DeformationNoiseScale;
		uniform float _DeformationNoiseValue;
		uniform float _YLerpConstant;
		uniform float _YDeformation;
		uniform float _DeformationScale;
		uniform float4 _Albedo;
		uniform float _Bias;
		uniform float _Scale;
		uniform float _Power;
		uniform float4 _FresnelColor;
		uniform float2 _SummedBlend;
		uniform float4 _BlendColor;
		uniform float4 _BlendColor2;
		uniform float4 _TopColor;
		uniform float _BlendTransitionOffset;
		uniform float _BlendTransitionRatio;
		uniform float _ColorNoiseScale;
		uniform float _ColorNoiseValue;
		uniform float4 _NoiseColor;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float simplePerlin2D23 = snoise( (mul( float4( ase_vertex3Pos , 0.0 ), unity_WorldToObject ).xyz*1.0 + ( _Time.y * _TimeOffset )).xy*_DeformationNoiseScale );
			simplePerlin2D23 = simplePerlin2D23*0.5 + 0.5;
			float temp_output_17_0 = ( simplePerlin2D23 * _DeformationNoiseValue );
			float3 ase_vertexNormal = v.normal.xyz;
			float4 appendResult13 = (float4(( temp_output_17_0 + ase_vertexNormal.x ) , ase_vertexNormal.y , ( temp_output_17_0 + ase_vertexNormal.z ) , 0.0));
			float4 lerpResult27 = lerp( appendResult13 , float4( 0,0,0,0 ) , ( mul( float4( ase_vertex3Pos , 0.0 ), unity_WorldToObject ).xyz.y + _YLerpConstant ));
			float4 break37 = lerpResult27;
			float4 appendResult11 = (float4(break37.x , ( break37.y * _YDeformation ) , break37.z , 0.0));
			v.vertex.xyz += ( appendResult11 * _DeformationScale ).xyz;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _Albedo.rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV81 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode81 = ( _Bias + _Scale * pow( 1.0 - fresnelNdotV81, _Power ) );
			float4 lerpResult101 = lerp( _BlendColor , _BlendColor2 , float4( 0,0,0,0 ));
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float clampResult147 = clamp( ( ( ase_vertex3Pos.y + _BlendTransitionOffset ) * _BlendTransitionRatio ) , 0.0 , 1.0 );
			float4 lerpResult119 = lerp( lerpResult101 , _TopColor , clampResult147);
			float simplePerlin2D70 = snoise( (ase_worldPos*1.0 + ( _Time.y * float3(0.3,0.3,0.3) )).xy*_ColorNoiseScale );
			simplePerlin2D70 = simplePerlin2D70*0.5 + 0.5;
			float temp_output_76_0 = ( simplePerlin2D70 + _ColorNoiseValue );
			float2 weightedBlendVar91 = _SummedBlend;
			float4 weightedBlend91 = ( weightedBlendVar91.x*( ( 1.0 - fresnelNode81 ) * lerpResult119 ) + weightedBlendVar91.y*( ( temp_output_76_0 * lerpResult119 ) + ( ( 1.0 - temp_output_76_0 ) * _NoiseColor ) ) );
			o.Emission = ( ( fresnelNode81 * _FresnelColor ) + weightedBlend91 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=17800
0;0;1920;1019;4877.978;1419.974;2.224897;True;False
Node;AmplifyShaderEditor.Vector3Node;60;-2480.1,1731.854;Inherit;False;Property;_TimeOffset;Time Offset;14;0;Create;True;0;0;False;0;0.21,0.12,0.15;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldToObjectMatrix;53;-2474.745,1552.783;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.PosVertexDataNode;148;-2980.859,1624.387;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;19;-2484.256,1642.397;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-2287.643,1305.101;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-2272.371,1499.341;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;113;-3023.674,-630.9788;Inherit;False;Constant;_Timemultiplier;Time multiplier;21;0;Create;True;0;0;False;0;0.3,0.3,0.3;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;64;-3041.279,-764.0706;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-2090.216,1148.469;Inherit;False;Property;_DeformationNoiseScale;Deformation Noise Scale;13;0;Create;True;0;0;False;0;1.09;1.04;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;24;-2104.871,1334.456;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-2652.022,-739.2516;Inherit;False;Constant;_Float5;Float 5;18;0;Create;True;0;0;False;0;1;1.12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;23;-1837.968,1096.006;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0.52,0;False;1;FLOAT;6.14;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-2826.19,-703.0715;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1710.078,1374.525;Inherit;False;Property;_DeformationNoiseValue;Deformation Noise Value;12;0;Create;True;0;0;False;0;0.47;0.47;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;146;-1653.939,674.3532;Inherit;False;Property;_BlendTransitionOffset;Blend Transition Offset;5;0;Create;True;0;0;False;0;-0.55;0.46;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;104;-2623.456,-89.53333;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;67;-2842.72,-915.3392;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldToObjectMatrix;32;-1056.416,2038.434;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.RangedFloatNode;144;-1602.083,226.9059;Inherit;False;Property;_BlendTransitionRatio;Blend Transition Ratio;4;0;Create;True;0;0;False;0;3.69;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;69;-2441.858,-757.2795;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-1522.501,1338.278;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;36;-1516.278,1575.477;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-876.9142,1938.953;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-2460.817,-997.7123;Inherit;False;Property;_ColorNoiseScale;Color Noise Scale;9;0;Create;True;0;0;False;0;6.89;1.04;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;145;-1428.526,509.8255;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;18;-1255.886,1341.311;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;31;-1264.632,1686.467;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;35;-733.7036,1922.51;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;71;-2112.013,-734.9296;Inherit;False;Property;_ColorNoiseValue;Color Noise Value;8;0;Create;True;0;0;False;0;0.22;0.22;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;70;-2190.757,-1038.288;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0.52,0;False;1;FLOAT;6.14;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-664.3007,2052.636;Inherit;False;Property;_YLerpConstant;Y Lerp Constant;15;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;102;-2856.597,-387.11;Inherit;False;Property;_BlendColor;Blend Color;1;1;[HDR];Create;True;0;0;False;0;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;-1183.602,466.4172;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;103;-2878.415,-225.786;Inherit;False;Property;_BlendColor2;Blend Color 2;2;1;[HDR];Create;True;0;0;False;0;0.08960724,1,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;75;-1614.979,-238.3777;Inherit;False;Property;_Scale;Scale;19;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;13;-951.6201,1555.307;Inherit;True;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ClampOpNode;147;-927.0732,409.8828;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;101;-2093.445,-459.0439;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-1612.979,-315.3778;Inherit;False;Property;_Bias;Bias;17;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;120;-2083.483,-299.2288;Inherit;False;Property;_TopColor;Top Color;3;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;73;-1613.979,-152.3779;Inherit;False;Property;_Power;Power;18;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;76;-1869.797,-899.8945;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;57;-461.1426,1962.575;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;79;-1574.141,-1034.448;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;119;-1826.106,-495.0383;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;81;-1466.661,-324.1444;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;2;False;3;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;27;-263.6581,1578.046;Inherit;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;78;-1579.241,-812.2476;Inherit;False;Property;_NoiseColor;Noise Color;7;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;56;-55.7423,1845.488;Inherit;False;Property;_YDeformation;Y Deformation;11;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;84;-1141.956,-177.3265;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;82;-1557.289,-623.5026;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;37;-28.13745,1577.17;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-1320.107,-909.6946;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;89;-948.8508,-321.4597;Inherit;False;Property;_FresnelColor;Fresnel Color;16;0;Create;True;0;0;False;0;0.7028302,0.965398,1,0;0.7028302,0.965398,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;130.3555,1824.026;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;88;-1078.912,-690.8631;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;-914.4101,-106.7062;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;86;-551.3734,58.84557;Inherit;False;Property;_SummedBlend;SummedBlend;6;0;Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode;11;219.3599,1577.328;Inherit;True;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;3;300.0188,1896.663;Inherit;False;Property;_DeformationScale;Deformation Scale;10;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SummedBlendNode;91;-342.805,-43.47594;Inherit;False;5;0;FLOAT2;0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-528.6299,-409.4238;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;520.1465,1747.032;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;99;-190.7209,-315.2736;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;10;-164.1445,-737.7386;Inherit;False;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;54;-2467.175,1262.09;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;34;-2380.743,2053.641;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;816.235,127.2633;Float;False;True;-1;2;;0;0;Standard;Custom/Shader_Fluffy_Platform;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;52;0;148;0
WireConnection;52;1;53;0
WireConnection;25;0;19;0
WireConnection;25;1;60;0
WireConnection;24;0;52;0
WireConnection;24;2;25;0
WireConnection;23;0;24;0
WireConnection;23;1;22;0
WireConnection;65;0;64;0
WireConnection;65;1;113;0
WireConnection;69;0;67;0
WireConnection;69;1;66;0
WireConnection;69;2;65;0
WireConnection;17;0;23;0
WireConnection;17;1;16;0
WireConnection;33;0;148;0
WireConnection;33;1;32;0
WireConnection;145;0;104;2
WireConnection;145;1;146;0
WireConnection;18;0;17;0
WireConnection;18;1;36;1
WireConnection;31;0;17;0
WireConnection;31;1;36;3
WireConnection;35;0;33;0
WireConnection;70;0;69;0
WireConnection;70;1;68;0
WireConnection;143;0;145;0
WireConnection;143;1;144;0
WireConnection;13;0;18;0
WireConnection;13;1;36;2
WireConnection;13;2;31;0
WireConnection;147;0;143;0
WireConnection;101;0;102;0
WireConnection;101;1;103;0
WireConnection;76;0;70;0
WireConnection;76;1;71;0
WireConnection;57;0;35;1
WireConnection;57;1;58;0
WireConnection;79;0;76;0
WireConnection;119;0;101;0
WireConnection;119;1;120;0
WireConnection;119;2;147;0
WireConnection;81;1;74;0
WireConnection;81;2;75;0
WireConnection;81;3;73;0
WireConnection;27;0;13;0
WireConnection;27;2;57;0
WireConnection;84;0;81;0
WireConnection;82;0;76;0
WireConnection;82;1;119;0
WireConnection;37;0;27;0
WireConnection;83;0;79;0
WireConnection;83;1;78;0
WireConnection;55;0;37;1
WireConnection;55;1;56;0
WireConnection;88;0;82;0
WireConnection;88;1;83;0
WireConnection;87;0;84;0
WireConnection;87;1;119;0
WireConnection;11;0;37;0
WireConnection;11;1;55;0
WireConnection;11;2;37;2
WireConnection;91;0;86;0
WireConnection;91;1;87;0
WireConnection;91;2;88;0
WireConnection;100;0;81;0
WireConnection;100;1;89;0
WireConnection;2;0;11;0
WireConnection;2;1;3;0
WireConnection;99;0;100;0
WireConnection;99;1;91;0
WireConnection;0;0;10;0
WireConnection;0;2;99;0
WireConnection;0;11;2;0
ASEEND*/
//CHKSM=3E30BB51B928E1F8EBA3B866C7424DEA0B3B4168