using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public delegate void HanoiNodeAction(HanoiNode n);

public class HanoiUtil 
{
    public static void ForeachInParentChain(HanoiNode n, HanoiNodeAction act)
    {
        HanoiNode target = n;
        while (target.Parent != null)
        {
            if (act != null)
                act(target);

            target = target.Parent;            
        }
    }

    public static void DrawBlankSpaceRecursively(HanoiNode n)
    {
        if (n is HanoiBlankSpace)
        {
            Color c = n.GetNodeColor();
            c.a = 0.5f;
            n.renderRect = new Rect((float)n.beginTime - DrawingShrinkedAccumulated, 0.0f, HanoiVars.BlankSpaceWidth, HanoiVars.StackHeight * (HanoiVars.DrawnStackCount - 1));
            HanoiUtil.DrawingShrinkedAccumulated += (float)n.timeConsuming - HanoiVars.BlankSpaceWidth;
            Handles.DrawSolidRectangleWithOutline(n.renderRect, c, c);
        }

        if (n.stackLevel == 0)
        {
            for (int i = 0; i < n.Children.Count; i++)
            {
                DrawBlankSpaceRecursively(n.Children[i]);
            }
        }
    }

    static public float DrawingShrinkedAccumulated = 0.0f;
    static public float DrawingShrinkedTotal = 0.0f;
    static public int DrawingCounts = 0;
    static Dictionary<int, Color> m_colors = new Dictionary<int, Color>();

    public static void DrawRecursively(HanoiNode n)
    {
        int hash = n.GetHashCode();
        Color c;
        if (!m_colors.TryGetValue(hash, out c))
        {
            m_colors[hash] = c = n.GetNodeColor();
        }

        if (n is HanoiBlankSpace)
        {
            HanoiUtil.DrawingShrinkedAccumulated += (float)n.timeConsuming - HanoiVars.BlankSpaceWidth;
        }
        else
        {
            float renderedWidth = (float)n.timeConsuming;

            n.renderRect = new Rect((float)n.beginTime - DrawingShrinkedAccumulated, HanoiVars.StackHeight * (HanoiVars.DrawnStackCount - n.stackLevel - 1), (float)n.timeConsuming, HanoiVars.StackHeight);
        }

        Handles.DrawSolidRectangleWithOutline(n.renderRect, c, n.highlighted ? Color.white : c);

        DrawingCounts++;

        for (int i = 0; i < n.Children.Count; i++)
        {
            DrawRecursively(n.Children[i]);
        }
    }

    public static void DrawLabelsRecursively(HanoiNode n)
    {
        if (n.highlighted)
        {
            Rect r = n.renderRect;

            r.width = HanoiVars.LabelBackgroundWidth;
            r.height = 45;
            Color bg = Color.black;
            bg.a = 0.5f;
            Handles.DrawSolidRectangleWithOutline(r, bg, bg);

            GUI.color = Color.white;
            Handles.Label(new Vector3(n.renderRect.xMin, n.renderRect.yMin), n.funcName);
            Handles.Label(new Vector3(n.renderRect.xMin, n.renderRect.yMin + 15), n.moduleName);
            Handles.Label(new Vector3(n.renderRect.xMin, n.renderRect.yMin + 30), string.Format("Time: {0:0.000}", n.timeConsuming));
        }

        for (int i = 0; i < n.Children.Count; i++)
        {
            DrawLabelsRecursively(n.Children[i]);
        }
    }
}
