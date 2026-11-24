using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("PSX/Dithering")]
public class Dithering : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enabled = new BoolParameter(false);

    public IntParameter patternIndex = new IntParameter(0);

    // под твой новый шейдер нормальные диапазоны:
    public ClampedFloatParameter ditherThreshold = new ClampedFloatParameter(0.5f, 0f, 1f);
    public ClampedFloatParameter ditherStrength  = new ClampedFloatParameter(0.35f, 0f, 2f);
    public ClampedFloatParameter ditherScale     = new ClampedFloatParameter(1f, 0.25f, 8f);

    public bool IsActive() => enabled.value;
    public bool IsTileCompatible() => true;
}
