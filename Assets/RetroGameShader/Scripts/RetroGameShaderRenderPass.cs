using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RetroGameShaderRenderPass : ScriptableRenderPass
{
    private static readonly ProfilingSampler RetroGameShaderProfilingSampler = new ProfilingSampler("RetroGameShader");

    RetroGameShaderVolume volume;
    Material material;
    RenderTargetIdentifier renderTarget;
    RenderTargetHandle tempColorTarget;


    public RetroGameShaderRenderPass(RenderPassEvent renderPassEvent)
    {
        this.renderPassEvent = renderPassEvent;
        var shader = Shader.Find("RetroGameShader/RetroGameShader");
        if (!shader) {
            Debug.LogError("[RetroGameShader] Failed to find shader.");
            return;
        }
        material = CoreUtils.CreateEngineMaterial(shader);
        if (!material) {
            Debug.LogError("[RetroGameShader] Failed to create material.");
            return;
        }
        tempColorTarget.Init("_RetroGameShaderTexture");
    }

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		var renderer = renderingData.cameraData.renderer;
        renderTarget = renderer.cameraColorTarget;
	}

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!material) return;

        var stack = VolumeManager.instance.stack;
        volume = stack.GetComponent<RetroGameShaderVolume>();
        if (!volume || !volume.IsActive()) return;

        var cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, RetroGameShaderProfilingSampler)) {
            Blit(cmd, renderTarget, tempColorTarget.Identifier());
            ApplyMaterial(cmd, ref renderingData);
            Blit(cmd, tempColorTarget.Identifier(), renderTarget, material);
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        var tempColorTargetDescripter = cameraTextureDescriptor;
        tempColorTargetDescripter.useMipMap = false;
        tempColorTargetDescripter.autoGenerateMips = false;
        tempColorTargetDescripter.depthBufferBits = 0;
        cmd.GetTemporaryRT(tempColorTarget.id, tempColorTargetDescripter);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(tempColorTarget.id);
    }

    void ApplyMaterial(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;

        Vector4 pixelParam = new Vector4((float)volume.pixelSize.value, volume.gamma.value, volume.gradation.value, volume.dither.value ? 1.0f : 0.0f);
        material.SetVector("_PixelParam", pixelParam);
        var texture = volume.crtTexture.value ? volume.crtTexture.value : Texture2D.whiteTexture;
        material.SetTexture("_CRTTex", texture);
        Vector4 crtParam = new Vector4(texture.width * volume.crtTextureScale.value, texture.height * volume.crtTextureScale.value, volume.crtColorGain.value, volume.crtCurve.value + 1.0f);
        material.SetVector("_CRTParam", crtParam);
        Vector4 grayScaleParam = new Vector4(volume.grayScaleFactor.value.x, volume.grayScaleFactor.value.y, volume.grayScaleFactor.value.z, volume.grayScaleRatio.value);
        material.SetVector("_GrayScaleParam", grayScaleParam);
    }
}
