Shader "Custom/mySolidColorShader_2"
{
    Properties
    {
        _Color("My Solid Color", Color) = (1,1,1,1)

        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcFactor("Src Factor", Float) = 5 
        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstFactor("Dst Factor", Float) = 1 
        [Enum(UnityEngine.Rendering.BlendOp)]
        _Opp("Operation", Float) = 0 
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Blend [_SrcFactor] [_DstFactor]
        BlendOp [_Opp]


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            fixed4 _Color;
            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
