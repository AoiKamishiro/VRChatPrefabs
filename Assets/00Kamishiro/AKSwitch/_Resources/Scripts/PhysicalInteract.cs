/*
 * 
 * Copyright (c) 2021 AoiKamishiro
 * 
 * This code is provided under the MIT license.
 * 
 */

using UdonSharp;
using UnityEngine;

namespace Kamishiro.VRChatUDON.AKSwitch
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class PhysicalInteract : UdonSharpBehaviour
    {
        private Collider _enteredCollider;
        private PlayerBoneTracker _enteredPlayerBoneTracker;
        private PlayerTrackpointTracker _enteredPlayerTrackpointTracker;
        public Transform startPoint;
        public Transform endPoint;
        public AKSwitch akSwitch;
        public BoxCollider _boxCollider;
        private Vector3 _initPos;
        private Vector3 _VVector;
        private float _distance;
        private float _maxDistance;
        private bool _switched = false;
        private Vector3 _pos;
        private Vector3 _initPosLocal;

        private void Start()
        {
            if (startPoint == null)
            {
                enabled = false;
                return;
            }

            _initPos = startPoint.position;
            _VVector = Vector3.Normalize(_initPos - endPoint.position);
            _maxDistance = Vector3.Distance(_initPos, endPoint.position);

            _initPosLocal = startPoint.localPosition;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
                return;

            PlayerBoneTracker p1 = other.GetComponent<PlayerBoneTracker>();
            PlayerTrackpointTracker p2 = other.GetComponent<PlayerTrackpointTracker>();
            if (p1 == null && p2 == null) return;

            _enteredCollider = other;
            _enteredPlayerBoneTracker = p1;
            _enteredPlayerTrackpointTracker = p2;
            _initPos = transform.TransformPoint(_initPosLocal);
            _VVector = Vector3.Normalize(_initPos - endPoint.position);
            _distance = Vector3.Dot(_VVector, _enteredCollider.transform.position - _initPos);
            _switched = false;
        }
        private void OnTriggerStay(Collider other)
        {
            if (other == null || _enteredCollider != other) return;

            _initPos = transform.TransformPoint(_initPosLocal);
            _VVector = Vector3.Normalize(_initPos - endPoint.position);
            float delta = _distance - Vector3.Dot(_VVector, _enteredCollider.transform.position - _initPos);

            if (delta < 0) delta = 0;
            else if (delta > _maxDistance) delta = _maxDistance;

            _pos = _initPos - _VVector * delta;
            SendCustomEvent(nameof(_SetPosition));

            if (_switched) return;

            if (delta < _maxDistance) return;

            akSwitch.SendCustomEvent(nameof(AKSwitch.OnInteracted));
            if (_enteredPlayerBoneTracker != null) _enteredPlayerBoneTracker.SendCustomEvent(nameof(PlayerBoneTracker._PlayHaptics));
            if (_enteredPlayerTrackpointTracker != null) _enteredPlayerTrackpointTracker.SendCustomEvent(nameof(PlayerTrackpointTracker._PlayHaptics));
            _switched = true;
        }
        private void OnTriggerExit(Collider other)
        {
            if (other == null) return;

            if (_enteredCollider != other) return;

            _pos = _initPos;
            _switched = false;
            SendCustomEvent(nameof(_SetPosition));
        }
        public void _SetPosition()
        {
            if (startPoint.position != _pos) startPoint.position = _pos;
        }
        public void _EnablePhysicalInteraction()
        {
            _boxCollider.enabled = true;
        }
        public void _DisblePhysicalInteraction()
        {
            _boxCollider.enabled = false;
        }
    }
}