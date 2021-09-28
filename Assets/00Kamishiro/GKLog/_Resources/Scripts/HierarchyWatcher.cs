/*
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
using UnityEngine.UI;
using VRC.Udon;

namespace Kamishiro.VRChatUDON.GKLog
{
    [InitializeOnLoadAttribute]
    public static class HierarchyWatcher
    {
        static HierarchyWatcher()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            PlaymodeStateObserver.OnPressedPlayButton += () =>
            {
                EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            };
            PlaymodeStateObserver.OnEnded += () =>
            {
                EditorApplication.hierarchyChanged += OnHierarchyChanged;
            };
        }

        private static void OnHierarchyChanged()
        {
            List<UdonBehaviour> playerLogSystems = new List<UdonBehaviour>();
            foreach (UdonBehaviour udon in GetComponentsInActiveScene<UdonBehaviour>())
            {
                if (udon == null)
                    continue;

                if (udon.programSource == null)
                    continue;

                if (udon.programSource.name != nameof(PlayerLogSystem))
                    continue;

                playerLogSystems.Add(udon);
            }

            foreach (UdonBehaviour ud in playerLogSystems)
            {
                LogObject[] logObjects = ud.gameObject.GetUdonSharpComponentsInChildren<LogObject>();
                PlayerLogSystem playerLogSystem = ud.gameObject.GetUdonSharpComponent<PlayerLogSystem>();
                playerLogSystem.SetMainCam();
                ScrollRect scrollRect = ud.GetComponentInChildren<ScrollRect>();
                playerLogSystem.UpdateProxy();
                if (playerLogSystem.scrollRect != scrollRect) playerLogSystem.scrollRect = scrollRect;
                if (!playerLogSystem._logObjects.ArrayEqual(logObjects)) playerLogSystem._logObjects = logObjects;
                playerLogSystem.ApplyProxyModifications();
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
        private static bool ArrayEqual<T>(this T[] array1, T[] array2) where T : Component
        {
            if (array1 == null || array2 == null) return true;
            if (array1.Length != array2.Length) return false;
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }
            return true;
        }
    }
}
#endif