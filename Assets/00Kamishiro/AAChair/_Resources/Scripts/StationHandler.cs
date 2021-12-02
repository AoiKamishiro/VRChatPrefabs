
using UdonSharp;
using VRC.SDKBase;

namespace Kamishiro.VRChatUDON.AAChair
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class StationHandler : UdonSharpBehaviour
    {
        public AutoAdjustStation aAChair;

        public override void Interact()
        {
            aAChair.OnInteract();
        }
        public void _AAChair_UseAttachedStation(VRCPlayerApi player)
        {
            player.UseAttachedStation();
        }
        public override void OnStationEntered(VRCPlayerApi player)
        {
            aAChair._AAChair_OnStationEntered(player);
        }
        public override void OnStationExited(VRCPlayerApi player)
        {
            aAChair._AAChair_OnStationExited(player);
        }
    }
}