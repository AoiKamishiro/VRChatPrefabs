/*
 * 
 * Copyright (c) 2021 AoiKamishiro
 * 
 * This code is provided under the MIT license.
 * 
 */

using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Kamishiro.VRChatUDON.AKSwitch.SampleScene
{
    public class Clock : UdonSharpBehaviour
    {
        public Text text;

        private void Update()
        {
            text.text = DateTime.Now.ToString(DateTime.Now.Second % 2 == 0 ? "HH:mm" : "HH mm") + "\n" + "<size=100>" + DateTime.Now.ToString("yyyy/MM/dd") + "</size>";
        }
    }
}