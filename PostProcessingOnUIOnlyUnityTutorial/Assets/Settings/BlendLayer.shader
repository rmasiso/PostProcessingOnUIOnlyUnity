// adapted by xrmasiso --> https://www.youtube.com/xrmasiso
// adapted from Aljosha --> https://discussions.unity.com/t/post-processing-with-multiple-cameras-is-currently-very-problematic/822011/268

// this is a little different from what was in the video tutorial
// this script exposes the blendmodes on the material inspector
// this is so that it is easier to experiment
// in other words, if you want to change the blend modes, just click on the material with this shader
// and you'll be able to change the different parameters that go into a blend from the inspector


Shader "BlendLayer"
{
   Properties
   {
        [Enum (UnityEngine.Rendering.BlendMode)] // expose enums in inspector
        _SrcFactor("Source Blend Factor", Float) = 5  // OneMinusSrcAlpha
        [Enum (UnityEngine.Rendering.BlendMode)]
        _DstFactor("Destination Blend Factor", Float) = 10 // SrcAlpha
        [Enum (UnityEngine.Rendering.BlendOp)]
        _Opp("Blend Operation",       Float) = 0      // Add

   }

   SubShader
   {
       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}

       //Blend SrcAlpha One // switch this to OneMinusSrcAlpha if you want original behavior
        
       Blend [_SrcFactor] [_DstFactor]
       BlendOp [_Opp]
        
       ZWrite Off Cull Off
       Pass
       {
           Name "BlendLayer"

           HLSLPROGRAM
           #pragma multi_compile_local_fragment _ _USE_FBF
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

           #pragma vertex Vert
           #pragma fragment Frag


#ifdef _USE_FBF
           // Declares the framebuffer input as a texture 2d containing half.
           FRAMEBUFFER_INPUT_HALF(0);
#endif
           // unity doesn't recommend cgprogram anymore, so while frag functions
           // were able to be of fixed4 type, now it's not recommended, so either float4 or half4 for this kind of stuff
           // Out frag function takes as input a struct that contains the screen space coordinate we are going to use to sample our texture. It also writes to SV_Target0, this has to match the index set in the UseTextureFragment(sourceTexture, 0, …) we defined in our render pass script.   
           float4 Frag(Varyings input) : SV_Target0
           {
               // this is needed so we account XR platform differences in how they handle texture arrays
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

               // read the current pixel from the framebuffer
               float2 uv = input.texcoord.xy;

#ifdef _USE_FBF
               // read previous subpasses directly from the framebuffer.
               half4 color = LOAD_FRAMEBUFFER_INPUT(0, input.positionCS.xy);
#else
               // the '_X' is usually a signature associated with making this work in single pass
               // i've had issues with VR unless things are in multi-pass; need to investigate more. 
               half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
#endif
               // Modify the sampled color
               return color;
           }

           ENDHLSL
       }
   }
}