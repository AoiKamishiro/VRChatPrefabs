using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace online.kamishiro.vrc.udon.aachair
{
    [DefaultExecutionOrder(0), UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class StationHandler : UdonSharpBehaviour
    {
        private AutoAdjustStation _autoAdjustStation;
        private AutoAdjustStation AutoAdjustStation
        {
            get
            {
                if (!_autoAdjustStation) _autoAdjustStation = GetComponentInParent<AutoAdjustStation>();
                return _autoAdjustStation;
            }
        }

        public override void Interact() => AutoAdjustStation.OnInteract();
        public void _AAChair_UseAttachedStation(VRCPlayerApi player) => player.UseAttachedStation();
        public override void OnStationEntered(VRCPlayerApi player) => AutoAdjustStation._AAChair_OnStationEntered(player);
        public override void OnStationExited(VRCPlayerApi player) => AutoAdjustStation._AAChair_OnStationExited(player);
    }
}