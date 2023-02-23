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
            CountPlayer();
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            CountPlayer();
        }
        private void CountPlayer()
        {
            int playerCount = 0;
            playerApis = VRCPlayerApi.GetPlayers(playerApis);
            foreach (VRCPlayerApi playerApi in playerApis) if (Utilities.IsValid(playerApi)) playerCount++;

            if (_uiText != null) _uiText.text = playerCount.ToString().PadLeft(2, '0') + suffix;
            if (_tmText != null) _tmText.text = playerCount.ToString().PadLeft(2, '0') + suffix;
        }
    }
}