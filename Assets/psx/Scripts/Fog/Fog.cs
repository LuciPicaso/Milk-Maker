using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("PSX/Fog")]
public class Fog : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enabled = new BoolParameter(false);

    public ClampedFloatParameter fogDensity   = new ClampedFloatParameter(0.2f, 0f, 2f);
    public ClampedFloatParameter fogDistance  = new ClampedFloatParameter(50f, 0.01f, 500f);
    public ColorParameter        fogColor     = new ColorParameter(Color.gray, true, true, true);

    public ClampedFloatParameter fogNear      = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedFloatParameter fogFar       = new ClampedFloatParameter(1f, 0f, 1f);

    public ClampedFloatParameter fogAltScale  = new ClampedFloatParameter(0f, 0f, 5f);
    public ClampedFloatParameter fogThinning  = new ClampedFloatParameter(1f, 0f, 5f);

    public ClampedFloatParameter noiseScale   = new ClampedFloatParameter(1f, 0f, 10f);
    public ClampedFloatParameter noiseStrength= new ClampedFloatParameter(0.2f, 0f, 2f);

    public bool IsActive() => enabled.value;
    public bool IsTileCompatible() => true;
}
