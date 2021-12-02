Shader "Kamishiro/ASKwitch"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" { }
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _BumpMap ("Nomalmap", 2D) = "bump" { }
        _ITex ("Icon Texture", 2D) = "black" { }
        _IColor ("Icon Color", Color) = (1, 1, 1, 1)
        _IEmission ("Icon Emission", Color) = (0, 0, 0, 1)
        _IArea ("Icon Area", Vector) = (0, 1, 0, 1)
        _IRect ("Icon Rect", vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _ITex;

        struct Input
        {
            float2 uv_MainTex;
        };

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(half, _Glossiness)
        UNITY_DEFINE_INSTANCED_PROP(half, _Metallic)
        UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
        UNITY_DEFINE_INSTANCED_PROP(fixed4, _IColor)
        UNITY_DEFINE_INSTANCED_PROP(fixed4, _IEmission)
        UNITY_DEFINE_INSTANCED_PROP(fixed4, _IArea)
        UNITY_DEFINE_INSTANCED_PROP(fixed4, _IRect)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            half Glossiness = UNITY_ACCESS_INSTANCED_PROP(Props, _Glossiness);
            half Metallic = UNITY_ACCESS_INSTANCED_PROP(Props, _Metallic);
            fixed4 Color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            fixed4 IArea = UNITY_ACCESS_INSTANCED_PROP(Props, _IArea);
            fixed4 IRect = UNITY_ACCESS_INSTANCED_PROP(Props, _IRect);
            fixed4 IColor = UNITY_ACCESS_INSTANCED_PROP(Props, _IColor);
            fixed4 IEmission = UNITY_ACCESS_INSTANCED_PROP(Props, _IEmission);

            fixed2 uvM = IN.uv_MainTex;
            fixed2 uvI = fixed2(((clamp(uvM.x, IArea.x, IArea.y) - IArea.x) / (IArea.y - IArea.x) + IRect.z) / IRect.x, ((clamp(uvM.y, IArea.z, IArea.w) - IArea.z) / (IArea.w - IArea.z) + IRect.w) / IRect.y);
            fixed4 c1 = tex2D(_MainTex, uvM) * Color;
            fixed4 c2 = IArea.x < uvM.x && uvM.x < IArea.y && IArea.z < uvM.y && uvM.y < IArea.w ? tex2D(_ITex, uvI) * IColor : fixed4(0, 0, 0, 0);
            o.Albedo = c1.rgb * (1.0f - c2.a) + c2.rgb * c2.a;
            o.Emission = IEmission.rgb * c2.a;
            o.Normal = UnpackNormal(tex2D(_BumpMap, uvM));
            o.Metallic = Metallic;
            o.Smoothness = Glossiness;
            o.Alpha = 1;
        }
        ENDCG

    }
    FallBack "Diffuse"
}
