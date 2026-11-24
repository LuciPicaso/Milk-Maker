using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace PSX
{
    public class PixelationRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public string shaderPath = "PostEffect/Pixelation";
        }

        public Settings settings = new Settings();
        PixelationPass _pass;

        public override void Create()
        {
            _pass = new PixelationPass(settings.shaderPath)
            {
                renderPassEvent = settings.passEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_pass == null)
                return;
            _pass.renderPassEvent = settings.passEvent;
            _pass.ConfigureInput(ScriptableRenderPassInput.Color);

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

    internal sealed class PixelationPass : ScriptableRenderPass
    {
/// <summary>
///        static readonly string k_RenderTag = "Render Pixelation Effects";
/// </summary>

        static readonly int WidthPixelation = Shader.PropertyToID("_WidthPixelation");
        static readonly int HeightPixelation = Shader.PropertyToID("_HeightPixelation");
        static readonly int ColorPrecision = Shader.PropertyToID("_ColorPrecision");

        readonly string _shaderPath;
        Material _material;

#if URP_COMPATIBILITY_MODE
        RTHandle _source;
        RTHandle _temp;
#endif

        public PixelationPass(string shaderPath) => _shaderPath = shaderPath;

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
                name: "_TempTargetPixelation"
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
            var volume = stack.GetComponent<Pixelation>();
            if (volume == null || !volume.IsActive())
                return;

            _material.SetFloat(WidthPixelation, volume.widthPixelation.value);
            _material.SetFloat(HeightPixelation, volume.heightPixelation.value);
            _material.SetFloat(ColorPrecision, volume.colorPrecision.value);

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
            public TextureHandle temp;
            public Material material;
            public float w;
            public float h;
            public float colorPrecision;
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
            var volume = stack.GetComponent<Pixelation>();
            if (volume == null || !volume.IsActive())
                return;

            var resources = frameData.Get<UniversalResourceData>();
            var scaleBias = new Vector4(1, 1, 0, 0);

            TextureDesc desc = resources.activeColorTexture.GetDescriptor(renderGraph);
            desc.depthBufferBits = 0;
            desc.name = "_TempTargetPixelation_RG";
            var temp = renderGraph.CreateTexture(desc);

            // copy pass
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelation Copy", out var pass))
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

            // apply pass
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelation Apply", out var pass))
            {
                pass.temp = temp;
                pass.material = _material;

                pass.w = volume.widthPixelation.value;
                pass.h = volume.heightPixelation.value;
                pass.colorPrecision = volume.colorPrecision.value;

                builder.UseTexture(pass.temp);
                builder.SetRenderAttachment(resources.activeColorTexture, 0);

                builder.SetRenderFunc(static (PassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetFloat(WidthPixelation, data.w);
                    data.material.SetFloat(HeightPixelation, data.h);
                    data.material.SetFloat(ColorPrecision, data.colorPrecision);

                    Blitter.BlitTexture(ctx.cmd, data.temp, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }
            
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
