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

namespace Kamishiro.VRChatUDON.AKSwitch
{
    public class TrackerActivator : UdonSharpBehaviour
    {
        public AKSwitch aKSwitch;
        public PlayerBoneTracker playerBoneTracker;
        public PlayerTrackpointTracker playerTrackpointTracker;
        public Collider touchCollider;
        public VRCPlayerApi localPlayer;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            SendCustomEventDelayedSeconds(nameof(_DetectUserMode), 5.0f);
        }
        public void _DetectUserMode()
        {
            if (localPlayer == null || !localPlayer.IsUserInVR())
            {
                gameObject.SetActive(false);
                return;
            }

            if (aKSwitch == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if (aKSwitch.isMain)
            {
                if (playerBoneTracker != null) playerBoneTracker.enabled = true;
                if (playerTrackpointTracker != null) playerTrackpointTracker.enabled = true;
                if (touchCollider != null) touchCollider.enabled = true;
            }
            else
            {
                gameObject.SetActive(false);
                return;
            }
        }
    }
}