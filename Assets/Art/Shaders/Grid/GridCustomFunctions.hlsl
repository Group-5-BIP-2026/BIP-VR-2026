float GridLineMask(float3 WorldPos, float worldSpacing, float lineThickness)
{
    float2 coord = WorldPos.xz / max(worldSpacing, 1e-6);
    float2 deriv = fwidth(coord);
    float2 grid = abs(frac(coord - 0.5) - 0.5);
    float2 lines = smoothstep(0.0, deriv * lineThickness, grid);
    return 1.0 - min(lines.x, lines.y);
}

void InfiniteGridCore_float(
    float3 WorldPos,
    float3 CameraPos,
    float BaseSize,
    float LineThickness,
    float4 MeterColor,
    float4 CentimeterColor,
    float4 DecimeterColor,
    float4 DecameterColor,
    float4 HectometerColor,
    float4 FallbackColor,
    out float Alpha,
    out float3 Color)
{
    // Fixed real-world unit spacings so colors stay consistent regardless of zoom.
    float cameraHeight = abs(CameraPos.y);

    float cmSpacing = BaseSize * 0.01;
    float dmSpacing = BaseSize * 0.1;
    float mSpacing = BaseSize;
    float damSpacing = BaseSize * 10.0;
    float hmSpacing = BaseSize * 100.0;
    float farSpacing = BaseSize * 1000.0;

    // Only reveal very coarse layers when the camera is genuinely far away.
    float hectometerVisibility = smoothstep(hmSpacing * 0.75, hmSpacing * 3.0, cameraHeight);
    float farVisibility = smoothstep(farSpacing * 0.5, farSpacing * 2.0, cameraHeight);

    float lineCm = GridLineMask(WorldPos, cmSpacing, LineThickness);
    float lineDm = GridLineMask(WorldPos, dmSpacing, LineThickness);
    float lineM = GridLineMask(WorldPos, mSpacing, LineThickness);
    float lineDam = GridLineMask(WorldPos, damSpacing, LineThickness);
    float lineHm = GridLineMask(WorldPos, hmSpacing, LineThickness) * hectometerVisibility;
    float lineFar = GridLineMask(WorldPos, farSpacing, LineThickness) * farVisibility;

    // Priority from coarsest to finest to keep outer boundaries visually dominant.
    float aFar = saturate(lineFar);
    float aHm = saturate(lineHm * (1.0 - aFar));
    float aDam = saturate(lineDam * (1.0 - max(aFar, aHm)));
    float aM = saturate(lineM * (1.0 - max(aDam, max(aFar, aHm))));
    float aDm = saturate(lineDm * (1.0 - max(aM, max(aDam, max(aFar, aHm)))));
    float aCm = saturate(lineCm * (1.0 - max(aDm, max(aM, max(aDam, max(aFar, aHm))))));

    float3 weightedColor =
        FallbackColor.rgb * aFar +
        HectometerColor.rgb * aHm +
        DecameterColor.rgb * aDam +
        MeterColor.rgb * aM +
        DecimeterColor.rgb * aDm +
        CentimeterColor.rgb * aCm;

    Alpha = saturate(aFar + aHm + aDam + aM + aDm + aCm);
    Color = (Alpha > 0.0001) ? (weightedColor / Alpha) : MeterColor.rgb;
}

void InfiniteGrid_float(
    float3 WorldPos,
    float3 CameraPos,
    float3 CameraForward,
    float BaseSize,
    float LineThickness,
    float FadeDistance,
    float4 MeterColor,
    float4 CentimeterColor,
    float4 DecimeterColor,
    float4 DecameterColor,
    float4 HectometerColor,
    float4 FallbackColor,
    out float Alpha,
    out float3 Color)
{
    InfiniteGridCore_float(
        WorldPos,
        CameraPos,
        BaseSize,
        LineThickness,
        MeterColor,
        CentimeterColor,
        DecimeterColor,
        DecameterColor,
        HectometerColor,
        FallbackColor,
        Alpha,
        Color);
}

void InfiniteGrid_float(
    float3 WorldPos,
    float3 CameraPos,
    float CameraForward,
    float BaseSize,
    float LineThickness,
    float FadeDistance,
    float4 MeterColor,
    float4 CentimeterColor,
    float4 DecimeterColor,
    float4 DecameterColor,
    float4 HectometerColor,
    float4 FallbackColor,
    out float Alpha,
    out float3 Color)
{
    InfiniteGridCore_float(
        WorldPos,
        CameraPos,
        BaseSize,
        LineThickness,
        MeterColor,
        CentimeterColor,
        DecimeterColor,
        DecameterColor,
        HectometerColor,
        FallbackColor,
        Alpha,
        Color);
}

// Backward-compatible overload (without DecimeterColor input)
void InfiniteGrid_float(
    float3 WorldPos,
    float3 CameraPos,
    float3 CameraForward,
    float BaseSize,
    float LineThickness,
    float FadeDistance,
    float4 MeterColor,
    float4 CentimeterColor,
    float4 DecameterColor,
    float4 HectometerColor,
    float4 FallbackColor,
    out float Alpha,
    out float3 Color)
{
    float4 defaultDecimeterColor = lerp(CentimeterColor, MeterColor, 0.5);
    InfiniteGrid_float(
        WorldPos,
        CameraPos,
        CameraForward,
        BaseSize,
        LineThickness,
        FadeDistance,
        MeterColor,
        CentimeterColor,
        defaultDecimeterColor,
        DecameterColor,
        HectometerColor,
        FallbackColor,
        Alpha,
        Color);
}

void InfiniteGrid_float(
    float3 WorldPos,
    float3 CameraPos,
    float CameraForward,
    float BaseSize,
    float LineThickness,
    float FadeDistance,
    float4 MeterColor,
    float4 CentimeterColor,
    float4 DecameterColor,
    float4 HectometerColor,
    float4 FallbackColor,
    out float Alpha,
    out float3 Color)
{
    float4 defaultDecimeterColor = lerp(CentimeterColor, MeterColor, 0.5);
    InfiniteGrid_float(
        WorldPos,
        CameraPos,
        CameraForward,
        BaseSize,
        LineThickness,
        FadeDistance,
        MeterColor,
        CentimeterColor,
        defaultDecimeterColor,
        DecameterColor,
        HectometerColor,
        FallbackColor,
        Alpha,
        Color);
}

void InfiniteGrid_float(
    float3 WorldPos,
    float3 CameraPos,
    float3 CameraForward,
    float BaseSize,
    float LineThickness,
    float FadeDistance,
    out float Alpha)
{
    float3 color;
    InfiniteGrid_float(
        WorldPos,
        CameraPos,
        CameraForward,
        BaseSize,
        LineThickness,
        FadeDistance,
        float4(1, 1, 1, 1),
        float4(1, 1, 1, 1),
        float4(1, 1, 1, 1),
        float4(1, 1, 1, 1),
        float4(1, 1, 1, 1),
        Alpha,
        color);
}

void InfiniteGrid_float(
    float3 WorldPos,
    float3 CameraPos,
    float CameraForward,
    float BaseSize,
    float LineThickness,
    float FadeDistance,
    out float Alpha)
{
    float3 color;
    InfiniteGrid_float(
        WorldPos,
        CameraPos,
        CameraForward,
        BaseSize,
        LineThickness,
        FadeDistance,
        float4(1, 1, 1, 1),
        float4(1, 1, 1, 1),
        float4(1, 1, 1, 1),
        float4(1, 1, 1, 1),
        float4(1, 1, 1, 1),
        Alpha,
        color);
}
