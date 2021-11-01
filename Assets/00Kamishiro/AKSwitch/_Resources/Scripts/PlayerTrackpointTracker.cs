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
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerTrackpointTracker : UdonSharpBehaviour
    {
        public TrackerActivator trackerActivator;
        public VRCPlayerApi.TrackingDataType trackType = VRCPlayerApi.TrackingDataType.LeftHand;
        public VRC_Pickup.PickupHand hapticHand = VRC_Pickup.PickupHand.None;
        private VRCPlayerApi _lp;

        private void OnEnable()
        {
            _lp = trackerActivator.localPlayer;
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