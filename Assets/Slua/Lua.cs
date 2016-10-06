using UnityEngine;
using System.Collections;
using SLua;
using LuaInterface;
using System.Collections.Generic;
using System.IO;

[SLua.CustomLuaClass]
public class Lua
{
    private static Lua m_Instance = null;

    public LuaSvr m_LuaSvr = null;

    private string m_strPath = Application.temporaryCachePath;
    private string m_strTime = Application.bundleIdentifier + "." + System.DateTime.Now.Year.ToString() + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Day.ToString() + "-" + System.DateTime.Now.Hour.ToString() + "-" + System.DateTime.Now.Minute.ToString() + "-" + System.DateTime.Now.Second.ToString();
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

    public void InitLuaProfiler()
    {
        m_LuaSvr = new LuaSvr();
        m_LuaSvr.init(null, null, LuaSvrFlag.LSF_BASIC);
        LuaDLL.init_profiler(m_LuaSvr.luaState.L);
        Debug.Log(m_strPath);
        m_strPath = m_strPath + "/" + m_strTime;
        Debug.Log(m_strPath);
        DirectoryInfo myDirectoryInfo = new DirectoryInfo(m_strPath);
        if (!myDirectoryInfo.Exists)
        {
            Directory.CreateDirectory(m_strPath);
        }
    }

    public void StartLuaProfiler()
    {
        string file = m_strPath + "/" + m_strTime + ".json";
        object o = m_LuaSvr.luaState.getFunction("profiler_start").call(file);
    }

    public void StopLuaProfiler()
    {
        object o = m_LuaSvr.luaState.getFunction("profiler_stop").call();
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

