﻿/*
* Copyright (c) 2021 AoiKamishiro
* 
* This code is provided under the MIT license.
* 
*/

using TMPro;
using UdonSharp;
using UnityEngine.UI;
using VRC.SDKBase;

namespace online.kamishiro.vrc.udon.gklog
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerCounter : UdonSharpBehaviour
    {
        public Text _uiText;
        public TextMeshProUGUI _tmText;
        public string suffix = "";
        private VRCPlayerApi[] playerApis = new VRCPlayerApi[80];

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(CountPlayer), 10);
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(CountPlayer), 10);
        }
        public void CountPlayer()
        {
            int playerCount = VRCPlayerApi.GetPlayerCount();

            if (_uiText != null) _uiText.text = playerCount.ToString().PadLeft(2, '0') + suffix;
            if (_tmText != null) _tmText.text = playerCount.ToString().PadLeft(2, '0') + suffix;
        }
    }
}