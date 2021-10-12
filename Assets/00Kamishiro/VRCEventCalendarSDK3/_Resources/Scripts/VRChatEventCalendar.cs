/*
* Copyright (c) 2021 AoiKamishiro
* 
* This code is provided under the MIT license.
* 
*/

#if VRC_SDK_VRCSDK3

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UdonSharpEditor;
#endif

namespace Kamishiro.VRChatUDON.VRChatEventCalendar.SDK3
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VRChatEventCalendar : UdonSharpBehaviour
    {
        public string cuid = "";
        public bool isMain = false;
        public float initialLoad = 10f;
        public bool affectedLight = false;
        public Material litCalendarMat;
        public Material unlitCalendarMat;
        public SphereCollider[] colliders; //setMainCollider
        public UdonBehaviour videoPlayer;
        public UdonBehaviour colorCodeReader;
        public SphereCollider[] s_touchColliders;//self
        public SyncCalendar s_syncScrollCalendar;//self
        public TouchDetector[] s_touchDetectors;//self
        public VRCPlayerApi _lp;

        #region editorObject
        public float intensity = 0;
        public string param = "_Intensity";
        public Camera codeReaderCam;
        public Canvas canvas;
        public MeshRenderer display;
        public int platform = 0;
        #endregion

        private void Start()
        {
            _lp = Networking.LocalPlayer;
            SendCustomEventDelayedSeconds(nameof(ActivateTouch), 2.5f);
        }
        public void ActivateTouch()
        {
            if (_lp != null && _lp.IsUserInVR())
            {
                s_touchDetectors[0].transform.parent.gameObject.SetActive(true);
                colorCodeReader.transform.parent.gameObject.SetActive(true);
                if (isMain)
                    colliders[0].transform.parent.gameObject.SetActive(true);
            }
        }
        public void _SliderDraggd()
        {
            s_syncScrollCalendar.SendCustomEvent(nameof(SyncCalendar._TakeOwnerShip));
            s_syncScrollCalendar.SendCustomEvent(nameof(SyncCalendar._SliderVaueChanged));
        }
        public void _ReLoadVideo()
        {
            videoPlayer.SendCustomEvent(nameof(ImageLoader._ReLoad));
        }
        public void _OnCalendarReady()
        {
            colorCodeReader.SendCustomEvent(nameof(ColorCodeReader._CalclateHeight));
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void OnValidate()
        {
            SetMaterialEmission();
            SwitchMaterials();
        }
        public void EditorUpdate()
        {
            SetMainCam();
            AdjustScale();
            AdjustReaderCameraSize();
        }
        public void SetMainCam()
        {
            if (string.IsNullOrWhiteSpace(gameObject.scene.path))
                return;

            if (canvas == null)
                return;

            if (canvas.worldCamera != null)
                return;

            GameObject[] cams = GameObject.FindGameObjectsWithTag("MainCamera");
            if (cams.Length == 0)
                return;

            Camera mCam = cams[0].GetComponent<Camera>();
            if (mCam == null)
                return;

            canvas.worldCamera = mCam;

            Undo.RecordObject(canvas, "Udon Event Calendar - Setup Canvas Maincam");
            EditorUtility.SetDirty(canvas);
        }
        public void AdjustScale()
        {
            Vector3 localScale = transform.localScale;
            Vector3 newScale = localScale;
            if (transform.parent == null)
            {
                if (localScale.y == localScale.z && localScale.x != localScale.y)
                {
                    newScale = new Vector3(localScale.x, localScale.x, localScale.x);
                }
                else if (localScale.x == localScale.z && localScale.y != localScale.z)
                {
                    newScale = new Vector3(localScale.y, localScale.y, localScale.y);
                }
                else if (localScale.x == localScale.y && localScale.z != localScale.x)
                {
                    newScale = new Vector3(localScale.z, localScale.z, localScale.z);
                }
                else if (localScale.x != localScale.y && localScale.y != localScale.z)
                {
                    newScale = new Vector3(localScale.x, localScale.x, localScale.x);
                }
            }
            else
            {
                Vector3 parentScale = transform.parent.lossyScale;
                if (localScale.y * parentScale.z == localScale.z * parentScale.y && localScale.x * parentScale.y != localScale.y * parentScale.x)
                {
                    newScale = new Vector3(localScale.x, localScale.x * parentScale.x / parentScale.y, localScale.x * parentScale.x / parentScale.z);
                }
                else if (localScale.x * parentScale.z == localScale.z * parentScale.x && localScale.y * parentScale.z != localScale.z * parentScale.y)
                {
                    newScale = new Vector3(localScale.y * parentScale.y / parentScale.x, localScale.y, localScale.y * parentScale.y / parentScale.z);
                }
                else if (localScale.x * parentScale.y == localScale.y * parentScale.x && localScale.z * parentScale.x != localScale.x * parentScale.z)
                {
                    newScale = new Vector3(localScale.z * parentScale.z / parentScale.x, localScale.z * parentScale.z / parentScale.y, localScale.z);
                }
                else if (localScale.x * parentScale.y != localScale.y * parentScale.x && localScale.y * parentScale.z != localScale.z * parentScale.y)
                {
                    newScale = new Vector3(localScale.x, localScale.x * parentScale.x / parentScale.y, localScale.x * parentScale.x / parentScale.z);
                }
            }
            if (localScale != newScale)
            {
                transform.localScale = newScale;
                Undo.RecordObject(transform, "Udon Event Calendar - Adjust Scale");
                EditorUtility.SetDirty(transform);
            }
        }
        public void AdjustReaderCameraSize()
        {
            if (codeReaderCam == null)
                return;

            float size = codeReaderCam.transform.lossyScale.x * 1.0f / 2.0f;

            if (codeReaderCam.orthographicSize == size)
                return;

            codeReaderCam.orthographicSize = size;

            Undo.RecordObject(codeReaderCam, "Udon Event Calendar - Adjust ReaderCameraSize");
            EditorUtility.SetDirty(codeReaderCam);
        }
        public void SetMaterialEmission()
        {
            Material[] materials = new Material[] { unlitCalendarMat, litCalendarMat };
            foreach (Material material in materials)
            {
                if (material == null)
                    continue;

                if (material.GetFloat(param) == intensity)
                    continue;

                material.SetFloat(param, intensity);
                Undo.RecordObject(material, "UECalendar2 - Set Material Emission");
                EditorUtility.SetDirty(material);
            }
        }
        public void SwitchMaterials()
        {
            if (display != null)
            {
                display.sharedMaterial = affectedLight ? litCalendarMat : unlitCalendarMat;
                Undo.RecordObject(display, "UECalendar2 - Set Materials");
                EditorUtility.SetDirty(display);
            }
        }
#endif
    }
#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [CustomEditor(typeof(VRChatEventCalendar))]
    public class UECalendarV2Editor : UnityEditor.Editor
    {
        #region SerializedProperty
        private SerializedProperty _cuid;
        private SerializedProperty _isMain;
        private SerializedProperty _initialLoad;
        private SerializedProperty _affectedLight;
        private SerializedProperty _litCalendarMat;
        private SerializedProperty _unlitCalendarMat;
        private SerializedProperty _videoPlayer;
        private SerializedProperty _colorCodeReader;
        private SerializedProperty _colliders;
        private SerializedProperty _s_touchColliders;
        private SerializedProperty _s_touchDetectors;
        private SerializedProperty _s_syncScrollCalendar;
        private SerializedProperty _intensity;
        private SerializedProperty _param;
        private SerializedProperty _codeReaderCam;
        private SerializedProperty _canvas;
        private SerializedProperty _display;
        private SerializedProperty _platform;
        #endregion
        #region Editor Objects
        private Texture headerTexture;
        private Texture githubTexture;
        private Texture twitterTexture;
        private Texture discordTexture;
        private const string guidHeader = "f873df6cfa33a12488669e8b52419406";
        private const string guidDiscordIcon = "5230bd08cdca76d458733bf746392709";
        private const string guidGitHubIcon = "02a0f664576d2fb43b62d88d7143775f";
        private const string guidTwitterIcon = "fe62b470adc237a4ea6a6fc6e51be64f";
        private const string urlTwitter = "https://twitter.com/aoi3192";
        private const string urlDiscord = "https://discord.gg/8muNKrzaSK";
        private const string urlGitHub = "https://github.com/AoiKamishiro/VRC_UdonPrefabs";
        private Texture[] textures;
        private string[] guids;
        private string[] urls;
        private VRChatEventCalendar _uec2;
        private static bool fold_InternalReference = false;
        private static bool fold_udonSetting = false;
        #endregion
        #region Transrator
        private static Language lang = Language.Japanese;
        private string generalSettingEN = "General Setting";
        private string generalSettingJP = "一般設定";
        private string referenceSettingEN = "Reference Setting";
        private string referenceSettingJP = "参照設定";

        #endregion

        private enum Language { Japanese, English };
        private enum Platform { PC, Quest };
        private void OnEnable()
        {
            _uec2 = target as VRChatEventCalendar;

            _initialLoad = serializedObject.FindProperty(nameof(VRChatEventCalendar.initialLoad));
            _affectedLight = serializedObject.FindProperty(nameof(VRChatEventCalendar.affectedLight));
            _intensity = serializedObject.FindProperty(nameof(VRChatEventCalendar.intensity));

            _cuid = serializedObject.FindProperty(nameof(VRChatEventCalendar.cuid));
            _litCalendarMat = serializedObject.FindProperty(nameof(VRChatEventCalendar.litCalendarMat));
            _unlitCalendarMat = serializedObject.FindProperty(nameof(VRChatEventCalendar.unlitCalendarMat));
            _videoPlayer = serializedObject.FindProperty(nameof(VRChatEventCalendar.videoPlayer));
            _colorCodeReader = serializedObject.FindProperty(nameof(VRChatEventCalendar.colorCodeReader));
            _colliders = serializedObject.FindProperty(nameof(VRChatEventCalendar.colliders));
            _s_touchColliders = serializedObject.FindProperty(nameof(VRChatEventCalendar.s_touchColliders));
            _s_touchDetectors = serializedObject.FindProperty(nameof(VRChatEventCalendar.s_touchDetectors));
            _s_syncScrollCalendar = serializedObject.FindProperty(nameof(VRChatEventCalendar.s_syncScrollCalendar));
            _codeReaderCam = serializedObject.FindProperty(nameof(VRChatEventCalendar.codeReaderCam));
            _canvas = serializedObject.FindProperty(nameof(VRChatEventCalendar.canvas));
            _display = serializedObject.FindProperty(nameof(VRChatEventCalendar.display));
            _isMain = serializedObject.FindProperty(nameof(VRChatEventCalendar.isMain));
            _param = serializedObject.FindProperty(nameof(VRChatEventCalendar.param));
            _platform = serializedObject.FindProperty(nameof(VRChatEventCalendar.platform));

            headerTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidHeader), typeof(Texture)) as Texture;
            githubTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidGitHubIcon), typeof(Texture)) as Texture;
            twitterTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidTwitterIcon), typeof(Texture)) as Texture;
            discordTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidDiscordIcon), typeof(Texture)) as Texture;
            textures = new Texture[] { githubTexture, discordTexture, twitterTexture };
            guids = new string[] { guidGitHubIcon, guidDiscordIcon, guidTwitterIcon };
            urls = new string[] { urlGitHub, urlDiscord, urlTwitter };
        }
        private void DrawLogoTexture(string guid, Texture texture)
        {
            if (texture == null)
                texture = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guid));

            if (texture == null)
                return;

            float w = EditorGUIUtility.currentViewWidth;
            Rect rect = new Rect
            {
                width = w - 40f
            };
            rect.height = rect.width / 4f;
            Rect rect2 = GUILayoutUtility.GetRect(rect.width, rect.height);
            rect.x = ((EditorGUIUtility.currentViewWidth - rect.width) * 0.5f) - 4.0f;
            rect.y = rect2.y;
            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill);
        }
        private void DrawSocialLinks(Texture[] textures, string[] guids, string[] urls)
        {
            float space = 10f;
            float padding = 10f;
            float size = 40f;

            float w = size * textures.Length + space * (textures.Length - 1);
            Rect socialAreaRect = new Rect
            {
                width = w,
                height = size + padding * 2
            };
            Rect sar = GUILayoutUtility.GetRect(socialAreaRect.width, socialAreaRect.height);
            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] == null)
                    textures[i] = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guids[i]));

                if (textures[i] != null)
                {
                    Rect rect = new Rect
                    {
                        width = size,
                        height = size,
                        x = ((EditorGUIUtility.currentViewWidth - w) * 0.5f) - 4.0f + size * i + space * i,
                        y = sar.y + padding
                    };
                    GUI.DrawTexture(rect, textures[i], ScaleMode.StretchToFill);
                    if (GUI.Button(rect, "", new GUIStyle()))
                    {
                        Application.OpenURL(urls[i]);
                    }
                }
            }

        }
        private void DrawUdonSettings(Object obj)
        {
            fold_udonSetting = ShurikenUI.Foldout("Udon Setting", fold_udonSetting);
            if (fold_udonSetting)
            {
                EditorGUI.indentLevel++;
                if (UdonSharpGUI.DrawConvertToUdonBehaviourButton(obj) || UdonSharpGUI.DrawProgramSource(obj))
                    return;
                UdonSharpGUI.DrawSyncSettings(obj);
                UdonSharpGUI.DrawUtilities(obj);
                //UdonSharpGUI.DrawUILine();
                EditorGUI.indentLevel--;
            }
        }
        public override void OnInspectorGUI()
        {
            if (_uec2 == null) _uec2 = target as VRChatEventCalendar;

            _uec2.EditorUpdate();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawLogoTexture(guidHeader, headerTexture);

            ShurikenUI.Header(lang == Language.Japanese ? generalSettingJP : generalSettingEN);
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField(_platform.intValue == 0 ? lang == Language.Japanese ? "PC版カレンダー" : "PC Calendar" : lang == Language.Japanese ? "Quest版カレンダー" : "Quest Calendar", EditorStyles.boldLabel);
            lang = (Language)EditorGUILayout.EnumPopup("Language", lang);
            EditorGUILayout.PropertyField(_initialLoad, new GUIContent(lang == Language.Japanese ? "初回読み込み遅延" : "Itinital Load Delay"), true);
            EditorGUILayout.PropertyField(_affectedLight, new GUIContent(lang == Language.Japanese ? "光源処理" : "Use Lighting"), true);
            _intensity.floatValue = EditorGUILayout.Slider(lang == Language.Japanese ? "輝度" : "Intensity", _intensity.floatValue, -1f, 1f);
            EditorGUI.indentLevel--;
            fold_InternalReference = ShurikenUI.Foldout(lang == Language.Japanese ? referenceSettingJP : referenceSettingEN, fold_InternalReference);
            if (fold_InternalReference)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Calendar Settings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_litCalendarMat, true);
                EditorGUILayout.PropertyField(_unlitCalendarMat, true);
                EditorGUILayout.PropertyField(_param, true);
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Self Objects", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_s_touchColliders, true);
                EditorGUILayout.PropertyField(_s_touchDetectors, true);
                EditorGUILayout.PropertyField(_s_syncScrollCalendar, true);
                EditorGUILayout.PropertyField(_codeReaderCam, true);
                EditorGUILayout.PropertyField(_canvas, true);
                EditorGUILayout.PropertyField(_display, true);
                EditorGUILayout.PropertyField(_videoPlayer, true);
                EditorGUILayout.PropertyField(_colorCodeReader, true);
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Multiple Setting", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_cuid, true);
                EditorGUILayout.PropertyField(_isMain, true);
                EditorGUILayout.PropertyField(_colliders, true);
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }

            DrawUdonSettings(target);

            EditorGUILayout.Space();
            DrawSocialLinks(textures, guids, urls);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif