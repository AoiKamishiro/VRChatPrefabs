using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UdonSharpEditor;
#endif

namespace online.kamishiro.vrc.udon.aachair
{
    [DefaultExecutionOrder(0), UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AutoAdjustStation : UdonSharpBehaviour
    {
        private const float _zError = 0.05f;//ふくらはぎ！！
        private const float _yError = 0.05f;//ふともも！！
        private const float _maxValue = 2.0f;
        public Transform adjustPint;
        public Transform enterPoint;
        private float avatarHeight = 0.0f;
        private const string animParam_adjust = "adjust";
        private const string animParam_stop = "stop";
        private const string animParam_recaluc = "recaluc";

        private VRCPlayerApi _targetPlayer;
        private VRCPlayerApi TargetPlayer
        {
            get => _targetPlayer;
            set => _targetPlayer = value;
        }

        private VRCPlayerApi _localPlayer;
        private VRCPlayerApi LocalPlayer
        {
            get
            {
                if (!Utilities.IsValid(_localPlayer)) _localPlayer = Networking.LocalPlayer;
                return _localPlayer;
            }
        }

        private UpdateHandler _updateHandler;
        private UpdateHandler UpdateHandler
        {
            get
            {
                if (!_updateHandler) _updateHandler = GetComponentInChildren<UpdateHandler>();
                return _updateHandler;
            }
        }

        private StationHandler _stationHandler;
        private StationHandler StationHandler
        {
            get
            {
                if (!_stationHandler) _stationHandler = GetComponentInChildren<StationHandler>();
                return _stationHandler;
            }
        }

        private Animator _adjusterAnimator;
        private Animator AdjusterAnimator
        {
            get
            {
                if (!_adjusterAnimator) _adjusterAnimator = UpdateHandler.GetComponent<Animator>();
                return _adjusterAnimator;
            }
        }

        public void OnInteract() => StationHandler._AAChair_UseAttachedStation(LocalPlayer);
        public void _AAChair_ReCalulateAvatarHeight()
        {
            if (TargetPlayer == null) return;

            float newHeight = GetAvatarHeight(TargetPlayer);
            if (Mathf.Abs(newHeight - avatarHeight) >= 0.2)
            {
                avatarHeight = newHeight;
                AdjusterAnimator.SetTrigger(animParam_adjust);
            }
            AdjusterAnimator.SetTrigger(animParam_recaluc);
        }
        public void _AAChair_OnStationEntered(VRCPlayerApi player)
        {
            TargetPlayer = player;
            avatarHeight = GetAvatarHeight(player);
            AdjusterAnimator.ResetTrigger(animParam_stop);
            AdjusterAnimator.SetTrigger(animParam_adjust);
            AdjusterAnimator.SetTrigger(animParam_recaluc);
        }
        public void _AAChair_OnStationExited(VRCPlayerApi player)
        {
            TargetPlayer = null;
            AdjusterAnimator.SetTrigger(animParam_stop);
        }
        public void _AAChair_AdjustPosition()
        {
            enterPoint.localPosition = GetAdjustedPosition(TargetPlayer);
            enterPoint.localRotation = Quaternion.identity;
        }
        private Vector3 GetAdjustedPosition(VRCPlayerApi player)
        {
            if (player == null) return Vector3.zero;
            Vector3 scale = transform.lossyScale;
            float maxy = _maxValue / scale.y;
            float maxz = _maxValue / scale.z;
            Vector3 lflp = transform.InverseTransformPoint(player.GetBonePosition(HumanBodyBones.LeftFoot));
            Vector3 rflp = transform.InverseTransformPoint(player.GetBonePosition(HumanBodyBones.RightFoot));
            Vector3 llllp = transform.InverseTransformPoint(player.GetBonePosition(HumanBodyBones.LeftLowerLeg));
            Vector3 rlllp = transform.InverseTransformPoint(player.GetBonePosition(HumanBodyBones.RightLowerLeg));
            Vector3 lullp = transform.InverseTransformPoint(player.GetBonePosition(HumanBodyBones.LeftUpperLeg));
            Vector3 rullp = transform.InverseTransformPoint(player.GetBonePosition(HumanBodyBones.RightUpperLeg));
            float ydis = enterPoint.localPosition.y + adjustPint.localPosition.y;
            float zdis = enterPoint.localPosition.z + adjustPint.localPosition.z;

            float[] yDistances = new float[] { llllp.y, rlllp.y, lullp.y, rullp.y };
            float[] zDistances = new float[] { lflp.z, rflp.z, llllp.z, rlllp.z };

            float lposy = ydis - Mathf.Min(yDistances) + _yError * avatarHeight;
            float lposz = zdis - Mathf.Min(zDistances) + _zError * avatarHeight;

            return new Vector3(0, Mathf.Clamp(lposy, -1 * maxy, maxy), Mathf.Clamp(lposz, -1 * maxz, maxz));
        }
        private float GetAvatarHeight(VRCPlayerApi player)
        {
            Vector3 pLFoot = player.GetBonePosition(HumanBodyBones.LeftFoot);
            Vector3 pRFoot = player.GetBonePosition(HumanBodyBones.RightFoot);
            Vector3 pLLowLeg = player.GetBonePosition(HumanBodyBones.LeftLowerLeg);
            Vector3 pRLowLeg = player.GetBonePosition(HumanBodyBones.RightLowerLeg);
            Vector3 pLUpLeg = player.GetBonePosition(HumanBodyBones.LeftUpperLeg);
            Vector3 pRUpLeg = player.GetBonePosition(HumanBodyBones.RightUpperLeg);
            Vector3 pSpine = player.GetBonePosition(HumanBodyBones.Spine);
            Vector3 pHead = player.GetBonePosition(HumanBodyBones.Head);

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
        private const string urlGitHub = "https://github.com/AoiKamishiro/VRChatPrefabs";
        private Texture[] textures;
        private string[] guids;
        private string[] urls;
        private bool internal_fold = false;
        private const string adjustMessage = "When replacing the chair model, adjust the \"AdjustPoint\" object in the Prefab so that it is on the back of the knee.";
        private const string adjustMessageJp = "椅子のモデルを変更する際には、プレハブの「AdjustPoint」オブジェクトが膝の裏の位置にくるように調整してください。";

        private void OnEnable()
        {
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
            if (UdonSharpGUI.DrawProgramSource(target)) return;
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
            EditorGUI.BeginDisabledGroup(true);
            internal_fold = EditorGUILayout.Foldout(internal_fold, "Object Reference");
            if (internal_fold)
            {
                EditorGUILayout.PropertyField(_enterPoint, true);
                EditorGUILayout.PropertyField(_referencePoint, true);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;


            EditorGUILayout.Space();
            DrawSocialLinks(textures, guids, urls);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}