/*
 * 
 * Copyright (c) 2021 AoiKamishiro
 * 
 * This code is provided under the MIT license.
 * 
 */

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Kamishiro.VRChatUDON.AKSwitch.SampleScene
{
    public class PlayerCounter : UdonSharpBehaviour
    {
        public Text text;
        public int maxPlayer = 32;
        private int count = 0;
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            count += 1;
            text.text = count + "<size=90>人</size>/" + maxPlayer + "<size=90>人</size>";
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            count -= 1;
            text.text = count + "<size=90>人</size>/" + maxPlayer + "<size=90>人</size>";
        }
    }
}