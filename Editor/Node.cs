using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AmazingNodeEditor
{
    public class Node
    {
        public Rect rect;
        public string title;
        public bool isDragged;
        public bool isSelected;

        public ConnectionPoint inPoint;
        public ConnectionPoint outPoint;

        public GUIStyle style;
        public GUIStyle defaultNodeStyle;
        public GUIStyle selectedNodeStyle;

        public Action<Node> OnRemoveNode;

        public Node(Vector2 position, float width, float height, GUIStyle nodeStyle,
            GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, 
            Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint,
            Action<Node> onClickRemoveNode)
        {
            rect = new Rect(position.x, position.y, width, height);
            style = nodeStyle;
            inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint);
            outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, onClickOutPoint);
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
