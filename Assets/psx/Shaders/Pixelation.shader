Shader "PostEffect/Pixelation"
{
    Properties
    {
        _WidthPixelation("Width Pixelation", Float) = 320
        _HeightPixelation("Height Pixelation", Float) = 180
        _ColorPrecision("Color Precision", Float) = 32
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "PSX_PIXELATION"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _WidthPixelation;
            float _HeightPixelation;
            float _ColorPrecision;

            float3 Quantize(float3 c, float steps)
            {
                steps = max(steps, 2.0);
                return floor(c * steps + 0.5) / steps;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                // pixelate UV in screen space
                float2 grid = float2(max(_WidthPixelation, 1.0), max(_HeightPixelation, 1.0));
                uv = floor(uv * grid) / grid;

                // IMPORTANT: Blitter source comes from _BlitTexture
                float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;

                // optional color precision
                col = saturate(col);
                col = Quantize(col, _ColorPrecision);

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
