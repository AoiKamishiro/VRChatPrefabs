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
    public class LogObject : UdonSharpBehaviour
    {
        public Text _unityText;
        public TextMeshProUGUI _tmText;
        public GameObject textGameObject;
        public LayoutElement _layoutElement;
        public PlayerLogSystem _playerLog;
        public TimeSpan _timeSpam;
        public string _timeFormat;
        public string[] _joinFormat;
        public string[] _leftFormat;
        public string _patternTime;
        public string _patternPlayer;
        
        public void SetText(string content)
        {
            if (content.Length < 1)
            {
                Reset();
                return;
            }

            string logText = "";
            if (content.StartsWith("1"))
            {
                string time = (DateTime.Parse(content.Substring(1, 19)) + _timeSpam).ToString(_timeFormat);
                foreach (string tex in _joinFormat)
                {
                    if (tex == _patternTime)
                    {
                        logText += time;
                        continue;
                    }
                    else if (tex == _patternPlayer)
                    {
                        logText += content.Substring(20);
                        continue;
                    }
                    else
                    {
                        logText += tex;
                    }
                }
            }
            else if (content.StartsWith("2"))
            {
                string time = (DateTime.Parse(content.Substring(1, 19)) + _timeSpam).ToString(_timeFormat);
                foreach (string tex in _leftFormat)
                {
                    if (tex == _patternTime)
                    {
                        logText += time;
                        continue;
                    }
                    else if (tex == _patternPlayer)
                    {
                        logText += content.Substring(20);
                        continue;
                    }
                    else
                    {
                        logText += tex;
                    }
                }
            }
            else
            {
                logText = content.Substring(1);
            }

            if (_unityText != null)
                _unityText.text = logText;

            if (_tmText != null)
                _tmText.text = logText;

            if (textGameObject != null)
                if (!textGameObject.activeSelf)
                    textGameObject.SetActive(true);

            if (!_layoutElement.enabled)
                _layoutElement.enabled = true;
        }
        public void Reset()
        {
            if (_unityText != null)
                _unityText.text = "";

            if (_tmText != null)
                _tmText.text = "";

            if (textGameObject != null)
                if (textGameObject.activeSelf)
                    textGameObject.SetActive(false);

            if (_layoutElement != null)
                if (_layoutElement.enabled)
                    _layoutElement.enabled = false;
        }
    }
}