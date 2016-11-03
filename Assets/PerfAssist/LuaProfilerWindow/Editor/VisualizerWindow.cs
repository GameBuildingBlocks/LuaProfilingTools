using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
     public class VisualizerWindow : EditorWindow
     {
         public enum MouseInArea
         {
             none,
             ControlScreen,
             NavigationScreen ,
             DetailScreen ,
         };

         [SerializeField]
         internal Vector2 m_Scale = new Vector2(1.0f, 1.0f);
         [SerializeField]
         internal Vector2 m_Translation = new Vector2(0, 0);

         MouseInArea m_mouseArea = MouseInArea.none;

         float targetTranslationX = 0;
         float targetTranslationInterval = 0;
         float viewPointToGlobalTimeMarin = 10;

         public static float m_winWidth = 0.0f;
         public static float m_winHeight = 0.0f;

         public static float m_controlScreenHeight = 0.0f;
         public static float m_controlScreenPosY = 0.0f;

         public static float m_navigationScreenHeight = 0.0f;
         public static float m_navigationScreenPosY = 0.0f;

         public static float m_detailScreenHeight = 0.0f;
         public static float m_detailScreenPosY = 0.0f;

         List<string> sessionMsgList = new List<string>();
         
         float  HanleSessionTime = 0.0f;

         HanoiData m_data = new HanoiData();
         HanoiNode m_picked;

         public bool m_isTestCallLua = false;

         int _selectedJsonFileIndex = -1;
         string[] _JsonFilesPath = new string[] { };

         public class SessionJsonObj
         {
             public bool readerFlag = false;
             public ResovleSessionJsonResult m_resovleSessionResult;
             public ResovleSessionJsonResult readResovleJsonResult()
             {
                 if (m_resovleSessionResult == null)
                     return null;
                 lock (this)  
                 {
                     if (!readerFlag)
                     {          
                         try
                         {
                             Monitor.Wait(this);
                         }
                         catch (SynchronizationLockException e)
                         {
                             Console.WriteLine(e);
                         }
                         catch (ThreadInterruptedException e)
                         {
                             Console.WriteLine(e);
                         }
                     }
                     readerFlag = false;   
                     Monitor.Pulse(this);   
                 }
                 return m_resovleSessionResult;
             }

             public void writeResovleJsonResult(ResovleSessionJsonResult resovleJsonResult)
             {
                 lock (this)  
                 {
                     if (readerFlag)
                     {      
                         try
                         {
                             Monitor.Wait(this);  
                         }
                         catch (SynchronizationLockException e)
                         {
                             Console.WriteLine(e);
                         }
                         catch (ThreadInterruptedException e)
                         {
                             Console.WriteLine(e);
                         }
                     }
                     m_resovleSessionResult = resovleJsonResult;
                     readerFlag = true;  
                     Monitor.Pulse(this); 
                 } 
             }
         }

         static SessionJsonObj  sessionJsonObj  = new SessionJsonObj();

         [MenuItem("Window/"+Lua.g_editorWindow)]
         static void Create()
         {
             //// Get existing open window or if none, make a new one:
                VisualizerWindow m_window = (VisualizerWindow)EditorWindow.GetWindow(typeof(VisualizerWindow));
                 m_window.Show();
                 m_window.wantsMouseMove = true;
                 m_window.CheckForResizing();
         }

         public void handleMsgAsyn(object o)
         {
             //var watch = new System.Diagnostics.Stopwatch();
             //watch.Start();

             StringBuilder strBuilder = new StringBuilder("");
             foreach (string msg in (string[])o)
             {
                 strBuilder.Append(msg);
                 strBuilder.Append(",");
             }
             var result = new JSONObject("[$$]".Replace("$$",strBuilder.ToString()));
             var resovleJsonResult =m_data.handleSessionJsonObj(result);
             if (resovleJsonResult != null)
                 sessionJsonObj.writeResovleJsonResult(resovleJsonResult);
             //watch.Stop();
             //UnityEngine.Debug.LogFormat("handleMsgAsyn  time {0}", watch.ElapsedMilliseconds);
         }


         void Update()
         {
             //if ((Lua.Instance != null) && Lua.Instance.IsRegisterLuaProfilerCallback())
             //    Lua.Instance.m_LuaSvr.luaState.getFunction("foo").call(1, 2, 3);

             if ((Lua.Instance != null) && Lua.Instance.IsRegisterLuaProfilerCallback())
                 Lua.Instance.SetFrameInfo();

             doTranslationAnimation();

             
             if (m_data != null &&sessionMsgList.Count>0 &&Time.realtimeSinceStartup - HanleSessionTime >= 1.0f)
             {
                HanleSessionTime = Time.realtimeSinceStartup;
              
                Thread mThread = new Thread(new ParameterizedThreadStart(handleMsgAsyn));
                mThread.Start(sessionMsgList.ToArray());
                sessionMsgList.Clear();

                 if (sessionJsonObj.readerFlag)
                 {
                     ResovleSessionJsonResult result = sessionJsonObj.readResovleJsonResult();
                     if (result != null)
                     {
                         m_data.m_hanoiData.callStats.Children.AddRange(result.DetailResult);
                         var dataInfoMap = result.NavigateResult;
                         GraphItLuaPro.Log(HanoiData.GRAPH_TIMECONSUMING, HanoiData.SUBGRAPH_LUA_TIMECONSUMING_INCLUSIVE, dataInfoMap[HanoiData.SUBGRAPH_LUA_TIMECONSUMING_INCLUSIVE]);
                         GraphItLuaPro.Log(HanoiData.GRAPH_TIMECONSUMING, HanoiData.SUBGRAPH_LUA_TIMECONSUMING_EXCLUSIVE, dataInfoMap[HanoiData.SUBGRAPH_LUA_TIMECONSUMING_EXCLUSIVE]);
                         GraphItLuaPro.Log(HanoiData.GRAPH_TIME_PERCENT, HanoiData.SUBGRAPH_LUA_PERCENT_INCLUSIVE, dataInfoMap[HanoiData.SUBGRAPH_LUA_PERCENT_INCLUSIVE]);
                         GraphItLuaPro.Log(HanoiData.GRAPH_TIME_PERCENT, HanoiData.SUBGRAPH_LUA_PERCENT_EXCLUSIVE, dataInfoMap[HanoiData.SUBGRAPH_LUA_PERCENT_EXCLUSIVE]);
                         Repaint();
                     }
                 }
             }
         }

         private void doTranslationAnimation()
         {
             float delta = targetTranslationX - m_Translation.x;
             if (delta == 0)
                 return;
             if (Mathf.Abs(delta) < targetTranslationInterval)
                 m_Translation.x = targetTranslationX;
             else
             {
                 if (delta > 0)
                 {
                     m_Translation.x += targetTranslationInterval;
                 }
                 else
                 {
                     m_Translation.x -= targetTranslationInterval;
                 }
             }                   
             Repaint();
         }

         public void onSessionMessage(string strInfo)
         {
             if (string.IsNullOrEmpty(strInfo))
                 return;
             sessionMsgList.Add(strInfo);
         }

         void OnDestroy() {
              if (Lua.Instance.IsRegisterLuaProfilerCallback())
                  Lua.Instance.UnRegisterLuaProfilerCallback();
         }

         public VisualizerWindow()
         {
             refreshCheckJasonFilesUpadate();
         }

         public void OnGUI()
         {
             handleCommandEvent();
             CheckForResizing();
             Handles.BeginGUI();
             Handles.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
             //control窗口内容
             GUILayout.BeginArea(new Rect(0, m_controlScreenPosY, m_winWidth, m_controlScreenHeight));
             {
                 drawGUIElement();
             }
             GUILayout.EndArea();

             //navigation窗口内容
             GUILayout.BeginArea(new Rect(0, m_navigationScreenPosY, m_winWidth, m_navigationScreenHeight));
             {
                 if (m_data.isHanoiDataHasContent())
                     GraphItWindowLuaPro.DrawGraphs(position, this);
             }
             GUILayout.EndArea();

             if (m_data.isHanoiDataLoadSucc() && (EditorWindow.focusedWindow == this))
             {
                 CheckForInput();
                 //detail窗口内容
                 GUILayout.BeginArea(new Rect(0, m_detailScreenPosY, m_winWidth, m_detailScreenHeight));
                 {
                     Handles.matrix = Matrix4x4.TRS(m_Translation, Quaternion.identity, new Vector3(m_Scale.x, m_Scale.y, 1));
                     HanoiUtil.CalculateFrameInterval(m_data.Root.callStats, null);
                     calculateStackHeight();

                     calculateScreenClipRange();
                     DrawHanoiData(m_data.Root);

                     drawTimeInterval();
                     drawFrameInfo(m_data.Root.callStats, mousePositionInDrawing.x);
                     if(m_mouseArea == MouseInArea.DetailScreen)showMouseGlobalTime();
                 }
                 GUILayout.EndArea();
             }

             Handles.EndGUI();
         }

         private void reInitHanoiRoot() {
             GraphItLuaPro.Clear();
             m_data.m_hanoiData = new HanoiRoot();
             m_data.Root.callStats = new HanoiNode(null);
             _selectedJsonFileIndex = _JsonFilesPath.Length - 1;
             GraphItWindowLuaPro.MouseXOnPause = -1;
         }

         private void handleCommandEvent()
         {
             if (Event.current.commandName.Equals("AppStarted"))
             {
                 if((Lua.Instance!=null) &&!Lua.Instance.IsRegisterLuaProfilerCallback())
                 {
                     reInitHanoiRoot();
                     Lua.Instance.RegisterLuaProfilerCallback(this.onSessionMessage);
                 }
             }

             if (Event.current.commandName.Equals("AppStoped"))
             {
                 refreshCheckJasonFilesUpadate();
                 if (Lua.Instance.IsRegisterLuaProfilerCallback())
                     Lua.Instance.UnRegisterLuaProfilerCallback();
             }
         }
         private void drawGUIElement()
         {
             GUILayout.BeginHorizontal();
             {
                 GUILayout.MaxWidth(150);
                 GUI.color = Color.white;
                 Handles.color = Color.white;
                 EditorGUIUtility.labelWidth = 80;
                 int currentSelectedIndex = EditorGUILayout.Popup(string.Format("Sessions"), _selectedJsonFileIndex, _JsonFilesPath, GUILayout.Width(350));
                 _selectedJsonFileIndex = currentSelectedIndex;
                 if (GUILayout.Button("Load", GUILayout.Width(50)))
                     loadSelectedSessions(currentSelectedIndex);

                 if (GUILayout.Button("Loadfile", GUILayout.Width(90)))
                 {
                     string path = EditorUtility.OpenFilePanel("Open a lua_perf json", "", "json");
                     if (path.Length != 0)
                     {
                         loadSession(path);
                     }
                 }

                 GraphItWindowLuaPro._TimeLimitSelectIndex = GUI.SelectionGrid(new Rect(500, 0, 350, 20), GraphItWindowLuaPro._TimeLimitSelectIndex, GraphItWindowLuaPro._TimeLimitStrOption, GraphItWindowLuaPro._TimeLimitStrOption.Length);
                 GraphItWindowLuaPro._PercentLimitSelectIndex = GUI.SelectionGrid(new Rect(950, 0, 300, 20), GraphItWindowLuaPro._PercentLimitSelectIndex, GraphItWindowLuaPro._PercentLimitStrOption, GraphItWindowLuaPro._PercentLimitStrOption.Length);
             }
             GUILayout.EndHorizontal();
         }

         private void loadSelectedSessions(int currentSelectedIndex)
         {
             try
             {
                 if (currentSelectedIndex < 0)
                     throw new System.ArgumentException(string.Format("invalid selected index ({0}). ", currentSelectedIndex));

                 if (currentSelectedIndex == _JsonFilesPath.Length - 1)
                 {
                     reInitHanoiRoot();
                     return;
                 }

                 string file = getSessionsBySelectedIndex(HanoiUtil.GetVaildJsonFolders(), currentSelectedIndex);
                 loadSession(file);
             }
             catch (System.Exception ex)
             {
                 _selectedJsonFileIndex = -1;
                 Debug.LogErrorFormat("[Hanoi] Loading session failed. ({0})", ex.Message);
                 EditorUtility.DisplayDialog("load sessions error", string.Format("[Hanoi] Loading session failed. ({0})", ex.Message), "确认");
             }
         }

         private void loadSession(string file)
         {
             if (!loadJsonData(file))
                 throw new System.ArgumentException(string.Format("loading file `{0}` failed. ", file));

             HanoiUtil.CalculateFrameInterval(m_data.Root.callStats, null);
             calculateStackHeight();
         }


         private string getSessionsBySelectedIndex(string[] jsonFolders ,int selectedIndex)
         {
             if (selectedIndex < 0) 
                 return "";
             string[] filePaths = Lua.Instance.GetProfilerFiles(jsonFolders[selectedIndex]);
             if (filePaths.Length <= 0) 
                 return "";
             return filePaths[0];
         }

         private void refreshCheckJasonFilesUpadate()
        {
            try
            {
                string[] files =  HanoiUtil.GetVaildJsonFolders();
                for (int i = 0; i < files.Length; i++)
                {
                    int begin = files[i].LastIndexOfAny(new char[] { '\\', '/' });
                    if (begin != -1)
                    {
                        files[i] = files[i].Substring(begin + 1);
                    }
                }
                _JsonFilesPath = files;
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                _JsonFilesPath = new string[] { };
            }
        }

         /// <summary>
         /// 计算屏幕剪裁的时间范围
         /// </summary>
         public void calculateScreenClipRange()
         {
             HanoiUtil.ScreenClipMinX=ViewToDrawingTransformValue(0.0f);   
             HanoiUtil.ScreenClipMaxX=ViewToDrawingTransformValue(m_winWidth);
         }

         private void drawFrameInfo(HanoiNode n,float mouseX)
         {
             HanoiUtil.DrawFrameStatementRecursively(n);
             HanoiUtil.DrawSelectedFrameInfoRecursively(n, mouseX);
         }

         /// <summary>
         /// 鼠标旁显示全局时间
         /// </summary>
         private void showMouseGlobalTime() {
             float globalTimeLabelHight = m_detailScreenHeight / 10;
             Rect r = new Rect();
             r.position = new Vector2(mousePositionInDrawing.x, globalTimeLabelHight * 2);
             r.width = HanoiVars.LabelBackgroundWidth/2;
             r.height = 15;
             Color bg = Color.yellow;
             bg.a = 0.5f;
             Handles.DrawSolidRectangleWithOutline(r, bg, bg);
             
             GUI.color = Color.black;
             Handles.color = Color.yellow;
             Handles.DrawLine(new Vector3(mousePositionInDrawing.x, 0), new Vector3(mousePositionInDrawing.x, m_detailScreenHeight));
             Handles.Label(new Vector3(mousePositionInDrawing.x, globalTimeLabelHight * 2), string.Format("Time: {0:0.000}", mousePositionInDrawing.x + HanoiUtil.FrameTimeOnPause));
         }

         private bool loadJsonData(string jsonFile)
         {
             if (string.IsNullOrEmpty(jsonFile)) 
                 return false;
             bool succ = m_data.Load(jsonFile);
             if (!succ) 
                 return false;
             
             return true;
         }

         private void drawTimeInterval() {
             float timeLineHight = m_detailScreenHeight - m_detailScreenHeight / 30 ; 
             Handles.color = Color.white;
             GUI.color = Color.yellow;
             float timeInterval = getTimeInterval(HanoiUtil.ScreenClipMinX,HanoiUtil.ScreenClipMaxX);

             if ((HanoiUtil.ScreenClipMaxX - HanoiUtil.ScreenClipMinX) / timeInterval > 200)
                 return;

             timeInterval = Mathf.Max(timeInterval,0.001f);
             List<float> timeIntervalPosXList = new List<float>();
             float baseNum= (int)(HanoiUtil.ScreenClipMinX / timeInterval) * timeInterval;
             while (baseNum<HanoiUtil.ScreenClipMaxX)
             {
                 timeIntervalPosXList.Add(baseNum);
                 baseNum += timeInterval;
             }

             foreach(float posX  in timeIntervalPosXList){
                 Handles.DrawLine(new Vector3(posX, timeLineHight), new Vector3(posX, m_detailScreenHeight));
                 Handles.Label(new Vector3(posX, timeLineHight), string.Format("{0:0.00}", posX + HanoiUtil.FrameTimeOnPause));
             }
         }

         /// <summary>
         ///  获得帧间时间的显示间隔
         /// </summary>
         private float getTimeInterval(float x0, float x1) {
             float screenClipDelta = x1 - x0;
             if (screenClipDelta < 0.1)
                 return 0.01f;

             float interval = 10000000;
             while (screenClipDelta < interval * 3.0f)
             {
                 interval /= 10;
             }
            return interval;
         }


         public void CheckForResizing()
         {
             if (Mathf.Approximately(position.width, m_winWidth) && 
                 Mathf.Approximately(position.height, m_winHeight))
                 return; 

             m_winWidth = position.width;
             m_winHeight = position.height;

             m_detailScreenHeight = m_winHeight / 1.65f;
             m_detailScreenPosY = m_winHeight / 2.5f;

             m_navigationScreenHeight = (m_winHeight - m_detailScreenHeight)/1.1f;
             m_navigationScreenPosY = m_detailScreenPosY/10.0f;

             m_controlScreenHeight = m_winHeight - m_detailScreenHeight - m_navigationScreenHeight;
             m_controlScreenPosY = 0.0f;

             GraphItLuaPro.GraphSetupHeight(HanoiData.GRAPH_TIMECONSUMING, m_navigationScreenHeight / 2 - GraphItWindowLuaPro.y_gap);
             GraphItLuaPro.GraphSetupHeight(HanoiData.GRAPH_TIME_PERCENT, m_navigationScreenHeight / 2 - GraphItWindowLuaPro.y_gap);
             GraphItLuaPro.ShareYAxis(HanoiData.GRAPH_TIMECONSUMING,true);
             GraphItLuaPro.ShareYAxis(HanoiData.GRAPH_TIME_PERCENT,true);
         }

         private void calculateStackHeight() {
             HanoiVars.StackHeight = (m_data.MaxStackLevel != 0) ? (m_detailScreenHeight / m_data.MaxStackLevel) : m_detailScreenHeight;
         }

         private void DrawHanoiData(HanoiRoot r)
         {
             if (r.callStats == null)
                 return;

             HanoiVars.LabelBackgroundWidth = GetDrawingLengthByPanelPixels(200);
             HanoiUtil.m_ShowDigitMax = GetDrawingLengthByPanelPixels(1) / 1000.0f;
             HanoiVars.DrawnStackCount = m_data.MaxStackLevel;

             // draw 3 passes
             HanoiUtil.DrawRecursively(r.callStats);
             HanoiUtil.DrawLabelsRecursively(r.callStats);
         }

         public Vector2 ViewToDrawingTransformPoint(Vector2 lhs)
         {return new Vector2((lhs.x - m_Translation.x) / m_Scale.x, (lhs.y - m_Translation.y) / m_Scale.y);}
         public Vector3 ViewToDrawingTransformPoint(Vector3 lhs)
         { return new Vector3((lhs.x - m_Translation.x) / m_Scale.x, (lhs.y - m_Translation.y) / m_Scale.y,0); }

         public Vector2 DrawingToViewTransformVector(Vector2 lhs)
         { return new Vector2(lhs.x * m_Scale.x, lhs.y * m_Scale.y); }
         public Vector3 DrawingToViewTransformVector(Vector3 lhs)
         { return new Vector3(lhs.x * m_Scale.x, lhs.y * m_Scale.y, 0); }

         public Vector2 ViewToDrawingTransformVector(Vector2 lhs)
         { return new Vector2(lhs.x / m_Scale.x, lhs.y / m_Scale.y); }
         public Vector3 ViewToDrawingTransformVector(Vector3 lhs)
         { return new Vector3(lhs.x / m_Scale.x, lhs.y / m_Scale.y, 0); }

         public float ViewToDrawingTransformValue(float x)
         { return (x - m_Translation.x) / m_Scale.x;}

         public Vector2 mousePositionInDrawing
         {
             get { return ViewToDrawingTransformPoint(Event.current.mousePosition);}
         }

         public Vector2 mousePositionInDetailScreen
         {
             get { return mousePositionInDrawing - new Vector2(0, m_detailScreenPosY); }
         }

         public float GetDrawingLengthByPanelPixels(int pixels)
         {
             return Mathf.Abs(ViewToDrawingTransformPoint(new Vector2(pixels, 0)).x - ViewToDrawingTransformPoint(new Vector2(0, 0)).x); 
         }

         /// <summary>
         /// 将屏幕中心移动到给定的时间上
         /// </summary>
         public void setViewPointToGlobalTime(float globalTime,float interval,float mouseX) {
             m_Scale.x = (m_winWidth - viewPointToGlobalTimeMarin*2) / interval;
             float viewMidValue = Mathf.Abs(DrawingToViewTransformVector(new Vector2(globalTime, 0)).x);
             targetTranslationX = (m_winWidth - viewPointToGlobalTimeMarin) - viewMidValue;

             float delta = targetTranslationX - m_Translation.x;
             targetTranslationInterval = Mathf.Abs(delta) / 30;
         }

         private void CheckForInput()
         {
             switch (Event.current.type)
             {
                 case EventType.KeyUp:
                     m_isTestCallLua = !m_isTestCallLua;
                     EditorApplication.isPaused = false;
                     break;
                 case EventType.mouseMove:
                     {
                         if (m_picked != null)
                         {
                             HanoiUtil.ForeachInParentChain(m_picked, (n) => { n.highlighted = false; });
                             m_picked = null;
                         }

                         HanoiNode picked = PickHanoiRecursively(m_data.Root.callStats, mousePositionInDetailScreen);
                         if (picked != null)
                         {
                             HanoiUtil.ForeachInParentChain(picked, (n) =>
                             {
                                 n.highlighted = true;
                             });
                             m_picked = picked;

                             Debug.LogFormat("Picked: f {0}, m {1}", m_picked.funcName, m_picked.moduleName);
                         }
                         else
                         {
                             // Debug.LogFormat("Picked nothing.");
                         }

                         if (EditorWindow.focusedWindow == this)
                         {
                             if ((Event.current.mousePosition.x >= 0 && Event.current.mousePosition.x <= m_winWidth) &&
                                 (Event.current.mousePosition.y >= m_detailScreenPosY && Event.current.mousePosition.y <= m_winHeight))
                             {
                                 m_mouseArea = MouseInArea.DetailScreen;
                                 Repaint();
                             }
                             else
                             {
                                 m_mouseArea = MouseInArea.none;
                             }
                         }
                     }
                     break;
                 case EventType.MouseDrag:
                     if (Event.current.button == 1)
                     {
                         if (m_mouseArea == MouseInArea.DetailScreen)
                         {
                             m_Translation.x += Event.current.delta.x;
                             targetTranslationX = m_Translation.x;
                             Repaint();
                         }
                     }
                     break;
                 case EventType.ScrollWheel:
                     {
                         float delta = Event.current.delta.x + Event.current.delta.y;
                         delta = -delta;

                         // Scale multiplier. Don't allow scale of zero or below!
                         float scale = Mathf.Max(0.1F, 1 + delta * 0.1F);

                         // Offset to make zoom centered around cursor position
                         m_Translation.x -= mousePositionInDrawing.x * (scale - 1) * m_Scale.x;
                         targetTranslationX = m_Translation.x;
                        
                         // Apply zooming
                         m_Scale.x *= scale;
                         Repaint();
                     }
                     break;
                 default:
                     break;
             }
         }

         public void setFrameTimeOnPause(float frameTime) {
             HanoiUtil.FrameTimeOnPause = frameTime;
         }


         private HanoiNode PickHanoiRecursively(HanoiNode n, Vector2 mousePos)
         {
             if (n.HasValidRect()) {
                 if (n.renderRect.xMin > mousePos.x || n.renderRect.xMax < mousePos.x)
                     return null;

                 if (n.renderRect.Contains(mousePos))
                     return n;
             }

             for (int i = 0; i < n.Children.Count; i++)
             {
                 HanoiNode child = PickHanoiRecursively(n.Children[i], mousePos);
                 if (child != null)
                 {
                     return child;
                 }
             }
             return null;
         }
     }
 