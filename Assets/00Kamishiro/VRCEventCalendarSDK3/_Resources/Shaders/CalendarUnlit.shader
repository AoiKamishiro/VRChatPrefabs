/*
* Copyright (c) 2021 AoiKamishiro
*
* This code is provided under the MIT license.
*
* This program uses the following code, which is provided under the MIT License.
* https://download.unity3d.com/download_unity/008688490035/builtin_shaders-2018.4.20f1.zip?_ga=2.171325672.957521966.1599549120-262519615.1592172043
*
*/

Shader "Kamishiro/ScrollCalendar/CalendarUnlit"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Main Texture", 2D) = "white" { }
        [NoScaleOffset] _Loading ("Loading Texture", 2D) = "white" { }
        [NoScaleOffset] _Handle ("Handle Texture", 2D) = "white" { }
        _Scroll ("Scroll", Range(0, 1)) = 0
        _Intensity ("Unlit Intensity", Range(0, 1)) = 0
    }
    SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Opaque" "LightMode" = "Always" }

            CGPROGRAM

            #pragma multi_compile_fog
            #pragma vertex   vert
            #pragma fragment frag
            #include "CalendarCore.hlsl"
            ENDCG

        }
    }
    Fallback "Unlit"
}
