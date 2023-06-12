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

namespace online.kamishiro.vrc.udon.gklog
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LogObject : UdonSharpBehaviour
    {
        public Text _unityText;
        public TextMeshProUGUI _tmText;
        public GameObject _textGameObject;
        public LayoutElement _layoutElement;
        private string[] joinFormat;
        private string[] leftFormat;
        private string timeFormat;
        private TimeSpan _timeSpan;
        private string patternPlayer;
        private string patternTime;
        private string _content;
        internal string Text
        {
            set
            {
                _content = value;
                Apply();
            }
        }
        internal TimeSpan TimeSpan
        {
            set
            {
                _timeSpan = value;
                Apply();
            }
        }

        public void Init(PlayerLogSystem playerLogSystem)
        {
            joinFormat = playerLogSystem.joinFormats;
            leftFormat = playerLogSystem.leftFormats;
            timeFormat = playerLogSystem.timeFormat;
            _timeSpan = playerLogSystem.TimeSpan;
            patternPlayer = playerLogSystem.patternPlayer;
            patternTime = playerLogSystem.patternTime;
        }

        private void Apply()
        {
            Debug.Log("Apply");
            string logText;
            bool activate;

            if (_content.Length < 1)
            {
                logText = "";
                activate = false;
            }
            else if (_content.StartsWith("1") || _content.StartsWith("2"))
            {
                logText = "";
                string time = (DateTime.Parse(_content.Substring(1, 19)) + _timeSpan).ToString(timeFormat);
                string[] format = _content.StartsWith("1") ? joinFormat : leftFormat;
                foreach (string tex in format)
                {
                    if (tex == patternTime) logText += time;
                    else if (tex == patternPlayer) logText += _content.Substring(20);
                    else logText += tex;
                }
                activate = true;
            }
            else
            {
                logText = _content.Substring(1);
                activate = true;
            }

            if (_unityText != null) _unityText.text = logText;
            if (_tmText != null) _tmText.text = logText;
            if (_textGameObject != null) if (!_textGameObject.activeSelf) _textGameObject.SetActive(activate);
            if (_layoutElement != null) if (!_layoutElement.enabled) _layoutElement.enabled = activate;
        }
        public void Reset()
        {
            Text = string.Empty;
        }
    }
}