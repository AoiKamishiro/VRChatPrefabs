
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UdonSharpEditor;
#endif

namespace Kamishiro.VRChatUDON.AAChair
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class AutoAdjustStation : UdonSharpBehaviour
    {
        private VRCPlayerApi _tp;
        private VRCPlayerApi _lp;
        private const float _zError = 0.05f;//ふくらはぎ！！
        private const float _yError = 0.05f;//ふともも！！
        private const float _maxValue = 2.0f;
        public Animator anim;
        public UpdateHandler adjuster;
        public StationHandler station;
        public Transform adjustPint;
        public Transform enterPoint;
        private float avatarHeight = 0.0f;
        private const string animParam_adjust = "adjust";
        private const string animParam_stop = "stop";
        private const string animParam_recaluc = "recaluc";

        private void OnEnable()
        {
            _lp = Networking.LocalPlayer;
        }
        public void OnInteract()
        {
            station._AAChair_UseAttachedStation(_lp);
        }
        public void _AAChair_ReCalulateAvatarHeight()
        {
            if (_tp == null) return;

            float newHeight = GetAvatarHeight(_tp);
            if (Mathf.Abs(newHeight - avatarHeight) >= 0.2)
            {
                avatarHeight = newHeight;
                anim.SetTrigger(animParam_adjust);
            }
            anim.SetTrigger(animParam_recaluc);
        }
        public void _AAChair_OnStationEntered(VRCPlayerApi player)
        {
            _tp = player;
            avatarHeight = GetAvatarHeight(player);
            anim.ResetTrigger(animParam_stop);
            anim.SetTrigger(animParam_adjust);
            anim.SetTrigger(animParam_recaluc);
        }
        public void _AAChair_OnStationExited(VRCPlayerApi player)
        {
            _tp = null;
            anim.SetTrigger(animParam_stop);
        }
        public void _AAChair_AdjustPosition()
        {
            enterPoint.localPosition = GetAdjustedPosition(_tp);
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
        private SerializedProperty _anim;
        private AutoAdjustStation _aaStation;
        private SerializedProperty _adjuster;
        private SerializedProperty _station;
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
            _anim = serializedObject.FindProperty(nameof(AutoAdjustStation.anim));
            _adjuster = serializedObject.FindProperty(nameof(AutoAdjustStation.adjuster));
            _station = serializedObject.FindProperty(nameof(AutoAdjustStation.station));
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
                EditorGUILayout.PropertyField(_anim, true);
                EditorGUILayout.PropertyField(_adjuster, true);
                EditorGUILayout.PropertyField(_station, true);
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