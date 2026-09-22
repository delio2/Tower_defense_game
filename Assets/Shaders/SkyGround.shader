// The arena floor (docs/03 A2, A6): a dusk gradient from the far edge (top of the screen, lighter haze) to the
// near edge (darker), a soft glow under the Core, and the soft shadows of the Core, modules and enemies.
// Colours per act come from the act themes (tokens sky-far / sky-mid / sky-near).
Shader "TowerDefense/SkyGround"
{
    Properties
    {
        _FarColor ("Sky far", Color) = (0.23, 0.25, 0.48, 1)
        _MidColor ("Sky mid", Color) = (0.11, 0.13, 0.31, 1)
        _NearColor ("Sky near", Color) = (0.05, 0.06, 0.19, 1)
        _GlowColor ("Glow under the Core", Color) = (0.18, 0.2, 0.47, 1)
        _FarZ ("Far edge z", Float) = 14
        _NearZ ("Near edge z", Float) = -14
        _GlowRadius ("Glow radius", Float) = 6
        _ShadowStrength ("Shadow strength", Range(0, 1)) = 0.45
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry-10" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _FarColor;
                half4 _MidColor;
                half4 _NearColor;
                half4 _GlowColor;
                float _FarZ;
                float _NearZ;
                float _GlowRadius;
                half _ShadowStrength;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float t = saturate((input.positionWS.z - _NearZ) / (_FarZ - _NearZ));
                half3 colour = t < 0.5 ? lerp(_NearColor.rgb, _MidColor.rgb, t * 2.0) : lerp(_MidColor.rgb, _FarColor.rgb, (t - 0.5) * 2.0);
                float glow = 1.0 - saturate(length(input.positionWS.xz) / _GlowRadius);
                colour = 1.0 - (1.0 - colour) * (1.0 - _GlowColor.rgb * glow * glow); // screen blend
                half shadow = GetMainLight(TransformWorldToShadowCoord(input.positionWS)).shadowAttenuation;
                colour *= lerp(1.0h - _ShadowStrength, 1.0h, shadow);
                return half4(colour, 1.0h);
            }
            ENDHLSL
        }
    }
}
