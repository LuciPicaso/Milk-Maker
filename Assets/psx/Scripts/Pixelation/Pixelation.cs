using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("PSX/Pixelation")]
public class Pixelation : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enabled = new BoolParameter(false);

    public ClampedFloatParameter widthPixelation  = new ClampedFloatParameter(320, 1, 4096);
    public ClampedFloatParameter heightPixelation = new ClampedFloatParameter(180, 1, 4096);
    public ClampedFloatParameter colorPrecision   = new ClampedFloatParameter(32, 2, 64);

    // вместо override IsActive()
    public bool IsActive() => enabled.value;

    // обычно можно так
    public bool IsTileCompatible() => true;
}
