Shader "PSX/FogCard"
{
    Properties
    {
        _MainTex("Noise/Smoke (A)", 2D) = "white" {}
        _Tint("Tint", Color) = (0.75, 0.75, 0.75, 1)
        _Intensity("Intensity", Range(0, 2)) = 0.6

        _PanSpeed("Pan Speed (XY)", Vector) = (0.02, 0.01, 0, 0)
        _Tiling("Tiling", Vector) = (1, 1, 0, 0)

        _Softness("Edge Softness", Range(0, 8)) = 2.0
        _DepthFade("Depth Fade", Range(0.001, 5)) = 0.7

        _HeightFade("Height Fade", Range(0, 4)) = 1.0
        _HeightBias("Height Bias", Range(-1, 1)) = 0.0

        _DitherStrength("Dither Strength", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "FOG_CARD"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_ST;

            float4 _Tint;
            float _Intensity;

            float4 _PanSpeed;
            float4 _Tiling;

            float _Softness;
            float _DepthFade;

            float _HeightFade;
            float _HeightBias;

            float _DitherStrength;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 ws = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(ws);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.positionHCS);
                return o;
            }

            // cheap hash for subtle dithering
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            half4 frag(Varyings i) : SV_Target
            {
                // UV panning (time-based)
                float2 uv = i.uv;
                uv *= _Tiling.xy;
                uv += _Time.y * _PanSpeed.xy;

                // texture sample (prefer alpha channel as density mask)
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                float mask = tex.a; // use alpha as fog density
                //float mask = dot(tex.rgb, float3(0.3333, 0.3333, 0.3333)); // яркость
                // soft edges (fade near quad borders)
                // uv 0..1 -> center fade
                float2 p = abs(i.uv * 2.0 - 1.0);              // 0 center -> 1 edges
                float edge = saturate(1.0 - max(p.x, p.y));    // 1 center -> 0 edges
                edge = pow(edge, max(_Softness, 0.001));

                // height fade (stronger at bottom of quad)
                // uv.y: 0 bottom -> 1 top
                float h = saturate(1.0 - (i.uv.y + _HeightBias));
                h = pow(h, max(_HeightFade, 0.001));

                // depth fade: reduces hard intersections with geometry
                float2 screenUV = i.screenPos.xy / max(i.screenPos.w, 1e-5);
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneEye = LinearEyeDepth(rawDepth, _ZBufferParams);

                float quadEye = LinearEyeDepth(i.positionHCS.z / i.positionHCS.w, _ZBufferParams);
                // when scene is closer than quad -> fade out
                float df = saturate((sceneEye - quadEye) / max(_DepthFade, 1e-5));

                float alpha = mask * edge * h * df * _Intensity;

                // optional tiny dithering in alpha (helps banding in transparent fog)
                if (_DitherStrength > 0.0001)
                {
                    float n = Hash21(screenUV * _ScreenParams.xy) - 0.5;
                    alpha = saturate(alpha + n * _DitherStrength * 0.05);
                }

                float3 rgb = _Tint.rgb;
                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }
}
