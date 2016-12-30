using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class  DataInfo{
    private float m_graphNum;
    private float m_frameTime;
    private float m_frameInterval;
    private float m_luaConsuming;
    private float m_funConsuming;
    private int m_frameID;
    public DataInfo(float graphNum)
    {
        m_graphNum = graphNum;
    }
    public DataInfo(float graphNum, float frameTime)
    {
        m_graphNum = graphNum;
        m_frameTime = frameTime;
    }

    public DataInfo(float graphNum, float frameTime, float frameInterval)
    {
        m_graphNum = graphNum;
        m_frameTime = frameTime;
        m_frameInterval = frameInterval;
    }

    public DataInfo(float graphNum, float frameTime, float frameInterval, int frameID)
    {
        m_graphNum = graphNum;
        m_frameTime = frameTime;
        m_frameInterval = frameInterval;
        m_frameID = frameID;
    }

    public float FunConsuming
    {
        get
        {
            return m_funConsuming;
        }

        set { m_funConsuming = value; }
    }

    public float LuaConsuming
    {
        get
        {
            return m_luaConsuming;
        }

        set { m_luaConsuming = value; }
    }

    public float GraphNum
    {
        get
        {
            return m_graphNum;
        }

        set { m_graphNum = value; }
    }

    public float FrameTime
    {
        get
        {
            return m_frameTime;
        }

        set { m_frameTime = value; }
    }

    public float FrameInterval
    {
        get
        {
            return m_frameInterval;
        }

        set { m_frameInterval = value; }
    }

    public int FrameID
    {
        get
        {
            return m_frameID;
        }

        set { m_frameID = value; }
    }
}

public class GraphItDataInternalLuaPro
{
    public GraphItDataInternalLuaPro( int subgraph_index )
    {
        mDataInfos = new List<DataInfo>();
        mMin = 0.0f;
        mMax = 0.0f;

        switch(subgraph_index)
        {
            case 0:
                mColor = new Color( 0, 0.85f, 1, 1);
                break;
            case 1:
                mColor = Color.yellow;
                break;
            case 2:
                mColor = Color.green;
                break;
            case 3:
                mColor = Color.red;
                break;
            default:
                mColor = Color.green;
                break;
        }
    }
    public List<DataInfo> mDataInfos;
    public float mMin;
    public float mMax;
    public Color mColor;
}

public class GraphItDataLuaPro
{
    public const int DEFAULT_SAMPLES = 2048;
    public const int RECENT_WINDOW_SIZE = 120;
    
    public Dictionary<string, GraphItDataInternalLuaPro> mData = new Dictionary<string, GraphItDataInternalLuaPro>();

    public string mName;

    public int mCurrentIndex;
    public bool mInclude0;

    public bool mReadyForUpdate;
    public bool mFixedUpdate;

    public int mWindowSize;

    public bool mSharedYAxis;

    protected bool mHidden;
    protected float mHeight;


    public GraphItDataLuaPro( string name)
    {
        mName = name;

        mData = new Dictionary<string, GraphItDataInternalLuaPro>();

        mCurrentIndex = 0;
        mInclude0 = true;

        mReadyForUpdate = false;
        mFixedUpdate = false;

        mWindowSize = DEFAULT_SAMPLES;

        mSharedYAxis = false;
        mHidden = false;
        mHeight = 175;


        if (PlayerPrefs.HasKey(mName + "_height"))
        {
            SetHeight(PlayerPrefs.GetFloat(mName + "_height"));
        }
        if (PlayerPrefs.HasKey(mName + "_hidden"))
        {
            SetHidden(PlayerPrefs.GetInt(mName + "_hidden")==1);
        }
    }

    public int GraphLength()
    {
        return mCurrentIndex / mData.Count;
    }

