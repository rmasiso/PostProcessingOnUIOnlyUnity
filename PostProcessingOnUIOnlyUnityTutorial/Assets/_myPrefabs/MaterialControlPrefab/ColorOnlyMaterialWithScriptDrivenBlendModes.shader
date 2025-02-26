Shader "Custom/ColorOnlyShaderWithBlendModes"
{
    Properties
    {
        _Color("My Solid Color", color) = (1, 1, 1, 1) // _Color is the property, 'color' is the type.

        _SrcFactor("Src Factor", Float) = 5 // OneMinusSrcAlpha
        _DstFactor("Dst Factor", Float) = 10 // SrcAlpha
        _Opp("Operation", Float) = 0 // Add

    }
    SubShader // there can be many of these.
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline" = "UniversalPipeline" // could comment this out, but it's better to be safe. 
            // the Unity unlitshader template doesnt include this, but I will just in case.
        }
        LOD 100
        Blend [_SrcFactor] [_DstFactor]
        BlendOp [_Opp]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc" // like 'import' in python or 'using' in Unity; allows us to use UnityObjectToClipPos
            // i can comment it out and the shader will still work, but it's good practice to keep it for it to work across 
            // other platforms.
            // apparently though, unity recommends moving away from CGPROGRAM --- and sticking to HLSL only

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 _Color; // define color as fixed4
            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
