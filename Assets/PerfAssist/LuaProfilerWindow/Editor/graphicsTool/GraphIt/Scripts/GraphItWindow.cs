using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class GraphItWindowLuaPro : EditorWindow
{
    static Vector2 mScrollPos;
    static float mWidth;
    static int mMouseOverGraphIndex = -1;
    static float mMouseX = 0;

    public static string[] _TimeLimitStrOption = new string[] {"1ms", "5ms", "10ms", "50ms", "100ms" };
    public static int[] _TimeLimitValue = new int[] {1, 5, 10, 50, 100 };
    public static int _TimeLimitSelectIndex = 0;

    public static string[] _PercentLimitStrOption = new string[] { "100%", "20%", "5%" };
    public static int[] _PercentLimitValue = new int[] { 100, 20, 5 };
    public static int _PercentLimitSelectIndex = 0;

    static float x_offset = 5.0f;
    static float XStep = 4;
    public static float y_gap = 15.0f;
    static float y_offset = 0;
    //static int precision_slider = 3;

    static GUIStyle NameLabel;
    static GUIStyle SmallLabel;
    static GUIStyle HoverText;
    static GUIStyle FracGS;

    static Material mLineMaterial;
    static Material mRectMaterial;

    public static float MouseXOnPause = 0;
    public static int   FrameIDOnPause = 0;
    static string num_format = "###,###,###,##0.###";
    static void InitializeStyles()
    {
        if (NameLabel == null)
        {
            NameLabel = new GUIStyle(EditorStyles.whiteBoldLabel);
            NameLabel.normal.textColor = Color.white;
            SmallLabel = new GUIStyle(EditorStyles.whiteLabel);
            SmallLabel.normal.textColor = Color.white;

            HoverText = new GUIStyle(EditorStyles.whiteLabel);
            HoverText.alignment = TextAnchor.UpperRight;
            HoverText.normal.textColor = Color.white;

            FracGS = new GUIStyle(EditorStyles.whiteLabel);
            FracGS.alignment = TextAnchor.LowerLeft;
        }
    }

    static void DrawGraphGridLines(float y_pos, float width, float height, bool draw_mouse_line)
    {
        GL.Begin(GL.LINES);
        GL.Color(new Color(0.3f, 0.3f, 0.3f));
        float steps = 8;
        float x_step = width / steps;
        float y_step = height / steps;
        for (int i = 0; i < steps + 1; ++i)
        {
            Plot(x_offset + x_step * i, y_pos, x_offset + x_step * i, y_pos + height);
            Plot(x_offset, y_pos + y_step * i, x_offset + width, y_pos + y_step * i);
        }

        GL.Color(new Color(0.4f, 0.4f, 0.4f));
        steps = 4;
        x_step = width / steps;
        y_step = height / steps;
        for (int i = 0; i < steps + 1; ++i)
        {
            Plot(x_offset + x_step * i, y_pos, x_offset + x_step * i, y_pos + height);
            Plot(x_offset, y_pos + y_step * i, x_offset + width, y_pos + y_step * i);
        }

        GL.Color(new Color(0.8f, 0.8f, 0.8f));
        Plot(mMouseX, 0, mMouseX, height * 2 + y_gap);

        GL.End();

        if (EditorApplication.isPaused && MouseXOnPause > 0)
        {
            Rect r2 = new Rect();
            r2.position = new Vector2(MouseXOnPause - XStep / 2, 0);
            r2.width = XStep ;
            r2.height = height * 2 + y_gap;
            Color bg = Color.white;
            bg.a = 0.2f;
            Handles.DrawSolidRectangleWithOutline(r2, bg, bg);
        }
    }

    static void Plot(float x0, float y0, float x1, float y1)
    {
        GL.Vertex3(x0, y0, 0);
        GL.Vertex3(x1, y1, 0);
    }

    static void DrawDataRect(float x0, float y0, float x1, float y1, float yBottom, Color c)
    {
        mRectMaterial.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(c);
        GL.Vertex3(x0, y0, 0);
        GL.Vertex3(x1, y1, 0);
        GL.Vertex3(x1, yBottom, 0);
        GL.Vertex3(x0, yBottom, 0);
        GL.End();
    }

    static void CreateLineMaterial()
    {
        if (!mLineMaterial)
        {
            mLineMaterial = new Material(Shader.Find("Custom/GraphIt"));
            mLineMaterial.hideFlags = HideFlags.HideAndDontSave;
            mLineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
        if (!mRectMaterial)
        {
            mRectMaterial = new Material(Shader.Find("Custom/GraphIt2"));
            mRectMaterial.hideFlags = HideFlags.HideAndDontSave;
            mRectMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    public static float getDataHeightMaxLimit(string graphName)
    {
        if (graphName.Equals(HanoiData.GRAPH_TIMECONSUMING))
        {
            if (_TimeLimitSelectIndex >= 0)
                return _TimeLimitValue[_TimeLimitSelectIndex];
        }

        if (graphName.Equals(HanoiData.GRAPH_TIME_PERCENT))
        {
            if (_PercentLimitSelectIndex >= 0)
                return _PercentLimitValue[_PercentLimitSelectIndex];
        }
        return -1;
    }

    public static void DrawGraphs(Rect rect, EditorWindow window)
    {
        if (GraphItLuaPro.Instance)
        {

            //string num_format = "###,###,###,##0.###";
            //for (int i = 0; i < precision_slider; i++)
            //{
            //    num_format += "#";
            //}
            InitializeStyles();
            CreateLineMaterial();

            int graph_index = 0;

            //use this to get the starting y position for the GL rendering
            //Rect find_y = EditorGUILayout.BeginVertical(GUIStyle.none);
            EditorGUILayout.EndVertical();
            float scrolled_y_pos = y_offset - mScrollPos.y;
            float scrolled_x_pos = x_offset - mScrollPos.x;
            if (Event.current.type == EventType.Repaint)
            {
                //Draw Lines
                foreach (KeyValuePair<string, GraphItDataLuaPro> kv in GraphItLuaPro.Instance.Graphs)
                {
                    if (kv.Value.GetHidden())
                    {
                        continue;
                    }
                    graph_index++;

                    float height = kv.Value.GetHeight();
                    mRectMaterial.SetFloat("_BottomLineHeight", scrolled_y_pos + height);
                    mRectMaterial.SetFloat("_LineHeight", height);

                    mRectMaterial.SetFloat("_DataHeightMaxLimit", getDataHeightMaxLimit(kv.Value.mName));
                    mLineMaterial.SetPass(0);
                    DrawGraphGridLines(scrolled_y_pos, mWidth, height, graph_index == mMouseOverGraphIndex);
                    if (kv.Value.GraphLength() > 0)
                    {
                        bool isShowLeftSideTips = true;
                        foreach (KeyValuePair<string, GraphItDataInternalLuaPro> entry in kv.Value.mData)
                        {
                            GraphItDataInternalLuaPro g = entry.Value;

                            float y_min = 0;
                            float y_max = kv.Value.GetMax(entry.Key);
                            y_max = getDataHeightMaxLimit(kv.Value.mName);

                            float y_range = Mathf.Max(y_max - y_min, 0.00001f);
                            GL.Begin(GL.LINES);
                            //draw the 0 line
                            if (y_max > 0.0f && y_min < 0.0f)
                            {
                                GL.Color(g.mColor * 0.5f);
                                float y = scrolled_y_pos + height * (1 - (0.0f - y_min) / y_range);
                                Plot(x_offset, y, x_offset + mWidth, y);
                            }
                            GL.End();
                            float previous_value = 0;
                            int len = kv.Value.GraphLength();
                            for (int i = 0; i < len; ++i)
                            {
                                float value = 0;
                                if (i >= 1)
                                {
                                    float x0 = x_offset + (i - 1) * XStep + scrolled_x_pos;
                                    if (x0 <= -XStep * 2) continue;
                                    if (x0 >= mWidth + XStep * 2) break;
                                    value = g.mDataInfos[i].GraphNum;
                                    float y0 = scrolled_y_pos + height * (1 - (previous_value - y_min) / y_range);

                                    float x1 = x_offset + i * XStep + scrolled_x_pos;
                                    float y1 = scrolled_y_pos + height * (1 - (value - y_min) / y_range);
                                    DrawDataRect(x0, y0, x1, y1, scrolled_y_pos + height, g.mColor);

                                    if (EditorApplication.isPaused && MouseXOnPause > 0 && g.mDataInfos[i].FrameID == FrameIDOnPause)
                                    {
                                        Rect r = new Rect();
                                        r.position = new Vector2(x0 +10, 0);
                                        r.width = 35;
                                        r.height = 15;
                                        Color bg2 = Color.white;
                                        bg2.a = 0.2f;
                                        Handles.DrawSolidRectangleWithOutline(r, bg2, bg2);
                                        GUI.Label(r, FrameIDOnPause.ToString(), HoverText);


                                        Rect r2 = new Rect();
                                        y1 -= 20;
                                        y1=Mathf.Max(y1, 0);
                                        if (isShowLeftSideTips)
                                            x0 -= 60;
                                        else
                                            x0 += 10;
                                        r2.position = new Vector2(x0,y1);
                                        r2.width = 60;
                                        r2.height = 15;
                                        Color bg = g.mColor;
                                        bg.a = 0.5f;
                                        Handles.DrawSolidRectangleWithOutline(r2, bg, bg);
                                        

                                        string afterFix=null;
                                        if (kv.Key.Equals(HanoiData.GRAPH_TIMECONSUMING))
                                        {
                                            afterFix = "ms";
                                        }
                                        else
                                        {
                                            afterFix = "%";
                                        }
                                        string text = value.ToString(num_format) + afterFix;
                                        HoverText.normal.textColor = Color.black;
                                        GUI.Label(r2, text, HoverText);
                                    }
                                }
                                previous_value = value;
                            }
                            isShowLeftSideTips = !isShowLeftSideTips;
                        }
                    }
                    scrolled_y_pos += (height + y_gap);
                }
            }

            graph_index = 0;
            if (Event.current.type == EventType.Repaint)
            {
                mMouseOverGraphIndex = -1; //clear it out every repaint to ensure when the mouse leaves we don't leave the pointer around
            }
            foreach (KeyValuePair<string, GraphItDataLuaPro> kv in GraphItLuaPro.Instance.Graphs)
            {
                if (kv.Value.GetHidden())
                {
                    continue;
                }
                graph_index++;


                mWidth = window.position.width - x_offset;

                float height = kv.Value.GetHeight();
                float width = kv.Value.GraphLength() * XStep;
                if (width < mWidth)
                {
                    width = mWidth - x_offset * 2;
                }
                else
                {
                    if (!EditorApplication.isPaused)
                        mScrollPos.x = width - mWidth;
                }

                Rect r = EditorGUILayout.BeginVertical();
                r.height = height;

                //Determine if we can fit all of the text
                float row_size = 18;
                float text_block_size = row_size * 4;
                if (kv.Value.mData.Count == 1)
                {
                    text_block_size = row_size * 3;
                }
                bool show_full_text = (kv.Value.mData.Count * text_block_size + row_size) < height;

                string fu_str = " " + (kv.Value.mFixedUpdate ? "(FixedUpdate)" : "");
                //skip subgraph title if only one, and it's the same.
                if (show_full_text)
                {
                    NameLabel.normal.textColor = Color.white;
                    EditorGUILayout.LabelField(kv.Key + fu_str, NameLabel);
                }

                foreach (KeyValuePair<string, GraphItDataInternalLuaPro> entry in kv.Value.mData)
                {
                    GraphItDataInternalLuaPro g = entry.Value;
                    if (!show_full_text)
                    {
                        //fit each line manually or drop it
                        height -= row_size;
                        if (height >= 0)
                        {
                            if (kv.Value.mData.Count > 1)
                            {
                                NameLabel.normal.textColor = g.mColor;
                            }
                            else
                            {
                                NameLabel.normal.textColor = Color.white;
                            }
                            string text = entry.Key;
                            if (text == GraphItLuaPro.BASE_GRAPH)
                            {
                                text = kv.Key;
                            }
                            EditorGUILayout.LabelField(text + fu_str, NameLabel);
                        }
                        height -= row_size;
                        if (height >= 0)
                        {
                            EditorGUILayout.LabelField("-" ,SmallLabel);
                        }
                    }
                }

                //Respond to mouse input!
                if (Event.current.type == EventType.MouseDrag && r.Contains(Event.current.mousePosition - Event.current.delta))
                {
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        if (Event.current.button == 1)
                        {
                            mScrollPos.x += Event.current.delta.x;
                            MouseXOnPause = -1;
                        }
                        window.Repaint();
                    }

                }
                else if (Event.current.type != EventType.Layout && r.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.Repaint||(Event.current.type == EventType.MouseDown && Event.current.button == 0))
                    {
                        mMouseOverGraphIndex = graph_index;
                        mMouseX = Event.current.mousePosition.x;
                        float offsetMouseX = mMouseX - x_offset+XStep*2;
                        if (kv.Value.GraphLength() > 0)
                        {
                            foreach (KeyValuePair<string, GraphItDataInternalLuaPro> entry in kv.Value.mData)
                            {
                                GraphItDataInternalLuaPro g = entry.Value;

                                for (int i = 0; i < kv.Value.GraphLength(); ++i)
                                {
                                    if (i >= 1)
                                    {
                                        //float value = g.mDataInfos[i - 1].GraphNum;
                                        float interval = g.mDataInfos[i - 1].FrameInterval;
                                        int frameID = g.mDataInfos[i - 1].FrameID;
                                        float frameTime = g.mDataInfos[i - 1].FrameTime;
                                        float x0 = x_offset + (i - 1) * XStep + scrolled_x_pos;
                                        float x1 = x_offset + i * XStep + scrolled_x_pos ;
                                        if (x0 < offsetMouseX && offsetMouseX <= x1)
                                        {
                                            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                                            {
                                                MouseXOnPause = x0;
                                                FrameIDOnPause = frameID;
                                                LuaProfilerWindow myWindow = (LuaProfilerWindow)EditorWindow.GetWindow(typeof(LuaProfilerWindow));
                                                myWindow.setFrameTimeOnPause(frameTime);
                                                myWindow.setViewPointToGlobalTime(0, interval, mMouseX);
                                                myWindow.m_isTestCallLua = false;
                                                EditorApplication.isPaused = true;
                                                break;
                                            }
                                            else
                                            {
                                                Vector2 position = Event.current.mousePosition + new Vector2(10, -10);
                                                Vector2 size = new Vector2(45, 15);
                                                Rect back = new Rect(position, size);
                                                Color bg = Color.grey;
                                                Handles.DrawSolidRectangleWithOutline(back, bg, bg);

                                                Rect tooltip_r = new Rect(position, size);
                                                HoverText.normal.textColor = Color.white;
                                                GUI.Label(tooltip_r, frameID.ToString(), HoverText);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (Event.current.type == EventType.MouseMove)
                    {
                        window.Repaint();
                    }
                }
                EditorGUILayout.EndVertical();
                float preScrollPosX = mScrollPos.x;
                mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos, GUILayout.Width(mWidth), GUILayout.Height(height + y_gap));
                if (preScrollPosX != mScrollPos.x)
                {
                    MouseXOnPause = -1;
                }
                GUILayout.Label("", GUILayout.Width(width), GUILayout.Height(0));
                EditorGUILayout.EndScrollView();
            }
        }
    }
}