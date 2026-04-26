// =====================================================================
// GuJianFullscreen.shader
// 古风静谧 + 战斗紧凑 的摄像机全屏后处理。
//
// 效果分层:
//   1. 茶褐暖色调映射 (tea_tone)      —— 把场景整体压向古籍/宣纸的底色
//   2. 墨色暗角     (ink vignette)    —— 中心清亮、四角如墨晕开
//   3. 纸纹颗粒     (paper grain)     —— 微弱、动态、不可见但有呼吸感
//   4. 战斗紧绷     (_CombatTension)  —— 数值 0..1,高时整体对比度↑
//                                         + 暗角更紧 + 微红偏移
//   5. 击中红闪     (_HitFlash)       —— 外部短时拉高,给命中/受击一个冲击帧
//
// 使用方式:
//   - URP 下由 GuJianPostFXFeature (ScriptableRendererFeature) 在 AfterRenderingPostProcessing
//     阶段 Blit 到屏幕。在 Universal Renderer 里把 Feature 加到 Default Renderer 上。
//   - Built-in 下也可用脚本 Graphics.Blit 调用 (OnRenderImage)。
// =====================================================================
Shader "GuJian/Fullscreen"
{
    Properties
    {
        _MainTex        ("Source",            2D)     = "white" {}
        _TeaTint        ("Tea Tint",          Color)  = (0.86, 0.72, 0.49, 1)
        _InkColor       ("Ink Vignette Color",Color)  = (0.07, 0.05, 0.04, 1)
        _TeaAmount      ("Tea Amount",        Range(0,1)) = 0.35
        _VigInner       ("Vignette Inner",    Range(0,1)) = 0.45
        _VigOuter       ("Vignette Outer",    Range(0,1.5)) = 1.05
        _VigStrength    ("Vignette Strength", Range(0,1)) = 0.55
        _GrainAmount    ("Grain Amount",      Range(0,0.2)) = 0.035
        _Contrast       ("Contrast",          Range(0.5,1.8)) = 1.05
        _Saturation     ("Saturation",        Range(0,1.5)) = 0.88
        _CombatTension  ("Combat Tension",    Range(0,1)) = 0.0
        _HitFlash       ("Hit Flash",         Range(0,1)) = 0.0
        _TimeScale      ("Grain Time Scale",  Range(0,10)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZTest Always  Cull Off  ZWrite Off

        Pass
        {
            Name "GuJianPostFX"
            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
            // URP Blitter (Unity 2022.2+)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _TeaTint;
            float4 _InkColor;
            float  _TeaAmount;
            float  _VigInner;
            float  _VigOuter;
            float  _VigStrength;
            float  _GrainAmount;
            float  _Contrast;
            float  _Saturation;
            float  _CombatTension;
            float  _HitFlash;
            float  _TimeScale;

            // 伪随机 (IQ 的经典)
            float hash12(float2 p) {
                p = frac(p * float2(443.897, 441.423));
                p += dot(p, p.yx + 19.19);
                return frac((p.x + p.y) * p.x);
            }

            half3 Saturate3(half3 c, float s) {
                float l = dot(c, half3(0.2126, 0.7152, 0.0722));
                return lerp(half3(l,l,l), c, s);
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 uv = i.texcoord;
                half3  col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;

                // ---- 1. 茶褐暖色 ----
                // 基于亮度的 sepia 样式:保留细节但把全局色温推向茶色
                float luma = dot(col, half3(0.299, 0.587, 0.114));
                half3 tea  = luma * _TeaTint.rgb * 1.18;
                col = lerp(col, tea, _TeaAmount);

                // ---- 2. 饱和度 & 对比度 ----
                col = Saturate3(col, _Saturation);
                col = (col - 0.5) * _Contrast + 0.5;

                // ---- 3. 墨色暗角 ----
                float2 d = (uv - 0.5) * float2(1.0, 0.56); // 16:9 修正,让暗角椭圆化
                float r = length(d);
                float vig = smoothstep(_VigInner, _VigOuter, r);
                // 战斗紧绷 会让暗角更紧、更收束
                float combatVig = _CombatTension * 0.35;
                vig = saturate(vig + combatVig * smoothstep(_VigInner - 0.2, _VigOuter, r));
                col = lerp(col, _InkColor.rgb, vig * _VigStrength);

                // ---- 4. 纸纹颗粒 ----
                float t = _Time.y * _TimeScale;
                float g = hash12(uv * 1024.0 + t) - 0.5;
                col += g * _GrainAmount;

                // ---- 5. 战斗时轻微红色偏移 + 对比度加强 ----
                if (_CombatTension > 0.001) {
                    half3 tense = col;
                    tense.r += 0.04 * _CombatTension;
                    tense.gb *= (1.0 - 0.06 * _CombatTension);
                    col = lerp(col, tense, _CombatTension);
                }

                // ---- 6. 命中红闪 (瞬时) ----
                col = lerp(col, half3(1.0, 0.25, 0.18), _HitFlash * 0.55);

                return half4(saturate(col), 1.0);
            }
            ENDHLSL
        }
    }

    // 回落:Built-in / 老版 URP 没有 Blit.hlsl 的情况
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZTest Always  Cull Off  ZWrite Off

        Pass
        {
            Name "GuJianPostFX_Fallback"
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 pos : SV_POSITION;  float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TeaTint;
            float4 _InkColor;
            float  _TeaAmount;
            float  _VigInner;
            float  _VigOuter;
            float  _VigStrength;
            float  _GrainAmount;
            float  _Contrast;
            float  _Saturation;
            float  _CombatTension;
            float  _HitFlash;
            float  _TimeScale;

            v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            float hash12(float2 p) {
                p = frac(p * float2(443.897, 441.423));
                p += dot(p, p.yx + 19.19);
                return frac((p.x + p.y) * p.x);
            }

            half3 Saturate3(half3 c, float s) {
                float l = dot(c, half3(0.2126, 0.7152, 0.0722));
                return lerp(half3(l,l,l), c, s);
            }

            fixed4 frag(v2f i) : SV_Target {
                half3 col = tex2D(_MainTex, i.uv).rgb;
                float luma = dot(col, half3(0.299, 0.587, 0.114));
                half3 tea  = luma * _TeaTint.rgb * 1.18;
                col = lerp(col, tea, _TeaAmount);
                col = Saturate3(col, _Saturation);
                col = (col - 0.5) * _Contrast + 0.5;
                float2 d = (i.uv - 0.5) * float2(1.0, 0.56);
                float r = length(d);
                float vig = smoothstep(_VigInner, _VigOuter, r);
                vig = saturate(vig + _CombatTension * 0.35 * smoothstep(_VigInner - 0.2, _VigOuter, r));
                col = lerp(col, _InkColor.rgb, vig * _VigStrength);
                col += (hash12(i.uv * 1024.0 + _Time.y * _TimeScale) - 0.5) * _GrainAmount;
                if (_CombatTension > 0.001) {
                    half3 tense = col;
                    tense.r += 0.04 * _CombatTension;
                    tense.gb *= (1.0 - 0.06 * _CombatTension);
                    col = lerp(col, tense, _CombatTension);
                }
                col = lerp(col, half3(1.0, 0.25, 0.18), _HitFlash * 0.55);
                return half4(saturate(col), 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
