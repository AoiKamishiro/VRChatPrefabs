/*
* Copyright (c) 2021 AoiKamishiro
* 
* This code is provided under the MIT license.
* 
*/

#if VRC_SDK_VRCSDK3

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kamishiro.VRChatUDON.VRChatEventCalendar.SDK3
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AutoReload : UdonSharpBehaviour
    {
        public VRChatEventCalendar vRChatEventCalendar;
        public Animator animator;
        public void _Reload()
        {
            vRChatEventCalendar._ReLoadVideo();
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player == vRChatEventCalendar._lp) return;
            animator.SetTrigger("Reload");
        }
    }
}
#endif