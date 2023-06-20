using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RetroGameShader : MonoBehaviour
{
    public int pixelSize = 3;
    public float gamma = 0.6f;
    public float gradation = 8.0f;
    public bool dither = true;
    public Texture2D crtTexture;
    public float crtColorGain = 1.0f;
    public float crtTextureScale = 1.0f;
    public float crtCurve = 0.0f;
    public Vector3 grayScaleFactor = new Vector3(0.2126f, 0.7152f, 0.0722f);
    public float grayScaleRatio = 0.0f;

    private Shader shader;
    private Material pixelizeMaterial;

    void CreateResource() {
        if (!shader) {
            shader = Shader.Find("RetroGameShader/RetroGameShader");
            if (!shader) {
                Debug.LogError("[RetroGameShader] Failed to find shader.");
            }
        }
        if (!pixelizeMaterial) {
            if (shader) {
                pixelizeMaterial = new Material(shader);
                if (!pixelizeMaterial) {
                    Debug.LogError("[RetroGameShader] Failed to create material.");
                }
            }
        }
    }

    void Start() {
        CreateResource();
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination) {
        CreateResource();
        if (pixelizeMaterial) {
            Vector4 pixelParam = new Vector4((float)pixelSize, gamma, gradation, dither ? 1.0f : 0.0f);
            pixelizeMaterial.SetVector("_PixelParam", pixelParam);
            var texture = crtTexture ? crtTexture : Texture2D.whiteTexture;
            pixelizeMaterial.SetTexture("_CRTTex", texture);
            Vector4 crtParam = new Vector4(texture.width * crtTextureScale, texture.height * crtTextureScale, crtColorGain, crtCurve + 1.0f);
            pixelizeMaterial.SetVector("_CRTParam", crtParam);
            Vector4 grayScaleParam = new Vector4(grayScaleFactor.x, grayScaleFactor.y, grayScaleFactor.z, grayScaleRatio);
            pixelizeMaterial.SetVector("_GrayScaleParam", grayScaleParam);
            Graphics.Blit(source, destination, pixelizeMaterial);
        } else {
            Graphics.Blit(source, destination);
        }
    }
}
