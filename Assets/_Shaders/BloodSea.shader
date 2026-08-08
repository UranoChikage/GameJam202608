Shader "Custom/BloodSea"
{
    Properties
    {
        _DeepColor("Deep Color", Color) = (0.12, 0.0, 0.0, 1)
        _ShallowColor("Shallow/Edge Color", Color) = (0.55, 0.02, 0.02, 1)
        _RimColor("Rim Color", Color) = (0.9, 0.1, 0.05, 1)
        _RimPower("Rim Power", Range(0.5, 8)) = 3
        _DepthFadeDistance("Depth Fade Distance", Range(0.01, 5)) = 1.2
        _WaveAmplitude("Wave Amplitude", Range(0, 0.5)) = 0.05
        _WaveFrequency("Wave Frequency", Range(0, 5)) = 0.6
        _WaveSpeed("Wave Speed", Range(0, 5)) = 0.8
        _SparkleScale("Sparkle Scale", Range(1, 50)) = 12
        _SparkleSpeed("Sparkle Speed", Range(0, 5)) = 0.5
        _SparkleStrength("Sparkle Strength", Range(0, 1)) = 0.3
        _Smoothness("Smoothness", Range(0, 1)) = 0.7
        _Alpha("Base Alpha", Range(0, 1)) = 0.9
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _DeepColor;
                float4 _ShallowColor;
                float4 _RimColor;
                float _RimPower;
                float _DepthFadeDistance;
                float _WaveAmplitude;
                float _WaveFrequency;
                float _WaveSpeed;
                float _SparkleScale;
                float _SparkleSpeed;
                float _SparkleStrength;
                float _Smoothness;
                float _Alpha;
            CBUFFER_END

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS);

                // Boxの頂点数のままでも波打って見えるよう、2方向のsin波を重ねて面を歪ませる
                float wave1 = sin((positionWS.x + positionWS.z) * _WaveFrequency + _Time.y * _WaveSpeed);
                float wave2 = sin((positionWS.x - positionWS.z) * _WaveFrequency * 1.7 + _Time.y * _WaveSpeed * 1.3);
                positionWS.y += (wave1 + wave2) * 0.5 * _WaveAmplitude;

                OUT.positionWS = positionWS;
                OUT.positionCS = TransformWorldToHClip(positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 頂点で歪ませた面からスクリーン空間微分で法線を復元する(頂点数が少なくても破綻しない)
                float3 dx = ddx(IN.positionWS);
                float3 dy = ddy(IN.positionWS);
                float3 geoNormal = normalize(cross(dy, dx));

                // 外部テクスチャなしのプロシージャルノイズでキラつきを追加
                float2 sparkleUV = IN.positionWS.xz * _SparkleScale + _Time.y * _SparkleSpeed;
                float n0 = Hash21(floor(sparkleUV));
                float n1 = Hash21(floor(sparkleUV) + float2(1, 0));
                float n2 = Hash21(floor(sparkleUV) + float2(0, 1));
                float2 bump = float2(n1 - n0, n2 - n0) * _SparkleStrength;

                float3 normalWS = normalize(geoNormal + float3(bump.x, 0, bump.y));

                float3 viewDirWS = normalize(GetCameraPositionWS() - IN.positionWS);

                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                float3 halfDir = normalize(mainLight.direction + viewDirWS);
                float NdotH = saturate(dot(normalWS, halfDir));
                float specular = pow(NdotH, lerp(8, 128, _Smoothness)) * _Smoothness;

                // 深度差で浅瀬(明るい)から深部(暗い)へブレンドする
                float2 screenUV = IN.positionCS.xy / _ScreenParams.xy;
                float sceneRawDepth = SampleSceneDepth(screenUV);
                float sceneEyeDepth = LinearEyeDepth(sceneRawDepth, _ZBufferParams);
                float surfaceEyeDepth = IN.positionCS.w;
                float depthDiff = saturate((sceneEyeDepth - surfaceEyeDepth) / _DepthFadeDistance);

                float3 baseColor = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthDiff);

                float fresnel = pow(1 - saturate(dot(normalWS, viewDirWS)), _RimPower);
                float3 rim = _RimColor.rgb * fresnel;

                float3 litColor = baseColor * (0.35 + NdotL * 0.65) + specular + rim;
                float alpha = saturate(_Alpha + fresnel * 0.3);

                return half4(litColor, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
