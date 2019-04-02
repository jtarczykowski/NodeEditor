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

        public Node(Vector2 position, float width, float height, GUIStyle nodeStyle,
            GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, 
            Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint,
            Action<Node> onClickRemoveNode, string inPointId = null, string outPointId = null)
        {
            rect = new Rect(position.x, position.y, width, height);
            style = nodeStyle;
            inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint,inPointId);
            outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, onClickOutPoint, outPointId);
            defaultNodeStyle = nodeStyle;
            selectedNodeStyle = selectedStyle;
            OnRemoveNode = onClickRemoveNode;
        }

        public void Drag(Vector2 delta)
        {
            rect.position += delta;
        }

        public void Draw()
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

                    if(e.button == 1 && isSelected && rect.Contains(e.mousePosition))
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

        private const string removeNodeText = "Remove node";
        private void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent(removeNodeText), false, OnClickRemoveNode);
            genericMenu.ShowAsContext();
        }

        private void OnClickRemoveNode()
        {
            OnRemoveNode?.Invoke(this);
        }
    }
}
