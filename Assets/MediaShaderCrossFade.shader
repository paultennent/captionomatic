﻿// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Unlit/MediaShaderCrossFade" {
Properties {
	_SecondaryTex ("Secondary (RGB) Trans (A)", 2D) = "white" {}
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _MaskTex ("Mask alpha texture", 2D) = "white" {}

	_Blend("Blend", Range(0, 1)) = 0.5
}

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 100

    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha


    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2 : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MaskTex;
            sampler2D _MainTex;
			sampler2D _SecondaryTex;
            float4 _MainTex_ST;
			float _Blend;

            v2f vert (appdata_t v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord* _MainTex_ST.xy+ _MainTex_ST.zw;
                o.texcoord2 = v.texcoord;

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
				fixed4 col2 = tex2D(_SecondaryTex, i.texcoord);

                fixed4 mask= tex2D(_MaskTex,i.texcoord2);
                if (i.texcoord.x <= 0.0f || i.texcoord.y <= 0.0f || i.texcoord.x >= 1.0f || i.texcoord.y >= 1.0f)
                {
                    col = fixed4 (0,0,0,0);
                }else
                {
					col = fixed4(lerp(col.r, col2.r, _Blend), lerp(col.g, col2.g, _Blend), lerp(col.b, col2.b, _Blend), 1);
                    col.a*=mask.a;
                }

                return col;
            }
        ENDCG
    }
}

}