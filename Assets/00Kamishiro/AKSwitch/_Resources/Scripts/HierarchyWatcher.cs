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
using UdonSharpEditor;
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
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private static void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
        {
            List<AKSwitch> akswitches = new List<AKSwitch>();
            foreach (UdonBehaviour udon in GetComponentsInScene<UdonBehaviour>(scene))
            {
                if (udon == null || udon.programSource == null || udon.programSource.name != nameof(AKSwitch)) continue;
                akswitches.Add(udon.GetUdonSharpComponent<AKSwitch>());
            }

            if (akswitches.Count == 0) return;

            foreach (AKSwitch aKSwitch in akswitches)
            {
                if (aKSwitch.mainAKSwitch == akswitches[0]) continue;
                aKSwitch.UpdateProxy();
                aKSwitch.mainAKSwitch = akswitches[0];
                aKSwitch.FixInternalObjectsReference();
                aKSwitch.ApplyProxyModifications();
            }
        }

        private static void OnHierarchyChanged()
        {
            if (PlaymodeStateObserver.playModeState != PlaymodeStateObserver.PlayModeStateChangedType.Ended) return;
            MultipleSwitchOptimization();
        }
        private static void MultipleSwitchOptimization()
        {
            List<AKSwitch> akswitches = new List<AKSwitch>();
            foreach (UdonBehaviour udon in GetComponentsInScene<UdonBehaviour>(SceneManager.GetActiveScene()))
            {
                if (udon == null || udon.programSource == null || udon.programSource.name != nameof(AKSwitch)) continue;
                akswitches.Add(udon.GetUdonSharpComponent<AKSwitch>());
            }

            if (akswitches.Count == 0) return;

            foreach (AKSwitch aKSwitch in akswitches)
            {
                aKSwitch.ApplyMaterialPropertyBlock();
                if (aKSwitch.mainAKSwitch == akswitches[0]) continue;

                aKSwitch.UpdateProxy();
                aKSwitch.mainAKSwitch = akswitches[0];
                aKSwitch.ApplyProxyModifications();
            }
        }
        private static T[] GetComponentsInScene<T>(Scene scene, bool includeInactive = true) where T : Component
        {
            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            IEnumerable<T> resultComponents = (T[])Enumerable.Empty<T>();
            foreach (GameObject item in rootGameObjects)
            {
                T[] components = item.GetComponentsInChildren<T>(includeInactive);
                resultComponents = resultComponents.Concat(components);
            }
            return resultComponents.ToArray();
        }
    }
}
#endif