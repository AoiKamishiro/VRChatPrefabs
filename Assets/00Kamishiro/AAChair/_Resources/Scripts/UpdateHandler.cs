
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kamishiro.VRChatUDON.AAChair
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class UpdateHandler : UdonSharpBehaviour
    {
        public AutoAdjustStation aAChair;
        private void LateUpdate()
        {
            aAChair._AAChair_AdjustPosition();
        }
    }
}