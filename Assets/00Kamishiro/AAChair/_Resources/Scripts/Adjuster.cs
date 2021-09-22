
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kamishiro.VRChatUDON.AAChair
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Adjuster : UdonSharpBehaviour
    {
        public AutoAdjustStation aAChair;
        private void LateUpdate()
        {
            aAChair._AAChair_AdjustPosition();
        }
    }
}