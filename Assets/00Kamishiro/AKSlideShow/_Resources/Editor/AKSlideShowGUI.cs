/*
 * Copyright (c) 2021 AoiKamishiro / 神城葵
 * 
 * This code is provided under the MIT license.
 * 
 * This program uses the following code, which is provided under the MIT License.
 * https://download.unity3d.com/download_unity/008688490035/builtin_shaders-2018.4.20f1.zip?_ga=2.171325672.957521966.1599549120-262519615.1592172043
 * 
 */

#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kamishiro.UnityShader.AKSlideShow
{
    public class AKSlideShowGUI : ShaderGUI
    {
        internal enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
        }
        private MaterialEditor m_MaterialEditor;
        private Material m_Material;
        #region Matdrial Property
        private MaterialProperty blendMode;
        private MaterialProperty dispMode;
        private MaterialProperty lightingMode;
        private MaterialProperty dispTime;
        private MaterialProperty fadeTime;
        private MaterialProperty direction;
        private MaterialProperty count;
        private MaterialProperty useManualControl;
        private MaterialProperty manualSelectedIndex;
        private MaterialProperty textureblendMode;
        private MaterialProperty replaceRatio;
        private MaterialProperty secondaryEmissionPower;
        private MaterialProperty factorTexture;
        private MaterialProperty cull;
        private MaterialProperty color;
        private MaterialProperty cutout;
        private MaterialProperty metallicMap;
        private MaterialProperty metallic;
        private MaterialProperty smoothness;
        private MaterialProperty smoothnessScale;
        private MaterialProperty bumpScale;
        private MaterialProperty bumpMap;
        private MaterialProperty occlusionStrength;
        private MaterialProperty occlusionMap;
        private MaterialProperty emissionColorForRendering;
        private MaterialProperty emissionMap;
        private MaterialProperty highlights;
        private MaterialProperty reflections;
        #region  Albedo Texture
        private MaterialProperty mainTex;
        private MaterialProperty[] textures = new MaterialProperty[32];
        #endregion
        #endregion

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            m_MaterialEditor = materialEditor;
            m_Material = m_MaterialEditor.target as Material;
            FindProperties(properties);
            ShaderPropertiesGUI();
        }
        private void FindProperties(MaterialProperty[] props)
        {
            blendMode = FindProperty("_Mode", props);

            //Main Map
            mainTex = FindProperty("_MainTex", props);
            color = FindProperty("_Color", props);
            cutout = FindProperty("_Cutoff", props);
            metallicMap = FindProperty("_MetallicGlossMap", props);
            metallic = FindProperty("_Metallic", props);
            smoothness = FindProperty("_Glossiness", props);
            smoothnessScale = FindProperty("_GlossMapScale", props);
            bumpScale = FindProperty("_BumpScale", props);
            bumpMap = FindProperty("_BumpMap", props);
            occlusionStrength = FindProperty("_OcclusionStrength", props);
            occlusionMap = FindProperty("_OcclusionMap", props);
            emissionColorForRendering = FindProperty("_EmissionColor", props);
            emissionMap = FindProperty("_EmissionMap", props);
            highlights = FindProperty("_SpecularHighlights", props);
            reflections = FindProperty("_GlossyReflections", props);

            //Secondary Map
            dispMode = FindProperty("_DispMode", props);
            lightingMode = FindProperty("_LightingMode", props);
            dispTime = FindProperty("_DisplayTime", props);
            fadeTime = FindProperty("_TransitionTime", props);
            count = FindProperty("_TextureCount", props);
            direction = FindProperty("_Direction", props);
            cull = FindProperty("_Cull", props);
            useManualControl = FindProperty("_UseManualControl", props);
            manualSelectedIndex = FindProperty("_ManualSelectedIndex", props);
            textureblendMode = FindProperty("_BlendMode", props);
            replaceRatio = FindProperty("_ReplaceRatio", props);
            secondaryEmissionPower = FindProperty("_SecondaryEmissionPower", props);
            factorTexture = FindProperty("_FactorTexture", props);
            for (int i = 0; i < textures.Length; i++) textures[i] = FindProperty("_Tex" + i.ToString(), props);
        }

        private void ShaderPropertiesGUI()
        {
            bool blendModeChanged = false;

            EditorGUI.BeginChangeCheck();
            {

                //blendModeChanged = BlendModePopup();
                m_MaterialEditor.ShaderProperty(lightingMode, Styles.lightingText);

                EditorGUILayout.Space();
                UIHelper.ShurikenHeader(Styles.primaryMapsText);
                {
                    // Albedo
                    m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, mainTex, color);
                    if ((BlendMode)m_Material.GetFloat("_Mode") == BlendMode.Cutout) m_MaterialEditor.ShaderProperty(cutout, cutout.displayName, 1);

                    if (lightingMode.floatValue == 0)
                    {
                        // Metallic Smoothness
                        bool hasGlossMap = metallicMap.textureValue != null;
                        m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap, hasGlossMap ? null : metallic);

                        m_MaterialEditor.ShaderProperty(hasGlossMap ? smoothnessScale : smoothness, hasGlossMap ? Styles.smoothnessScaleText : Styles.smoothnessText, 2);

                        // Normal
                        m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);

                        // Occlusion
                        m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMap, occlusionMap.textureValue != null ? occlusionStrength : null);

                        // Emission
                        if (m_MaterialEditor.EmissionEnabledProperty())
                        {
                            bool hadEmissionTexture = emissionMap.textureValue != null;
                            m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, false);
                            m_MaterialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
                        }
                    }
                }

                EditorGUILayout.Space();
                UIHelper.ShurikenHeader(Styles.secondaryMapsText);
                {
                    m_MaterialEditor.ShaderProperty(dispMode, dispMode.displayName);
                    if (dispMode.floatValue == 1)
                    {
                        m_MaterialEditor.ShaderProperty(direction, direction.displayName, 1);
                    }
                    if (dispMode.floatValue == 0)
                    {
                        m_MaterialEditor.ShaderProperty(useManualControl, useManualControl.displayName, 1);
                        if (useManualControl.floatValue == 1) m_MaterialEditor.ShaderProperty(manualSelectedIndex, manualSelectedIndex.displayName, 2);
                    }
                    if (dispMode.floatValue == 3)
                    {
                        m_MaterialEditor.TexturePropertySingleLine(new GUIContent(factorTexture.displayName), factorTexture);
                    }

                    EditorGUILayout.Space();

                    m_MaterialEditor.ShaderProperty(dispTime, dispTime.displayName);
                    if (dispMode.floatValue == 1 || dispMode.floatValue == 2 || dispMode.floatValue == 3) m_MaterialEditor.ShaderProperty(fadeTime, fadeTime.displayName, 1);

                    EditorGUILayout.Space();

                    m_MaterialEditor.ShaderProperty(textureblendMode, textureblendMode.displayName);
                    if (textureblendMode.floatValue == 1) m_MaterialEditor.ShaderProperty(replaceRatio, replaceRatio.displayName, 1);

                    EditorGUILayout.Space();

                    if (m_Material.shaderKeywords.ToList().Contains("_EMISSION") && lightingMode.floatValue == 0)
                    {
                        m_MaterialEditor.ShaderProperty(secondaryEmissionPower, secondaryEmissionPower.displayName);

                        EditorGUILayout.Space();
                    }

                    count.floatValue = EditorGUILayout.IntSlider(count.displayName, (int)count.floatValue, 1, 32);
                    int num = (int)count.floatValue;
                    for (int i = 0; i < textures.Length; i++) if (num >= i + 1) m_MaterialEditor.TexturePropertySingleLine(new GUIContent(textures[i].displayName), textures[i]);
                }

                EditorGUILayout.Space();
                UIHelper.ShurikenHeader(Styles.forwardText);
                {
                    m_MaterialEditor.ShaderProperty(highlights, Styles.highlightsText);
                    m_MaterialEditor.ShaderProperty(reflections, Styles.reflectionsText);
                }

                EditorGUILayout.Space();
                UIHelper.ShurikenHeader(Styles.advancedText);
                {
                    m_MaterialEditor.ShaderProperty(cull, cull.displayName);
                    m_MaterialEditor.RenderQueueField();
                    m_MaterialEditor.EnableInstancingField();
                    m_MaterialEditor.DoubleSidedGIField();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (UnityEngine.Object obj in textureblendMode.targets) MaterialChanged((Material)obj, blendModeChanged);
            }
        }
        private void MaterialChanged(Material mat, bool overrideRenderQueue)
        {
            SetupMaterialWithBlendMode(mat, (BlendMode)mat.GetFloat("_Mode"), overrideRenderQueue);
            SetMaterialKeywords(mat);
        }

        private void SetMaterialKeywords(Material m)
        {
            SetKeyword(m, "_NORMALMAP", m.GetTexture("_BumpMap"));
            SetKeyword(m, "_METALLICGLOSSMAP", m.GetTexture("_MetallicGlossMap"));

            MaterialEditor.FixupEmissiveFlag(m);
            bool shouldEmissionBeEnabled = (m.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            SetKeyword(m, "_EMISSION", shouldEmissionBeEnabled);
        }
        private void SetKeyword(Material m, string keyword, bool state)
        {
            if (state) m.EnableKeyword(keyword);
            else m.DisableKeyword(keyword);
        }

        private bool BlendModePopup()
        {
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            BlendMode mode = (BlendMode)blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
            bool result = EditorGUI.EndChangeCheck();
            if (result)
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;

            return result;
        }

        private void SetupMaterialWithBlendMode(Material m, BlendMode blendMode, bool overrideRenderQueue)
        {
            int minRenderQueue = -1;
            int maxRenderQueue = 5000;
            int defaultRenderQueue = -1;
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    m.SetOverrideTag("RenderType", "");
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    m.SetInt("_ZWrite", 1);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.DisableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = -1;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest - 1;
                    defaultRenderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    m.SetOverrideTag("RenderType", "TransparentCutout");
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    m.SetInt("_ZWrite", 1);
                    m.EnableKeyword("_ALPHATEST_ON");
                    m.DisableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast;
                    defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    m.SetOverrideTag("RenderType", "Transparent");
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite", 0);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast + 1;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay - 1;
                    defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    m.SetOverrideTag("RenderType", "Transparent");
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite", 0);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.DisableKeyword("_ALPHABLEND_ON");
                    m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast + 1;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay - 1;
                    defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }

            if (overrideRenderQueue || m.renderQueue < minRenderQueue || m.renderQueue > maxRenderQueue)
            {
                if (!overrideRenderQueue) Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Render queue value outside of the allowed range ({0} - {1}) for selected Blend mode, resetting render queue to default", minRenderQueue, maxRenderQueue);
                m.renderQueue = defaultRenderQueue;
            }
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            MaterialChanged(material, true);
        }
    }
    internal class Styles
    {
        public static GUIContent albedoText = EditorGUIUtility.TrTextContent("Albedo", "Albedo (RGB) and Transparency (A)");
        public static GUIContent alphaCutoffText = EditorGUIUtility.TrTextContent("Alpha Cutoff", "Threshold for alpha cutoff");
        public static GUIContent metallicMapText = EditorGUIUtility.TrTextContent("Metallic", "Metallic (R) and Smoothness (A)");
        public static GUIContent smoothnessText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness value");
        public static GUIContent smoothnessScaleText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness scale factor");
        public static GUIContent highlightsText = EditorGUIUtility.TrTextContent("Specular Highlights", "Specular Highlights");
        public static GUIContent reflectionsText = EditorGUIUtility.TrTextContent("Reflections", "Glossy Reflections");
        public static GUIContent normalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");
        public static GUIContent occlusionText = EditorGUIUtility.TrTextContent("Occlusion", "Occlusion (G)");
        public static GUIContent emissionText = EditorGUIUtility.TrTextContent("Color", "Emission (RGB)");

        public static string primaryMapsText = "Main Maps";
        public static string secondaryMapsText = "Secondary Maps";
        public static string forwardText = "Forward Rendering Options";
        public static string lightingText = "Lighting Mode";
        public static string renderingMode = "Rendering Mode";
        public static string advancedText = "Advanced Options";

        public static readonly string[] blendNames = Enum.GetNames(typeof(AKSlideShowGUI.BlendMode));

        public const string localVer = "Local Version: ";
        public const string remoteVer = "Remote Version: ";
        public const string btnUpdate = "Download latest version.";
        public const string btnBooth = "Download from Booth";
        public const string btnVket = "Download from Vket";
        public const string btnDescription = "操作説明（日本語）";
        public const string btnReadme = "README.md";
        public const string nameMultiTexturePoster = "Multi Texture Poster";
        public const string author = "Author: AoiKamishiro / 神城アオイ";
        public const string linkRelease = "https://github.com/AoiKamishiro/UnityShader_MultiTexturePoster/releases";
        public const string linkReadme = "https://github.com/AoiKamishiro/UnityShader_MultiTexturePoster";
        public const string linkDescription = "https://github.com/AoiKamishiro/UnityShader_MultiTexturePoster/blob/master/Description.md";
        public const string linkBooth = "https://kamishirolab.booth.pm/items/2483104";
        public const string linkVket = "https://www.v-market.work/ec/mypage/items/3997/detail/";
    }
    internal class UIHelper
    {
        private static readonly int HEADER_HEIGHT = 22;
        private static Rect DrawShuriken(string title, Vector2 contentOffset)
        {
            GUIStyle style = new GUIStyle("ShurikenModuleTitle")
            {
                margin = new RectOffset(0, 0, 8, 0),
                font = new GUIStyle(EditorStyles.boldLabel).font,
                border = new RectOffset(15, 7, 4, 4),
                fixedHeight = HEADER_HEIGHT,
                contentOffset = contentOffset
            };
            Rect rect = GUILayoutUtility.GetRect(16f, HEADER_HEIGHT, style);
            GUI.Box(rect, title, style);
            return rect;
        }
        public static void ShurikenHeader(string title)
        {
            DrawShuriken(title, new Vector2(6f, -2f));
        }
        public static bool ShurikenFoldout(string title, bool display)
        {
            Rect rect = DrawShuriken(title, new Vector2(20f, -2f));
            Event e = Event.current;
            Rect toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }
            return display;
        }
        public static void OpenLink(string link)
        {
            Application.OpenURL(link);
        }
    }
}
#endif