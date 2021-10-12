/*
* Copyright (c) 2021 AoiKamishiro
* 
* This code is provided under the MIT license.
* 
*/

#if VRC_SDK_VRCSDK3
using UdonSharp;
using UnityEngine;

namespace Kamishiro.VRChatUDON.VRChatEventCalendar.SDK3
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ColorCodeReader : UdonSharpBehaviour
    {
        public Camera ccr_cam;
        public MeshRenderer ccr_mesh;
        public Texture2D texture;
        public float objectHeight = -500f;
        public int calendarHeight = 0;
        private Rect _rect;

        private void Start()
        {
            Vector3 pos = ccr_mesh.transform.parent.position;
            ccr_mesh.transform.parent.position = new Vector3(pos.x, objectHeight, pos.z);
            ccr_mesh.enabled = true;
            _rect = ccr_cam.pixelRect;
        }
        private void OnPostRender()
        {
            texture.ReadPixels(_rect, 0, 0, false);
            texture.Apply(false);

            Color s1 = texture.GetPixel(1, 126);
            Color s2 = texture.GetPixel(1, 125);
            Color s3 = texture.GetPixel(1, 124);

            int halfHeight =
              (Mathf.RoundToInt(Mathf.Pow(s1.r, 1.0f / 2.2f) * 7.0f) << 0) +
              (Mathf.RoundToInt(Mathf.Pow(s1.g, 1.0f / 2.2f) * 7.0f) << 2) +
              (Mathf.RoundToInt(Mathf.Pow(s1.b, 1.0f / 2.2f) * 7.0f) << 4) +
              (Mathf.RoundToInt(Mathf.Pow(s2.r, 1.0f / 2.2f) * 7.0f) << 6) +
              (Mathf.RoundToInt(Mathf.Pow(s2.g, 1.0f / 2.2f) * 7.0f) << 8) +
              (Mathf.RoundToInt(Mathf.Pow(s2.b, 1.0f / 2.2f) * 7.0f) << 10) +
              (Mathf.RoundToInt(Mathf.Pow(s3.r, 1.0f / 2.2f) * 7.0f) << 12) +
              (Mathf.RoundToInt(Mathf.Pow(s3.g, 1.0f / 2.2f) * 7.0f) << 14) +
              (Mathf.RoundToInt(Mathf.Pow(s3.b, 1.0f / 2.2f) * 7.0f) << 16);

            calendarHeight = halfHeight;
        }
        public void _SetCameraEnable()
        {
            ccr_cam.enabled = true;
        }
        public void _SetCameraDisable()
        {
            ccr_cam.enabled = false;
        }
        public void _CalclateHeight()
        {
            SendCustomEvent(nameof(_SetCameraEnable));
            SendCustomEventDelayedFrames(nameof(_SetCameraDisable), 30);
        }
    }
}
#endif