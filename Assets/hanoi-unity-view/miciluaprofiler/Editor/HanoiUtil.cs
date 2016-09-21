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
    //显示全局时间时的累积缩进
    static public float GlobalTimeShrinkedAccumulated = 0.0f;
    static public float DrawingShrinkedAccumulated = 0.0f;
    static public float DrawingShrinkedTotal = 0.0f;
    static public int DrawingCounts = 0;
    static Dictionary<int, Color> m_colors = new Dictionary<int, Color>();
    /// 画黑块的个数
    static public int DrawingBlackSpaceNum = 0;
    /// 鼠标X坐标在第几个黑块后面
    static public int MouseXOnBlankSpaceIndex = 0;
    /// 鼠标坐标在黑块中的偏移坐标
    static public float MouseXInBlankSpaceSkewing = 0.0f;
    static public float MouseXInBlankSpaceSkewingAccumulated = 0.0f;

    //全局时间转为修正坐标的修正值
    static public float GlobalTimeXToShrinkedPosXSkewing = 0.0f;
    static public bool ISMouseInBlankSpace = false;
    /// <summary>
    /// 检测鼠标X坐标在第几个黑块后面
    /// 如果鼠标X坐标点击在黑块中，计算鼠标坐标在黑块中的偏移坐标
    /// </summary>
    public static void checkMouseXInScroolWheelSkewing(HanoiNode n,float mouseX)
    {
        if (n is HanoiBlankSpace)
        {
            DrawingBlackSpaceNum++;
            //如果鼠标X在黑块后面
            if (mouseX >= n.beginTime - DrawingShrinkedAccumulated)
            {
                //如果鼠标X在黑块中间
                if (mouseX < n.beginTime - DrawingShrinkedAccumulated + HanoiVars.BlankSpaceWidth)
                {
                    MouseXInBlankSpaceSkewing = (float)n.beginTime - DrawingShrinkedAccumulated - mouseX;
                    return;
                }
                else {
                    MouseXOnBlankSpaceIndex = DrawingBlackSpaceNum;                
                }
            } 
            HanoiUtil.DrawingShrinkedAccumulated += (float)n.timeConsuming - HanoiVars.BlankSpaceWidth;
        }

        if (n.stackLevel == 0)
        {
            for (int i = 0; i < n.Children.Count; i++)
            {
                checkMouseXInScroolWheelSkewing(n.Children[i], mouseX);
            }
        }
    }


    public static void checkMouseXInBlankSpace(HanoiNode n, float mouseX)
    {
        if (n is HanoiBlankSpace)
        {
            //如果鼠标X在黑块后面
            if (mouseX >= n.beginTime - DrawingShrinkedAccumulated)
            {
                //如果鼠标X在黑块中间
                if (mouseX < n.beginTime - DrawingShrinkedAccumulated + HanoiVars.BlankSpaceWidth)
                {
                    ISMouseInBlankSpace = true;
                    return ;
                }
            }
            HanoiUtil.DrawingShrinkedAccumulated += (float)n.timeConsuming - HanoiVars.BlankSpaceWidth;
        }

        if (n.stackLevel == 0)
        {
            for (int i = 0; i < n.Children.Count; i++)
            {
                checkMouseXInBlankSpace(n.Children[i], mouseX);
            }
        }
    }


    public static void checkMouseXInGlobalTimeSkewing(HanoiNode n, float mouseX)
    {
        if (n is HanoiBlankSpace)
        {
            //如果鼠标X在黑块后面
            if (mouseX >= n.beginTime - DrawingShrinkedAccumulated)
            {
                //如果鼠标X在黑块中间
                if (mouseX < n.beginTime - DrawingShrinkedAccumulated + HanoiVars.BlankSpaceWidth)
                {
                    float skewingInBlankSpace = (float)n.beginTime - DrawingShrinkedAccumulated - mouseX;
                    MouseXInBlankSpaceSkewing = skewingInBlankSpace - (skewingInBlankSpace / HanoiVars.BlankSpaceWidth * (float)n.timeConsuming);

                    //黑块首尾时间显示
                    Rect r = new Rect();
                    r.position = new Vector2((float)n.beginTime - DrawingShrinkedAccumulated, 750);
                    r.width = HanoiVars.LabelBackgroundWidth / 3;
                    r.height = 35;
                    Color bg = Color.white;
                    bg.a = 0.5f;
                    Handles.DrawSolidRectangleWithOutline(r, bg, bg);
                    //黑块首尾时间显示
                    
                    Handles.Label(new Vector3((float)n.beginTime - DrawingShrinkedAccumulated, 750), string.Format("{0:0.00}", (float)n.beginTime));
                    Handles.Label(new Vector3((float)(n.beginTime + HanoiVars.LabelBackgroundWidth / 10) - DrawingShrinkedAccumulated, 770), string.Format("{0:0.00}", (float)n.endTime));

                    return;
                }
                else
                {
                    GlobalTimeShrinkedAccumulated += (float)n.timeConsuming - HanoiVars.BlankSpaceWidth;
                }
            }
            HanoiUtil.DrawingShrinkedAccumulated += (float)n.timeConsuming - HanoiVars.BlankSpaceWidth;
        }

        if (n.stackLevel == 0)
        {
            for (int i = 0; i < n.Children.Count; i++)
            {
                checkMouseXInGlobalTimeSkewing(n.Children[i], mouseX);
            }
        }
    }

    public static void getShrinkedPosXByGlobalTimeX(HanoiNode n, float globalTimeX)
    {
        if (n is HanoiBlankSpace)
        {
            //如果鼠标X在黑块中间
            if (globalTimeX > (float)n.beginTime && globalTimeX < (float)n.endTime)
            {
                float beginShrinkedX = (float)n.beginTime - DrawingShrinkedAccumulated;

                float intervalShrinkedX = (globalTimeX - (float)n.beginTime) / (float)n.timeConsuming * HanoiVars.BlankSpaceWidth;

                GlobalTimeXToShrinkedPosXSkewing = beginShrinkedX + intervalShrinkedX;
                HanoiUtil.DrawingShrinkedAccumulated += (float)n.timeConsuming - HanoiVars.BlankSpaceWidth;
                return;
            }
            if (globalTimeX > (float)n.endTime) {
                HanoiUtil.DrawingShrinkedAccumulated += (float)n.timeConsuming - HanoiVars.BlankSpaceWidth;
                return;
            }
        }

        if (n.stackLevel == 0)
        {
            for (int i = 0; i < n.Children.Count; i++)
            {
                getShrinkedPosXByGlobalTimeX(n.Children[i], globalTimeX);
            }
        }
    }

    public static void DataRecursively(HanoiNode n)
    {
        if (n is HanoiBlankSpace)
        {
            HanoiUtil.DrawingShrinkedAccumulated += (float)n.timeConsuming - HanoiVars.BlankSpaceWidth;
            HanoiUtil.DrawingBlackSpaceNum++;
        }
        if (n.stackLevel == 0)
        {
            for (int i = 0; i < n.Children.Count; i++)
            {
                DataRecursively(n.Children[i]);
            }
        }
    }

    public static void DrawBlankSpaceRecursively(HanoiNode n)
    {
        if (n is HanoiBlankSpace)
        {
            Color c = n.GetNodeColor();
            c.a = 0.5f;
            //n.renderRect = new Rect((float)n.beginTime - DrawingShrinkedAccumulated, 0.0f, HanoiVars.BlankSpaceWidth, HanoiVars.StackHeight * (HanoiVars.DrawnStackCount - 1));
            n.renderRect = new Rect((float)n.beginTime - DrawingShrinkedAccumulated, 0.0f, HanoiVars.BlankSpaceWidth, 900);
            HanoiUtil.DrawingShrinkedAccumulated += (float)n.timeConsuming - HanoiVars.BlankSpaceWidth;
            Handles.DrawSolidRectangleWithOutline(n.renderRect, c, c);
            HanoiUtil.DrawingBlackSpaceNum++;
        }

        if (n.stackLevel == 0)
        {
            for (int i = 0; i < n.Children.Count; i++)
            {
                DrawBlankSpaceRecursively(n.Children[i]);
            }
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

        if (n is HanoiBlankSpace)
        {
            HanoiUtil.DrawingShrinkedAccumulated += (float)n.timeConsuming - HanoiVars.BlankSpaceWidth;
        }
        else
        {
            if (n.stackLevel==0)
            {
                //画最底层总时间
                n.renderRect = new Rect((float)n.beginTime, HanoiVars.StackHeight * (HanoiVars.DrawnStackCount - n.stackLevel - 1), TotalTimeConsuming - HanoiUtil.DrawingShrinkedTotal, HanoiVars.StackHeight);
            }
            else {
                n.renderRect = new Rect((float)n.beginTime - DrawingShrinkedAccumulated , HanoiVars.StackHeight * (HanoiVars.DrawnStackCount - n.stackLevel - 1), (float)n.timeConsuming, HanoiVars.StackHeight);            
            }
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
