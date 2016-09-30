using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum DyeType
{
    Default,
    globalInfo,
    frameStatement,
    CFunc,
    LuaMemBytes,
    LuaInFile,
    Blank,
}

public class DyePattern
{
    public float hueMin = 0.0f;
    public float hueMax = 1.0f;
    public float satMin = 0.0f;
    public float satMax = 1.0f;
    public float valMin = 0.0f;
    public float valMax = 1.0f;
}

public class HanoiConst
{
    public const int BAD_NUM = -1;

    public const double ShrinkThreshold = 5.0;
    public static Dictionary<DyeType, DyePattern> DyePallette = new Dictionary<DyeType, DyePattern>()
    {
        { DyeType.Default,      new DyePattern { hueMin = 0, hueMax = 0, satMin = 0, satMax = 0, valMin = 0.5f, valMax = 0.8f} },       // gray
        { DyeType.CFunc,        new DyePattern { hueMin = 208.0f / 360.0f, hueMax = 208.0f / 360.0f, satMin = 0.5f, satMax = 0.9f, valMin = 0.5f, valMax = 0.9f} },   // blue
        { DyeType.LuaMemBytes,  new DyePattern { hueMin = 80.0f / 360.0f, hueMax = 120.0f / 360.0f, satMin = 0.3f, satMax = 0.6f, valMin = 0.8f, valMax = 1.0f} },    // green
        { DyeType.LuaInFile,    new DyePattern { hueMin = 15.0f / 360.0f, hueMax = 30.0f / 360.0f, satMin = 0.5f, satMax = 0.9f, valMin = 0.5f, valMax = 0.9f} },     // red / orange
        { DyeType.Blank,    new DyePattern { hueMin = 0.0f / 360.0f, hueMax = 0.0f / 360.0f, satMin = 0.0f, satMax = 0.0f, valMin = 0.3f, valMax = 0.3f} },     // red / orange
        { DyeType.globalInfo,    new DyePattern { hueMin = 130.0f / 360.0f, hueMax = 0.0f / 360.0f, satMin = 0.0f, satMax = 0.0f, valMin = 0.3f, valMax = 0.3f} },     // red / orange
        { DyeType.frameStatement,    new DyePattern { hueMin = 0.0f / 360.0f, hueMax = 0.0f / 360.0f, satMin = 0.0f, satMax = 0.0f, valMin = 0.3f, valMax = 0.3f} },     // red / orange
    };

    public static Color GetDyeColor(DyeType t)
    {
        DyePattern p;
        if (!DyePallette.TryGetValue(t, out p))
            return Color.black;

        return Random.ColorHSV(p.hueMin, p.hueMax, p.satMin, p.satMax, p.valMin, p.valMax);
    }
}
