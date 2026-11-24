Shader "PostEffect/Dithering"
{
    Properties
    {
        _PatternIndex("Pattern Index", Int) = 0
        _DitherThreshold("Dither Threshold", Range(0,1)) = 0.5
        _DitherStrength("Dither Strength", Range(0,2)) = 0.5
        _DitherScale("Dither Scale", Range(0.25,8)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "PSX_DITHERING"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            int   _PatternIndex;
            float _DitherThreshold;
            float _DitherStrength;
            float _DitherScale;

            // 4x4 Bayer (0..15) normalized 0..1
            float Bayer4x4(int2 p)
            {
                int x = p.x & 3;
                int y = p.y & 3;

                int m[16] =
                {
                    0,  8,  2, 10,
                    12, 4, 14, 6,
                    3, 11, 1,  9,
                    15, 7, 13, 5
                };

                return (m[y * 4 + x] / 15.0);
            }

            // 8x8 Bayer (0..63) normalized 0..1
            float Bayer8x8(int2 p)
            {
                int x = p.x & 7;
                int y = p.y & 7;

                int m[64] =
                {
                    0, 32,  8, 40,  2, 34, 10, 42,
                    48,16, 56, 24, 50, 18, 58, 26,
                    12,44,  4, 36, 14, 46,  6, 38,
                    60,28, 52, 20, 62, 30, 54, 22,
                    3, 35, 11, 43,  1, 33,  9, 41,
                    51,19, 59, 27, 49, 17, 57, 25,
                    15,47,  7, 39, 13, 45,  5, 37,
                    63,31, 55, 23, 61, 29, 53, 21
                };

                return (m[y * 8 + x] / 63.0);
            }

            float GetPatternValue(int2 p, int patternIndex)
            {
                // 0 = Bayer4x4, 1 = Bayer8x8, остальное — тоже 4x4 (safe fallback)
                return (patternIndex == 1) ? Bayer8x8(p) : Bayer4x4(p);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                // Source from Blitter
                float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;

                // Screen-space position for patterns
                float2 pixelPos = uv * _ScreenParams.xy;
                pixelPos *= max(_DitherScale, 0.0001);

                int patternIndex = (int)_PatternIndex;
                float d = 0.0;
                d = GetPatternValue((int2)pixelPos, patternIndex); // 0..1

                // Apply as subtle noise around threshold
                // d centered -> [-0.5..+0.5]
                float centered = (d - _DitherThreshold);

                col = saturate(col + centered * _DitherStrength);

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
