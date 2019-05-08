using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace AmazingNodeEditor
{
    public class Node
    {
        public Rect rect;

        [XmlIgnore]
        public string title;
        [XmlIgnore]
        public bool isDragged;
        [XmlIgnore]
        public bool isSelected;

        public ConnectionPoint inPoint;
        public ConnectionPoint outPoint;

        [XmlIgnore]
        public GUIStyle style;
        [XmlIgnore]
        public GUIStyle defaultNodeStyle;
        [XmlIgnore]
        public GUIStyle selectedNodeStyle;

        [XmlIgnore]
        public Action<Node> OnRemoveNode;

        public Node() { }

        public Node(Vector2 position, Vector2 dimensions, NodeStyleInfo styleInfo, 
            Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint,
            Action<Node> onClickRemoveNode, string inPointId = null, string outPointId = null)
        {
            rect = new Rect(position.x, position.y, dimensions.x, dimensions.y);
            style = styleInfo.defaultNodeStyle;
            inPoint = new ConnectionPoint(this, ConnectionPointType.In, styleInfo.inPointStyle, onClickInPoint,inPointId);
            outPoint = new ConnectionPoint(this, ConnectionPointType.Out, styleInfo.outPointStyle, onClickOutPoint, outPointId);
            defaultNodeStyle = styleInfo.defaultNodeStyle;
            selectedNodeStyle = styleInfo.selectedNodeStyle;
            OnRemoveNode = onClickRemoveNode;
        }

        public void Drag(Vector2 delta)
        {
            rect.position += delta;
        }

        public virtual void Draw()
        {
            inPoint.Draw();
            outPoint.Draw();
            GUI.Box(rect, title, style);
        }

        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (rect.Contains(e.mousePosition))
                        {
                            isDragged = true;
                            GUI.changed = true;
                            isSelected = true;
                            style = selectedNodeStyle;
                        }
                        else
                        {
                            GUI.changed = true;
                            isSelected = false;
                            style = defaultNodeStyle;
                        }
                    }

                    if(e.button == 1 && rect.Contains(e.mousePosition))
                    {
                        ProcessContextMenu();
                        e.Use();
                    }

                    break;

                case EventType.MouseUp:
                    isDragged = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged)
                    {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;
            }

            return false;
        }

        protected const string removeNodeText = "Remove node";
        protected virtual void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent(removeNodeText), false, OnClickRemoveNode);
            genericMenu.ShowAsContext();
        }

        protected void OnClickRemoveNode()
        {
            OnRemoveNode?.Invoke(this);
        }
    }

    public struct NodeStyleInfo
    {
        public GUIStyle currentStyle;
        public GUIStyle defaultNodeStyle;
        public GUIStyle selectedNodeStyle;
        public GUIStyle inPointStyle;
        public GUIStyle outPointStyle;
    }
}
