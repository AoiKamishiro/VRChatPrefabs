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
using VRC.Udon;

namespace Kamishiro.VRChatUDON.VRChatEventCalendar.SDK3
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(BoxCollider))]
    public class TouchDetector : UdonSharpBehaviour
    {
        public bool isAbsolute = false;
        public Scrollbar scrollbar;
        public SyncCalendar syncScrollCalendar;
        public VRChatEventCalendar manager;
        public SphereCollider[] colliders;
        public UdonBehaviour colorCodeReader;
        private SphereCollider _enteredCollider;
        private float _extent = 0;
        //private float _prevHeight = float.MinValue;
        private float _error = 0.0001f;
        private float _spd = 0.0f;
        private float _timer = -1.0f;
        private bool _discord = false;
        public BoxCollider boxCollider;
        private float _scale;
        private float _dust;

        private void Start()
        {
            _scale = transform.lossyScale.y;
            _extent = boxCollider.size.y * _scale * 0.5f;
            colorCodeReader = (UdonBehaviour)manager.GetProgramVariable(nameof(VRChatEventCalendar.colorCodeReader));
            colliders = (SphereCollider[])manager.GetProgramVariable(nameof(VRChatEventCalendar.colliders));
        }
        private void Update()
        {
            if (isAbsolute)
                return;

            if (_timer < 0.0f)
                return;

            float delta = Mathf.Lerp(_spd, 0, 1 - _timer);
            float ratio = delta * Time.deltaTime;

            if (0.0f <= scrollbar.value + ratio || scrollbar.value <= 1.0f)
            {
                scrollbar.value += ratio;
                _timer -= Time.deltaTime;
            }
            else
            {
                if (0.0f > scrollbar.value + ratio)
                {
                    scrollbar.value = 0.0f;
                }
                else
                {
                    scrollbar.value = 1.0f;
                }
                _timer = 0.0f;
            }
            manager.SendCustomEvent(nameof(VRChatEventCalendar._SliderDraggd));
        }
        private void OnTriggerEnter(Collider other)
        {
            if (isAbsolute)
                return;

            foreach (SphereCollider collider in colliders)
            {
                if (other != collider)
                    continue;

                //_prevHeight = other.transform.position.y;
                _enteredCollider = (SphereCollider)other;
                _discord = true;
                _dust = CalcPos(collider);
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (isAbsolute)
            {
                foreach (SphereCollider sphere in colliders)
                {
                    if (other != sphere)
                        continue;

                    float top = boxCollider.center.y * _scale + boxCollider.size.y * 0.5f * _scale;
                    float buttom = boxCollider.center.y * _scale - boxCollider.size.y * 0.5f * _scale;

                    scrollbar.value = 1 - ((Mathf.Clamp(CalcPos(sphere), buttom, top) - buttom) / (top - buttom));
                }
                manager.SendCustomEvent(nameof(VRChatEventCalendar._SliderDraggd));
            }
            else
            {
                if (other != _enteredCollider)
                    return;

                int height = (int)colorCodeReader.GetProgramVariable(nameof(ColorCodeReader.calendarHeight));

                //if (height < 100)
                //return;

                float scale = height / 3508.0f;
                float pos = CalcPos(_enteredCollider);

                float delta = pos - _dust;
                float ratio = delta / (_extent * 2.0f) / scale;

                if (_discord && _error - Mathf.Abs(ratio) > 0.0f)
                    return;

                _discord = false;

                if (0.0f <= scrollbar.value + ratio || scrollbar.value <= 1.0f)
                {
                    scrollbar.value += ratio;
                    _timer -= Time.deltaTime;
                }
                else
                {
                    if (0.0f > scrollbar.value + ratio)
                    {
                        scrollbar.value = 0.0f;
                    }
                    else
                    {
                        scrollbar.value = 1.0f;
                    }
                    _timer = 0.0f;
                }
                manager.SendCustomEvent(nameof(VRChatEventCalendar._SliderDraggd));

                ((UdonBehaviour)_enteredCollider.GetComponent(typeof(UdonBehaviour))).SendCustomEvent("_PlayHaptic");

                _dust = pos;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (isAbsolute)
                return;

            if (other != _enteredCollider)
                return;

            int height = (int)colorCodeReader.GetProgramVariable(nameof(ColorCodeReader.calendarHeight));

            //if (height < 100)
            //return;

            float scale = height / 3508.0f;

            float delta = CalcPos(_enteredCollider) - _dust;
            float ratio = delta / (_extent * 2.0f) / scale;
            if (_error - Mathf.Abs(ratio) > 0.0f)
                return;

            _spd = ratio / Time.deltaTime;
            _timer = 1.0f;
        }
        private float CalcPos(Collider other)
        {
            return Vector3.Dot(transform.up, other.transform.position - transform.position);
        }
    }
}
#endif