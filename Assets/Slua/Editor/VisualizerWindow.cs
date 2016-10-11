 using UnityEngine;
 using UnityEditor;
 using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Security;
     public class VisualizerWindow : EditorWindow
     {
         [SerializeField]
         internal Vector2 m_Scale = new Vector2(1.0f, 1.0f);
         [SerializeField]
         internal Vector2 m_Translation = new Vector2(0, 0);
         public static float m_winWidth = 0.0f;
         public static float m_winHeight = 0.0f;

         public static string GRAPH_TIMECONSUMING = "timeConsuming";
         public static string SUBGRAPH_LUA_TIMECONSUMING_EXCLUSIVE = "luaTimeConsumingExclusive";
         public static string SUBGRAPH_LUA_TIMECONSUMING_UNEXCLUSIVE = "luaTimeConsumingUnExclusive";

         public static string GRAPH_TIME_PERCENT = "timePercent";
         public static string SUBGRAPH_LUA_PERCENT_EXCLUSIVE = "luaTimePercentExclusive";
         public static string SUBGRAPH_LUA_PERCENT_UNEXCLUSIVE = "luaTimePercentUnExclusive";

         public static float m_controlScreenHeight = 0.0f;
         public static float m_controlScreenPosY = 0.0f;

         public static float m_navigationScreenHeight = 0.0f;
         public static float m_navigationScreenPosY = 0.0f;

         public static float m_detailScreenHeight = 0.0f;
         public static float m_detailScreenPosY = 0.0f;

        private Vector2 scrollPosition = Vector2.zero;
        string t = "This is a string inside a Scroll view! \n  This is a string inside a Scroll view! \n This is a string inside a Scroll view!";

         HanoiData m_data = new HanoiData();
         HanoiNode m_picked;

         public bool m_isTestCallLua = false;

         int _selectedJsonFileIndex = -1;
         string[] _JsonFilesPath = new string[] { };

         [MenuItem("Window/"+Lua.g_editorWindow)]
         static void Create()
         {
             //// Get existing open window or if none, make a new one:
             VisualizerWindow m_window = (VisualizerWindow)EditorWindow.GetWindow(typeof(VisualizerWindow));
             m_window.Show();
             m_window.wantsMouseMove = true;
             m_window.CheckForResizing();
             //window.fitScreenSizeScale();
         }

         void Update() {
             if (m_isTestCallLua)
                 Lua.Instance.m_LuaSvr.luaState.getFunction("foo").call(1, 2, 3);
             Repaint();
         }

         public void onSessionMessage(string strInfo)
         {
             Debug.Log(strInfo);
             
             if (string.IsNullOrEmpty(strInfo))
                 return;
             
             string templateJsonText = "{ \"content\":[$$],}";
             JSONObject json = new JSONObject(templateJsonText.Replace("$$", strInfo));
             if (json.type != JSONObject.Type.OBJECT)
                 return;

             if (json.list.Count != 1)
                 return;

             JSONObject jsonContent = json.GetField("content");

             if (!jsonContent)
                 return;

             if (!jsonContent.IsArray)
                 return;

             if (jsonContent.list.Count != 1)
                 return;

             VisualizerWindow myWindow = (VisualizerWindow)EditorWindow.GetWindow(typeof(VisualizerWindow));
             myWindow.handleSessionMessage(jsonContent);

             myWindow.Repaint();
         }

         private void handleSessionMessage(JSONObject jsonMsg) {
             if (m_data!=null&&m_data.Root != null && m_data.Root.callStats != null)
             {
                 for (int i = 0; i < jsonMsg.list.Count; i++)
                 {
                     JSONObject jsonObj = (JSONObject)jsonMsg.list[i];
                     HanoiNode newNode = null;

                     bool isFrameInfo = jsonObj.GetField("frameID");
                     //是帧信息
                     if (isFrameInfo)
                     {
                         newNode = new HanoiFrameInfo(m_data.Root.callStats);
                     }
                     else
                     {
                         //函数信息
                         newNode = new HanoiNode(m_data.Root.callStats);
                     }
                     if (m_data.readObject(jsonObj, newNode))
                     {
                         m_data.Root.callStats.Children.Add(newNode);
                     }
                 }
             }

             JSONObject j = (JSONObject)jsonMsg.list[0];

             JSONObject luaConsuming = j.GetField("luaConsuming");
             if (luaConsuming)
             {
                 if (!luaConsuming.IsNumber)
                 {
                     Debug.LogFormat("luaConsuming load error");
                     return;
                 }

                 if (luaConsuming && luaConsuming.IsNumber)
                 {
                     JSONObject funConsuming = j.GetField("funConsuming");

                     JSONObject frameTime = j.GetField("frameTime");
                     if (!frameTime || !frameTime.IsNumber)
                     {
                         Debug.LogFormat("frameTime load error");
                         return;                            
                     }

                     JSONObject frameInterval = j.GetField("frameInterval");
                     if (!frameInterval || !frameInterval.IsNumber)
                     {
                         Debug.LogFormat("frameInterval load error");
                         return;
                     }


                     GraphIt.LogFixed(GRAPH_TIMECONSUMING, SUBGRAPH_LUA_TIMECONSUMING_UNEXCLUSIVE, new DataInfo((float)luaConsuming.n,frameTime.f));
                     GraphIt.LogFixed(GRAPH_TIMECONSUMING, SUBGRAPH_LUA_TIMECONSUMING_EXCLUSIVE, new DataInfo((float)(funConsuming.n - luaConsuming.n), frameTime.f));
                     GraphIt.PauseGraph(GRAPH_TIMECONSUMING);
                     GraphIt.StepGraph(GRAPH_TIMECONSUMING);

                     GraphIt.LogFixed(GRAPH_TIME_PERCENT, SUBGRAPH_LUA_PERCENT_UNEXCLUSIVE, new DataInfo((float)(luaConsuming.n / frameInterval.n) * 100.0f, frameTime.f));
                     GraphIt.LogFixed(GRAPH_TIME_PERCENT, SUBGRAPH_LUA_PERCENT_EXCLUSIVE, new DataInfo((float)((funConsuming.n - luaConsuming.n) / frameInterval.n * 100.0f), frameTime.f));
                     GraphIt.PauseGraph(GRAPH_TIME_PERCENT);
                     GraphIt.StepGraph(GRAPH_TIME_PERCENT);

                 }
                 else
                 {
                     Debug.LogFormat("timeConsuming load error");
                 }
             }
         }

         void OnDestroy() {
             if (!Lua.Instance.IsRegisterLuaProfilerCallback())
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
                 Rect w = new Rect();
                 w.position = new Vector2(0, 0);
                 w.width = m_winWidth;
                 w.height = m_controlScreenHeight;
                 Color bgw = Color.green;
                 bgw.a = 0.5f;
                 Handles.DrawSolidRectangleWithOutline(w, bgw, bgw);
                 drawGUIElement();
             }
             GUILayout.EndArea();

             //navigation窗口内容
             GUILayout.BeginArea(new Rect(0, m_navigationScreenPosY, m_winWidth, m_navigationScreenHeight));
             {
                 if (m_data.isHanoiDataHasContent())
                     GraphItGlobal.DrawGraphs(position, this);
             }
             GUILayout.EndArea();

             if (m_data.isHanoiDataLoadSucc())
             {
                 CheckForInput();
                 //detail窗口内容
                 GUILayout.BeginArea(new Rect(0, m_detailScreenPosY, m_winWidth, m_detailScreenHeight));
                 {
                     Handles.matrix = Matrix4x4.TRS(m_Translation, Quaternion.identity, new Vector3(m_Scale.x, m_Scale.y, 1));
                     
                     HanoiUtil.TotalTimeConsuming = HanoiUtil.calculateTotalTimeConsuming(m_data.Root.callStats);
                     HanoiUtil.CalculateFrameInterval(m_data.Root.callStats, null);
                     calculateStackHeight();

                     calculateScreenClipRange();
                     DrawHanoiData(m_data.Root);

                     drawTimeInterval();
                     drawFrameInfo(m_data.Root.callStats, mousePositionInDrawing.x);
                     showMouseGlobalTime();
                 }
                 GUILayout.EndArea();
             }

             Handles.EndGUI();
         }

         private void reInitHanoiRoot() {
             m_data.m_hanoiData = new HanoiRoot();
             m_data.Root.callStats = new HanoiNode(null);
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
             }
         }
         private void drawGUIElement()
         {
             GUILayout.BeginHorizontal();
             {
                 GUILayout.MaxWidth(150);
                 GUI.color = Color.white;
                 Handles.color = Color.white;
                 EditorGUIUtility.labelWidth =80;
                 int currentSelectedIndex = EditorGUILayout.Popup(string.Format("jasonFiles"), _selectedJsonFileIndex, _JsonFilesPath, GUILayout.Width(350));
                 loadSelectedSessions(currentSelectedIndex);

             }
             GUILayout.EndHorizontal();
         }

         private void loadSelectedSessions(int currentSelectedIndex)
         {
             if (_selectedJsonFileIndex == currentSelectedIndex)
                 return;
             try
             {
                 if (currentSelectedIndex < 0)
                     throw new System.ArgumentException(string.Format("invalid selected index ({0}). ", currentSelectedIndex));

                 string file = getSessionsBySelectedIndex(HanoiUtil.GetVaildJsonFolders(), currentSelectedIndex);
                 if (!loadJsonData(file))
                     throw new System.ArgumentException(string.Format("loading file `{0}` failed. ", file));

                 HanoiUtil.TotalTimeConsuming = HanoiUtil.calculateTotalTimeConsuming(m_data.Root.callStats);
                 HanoiUtil.CalculateFrameInterval(m_data.Root.callStats, null);
                 calculateStackHeight();
                 _selectedJsonFileIndex = currentSelectedIndex;
             }
             catch (System.Exception ex)
             {
                 _selectedJsonFileIndex = -1;
                 Debug.LogErrorFormat("[Hanoi] Loading session failed. ({0})", ex.Message);
             }
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
             r.position = new Vector2(mousePositionInDrawing.x, globalTimeLabelHight);
             r.width = HanoiVars.LabelBackgroundWidth/2;
             r.height = 15;
             Color bg = Color.yellow;
             bg.a = 0.5f;
             Handles.DrawSolidRectangleWithOutline(r, bg, bg);
             
             GUI.color = Color.black;
             Handles.color = Color.yellow;
             Handles.DrawLine(new Vector3(mousePositionInDrawing.x, 0), new Vector3(mousePositionInDrawing.x, m_detailScreenHeight));
             Handles.Label(new Vector3(mousePositionInDrawing.x, globalTimeLabelHight), string.Format("Time: {0:0.000}", mousePositionInDrawing.x));
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
                 Handles.Label(new Vector3(posX, timeLineHight), string.Format("{0:0.00}", posX));
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

         /// <summary>
         /// 自动计算scale大小，使图形适配屏幕大小
         /// </summary>
         public void fitScreenSizeScale() {
             float fitScaleX = m_winWidth / (HanoiUtil.TotalTimeConsuming);
             m_Scale.x = fitScaleX;
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

             GraphIt.GraphSetupHeight(GRAPH_TIMECONSUMING, m_navigationScreenHeight/2);
             GraphIt.GraphSetupHeight(GRAPH_TIME_PERCENT, m_navigationScreenHeight/2);

             GraphIt.setWinSize(new Vector2(position.width, position.height));
         }

         private void calculateStackHeight() {
             HanoiVars.StackHeight = (m_data.MaxStackLevel != 0) ? (m_detailScreenHeight / m_data.MaxStackLevel) : m_detailScreenHeight;
         }

         private void DrawHanoiData(HanoiRoot r)
         {
             if (r.callStats == null)
                 return;

             HanoiVars.LabelBackgroundWidth = GetDrawingLengthByPanelPixels(200);
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
         public void setViewPointToGlobalTime(float globalTime) {
             m_Scale.x = 25;
             float viewMidValue = Mathf.Abs(DrawingToViewTransformVector(new Vector2(globalTime, 0)).x);
             m_Translation.x = -1 * viewMidValue + m_winWidth / 2;
         }


         private void CheckForInput()
         {
             switch (Event.current.type)
             {
                 case EventType.KeyUp:
                     //setViewPointToGlobalTime(5000);
                     m_isTestCallLua = !m_isTestCallLua;
                     EditorApplication.isPaused = false;
                     break;
                 case EventType.MouseMove:
                     {
                         if (m_picked != null)
                         {
                             HanoiUtil.ForeachInParentChain(m_picked, (n) => { n.highlighted = false; });
                             m_picked = null;
                         }

                         HanoiNode picked = PickHanoiRecursively(m_data.Root.callStats, mousePositionInDetailScreen);
                         if (picked != null)
                         {
                             HanoiUtil.ForeachInParentChain(picked, (n) => { 
                                 n.highlighted = true; 
                             });
                             m_picked = picked;

                             Debug.LogFormat("Picked: f {0}, m {1}", m_picked.funcName, m_picked.moduleName);
                         }
                         else
                         {
                             Debug.LogFormat("Picked nothing.");
                         }

                         if (EditorWindow.focusedWindow == this)
                         {
                             if ((Event.current.mousePosition.x >= 0 && Event.current.mousePosition.x <= m_winWidth) &&
                                 (Event.current.mousePosition.y >= m_detailScreenPosY && Event.current.mousePosition.y <= m_winHeight))
                             {
                                 Repaint();
                             }
                         }
                     }
                     break;
                 case EventType.MouseDrag:
                     if (Event.current.button == 1)
                     {
                         m_Translation.x += Event.current.delta.x;
                         Repaint();
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
                         
                         // Apply zooming
                         m_Scale.x *= scale;

                         Repaint();
                     }
                     break;
                 default:
                     break;
             }
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
 