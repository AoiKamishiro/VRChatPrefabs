/*
 * 
 * Copyright (c) 2021 AoiKamishiro
 * 
 * This code is provided under the MIT license.
 * 
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UnityEditorInternal;
using UdonSharpEditor;
using Object = UnityEngine.Object;
#endif

namespace Kamishiro.VRChatUDON.AKSwitch
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AKSwitch : UdonSharpBehaviour
    {
        #region General Seting
        public string descriptionText = "";
        public bool useRaycastModeOnly = false;
        public bool useAutomaticHeight = true;
        public bool isGlobal = false;
        public AudioClip audioClip;
        public int maxStep = 5;
        public Sprite iconSprite;
        public Color iconColor = new Color(1, 1, 1, 1);
        #endregion
        #region Object Setting
        public GameObject[] toggleGroup1;
        public GameObject[] toggleGroup2;
        public GameObject[] toggleGroup3;
        public GameObject[] toggleGroup4;
        public GameObject[] toggleGroup5;
        public Color toggleColor1 = new Color(0, 0, 0, 1);
        public Color toggleColor2 = new Color(0, 0, 0, 1);
        public Color toggleColor3 = new Color(0, 0, 0, 1);
        public Color toggleColor4 = new Color(0, 0, 0, 1);
        public Color toggleColor5 = new Color(0, 0, 0, 1);
        #endregion
        #region Internal Reference
        public bool isMainSwitch;
        public PhysicalInteract physicalInteract;
        public RaycastInteract raycastInteract;
        public UdonBehaviour raycastInteractU;
        public AudioSource audioSource;
        public Renderer iconRenderer;
        public Material material;
        public int materialIndex = 0;
        public string materialParam = "_Emission";
        public Transform touchColliders;
        public Transform heightCalculator;
        public Transform s_touchColliders;
        public Transform s_heightCalculator;
        #endregion
        #region External Objects
        public UdonBehaviour otherUdon;
        public string method;
        public Animator animator;
        public string param;
        #endregion
        #region Internal Calculation
        [UdonSynced] private int syncState = 0;
        private int _localState = 0;
        private int _initialState = 0;
        public int State
        {
            get
            {
                return _localState + 1;
            }
            set
            {
                if (value < 1) value = 1;
                if (value > maxStep) value = maxStep + 1;

                if (value != _localState + 1)
                {
                    _prevState = _localState;
                    _localState = value - 1;
                    if (isGlobal) syncState = _localState;

                    SendCustomEvent(nameof(_SendExternalEvent));
                    SendCustomEvent(nameof(_SetMaterialParameter));
                    SendCustomEvent(nameof(_ToggleObjects));
                    SendCustomEvent(nameof(_PlayAudio));
                }
            }
        }
        private VRCPlayerApi _lp;
        private GameObject[][] _stepObjects;
        private Color[] _stepColors;
        private bool _materialEnable = false;
        private bool _audioEnable = false;
        private bool _exUdonEnable = false;
        private bool _exAnimatorEnable = false;
        private int _prevState = 0;
        private UdonBehaviour _heightAdjustor;
        private Vector3 _basePoint = Vector3.zero;
        private float _avatarHeight = 1.0f;
        private const float _timespan = 0.1f;
        #endregion

        private void Start()
        {
            _lp = Networking.LocalPlayer;

            if (_lp == null)
            {
                enabled = false;
                return;
            }

            _stepObjects = new GameObject[][] { toggleGroup1, toggleGroup2, toggleGroup3, toggleGroup4, toggleGroup5 };
            _stepColors = new Color[] { toggleColor1, toggleColor2, toggleColor3, toggleColor4, toggleColor5 };
            _materialEnable = iconRenderer != null && iconRenderer.materials.Length > materialIndex && iconRenderer.materials[materialIndex] != null;
            _audioEnable = audioSource != null && audioClip != null;
            _exUdonEnable = otherUdon != null && !string.IsNullOrWhiteSpace(method);
            _exAnimatorEnable = animator != null && !string.IsNullOrWhiteSpace(param);

            if (_materialEnable) iconRenderer.material = iconRenderer.material;

            if (_audioEnable) audioSource.clip = audioClip;

            if (useAutomaticHeight)
            {
                _basePoint = transform.position - transform.up.normalized;
                _heightAdjustor = (UdonBehaviour)heightCalculator.GetComponent(typeof(UdonBehaviour));
                SendCustomEventDelayedSeconds(nameof(_SetPositionByAvatarHeight), _timespan);
            }
            _localState = _initialState;
            syncState = _initialState;
            _prevState = _initialState;
            SendCustomEvent(nameof(_SendExternalEvent));
            SendCustomEvent(nameof(_SetMaterialParameter));
            SendCustomEvent(nameof(_ToggleObjects));
            SendCustomEventDelayedSeconds(nameof(_SetSwitchMode), 5.0f);
        }
        public void _SetSwitchMode()
        {
            if (useRaycastModeOnly || !_lp.IsUserInVR()) SendCustomEvent(nameof(_RaycastMode));
            else SendCustomEvent(nameof(_PhysicalMode));
        }
        public void _SetPositionByAvatarHeight()
        {
            float calcedHeight = (float)_heightAdjustor.GetProgramVariable(nameof(HeightCalculator.playerHeight));

            if (_avatarHeight != calcedHeight)
            {
                _avatarHeight = calcedHeight;
                transform.position = _basePoint + transform.up.normalized * calcedHeight;
            }
            SendCustomEventDelayedSeconds(nameof(_SetPositionByAvatarHeight), _timespan);
        }
        public void _PhysicalMode()
        {
            physicalInteract.SendCustomEvent(nameof(PhysicalInteract._EnablePhysicalInteraction));
            raycastInteract.SendCustomEvent(nameof(RaycastInteract._DisbleRaycastInteraction));
        }
        public void _RaycastMode()
        {
            physicalInteract.SendCustomEvent(nameof(PhysicalInteract._DisblePhysicalInteraction));
            raycastInteract.SendCustomEvent(nameof(RaycastInteract._EnableRaycastInteraction));
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!isGlobal || player != _lp) return;

            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(RequestedState));
        }
        public void RequestedState()
        {
            RequestSerialization();
        }
        public void _SendExternalEvent()
        {
            if (_exUdonEnable) otherUdon.SendCustomEvent(method);

            if (_exAnimatorEnable) animator.SetInteger(param, _localState + 1);
        }
        public void _SetMaterialParameter()
        {
            if (_materialEnable) iconRenderer.materials[materialIndex].SetColor(materialParam, _stepColors[_localState]);
        }
        public void _PlayAudio()
        {
            if (_audioEnable) audioSource.Play();
        }
        public void _ToggleObjects()
        {
            foreach (GameObject item in _stepObjects[_localState])
            {
                if (item != null) item.SetActive(true);
            }

            for (int i = 0; i < maxStep; i++)
            {
                if (i == _localState) continue;

                foreach (GameObject item in _stepObjects[i])
                {
                    if (item == null) continue;
                    bool isExclude = false;
                    foreach (GameObject exclude in _stepObjects[_localState]) if (item == exclude) isExclude = true;
                    if (!isExclude) item.SetActive(false);
                }
            }
        }
        public override void OnDeserialization()
        {
            if (_localState == syncState) return;

            if (!isGlobal) return;

            _localState = syncState;
            SendCustomEvent(nameof(_SendExternalEvent));
            SendCustomEvent(nameof(_SetMaterialParameter));
            SendCustomEvent(nameof(_ToggleObjects));
            SendCustomEvent(nameof(_PlayAudio));
        }
        public void OnInteracted()
        {
            SendCustomEvent(nameof(_LocalOnInteracted));

            if (!isGlobal) return;

            SendCustomEvent(nameof(_GlobalOnInteracted));
        }
        public void _GlobalOnInteracted()
        {
            if (Networking.IsOwner(_lp, gameObject))
            {
                syncState = _localState;
                RequestSerialization();
            }
            else
            {
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(OnInteracted));
            }
        }
        public void _LocalOnInteracted()
        {
            _prevState = _localState;
            _localState = (_localState + 1) <= (maxStep - 1) ? (_localState + 1) : 0;
            SendCustomEvent(nameof(_SendExternalEvent));
            SendCustomEvent(nameof(_SetMaterialParameter));
            SendCustomEvent(nameof(_ToggleObjects));
            SendCustomEvent(nameof(_PlayAudio));
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public void EditorUpdate()
        {
            SetDescriptionText();
            StepCountCheck();
            SetMaterial();
            SetSpriteColor();
            SetSpriteImage();
        }
        private void SetMaterial()
        {
            if (iconRenderer == null) return;

            Material[] materials = iconRenderer.sharedMaterials;
            if (materials == null || materials.Length == 0 || materials.Length < materialIndex) return;

            if (materials[materialIndex] == material) return;

            materials[materialIndex] = material;
            iconRenderer.sharedMaterials = materials;
            Undo.RecordObject(iconRenderer, "AKSwitch - Set Renderer Matrial");
            EditorUtility.SetDirty(iconRenderer);
        }
        private void SetDescriptionText()
        {
            if (raycastInteractU == null) return;

            if (string.IsNullOrWhiteSpace(descriptionText)) return;

            if (raycastInteractU.interactText == descriptionText) return;

            raycastInteractU.interactText = descriptionText;
            Undo.RecordObject(raycastInteractU, "AKSwitch - Set Description Text");
            EditorUtility.SetDirty(raycastInteractU);
        }
        private void StepCountCheck()
        {
            bool clumped = false;
            if (maxStep < 2)
            {
                maxStep = 2;
                clumped = true;
            }
            if (maxStep > 5)
            {
                maxStep = 5;
                clumped = true;
            }
            if (clumped)
            {
                Undo.RecordObject(this, "AKSwitch - Set MaxStep");
                EditorUtility.SetDirty(this);
            }

            clumped = false;

            if (_initialState < 0)
            {
                _initialState = 0;
                clumped = true;
            }
            if (_initialState > maxStep - 1)
            {
                _initialState = maxStep - 1;
                clumped = true;
            }
            if (clumped)
            {
                Undo.RecordObject(this, "AKSwitch - Set InitialState");
                EditorUtility.SetDirty(this);
            }
        }
        private void SetSpriteImage()
        {
            if (iconRenderer == null || iconRenderer.GetType() != typeof(SpriteRenderer)) return;

            SpriteRenderer spriteRenderer = iconRenderer as SpriteRenderer;
            if (spriteRenderer.sprite == iconSprite) return;

            spriteRenderer.sprite = iconSprite;
            Undo.RecordObject(iconRenderer, "AKSwitch - Set Sprite Image");
            EditorUtility.SetDirty(iconRenderer);
        }
        private void SetSpriteColor()
        {
            if (iconRenderer == null || iconRenderer.GetType() != typeof(SpriteRenderer))
                return;

            SpriteRenderer spriteRenderer = iconRenderer as SpriteRenderer;
            if (spriteRenderer.color == iconColor) return;

            spriteRenderer.color = iconColor;
            Undo.RecordObject(iconRenderer, "AKSwitch - Set Sprite Color");
            EditorUtility.SetDirty(iconRenderer);
        }
#endif
    }

#if UNITY_EDITOR && !COMPILER_UDONSHARP

    [CustomEditor(typeof(AKSwitch))]
    public class AKSwitchEditor : Editor
    {
        private enum Language { English, Japanese }

        #region General Seting
        private SerializedProperty _descriptionText;
        private SerializedProperty _isGlobal;
        private SerializedProperty _useAutomaticHeight;
        private SerializedProperty _audioClip;
        private SerializedProperty _maxStep;
        private SerializedProperty _raycastOnly;
        private SerializedProperty _iconSprite;
        private SerializedProperty _iconColor;
        #endregion
        #region Object Setting
        private SerializedProperty _toggleGroup1;
        private SerializedProperty _toggleGroup2;
        private SerializedProperty _toggleGroup3;
        private SerializedProperty _toggleGroup4;
        private SerializedProperty _toggleGroup5;
        private SerializedProperty _toggleColor1;
        private SerializedProperty _toggleColor2;
        private SerializedProperty _toggleColor3;
        private SerializedProperty _toggleColor4;
        private SerializedProperty _toggleColor5;
        #endregion
        #region Internal Reference
        private SerializedProperty _isMain;
        private SerializedProperty _physicalInteract;
        private SerializedProperty _raycastInteract;
        private SerializedProperty _raycastInteractU;
        private SerializedProperty _audioSource;
        private SerializedProperty _meshRenderer;
        private SerializedProperty _material;
        private SerializedProperty _materialIndex;
        private SerializedProperty _materialParam;
        private SerializedProperty _touchColliders;
        private SerializedProperty _heightAdjustor;
        private SerializedProperty _s_touchColliders;
        private SerializedProperty _s_heightAdjustor;
        #endregion
        #region External Objects
        private SerializedProperty _otherUdon;
        private SerializedProperty _method;
        private SerializedProperty _animator;
        private SerializedProperty _param;
        #endregion
        #region Editor Objects
        private Texture headerTexture;
        private Texture githubTexture;
        private Texture twitterTexture;
        private Texture discordTexture;
        private const string guidHeader = "a2f8de10fa81c494984fb77a82a2343a";
        private const string guidDiscordIcon = "e0bf419c10883b84eba1425790d17619";
        private const string guidGitHubIcon = "3f838e910032985419f0f9a57a2983c7";
        private const string guidTwitterIcon = "7c450ce60285c124e921379f9547d8e1";
        private const string urlTwitter = "https://twitter.com/aoi3192";
        private const string urlDiscord = "https://discord.gg/8muNKrzaSK";
        private const string urlGitHub = "https://github.com/AoiKamishiro/VRC_UdonPrefabs";
        private Texture[] textures;
        private string[] guids;
        private string[] urls;
        private AKSwitch _akSwitch;
        private static bool fold_switchSetting = false;
        private static bool fold_objectReference = false;
        private static bool fold_externalEventObjects = false;
        private static bool fold_udonSetting = false;
        private static Language lang = Language.Japanese;
        #endregion
        #region UI Text
        private readonly GUIContent descriptionTextLabel_en = new GUIContent("Description Text", "This also applies to interaction text.");
        private readonly GUIContent descriptionTextLabel_jp = new GUIContent("説明文", "インタラクトのテキストにも表示されます。");
        private readonly GUIContent isGlobalLabel_en = new GUIContent("Is Global", "Sets the local or global operation.");
        private readonly GUIContent isGlobalLabel_jp = new GUIContent("グローバル", "スイッチのローカル・グローバル動作を設定します。");
        private readonly GUIContent useAutoHeight_en = new GUIContent("Use Auto Height", "The height of the switch is automatically adjusted according to the player's height.");
        private readonly GUIContent useAutoHeight_jp = new GUIContent("自動位置調整", "スイッチの高さを身長に応じて自動で調節します。");
        private readonly GUIContent audioClip_en = new GUIContent("Audio Clip", "Sound effects when using the switch.");
        private readonly GUIContent audioClip_jp = new GUIContent("スイッチ音源", "スイッチを使用した時の効果音");
        private readonly GUIContent raycastOnly_en = new GUIContent("Raycast Mode Only", "It does not switch to physical touch, even when the user is in VR.");
        private readonly GUIContent raycastOnly_jp = new GUIContent("物理スイッチを使用しない", "ユーザーがVRの時でも、物理タッチへの切り替えを行いません。");
        private readonly GUIContent iconSprite_en = new GUIContent("Icon Sprite", "Image of switch icon");
        private readonly GUIContent iconSprite_jp = new GUIContent("アイコン画像(Sprite)", "スイッチの画像");
        private readonly GUIContent iconColor_en = new GUIContent("Icon Base Color", "Base Color of Switch Icon");
        private readonly GUIContent iconColor_jp = new GUIContent("ベースカラー", "スイッチの色");
        private readonly GUIContent stepObjectsLabel0_en = new GUIContent("Group 1");
        private readonly GUIContent stepObjectsLabel0_jp = new GUIContent("グループ 1");
        private readonly GUIContent stepObjectsLabel1_en = new GUIContent("Group 2");
        private readonly GUIContent stepObjectsLabel1_jp = new GUIContent("グループ 2");
        private readonly GUIContent stepObjectsLabel2_en = new GUIContent("Group 3");
        private readonly GUIContent stepObjectsLabel2_jp = new GUIContent("グループ 3");
        private readonly GUIContent stepObjectsLabel3_en = new GUIContent("Group 4");
        private readonly GUIContent stepObjectsLabel3_jp = new GUIContent("グループ 4");
        private readonly GUIContent stepObjectsLabel4_en = new GUIContent("Group 5");
        private readonly GUIContent stepObjectsLabel4_jp = new GUIContent("グループ 5");
        private readonly GUIContent maxStepLevel_en = new GUIContent("Levels (2-5)");
        private readonly GUIContent maxStepLevel_jp = new GUIContent("切り替え数 (2-5)");
        private readonly GUIContent label_emColor_en = new GUIContent("Emission Color");
        private readonly GUIContent label_emColor_jp = new GUIContent("発光色");
        private readonly GUIContent label_toggleObjects_en = new GUIContent("Toggle Objects");
        private readonly GUIContent label_toggleObjects_jp = new GUIContent("対象オブジェクト");
        private const string langageLabel_en = "Language";
        private const string langageLabel_jp = "言語";
        private const string generalSetting_en = "General Settings";
        private const string generalSetting_jp = "全般設定";
        private const string swSetting_en = "Switch Setting";
        private const string swSetting_jp = "スイッチ設定";
        private const string internalSetting_en = "Internal Setting";
        private const string internalSetting_jp = "内部参照設定";
        private const string externalSetting_en = "External Event Object";
        private const string externalSetting_jp = "外部連動設定";
        private const string udonSetting_en = "Udon Setting";
        private const string udonSetting_jp = "Udon 設定";
        private const string swSetupHelpbox_en_12 = "The active group of game objects will be switched in the order 1 -> 2 -> ";
        private const string swSetupHelpbox_en_3 = "3 -> ";
        private const string swSetupHelpbox_en_4 = "4 -> ";
        private const string swSetupHelpbox_en_5 = "5 -> ";
        private const string swSetupHelpbox_en_1 = "1.";
        private const string swSetupHelpbox_jp_12 = "アクティブなゲームオブジェクト群は 1→2→";
        private const string swSetupHelpbox_jp_3 = "3→";
        private const string swSetupHelpbox_jp_4 = "4→";
        private const string swSetupHelpbox_jp_5 = "5→";
        private const string swSetupHelpbox_jp_1 = "1 の順に切り替わります。";
        private const string externalUdonHelp_en = "Sends an event when the button is pressed.";
        private const string externalUdonHelp_jp = "ボタンが押されたときにイベントを送信します。";
        private const string externalAnimator_en = "Sets the current state to the parameter when button is pressed. (1-5)";
        private const string externalAnimator_jp = "ボタンを押したときの現在の状態をパラメータに設定します。(1-5)";
        #endregion

        private ReorderableList objList1;
        private ReorderableList objList2;
        private ReorderableList objList3;
        private ReorderableList objList4;
        private ReorderableList objList5;

        private void OnEnable()
        {
            _descriptionText = serializedObject.FindProperty(nameof(AKSwitch.descriptionText));
            _isGlobal = serializedObject.FindProperty(nameof(AKSwitch.isGlobal));
            _useAutomaticHeight = serializedObject.FindProperty(nameof(AKSwitch.useAutomaticHeight));
            _audioClip = serializedObject.FindProperty(nameof(AKSwitch.audioClip));
            _maxStep = serializedObject.FindProperty(nameof(AKSwitch.maxStep));
            _raycastOnly = serializedObject.FindProperty(nameof(AKSwitch.useRaycastModeOnly));
            _iconColor = serializedObject.FindProperty(nameof(AKSwitch.iconColor));
            _iconSprite = serializedObject.FindProperty(nameof(AKSwitch.iconSprite));
            _isMain = serializedObject.FindProperty(nameof(AKSwitch.isMainSwitch));
            _heightAdjustor = serializedObject.FindProperty(nameof(AKSwitch.heightCalculator));
            _touchColliders = serializedObject.FindProperty(nameof(AKSwitch.touchColliders));
            _s_heightAdjustor = serializedObject.FindProperty(nameof(AKSwitch.s_heightCalculator));
            _s_touchColliders = serializedObject.FindProperty(nameof(AKSwitch.s_touchColliders));
            _toggleGroup1 = serializedObject.FindProperty(nameof(AKSwitch.toggleGroup1));
            _toggleGroup2 = serializedObject.FindProperty(nameof(AKSwitch.toggleGroup2));
            _toggleGroup3 = serializedObject.FindProperty(nameof(AKSwitch.toggleGroup3));
            _toggleGroup4 = serializedObject.FindProperty(nameof(AKSwitch.toggleGroup4));
            _toggleGroup5 = serializedObject.FindProperty(nameof(AKSwitch.toggleGroup5));
            _toggleColor1 = serializedObject.FindProperty(nameof(AKSwitch.toggleColor1));
            _toggleColor2 = serializedObject.FindProperty(nameof(AKSwitch.toggleColor2));
            _toggleColor3 = serializedObject.FindProperty(nameof(AKSwitch.toggleColor3));
            _toggleColor4 = serializedObject.FindProperty(nameof(AKSwitch.toggleColor4));
            _toggleColor5 = serializedObject.FindProperty(nameof(AKSwitch.toggleColor5));
            _physicalInteract = serializedObject.FindProperty(nameof(AKSwitch.physicalInteract));
            _raycastInteract = serializedObject.FindProperty(nameof(AKSwitch.raycastInteract));
            _raycastInteractU = serializedObject.FindProperty(nameof(AKSwitch.raycastInteractU));
            _audioSource = serializedObject.FindProperty(nameof(AKSwitch.audioSource));
            _meshRenderer = serializedObject.FindProperty(nameof(AKSwitch.iconRenderer));
            _material = serializedObject.FindProperty(nameof(AKSwitch.material));
            _materialIndex = serializedObject.FindProperty(nameof(AKSwitch.materialIndex));
            _materialParam = serializedObject.FindProperty(nameof(AKSwitch.materialParam));
            _otherUdon = serializedObject.FindProperty(nameof(AKSwitch.otherUdon));
            _method = serializedObject.FindProperty(nameof(AKSwitch.method));
            _animator = serializedObject.FindProperty(nameof(AKSwitch.animator));
            _param = serializedObject.FindProperty(nameof(AKSwitch.param));

            objList1 = new ReorderableList(serializedObject, _toggleGroup1, true, true, true, true);
            objList1.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect testFieldRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(testFieldRect, objList1.serializedProperty.GetArrayElementAtIndex(index), label: new GUIContent());
            };
            objList1.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, lang == Language.Japanese ? label_toggleObjects_jp : label_toggleObjects_en); };

            objList2 = new ReorderableList(serializedObject, _toggleGroup2, true, true, true, true);
            objList2.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect testFieldRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(testFieldRect, objList2.serializedProperty.GetArrayElementAtIndex(index), label: new GUIContent());
            };
            objList2.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, lang == Language.Japanese ? label_toggleObjects_jp : label_toggleObjects_en); };

            objList3 = new ReorderableList(serializedObject, _toggleGroup3, true, true, true, true);
            objList3.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect testFieldRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(testFieldRect, objList3.serializedProperty.GetArrayElementAtIndex(index), label: new GUIContent());
            };
            objList3.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, lang == Language.Japanese ? label_toggleObjects_jp : label_toggleObjects_en); };

            objList4 = new ReorderableList(serializedObject, _toggleGroup4, true, true, true, true);
            objList4.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect testFieldRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(testFieldRect, objList4.serializedProperty.GetArrayElementAtIndex(index), label: new GUIContent());
            };
            objList4.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, lang == Language.Japanese ? label_toggleObjects_jp : label_toggleObjects_en); };

            objList5 = new ReorderableList(serializedObject, _toggleGroup5, true, true, true, true);
            objList5.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect testFieldRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(testFieldRect, objList5.serializedProperty.GetArrayElementAtIndex(index), label: new GUIContent());
            };
            objList5.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, lang == Language.Japanese ? label_toggleObjects_jp : label_toggleObjects_en); };

            headerTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidHeader), typeof(Texture)) as Texture;
            githubTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidGitHubIcon), typeof(Texture)) as Texture;
            twitterTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidTwitterIcon), typeof(Texture)) as Texture;
            discordTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidDiscordIcon), typeof(Texture)) as Texture;
            textures = new Texture[] { githubTexture, discordTexture, twitterTexture };
            guids = new string[] { guidGitHubIcon, guidDiscordIcon, guidTwitterIcon };
            urls = new string[] { urlGitHub, urlDiscord, urlTwitter };
        }
        private void DrawReordableList(ReorderableList reorderableList, int indentLevel)
        {
            int level = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            GUIStyle style = new GUIStyle()
            {
                margin = new RectOffset(14 * indentLevel, 0, 8, 0),
                font = new GUIStyle(EditorStyles.boldLabel).font
            };
            Rect rect = GUILayoutUtility.GetRect(100, reorderableList.GetHeight(), style);
            reorderableList.DoList(rect);
            EditorGUI.indentLevel = level;
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
            fold_udonSetting = ShurikenUI.Foldout(lang == Language.Japanese ? udonSetting_jp : udonSetting_en, fold_udonSetting);
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
            if (_akSwitch == null)
                _akSwitch = target as AKSwitch;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawLogoTexture(guidHeader, headerTexture);

            ShurikenUI.Header(lang == Language.Japanese ? generalSetting_jp : generalSetting_en);
            EditorGUI.indentLevel++;

            lang = (Language)EditorGUILayout.EnumPopup(lang == Language.Japanese ? langageLabel_jp : langageLabel_en, lang);
            EditorGUILayout.PropertyField(_descriptionText, lang == Language.Japanese ? descriptionTextLabel_jp : descriptionTextLabel_en, true);
            EditorGUILayout.PropertyField(_isGlobal, lang == Language.Japanese ? isGlobalLabel_jp : isGlobalLabel_en, true);
            EditorGUILayout.PropertyField(_useAutomaticHeight, lang == Language.Japanese ? useAutoHeight_jp : useAutoHeight_en, true);
            EditorGUILayout.PropertyField(_audioClip, lang == Language.Japanese ? audioClip_jp : audioClip_en, true);
            EditorGUILayout.PropertyField(_raycastOnly, lang == Language.Japanese ? raycastOnly_jp : raycastOnly_en, true);
            EditorGUILayout.PropertyField(_iconSprite, lang == Language.Japanese ? iconSprite_jp : iconSprite_en, true);
            EditorGUILayout.PropertyField(_iconColor, lang == Language.Japanese ? iconColor_jp : iconColor_en, true);
            EditorGUI.indentLevel--;


            fold_switchSetting = ShurikenUI.Foldout(lang == Language.Japanese ? swSetting_jp : swSetting_en, fold_switchSetting);
            if (fold_switchSetting)
            {
                string swhSetupHelpbox_jp = swSetupHelpbox_jp_12;
                if (_maxStep.intValue > 2) swhSetupHelpbox_jp += swSetupHelpbox_jp_3;
                if (_maxStep.intValue > 3) swhSetupHelpbox_jp += swSetupHelpbox_jp_4;
                if (_maxStep.intValue > 4) swhSetupHelpbox_jp += swSetupHelpbox_jp_5;
                swhSetupHelpbox_jp += swSetupHelpbox_jp_1;

                string swSetupHelpbox_en = swSetupHelpbox_en_12;
                if (_maxStep.intValue > 2) swSetupHelpbox_en += swSetupHelpbox_en_3;
                if (_maxStep.intValue > 3) swSetupHelpbox_en += swSetupHelpbox_en_4;
                if (_maxStep.intValue > 4) swSetupHelpbox_en += swSetupHelpbox_en_5;
                swSetupHelpbox_en += swSetupHelpbox_en_1;

                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(lang == Language.Japanese ? swhSetupHelpbox_jp : swSetupHelpbox_en, MessageType.Info);
                EditorGUILayout.PropertyField(_maxStep, lang == Language.Japanese ? maxStepLevel_jp : maxStepLevel_en, true);
                if (_maxStep.intValue > 0)
                {
                    ShurikenUI.Header(lang == Language.Japanese ? stepObjectsLabel0_jp : stepObjectsLabel0_en, EditorGUI.indentLevel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_toggleColor1, lang == Language.Japanese ? label_emColor_jp : label_emColor_en, true);
                    DrawReordableList(objList1, 3);
                    EditorGUI.indentLevel--;
                }
                if (_maxStep.intValue > 1)
                {
                    ShurikenUI.Header(lang == Language.Japanese ? stepObjectsLabel1_jp : stepObjectsLabel1_en, EditorGUI.indentLevel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_toggleColor2, lang == Language.Japanese ? label_emColor_jp : label_emColor_en, true);
                    DrawReordableList(objList2, 3);
                    EditorGUI.indentLevel--;
                }
                if (_maxStep.intValue > 2)
                {
                    ShurikenUI.Header(lang == Language.Japanese ? stepObjectsLabel2_jp : stepObjectsLabel2_en, EditorGUI.indentLevel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_toggleColor3, lang == Language.Japanese ? label_emColor_jp : label_emColor_en, true);
                    DrawReordableList(objList3, 3);
                    EditorGUI.indentLevel--;
                }
                if (_maxStep.intValue > 3)
                {
                    ShurikenUI.Header(lang == Language.Japanese ? stepObjectsLabel3_jp : stepObjectsLabel3_en, EditorGUI.indentLevel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_toggleColor4, lang == Language.Japanese ? label_emColor_jp : label_emColor_en, true);
                    DrawReordableList(objList4, 3);
                    EditorGUI.indentLevel--;
                }
                if (_maxStep.intValue > 4)
                {
                    ShurikenUI.Header(lang == Language.Japanese ? stepObjectsLabel4_jp : stepObjectsLabel4_en, EditorGUI.indentLevel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_toggleColor5, lang == Language.Japanese ? label_emColor_jp : label_emColor_en, true);
                    DrawReordableList(objList5, 3);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            UdonSharpGUI.DrawUILine();

            fold_objectReference = ShurikenUI.Foldout(lang == Language.Japanese ? internalSetting_jp : internalSetting_en, fold_objectReference);
            if (fold_objectReference)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Renderer Setting", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_material, true);
                EditorGUILayout.PropertyField(_materialIndex, true);
                EditorGUILayout.PropertyField(_materialParam, true);
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Object Reference", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(_physicalInteract, true);
                    EditorGUILayout.PropertyField(_raycastInteract, true);
                    EditorGUILayout.PropertyField(_raycastInteractU, true);
                    EditorGUILayout.PropertyField(_audioSource, true);
                    EditorGUILayout.PropertyField(_meshRenderer, true);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Multi Switch Setting (Auto Setup)", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(_isMain, true);
                    EditorGUILayout.PropertyField(_heightAdjustor, true);
                    EditorGUILayout.PropertyField(_touchColliders, true);
                    EditorGUILayout.PropertyField(_s_heightAdjustor, true);
                    EditorGUILayout.PropertyField(_s_touchColliders, true);
                }
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }

            fold_externalEventObjects = ShurikenUI.Foldout(lang == Language.Japanese ? externalSetting_jp : externalSetting_en, fold_externalEventObjects);
            if (fold_externalEventObjects)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(lang == Language.Japanese ? externalUdonHelp_jp : externalUdonHelp_en, MessageType.Info);
                EditorGUILayout.PropertyField(_otherUdon, new GUIContent("UdonBehaviour"), true);
                EditorGUILayout.PropertyField(_method, new GUIContent("Method"), true);
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(lang == Language.Japanese ? externalAnimator_jp : externalAnimator_en, MessageType.Info);
                EditorGUILayout.PropertyField(_animator, new GUIContent("Animator"), true);
                EditorGUILayout.PropertyField(_param, new GUIContent("Param (int)"), true);
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            DrawUdonSettings(target);

            EditorGUILayout.Space();
            DrawSocialLinks(textures, guids, urls);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
            _akSwitch.EditorUpdate();
        }
    }

#endif
}