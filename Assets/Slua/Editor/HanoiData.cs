using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;

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

public class HanoiNode
{
    public static int s_count = 0;
    public HanoiNode(HanoiNode parent)
    {
        s_count++;

        Parent = parent;
    }

    public string moduleName = "";
    public string funcName = "";

    public int stackLevel = HanoiConst.BAD_NUM;

    public int currentLine = HanoiConst.BAD_NUM;
    public int lineDefined = HanoiConst.BAD_NUM;

    public eHanoiCallType callType = eHanoiCallType.None;
    public double timeConsuming = 0.0;
    public double beginTime = 0.0;
    public double endTime = 0.0;

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
    public HanoiFrameInfo(HanoiNode parent)
        : base(parent)
    {
    }
}

public class HanoiData 
{
    public HanoiRoot Root { get { return m_hanoiData; } }

    public  bool isHanoiDataLoadSucc(){ 
        return (m_hanoiData !=null);
    }

    public int MaxStackLevel { get { return m_maxStackLevel; } }
    int m_maxStackLevel = 0;

    JSONObject m_json;
    HanoiRoot m_hanoiData;

    public bool Load(string filename)
    {
        m_hanoiData = null;
        try
        {
            string templateJsonText = System.IO.File.ReadAllText("Assets/Resources/luaprofiler_jsonObjTemplates.json");
            string text = System.IO.File.ReadAllText(filename);
            m_json = new JSONObject(templateJsonText.Replace("$$", text));

            if (m_json.type != JSONObject.Type.OBJECT)
                return false;

            if (m_json.list.Count != 1)
                return false;


            HanoiNode.s_count = 0;

            m_hanoiData = new HanoiRoot();
            if (m_json.GetField("content")&& m_json.GetField("content").IsArray)
            {
                JSONObject jsonContent =m_json.GetField("content");
                HanoiNode contentNode = new HanoiNode(null);

                for (int i = 0; i < jsonContent.list.Count; i++)
                {
                    JSONObject j = (JSONObject)jsonContent.list[i];
                    HanoiNode newNode =null;
                    
                    bool isFrameInfo = j.GetField("frameID");
                    //是帧信息
                    if (isFrameInfo)
                    {
                        newNode = new HanoiFrameInfo(contentNode);
                    }
                    else
                    {
                        //函数信息
                        newNode = new HanoiNode(contentNode);
                    }
                    if (readObject(j, newNode))
                    {
                        contentNode.Children.Add(newNode);
                    }
                }
                Root.callStats = contentNode;
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

    bool readObject(JSONObject obj, HanoiNode node)
    {
        if (obj.type != JSONObject.Type.OBJECT)
            return false;
        JSONObject loadObj = null;
        if (node is HanoiFrameInfo)
        {
            HanoiFrameInfo frameNode = (HanoiFrameInfo)node;
            loadObj =obj.GetField("frameTime");
            if (loadObj && loadObj.IsNumber)
            {
                frameNode.frameTime = obj.GetField("frameTime").f;
            }
            else {
                Debug.LogFormat("frameTime load error");
            }

            loadObj = obj.GetField("frameUnityTime");
            if (loadObj && loadObj.IsNumber)
            {
                frameNode.frameUnityTime = obj.GetField("frameUnityTime").f;
            }
            else
            {
                Debug.LogFormat("frameUnityTime load error");
            }

            loadObj = obj.GetField("frameID");
            if (loadObj && loadObj.IsNumber)
            {
                frameNode.frameID = (int)obj.GetField("frameID").n;
            }
            else
            {
                Debug.LogFormat("frameID load error");
            }
        }
        else{
            loadObj = obj.GetField("currentLine");
            if (loadObj && loadObj.IsNumber)
            {
                node.currentLine = (int)obj.GetField("currentLine").n;
            }
            else
            {
                Debug.LogFormat("currentLine load error");
            }

            loadObj = obj.GetField("lineDefined");
            if (loadObj && loadObj.IsNumber)
            {
                node.lineDefined = (int)obj.GetField("lineDefined").n;
            }
            else
            {
                Debug.LogFormat("lineDefined load error");
            }

            loadObj = obj.GetField("timeConsuming");
            if (loadObj && loadObj.IsNumber)
            {
                node.timeConsuming = obj.GetField("timeConsuming").n;
            }
            else
            {
                Debug.LogFormat("timeConsuming load error");
            }

            loadObj = obj.GetField("stackLevel");
            if (loadObj && loadObj.IsNumber)
            {
                node.stackLevel = (int)obj.GetField("stackLevel").n;
                if (node.stackLevel > m_maxStackLevel)
                {
                    m_maxStackLevel = node.stackLevel;
                }
            }
            else
            {
                Debug.LogFormat("stackLevel load error");
            }

            loadObj = obj.GetField("callType");
            if (loadObj && loadObj.IsString)
            {
                switch (obj.GetField("callType").str)
                {
                    case "C":
                        node.callType = eHanoiCallType.C;
                        break;
                    case "Lua":
                        node.callType = eHanoiCallType.Lua;
                        break;
                }
            }
            else
            {
                Debug.LogFormat("callType load error");
            }

            loadObj = obj.GetField("begintime");
            if (loadObj && loadObj.IsNumber)
            {
                node.beginTime = obj.GetField("begintime").n;
            }
            else
            {
                Debug.LogFormat("beginTime load error");
            }

            loadObj = obj.GetField("endtime");
            if (loadObj && loadObj.IsNumber)
            {
                node.endTime = obj.GetField("endtime").n;
            }
            else
            {
                Debug.LogFormat("endTime load error");
            }

            loadObj = obj.GetField("moduleName");
            if (loadObj && loadObj.IsString)
            {
                node.moduleName = obj.GetField("moduleName").str;
            }
            else
            {
                Debug.LogFormat("moduleName load error");
            }

            loadObj = obj.GetField("funcName");
            if (loadObj && loadObj.IsString)
            {
                node.funcName = obj.GetField("funcName").str;
            }
            else
            {
                Debug.LogFormat("funcName load error");
            }

            foreach (JSONObject childJson in obj.GetField("children").list)
            {
                HanoiNode child = new HanoiNode(node);
                if (readObject(childJson, child))
                {
                    node.Children.Add(child);
                }
            }
        }
        return true;
    }
}
