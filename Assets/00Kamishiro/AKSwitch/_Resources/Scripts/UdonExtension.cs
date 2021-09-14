/*
 * 
 * Copyright (c) 2021 AoiKamishiro
 * 
 * This code is provided under the MIT license.
 * 
 */

#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace Kamishiro.VRChatUDON.AKSwitch
{
    internal static class UdonExtension
    {
        internal static void SetVariavle<T>(this UdonBehaviour udon, string symbolName, T variable)
        {
            if (udon.publicVariables.TrySetVariableValue(symbolName, variable))
                return;

            IUdonVariable udonVariable = (IUdonVariable)Activator.CreateInstance(typeof(UdonVariable<>).MakeGenericType(typeof(T)), symbolName, variable);
            if (!udon.publicVariables.TryAddVariable(udonVariable))
            {
                Debug.LogError("Failed to set variable: " + symbolName);
            }

            EditorUtility.SetDirty(udon);
            Undo.RecordObject(udon, "Set Variable");
        }
        internal static T GetVariable<T>(this UdonBehaviour udon, string symbolName)
        {
            _ = udon.publicVariables.TryGetVariableValue(symbolName, out T variable);
            return variable;
        }
    }
}
#endif