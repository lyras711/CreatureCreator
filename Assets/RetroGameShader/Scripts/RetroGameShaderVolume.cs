using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[VolumeComponentMenu("RetroGameShaderVolume")]
public class RetroGameShaderVolume : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
    public IntParameter pixelSize = new IntParameter(3);
    public FloatParameter gamma = new FloatParameter(0.6f);
    public FloatParameter gradation = new FloatParameter(8.0f);
    public BoolParameter dither = new BoolParameter(true);
    public TextureParameter crtTexture = new TextureParameter(null);
    public FloatParameter crtColorGain = new FloatParameter(1.0f);
    public FloatParameter crtTextureScale = new FloatParameter(1.0f);
    public FloatParameter crtCurve = new FloatParameter(0.0f);
    public Vector3Parameter grayScaleFactor = new Vector3Parameter(new Vector3(0.2126f, 0.7152f, 0.0722f));
    public FloatParameter grayScaleRatio = new FloatParameter(0.0f);
    

    public bool IsActive() => enable.value;
    public bool IsTileCompatible() => false;
}
