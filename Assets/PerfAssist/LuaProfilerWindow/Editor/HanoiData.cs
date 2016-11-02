using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEditor;
public enum eHanoiCallType
{
    None,
    C,
    Lua,
}

public class HanoiRoot
{
    public int totalCalls = 0;
    public double starttime = 0.0f;
    public double stoptime = 0.0f;
    public float totalCalltimeConsuming = 0.0f;
    public float processTimeConsuming = 0.0f;
    public HanoiNode callStats;
}
public class ResovleSessionJsonResult
{
    private List<HanoiNode> m_detailResult;
    private Dictionary<string, List<DataInfo>> m_navigateResult;
    public ResovleSessionJsonResult()
    {
    }

    public List<HanoiNode> DetailResult
    {
        get
        {
            return m_detailResult;
        }

        set { m_detailResult = value; }
    }

    public Dictionary<string, List<DataInfo>> NavigateResult
    {
        get
        {
            return m_navigateResult;
        }

        set { m_navigateResult = value; }
    }
}

public class HanoiNode
{
    public static int s_count = 0;
    public HanoiNode(HanoiNode parent)
    {
        s_count++;

        Parent = parent;
    }

    public virtual void processJsonObject(JSONObject job, HanoiData hd)
    {
        for (int i = 0; i < job.keys.Count; i++)
        {
            JSONObject val = job.list[i];
            switch (job.keys[i])
            {
                case "currentLine":
                    if (val.IsNumber)
                    {
                        currentLine = (int)val.n;
                    }
                    break;
                case "lineDefined":
                    if (val.IsNumber)
                    {
                        lineDefined = (int)val.n;
                    }
                    break;
                case "timeConsuming":
                    if (val.IsNumber)
                    {
                        timeConsuming = val.n;
                    }
                    break;
                case "stackLevel":
                    if (val.IsNumber)
                    {
                        stackLevel = (int)val.n;
                        if (stackLevel > hd.m_maxStackLevel)
                        {
                            hd.m_maxStackLevel = stackLevel;
                        }
                    }
                    break;
                case "callType":
                    if (val.IsString)
                    {
                        string type = val.str;
                        if (type.Equals("C"))
                            callType = eHanoiCallType.C;
                        if (type.Equals("Lua"))
                            callType = eHanoiCallType.Lua;
                    }
                    break;
                case "begintime":
                    if (val.IsNumber)
                    {
                        beginTime = val.n;
                    }
                    break;
                case "endtime":
                    if (val.IsNumber)
                    {
                        endTime = val.n;
                    }
                    break;
                case "moduleName":
                    if (val.IsString)
                    {
                        moduleName = val.str;
                    }
                    break;
                case "funcName":
                    if (val.IsString)
                    {
                        funcName = val.str;
                    }
                    break;
                case "children":
                    foreach (JSONObject childJson in val.list)
                    {
                        HanoiNode child = new HanoiNode(this);
                        if (hd.readObject(childJson, child))
                        {
                            Children.Add(child);
                        }
                    }
                    break;
                default:
                    // Debug.LogFormat("unknown field: {0}", obj.keys[i]);
                    break;
            }
        }
    }
    public string moduleName = "";
    public string funcName = "";

    public int stackLevel = HanoiConst.BAD_NUM;

    public int currentLine = HanoiConst.BAD_NUM;
    public int lineDefined = HanoiConst.BAD_NUM;

    public eHanoiCallType callType = eHanoiCallType.None;
    public double timeConsuming ;
    public double beginTime;
    public double endTime;

    public HanoiNode Parent;
    public List<HanoiNode> Children = new List<HanoiNode>();

    // rendering properties
    public bool HasValidRect() { return renderRect.width > 0 && renderRect.height > 0; }
    public Rect renderRect;

    public bool highlighted = false;

    public Color GetNodeColor()
    {
        if (callType == eHanoiCallType.C)
            return HanoiConst.GetDyeColor(DyeType.CFunc);

        if (moduleName.StartsWith("@Lua"))
            return HanoiConst.GetDyeColor(DyeType.LuaInFile);

        if (moduleName.StartsWith("\\n"))
            return HanoiConst.GetDyeColor(DyeType.LuaMemBytes);

        return HanoiConst.GetDyeColor(DyeType.Default);
    }
}

public class HanoiFrameInfo : HanoiNode
{
    public int frameID = 0;
    public float frameTime = 0.0f;
    public float frameUnityTime = 0.0f;
    public float frameEndTime = 0.0f;
    public float frameLuaTime = 0.0f;
    public float frameFunTime = 0.0f;
    public bool isHandle = false;

