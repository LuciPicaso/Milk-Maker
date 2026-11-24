using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace PSX
{
    public class FogRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public string shaderPath = "PostEffect/Fog";
        }

        public Settings settings = new Settings();
        FogPass _pass;

        public override void Create()
        {
            _pass = new FogPass(settings.shaderPath)
            {
                renderPassEvent = settings.passEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_pass == null)
                return;
            _pass.renderPassEvent = settings.passEvent;
            _pass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);

#if URP_COMPATIBILITY_MODE
            _pass.Setup(renderer.cameraColorTargetHandle);
#endif
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            _pass?.Dispose();
        }
    }

    internal sealed class FogPass : ScriptableRenderPass
    {
/// <summary>
///        static readonly string k_RenderTag = "Render Fog Effects";
/// </summary>

        static readonly int FogDensity = Shader.PropertyToID("_FogDensity");
        static readonly int FogDistance = Shader.PropertyToID("_FogDistance");
        static readonly int FogColor = Shader.PropertyToID("_FogColor");
        static readonly int FogNear = Shader.PropertyToID("_FogNear");
        static readonly int FogFar = Shader.PropertyToID("_FogFar");
        static readonly int FogAltScale = Shader.PropertyToID("_FogAltScale");
        static readonly int FogThinning = Shader.PropertyToID("_FogThinning");
        static readonly int NoiseScale = Shader.PropertyToID("_NoiseScale");
        static readonly int NoiseStrength = Shader.PropertyToID("_NoiseStrength");

        readonly string _shaderPath;
        Material _material;

#if URP_COMPATIBILITY_MODE
        RTHandle _source;
        RTHandle _temp;
#endif

        public FogPass(string shaderPath) => _shaderPath = shaderPath;

#if URP_COMPATIBILITY_MODE
        public void Setup(RTHandle source) => _source = source;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateHandleIfNeeded(
                ref _temp,
                desc,
                FilterMode.Point,
                TextureWrapMode.Clamp,
                name: "_TempTargetFog"
            );
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            EnsureMaterial();
            if (_material == null)
                return;

            if (!renderingData.cameraData.postProcessEnabled)
                return;

            var stack = VolumeManager.instance.stack;
            var volume = stack.GetComponent<Fog>();
            if (volume == null || !volume.IsActive())
                return;

            ApplyParams(volume);

            var cmd = CommandBufferPool.Get(k_RenderTag);
            using (new ProfilingScope(cmd, new ProfilingSampler(k_RenderTag)))
            {
                Blitter.BlitCameraTexture(cmd, _source, _temp);
                Blitter.BlitCameraTexture(cmd, _temp, _source, _material, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
#endif

        class PassData
        {
            public TextureHandle source;
            public TextureHandle depth;
            public TextureHandle temp;
            public Material material;

            public float density, distance, near, far, altScale, thinning, noiseScale, noiseStrength;
            public Color color;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            EnsureMaterial();
            if (_material == null)
                return;

            var cameraData = frameData.Get<UniversalCameraData>();
            if (!cameraData.postProcessEnabled)
                return;

            var stack = VolumeManager.instance.stack;
            var volume = stack.GetComponent<Fog>();
            if (volume == null || !volume.IsActive())
                return;

            var resources = frameData.Get<UniversalResourceData>();
            var scaleBias = new Vector4(1, 1, 0, 0);
            

            TextureDesc desc = resources.activeColorTexture.GetDescriptor(renderGraph);
            desc.depthBufferBits = 0;
            desc.name = "_TempTargetFog_RG";
            var temp = renderGraph.CreateTexture(desc);

            // copy pass
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Fog Copy", out var pass))
            {
                pass.source = resources.activeColorTexture;
                pass.temp = temp;

                builder.UseTexture(pass.source);
                builder.SetRenderAttachment(pass.temp, 0);

                builder.SetRenderFunc(static (PassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }

            // apply pass (needs depth as sampled texture)
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Fog Apply", out var pass))
            {
                pass.temp = temp;
                pass.depth = resources.cameraDepthTexture; // ensures depth exists for _CameraDepthTexture sampling
                pass.material = _material;

                pass.density = volume.fogDensity.value;
                pass.distance = volume.fogDistance.value;
                pass.color = volume.fogColor.value;
                pass.near = volume.fogNear.value;
                pass.far = volume.fogFar.value;
                pass.altScale = volume.fogAltScale.value;
                pass.thinning = volume.fogThinning.value;
                pass.noiseScale = volume.noiseScale.value;
                pass.noiseStrength = volume.noiseStrength.value;

                builder.UseTexture(pass.temp);
                builder.UseTexture(pass.depth);
                builder.SetRenderAttachment(resources.activeColorTexture, 0);

                builder.SetRenderFunc(static (PassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetFloat(FogDensity, data.density);
                    data.material.SetFloat(FogDistance, data.distance);
                    data.material.SetColor(FogColor, data.color);
                    data.material.SetFloat(FogNear, data.near);
                    data.material.SetFloat(FogFar, data.far);
                    data.material.SetFloat(FogAltScale, data.altScale);
                    data.material.SetFloat(FogThinning, data.thinning);
                    data.material.SetFloat(NoiseScale, data.noiseScale);
                    data.material.SetFloat(NoiseStrength, data.noiseStrength);

                    Blitter.BlitTexture(ctx.cmd, data.temp, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }
        }

        void ApplyParams(Fog volume)
        {
            _material.SetFloat(FogDensity, volume.fogDensity.value);
            _material.SetFloat(FogDistance, volume.fogDistance.value);
            _material.SetColor(FogColor, volume.fogColor.value);
            _material.SetFloat(FogNear, volume.fogNear.value);
            _material.SetFloat(FogFar, volume.fogFar.value);
            _material.SetFloat(FogAltScale, volume.fogAltScale.value);
            _material.SetFloat(FogThinning, volume.fogThinning.value);
            _material.SetFloat(NoiseScale, volume.noiseScale.value);
            _material.SetFloat(NoiseStrength, volume.noiseStrength.value);
        }

        void EnsureMaterial()
        {
            if (_material != null)
                return;

            var shader = Shader.Find(_shaderPath);
            if (shader == null)
            {
                Debug.LogError($"[PSX] Shader not found: {_shaderPath}");
                return;
            }

            _material = CoreUtils.CreateEngineMaterial(shader);
        }

        public void Dispose()
        {
            CoreUtils.Destroy(_material);
            _material = null;

#if URP_COMPATIBILITY_MODE
            _temp?.Release();
            _temp = null;
#endif
        }
    }
}
