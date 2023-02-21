// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Cloud"
{
	Properties
	{
		[HDR]_BlendColor1("Blend Color 1", Color) = (1,0,0,0)
		[HDR]_BlendColor2("Blend Color 2", Color) = (0.2180707,1,0,0)
		_BlendScale("Blend Scale", Float) = 0
		_BlendOffset("Blend Offset", Float) = 0
		_SummedWeight("Summed Weight", Vector) = (1,0.5,0,0)
		_TimeOffset("Time Offset", Vector) = (0.3,0.3,0,0)
		_EdgeLength ( "Edge length", Range( 2, 50 ) ) = 15
		_DeformationScale("Deformation Scale", Float) = 0.1
		[HDR]_FresnelColor("Fresnel Color", Color) = (0.7028302,0.965398,1,0)
		[HDR]_NoiseColor("Noise Color", Color) = (1,1,1,1)
		_NoiseColorScale("Noise Color Scale", Float) = 0.22
		_NoiseSize("Noise Size", Float) = 1.04
		_Bias("Bias", Float) = 0
		_Power("Power", Float) = 0
		_Scale("Scale", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "Tessellation.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float2 _TimeOffset;
		uniform float _NoiseSize;
		uniform float _DeformationScale;
		uniform float4 _FresnelColor;
		uniform float _Bias;
		uniform float _Scale;
		uniform float _Power;
		uniform float2 _SummedWeight;
		uniform float4 _BlendColor2;
		uniform float4 _BlendColor1;
		uniform float _BlendOffset;
		uniform float _BlendScale;
		uniform float _NoiseColorScale;
		uniform float4 _NoiseColor;
		uniform float _EdgeLength;


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


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess (v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float simplePerlin2D108 = snoise( (ase_worldPos*0.12 + float3( ( _Time.y * _TimeOffset ) ,  0.0 )).xy*_NoiseSize );
			simplePerlin2D108 = simplePerlin2D108*0.5 + 0.5;
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( ( simplePerlin2D108 * ase_vertexNormal ) * _DeformationScale );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV117 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode117 = ( _Bias + _Scale * pow( 1.0 - fresnelNdotV117, _Power ) );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float4 lerpResult219 = lerp( _BlendColor2 , _BlendColor1 , ( ( ase_vertex3Pos.y + _BlendOffset ) * _BlendScale ));
			float simplePerlin2D108 = snoise( (ase_worldPos*0.12 + float3( ( _Time.y * _TimeOffset ) ,  0.0 )).xy*_NoiseSize );
			simplePerlin2D108 = simplePerlin2D108*0.5 + 0.5;
			float temp_output_138_0 = ( simplePerlin2D108 + _NoiseColorScale );
			float2 weightedBlendVar215 = _SummedWeight;
			float4 weightedBlend215 = ( weightedBlendVar215.x*( ( 1.0 - fresnelNode117 ) * lerpResult219 ) + weightedBlendVar215.y*( ( temp_output_138_0 * lerpResult219 ) + ( ( 1.0 - temp_output_138_0 ) * _NoiseColor ) ) );
			o.Emission = ( ( _FresnelColor * fresnelNode117 ) + weightedBlend215 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc tessellate:tessFunction 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6
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
				vertexDataFunc( v );
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
0;0;1920;1019;4325.688;2616.382;2.98018;True;False
Node;AmplifyShaderEditor.SimpleTimeNode;97;-2775.335,-1730.959;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;98;-2773.335,-1649.56;Inherit;False;Property;_TimeOffset;Time Offset;5;0;Create;True;0;0;False;0;0.3,0.3;1,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;140;-2386.079,-1706.14;Inherit;False;Constant;_NoiseSpeed;Noise Speed;19;0;Create;True;0;0;False;0;0.12;1.12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-2560.247,-1669.96;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldPosInputsNode;36;-2575.631,-1834.073;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;107;-2165.564,-1839.458;Inherit;False;Property;_NoiseSize;Noise Size;15;0;Create;True;0;0;False;0;1.04;1.04;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;222;-2163.109,-951.3041;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;101;-2175.915,-1724.168;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;225;-2012.373,-753.8071;Inherit;False;Property;_BlendOffset;Blend Offset;3;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;139;-1608.016,-1664.75;Inherit;False;Property;_NoiseColorScale;Noise Color Scale;14;0;Create;True;0;0;False;0;0.22;0.22;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;226;-1864.947,-756.9438;Inherit;False;Property;_BlendScale;Blend Scale;2;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;108;-1948.237,-1827.152;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0.52,0;False;1;FLOAT;6.14;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;223;-1938.659,-883.981;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;218;-2101.633,-1352.822;Inherit;False;Property;_BlendColor2;Blend Color 2;1;1;[HDR];Create;True;0;0;False;0;0.2180707,1,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;204;-1215.06,-1198.172;Inherit;False;Property;_Scale;Scale;18;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;224;-1775.55,-957.694;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;205;-1213.06,-1275.172;Inherit;False;Property;_Bias;Bias;16;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;138;-1412.522,-1711.895;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;203;-1214.06,-1112.173;Inherit;False;Property;_Power;Power;17;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;217;-2099.191,-1175.896;Inherit;False;Property;_BlendColor1;Blend Color 1;0;1;[HDR];Create;True;0;0;False;0;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;144;-1248.908,-1773.317;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;145;-1304.794,-1707.536;Inherit;False;Property;_NoiseColor;Noise Color;13;1;[HDR];Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;117;-1093.408,-1275.583;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;2;False;3;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;219;-1724.808,-1117.777;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;-1234.088,-1541.136;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;151;-905.3376,-1086.258;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;146;-1057.848,-1689.192;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;89;-1816.347,-373.9068;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;147;-920.8204,-1692.047;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;152;-736.5457,-968.0691;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;216;-369.5718,-973.9202;Inherit;False;Property;_SummedWeight;Summed Weight;4;0;Create;True;0;0;False;0;1,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ColorNode;120;-387.6909,-1493.807;Inherit;False;Property;_FresnelColor;Fresnel Color;12;1;[HDR];Create;True;0;0;False;0;0.7028302,0.965398,1,0;0.7028302,0.965398,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;92;-1493.464,-286.7199;Inherit;False;Property;_DeformationScale;Deformation Scale;11;0;Create;True;0;0;False;0;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SummedBlendNode;215;-99.49337,-939.6997;Inherit;False;5;0;FLOAT2;0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;-1480.595,-445.1758;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;121;-168.4656,-1303.111;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;91;-1317.464,-360.7199;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;150;243.0051,-842.4005;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;545.6791,-672.0741;Float;False;True;-1;6;;0;0;Standard;Custom/Cloud;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;6;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;100;0;97;0
WireConnection;100;1;98;0
WireConnection;101;0;36;0
WireConnection;101;1;140;0
WireConnection;101;2;100;0
WireConnection;108;0;101;0
WireConnection;108;1;107;0
WireConnection;223;0;222;2
WireConnection;223;1;225;0
WireConnection;224;0;223;0
WireConnection;224;1;226;0
WireConnection;138;0;108;0
WireConnection;138;1;139;0
WireConnection;144;0;138;0
WireConnection;117;1;205;0
WireConnection;117;2;204;0
WireConnection;117;3;203;0
WireConnection;219;0;218;0
WireConnection;219;1;217;0
WireConnection;219;2;224;0
WireConnection;143;0;138;0
WireConnection;143;1;219;0
WireConnection;151;0;117;0
WireConnection;146;0;144;0
WireConnection;146;1;145;0
WireConnection;147;0;143;0
WireConnection;147;1;146;0
WireConnection;152;0;151;0
WireConnection;152;1;219;0
WireConnection;215;0;216;0
WireConnection;215;1;152;0
WireConnection;215;2;147;0
WireConnection;90;0;108;0
WireConnection;90;1;89;0
WireConnection;121;0;120;0
WireConnection;121;1;117;0
WireConnection;91;0;90;0
WireConnection;91;1;92;0
WireConnection;150;0;121;0
WireConnection;150;1;215;0
WireConnection;0;2;150;0
WireConnection;0;11;91;0
ASEEND*/
//CHKSM=D0A2A9448CDCB374DEFCA3007C6D90B54DA61582