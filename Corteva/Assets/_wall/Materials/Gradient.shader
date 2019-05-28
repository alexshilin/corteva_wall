// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Unlit/Color Gradient" {
     Properties {
         [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
         _ColorStart ("Start Color", Color) = (1,1,1,1)
         _ColorEnd ("End Color", Color) = (1,1,1,1)
         _Range ("Range", Range(0.00000, 1.0000)) = 1.00000
     }
 
     SubShader {
         Tags { "RenderType"="Opaque" }
         LOD 100
 
         ZWrite On
 
         Pass {
         Lighting Off
         CGPROGRAM
         #pragma vertex vert  
         #pragma fragment frag
         #include "UnityCG.cginc"
 
         fixed4 _ColorStart;
         fixed4 _ColorEnd;
         float  _Range;
 
         struct v2f {
             float4 pos : SV_POSITION;
             float4 texcoord : TEXCOORD0;
         };
 
         v2f vert (appdata_full v) {
             v2f o;
             o.pos = UnityObjectToClipPos (v.vertex);
             o.texcoord = v.texcoord;
             return o;
         }
 
         fixed4 frag (v2f i) : COLOR {
         	fixed4 c = lerp(_ColorEnd, _ColorStart, _Range / i.texcoord.y);
             c.a = 1;
             return c;
         }
         ENDCG
         }
     }
 }