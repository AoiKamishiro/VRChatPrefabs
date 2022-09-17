/*
 * 
 * Copyright (c) 2021 AoiKamishiro
 * 
 * This code is provided under the MIT license.
 * 
 */

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UnityEngine;

namespace Kamishiro.VRChatUDON.AKSwitch
{
    public static class ShurikenUI
    {
        private static readonly int HEADER_HEIGHT = 22;
        private static Rect DrawShuriken(string title, Vector2 contentOffset)
        {
            return DrawShuriken(title, contentOffset, 0);
        }
        private static Rect DrawShuriken(string title, Vector2 contentOffset, int indentLevel)
        {
            GUIStyle style = new GUIStyle("ShurikenModuleTitle")
            {
                margin = new RectOffset(indentLevel * 28, 0, 8, 0),
                font = new GUIStyle(EditorStyles.boldLabel).font,
                border = new RectOffset(15, 7, 4, 4),
                fixedHeight = HEADER_HEIGHT,
                contentOffset = contentOffset
            };
            Rect rect = GUILayoutUtility.GetRect(16f, HEADER_HEIGHT, style);
            GUI.Box(rect, title, style);
            return rect;
        }

        public static void Header(string title)
        {
            //DrawShuriken(title, new Vector2(6f, -2f));
            Header(title, 0);
        }
        public static void Header(GUIContent gUIContent)
        {
            //DrawShuriken(title, new Vector2(6f, -2f));
            Header(gUIContent, 0);
        }
        public static void Header(string title, int indentLevel)
        {
            //DrawShuriken(title, new Vector2(6f, -2f));
            DrawShuriken(title, new Vector2(20f, -2f), indentLevel);
        }
        public static void Header(GUIContent gUIContent, int indentLevel)
        {
            //DrawShuriken(title, new Vector2(6f, -2f));
            DrawShuriken(gUIContent.text, new Vector2(20f, -2f), indentLevel);
        }
        public static bool Foldout(string title, bool display)
        {
            return Foldout(title, display, 0);
        }
        public static bool Foldout(string title, bool display, int indentLevel)
        {
            Rect rect = DrawShuriken(title, new Vector2(20f, -2f), indentLevel);
            Event e = Event.current;
            Rect toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }
            return display;
        }
    }
}
#endif