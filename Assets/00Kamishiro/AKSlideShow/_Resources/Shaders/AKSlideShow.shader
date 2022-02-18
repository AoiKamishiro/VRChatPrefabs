Shader "Kamishiro/AKSlideShow"
{
    Properties
    {
        //-------------------------------------------------------------------------------------
        //#region Standard Shading Properties
        _MainTex ("Main Texture", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1)
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _Glossiness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale ("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Gamma] _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap ("Metallic", 2D) = "white" { }
        _BumpScale ("Scale", Float) = 1.0
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" { }
        _OcclusionStrength ("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap ("Occlusion", 2D) = "white" { }
        _EmissionColor ("Color", Color) = (0, 0, 0)
        _EmissionMap ("Emission", 2D) = "white" { }
        [ToggleOff] _SpecularHighlights ("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections ("Glossy Reflections", Float) = 1.0
        //#endregion

        //-------------------------------------------------------------------------------------
        //#region AKSS Properties
        _DisplayTime ("Display Time", float) = 5
        _TransitionTime ("Transition Time", float) = 1
        _TextureCount ("Texture Count", int) = 5
        [Enum(Simple, 0, Slide, 1, XFade, 2, Texture, 3)]_DispMode ("Mode", int) = 0
        [Enum(Up, 0, Down, 1, Right, 2, Left, 3)]_Direction ("Scroll Direction", float) = 0
        [Enum(Standard, 0, Unlit, 1)]_LightingMode ("Lighting Mode", float) = 0
        [Enum(Multiply, 0, Replace, 1, Substruct, 2, Additive, 3)]_BlendMode ("Texture Blend Mode", float) = 0
        _ReplaceRatio ("Replace Ratio", Range(0, 1)) = 0
        _SecondaryEmissionPower ("Emission Ratio", Range(0, 1)) = 0
        [MaterialToggle]_UseManualControl ("Use Manual Control", float) = 0
        _ManualSelectedIndex ("Selected Texture Index", int) = 0
        _FactorTexture ("Factor Texture", 2D) = "gray" { }
        _Dummy ("_Dummy", 2D) = "white" { }
        _Tex0 ("Texture0", 2D) = "black" { }
        _Tex1 ("Texture1", 2D) = "black" { }
        _Tex2 ("Texture2", 2D) = "black" { }
        _Tex3 ("Texture3", 2D) = "black" { }
        _Tex4 ("Texture4", 2D) = "black" { }
        _Tex5 ("Texture5", 2D) = "black" { }
        _Tex6 ("Texture6", 2D) = "black" { }
        _Tex7 ("Texture7", 2D) = "black" { }
        _Tex8 ("Texture8", 2D) = "black" { }
        _Tex9 ("Texture9", 2D) = "black" { }
        _Tex10 ("Texture10", 2D) = "black" { }
        _Tex11 ("Texture11", 2D) = "black" { }
        _Tex12 ("Texture12", 2D) = "black" { }
        _Tex13 ("Texture13", 2D) = "black" { }
        _Tex14 ("Texture14", 2D) = "black" { }
        _Tex15 ("Texture15", 2D) = "black" { }
        _Tex16 ("Texture16", 2D) = "black" { }
        _Tex17 ("Texture17", 2D) = "black" { }
        _Tex18 ("Texture18", 2D) = "black" { }
        _Tex19 ("Texture19", 2D) = "black" { }
        _Tex20 ("Texture20", 2D) = "black" { }
        _Tex21 ("Texture21", 2D) = "black" { }
        _Tex22 ("Texture22", 2D) = "black" { }
        _Tex23 ("Texture23", 2D) = "black" { }
        _Tex24 ("Texture24", 2D) = "black" { }
        _Tex25 ("Texture25", 2D) = "black" { }
        _Tex26 ("Texture26", 2D) = "black" { }
        _Tex27 ("Texture27", 2D) = "black" { }
        _Tex28 ("Texture28", 2D) = "black" { }
        _Tex29 ("Texture29", 2D) = "black" { }
        _Tex30 ("Texture30", 2D) = "black" { }
        _Tex31 ("Texture31", 2D) = "black" { }
        //#endregion

        //-------------------------------------------------------------------------------------
        //#region Rendering Options
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", float) = 2
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
        //#endregion

        //-------------------------------------------------------------------------------------
        //#region Version Checker
        [HideInInspector]_Version ("Version", int) = 3
        //#endregion

    }
    SubShader
    {
        Name "AKSS"
        Tags { "RenderType" = "Opaque" }
        Cull [_Cull]
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]
        
        CGPROGRAM

        //-------------------------------------------------------------------------------------
        //#region pragma
        //#if defined(_ALPHABLEND_ON)
        //#pragma surface surf AKSS vertex:vert fullforwardshadows addshadow alpha:fade
        //#elif defined(_ALPHAPREMULTIPLY_ON)
        //#pragma surface surf AKSS vertex:vert fullforwardshadows addshadow alpha:blend
        //#elif defined(_ALPHATEST_ON)
        //#pragma surface surf AKSS vertex:vert fullforwardshadows addshadow alphatest:_Cutoff
        //#else
            #pragma surface surf AKSS vertex:vert fullforwardshadows addshadow
        //#endif

        #pragma target 3.5
        #pragma multi_compile_fog
        #pragma multi_compile_instancing
        #pragma shader_feature _EMISSION
        #pragma shader_feature_local _NORMALMAP
        #pragma shader_feature_local _METALLICGLOSSMAP
        //#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
        #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
        #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF
        //#endregion

        //-------------------------------------------------------------------------------------
        //#region Structs
        struct Appdata
        {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
            float4 texcoord1 : TEXCOORD1;
            float4 texcoord2 : TEXCOORD2;
            fixed4 color : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Input
        {
            fixed singleTime;
            fixed cycleTime;
            fixed currentId;
            fixed nextId;
            fixed currentRatio;
            fixed isTransision;
            fixed lightingMode;
            fixed dispMode;
            fixed direction;
            float2 uv_MainTex;
            fixed4 color;
            fixed bumpScale;
            fixed occlusionStrength;
            fixed glossMapScale;
            fixed glossiness;
            fixed metallic;
            fixed3 emissionColor;
            fixed blendMode;
            fixed replaceRatio;
            fixed secondaryEmissionPower;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };
        struct SurfaceOutputAKSS
        {
            fixed3 Albedo;
            float3 Normal;
            half3 Emission;
            fixed Metallic;
            fixed Smoothness;
            fixed Occlusion;
            fixed Alpha;
            fixed LightingMode;
        };
        //#endregion

        //-------------------------------------------------------------------------------------
        //#region BRDF Selector
        #if !defined(UNITY_BRDF_PBS)
            #if SHADER_TARGET < 30 || defined(SHADER_TARGET_SURFACE_ANALYSIS)
                #define UNITY_BRDF_PBS BRDF3_Unity_PBS
            #elif defined(UNITY_PBS_USE_BRDF3)
                #define UNITY_BRDF_PBS BRDF3_Unity_PBS
            #elif defined(UNITY_PBS_USE_BRDF2)
                #define UNITY_BRDF_PBS BRDF2_Unity_PBS
            #elif defined(UNITY_PBS_USE_BRDF1)
                #define UNITY_BRDF_PBS BRDF1_Unity_PBS
            #else
                #error something broke in auto - choosing BRDF
            #endif
        #endif
        //#endregion

        //-------------------------------------------------------------------------------------
        //#region Instanced Properties
        UNITY_INSTANCING_BUFFER_START(prop)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _DispMode)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _TextureCount)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _DisplayTime)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _TransitionTime)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _UseManualControl)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _ManualSelectedIndex)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _Direction)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _LightingMode)
        UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _Metallic)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _Glossiness)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _GlossMapScale)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _OcclusionStrength)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _BumpScale)
        UNITY_DEFINE_INSTANCED_PROP(fixed3, _EmissionColor)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _BlendMode)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _ReplaceRatio)
        UNITY_DEFINE_INSTANCED_PROP(fixed, _SecondaryEmissionPower)
        UNITY_INSTANCING_BUFFER_END(prop)
        //#endregion

        //-------------------------------------------------------------------------------------
        //#region Textures sampler
        UNITY_DECLARE_TEX2D(_Dummy);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex1);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex2);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex3);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex4);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex5);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex6);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex7);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex8);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex9);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex10);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex11);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex12);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex13);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex14);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex15);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex16);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex17);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex18);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex19);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex20);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex21);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex22);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex23);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex24);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex25);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex26);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex27);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex28);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex29);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex30);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Tex31);
        
        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _MetallicGlossMap;
        sampler2D _OcclusionMap;
        sampler2D _EmissionMap;
        sampler2D _FactorTexture;
        //#endregion

        //-------------------------------------------------------------------------------------
        //#region Fragment Shader Functions
        half4 SimpleMode(float2 uv, fixed currentId)
        {
            half4 col = UNITY_SAMPLE_TEX2D(_Dummy, uv);
            switch(round(currentId))
            {
                case 0:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex0, _Dummy, uv);
                case 1:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex1, _Dummy, uv);
                case 2:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex2, _Dummy, uv);
                case 3:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex3, _Dummy, uv);
                case 4:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex4, _Dummy, uv);
                case 5:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex5, _Dummy, uv);
                case 6:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex6, _Dummy, uv);
                case 7:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex7, _Dummy, uv);
                case 8:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex8, _Dummy, uv);
                case 9:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex9, _Dummy, uv);
                case 10:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex10, _Dummy, uv);
                case 11:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex11, _Dummy, uv);
                case 12:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex12, _Dummy, uv);
                case 13:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex13, _Dummy, uv);
                case 14:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex14, _Dummy, uv);
                case 15:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex15, _Dummy, uv);
                case 16:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex16, _Dummy, uv);
                case 17:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex17, _Dummy, uv);
                case 18:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex18, _Dummy, uv);
                case 19:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex19, _Dummy, uv);
                case 20:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex20, _Dummy, uv);
                case 21:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex21, _Dummy, uv);
                case 22:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex22, _Dummy, uv);
                case 23:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex23, _Dummy, uv);
                case 24:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex24, _Dummy, uv);
                case 25:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex25, _Dummy, uv);
                case 26:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex26, _Dummy, uv);
                case 27:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex27, _Dummy, uv);
                case 28:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex28, _Dummy, uv);
                case 29:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex29, _Dummy, uv);
                case 30:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex30, _Dummy, uv);
                case 31:
                    return col *= UNITY_SAMPLE_TEX2D_SAMPLER(_Tex31, _Dummy, uv);
                default:
                    return col *= float4(0, 0, 0, 1);
            }
        }
        half4 SlideMode(float2 uv, fixed currentId, fixed nextId, fixed isTransision, fixed ratio, fixed direction)
        {
            isTransision = round(isTransision);
            ratio = saturate(isTransision ? ratio : 0);
            direction = round(direction);
            switch(direction)
            {
                case 0:
                    return SimpleMode(isTransision ? float2(uv.x, direction == 0 ? uv.y - ratio : uv.y + ratio) : uv, uv.y > ratio ? currentId : nextId);
                case 1:
                    return SimpleMode(isTransision ? float2(uv.x, direction == 0 ? uv.y - ratio : uv.y + ratio) : uv, uv.y > (1 - ratio) ? nextId : currentId);
                case 2:
                    return SimpleMode(isTransision ? float2(direction == 2 ? uv.x - ratio : uv.x + ratio, uv.y) : uv, uv.x > ratio ? currentId : nextId);
                case 3:
                    return SimpleMode(isTransision ? float2(direction == 2 ? uv.x - ratio : uv.x + ratio, uv.y) : uv, uv.x > (1 - ratio) ? nextId : currentId);
                default:
                    return half4(0, 0, 0, 1);
            }
        }
        half4 XFadeMode(float2 uv, fixed currentId, fixed nextId, fixed isTransision, fixed ratio)
        {
            half4 curr = SimpleMode(uv, currentId);
            half4 next = SimpleMode(uv, nextId);

            ratio = step(0.5f, isTransision) * ratio;

            return curr * (1 - ratio) + next * ratio;
        }
        half4 TextureMode(float2 uv, fixed currentId, fixed nextId, fixed isTransision, fixed ratio)
        {
            half4 curr = SimpleMode(uv, currentId);
            half4 next = SimpleMode(uv, nextId);
            fixed factor = tex2D(_FactorTexture, uv).r;

            ratio = step(0.5f, isTransision) * ratio;

            return ratio > factor ? next : curr;
        }
        half4 SlideShow(Input v2f)
        {
            switch(round(v2f.dispMode))
            {
                case 0:
                    return SimpleMode(v2f.uv_MainTex, v2f.currentId);
                case 1:
                    return SlideMode(v2f.uv_MainTex, v2f.currentId, v2f.nextId, v2f.isTransision, v2f.currentRatio, v2f.direction);
                case 2:
                    return XFadeMode(v2f.uv_MainTex, v2f.currentId, v2f.nextId, v2f.isTransision, v2f.currentRatio);
                case 3:
                    return TextureMode(v2f.uv_MainTex, v2f.currentId, v2f.nextId, v2f.isTransision, v2f.currentRatio);
                default:
                    return half4(0, 0, 0, 1);
            }
        }
        half4 Albedo(Input v2f, half4 slideShow)
        {
            half4 prim = tex2D(_MainTex, v2f.uv_MainTex) * v2f.color;
            half4 sec = slideShow;
            switch(round(v2f.blendMode))
            {
                case 0:
                    return prim * sec;
                case 1:
                    return prim * (1 - v2f.replaceRatio) + sec * v2f.replaceRatio;
                case 2:
                    return saturate(prim - sec);
                case 3:
                    return saturate(prim + sec);
                default:
                    return prim * sec;
            }
        }
        fixed3 Normal(Input v2f)
        {
            return UnpackScaleNormal(tex2D(_BumpMap, v2f.uv_MainTex), v2f.bumpScale);
        }
        fixed2 MetallicGloss(Input v2f)
        {
            half2 mg = half2(0, 0);
            
            #ifdef _METALLICGLOSSMAP
                mg = tex2D(_MetallicGlossMap, v2f.uv_MainTex).ra * half2(1, v2f.glossMapScale);
            #else
                mg = half2(v2f.metallic, v2f.glossiness);
            #endif

            return mg;
        }
        fixed Occlusion(Input v2f)
        {
            half occ = tex2D(_OcclusionMap, v2f.uv_MainTex).g;
            return LerpOneTo(occ, v2f.occlusionStrength);
        }
        fixed3 Emission(Input v2f, half4 slideShow)
        {
            #ifdef _EMISSION
                fixed3 base = tex2D(_EmissionMap, v2f.uv_MainTex).rgb * v2f.emissionColor.rgb;
                return v2f.lightingMode == 0 ? base * (1 - v2f.secondaryEmissionPower) + slideShow.rgb * v2f.secondaryEmissionPower : 0;
            #else
                return 0;
            #endif
        }
        //#endregion

        //-------------------------------------------------------------------------------------
        //#region AKSS Lighting
        half4 LightingAKSS(SurfaceOutputAKSS s, float3 viewDir, UnityGI gi)
        {
            if (s.LightingMode < 0.5)
            {
                s.Normal = normalize(s.Normal);
                half oneMinusReflectivity;
                half3 specColor;
                s.Albedo = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, specColor, oneMinusReflectivity);
                half outputAlpha;
                s.Albedo = PreMultiplyAlpha(s.Albedo, s.Alpha, oneMinusReflectivity, outputAlpha);
                half4 c = UNITY_BRDF_PBS(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
                c.a = outputAlpha;
                return c;
            }
            else
            {
                return half4(s.Albedo, s.Alpha);
            }
        }
        inline half4 LightingAKSS_Deferred(SurfaceOutputAKSS s, float3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
        {
            if (s.LightingMode == 0)
            {
                half oneMinusReflectivity;
                half3 specColor;
                s.Albedo = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, specColor, oneMinusReflectivity);
                half4 c = UNITY_BRDF_PBS(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);

                UnityStandardData data;
                data.diffuseColor = s.Albedo;
                data.occlusion = s.Occlusion;
                data.specularColor = specColor;
                data.smoothness = s.Smoothness;
                data.normalWorld = s.Normal;

                UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

                half4 emission = half4(s.Emission + c.rgb, 1);
                return emission;
            }
            else
            {
                half oneMinusReflectivity;
                half3 specColor;
                half4 c = UNITY_BRDF_PBS(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);

                UnityStandardData data;
                data.diffuseColor = s.Albedo;
                data.occlusion = 1;
                data.specularColor = half3(1, 1, 1);
                data.smoothness = 0;
                data.normalWorld = s.Normal;

                UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

                half4 emission = half4(0, 0, 0, 1);
                return emission;
            }
        }
        inline void LightingAKSS_GI(SurfaceOutputAKSS s, UnityGIInput data, inout UnityGI gi)
        {
            #if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
                gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
            #else
                Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Metallic));
                gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
            #endif
        }
        //#endregion

        //-------------------------------------------------------------------------------------
        //#region Vertex Shader
        void vert(inout Appdata vertIn, out Input vertOut)
        {
            UNITY_SETUP_INSTANCE_ID(vertIn);
            UNITY_INITIALIZE_OUTPUT(Input, vertOut);
            UNITY_TRANSFER_INSTANCE_ID(vertIn, vertOut);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertOut);

            fixed dispMode = UNITY_ACCESS_INSTANCED_PROP(prop, _DispMode);
            fixed displayTime = UNITY_ACCESS_INSTANCED_PROP(prop, _DisplayTime);
            fixed transitionTime = UNITY_ACCESS_INSTANCED_PROP(prop, _TransitionTime);
            fixed textureCount = UNITY_ACCESS_INSTANCED_PROP(prop, _TextureCount);
            fixed useManualControl = UNITY_ACCESS_INSTANCED_PROP(prop, _UseManualControl);
            fixed manualSelectedIndex = UNITY_ACCESS_INSTANCED_PROP(prop, _ManualSelectedIndex);
            fixed lightingMode = UNITY_ACCESS_INSTANCED_PROP(prop, _LightingMode);
            fixed direction = UNITY_ACCESS_INSTANCED_PROP(prop, _Direction);
            
            fixed singleTime = dispMode == 0 ?displayTime : displayTime + transitionTime;
            fixed cycleTime = singleTime * textureCount;
            fixed localTime = useManualControl == 1 ? fmod(singleTime * manualSelectedIndex + 0.001, cycleTime) : fmod(_Time.y, cycleTime);
            fixed currentId = floor(localTime / singleTime);
            fixed nextId = currentId + 1 >= textureCount ? 0 : currentId + 1;
            fixed currentSingleTime = fmod(localTime, singleTime);
            fixed currentRatio = (currentSingleTime - displayTime) / transitionTime;
            fixed isTransision = currentSingleTime < displayTime ? 0 : 1;

            vertOut.uv_MainTex = vertIn.texcoord;
            vertOut.singleTime = singleTime;
            vertOut.cycleTime = cycleTime;
            vertOut.currentId = currentId;
            vertOut.nextId = nextId;
            vertOut.currentRatio = currentRatio;
            vertOut.isTransision = isTransision;
            vertOut.lightingMode = lightingMode;
            vertOut.dispMode = dispMode;
            vertOut.direction = direction;
            vertOut.color = UNITY_ACCESS_INSTANCED_PROP(prop, _Color);
            vertOut.metallic = UNITY_ACCESS_INSTANCED_PROP(prop, _Metallic);
            vertOut.glossiness = UNITY_ACCESS_INSTANCED_PROP(prop, _Glossiness);
            vertOut.glossMapScale = UNITY_ACCESS_INSTANCED_PROP(prop, _GlossMapScale);
            vertOut.occlusionStrength = UNITY_ACCESS_INSTANCED_PROP(prop, _OcclusionStrength);
            vertOut.bumpScale = UNITY_ACCESS_INSTANCED_PROP(prop, _BumpScale);
            vertOut.emissionColor = UNITY_ACCESS_INSTANCED_PROP(prop, _EmissionColor);
            vertOut.blendMode = UNITY_ACCESS_INSTANCED_PROP(prop, _BlendMode);
            vertOut.replaceRatio = UNITY_ACCESS_INSTANCED_PROP(prop, _ReplaceRatio);
            vertOut.secondaryEmissionPower = UNITY_ACCESS_INSTANCED_PROP(prop, _SecondaryEmissionPower);
        }
        //#endregion

        //-------------------------------------------------------------------------------------
        //#region Surface Shader
        void surf(Input surfIn, inout SurfaceOutputAKSS surfOut)
        {
            half4 slideShow = SlideShow(surfIn);
            half4 albedo = Albedo(surfIn, slideShow);
            fixed2 metal = MetallicGloss(surfIn);

            #if defined(_ALPHATEST_ON)
                albedo.a = step(0.5, albedo.a);
                clip(albedo - 0.1);
            #elif !defined(_ALPHABLEND_ON) && !defined(_ALPHAPREMULTIPLY_ON)
                albedo.a = 1;
            #endif


            surfOut.Albedo = albedo.rgb;
            surfOut.Alpha = albedo.a;
            surfOut.Normal = Normal(surfIn);
            surfOut.Emission = Emission(surfIn, slideShow);
            surfOut.Metallic = metal.x;
            surfOut.Smoothness = metal.y;
            surfOut.Occlusion = Occlusion(surfIn);
            surfOut.LightingMode = surfIn.lightingMode;
        }
        //#endregion

        ENDCG

    }
    CustomEditor "Kamishiro.UnityShader.AKSlideShow.AKSlideShowGUI"
    Fallback "Standard"
}