    public override  void processJsonObject(JSONObject job, HanoiData hd)
    {
        for (int i = 0; i < job.keys.Count; i++)
        {
            JSONObject val = job.list[i];
            switch (job.keys[i])
            {
                case "frameTime":
                    if (val.IsNumber)
                    {
                        frameTime = val.f;
                    }
                    break;
                case "frameUnityTime":
                    if (val.IsNumber)
                    {
                        frameUnityTime = val.f;
                    }
                    break;
                case "frameID":
                    if (val.IsNumber)
                    {
                        frameID = (int)val.n;
                    }
                    break;
                case "luaConsuming":
                    if (val.IsNumber)
                    {
                        frameLuaTime = val.f;
                    }
                    break;
                case "funConsuming":
                    if (val.IsNumber)
                    {
                        frameFunTime = val.f;
                    }
                    break;
                default:
                    // Debug.LogFormat("unknown field: {0}", obj.keys[i]);
                    break;
            }
        }
    }

    public HanoiFrameInfo(HanoiNode parent)
        : base(parent)
    {
    }
}

public class HanoiData 
{
    public HanoiRoot Root {
        get { return m_hanoiData; } }

    public  bool isHanoiDataLoadSucc(){ 
        return (m_hanoiData !=null);
    }

    public bool isHanoiDataHasContent()
    {
        return (m_hanoiData!=null &&m_hanoiData.callStats != null && m_hanoiData.callStats.Children.Count > 0);
    }

    public int MaxStackLevel { get { return m_maxStackLevel; } }
    public int m_maxStackLevel = 0;

    JSONObject m_json;
    public HanoiRoot m_hanoiData;

    public static string GRAPH_TIMECONSUMING = "timeConsuming";
    public static string SUBGRAPH_LUA_TIMECONSUMING_EXCLUSIVE = "luaTimeConsumingExclusive";
    public static string SUBGRAPH_LUA_TIMECONSUMING_INCLUSIVE = "luaTimeConsumingInclusive";

    public static string GRAPH_TIME_PERCENT = "timePercent";
    public static string SUBGRAPH_LUA_PERCENT_EXCLUSIVE = "luaTimePercentExclusive";
    public static string SUBGRAPH_LUA_PERCENT_INCLUSIVE = "luaTimePercentInclusive";

