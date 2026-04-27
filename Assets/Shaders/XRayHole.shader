Shader "GuJian/XRayHole"
{
    Properties
    {
        _MainTex        ("Albedo (RGB)", 2D)       = "white" {}
        _Color          ("Color", Color)            = (1,1,1,1)
        _Glossiness     ("Smoothness", Range(0,1))  = 0.5
        _Metallic       ("Metallic",   Range(0,1))  = 0.0
        [Normal]_BumpMap("Normal Map", 2D)          = "bump" {}

        [HideInInspector] _PlayerPos  ("Player World Pos", Vector) = (0,0,0,0)
        [HideInInspector] _HoleRadius ("Hole Radius",  Float)      = 3.0
        [HideInInspector] _HoleSoft   ("Hole Softness",Float)      = 0.8
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        Cull Back
        ZWrite On

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4    _Color;
        half      _Glossiness;
        half      _Metallic;
        float3    _PlayerPos;
        float     _HoleRadius;
        float     _HoleSoft;

        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldPos;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float dist = length(IN.worldPos.xz - _PlayerPos.xz);
            float vis  = smoothstep(_HoleRadius - _HoleSoft, _HoleRadius, dist);
            clip(vis - 0.01);

            fixed4 c    = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo    = c.rgb;
            o.Alpha     = c.a;
            o.Metallic  = _Metallic;
            o.Smoothness= _Glossiness;
            o.Normal    = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
        }
        ENDCG
    }
    FallBack "Diffuse"
}