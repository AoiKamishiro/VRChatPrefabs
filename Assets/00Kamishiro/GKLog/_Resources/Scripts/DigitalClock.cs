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
    public class DigitalClock : UdonSharpBehaviour
    {
        [SerializeField] public Text _uiText;
        [SerializeField] public TextMeshProUGUI _tmText;
        [SerializeField] public bool colonFlash = true;
        private float _updateCounter = 0f;

        private void Update()
        {
            if (_uiText != null)
            {
                if (_updateCounter > 0.2f)
                {
                    _uiText.text = colonFlash ? DateTime.Now.ToString($"MM/dd HH{(DateTime.Now.Second % 2 == 0 ? ":" : "<color=#0000>:</color>")}mm") : DateTime.Now.ToString("MM/dd HH:mm");
                    _updateCounter = 0f;
                }
                else
                {
                    _updateCounter += Time.deltaTime;
                }
            }
            if (_tmText != null)
            {
                if (_updateCounter > 0.2f)
                {
                    _tmText.text = colonFlash ? DateTime.Now.ToString($"MM/dd HH{(DateTime.Now.Second % 2 == 0 ? ":" : "<color=#0000>:</color>")}mm") : DateTime.Now.ToString("MM/dd HH:mm");
                    _updateCounter = 0f;
                }
                else
                {
                    _updateCounter += Time.deltaTime;
                }
            }
        }
    }
}