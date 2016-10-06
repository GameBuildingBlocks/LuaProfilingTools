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
    // Use this for initialization
    void Start()
	{
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
    }

	void OnGUI()
	{
		if(progress!=100)
			GUI.Label(new Rect(0, 0, 100, 50), string.Format("Loading {0}%", progress));
    }

    public void onClickStart()
    {
        isStarted = true;
        Lua.Instance.StartLuaProfiler();
        Lua.Instance.m_LuaSvr.start("main");
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
            object o = Lua.Instance.m_LuaSvr.luaState.getFunction("foo").call(1, 2, 3);
            //Lua.Instance.SetFrameInfo();
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

}