    public float GetMin( string subgraph )
    {
        if (mSharedYAxis)
        {
            bool min_set = false;
            float min = 0;
            foreach (KeyValuePair<string, GraphItDataInternalLuaPro> entry in mData)
            {
                GraphItDataInternalLuaPro g = entry.Value;
                if (!min_set)
                {
                    min = g.mMin;
                    min_set = true;
                }
                min = Math.Min(min, g.mMin);
            }
            return min;
        }
        else
        {
            if (!mData.ContainsKey(subgraph))
            {
                mData[subgraph] = new GraphItDataInternalLuaPro(mData.Count);
            }
            return mData[subgraph].mMin;
        }
    }

    public float GetMax( string subgraph )
    {
        if (mSharedYAxis)
        {
            bool max_set = false;
            float max = 0;
            foreach (KeyValuePair<string, GraphItDataInternalLuaPro> entry in mData)
            {
                GraphItDataInternalLuaPro g = entry.Value;
                if (!max_set)
                {
                    max = g.mMax;
                    max_set = true;
                }
                max = Math.Max(max, g.mMax);
            }
            return max;
        }
        else
        {
            if (!mData.ContainsKey(subgraph))
            {
                mData[subgraph] = new GraphItDataInternalLuaPro(mData.Count);
            }
            return mData[subgraph].mMax;
        }
    }

    public float GetHeight()
    {
        return mHeight;
    }
    public void SetHeight( float height )
    {
        mHeight = height;
    }
    public void DoHeightDelta(float delta)
    {
        SetHeight( Mathf.Max(mHeight + delta, 50) );
        PlayerPrefs.SetFloat( mName+"_height", GetHeight() );
    }

    public bool GetHidden()
    {
        return mHidden;
    }
    public void SetHidden(bool hidden)
    {
        mHidden = hidden;
        PlayerPrefs.SetInt(mName + "_hidden", GetHidden() ? 1 : 0 );
    }

}

public class GraphItLuaPro : MonoBehaviour
{

#if UNITY_EDITOR
    public const string BASE_GRAPH = "base";
    public const string VERSION = "1.2.0";
    public Dictionary<string, GraphItDataLuaPro> Graphs = new Dictionary<string, GraphItDataLuaPro>();
    static GraphItLuaPro mInstance = null;
#endif

    public static GraphItLuaPro Instance
    {
        get
        {
#if UNITY_EDITOR
            if( mInstance == null )
            {
                GameObject go = new GameObject("GraphIt");
                go.hideFlags = HideFlags.HideAndDontSave;
                mInstance = go.AddComponent<GraphItLuaPro>();
            }
            return mInstance;
#else
            return null;
#endif
        }
    }

    public static void Clear()
    {
        if (!mInstance) 
            return;
        foreach (KeyValuePair<string, GraphItDataLuaPro> kv in mInstance.Graphs)
        {
            GraphItDataLuaPro g = kv.Value;
            g.mCurrentIndex = 0;
            foreach (KeyValuePair<string, GraphItDataInternalLuaPro> entry in g.mData)
            {
                GraphItDataInternalLuaPro gdi = entry.Value;
                gdi.mDataInfos.Clear();
            }
        }
    }

    /// <summary>
    /// Optional setup function that allows you to specify both the inclusion of Y-axis 0, and how many samples to track.
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="include_0"></param>
    /// <param name="sample_window"></param>
    public static void GraphSetup(string graph, bool include_0, int sample_window)
    {
#if UNITY_EDITOR
        GraphSetupInclude0(graph, include_0);
        GraphSetupSampleWindowSize(graph, sample_window);
#endif
    }
    
    /// <summary>
    /// Optional setup function that allows you to specify both the inclusion of Y-axis 0.
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="subgraph"></param>
    /// <param name="include_0"></param>
    public static void GraphSetupInclude0(string graph, bool include_0)
    {
#if UNITY_EDITOR
        if (!Instance.Graphs.ContainsKey(graph))
        {
            Instance.Graphs[graph] = new GraphItDataLuaPro(graph);
        }

        GraphItDataLuaPro g = Instance.Graphs[graph];
        g.mInclude0 = include_0;
#endif
    }

