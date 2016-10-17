using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class GraphItWindow : EditorWindow
{
    static Vector2 mScrollPos;
    static float mWidth;
    static int mMouseOverGraphIndex = -1;
    static float mMouseX = 0;

    public static int SelectTimeLimitIndex = -1;

    static float x_offset = 0.0f;
    static float XStep = 5;
    public static float y_gap = 15.0f;
    static float y_offset = 0;
    static int precision_slider = 3;

    static GUIStyle NameLabel;
    static GUIStyle SmallLabel;
    static GUIStyle HoverText;
    static GUIStyle FracGS;

    static Material mLineMaterial;
    static Material mRectMaterial;

    static float mouseXOnLeftBtn = 0;
    
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
        Plot(mMouseX, 0, mMouseX, height * 2 + y_gap * 2);

        GL.End();

        if (EditorApplication.isPaused && mouseXOnLeftBtn >0)
        {
            Rect r2 = new Rect();
            r2.position = new Vector2(mouseXOnLeftBtn,0);
            r2.y = -20;
            r2.width = XStep;
            r2.height = y_pos +height * 2 + y_gap * 2;
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
        mRectMaterial.SetFloat("_BottomLineHeight", yBottom);
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

    public static void DrawGraphs(Rect rect, EditorWindow window)
    {
        if (GraphIt.Instance)
        {
            InitializeStyles();
            CreateLineMaterial();

            int graph_index = 0;

            //use this to get the starting y position for the GL rendering
            Rect find_y = EditorGUILayout.BeginVertical(GUIStyle.none);
            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint)
            {
                //Draw Lines
                float scrolled_y_pos = y_offset - mScrollPos.y;
                float scrolled_x_pos = x_offset - mScrollPos.x;
                foreach (KeyValuePair<string, GraphItData> kv in GraphIt.Instance.Graphs)
                {
                    if (kv.Value.GetHidden())
                    {
                        continue;
                    }
                    graph_index++;

                    float height = kv.Value.GetHeight();
                    mRectMaterial.SetFloat("_LineHeight", height);
                    if (SelectTimeLimitIndex > 0)
                    {
                        mRectMaterial.SetFloat("_DataHeightMaxLimit",height* (6-SelectTimeLimitIndex) * 0.2f);
                    }
                    else {
                        mRectMaterial.SetFloat("_DataHeightMaxLimit", 9999);                    
                    }

                    mLineMaterial.SetPass(0);
                    DrawGraphGridLines(scrolled_y_pos, mWidth, height, graph_index == mMouseOverGraphIndex);
                    if (kv.Value.GraphLength() > 0)
                    {
                        foreach (KeyValuePair<string, GraphItDataInternal> entry in kv.Value.mData)
                        {
                            GraphItDataInternal g = entry.Value;

                            float y_min = 0;
                            float y_max = kv.Value.GetMax(entry.Key);
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
                                float value = g.mDataInfos[i].GraphNum;
                                if (i >= 1)
                                {
                                    float x0 = x_offset + (i - 1) * XStep + scrolled_x_pos;
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
            foreach (KeyValuePair<string, GraphItData> kv in GraphIt.Instance.Graphs)
            {
                if (kv.Value.GetHidden())
                {
                    continue;
                }
                graph_index++;


                mWidth = window.position.width - 2 * x_offset;

                float height = kv.Value.GetHeight();
                float width = kv.Value.GraphLength() * XStep;
                if (width < mWidth)
                {
                    width = mWidth;                
                }
                else {
                    if (!EditorApplication.isPaused)
                         mScrollPos.x = width - mWidth;                
                }
 
                Rect r = EditorGUILayout.BeginVertical(); 

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

                foreach (KeyValuePair<string, GraphItDataInternal> entry in kv.Value.mData)
                {
                    GraphItDataInternal g = entry.Value;
                    if (show_full_text)
                    {
                        if (kv.Value.mData.Count > 1 || entry.Key != GraphIt.BASE_GRAPH)
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
                            if (text == GraphIt.BASE_GRAPH)
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
                    if (Event.current.type == EventType.MouseDrag && Event.current.button == 1)
                    {
                        mScrollPos.x += Event.current.delta.x;
                        mouseXOnLeftBtn = -1;
                    }
                }
                else if (Event.current.type != EventType.Layout && r.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        mMouseOverGraphIndex = graph_index;
                        mMouseX = Event.current.mousePosition.x;
                        float hover_y_offset = 0;
                        if (kv.Value.GraphLength() > 0)
                        {
                            foreach (KeyValuePair<string, GraphItDataInternal> entry in kv.Value.mData)
                            {
                                GraphItDataInternal g = entry.Value;

                                for (int i = 0; i < kv.Value.GraphLength(); ++i)
                                {
                                    float value = g.mDataInfos[i].GraphNum;
                                    float frameTime = g.mDataInfos[i].FrameTime;
                                    if (i >= 1)
                                    {
                                        float x0 = x_offset + (i - 1) * XStep;
                                        float x1 = x_offset + i * XStep;
                                        if (x0 < mMouseX && mMouseX <= x1)
                                        {
                                            string text = value.ToString(num_format);
                                            Rect tooltip_r = new Rect(Event.current.mousePosition - new Vector2(250, 2 - hover_y_offset), new Vector2(250, 20));
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
                        mMouseX = Event.current.mousePosition.x;

                        if (kv.Value.GraphLength() > 0)
                        {
                            foreach (KeyValuePair<string, GraphItDataInternal> entry in kv.Value.mData)
                            {
                                GraphItDataInternal g = entry.Value;

                                for (int i = 0; i < kv.Value.GraphLength(); ++i)
                                {
                                    float value = g.mDataInfos[i].FrameTime;
                                    float interval = g.mDataInfos[i].FrameInterval;
                                    if (i >= 1)
                                    {
                                        float x0 = x_offset + (i - 1) * XStep;
                                        float x1 = x_offset + i * XStep;
                                        if (x0 < mMouseX && mMouseX <= x1)
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
                mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos, GUILayout.Width(mWidth), GUILayout.Height(height+y_gap));
                GUILayout.Label("", GUILayout.Width(width), GUILayout.Height(0));
                EditorGUILayout.EndScrollView();
            }
        }
    }
}