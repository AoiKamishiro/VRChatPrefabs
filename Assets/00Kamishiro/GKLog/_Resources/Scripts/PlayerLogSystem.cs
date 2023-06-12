/*
* Copyright (c) 2021 AoiKamishiro
* 
* This code is provided under the MIT license.
* 
*/

using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#endif

namespace online.kamishiro.vrc.udon.gklog
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [DefaultExecutionOrder(-1)]
    public class PlayerLogSystem : UdonSharpBehaviour
    {
        [UdonSynced(UdonSyncMode.None)] private string _logStrings = "\"0[Start] GKLog V3.1.3 by AoiKamishiro\"";//\"1 2100/12/31 11:59:59 AoiKamishiro\",//UTC
        //[UdonSynced(UdonSyncMode.None)] private bool isSending = false;
        //[UdonSynced(UdonSyncMode.None)] private int dataIndex = 0;
        public string timeFormat = "[MM/dd HH:mm]";
        public string joinFormat = "[<color=#00ff00>Join</color>]{time} {player}";
        public string leftFormat = "[<color=#ff0000>Left</color>]{time} {player}";
        public string[] joinFormats;
        public string[] leftFormats;
        public readonly string patternTime = "{time}";
        public readonly string patternPlayer = "{player}";
        private bool isSyncStandby = true;


        public ScrollRect _scrollRect;
        private ScrollRect ScrollRect
        {
            get
            {
                if (!_scrollRect) _scrollRect = GetComponentInChildren<ScrollRect>();
                return _scrollRect;
            }
        }
        private TimeSpan _timeSpan = TimeSpan.Zero;
        internal TimeSpan TimeSpan
        {
            get
            {
                if (_timeSpan == TimeSpan.Zero) _timeSpan = DateTime.Now - DateTime.UtcNow;
                return _timeSpan;
            }
        }
        private VRCPlayerApi _localPlayer;
        internal VRCPlayerApi LocalPlayer
        {
            get
            {
                if (!Utilities.IsValid(_localPlayer)) _localPlayer = Networking.LocalPlayer;
                return _localPlayer;

            }
        }

        private LogObject[] _logObjects;
        internal LogObject[] LogObjects
        {
            get
            {
                if (_logObjects == null) _logObjects = GetComponentsInChildren<LogObject>(true);
                return _logObjects;
            }
        }

        private void Start()
        {
            foreach (LogObject logObject in LogObjects) if (logObject) { logObject.Init(this); }
            SendCustomEventDelayedSeconds(nameof(ReInit), 5.0f);
        }
        public void ReInit()
        {
            _timeSpan = DateTime.Now - DateTime.UtcNow;
            foreach (LogObject logObject in LogObjects) if (logObject) { logObject.TimeSpan = _timeSpan; }
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (LocalPlayer == player) SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(RequestSyncVariable));

            if (!Networking.IsOwner(LocalPlayer, gameObject)) return;

            string currentLog = GetJoinLog(player.displayName);
            string[] logs = Add(PerseLog(_logStrings), currentLog);
            logs = Clamp(logs, LogObjects.Length);
            DispLog(logs);
            _logStrings = CreateLogStrings(logs);
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(LocalPlayer, gameObject)) return;

            string currentLog = GetLeftLog(player.displayName);
            string[] logs = Add(PerseLog(_logStrings), currentLog);
            logs = Clamp(logs, LogObjects.Length);
            DispLog(logs);
            _logStrings = CreateLogStrings(logs);
            RequestSyncVariable();
        }
        public override void OnDeserialization()
        {
            DispLog(PerseLog(_logStrings));
        }
        private void DispLog(string[] logs)
        {
            for (int i = 0; i < LogObjects.Length; i++)
            {
                if (!LogObjects[i]) continue;
                if (i >= logs.Length)
                {
                    LogObjects[i].Reset();
                    continue;
                }
                if (logs[i] == null)
                {
                    LogObjects[i].Reset();
                    continue;
                }

                LogObjects[i].Text = logs[i];
                LogObjects[i].transform.SetAsFirstSibling();
            }
            ScrollRect.CalculateLayoutInputVertical();
        }
        private string CreateLogStrings(string[] vs)
        {
            string logStrings = "";
            for (int i = 0; i < vs.Length; i++)
            {
                logStrings += "\"" + vs[i] + "\"";

                if (i != vs.Length - 1)
                    logStrings += ",";
            }
            return logStrings;
        }
        private string[] PerseLog(string logStrings)
        {
            string[] splitedLogs = logStrings.Split(',');
            string[] logs = new string[] { };
            for (int i = 0; i < splitedLogs.Length; i++)
            {
                string log = splitedLogs[i];

                if (!splitedLogs[i].StartsWith("\""))
                    continue;

                while (true)
                {
                    if (splitedLogs[i].EndsWith("\""))
                        break;

                    if (i + 1 >= splitedLogs.Length)
                        break;

                    if (splitedLogs[i + 1].StartsWith("\""))
                        break;

                    log += "," + splitedLogs[i + 1];
                    i++;
                }

                if (log.StartsWith("\""))
                    log = log.Substring(1);

                if (log.EndsWith("\""))
                    log = log.Substring(0, log.Length - 1);

                logs = Add(logs, log);
            }
            return logs;
        }
        private string GetJoinLog(string playerDisplayName)
        {
            return "1" + DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss") + playerDisplayName;
        }
        private string GetLeftLog(string playerDisplayName)
        {
            return "2" + DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss") + playerDisplayName;
        }
        private string[] Add(string[] array, string str)
        {
            string[] newArray = new string[array.Length + 1];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }
            newArray[array.Length] = str;
            return newArray;
        }
        private string[] Clamp(string[] array, int maxLength)
        {
            if (array.Length <= maxLength)
                return array;

            string[] newArray = new string[maxLength];
            int substruct = array.Length - maxLength;
            for (int i = 0; i < maxLength; i++)
            {
                newArray[i] = array[i + substruct];
            }
            return newArray;
        }
        public void RequestSyncVariable()
        {
            if (!isSyncStandby) return;
            SendCustomEvent(nameof(_DoSyncVariable));
        }
        public void _DoSyncVariable()
        {
            bool isClogged = Networking.IsClogged;

            if (!isClogged) RequestSerialization();
            else SendCustomEventDelayedFrames(nameof(_DoSyncVariable), UnityEngine.Random.Range(30, 60) * 10);

            isSyncStandby = !isClogged;
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        internal void CheckFormats()
        {
            List<string> joinStrs = new List<string>();
            string[] joinSplitedByTime = Regex.Split(joinFormat, patternTime);

            for (int i = 0; i < joinSplitedByTime.Length; i++)
            {
                string[] splitedByPlayer = Regex.Split(joinSplitedByTime[i], patternPlayer);
                for (int j = 0; j < splitedByPlayer.Length; j++)
                {
                    joinStrs.Add(splitedByPlayer[j]);
                    if (j + 1 < splitedByPlayer.Length)
                        joinStrs.Add(patternPlayer);
                }
                if (i + 1 < joinSplitedByTime.Length)
                    joinStrs.Add(patternTime);
            }
            if (joinFormats.Length != joinStrs.Count)
            {
                joinFormats = joinStrs.ToArray();
            }
            else
            {
                for (int i = 0; i < joinFormats.Length; i++)
                {
                    if (joinFormats[i] == joinStrs[i])
                        continue;

                    joinFormats = joinStrs.ToArray();
                    break;
                }
            }


            List<string> leftStrs = new List<string>();
            string[] leftSplitedByTime = Regex.Split(leftFormat, patternTime);

            for (int i = 0; i < leftSplitedByTime.Length; i++)
            {
                string[] splitedByPlayer = Regex.Split(leftSplitedByTime[i], patternPlayer);
                for (int j = 0; j < splitedByPlayer.Length; j++)
                {
                    leftStrs.Add(splitedByPlayer[j]);
                    if (j + 1 < splitedByPlayer.Length)
                        leftStrs.Add(patternPlayer);
                }
                if (i + 1 < leftSplitedByTime.Length)
                    leftStrs.Add(patternTime);
            }
            if (leftFormats.Length != leftStrs.Count)
            {
                leftFormats = leftStrs.ToArray();
            }
            else
            {
                for (int i = 0; i < leftFormats.Length; i++)
                {
                    if (leftFormats[i] == leftStrs[i])
                        continue;

                    leftFormats = leftStrs.ToArray();
                    break;
                }
            }
        }
        internal void SetMainCam()
        {
            if (string.IsNullOrWhiteSpace(gameObject.scene.path))
                return;

            Canvas _canvas = GetComponentInChildren<Canvas>();

            if (_canvas == null)
                return;

            if (_canvas.worldCamera != null)
                return;

            Camera mCam = GameObject.FindGameObjectsWithTag("MainCamera")[0].GetComponent<Camera>();
            if (mCam == null)
                return;
            _canvas.worldCamera = mCam;
            Undo.RecordObject(_canvas, "GKLog - Setup Canvas Maincam");
            EditorUtility.SetDirty(_canvas);
        }
#endif
    }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [CustomEditor(typeof(PlayerLogSystem))]
    internal class PlayerLogEditor : Editor
    {
        private enum Language { English, Japanese }

        #region LogSetting
        private SerializedProperty _timeFormat;
        private SerializedProperty _joinFormat;
        private SerializedProperty _leftFormat;
        #endregion

        private string _playerName = "Player Name";
        private PlayerLogSystem _playerLog;
        private readonly GUIStyle _style = new GUIStyle();
        private Texture headerTexture;
        private Texture githubTexture;
        private Texture twitterTexture;
        private Texture discordTexture;
        private readonly GUIContent _timeFormatContent = new GUIContent("Time Format", "DateTime.ToString() format. Milliseconds are not supported.");
        private readonly GUIContent _joinFormatContent = new GUIContent("Join Format", "The format of the join log.");
        private readonly GUIContent _leftFormatContent = new GUIContent("Left Format", "The format of the left log.");
        private const string guidHeader = "fb2e1bbf3fb1a09499def05d00ef9b02";
        private const string guidDiscordIcon = "16a02f4eb32f24442b3546d2a4415d8c";
        private const string guidGitHubIcon = "2a7e32c558737884686dca0c5e89d78a";
        private const string guidTwitterIcon = "6e15d2cffc23d6e469f30daec548de5e";
        private const string urlTwitter = "https://twitter.com/aoi3192";
        private const string urlDiscord = "https://discord.gg/8muNKrzaSK";
        private const string urlGitHub = "https://github.com/AoiKamishiro/VRChatPrefabs";
        private Texture[] textures;
        private string[] guids;
        private string[] urls;


        private void OnEnable()
        {
            _style.richText = true;
            _timeFormat = serializedObject.FindProperty(nameof(PlayerLogSystem.timeFormat));
            _joinFormat = serializedObject.FindProperty(nameof(PlayerLogSystem.joinFormat));
            _leftFormat = serializedObject.FindProperty(nameof(PlayerLogSystem.leftFormat));
            headerTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidHeader), typeof(Texture)) as Texture;
            githubTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidGitHubIcon), typeof(Texture)) as Texture;
            twitterTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidTwitterIcon), typeof(Texture)) as Texture;
            discordTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guidDiscordIcon), typeof(Texture)) as Texture;
            textures = new Texture[] { githubTexture, discordTexture, twitterTexture };
            guids = new string[] { guidGitHubIcon, guidDiscordIcon, guidTwitterIcon };
            urls = new string[] { urlGitHub, urlDiscord, urlTwitter };
        }
        private string GetJoinLog(string playerName)
        {
            string logText = "";
            string time = DateTime.Now.ToString(_playerLog.timeFormat);
            foreach (string tex in _playerLog.joinFormats)
            {
                if (tex == _playerLog.patternTime)
                {
                    logText += time;
                    continue;
                }
                else if (tex == _playerLog.patternPlayer)
                {
                    logText += playerName;
                    continue;
                }
                else
                {
                    logText += tex;
                }
            }
            return logText;
        }
        private string GetLeftLog(string playerName)
        {
            string logText = "";
            string time = DateTime.Now.ToString(_playerLog.timeFormat);
            foreach (string tex in _playerLog.leftFormats)
            {
                if (tex == _playerLog.patternTime)
                {
                    logText += time;
                    continue;
                }
                else if (tex == _playerLog.patternPlayer)
                {
                    logText += playerName;
                    continue;
                }
                else
                {
                    logText += tex;
                }
            }
            return logText;
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
        public override void OnInspectorGUI()
        {
            if (_playerLog == null)
                _playerLog = target as PlayerLogSystem;

            EditorGUILayout.Space();
            DrawLogoTexture(guidHeader, headerTexture);

            EditorGUILayout.LabelField("Udon Setting", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (UdonSharpGUI.DrawProgramSource(target)) return;

            UdonSharpGUI.DrawSyncSettings(target);
            UdonSharpGUI.DrawUtilities(target);
            UdonSharpGUI.DrawUILine();
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Text Setting", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_timeFormat, _timeFormatContent);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_joinFormat, _joinFormatContent);
            EditorGUILayout.PropertyField(_leftFormat, _leftFormatContent);
            EditorGUILayout.HelpBox("{time}, {player} will be replaced with the time and player name at runtime.", MessageType.Info);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Display Sample", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Sample PlayerName");
                _playerName = EditorGUILayout.TextField(_playerName);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Join Log");
                EditorGUILayout.LabelField(GetJoinLog(_playerName), _style);
                EditorGUILayout.LabelField("Left Log");
                EditorGUILayout.LabelField(GetLeftLog(_playerName), _style);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            DrawSocialLinks(textures, guids, urls);

            serializedObject.ApplyModifiedProperties();

            _playerLog.CheckFormats();
        }
    }
#endif
}