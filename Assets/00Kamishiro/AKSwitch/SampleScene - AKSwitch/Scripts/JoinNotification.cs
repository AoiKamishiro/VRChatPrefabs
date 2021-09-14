/*
 * 
 * Copyright (c) 2021 AoiKamishiro
 * 
 * This code is provided under the MIT license.
 * 
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kamishiro.VRChatUDON.AKSwitch.SampleScene
{
    public class JoinNotification : UdonSharpBehaviour
    {
        public AudioClip audioClip;
        public AudioSource audioSource;

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (audioClip == null || audioSource == null)
            {
                return;
            }

            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }
}