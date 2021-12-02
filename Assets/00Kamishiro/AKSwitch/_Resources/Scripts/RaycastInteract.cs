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
    public class RaycastInteract : UdonSharpBehaviour
    {
        public Animator animator;
        public string param = "push";
        public AKSwitch akSwitch;
        public BoxCollider _boxCollider;
        public override void Interact()
        {
            akSwitch.SendCustomEventDelayedSeconds(nameof(AKSwitch.OnInteracted), 0.2f);
            if (animator != null)  animator.SetTrigger(param);
        }
        public void _EnableRaycastInteraction()
        {
            _boxCollider.enabled = true;
            animator.enabled = true;
        }
        public void _DisbleRaycastInteraction()
        {
            _boxCollider.enabled = false;
            animator.enabled = false;
        }
    }
}