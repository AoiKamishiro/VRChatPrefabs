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
    public class DigitalClock : UdonSharpBehaviour
    {
        [SerializeField] public Text _uiText;
        [SerializeField] public TextMeshProUGUI _tmText;
        [SerializeField] public bool colonFlash = true;

        private void Start()
        {
            SendCustomEventDelayedSeconds(nameof(_UpdateClock), 2.0f);
        }
        public void _UpdateClock()
        {
            string text = colonFlash ? DateTime.Now.ToString($"MM/dd HH{(DateTime.Now.Second % 2 == 0 ? ":" : "<color=#0000>:</color>")}mm") : DateTime.Now.ToString("MM/dd HH:mm");
            if (_uiText != null) _uiText.text = text;
            if (_tmText != null) _tmText.text = text;
            SendCustomEventDelayedSeconds(nameof(_UpdateClock), 0.25f);
        }
    }
}