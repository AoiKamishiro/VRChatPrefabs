/*
* Copyright (c) 2021 AoiKamishiro
*
* This code is provided under the MIT license.
*
* This program uses the following code, which is provided under the MIT License.
* https://download.unity3d.com/download_unity/008688490035/builtin_shaders-2018.4.20f1.zip?_ga=2.171325672.957521966.1599549120-262519615.1592172043
*
*/

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "CalendarLib.hlsl"

struct appdata
{
    /*
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 texcoord : TEXCOORD0;
    float4 texcoord1 : TEXCOORD1;
    float4 texcoord2 : TEXCOORD2;
    float4 texcoord3 : TEXCOORD3;
    fixed4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID */

    float4 vertex: POSITION;
    float4 tangent: TANGENT;
    float3 normal: NORMAL;
    float2 texcoord: TEXCOORD0;
    float2 texcoord1: TEXCOORD1;
    fixed4 color: COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 pos: SV_POSITION;
    float2 texcoord: TEXCOORD0;
    #ifdef Calendar_Lighting
        #ifdef LIGHTMAP_ON
            float2 texcoord1: TEXCOORD1;
        #endif
        float3 vertex: TEXCOORD2;
        float4 ambient: COLOR;
        float3 normal: NORMAL;
        //SHADOW_COORDS(6)
        LIGHTING_COORDS(6, 7)

    #endif
    UNITY_FOG_COORDS(8)

    float4x4 data1: TEXCOORD10;
    float4x4 data2: TEXCOORD14;
    UNITY_VERTEX_OUTPUT_STEREO
};

sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _Loading;
float4 _Loading_ST;
sampler2D _Handle;
float4 _Handle_ST;
float _Scroll;
float _Intensity;

v2f vert(appdata v)
{
    v2f o;
    //UNITY_INITIALIZE_OUTPUT(v2f, o);
    //UNITY_SETUP_INSTANCE_ID(v);
    //UNITY_TRANSFER_INSTANCE_ID(v, o);
    o.pos = UnityObjectToClipPos(v.vertex);
    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
    UNITY_TRANSFER_FOG(o, v.vertex);

    VertCore(_MainTex, o.data1, o.data2);
    #ifdef Calendar_Lighting
        o.vertex = mul(unity_ObjectToWorld, v.vertex);
        o.normal = UnityObjectToWorldNormal(v.normal);
        o.ambient = fixed4(ShadeSH9(half4(o.normal, 1)), 1);
        #ifdef LIGHTMAP_ON
            o.texcoord1 = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        #endif
        //TRANSFER_SHADOW(o)
        TRANSFER_VERTEX_TO_FRAGMENT(o)
    #endif

    return o;
}

fixed4 frag(v2f i): SV_Target
{
    fixed4 col = fixed4(1, 1, 1, 1);
    float3 ss;
    FragCore(_MainTex, _Loading, _Handle, i.texcoord, i.data1, i.data2, _Scroll, ss);
    col.rgb = ss;
    fixed4 c = fixed4(1, 1, 1, 1);
    #ifdef Calendar_Lighting
        UNITY_LIGHT_ATTENUATION(attenuation, i, i.vertex);

        float3 normal = normalize(i.normal);
        float3 light = normalize(_WorldSpaceLightPos0.w == 0 ? _WorldSpaceLightPos0.xyz: _WorldSpaceLightPos0.xyz - i.vertex);
        float diffuse = saturate(dot(normal, light));

        c.rgb *= col.rgb * diffuse * _LightColor0 * attenuation;
        c.rgb += col.rgb * i.ambient;

        #ifdef LIGHTMAP_ON
            c.rgb += col.rgb * DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.texcoord1));
        #endif
    #else
        c *= col;
    #endif
    c.rgb += c.rgb * _Intensity;
    UNITY_APPLY_FOG(i.fogCoord, col);
    return c;
}
