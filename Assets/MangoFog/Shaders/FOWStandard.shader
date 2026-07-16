Shader "MangoFog/FOWStandard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Emission ("Emission", Color) = (0,0,0,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        CGPROGRAM
        #include "UnityPBSLighting.cginc"
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Custom fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float3 _Emission;
        
        // global
        // set once
        uniform float _ChunkSize;
        uniform float4 _GlobalPos;
        uniform half4 _GlobalUnexplored;
		uniform half4 _GlobalExplored;

        // set per tick
        uniform sampler2D _GlobalFOWData;
		uniform half _GlobalBlendFactor;

        // varying?
        float _VaryingAlpha;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        inline half4 LightingCustom (SurfaceOutputStandard s, half3 lightDir, UnityGI gi) {
            half4 standard = LightingStandard(s, lightDir, gi);
            half4 unlit = 0;
            return lerp(standard, unlit, _VaryingAlpha);
        }

        inline void LightingCustom_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi) 
        {
            LightingStandard_GI(s, data, gi);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 globalPos = _GlobalPos - (_ChunkSize * 0.5);
            float3 pos = IN.worldPos - globalPos;
            pos *= 1.0 / _ChunkSize;
            half4 data = tex2D(_GlobalFOWData, float2(pos.x, pos.z));
            half2 fog = lerp(data.rg, data.ba, _GlobalBlendFactor);
			half4 fowColor = lerp(_GlobalUnexplored, _GlobalExplored, fog.g);
            _VaryingAlpha = saturate((1 - fog.r) * fowColor.a);

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}