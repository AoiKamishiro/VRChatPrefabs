/*
 * 
 * Copyright (c) 2021 AoiKamishiro
 * 
 * This code is provided under the MIT license.
 * 
 */

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Udon;

namespace Kamishiro.VRChatUDON.AKSwitch
{
    [InitializeOnLoadAttribute]
    internal static class HierarchyWatcher
    {
        static HierarchyWatcher()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private static void OnHierarchyChanged()
        {
            if (PlaymodeStateObserver.playModeState != PlaymodeStateObserver.PlayModeStateChangedType.Ended) return;
            MultipleSwitchOptimization();
        }

        private static void MultipleSwitchOptimization()
        {
            List<UdonBehaviour> akswitches = new List<UdonBehaviour>();
            foreach (UdonBehaviour udon in GetComponentsInActiveScene<UdonBehaviour>())
            {
                if (udon == null)
                    continue;

                if (udon.programSource == null)
                    continue;

                if (udon.programSource.name != nameof(AKSwitch))
                    continue;

                akswitches.Add(udon);
            }

            if (akswitches.Count == 0)
                return;

            SetMasterInternalObject(akswitches[0]);

            if (!IsMaster(akswitches[0]))
                SetCalemdarAsMaster(akswitches[0]);

            if (akswitches.Count == 1)
                return;

            for (int i = 1; i < akswitches.Count; i++)
            {
                if (!IsCorrect(akswitches[i], akswitches[0]))
                    SetCalemdarAsNonMaster(akswitches[i], akswitches[0]);
            }
        }
        private static bool IsMaster(UdonBehaviour target)
        {
            bool isMain = target.GetVariable<bool>(nameof(AKSwitch.isMainSwitch));
            if (!isMain)
                return false;

            return IsCorrect(target, target);
        }
        private static bool IsCorrect(UdonBehaviour target, UdonBehaviour master)
        {
            Transform t_touchColliders = target.GetVariable<Transform>(nameof(AKSwitch.touchColliders));
            Transform t_heightCalculator = target.GetVariable<Transform>(nameof(AKSwitch.heightCalculator));
            Transform m_touchColliders = master.GetVariable<Transform>(nameof(AKSwitch.s_touchColliders));
            Transform m_heightCalculator = master.GetVariable<Transform>(nameof(AKSwitch.s_heightCalculator));

            return t_touchColliders == m_touchColliders && t_heightCalculator == m_heightCalculator;
        }
        private static void SetCalemdarAsMaster(UdonBehaviour target)
        {
            target.SetVariavle(nameof(AKSwitch.isMainSwitch), true);
            SetMasterObjects(target, target);
        }
        private static void SetCalemdarAsNonMaster(UdonBehaviour target, UdonBehaviour master)
        {
            target.SetVariavle(nameof(AKSwitch.isMainSwitch), false);
            SetMasterObjects(target, master);
        }
        private static void SetMasterObjects(UdonBehaviour target, UdonBehaviour master)
        {
            Transform m_touchColliders = master.GetVariable<Transform>(nameof(AKSwitch.s_touchColliders));
            Transform m_theightAdjustor = master.GetVariable<Transform>(nameof(AKSwitch.s_heightCalculator));

            target.SetVariavle(nameof(AKSwitch.touchColliders), m_touchColliders);
            target.SetVariavle(nameof(AKSwitch.heightCalculator), m_theightAdjustor);
        }

        private static T[] GetComponentsInActiveScene<T>(bool includeInactive = true) where T : Component
        {
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            IEnumerable<T> resultComponents = (T[])Enumerable.Empty<T>();
            foreach (GameObject item in rootGameObjects)
            {
                T[] components = item.GetComponentsInChildren<T>(includeInactive);
                resultComponents = resultComponents.Concat(components);
            }
            return resultComponents.ToArray();
        }

        private static void SetMasterInternalObject(UdonBehaviour akswitch)
        {
            Transform s_heightCalculator = akswitch.GetVariable<Transform>(nameof(AKSwitch.s_heightCalculator));
            Transform s_touchColliders = akswitch.GetVariable<Transform>(nameof(AKSwitch.s_touchColliders));
            if (s_heightCalculator == null)
            {
                akswitch.SetVariavle<Transform>(nameof(AKSwitch.s_heightCalculator), akswitch.transform.Find("HeightCalclator"));
            }
            if (s_touchColliders == null)
            {
                akswitch.SetVariavle<Transform>(nameof(AKSwitch.s_touchColliders), akswitch.transform.Find("Touch"));
            }
        }
    }
}
#endif