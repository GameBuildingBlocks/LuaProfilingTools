using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public delegate void HanoiNodeAction(HanoiNode n);

public class HanoiUtil 
{
    public static string[] SelectVaildJsonFolders(string[]  folders)
    {
        List<string> result= new List<string>(); 
        foreach(string folder in folders){
            string[] filePaths = Lua.Instance.GetProfilerFiles(folder);
            if (filePaths.Length > 0)
            {
                result.Add(folder);
            }
        }
        return result.ToArray();
    }

    public static string[] GetVaildJsonFolders()
    {
        return  SelectVaildJsonFolders(Lua.Instance.GetProfilerFolders());
    }

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

    static public float ScreenClipMinX = 0.0f;
    static public float ScreenClipMaxX = 0.0f;

    /// <summary>
    /// 检测一个时间范围，是否在屏幕剪裁范围内
    /// </summary>
    public static bool IsTimeRangeInScreenClipRange(float rangeLeft,float rangeRight)
    {
        //完全处于剪裁范围中
        bool isInScreenClipMid = ScreenClipMinX <= rangeLeft && ScreenClipMaxX >= rangeRight;
        //时间范围左边超出屏幕，右边在屏幕中
        bool isOutScreenClipLeft = rangeLeft < ScreenClipMinX && rangeRight > ScreenClipMinX;
        //时间范围右边超出屏幕，左边在屏幕中
        bool isOutScreenClipRight = rangeRight > ScreenClipMaxX && rangeLeft < ScreenClipMaxX;
        if (isInScreenClipMid || isOutScreenClipLeft || isOutScreenClipRight)
            return true;
        return false;
    }


    public static float calculateTotalTimeConsuming(HanoiNode n)
    {
        int count = n.Children.Count;
        for (int i = count-1; i>0; i--)
        {
            HanoiNode node = n.Children[i];
            if (!(node is HanoiFrameInfo))
            {
                return (float)node.endTime;
            }
        }
        return 0;
    }

    public static void CalculateFrameInterval(HanoiNode n, HanoiNode preN)
    {
        int preFrameIndex = 0;
        float preFrameStartTime=0.0f;
        int count = n.Children.Count;
        for (int i = 0; i < count;i++)
        {
            HanoiNode node =n.Children[i];
            if (node is HanoiFrameInfo)
            {
                HanoiFrameInfo hfi = (HanoiFrameInfo)node;
                if (preFrameStartTime>0)
                {
                    HanoiFrameInfo hfiChild=(HanoiFrameInfo)n.Children[preFrameIndex];
                    //只记录相邻帧的前帧结束时间，非相邻帧信息不显示
                    if (hfi.frameID - hfiChild.frameID == 1)
                    {
                        hfiChild.frameEndTime = hfi.frameTime;
                    }
                }
                preFrameIndex = i;
                preFrameStartTime = hfi.frameTime;
            }
        }
    }

    public static void DrawSelectedFrameInfoRecursively(HanoiNode n,float mouseX)
    {
        if (n is HanoiFrameInfo)
        {
            HanoiFrameInfo hfi = (HanoiFrameInfo)n;
            Color preColor = Handles.color;

            if (mouseX >= hfi.frameTime && mouseX <= hfi.frameEndTime)
            {
                float beginPosX = hfi.frameTime;
                Rect r = new Rect();
                r.position = new Vector2(beginPosX, 0);
                r.width = HanoiVars.LabelBackgroundWidth / 1.5f;
                r.height = 60;
                Color bg = Color.white;
                bg.a = 0.5f;
                Handles.DrawSolidRectangleWithOutline(r, bg, bg);

                Handles.color = Color.green;
                GUI.color = Color.black;
                Handles.Label(new Vector3(beginPosX, 0), string.Format("frameID:{0:0}", hfi.frameID));
                Handles.Label(new Vector3(beginPosX, 15), string.Format("frameTime:{0:0.00}", hfi.frameTime));
                Handles.Label(new Vector3(beginPosX, 30), string.Format("frameUnityTime:{0:0.00}", hfi.frameUnityTime));
                Handles.Label(new Vector3(beginPosX, 45), string.Format("frameInterval:{0:0.00}", hfi.frameEndTime-hfi.frameTime));
                Handles.DrawLine(new Vector3(hfi.frameTime, 0), new Vector3(hfi.frameTime, VisualizerWindow.m_winHeight));
                Handles.DrawLine(new Vector3(hfi.frameEndTime, 0), new Vector3(hfi.frameEndTime, VisualizerWindow.m_winHeight));
            }
          
            Handles.color = preColor;
        }

        for (int i = 0; i < n.Children.Count; i++)
        {
            DrawSelectedFrameInfoRecursively(n.Children[i],mouseX);
        }
    }

    public static void DrawFrameStatementRecursively(HanoiNode n)
    {
        if (n is HanoiFrameInfo)
        {
            HanoiFrameInfo hfi = (HanoiFrameInfo)n;
            Color preColor = Handles.color;
            Handles.color = Color.gray;
            if (IsTimeRangeInScreenClipRange((float)hfi.frameTime, (float)hfi.frameTime))
            {
                Handles.DrawLine(new Vector3(hfi.frameTime, 0), new Vector3(hfi.frameTime, VisualizerWindow.m_winHeight));
            }
            Handles.color = preColor;
        }

        for (int i = 0; i < n.Children.Count; i++)
        {
            DrawFrameStatementRecursively(n.Children[i]);
        }
    }

    public static void DrawRecursively(HanoiNode n)
    {
        int hash = n.GetHashCode();
        Color c;
        if (!m_colors.TryGetValue(hash, out c))
        {
            m_colors[hash] = c = n.GetNodeColor();
        }
        if (n is HanoiFrameInfo)
        {
        }
        else {
            if (IsTimeRangeInScreenClipRange((float)n.beginTime , (float)n.beginTime+ (float)n.timeConsuming))
            {
                n.renderRect = new Rect((float)n.beginTime,HanoiVars.StackHeight * (HanoiVars.DrawnStackCount - n.stackLevel)
                    , (float)n.timeConsuming, HanoiVars.StackHeight);
                Handles.DrawSolidRectangleWithOutline(n.renderRect, c, n.highlighted ? Color.white : c);
            }
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
