// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Texture Splatting" {

	Properties {
		_MainTex ("Splat Map", 2D) = "white" {}
		[NoScaleOffset] _Texture1 ("Texture 1", 2D) = "white" {}
	}

	SubShader {

		Pass {
			Tags { "Queue"="Transparent" "RenderType"="Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _Texture1, _Texture2, _Texture3, _Texture4;

			struct VertexData {
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Interpolators {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uvSplat : TEXCOORD1;
			};

			Interpolators MyVertexProgram (VertexData v) {
				Interpolators i;
				i.position = UnityObjectToClipPos(v.position);
				i.uv = TRANSFORM_TEX(v.uv, _MainTex);
				i.uvSplat = v.uv;
				return i;
			}


			//per pixel
			float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
				//get splat map
				float4 splat = tex2D(_MainTex, i.uv);
				//if some color value (red in this case) is less than some thresholed (0.5 of 1 in this case)
				if(splat.a <= 0.8)
                {
                	//set its alpha value to some value btw 0 and 1
                	splat.a = splat.r;
                	//set all other colors to 0
					splat.r = splat.g = splat.b = 0;
                }

                //apple to selected texture
				float4 f = tex2D(_Texture1, i.uvSplat) * splat.a;
				
				return f;
					

			}	

			ENDCG
		}
	}
}