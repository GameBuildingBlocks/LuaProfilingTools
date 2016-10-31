using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class GraphItWindow2 : EditorWindow
{
    static Vector2 mScrollPos;
    static float mWidth;
    static int mMouseOverGraphIndex = -1;
    static float mMouseX = 0;

    public static string[] _TimeLimitStrOption = new string[] { "normal", "100ms", "50ms", "10ms", "5ms", "1ms" };
    public static int[] _TimeLimitValue = new int[] { -1, 100, 50, 10, 5, 1 };
    public static int _TimeLimitSelectIndex = 0;

    public static string[] _PercentLimitStrOption = new string[] {"100%","20%","5%"};
    public static int[] _PercentLimitValue = new int[] {100,20,5};
    public static int _PercentLimitSelectIndex = 0;

    static float x_offset = 5.0f;
    static float XStep = 3;
    public static float y_gap = 15.0f;
    static float y_offset = 0;
    static int precision_slider = 3;

    static GUIStyle NameLabel;
    static GUIStyle SmallLabel;
    static GUIStyle HoverText;
    static GUIStyle FracGS;

    static Material mLineMaterial;
    static Material mRectMaterial;

    public static float mouseXOnLeftBtn = 0;
    
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

        if (EditorApplication.isPaused && mouseXOnLeftBtn >0)
        {
            Rect r2 = new Rect();
            r2.position = new Vector2(mouseXOnLeftBtn-XStep/2,0);
            r2.width = XStep+x_offset;
            r2.height = height * 2 + y_gap;
            Color bg = Color.yellow;
            bg.a = 0.2f;
            Handles.DrawSolidRectangleWithOutline(r2, bg, bg);
        }
    }

    static void Plot(float x0, float y0, float x1, float y1)
    {
        GL.Vertex3(x0, y0, 0);
        GL.Vertex3(x1, y1, 0);
    }

    static void DrawDataRect(float x0, float y0, float x1, float y1,float yBottom,Color c)
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
        if (!mRectMaterial) {
            mRectMaterial = new Material(Shader.Find("Custom/GraphIt2"));
            mRectMaterial.hideFlags = HideFlags.HideAndDontSave;
            mRectMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    public static float getDataHeightMaxLimit(string graphName) {
        if (graphName.Equals(HanoiData.GRAPH_TIMECONSUMING))
        {
            if(_TimeLimitSelectIndex >=0 )
                return _TimeLimitValue[_TimeLimitSelectIndex];
        }

        if (graphName.Equals(HanoiData.GRAPH_TIME_PERCENT))
        {
            if(_PercentLimitSelectIndex>=0)
                return _PercentLimitValue[_PercentLimitSelectIndex];
        }
        return -1;
    }

    public static void DrawGraphs(Rect rect, EditorWindow window)
    {
        if (GraphIt2.Instance)
        {
            InitializeStyles();
            CreateLineMaterial();

            int graph_index = 0;

            //use this to get the starting y position for the GL rendering
            Rect find_y = EditorGUILayout.BeginVertical(GUIStyle.none);
            EditorGUILayout.EndVertical();
                            float scrolled_y_pos = y_offset - mScrollPos.y;
                float scrolled_x_pos = x_offset - mScrollPos.x;
            if (Event.current.type == EventType.Repaint)
            {
                //Draw Lines
                foreach (KeyValuePair<string, GraphItData2> kv in GraphIt2.Instance.Graphs)
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
                        foreach (KeyValuePair<string, GraphItDataInternal2> entry in kv.Value.mData)
                        {
                            GraphItDataInternal2 g = entry.Value;

                            float y_min = 0;
                            float y_max = kv.Value.GetMax(entry.Key);
                            if (getDataHeightMaxLimit(kv.Value.mName)>0)
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
                            for (int i = 0; i < kv.Value.GraphLength(); ++i)
                            {
                                float value = 0;
                                if (i >= 1)
                                {
                                    float x0 = x_offset + (i - 1) * XStep + scrolled_x_pos;
                                    if (x0 <= -XStep*2) continue;
                                    if (x0 >= mWidth + XStep*2) break;
                                    value = g.mDataInfos[i].GraphNum;
                                    float y0 = scrolled_y_pos + height * (1 - (previous_value - y_min) / y_range);

                                    float x1 = x_offset + i * XStep + scrolled_x_pos;
                                    float y1 = scrolled_y_pos + height * (1 - (value - y_min) / y_range);
                                    DrawDataRect(x0, y0, x1, y1, scrolled_y_pos+height,g.mColor);
                                }
                                previous_value = value;
                            }
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
            foreach (KeyValuePair<string, GraphItData2> kv in GraphIt2.Instance.Graphs)
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
                    width = mWidth - x_offset*2;                
                }
                else {
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

                string num_format = "###,###,###,##0.";
                for (int i = 0; i < precision_slider; i++)
                {
                    num_format += "#";
                }

                string fu_str = " " + (kv.Value.mFixedUpdate ? "(FixedUpdate)" : "");
                //skip subgraph title if only one, and it's the same.
                if (show_full_text)
                {
                    NameLabel.normal.textColor = Color.white;
                    EditorGUILayout.LabelField(kv.Key + fu_str, NameLabel);
                }

                foreach (KeyValuePair<string, GraphItDataInternal2> entry in kv.Value.mData)
                {
                    GraphItDataInternal2 g = entry.Value;
                    if (show_full_text)
                    {
                        if (kv.Value.mData.Count > 1 || entry.Key != GraphIt2.BASE_GRAPH)
                        {
                            NameLabel.normal.textColor = g.mColor;
                            EditorGUILayout.LabelField(entry.Key, NameLabel);
                        }
                        EditorGUILayout.LabelField("Avg: " + g.mAvg.ToString(num_format) + " (" + g.mFastAvg.ToString(num_format) + ")", SmallLabel);
                        EditorGUILayout.LabelField("Min: " + g.mMin.ToString(num_format), SmallLabel);
                        EditorGUILayout.LabelField("Max: " + g.mMax.ToString(num_format), SmallLabel);
                    }
                    else
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
                            if (text == GraphIt2.BASE_GRAPH)
                            {
                                text = kv.Key;
                            }
                            EditorGUILayout.LabelField(text + fu_str, NameLabel);
                        }
                        height -= row_size;
                        if (height >= 0)
                        {
                            EditorGUILayout.LabelField("Avg: " + g.mAvg.ToString(num_format) + " (" + g.mFastAvg.ToString(num_format) +
                            ")  Min: " + g.mMin.ToString(num_format) +
                            "  Max: " + g.mMax.ToString(num_format)
                            , SmallLabel);
                        }
                    }
                }

                //Respond to mouse input!
                if (Event.current.type == EventType.MouseDrag && r.Contains(Event.current.mousePosition - Event.current.delta))
                {
                    if (Event.current.type == EventType.MouseDrag )
                    {
                        if (Event.current.button == 1)
                        {
                            mScrollPos.x += Event.current.delta.x;
                            mouseXOnLeftBtn = -1;
                        }
                        window.Repaint();
                    }

                }
                else if (Event.current.type != EventType.Layout && r.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        mMouseOverGraphIndex = graph_index;
                        mMouseX = Event.current.mousePosition.x;
                        float offsetMouseX = mMouseX - x_offset + XStep;
                        float hover_y_offset = 0;
                        if (kv.Value.GraphLength() > 0)
                        {
                            foreach (KeyValuePair<string, GraphItDataInternal2> entry in kv.Value.mData)
                            {
                                GraphItDataInternal2 g = entry.Value;

                                for (int i = 0; i < kv.Value.GraphLength(); ++i)
                                {
                                    if (i >= 1)
                                    {
                                        float value = g.mDataInfos[i - 1].GraphNum;
                                        float frameTime = g.mDataInfos[i - 1].FrameTime;
                                        float x0 = x_offset + (i - 1) * XStep + scrolled_x_pos;
                                        float x1 = x_offset + i * XStep + scrolled_x_pos;
                                        if (x0 < offsetMouseX && offsetMouseX <= x1)
                                        {
                                            Vector2 position = Event.current.mousePosition + new Vector2(10, -10 + hover_y_offset);
                                            Vector2 size = new Vector2(110, 15);
                                            Rect back = new Rect(position,size);
                                            Color bg = Color.grey;
                                            Handles.DrawSolidRectangleWithOutline(back, bg, bg);

                                            string text = value.ToString(num_format);
                                            Rect tooltip_r = new Rect(position, size);
                                            HoverText.normal.textColor = g.mColor;
                                            GUI.Label(tooltip_r, text + "||" + frameTime, HoverText);

                                            hover_y_offset += 13;
                                            break;
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

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        mMouseOverGraphIndex = graph_index;
                        mMouseX = Event.current.mousePosition.x ;
                        float offsetMouseX = mMouseX - x_offset;
                        if (kv.Value.GraphLength() > 0)
                        {
                            foreach (KeyValuePair<string, GraphItDataInternal2> entry in kv.Value.mData)
                            {
                                GraphItDataInternal2 g = entry.Value;

                                for (int i = 0; i < kv.Value.GraphLength(); ++i)
                                {
                                    if (i >= 1)
                                    {
                                        float value = g.mDataInfos[i-1].FrameTime;
                                        float interval = g.mDataInfos[i-1].FrameInterval;

                                        float x0 = x_offset + (i - 1) * XStep + scrolled_x_pos;
                                        float x1 = x_offset + i * XStep + scrolled_x_pos;

                                        if (x0 < offsetMouseX && offsetMouseX <= x1)
                                        {
                                            mouseXOnLeftBtn = x0;
                                            VisualizerWindow myWindow = (VisualizerWindow)EditorWindow.GetWindow(typeof(VisualizerWindow));
                                            myWindow.setViewPointToGlobalTime(value, interval, mMouseX);
                                            myWindow.m_isTestCallLua = false;
                                            EditorApplication.isPaused = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                float preScrollPosX = mScrollPos.x;
                mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos, GUILayout.Width(mWidth), GUILayout.Height(height + y_gap));
                if (preScrollPosX != mScrollPos.x)
                {
                    mouseXOnLeftBtn = -1;
                }
                GUILayout.Label("", GUILayout.Width(width), GUILayout.Height(0));
                EditorGUILayout.EndScrollView();
            }
        }
    }
}