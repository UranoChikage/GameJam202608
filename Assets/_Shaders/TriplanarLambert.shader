Shader "Custom/TriplanarLambert"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _TexScale("Tex Scale", Float) = 1
        _BlendSharpness("Blend Sharpness", Range(1, 32)) = 4
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP Lit/SimpleLitと同じライティング用バリアント
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float3 positionWS       : TEXCOORD0;
                float3 normalWS         : TEXCOORD1;
                float4 shadowCoord      : TEXCOORD2;
                half   fogFactor        : TEXCOORD3;
                half3  vertexLightColor : TEXCOORD4;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _TexScale;
                float _BlendSharpness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                OUT.shadowCoord = GetShadowCoord(positionInputs);
                OUT.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);

                OUT.vertexLightColor = half3(0, 0, 0);
                #if defined(_ADDITIONAL_LIGHTS_VERTEX)
                OUT.vertexLightColor = VertexLighting(positionInputs.positionWS, normalInputs.normalWS);
                #endif

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normalWS = normalize(IN.normalWS);

                // 各軸のブレンドウェイトを法線から算出
                float3 blendWeights = pow(abs(normalWS), _BlendSharpness);
                blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z);

                // ワールド座標を各平面に投影してサンプリング（tilingの伸びを回避）
                float2 uvX = IN.positionWS.zy * _TexScale;
                float2 uvY = IN.positionWS.xz * _TexScale;
                float2 uvZ = IN.positionWS.xy * _TexScale;

                half4 texX = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvX);
                half4 texY = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvY);
                half4 texZ = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvZ);

                half4 albedo =
                    texX * blendWeights.x +
                    texY * blendWeights.y +
                    texZ * blendWeights.z;

                albedo *= _Color;

                // URP Lit/SimpleLitと同じ経路でライティングを合成する
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = SafeNormalize(GetWorldSpaceViewDir(IN.positionWS));
                inputData.shadowCoord = IN.shadowCoord;
                inputData.fogCoord = IN.fogFactor;
                inputData.vertexLighting = IN.vertexLightColor;
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                // Lambert（拡散反射のみ）にするためspecular/smoothnessは0
                half4 color = UniversalFragmentBlinnPhong(
                    inputData,
                    albedo.rgb,
                    half4(0, 0, 0, 0),
                    0,
                    half3(0, 0, 0),
                    albedo.a,
                    half3(0, 0, 1)
                );

                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;
            }
            ENDHLSL
        }

        // ライトが正しく落影を評価するために必要（自身が影を落とす）
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // シャドウマップ/SSAOなどが深度を参照するために必要
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
}
