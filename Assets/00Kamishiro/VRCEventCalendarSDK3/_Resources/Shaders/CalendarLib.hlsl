/*
* Copyright (c) 2021 AoiKamishiro
*
* This code is provided under the MIT license.
*/

int ColorDecode(sampler2D tex, float2 uv1)
{
    int value = 0;
    int3 c1 = round(3.0f * LinearToGammaSpace(tex2Dlod(tex, float4(uv1.x, uv1.y, 0, 0)).rgb));
    value += (c1.r << 0) + (c1.g << 2) + (c1.b << 4);
    return value;
}
int ColorDecode3(sampler2D tex, float2 uv1, float2 uv2, float2 uv3)
{
    int value = 0;
    int3 c1 = round(3.0f * LinearToGammaSpace(tex2Dlod(tex, float4(uv1.x, uv1.y, 0, 0)).rgb));
    int3 c2 = round(3.0f * LinearToGammaSpace(tex2Dlod(tex, float4(uv2.x, uv2.y, 0, 0)).rgb));
    int3 c3 = round(3.0f * LinearToGammaSpace(tex2Dlod(tex, float4(uv3.x, uv3.y, 0, 0)).rgb));
    value += (c1.r << 0) + (c1.g << 2) + (c1.b << 4);
    value += (c2.r << 6) + (c2.g << 8) + (c2.b << 10);
    value += (c3.r << 12) + (c3.g << 14) + (c3.b << 16);
    return value;
}
float2 ScrollUV(sampler2D tex, float2 uv, float scroll, float4x4 data1)
{
    float header = data1[1][3];
    float footer = data1[2][0];
    float xscale = data1[2][1];
    float yscale = data1[2][2];
    
    float2 uv0 = float2(uv.x / xscale, 1 - (1 - uv.y) / yscale);
    //uv0 += step(uv0.y, 0) * float2(0.5f - 16.0f / 4096.0f, 1.0f);
    uv0 += step(uv0.y, 0) * float2(0.49609375f, 1.0f);

    float2 uv2 = float2(uv.x / xscale, 1 - (1 - uv.y) / yscale - scroll * (2 - 1 / yscale));
    uv2 += step(uv2.y, 0) * float2(0.496093750f, 1.0f);

    //float2 uv3 = float2(2016.0f / 4096.0f, 1 - (1 - ((2829.0f - 44.0f) / 2829.0f)) / yscale - scroll * (2 - 1 / yscale));
    //uv3 += step(uv3.y, 0) * float2(2032.0f / 4096.0f, 1.0f);
    float2 uv3 = float2(0.4921875f, 1 - (0.0155532f) / yscale - scroll * (2 - 1 / yscale));
    uv3 += step(uv3.y, 0) * float2(0.49609375f, 1.0f);
    int code = ColorDecode(tex, uv3);

    //float padding = (44.0f) / 2829.0f;
    //float header2 = (129.0f - 8.0f) / 2829.0f;
    float yhLatio = (header - uv.y) / (header - 0.9416755038f);
    int mxi1 = round(floor((code - 1) / 4.0f));
    int mxi2 = round(frac((code - 1) / 4.0f) * 4.0f);
    //float2 uv4 = float2(uv.x / xscale, (data1[mxi1][mxi2] + 1.0f - (161.0f - 4.0f) * yhLatio / 2480.0f * 2000.0f) / data1[3][3]);
    //uv4 += step(uv4.y, 0) * float2(0.5f - 16.0f / 4096.0f, 1.0f);
    float2 uv4 = float2(uv.x / xscale, (data1[mxi1][mxi2] + 1.0f - 157.0f * yhLatio * 0.8064516129f) / data1[3][3]);
    uv4 += step(uv4.y, 0) * float2(0.49609375f, 1.0f);

    //int h01 = ColorDecode(tex, float2(uv2.x < 2032.0f / 4096.0f ? 2016.0f / 4096.0f: 4048.0f / 4096.0f, uv2.y));
    //int h02 = ColorDecode(tex, float2(uv4.x < 2032.0f / 4096.0f ? 2016.0f / 4096.0f: 4048.0f / 4096.0f, uv4.y));
    int h01 = ColorDecode(tex, float2(uv2.x < 0.49609375f ? 0.4921875f: 0.98828125f, uv2.y));
    int h02 = ColorDecode(tex, float2(uv4.x < 0.49609375f ? 0.4921875f: 0.98828125f, uv4.y));

    return uv.y > header ? float2(uv.x / xscale, 1 - (1 - uv.y) / yscale):
    //uv.y < footer ? float2(uv.x / xscale + 0.5 - 16.0f / 4096.0f, uv.y / yscale):
    uv.y < footer ? float2(uv.x / xscale + 0.49609375f, uv.y / yscale):
    //yscale <= 1 ?uv0: uv.y > 1 - padding - header2 - (2.0f / 2829.0f) ? h01 > h02 || code == 0 ? uv2: uv4: uv2;
    yscale <= 1 ?uv0: uv.y > 0.940968540209f ? h01 > h02 || code == 0 ? uv2: uv4: uv2;
}
void FragCore(sampler2D tex, sampler2D loadTex, sampler2D handle, float2 uv, float4x4 data1, float4x4 data2, float scroll, out float3 color)
{
    //float sLeft = 1859.0f / 2000.0f;
    //float sRight = 1956.0f / 2000.0f;
    float sLeft = 0.9295f;
    float sRight = 0.978f;
    float bottom = data1[2][0] + (data1[1][3] - data1[2][0]) / 10.0f;
    float sBottom = data1[1][3] - (data1[1][3] - bottom) * scroll - ((data1[1][3] - data1[2][0]) / 10.0f);
    float sTop = data1[1][3] - (data1[1][3] - bottom) * scroll;

    //float outlinex = 4.0f / 2000.0f;
    //float outliney = 4.0f / 2829.0f;
    float outlinex = 0.002f;
    float outliney = 0.00141392718f;

    float3 sHadle = float3(data2[0][0], data2[0][1], data2[0][2]);
    float3 sBackGround = float3(data2[1][0], data2[1][1], data2[1][2]);
    float3 sOutline = float3(data2[2][0], data2[2][1], data2[2][2]);

    float xRatio = (uv.x - sLeft) / (sRight - sLeft);
    float yRatio = (uv.y - sBottom) / (sTop - sBottom);
    float4 handleColor = tex2D(handle, float2(xRatio, yRatio));

    float4 mainColor = tex2D(tex, ScrollUV(tex, uv, scroll, data1));
    
    color = data1[3][2] == 0.0f?tex2D(loadTex, uv): sBottom < uv.y && uv.y < sTop && sLeft < uv.x && uv.x < sRight?handleColor.a < 0.1 ?
    mainColor.rgb: (1 - handleColor.r) * sOutline + handleColor.r * sHadle: mainColor.rgb;
}
void VertCore(sampler2D tex, out float4x4 data1, out float4x4 data2)
{
    //float x = 1 - 16.0f / 4096.0f;
    //int totalHeight = ColorDecode3(tex, float2(x, 1 - (2 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (3 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (4 * 32.0f - 16.0f) / 4096.0f));
    //int h1 = totalHeight - 2 - ColorDecode3(tex, float2(x, 1 - (5 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (6 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (7 * 32.0f - 16.0f) / 4096.0f));
    //int h2 = totalHeight - 2 - ColorDecode3(tex, float2(x, 1 - (8 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (9 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (10 * 32.0f - 16.0f) / 4096.0f));
    //int h3 = totalHeight - 2 - ColorDecode3(tex, float2(x, 1 - (11 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (12 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (13 * 32.0f - 16.0f) / 4096.0f));
    //int h4 = totalHeight - 2 - ColorDecode3(tex, float2(x, 1 - (14 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (15 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (16 * 32.0f - 16.0f) / 4096.0f));
    //int h5 = totalHeight - 2 - ColorDecode3(tex, float2(x, 1 - (17 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (18 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (19 * 32.0f - 16.0f) / 4096.0f));
    //int h6 = totalHeight - 2 - ColorDecode3(tex, float2(x, 1 - (20 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (21 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (22 * 32.0f - 16.0f) / 4096.0f));
    //int h7 = totalHeight - 2 - ColorDecode3(tex, float2(x, 1 - (23 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (24 * 32.0f - 16.0f) / 4096.0f), float2(x, 1 - (25 * 32.0f - 16.0f) / 4096.0f));

    float x = 0.99609375f;
    int totalHeight = ColorDecode3(tex, float2(x, 0.98828125f), float2(x, 0.98046875f), float2(x, 0.97265625f));
    int h1 = totalHeight - 2 - ColorDecode3(tex, float2(x, 0.96484375f), float2(x, 0.95703125f), float2(x, 0.94921875f));
    int h2 = totalHeight - 2 - ColorDecode3(tex, float2(x, 0.94140625f), float2(x, 0.93359375f), float2(x, 0.92578125f));
    int h3 = totalHeight - 2 - ColorDecode3(tex, float2(x, 0.91796875f), float2(x, 0.91015625f), float2(x, 0.90234375f));
    int h4 = totalHeight - 2 - ColorDecode3(tex, float2(x, 0.89453125f), float2(x, 0.88671875f), float2(x, 0.87890625f));
    int h5 = totalHeight - 2 - ColorDecode3(tex, float2(x, 0.87109375f), float2(x, 0.86328125f), float2(x, 0.85546875f));
    int h6 = totalHeight - 2 - ColorDecode3(tex, float2(x, 0.84765625f), float2(x, 0.83984375f), float2(x, 0.83203125f));
    int h7 = totalHeight - 2 - ColorDecode3(tex, float2(x, 0.82421875f), float2(x, 0.81640625f), float2(x, 0.80859375f));

    //float xscale = 4096.0f / 2000.0f;
    //float yscale = totalHeight / 4096.0f * (3508.0f / 2480.0f) ;
    //float header = 1 - (44.0f) / 2829.0f ;
    //float footer = (44.0f * 2.0f + 161.0f - 2.0f) / (3508.0f / 2480.0f * 2000.0f);
    float xscale = 2.048f;
    float yscale = totalHeight * 0.000345340852f ;
    float header = 0.984446801f ;
    float footer = 0.087309008f;
    float isLoaded = tex2Dlod(tex, float4(1, 1, 0, 0)).rgb - 0.001 > 0 ? 1.0f: 0.0f;
    data1 = float4x4(h1, h2, h3, h4, h5, h6, h7, header, footer, xscale, yscale, 0, 0, 0, isLoaded, totalHeight);
    
    //float3 sHandle = tex2Dlod(tex, fixed4(1, 1 - (31 * 32.0f - 16.0f) / 4096.0f, 0, 0)).rgb;
    //float3 sBackground = tex2Dlod(tex, fixed4(1, 1 - (32 * 32.0f - 16.0f) / 4096.0f, 0, 0)).rgb;
    //float3 sOutline = tex2Dlod(tex, fixed4(1, 1 - (33 * 32.0f - 16.0f) / 4096.0f, 0, 0)).rgb;
    float3 sHandle = tex2Dlod(tex, fixed4(1, 0.76171875f, 0, 0)).rgb;
    float3 sBackground = tex2Dlod(tex, fixed4(1, 0.75390625f, 0, 0)).rgb;
    float3 sOutline = tex2Dlod(tex, fixed4(1, 0.74609375f, 0, 0)).rgb;
    data2 = float4x4(sHandle.r, sHandle.g, sHandle.b, 0, sBackground.r, sBackground.g, sBackground.b, 0, sOutline.r, sOutline.g, sOutline.b, 0, 0, 0, 0, 0);
}