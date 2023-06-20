using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RetroGameShaderRenderFeature : ScriptableRendererFeature
{
    RetroGameShaderRenderPass renderPass;
    public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;


    public override void Create()
    {
        renderPass = new RetroGameShaderRenderPass(Event);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }
}
