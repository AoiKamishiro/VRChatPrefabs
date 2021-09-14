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
    public class HeightCalculator : UdonSharpBehaviour
    {
        public AKSwitch aKSwitch;
        private VRCPlayerApi _lp;
        public float samplengTimeSpan = 2.0f;
        public float playerHeight = 1.0f;
        public float playerHeightMin = 0.35f;
        public float playerHeightMax = 2.00f;

        private void Start()
        {
            _lp = Networking.LocalPlayer;
            if (_lp == null || aKSwitch == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if (aKSwitch.isMain)
                SendCustomEventDelayedSeconds(nameof(_GetAvatarHeight), samplengTimeSpan);
            else
                gameObject.SetActive(false);
        }
        public void _GetAvatarHeight()
        {
            Vector3 pLFoot = _lp.GetBonePosition(HumanBodyBones.LeftFoot);
            Vector3 pRFoot = _lp.GetBonePosition(HumanBodyBones.RightFoot);
            Vector3 pLLowLeg = _lp.GetBonePosition(HumanBodyBones.LeftLowerLeg);
            Vector3 pRLowLeg = _lp.GetBonePosition(HumanBodyBones.RightLowerLeg);
            Vector3 pLUpLeg = _lp.GetBonePosition(HumanBodyBones.LeftUpperLeg);
            Vector3 pRUpLeg = _lp.GetBonePosition(HumanBodyBones.RightUpperLeg);
            Vector3 pSpine = _lp.GetBonePosition(HumanBodyBones.Spine);
            Vector3 pHead = _lp.GetBonePosition(HumanBodyBones.Head);

            float lowerLegLen = Mathf.Max(Vector3.Distance(pLLowLeg, pLFoot), Vector3.Distance(pRLowLeg, pRFoot));
            float upperLegLen = Mathf.Max(Vector3.Distance(pLUpLeg, pLLowLeg), Vector3.Distance(pRUpLeg, pRLowLeg));
            float hipLen = Vector3.Distance(Vector3.LerpUnclamped(pLUpLeg, pRUpLeg, 0.5f), pSpine);
            float spineLen = Vector3.Distance(pSpine, pHead);
            float h = lowerLegLen + upperLegLen + hipLen + spineLen;
            //Debug.Log(h);
            if (h < 0.001f) h = 1.0f;
            if (Mathf.Abs(playerHeight - h) > 0.01)
            {
                if (h < playerHeightMin) h = playerHeightMin;
                if (h > playerHeightMax) h = playerHeightMax;
                playerHeight = h;
            }
            SendCustomEventDelayedSeconds(nameof(_GetAvatarHeight), samplengTimeSpan);
        }
    }
}