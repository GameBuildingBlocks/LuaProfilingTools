using UnityEngine;
using System.Collections;
using SLua;
using LuaInterface;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif


[SLua.CustomLuaClass]
public class Lua
{
    private static Lua m_Instance = null;

    public LuaSvr m_LuaSvr = null;
    private string m_luaProfilerSessionsPath = Path.Combine(Application.temporaryCachePath,"LuaProfilerSessions");
    private string m_strTime = Application.bundleIdentifier + "." + System.DateTime.Now.Year.ToString() + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Day.ToString() + "-" + System.DateTime.Now.Hour.ToString() + "-" + System.DateTime.Now.Minute.ToString() + "-" + System.DateTime.Now.Second.ToString();

    bool _networkAvailable = false;
    public static Lua Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new Lua();
            }
            return m_Instance;
        }
    }


    public static void OnMessage(string data)
    {
        Debug.Assert(Instance != null);
        if (Instance == null)
            return;

        Debug.Assert(!string.IsNullOrEmpty(data));
        if (string.IsNullOrEmpty(data))
            return;

        if (Instance._networkAvailable)
        {
            UsCmd cmd = new UsCmd();
            cmd.WriteInt16((short)eNetCmd.SV_SendLuaProfilerMsg);
            cmd.WriteString(data);
            UsNet.Instance.SendCommand(cmd);
        }
    }

    public void InitLuaProfiler()
    {
        m_LuaSvr = new LuaSvr();
        m_LuaSvr.init(null, null, LuaSvrFlag.LSF_BASIC);
        m_LuaSvr.start("main");

        LuaDLL.init_profiler(m_LuaSvr.luaState.L);
        m_luaProfilerSessionsPath = Path.Combine(m_luaProfilerSessionsPath,m_strTime);
        Debug.Log(m_luaProfilerSessionsPath);
    }
    
    public void StartLuaProfiler()
    {
        DirectoryInfo myDirectoryInfo = new DirectoryInfo(m_luaProfilerSessionsPath);
        if (!myDirectoryInfo.Exists)
        {
            Directory.CreateDirectory(m_luaProfilerSessionsPath);
        }
        string file = Path.Combine(m_luaProfilerSessionsPath, m_strTime + ".json");
        m_LuaSvr.luaState.getFunction("profiler_start").call(file);

        if (!LuaDLL.isregister_callback())
            LuaDLL.register_callback(OnMessage);

        _networkAvailable = UsNet.Instance != null && UsNet.Instance.IsListening;
        if (_networkAvailable)
        {
            UsCmd cmd = new UsCmd();
            cmd.WriteInt16((short)eNetCmd.SV_StartLuaProfilerMsg);
            UsNet.Instance.SendCommand(cmd);
        }
    }

    public void StopLuaProfiler()
    {
        if (m_LuaSvr!=null && m_LuaSvr.luaState != null)
            m_LuaSvr.luaState.getFunction("profiler_stop").call();
        UnRegisterLuaProfilerCallback();
    }

    public bool IsRegisterLuaProfilerCallback()
    {
        return LuaDLL.isregister_callback();
    }


    public void RegisterLuaProfilerCallback2(string obj,string method)
    {
        LuaDLL.register_callback2(obj, method);
    }

    public void UnRegisterLuaProfilerCallback()
    {
        LuaDLL.unregister_callback();
    }


    public void SetFrameInfo()
    {
        LuaDLL.frame_profiler(Time.frameCount, System.DateTime.Now.Millisecond);
    }

     string[] GetSysDirector(string dir)
    {
        return System.IO.Directory.GetFileSystemEntries(dir);
    }

    public string[] GetProfilerFolders()
    {
        return GetSysDirector(Application.temporaryCachePath);
    }

    public string[] GetProfilerFiles(string path)
    {
        return GetSysDirector(path);
    }

    private int ConvertDateTimeInt(System.DateTime time)
    {
        System.DateTime startTime = System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        return (int)(time - startTime).TotalSeconds;
    }
}
