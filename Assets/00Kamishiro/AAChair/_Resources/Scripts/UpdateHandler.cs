using UdonSharp;
using UnityEngine;

namespace online.kamishiro.vrc.udon.aachair
{
    [DefaultExecutionOrder(0), UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UpdateHandler : UdonSharpBehaviour
    {
        private AutoAdjustStation _autoAdjustStation;
        private AutoAdjustStation AutoAdjustStation
        {
            get
            {
                if (!_autoAdjustStation) _autoAdjustStation = GetComponentInParent<AutoAdjustStation>();
                return _autoAdjustStation;
            }
        }
        private void LateUpdate() => AutoAdjustStation._AAChair_AdjustPosition();
    }
}