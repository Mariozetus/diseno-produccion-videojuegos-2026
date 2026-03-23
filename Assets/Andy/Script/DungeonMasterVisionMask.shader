Shader "UI/DungeonMasterVisionMask"
{
    Properties
    {
        _Color ("Tint", Color) = (0,0,0,0.85)
        _HoleCenter ("Hole Center", Vector) = (0.5,0.5,0,0)
        _HoleRadius ("Hole Radius", Float) = 0.15
        _EdgeSoftness ("Edge Softness", Float) = 0.02
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float4 _HoleCenter;
            float _HoleRadius;
            float _EdgeSoftness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float dist = distance(uv, _HoleCenter.xy);

                float alphaMask = smoothstep(_HoleRadius, _HoleRadius + _EdgeSoftness, dist);

                fixed4 col = _Color;
                col.a *= alphaMask;
                return col;
            }
            ENDCG
        }
    }
}