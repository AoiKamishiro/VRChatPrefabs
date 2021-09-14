﻿using UnityEditor;
using UnityEngine;

namespace VRWorldToolkit
{
    public class VRWTAbout : EditorWindow
    {
        [MenuItem("VRWorld Toolkit/About VRWorld Toolkit", false, 41)]
        public static void ShowWindow()
        {
            var window = (VRWTAbout) GetWindow(typeof(VRWTAbout), true, "VRWorld Toolkit");
            window.minSize = new Vector2(600, 380);
            window.maxSize = new Vector2(600, 380);
            window.Show();
        }

        private static GUIStyle header, text;

        private static Texture iconTwitter, iconDiscord, iconGithub, iconPatreon, iconKofi;

        public void OnEnable()
        {
            header = new GUIStyle
            {
                normal =
                {
                    background = Resources.Load("SplashTextures/VRWTSplashLogo") as Texture2D,
                    textColor = Color.white,
                },
                fixedHeight = 140
            };

            text = new GUIStyle("Label")
            {
                wordWrap = true,
                richText = true
            };

            iconTwitter = Resources.Load("SplashTextures/IconTwitter") as Texture2D;
            iconDiscord = Resources.Load("SplashTextures/IconDiscord") as Texture2D;
            iconGithub = Resources.Load("SplashTextures/IconGithub") as Texture2D;
            iconPatreon = Resources.Load("SplashTextures/IconPatreon") as Texture2D;
            iconKofi = Resources.Load("SplashTextures/IconKofi") as Texture2D;
        }

        private void OnGUI()
        {
            // Header Image
            GUILayout.Box("", header);

            // Information Texts
            GUILayout.Label("Welcome to VRWorld Toolkit!", EditorStyles.boldLabel);

            GUILayout.Label("VRWorld Toolkit is a project aimed at helping people get into world building faster without spending time combing different documentations for all the smaller mistakes you can make while making your first world. Even for experienced world builders, it helps make tedious steps like setting up post-processing faster and allows you not to forget the dozen little things you need to remember while building worlds.", text);

            GUILayout.Label("If you have suggestions, found problems with the included tools, or want to check my social channels, you can click on the buttons below. Feedback is always welcome, so I know what to improve!", text);

            GUILayout.FlexibleSpace();

            // Social Buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(iconTwitter, GUIStyle.none)) Application.OpenURL("https://twitter.com/Sackboy_1");
            GUILayout.Space(20);
            if (GUILayout.Button(iconDiscord, GUIStyle.none)) Application.OpenURL("https://discord.gg/8w2Tc6C");
            GUILayout.Space(20);
            if (GUILayout.Button(iconGithub, GUIStyle.none)) Application.OpenURL("https://github.com/oneVR/VRWorldToolkit");
            GUILayout.Space(20);
            if (GUILayout.Button(iconPatreon, GUIStyle.none)) Application.OpenURL("https://www.patreon.com/onevr");
            GUILayout.Space(20);
            if (GUILayout.Button(iconKofi, GUIStyle.none)) Application.OpenURL("https://ko-fi.com/onevr");

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);
        }
    }
}