    public bool Load(string filename)
    {
        m_hanoiData = null;
        GraphItLuaPro.Clear();
        try
        {
            string text = System.IO.File.ReadAllText(filename);
            HanoiNode.s_count = 0;

            m_hanoiData = new HanoiRoot();
            m_hanoiData.callStats = new HanoiNode(null);

            //invaild json doc ,convert correct;
            string templateJsonText = "[$$]";
            var resovleSessionJsonResult = handleSessionJsonObj(new JSONObject(templateJsonText.Replace("$$", text)));
            if (resovleSessionJsonResult != null)
            {
                m_hanoiData.callStats.Children.AddRange(resovleSessionJsonResult.DetailResult);
                var dataInfoMap = resovleSessionJsonResult.NavigateResult;
                GraphItLuaPro.Log(HanoiData.GRAPH_TIMECONSUMING, HanoiData.SUBGRAPH_LUA_TIMECONSUMING_INCLUSIVE, dataInfoMap[HanoiData.SUBGRAPH_LUA_TIMECONSUMING_INCLUSIVE]);
                GraphItLuaPro.Log(HanoiData.GRAPH_TIMECONSUMING, HanoiData.SUBGRAPH_LUA_TIMECONSUMING_EXCLUSIVE, dataInfoMap[HanoiData.SUBGRAPH_LUA_TIMECONSUMING_EXCLUSIVE]);
                GraphItLuaPro.Log(HanoiData.GRAPH_TIME_PERCENT, HanoiData.SUBGRAPH_LUA_PERCENT_INCLUSIVE, dataInfoMap[HanoiData.SUBGRAPH_LUA_PERCENT_INCLUSIVE]);
                GraphItLuaPro.Log(HanoiData.GRAPH_TIME_PERCENT, HanoiData.SUBGRAPH_LUA_PERCENT_EXCLUSIVE, dataInfoMap[HanoiData.SUBGRAPH_LUA_PERCENT_EXCLUSIVE]);
                
            }
            Debug.LogFormat("reading {0} objects.", HanoiNode.s_count);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
        return true;
    }

    public ResovleSessionJsonResult handleSessionJsonObj(JSONObject jsonContent)
    {
        if (jsonContent == null || jsonContent.IsNull)
            throw new System.Exception("json load error");
        var watch1 = new System.Diagnostics.Stopwatch();
        watch1.Start();
        ResovleSessionJsonResult result = new ResovleSessionJsonResult();
        Dictionary<string, List<DataInfo>> dataInfoMap = new Dictionary<string, List<DataInfo>>();
        dataInfoMap.Add(HanoiData.SUBGRAPH_LUA_TIMECONSUMING_INCLUSIVE, new List<DataInfo>());
        dataInfoMap.Add(HanoiData.SUBGRAPH_LUA_TIMECONSUMING_EXCLUSIVE, new List<DataInfo>());
        dataInfoMap.Add(HanoiData.SUBGRAPH_LUA_PERCENT_INCLUSIVE, new List<DataInfo>());
        dataInfoMap.Add(HanoiData.SUBGRAPH_LUA_PERCENT_EXCLUSIVE, new List<DataInfo>());
        if (jsonContent.type == JSONObject.Type.ARRAY)
        {
            watch1.Reset();
            watch1.Start();
            for (int i = 0; i < jsonContent.list.Count; i++)
            {
                JSONObject j = (JSONObject)jsonContent.list[i];
                handleMsgForNavigationScreen(j, dataInfoMap);
            }
            result.NavigateResult = dataInfoMap;
            watch1.Stop();
           // UnityEngine.Debug.LogFormat("resolve Navigation json {0}", watch1.ElapsedMilliseconds);

            watch1.Reset();
            watch1.Start();
            List<HanoiNode> resultNodeRoot = new List<HanoiNode>();

            for (int i = 0; i < jsonContent.list.Count; i++)
            {
                JSONObject j = (JSONObject)jsonContent.list[i];
                handleMsgForDetailScreen(j,resultNodeRoot);
            }
            watch1.Stop();
            //UnityEngine.Debug.LogFormat("resolve detail json {0}", watch1.ElapsedMilliseconds);
            result.DetailResult =resultNodeRoot;
            return result;
        }
        return null;
    }



    public void handleMsgForDetailScreen(JSONObject jsonMsg, List<HanoiNode> resultNodeRoot)
    {
        if (jsonMsg == null)
            return;
        if (jsonMsg.IsNull || jsonMsg.type != JSONObject.Type.OBJECT)
            return;
        if (Root == null || Root.callStats == null)
            return;

        HanoiNode newNode = null;

        bool isFrameInfo = jsonMsg.GetField("frameID");
        //是帧信息
        if (isFrameInfo)
        {
            newNode = new HanoiFrameInfo(Root.callStats);
        }
        else
        {
            //函数信息
            newNode = new HanoiNode(Root.callStats);
        }
        if (readObject(jsonMsg, newNode))
        {
            resultNodeRoot.Add(newNode);
        }
    }

    public  void handleMsgForNavigationScreen(JSONObject jsonMsg,Dictionary<string, List<DataInfo>> dataInfoMap)
    {
        if (jsonMsg == null || jsonMsg.IsNull || jsonMsg.type != JSONObject.Type.OBJECT)
            return;

        JSONObject luaConsuming = jsonMsg.GetField("luaConsuming");
        if (!luaConsuming ||!luaConsuming.IsNumber){
            //Debug.LogFormat("luaConsuming load error");
            return;
        }

        JSONObject funConsuming = jsonMsg.GetField("funConsuming");
        if (!funConsuming ||!funConsuming.IsNumber){
            Debug.LogFormat("funConsuming load error");
            return;
        }

        JSONObject frameTime = jsonMsg.GetField("frameTime");
        if (!frameTime || !frameTime.IsNumber)
        {
            Debug.LogFormat("frameTime load error");
            return;
        }

        JSONObject frameInterval = jsonMsg.GetField("frameInterval");
        if (!frameInterval || !frameInterval.IsNumber)
        {
            Debug.LogFormat("frameInterval load error");
            return;
        }

        JSONObject frameID = jsonMsg.GetField("frameID");
        if (!frameID || !frameID.IsNumber)
        {
            Debug.LogFormat("frameID load error");
            return;
        }

        dataInfoMap[HanoiData.SUBGRAPH_LUA_TIMECONSUMING_INCLUSIVE].Add(new DataInfo((float)funConsuming.n, frameTime.f, (float)frameInterval.n, (int)frameID.n - 1));
        dataInfoMap[HanoiData.SUBGRAPH_LUA_TIMECONSUMING_EXCLUSIVE].Add(new DataInfo((float)luaConsuming.n, frameTime.f, (float)frameInterval.n, (int)frameID.n - 1));
        dataInfoMap[HanoiData.SUBGRAPH_LUA_PERCENT_INCLUSIVE].Add(new DataInfo((float)(funConsuming.n / frameInterval.n * 100.0f), frameTime.f, (float)frameInterval.n, (int)frameID.n - 1));
        dataInfoMap[HanoiData.SUBGRAPH_LUA_PERCENT_EXCLUSIVE].Add(new DataInfo((float)(luaConsuming.n / frameInterval.n) * 100.0f, frameTime.f, (float)frameInterval.n, (int)frameID.n - 1));
    }


    public bool readObject(JSONObject obj, HanoiNode node)
    {
        if (obj.type != JSONObject.Type.OBJECT)
            return false;
        node.processJsonObject(obj,this);
        return true;
    }
}
