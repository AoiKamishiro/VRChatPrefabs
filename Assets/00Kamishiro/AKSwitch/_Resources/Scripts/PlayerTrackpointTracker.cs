/*
 * 
 * Copyright (c) 2021 AoiKamishiro
 * 
 * This code is provided under the MIT license.
 * 
 */

using UdonSharp;
using VRC.SDKBase;

namespace Kamishiro.VRChatUDON.AKSwitch
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerTrackpointTracker : UdonSharpBehaviour
    {
        public VRCPlayerApi.TrackingDataType trackType = VRCPlayerApi.TrackingDataType.LeftHand;
        public VRC_Pickup.PickupHand hapticHand = VRC_Pickup.PickupHand.None;
        private VRCPlayerApi _lp;

        private void Start()
        {
            _lp = Networking.LocalPlayer;
        }
        private void Update()
        {
            transform.position = _lp.GetTrackingData(trackType).position;
        }
        public void _PlayHaptics()
        {
            _lp.PlayHapticEventInHand(hapticHand, 1.0f, 0.5f, 0.5f);
        }

    }
}