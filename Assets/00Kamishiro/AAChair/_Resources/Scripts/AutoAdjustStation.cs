
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UdonSharpEditor;
#endif

namespace Kamishiro.VRChatUDON.AAChair
{
    public class AutoAdjustStation : UdonSharpBehaviour
    {
        [UdonSynced] private Vector3 position = Vector3.zero;
        private Quaternion rotation = Quaternion.identity;
        private VRCPlayerApi _lp;
        public Animator animator;
        public BoxCollider boxCollider;
        private bool _usingStation = false;
        private const float _zError = 0.05f;//ふくらはぎ！！
        private const float _yError = 0.05f;//ふともも！！
        private const float _maxValue = 2.0f;
        public Transform adjustPint;
        public Transform enterPoint;
        private float avatarHeight = 0.0f;
        private bool _adjustPosition = false;
        private const string animatorParam = "adjustPosition";

        private void Start()
        {
            _lp = Networking.LocalPlayer;
        }

        private void LateUpdate()
        {
            if (_adjustPosition)
                SendCustomEvent(nameof(AdjustPosition));
        }
        public override void Interact()
        {
            _lp.UseAttachedStation();
        }
        public override void OnStationEntered(VRCPlayerApi player)
        {
            if (_lp != player)
                return;

            _usingStation = true;
            Networking.SetOwner(_lp, gameObject);
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(DisableCollider));
            SendCustomEvent(nameof(SetAnimatorTrue));
        }
        public override void OnStationExited(VRCPlayerApi player)
        {
            _usingStation = false;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(EnableCollider));
            SendCustomEvent(nameof(SetAnimatorFalse));
        }
        public void Sync()
        {
            RequestSerialization();
        }
        public void SetAnimatorTrue()
        {
            animator.SetBool(animatorParam, true);
        }
        public void SetAnimatorFalse()
        {
            animator.SetBool(animatorParam, false);
        }
        public void StartAdjustPosition()
        {
            _adjustPosition = true;
        }
        public void FinishAdjustPosition()
        {
            _adjustPosition = false;
        }
        public override void OnDeserialization()
        {
            SendCustomEvent(nameof(SetPosition));
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(StationInUseCheck));
        }
        public void StationInUseCheck()
        {
            if (!_usingStation)
                return;

            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(DisableCollider));
            RequestSerialization();
        }

        public void AdjustPosition()
        {
            if (!_usingStation)
                return;

            position = GetAdjustedPosition();
            SendCustomEvent(nameof(SetPosition));
        }
        public Vector3 GetAdjustedPosition()
        {
            Vector3 scale = transform.lossyScale;
            float maxy = _maxValue / scale.y;
            float maxz = _maxValue / scale.z;
            avatarHeight = GetAvatarHeight();
            Vector3 lflp = transform.InverseTransformPoint(_lp.GetBonePosition(HumanBodyBones.LeftFoot));
            Vector3 rflp = transform.InverseTransformPoint(_lp.GetBonePosition(HumanBodyBones.RightFoot));
            Vector3 llllp = transform.InverseTransformPoint(_lp.GetBonePosition(HumanBodyBones.LeftLowerLeg));
            Vector3 rlllp = transform.InverseTransformPoint(_lp.GetBonePosition(HumanBodyBones.RightLowerLeg));
            Vector3 lullp = transform.InverseTransformPoint(_lp.GetBonePosition(HumanBodyBones.LeftUpperLeg));
            Vector3 rullp = transform.InverseTransformPoint(_lp.GetBonePosition(HumanBodyBones.RightUpperLeg));
            float ydis = enterPoint.localPosition.y + adjustPint.localPosition.y;
            float zdis = enterPoint.localPosition.z + adjustPint.localPosition.z;

            float[] yDistances = new float[] { llllp.y, rlllp.y, lullp.y, rullp.y };
            float[] zDistances = new float[] { lflp.z, rflp.z, llllp.z, rlllp.z };

            float lposy = ydis - Mathf.Min(yDistances) + _yError * avatarHeight;
            float lposz = zdis - Mathf.Min(zDistances) + _zError * avatarHeight;

            return new Vector3(0, Mathf.Clamp(lposy, -1 * maxy, maxy), Mathf.Clamp(lposz, -1 * maxz, maxz));
        }
        public void SetPosition()
        {
            enterPoint.localPosition = position;
            enterPoint.localRotation = rotation;
        }
        public void EnableCollider()
        {
            boxCollider.enabled = true;
        }
        public void DisableCollider()
        {
            boxCollider.enabled = false;
        }
        public float GetAvatarHeight()
        {
            Vector3 pLFoot = _lp.GetBonePosition(HumanBodyBones.LeftFoot);
            Vector3 pRFoot = _lp.GetBonePosition(HumanBodyBones.RightFoot);
            Vector3 pLLowLeg = _lp.GetBonePosition(HumanBodyBones.LeftLowerLeg);
            Vector3 pRLowLeg = _lp.GetBonePosition(HumanBodyBones.RightLowerLeg);
            Vector3 pLUpLeg = _lp.GetBonePosition(HumanBodyBones.LeftUpperLeg);
            Vector3 pRUpLeg = _lp.GetBonePosition(HumanBodyBones.RightUpperLeg);
            Vector3 pSpine = _lp.GetBonePosition(HumanBodyBones.Spine);
            Vector3 pHead = _lp.GetBonePosition(HumanBodyBones.Head);

            float lowerLegLen = Mathf.Max(Vector3.Distance(pLLowLeg, pLFoot), Vector3.Distance(pRLowLeg, pRFoot));
            float upperLegLen = Mathf.Max(Vector3.Distance(pLUpLeg, pLLowLeg), Vector3.Distance(pRUpLeg, pRLowLeg));
            float hipLen = Vector3.Distance(Vector3.LerpUnclamped(pLUpLeg, pRUpLeg, 0.5f), pSpine);
            float spineLen = Vector3.Distance(pSpine, pHead);
            return lowerLegLen + upperLegLen + hipLen + spineLen;
        }
    }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [CustomEditor(typeof(AutoAdjustStation))]
    public class AutoAdjustStationEditor : Editor
    {
        private AutoAdjustStation _aaStation;
        private SerializedProperty _animator;
        private SerializedProperty _boxCollider;
        private SerializedProperty _enterPoint;
        private SerializedProperty _referencePoint;
        private Texture headerTexture;
        private Texture githubTexture;
        private Texture twitterTexture;
        private Texture discordTexture;
        private const string guidHeader = "95543aeccf193ea4f83aa4f6b0655d6d";
        private const string guidDiscordIcon = "2b06cfa7e9f16594abd895bcbcd44bd3";
        private const string guidGitHubIcon = "a5613547026130949ae5fa362262b14b";
        private const string guidTwitterIcon = "1d5ff85f48ee02f4eb7690b9e231d567";
        private const string urlTwitter = "https://twitter.com/aoi3192";
        private const string urlDiscord = "https://discord.gg/8muNKrzaSK";
        private const string urlGitHub = "https://github.com/AoiKamishiro/VRC_UdonPrefabs";
        private Texture[] textures;
        private string[] guids;
        private string[] urls;
        private bool internal_fold = false;
        private const string adjustMessage = "When replacing the chair model, adjust the \"AdjustPoint\" object in the Prefab so that it is on the back of the knee.";
        private const string adjustMessageJp = "椅子のモデルを変更する際には、プレハブの「AdjustPoint」オブジェクトが膝の裏の位置にくるように調整してください。";

        private void OnEnable()
        {
            _animator = serializedObject.FindProperty(nameof(AutoAdjustStation.animator));
            _boxCollider = serializedObject.FindProperty(nameof(AutoAdjustStation.boxCollider));
            _enterPoint = serializedObject.FindProperty(nameof(AutoAdjustStation.enterPoint));
            _referencePoint = serializedObject.FindProperty(nameof(AutoAdjustStation.adjustPint));
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
            {
                texture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Texture)) as Texture;
            }
            if (texture != null)
            {
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
                {
                    textures[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(Texture)) as Texture;
                }

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
        private void DrawUdonSettings()
        {
            EditorGUILayout.LabelField("Udon Setting", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (UdonSharpGUI.DrawConvertToUdonBehaviourButton(target) || UdonSharpGUI.DrawProgramSource(target))
                return;
            UdonSharpGUI.DrawSyncSettings(target);
            UdonSharpGUI.DrawUtilities(target);
            UdonSharpGUI.DrawUILine();
            EditorGUI.indentLevel--;
        }
        public override void OnInspectorGUI()
        {
            if (_aaStation == null)
                _aaStation = target as AutoAdjustStation;

            EditorGUILayout.Space();
            DrawLogoTexture(guidHeader, headerTexture);
            EditorGUILayout.Space();

            DrawUdonSettings();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(adjustMessage, MessageType.Info);
            EditorGUILayout.HelpBox(adjustMessageJp, MessageType.Info);

            EditorGUILayout.Space();

            EditorGUI.indentLevel++;
            internal_fold = EditorGUILayout.Foldout(internal_fold, "Object Reference");
            if (internal_fold)
            {
                EditorGUILayout.PropertyField(_animator, true);
                EditorGUILayout.PropertyField(_boxCollider, true);
                EditorGUILayout.PropertyField(_enterPoint, true);
                EditorGUILayout.PropertyField(_referencePoint, true);
            }
            EditorGUI.indentLevel--;


            EditorGUILayout.Space();
            DrawSocialLinks(textures, guids, urls);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}