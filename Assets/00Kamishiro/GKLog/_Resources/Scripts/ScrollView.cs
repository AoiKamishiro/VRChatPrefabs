/*
* Copyright (c) 2021 AoiKamishiro
* 
* This code is provided under the MIT license.
* 
*/

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Kamishiro.VRChatUDON.GKLog
{
    public class ScrollView : UdonSharpBehaviour
    {
        public RectTransform scrollView;
        public RectTransform content;
        public Scrollbar scrollbar;
        private Vector3 contentDefaultPos;
        private bool _configured = false;
        private float contentHeight = 0f;
        private float scrollViewHeight = 0f;
        private float count = 5f;
        [SerializeField] private bool usePlayerJoinLeftAdjust = false;

        //[SerializeField] bool debug = false;
        private void Start()
        {
            if (scrollView == null) scrollView = gameObject.GetComponent<RectTransform>();
            _configured = content != null || scrollbar != null;
            if (_configured)
            {
                contentDefaultPos = content.localPosition;
                AdjustScrollView();
            }
        }

        private void Update()
        {
            if (count > 0f)
            {
                AdjustScrollView();
                count -= Time.deltaTime;
            }
        }

        public void AdjustScrollView()
        {
            if (!_configured) return;
            scrollViewHeight = scrollView.rect.height;
            contentHeight = 0f;
            int childCount = content.transform.childCount;
            int activeCount = 0;
            for (int i = 0; i < childCount; i++)
            {
                if (content.GetChild(i).gameObject.activeSelf)
                {
                    contentHeight += content.GetChild(i).GetComponent<RectTransform>().rect.height;
                    activeCount += 1;
                    //if (debug) Debug.Log(content.GetChild(i).name + " " + content.GetChild(i).GetComponent<RectTransform>().rect.height);
                }
            }
            if (activeCount > 0)
            {
                contentHeight += content.GetComponent<VerticalLayoutGroup>().spacing * (activeCount - 1);
            }
            //if (debug) Debug.Log(scrollViewHeight.ToString() + " " + contentHeight.ToString());
            scrollbar.size = (scrollViewHeight > contentHeight) ? 1f : (scrollViewHeight / contentHeight);
        }

        public void ScrollValueChanged()
        {
            if (!_configured) return;
            if (scrollViewHeight > contentHeight)
            {
                content.localPosition = contentDefaultPos;
            }
            else
            {
                content.localPosition = new Vector3(
                    contentDefaultPos.x,
                    contentDefaultPos.y + (contentHeight - scrollViewHeight) * scrollbar.value,
                    contentDefaultPos.z);
            }
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (usePlayerJoinLeftAdjust)
                count = 5f;
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (usePlayerJoinLeftAdjust)
                count = 5f;
        }
    }
}