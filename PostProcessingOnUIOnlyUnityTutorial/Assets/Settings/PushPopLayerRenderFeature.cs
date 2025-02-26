// adapted by xrmasiso --> https://www.youtube.com/xrmasiso
// adapted from Aljosha --> https://discussions.unity.com/t/post-processing-with-multiple-cameras-is-currently-very-problematic/822011/268
// recommended to use original script which allows for framebufferfetch
// this would allow you to combine this pass with a previous one in a single blit operation
// this was excluded from this script to simplify explanation of the render feature
// in general, it seems most examples Unity has put out do include this operation and it is also
// brought up in the official URP handbook: https://unity.com/resources/introduction-to-urp-advanced-creators-unity-6
// so, it's good practice.

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Experimental.Rendering;

public class PushPopLayerRenderFeature : ScriptableRendererFeature
{
    // a RenderPassEvent is needed for things to be added into the pipeline
    // here is where we can select via dropdown (or hardcode) the 'injection point'
    // there are two render passes defined in this script, so there are two injection points that need to be chosen
    public RenderPassEvent push = RenderPassEvent.AfterRenderingPostProcessing;
    public RenderPassEvent pop = RenderPassEvent.AfterRenderingPostProcessing+49;

    public Material blendMaterial;

    // super useful! allows us to 'store' references
    public class StackLayers : ContextItem
    {
        // the texture reference variable.
        public Stack<TextureHandle> layers = new();

        // Reset function required by ContextItem. It should reset all variables not carried
        // over to next frame.
        public override void Reset()
        {
            // We should always reset texture handles since they are only vaild for the current frame.
            layers.Clear();
        }
    }

    // ### THE SNAPSHOT TAKER ###
    // ##########################
    // --- Here we take a snapshot of what has been rendered UP to the moment we defined as our 'push' injection point
    // --- We also create a new 'render target' (a layer unto which Unity continues rendering on top of)
    class PushLayerRenderPass : ScriptableRenderPass
    {
        // allows this class to have material retrieved or set
        public Material blendMaterial { get; set; }

        // recommended to include profiling sampler in constructor
        // it's commented out here on purpose to showcase how in a blitpass you can just use a string
        // the unity official render feature samples sometimes just use a string
        // so if they do it too.. it's probably ok? xD
        //public PushLayerRenderPass()
        //{
        //    profilingSampler = new ProfilingSampler("Push: Take Snapshot!");
        //}


        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // resourceData has tons of useful stuff in it
            UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();

            // if statement checks that we only run if we have the blendMaterial with something
            // this blitpass is only here to demonstrate how to add things to the render graph viewer to illustrate the pipeline
            // this is unecessary and adds extra computational resources to the pipeline
            // i'm not sure what other ways one can add things into the render graph viewer for debugging
            // but, this seemed harmless with the simple scene in this example
            if (blendMaterial != null)
            {
                RenderGraphUtils.BlitMaterialParameters blitMaterialPArameters = new(resourcesData.cameraColor, resourcesData.cameraColor, blendMaterial, 0);
                renderGraph.AddBlitPass(blitMaterialPArameters, "Push: Take Snapshot!");
            }

            var layers = frameData.GetOrCreate<StackLayers>(); 

            layers.layers.Push(resourcesData.cameraColor); // and this pushes it into the storage container

            TextureDesc desc; 
            desc = resourcesData.cameraColor.GetDescriptor(renderGraph); // gets us the 'meta-data' 
            desc.clearBuffer = true; // recommended 
            desc.clearColor = new Color(0.0f, 0.0f, 0.0f, 1.0f); // set the color of new render target -- play around with this like = (1f, 0.0f, 0.0f, 1.0f)
            desc.name = "_NewRenderTarget_" + layers.layers.Count; // this texture shows up in render graph viewer and you can see when it's read and written on
            
            var layerColor = renderGraph.CreateTexture(desc);

            resourcesData.cameraColor = layerColor; // here we set the new render target to the texutre we created

            // checks whether you checkboxed 'alpha processing' in the pipeline asset
            if (!GraphicsFormatUtility.HasAlphaChannel(desc.format))
            {
                Debug.LogWarning("Layer does not have an alpha channel. Blending will overwrite the entire screen.");
            }
        }
    }

    // ### THE BLENDER ###
    // #############################
    // --- Here we blend the saved snapshot with everything else that rendered afterwards (i.e., the overlay elements).
    class PopLayerRenderPass : ScriptableRenderPass
    {
        // allows this class to have material retrieved or set
        public Material blendMaterial { get; set; }

        protected string m_FBFKeyword = "_USE_FBF";

        // recommended to include profiling sampler in constructor
        public PopLayerRenderPass()
        {
            profilingSampler = new ProfilingSampler("Pop: Blend Snapshot!");
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (blendMaterial != null)
            {
                UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();

                var layers = frameData.GetOrCreate<StackLayers>();

                if (layers.layers.Count > 0)
                {
                    var previousLayer = layers.layers.Pop(); // retrieve the texture reference

                    blendMaterial.DisableKeyword(m_FBFKeyword); // disable fbf, otherwise this breaks (since it has been removed from this script)

                    // setup blending
                    // https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@17.0/api/UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils.BlitMaterialParameters.html
                    RenderGraphUtils.BlitMaterialParameters blitMaterialParameters = new(resourcesData.cameraColor, previousLayer, blendMaterial, 0);

                    // execute blending as blitpass
                    renderGraph.AddBlitPass(blitMaterialParameters, passName);
                    resourcesData.cameraColor = previousLayer;
                    
                }
            }
        }
    }

    // declaration
    PushLayerRenderPass m_pushPass;
    PopLayerRenderPass m_popPass;

    // creation
    public override void Create()
    {
        m_pushPass = new PushLayerRenderPass(); // Create a new instance of the push render pass.
        m_popPass = new PopLayerRenderPass();
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_pushPass.renderPassEvent = push; // Set the timing for the push pass to the public variable.
        m_pushPass.blendMaterial = blendMaterial; // sets the blend material!

        m_popPass.renderPassEvent = pop;
        m_popPass.blendMaterial = blendMaterial;
       
        renderer.EnqueuePass(m_pushPass);
        renderer.EnqueuePass(m_popPass);
    }
}
