/*
* Copyright (c) 2021 AoiKamishiro
* 
* This code is provided under the MIT license.
* 
*/

using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Kamishiro.VRChatUDON.GKLog
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LogObject : UdonSharpBehaviour
    {
        public PlayerLogSystem playerLogSystem;
        public Text _unityText;
        public TextMeshProUGUI _tmText;
        public GameObject _textGameObject;
        public LayoutElement _layoutElement;
        private string[] joinFormat;
        private string[] leftFormat;
        private string timeFormat;
        private TimeSpan timeSpan;
        private string patternPlayer;
        private string patternTime;

        public void Init()
        {
            joinFormat = playerLogSystem.joinFormats;
            leftFormat = playerLogSystem.leftFormats;
            timeFormat = playerLogSystem.timeFormat;
            timeSpan = playerLogSystem.timeSpan;
            patternPlayer = playerLogSystem.patternPlayer;
            patternTime = playerLogSystem.patternTime;
        }
        public void SetText(string content)
        {
            string logText;
            bool activate;

            if (content.Length < 1)
            {
                logText = "";
                activate = false;
            }
            else if (content.StartsWith("1") || content.StartsWith("2"))
            {
                logText = "";
                string time = (DateTime.Parse(content.Substring(1, 19)) + timeSpan).ToString(timeFormat);
                string[] format = content.StartsWith("1") ? joinFormat : leftFormat;
                foreach (string tex in format)
                {
                    if (tex == patternTime) logText += time;
                    else if (tex == patternPlayer) logText += content.Substring(20);
                    else logText += tex;
                }
                activate = true;
            }
            else
            {
                logText = content.Substring(1);
                activate = true;
            }

            if (_unityText != null) _unityText.text = logText;
            if (_tmText != null) _tmText.text = logText;
            if (_textGameObject != null) if (!_textGameObject.activeSelf) _textGameObject.SetActive(activate);
            if (_layoutElement != null) if (!_layoutElement.enabled) _layoutElement.enabled = activate;
        }
        public void Reset()
        {
            SetText("");
        }
    }
}