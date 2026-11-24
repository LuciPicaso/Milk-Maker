Shader "PostEffect/Fog"
{
    Properties
    {
        _FogDensity("Fog Density", Range(0,2)) = 0.2
        _FogDistance("Fog Distance", Range(0.01,500)) = 50
        _FogColor("Fog Color", Color) = (0.5,0.5,0.5,1)

        _FogNear("Fog Near", Range(0,1)) = 0.0
        _FogFar("Fog Far", Range(0,1)) = 1.0

        _FogAltScale("Fog Alt Scale", Range(0,5)) = 0.0
        _FogThinning("Fog Thinning", Range(0,5)) = 1.0

        _NoiseScale("Noise Scale", Range(0,10)) = 1.0
        _NoiseStrength("Noise Strength", Range(0,2)) = 0.2
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "PSX_FOG"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _FogDensity;
            float _FogDistance;
            float4 _FogColor;

            float _FogNear;
            float _FogFar;

            float _FogAltScale;
            float _FogThinning;

            float _NoiseScale;
            float _NoiseStrength;

            // Small hash noise (stable, cheap)
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                // Source from Blitter
                float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;

                // Depth (0..1)
                float rawDepth = SampleSceneDepth(uv);
                float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams); // in world units along view

                // Basic distance fog (0..1)
                float dist01 = saturate(eyeDepth / max(_FogDistance, 0.0001));
                float fog = 1.0 - exp(-dist01 * max(_FogDensity, 0.0));

                // Remap with near/far controls
                fog = saturate((fog - _FogNear) / max((_FogFar - _FogNear), 0.0001));

                // Height-ish variation (cheap approximation without worldpos reconstruction)
                // Uses screen Y as a stylized “altitude” proxy:
                if (_FogAltScale > 0.0001)
                {
                    float h = saturate(1.0 - uv.y); // bottom heavier by default
                    fog *= lerp(1.0, pow(h, max(_FogThinning, 0.0001)), saturate(_FogAltScale));
                }

                // Noise
                float n = Hash21(uv * (_ScreenParams.xy / max(_NoiseScale, 0.0001)));
                fog = saturate(fog + (n - 0.5) * _NoiseStrength);

                // Blend
                col = lerp(col, _FogColor.rgb, fog);

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