    /// <summary>
    /// Optional setup function that allows you to specify the initial height of a graph.
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="subgraph"></param>
    /// <param name="height"></param>
    public static void GraphSetupHeight(string graph, float height)
    {
#if UNITY_EDITOR
        if (!Instance.Graphs.ContainsKey(graph))
        {
            Instance.Graphs[graph] = new GraphItDataLuaPro(graph);
        }

        GraphItDataLuaPro g = Instance.Graphs[graph];
        g.SetHeight(height);
#endif
    }

    /// <summary>
    /// Optional setup function that allows you to specify if the graph is hidden or not by default
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="subgraph"></param>
    /// <param name="include_0"></param>
    public static void GraphSetupHidden(string graph, bool hidden)
    {
#if UNITY_EDITOR
        if (!Instance.Graphs.ContainsKey(graph))
        {
            Instance.Graphs[graph] = new GraphItDataLuaPro(graph);
        }

        GraphItDataLuaPro g = Instance.Graphs[graph];
        g.SetHidden(hidden);
#endif
    }
    

    /// <summary>
    /// Optional setup function that allows you to specify how many samples to track.
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="sample_window"></param>
    public static void GraphSetupSampleWindowSize(string graph, int sample_window)
    {
#if UNITY_EDITOR
        if (!Instance.Graphs.ContainsKey(graph))
        {
            Instance.Graphs[graph] = new GraphItDataLuaPro(graph);
        }

        GraphItDataLuaPro g = Instance.Graphs[graph];
        int samples = Math.Max(sample_window, GraphItDataLuaPro.RECENT_WINDOW_SIZE + 1);
        g.mWindowSize = samples;
        //foreach (KeyValuePair<string, GraphItDataInternalLuaPro> entry in g.mData)
        //{
            //GraphItDataInternalLuaPro _g = entry.Value;
            //_g.mDataPoints = new float[samples];
       // }
#endif
    }

    /// <summary>
    /// Optional setup function that allows you to specify the color of the graph.
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="color"></param>
    public static void GraphSetupColour(string graph, Color color)
    {
#if UNITY_EDITOR
        GraphSetupColour(graph, BASE_GRAPH, color);
#endif
    }

    /// <summary>
    /// Optional setup function that allows you to specify the color of the subgraph.
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="subgraph"></param>
    /// <param name="color"></param>
    public static void GraphSetupColour(string graph, string subgraph, Color color)
    {
#if UNITY_EDITOR
        if (!Instance.Graphs.ContainsKey(graph))
        {
            Instance.Graphs[graph] = new GraphItDataLuaPro(graph);
        }

        GraphItDataLuaPro g = Instance.Graphs[graph];
        if (!g.mData.ContainsKey(subgraph))
        {
            g.mData[subgraph] = new GraphItDataInternalLuaPro(g.mData.Count);
        }
        g.mData[subgraph].mColor = color;
#endif
    }

    /// <summary>
    /// Log floating point data for this frame. Mutiple calls to this with the same graph will add logged values together.
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="subgraph"></param>
    /// <param name="f"></param>
    public static void Log(string graph, string subgraph, List<DataInfo> diList)
    {
#if UNITY_EDITOR
        if (!Instance.Graphs.ContainsKey(graph))
        {
            Instance.Graphs[graph] = new GraphItDataLuaPro(graph);
        }

        GraphItDataLuaPro g = Instance.Graphs[graph];
        if (!g.mData.ContainsKey(subgraph))
        {
            g.mData[subgraph] = new GraphItDataInternalLuaPro(g.mData.Count);
        }
        foreach (var di in diList)
        {
            g.mData[subgraph].mDataInfos.Add(di);
        }
        Instance.Graphs[graph].mCurrentIndex += diList.Count;
#endif
    }

    /// <summary>
    /// Allows you to switch between sharing the y-axis on a graph for all subgraphs, or for them to be independent.
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="shared_y_axis"></param>
    public static void ShareYAxis(string graph, bool shared_y_axis)
    {
#if UNITY_EDITOR
        if (!Instance.Graphs.ContainsKey(graph))
        {
            Instance.Graphs[graph] = new GraphItDataLuaPro(graph);
        }

        GraphItDataLuaPro g = Instance.Graphs[graph];
        g.mSharedYAxis = shared_y_axis;
#endif
    }
}
