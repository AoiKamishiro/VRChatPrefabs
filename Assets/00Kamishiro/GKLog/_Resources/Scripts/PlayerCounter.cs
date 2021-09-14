/*
* Copyright (c) 2021 AoiKamishiro
* 
* This code is provided under the MIT license.
* 
*/

using TMPro;
using UdonSharp;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Kamishiro.VRChatUDON.GKLog
{
    public class PlayerCounter : UdonSharpBehaviour
    {
        public Text _uiText;
        public TextMeshProUGUI _tmText;
        public string suffix = "";
        private int players = 0;

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            players += 1;
            if (_uiText != null)
                _uiText.text = players.ToString().PadLeft(2, '0') + suffix;
            if (_tmText != null)
                _tmText.text = players.ToString().PadLeft(2, '0') + suffix;
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            players -= 1;
            if (_uiText != null)
                _uiText.text = players.ToString().PadLeft(2, '0') + suffix;
            if (_tmText != null)
                _tmText.text = players.ToString().PadLeft(2, '0') + suffix;
        }
    }
}