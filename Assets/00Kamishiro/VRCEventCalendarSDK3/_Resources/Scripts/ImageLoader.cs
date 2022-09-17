/*
* Copyright (c) 2021 AoiKamishiro
* 
* This code is provided under the MIT license.
* 
*/

#if VRC_SDK_VRCSDK3

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;

namespace Kamishiro.VRChatUDON.VRChatEventCalendar.SDK3
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ImageLoader : UdonSharpBehaviour
    {
        public Button _LoadButton;
        public BaseVRCVideoPlayer _VideoPlayer;
        public RenderTexture _RenderTexture;
        public VRCUrl _VRCUrl = VRCUrl.Empty;
        public VRChatEventCalendar manager;
        public bool useInitDelayOnManager = true;
        public float initializeDelay = 10.0f;
        private const float _LoadCT = 6.0f;
        private bool _IsInCT = false;
        private const float RetryDeley = 2.0f;
        private const string InitializeError = "[<color=red>VRC Scroll Event Calendar</color>] VideoPlayer Initialization Failed. Please Check the UdonBehaviour.";
        private const string VdeoPlayerError = "[<color=red>VRC Scroll Event Calendar</color>] VideoPlayer Error. Retry to Play. Error Reason: ";

        private void Start()
        {
            _LoadButton.interactable = false;

            if (!Utilities.IsValid(_LoadButton) || !Utilities.IsValid(_VideoPlayer) || !Utilities.IsValid(_RenderTexture))
            {
                Debug.LogError(InitializeError);
                this.enabled = false;
                return;
            }

            if (useInitDelayOnManager) initializeDelay = (float)manager.GetProgramVariable(nameof(VRChatEventCalendar.initialLoad));

            SendCustomEventDelayedSeconds(nameof(_PlayVideo), initializeDelay);
        }
        public override void OnVideoError(VideoError videoError)
        {
            SendCustomEventDelayedSeconds(nameof(_PlayVideo), RetryDeley);
            Debug.LogError(VdeoPlayerError + videoError.ToString());
        }
        public override void OnVideoEnd()
        {
            manager.SendCustomEvent(nameof(VRChatEventCalendar._OnCalendarReady));
        }
        public void _PlayVideo()
        {
            _RenderTexture.Release();
            _VideoPlayer.PlayURL(_VRCUrl);
            SendCustomEvent(nameof(_EnterCT));
            SendCustomEventDelayedSeconds(nameof(_ExitCT), _LoadCT);
        }
        public void _EnterCT()
        {
            _IsInCT = true;
            _LoadButton.interactable = false;
        }
        public void _ExitCT()
        {
            _IsInCT = false;
            _LoadButton.interactable = true;
        }
        public void _ReLoad()
        {
            if (!_IsInCT) SendCustomEvent(nameof(_PlayVideo));
        }
    }
}
#endif