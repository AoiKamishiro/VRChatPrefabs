/*
* Copyright (c) 2021 AoiKamishiro
* 
* This code is provided under the MIT license.
* 
*/

#if VRC_SDK_VRCSDK3

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
#endif


namespace Kamishiro.VRChatUDON.VRChatEventCalendar.SDK3
{
#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [InitializeOnLoadAttribute]
    public static class HierarchyWatcher
    {
        static HierarchyWatcher()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }
        private static void OnHierarchyChanged()
        {
            MultipleCalendarOptimization();
        }
        private static void MultipleCalendarOptimization()
        {
            UdonBehaviour[] allUdons = GetComponentsInActiveScene<UdonBehaviour>();
            UdonBehaviour[] udons = new UdonBehaviour[] { };
            foreach (UdonBehaviour udon in allUdons)
            {
                if (udon.programSource == null)
                    continue;

                if (!udon.programSource.name.Equals(nameof(VRChatEventCalendar)))
                    continue;

                udons = udons.Concat(new UdonBehaviour[] { udon }).ToArray();
            }

            if (udons.Length == 0)
                return;

            if (!IsMaster(udons[0]))
                SetCalemdarAsMaster(udons[0]);

            if (udons.Length == 1)
                return;

            for (int i = 1; i < udons.Length; i++)
            {
                if (!IsCorrect(udons[i], udons[0]))
                    SetCalemdarAsNonMaster(udons[i], udons[0]);
            }
        }
        private static bool IsMaster(UdonBehaviour target)
        {
            target.publicVariables.TryGetVariableValue(nameof(VRChatEventCalendar.isMain), out bool isMain);
            if (!isMain)
                return false;

            return IsCorrect(target, target);
        }
        private static bool IsCorrect(UdonBehaviour target, UdonBehaviour master)
        {
            target.publicVariables.TryGetVariableValue(nameof(VRChatEventCalendar.colliders), out SphereCollider[] t_tc);
            master.publicVariables.TryGetVariableValue(nameof(VRChatEventCalendar.s_touchColliders), out SphereCollider[] m_tc);

            if (t_tc.Length == 0)
                return false;

            foreach (SphereCollider sphereCollider in t_tc)
            {
                if (sphereCollider == null)
                    return false;
            }

            if (t_tc.Length != m_tc.Length)
                return false;

            for (int i = 0; i < t_tc.Length; i++)
            {
                if (t_tc[i] != m_tc[i])
                    return false;
            }
            return true;
        }
        private static void SetCalemdarAsMaster(UdonBehaviour target)
        {
            target.publicVariables.TrySetVariableValue(nameof(VRChatEventCalendar.isMain), true);
            SetMasterObjects(target, target);
        }
        private static void SetCalemdarAsNonMaster(UdonBehaviour target, UdonBehaviour master)
        {
            target.publicVariables.TrySetVariableValue(nameof(VRChatEventCalendar.isMain), false);
            SetMasterObjects(target, master);
        }
        private static void SetMasterObjects(UdonBehaviour target, UdonBehaviour master)
        {
            master.publicVariables.TryGetVariableValue(nameof(VRChatEventCalendar.s_touchColliders), out SphereCollider[] m_tc);

            target.publicVariables.TrySetVariableValue(nameof(VRChatEventCalendar.colliders), m_tc);
        }

        private static void SetUdonVariavle<T>(UdonBehaviour udon, string symbolName, T variable)
        {
            if (udon.publicVariables.TrySetVariableValue(symbolName, variable))
                return;

            IUdonVariable udonVariable = (IUdonVariable)Activator.CreateInstance(typeof(UdonVariable<>).MakeGenericType(typeof(T)), symbolName, variable);
            if (!udon.publicVariables.TryAddVariable(udonVariable))
            {
                Debug.LogError($"Failed to set public variable '{symbolName}' value.");
            }
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

    }
#endif
}
#endif