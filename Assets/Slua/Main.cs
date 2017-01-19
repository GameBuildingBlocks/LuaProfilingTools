using UnityEngine;
using System.Collections;
using SLua;
using UnityEngine.UI;
using System.Collections.Generic;

public class Main : MonoBehaviour
{

	public Text logText;
	int progress=0;
    public Button mButtonStart;
    public Button mButtonStop;
    public Button mButtonFrame;
    public bool isStarted = false;

    public bool LogRemotely = true;
    public bool LogIntoFile = false;
    public bool InGameGui = false;

    private UsMain _usmooth;
    // Use this for initialization
    void Start()
	{
        _usmooth = new UsMain(LogRemotely, LogIntoFile, InGameGui);

#if UNITY_5
		Application.logMessageReceived += this.log;
#else
		Application.RegisterLogCallback(this.log);
#endif
		Lua.Instance.InitLuaProfiler();
    }

	void log(string cond, string trace, LogType lt)
	{
		logText.text += (cond + "\n");

	}

	void tick(int p)
	{
		progress = p;
	}

    void OnLevelWasLoaded()
    {
        if (_usmooth != null)
            _usmooth.OnLevelWasLoaded();
    }

	void complete()
	{
        /*
		l.start("main");
		object o = l.luaState.getFunction("foo").call(1, 2, 3);
		object[] array = (object[])o;
		for (int n = 0; n < array.Length; n++)
			Debug.Log(array[n]);
        o = l.luaState.getFunction("stop").call();
        */
    }

    void Update()
    {
        if (isStarted)
            Lua.Instance.SetFrameInfo();
        if (_usmooth != null)
            _usmooth.Update();
    }

	void OnGUI()
	{
		if(progress!=100)
			GUI.Label(new Rect(0, 0, 100, 50), string.Format("Loading {0}%", progress));
        if (_usmooth != null)
            _usmooth.OnGUI();
    }

    public void onClickStart()
    {
        isStarted = true;
        Lua.Instance.StartLuaProfiler();
        
    }

    public void onClickStop()
    {
        isStarted = false;
        Lua.Instance.StopLuaProfiler();
    }

    public void onClickCall()
    {
        if(isStarted)
        {
            Lua.Instance.m_LuaSvr.luaState.getFunction("foo").call(1, 2, 3);
            Lua.Instance.SetFrameInfo();
        }
    }

    public void onClickFolder()
    {
        string[] folders = Lua.Instance.GetProfilerFolders();

        if(folders.Length > 0)
        {
            string folder = folders[0];
            string[] files = Lua.Instance.GetProfilerFiles(folder);
            if (files.Length > 0)
                Debug.Log(files[0]);
        }
    }

    public void onClickCallback()
    {
        //Lua.Instance.RegisterLuaProfilerCallback(onUnityMessage);
        Lua.Instance.RegisterLuaProfilerCallback2("Canvas", "onUnityMessage");
    }

    public void onUnityMessage(string strInfo)
    {
        Debug.Log(strInfo);
        //print(strInfo);
    }

    void OnDestroy()
    {
        onClickStop();
        if (_usmooth != null)
            _usmooth.Dispose();
    }

    public void onClickLuaCallCSharp()
    {
        if(isStarted)
        {
            Lua.Instance.m_LuaSvr.luaState.getFunction("test").call();
        }
    }
}
