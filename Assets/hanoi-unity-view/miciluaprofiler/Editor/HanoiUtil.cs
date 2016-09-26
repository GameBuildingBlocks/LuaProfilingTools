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

    static public float TotalTimeConsuming = 0.0f;
    static Dictionary<int, Color> m_colors = new Dictionary<int, Color>();
    static public Vector2 ScreenClipRange = new Vector2(0,0);

    /// <summary>
    /// 检测一个时间范围，是否在屏幕剪裁范围内
    /// </summary>
    public static bool IsTimeRangeInScreenClipRange(float rangeLeft,float rangeRight)
    {
        //完全处于剪裁范围中
        bool isInScreenClipMid = ScreenClipRange.x <= rangeLeft && ScreenClipRange.y >= rangeRight;
        //时间范围左边超出屏幕，右边在屏幕中
        bool isOutScreenClipLeft = rangeLeft < ScreenClipRange.x && rangeRight > ScreenClipRange.x;
        //时间范围右边超出屏幕，左边在屏幕中
        bool isOutScreenClipRight = rangeRight > ScreenClipRange.y && rangeLeft < ScreenClipRange.y;
        if (isInScreenClipMid || isOutScreenClipLeft || isOutScreenClipRight)
            return true;
        return false;
    }

    public static void DrawRecursively(HanoiNode n)
    {
        if (IsTimeRangeInScreenClipRange((float)n.beginTime, (float)n.beginTime + (float)n.timeConsuming))
        {
            int hash = n.GetHashCode();
            Color c;
            if (!m_colors.TryGetValue(hash, out c))
            {
                m_colors[hash] = c = n.GetNodeColor();
            }
            n.renderRect = new Rect((float)n.beginTime, HanoiVars.StackHeight * (HanoiVars.DrawnStackCount - n.stackLevel - 1), (float)n.timeConsuming, HanoiVars.StackHeight);

            Handles.DrawSolidRectangleWithOutline(n.renderRect, c, n.highlighted ? Color.white : c);
        }

        for (int i = 0; i < n.Children.Count; i++)
        {
            DrawRecursively(n.Children[i]);
        }
    }

    public static void DrawLabelsRecursively(HanoiNode n)
    {
        if (n.highlighted)
        {
            if (IsTimeRangeInScreenClipRange(n.renderRect.xMin, n.renderRect.xMin + HanoiVars.LabelBackgroundWidth))
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
        }

        for (int i = 0; i < n.Children.Count; i++)
        {
            DrawLabelsRecursively(n.Children[i]);
        }
    }
}
