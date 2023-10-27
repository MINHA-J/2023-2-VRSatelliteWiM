Shader "Unlit/CubemapTestFrag"
{
	Properties
	{
		_MainTex ("Texture", 2DArray) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Cull Off
		LOD 100

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			// Includes xyz_to_uvw, uvw_to_xyz, xyz_to_side, xyz_to_uvw_force_side based on a macro for shaders
			#include "CubemapTransform.cs"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;

				// Custom interpolators
				float4 raw_position : TEXCOORD0;
			};

			UNITY_DECLARE_TEX2DARRAY(_MainTex);
			
			v2f vert (appdata v)
			{
				v2f o;
				o.raw_position = v.vertex;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// You can verify that both functions correspond with each other by uncommenting the following line (it should be black = no difference)
				// return abs(UNITY_SAMPLE_TEX2DARRAY(_MainTex, xyz_to_uvw(i.raw_position.xyz)) - UNITY_SAMPLE_TEX2DARRAY(_MainTex, xyz_to_uvw(uvw_to_xyz(xyz_to_uvw(i.raw_position.xyz)))));

				return UNITY_SAMPLE_TEX2DARRAY(_MainTex, xyz_to_uvw(i.raw_position.xyz));
			}

			ENDCG
		}
	}
}
