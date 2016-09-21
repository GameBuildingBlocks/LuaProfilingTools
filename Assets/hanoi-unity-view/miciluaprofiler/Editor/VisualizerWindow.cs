 using UnityEngine;
 using UnityEditor;
 using System.Collections.Generic;
 
     public class VisualizerWindow : EditorWindow
     {
         [SerializeField]
         internal Vector2 m_Scale = new Vector2(1.0f, 1.0f);
         [SerializeField]
         internal Vector2 m_Translation = new Vector2(0, 0);
         float m_winWidth = 0.0f;
         float m_winHeight = 0.0f;

         HanoiData m_data = new HanoiData();
         HanoiData m_ShrinkedFrameData = new HanoiData();
         HanoiNode m_picked;

         [MenuItem("Window/VisualizerWindow")]
         static void Create()
         {
             // Get existing open window or if none, make a new one:
             VisualizerWindow window = (VisualizerWindow)EditorWindow.GetWindow(typeof(VisualizerWindow));
             window.Show();
             window.wantsMouseMove = true;
             window.CheckForResizing();
             window.fitScreenSizeScale();
         }

         public VisualizerWindow()
         {
             //m_data.addFrameTag("Assets/hanoi-unity-view/luaprofiler_jx3pocket.json",
             //    "Assets/hanoi-unity-view/luaprofiler_jx3pocket_out.json");

             bool succ = m_data.Load("Assets/hanoi-unity-view/luaprofiler_jx3pocket_out.json");
             if (succ)
             {
                 HanoiUtil.TotalTimeConsuming = (float)m_data.Root.callStats.timeConsuming;
             }
         }

         public void OnGUI()
         {
             CheckForResizing();
             CheckForInput();
             Handles.BeginGUI();
             Handles.matrix = Matrix4x4.TRS(m_Translation, Quaternion.identity, new Vector3(m_Scale.x, m_Scale.y, 1));

             drawFrameInfo(mousePositionInDrawing.x);
             DrawHanoiData(m_data.Root);
             drawTimeInterval();

             showMouseGlobalTime();
             Handles.EndGUI();
             Repaint();
         }

         private void drawFrameInfo(float mouseX) {
             if (mouseX < HanoiData.FrameTagInfoList[0].frameTime || mouseX > HanoiData.FrameTagInfoList[HanoiData.FrameTagInfoList.Count-1].frameTime) return;
             bool isShowFrameInfoAlready =false;
             int  setNextFrameHighlightIndex = -1;
               for(int i =0;i<HanoiData.FrameTagInfoList.Count;i++)
               {
                       HanoiUtil.DrawingShrinkedAccumulated = 0;
                       HanoiUtil.GlobalTimeXToShrinkedPosXSkewing = 0;
                       HanoiUtil.getShrinkedPosXByGlobalTimeX(m_data.Root.callStats,HanoiData.FrameTagInfoList[i].frameTime); 
                       float beginPosX = 0;
                       if (HanoiUtil.GlobalTimeXToShrinkedPosXSkewing <= 0)
                       {
                           Color preColor = Handles.color;
                           Color LineColor = Color.gray;
                           LineColor.a = 0.5f;
                           Handles.color = LineColor;
                           if (i == 0)
                           {
                               beginPosX = HanoiData.FrameTagInfoList[i].frameTime;
                           }
                           else
                            {
                                beginPosX = HanoiData.FrameTagInfoList[i - 1].frameTime - HanoiUtil.DrawingShrinkedAccumulated;
                            }

                           GUI.color = Color.white;
                           if (!isShowFrameInfoAlready && (mouseX + HanoiUtil.DrawingShrinkedAccumulated) < HanoiData.FrameTagInfoList[i].frameTime && !checkMouseXInBlackSpace(m_data.Root.callStats, mouseX))
                           {
                               isShowFrameInfoAlready = true;
                               if (i + 1 != HanoiData.FrameTagInfoList.Count) 
                                   setNextFrameHighlightIndex = i + 1;
                               Rect r = new Rect();
                               r.position = new Vector2(beginPosX, 0);
                               r.width = HanoiVars.LabelBackgroundWidth / 1.5f;
                               r.height = 60;
                               Color bg = Color.white;
                               bg.a = 0.5f;
                               Handles.DrawSolidRectangleWithOutline(r, bg, bg);
                               Handles.Label(new Vector3(beginPosX, 0), string.Format("frameID:{0:0}", HanoiData.FrameTagInfoList[i-1].frameID));
                               Handles.Label(new Vector3(beginPosX, 15), string.Format("frameTime:{0:0.00}", HanoiData.FrameTagInfoList[i-1].frameTime));
                               Handles.Label(new Vector3(beginPosX, 30), string.Format("frameUnityTime:{0:0.00}", HanoiData.FrameTagInfoList[i-1].frameUnityTime));
                               if (i == 0)
                               {
                                   Handles.Label(new Vector3(beginPosX, 45), string.Format("frameInterval:{0:0.00}", HanoiData.FrameTagInfoList[i].frameTime));
                               }
                               else {
                                   Handles.Label(new Vector3(beginPosX, 45), string.Format("frameInterval:{0:0.00}", HanoiData.FrameTagInfoList[i].frameTime - HanoiData.FrameTagInfoList[i - 1].frameTime));
                               }
                               Handles.color = Color.green;
                           }
                           if (i == setNextFrameHighlightIndex)
                               Handles.color = Color.green;
                           Handles.DrawLine(new Vector3(beginPosX, -106), new Vector3(beginPosX, 685));
                           Handles.color = preColor;
                       }
               }
         }

         private bool checkMouseXInBlackSpace(HanoiNode n, float mouseX)
         {
             HanoiUtil.DrawingShrinkedAccumulated = 0;
             HanoiUtil.ISMouseInBlankSpace = false;
             HanoiUtil.checkMouseXInBlankSpace(n,mouseX);
             return HanoiUtil.ISMouseInBlankSpace;
         }

         /// <summary>
         /// 鼠标旁显示全局时间
         /// </summary>
         private void showMouseGlobalTime() {
             Rect r = new Rect();
             r.position = mousePositionInDrawing;
             r.width = HanoiVars.LabelBackgroundWidth/2;
             r.height = 15;
             Color bg = Color.yellow;
             bg.a = 0.5f;
             Handles.DrawSolidRectangleWithOutline(r, bg, bg);
             
             GUI.color = Color.black;
             Handles.color = Color.yellow;
             Handles.DrawLine(new Vector3(mousePositionInDrawing.x, -200), new Vector3(mousePositionInDrawing.x, 1000));
             Handles.Label(new Vector3(mousePositionInDrawing.x, mousePositionInDrawing.y), string.Format("Time: {0:0.000}", mousePositionInDrawing.x + getMouseGlobalTimeShrinkedWidth()));
         }

         private void drawTimeInterval() {
             Handles.color = Color.white;
             float timeInterval = getTimeIntervalByScaleX(m_Scale.x);
             int timeFrameNum = (int)(HanoiUtil.TotalTimeConsuming/getTimeIntervalByScaleX(m_Scale.x));
             for (int i = 0; i < timeFrameNum; i++)
             {
                 HanoiUtil.DrawingBlackSpaceNum = 0;
                 HanoiUtil.DrawingShrinkedAccumulated = 0;
                 HanoiUtil.GlobalTimeXToShrinkedPosXSkewing = 0;
                 HanoiUtil.getShrinkedPosXByGlobalTimeX(m_data.Root.callStats, i * timeInterval);
                 if (HanoiUtil.GlobalTimeXToShrinkedPosXSkewing <= 0)
                 {
                     Handles.DrawLine(new Vector3(i * timeInterval - HanoiUtil.DrawingShrinkedAccumulated, 780), new Vector3(i * timeInterval - HanoiUtil.DrawingShrinkedAccumulated, 830));
                     Handles.Label(new Vector3(i * timeInterval - HanoiUtil.DrawingShrinkedAccumulated, 780), string.Format("{0:0.00}", i * timeInterval));
                 }
             }
         }

         private float getTimeIntervalByScaleX(float scaleX) {
             if (scaleX <= 27) {
                 return 5;
             }
             else if (scaleX <= 65)
             {
                 return 4;
             }else if (scaleX <=125){
                 return 3;
             }
             else if (scaleX <= 295) {
                 return 2;             
             }
             return 1;
         }


         private float getMouseGlobalTimeShrinkedWidth() {
             HanoiUtil.GlobalTimeShrinkedAccumulated = 0;
             HanoiUtil.MouseXInBlankSpaceSkewing = 0;
             HanoiUtil.DrawingShrinkedAccumulated = 0;
             HanoiUtil.checkMouseXInGlobalTimeSkewing(m_data.Root.callStats, mousePositionInDrawing.x);
             return HanoiUtil.GlobalTimeShrinkedAccumulated + HanoiUtil.MouseXInBlankSpaceSkewing;
         }

         /// <summary>
         /// 自动计算scale大小，使图形适配屏幕大小
         /// </summary>
         public void fitScreenSizeScale() {
             HanoiVars.BlankSpaceWidth = GetDrawingLengthByPanelPixels(50);
             HanoiUtil.DrawingBlackSpaceNum = 0;
             HanoiUtil.DrawingShrinkedAccumulated = 0;
             HanoiUtil.DataRecursively(m_data.Root.callStats);
             float blankSpaceWidth = HanoiUtil.DrawingBlackSpaceNum * HanoiVars.BlankSpaceWidth;
             float fitScaleX = (m_winWidth - blankSpaceWidth) / (HanoiUtil.TotalTimeConsuming -  HanoiUtil.DrawingShrinkedAccumulated - blankSpaceWidth);
             m_Scale.x = fitScaleX;
         }

         public void CheckForResizing()
         {
             if (Mathf.Approximately(position.width, m_winWidth) && 
                 Mathf.Approximately(position.height, m_winHeight))
                 return; 

             m_winWidth = position.width;
             m_winHeight = position.height;

             HanoiVars.StackHeight = (m_data.MaxStackLevel != 0) ? (m_winHeight / m_data.MaxStackLevel) : m_winHeight;
         }

         private void DrawHanoiData(HanoiRoot r)
         {
             if (r.callStats == null)
                 return;

             HanoiVars.LabelBackgroundWidth = GetDrawingLengthByPanelPixels(200);
             HanoiVars.BlankSpaceWidth = GetDrawingLengthByPanelPixels(50);
             HanoiVars.DrawnStackCount = m_data.MaxStackLevel;

             HanoiUtil.DrawingCounts = 0;

             // draw 3 passes

             HanoiUtil.DrawingShrinkedTotal = 0;
             HanoiUtil.DrawingShrinkedAccumulated = 0;
             HanoiUtil.DrawBlankSpaceRecursively(r.callStats);

             HanoiUtil.DrawingShrinkedTotal = HanoiUtil.DrawingShrinkedAccumulated;
             HanoiUtil.DrawingShrinkedAccumulated = 0;
             HanoiUtil.DrawRecursively(r.callStats);

             HanoiUtil.DrawLabelsRecursively(r.callStats);
         }

         public Vector2 ViewToDrawingTransformPoint(Vector2 lhs)
         { return new Vector2((lhs.x - m_Translation.x) / m_Scale.x, (lhs.y - m_Translation.y) / m_Scale.y); }
         public Vector3 ViewToDrawingTransformPoint(Vector3 lhs)
         { return new Vector3((lhs.x - m_Translation.x) / m_Scale.x, (lhs.y - m_Translation.y) / m_Scale.y, 0); }

         public Vector2 DrawingToViewTransformVector(Vector2 lhs)
         { return new Vector2(lhs.x * m_Scale.x, lhs.y * m_Scale.y); }
         public Vector3 DrawingToViewTransformVector(Vector3 lhs)
         { return new Vector3(lhs.x * m_Scale.x, lhs.y * m_Scale.y, 0); }

         public Vector2 ViewToDrawingTransformVector(Vector2 lhs)
         { return new Vector2(lhs.x / m_Scale.x, lhs.y / m_Scale.y); }
         public Vector3 ViewToDrawingTransformVector(Vector3 lhs)
         { return new Vector3(lhs.x / m_Scale.x, lhs.y / m_Scale.y, 0); }

         public Vector2 mousePositionInDrawing
         {
             get { return ViewToDrawingTransformPoint(Event.current.mousePosition); }
         }

         public float GetDrawingLengthByPanelPixels(int pixels)
         {
             return Mathf.Abs(ViewToDrawingTransformPoint(new Vector2(pixels, 0)).x - ViewToDrawingTransformPoint(new Vector2(0, 0)).x); 
         }

         private void CheckForInput()
         {
             switch (Event.current.type)
             {
                 case EventType.MouseMove:
                     {
                         if (m_picked != null)
                         {
                             HanoiUtil.ForeachInParentChain(m_picked, (n) => { n.highlighted = false; });
                             m_picked = null;
                         }

                         HanoiNode picked = PickHanoiRecursively(m_data.Root.callStats, mousePositionInDrawing);
                         if (picked != null)
                         {
                             HanoiUtil.ForeachInParentChain(picked, (n) => { n.highlighted = true; });
                             m_picked = picked;

                             Debug.LogFormat("Picked: f {0}, m {1}", m_picked.funcName, m_picked.moduleName);

                             Repaint();
                         }
                         else
                         {
                             Debug.LogFormat("Picked nothing.");
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
                         m_Translation.x -= (mousePositionInDrawing.x - getBlackSpaceShrinkedWidth()) * (scale - 1) * m_Scale.x;
                         
                         // Apply zooming
                         m_Scale.x *= scale;

                         Repaint();
                     }
                     break;

                 default:
                     break;
             }
         }

         private float getBlackSpaceShrinkedWidth() {
             HanoiUtil.DrawingBlackSpaceNum = 0;
             HanoiUtil.DrawingShrinkedAccumulated = 0;
             HanoiUtil.MouseXOnBlankSpaceIndex = 0;
             HanoiUtil.MouseXInBlankSpaceSkewing = 0;
             HanoiUtil.checkMouseXInScroolWheelSkewing(m_data.Root.callStats, mousePositionInDrawing.x);
             return HanoiUtil.MouseXOnBlankSpaceIndex * HanoiVars.BlankSpaceWidth - HanoiUtil.MouseXInBlankSpaceSkewing;
         }

         private HanoiNode PickHanoiRecursively(HanoiNode n, Vector2 mousePos)
         {
             if (!n.HasValidRect())
                 return null;

             if (n.renderRect.xMin > mousePos.x || n.renderRect.xMax < mousePos.x)
                 return null;

             if (n.renderRect.Contains(mousePos))
                 return n;

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
 