using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AmazingNodeEditor
{
    public class NodeBasedEditor : EditorWindow
    {
        private List<Node> nodes;
        private List<Connection> connections;

        private GUIStyle nodeStyle;
        private GUIStyle selectedNodeStyle;
        private GUIStyle inPointStyle;
        private GUIStyle outPointStyle;

        private ConnectionPoint selectedInPoint;
        private ConnectionPoint selectedOutPoint;

        private Vector2 offset;
        private Vector2 drag;

        [MenuItem("Window/NodeBasedEditor")]
        private static void OpenWindow()
        {
            NodeBasedEditor window = GetWindow<NodeBasedEditor>();
            window.titleContent = new GUIContent("Node Based Editor");
        }

        private const string defaultSkinPath = "builtin skins/darkskin/images/node1.png";
        private const string defaultActiveSkinPath = "builtin skins/darkskin/images/node1 on.png";
        private const string defaultInPointSkinPath = "builtin skins/darkskin/images/btn left.png";
        private const string defaultInPointActiveSkinPath = "builtin skins/darkskin/images/btn left on.png";
        private const string defaultOutPointSkinPath = "builtin skins/darkskin/images/btn right.png";
        private const string defaultOutPointActiveSkinPath = "builtin skins/darkskin/images/btn right on.png";
        private const int defaultNodeSize = 12;
        private const int defaultPointSize = 4;

        private void OnEnable()
        {
            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load(defaultSkinPath) as Texture2D;
            nodeStyle.border = new RectOffset(defaultNodeSize, defaultNodeSize, defaultNodeSize, defaultNodeSize);

            selectedNodeStyle = new GUIStyle();
            selectedNodeStyle.normal.background = EditorGUIUtility.Load(defaultSkinPath) as Texture2D;
            selectedNodeStyle.border = new RectOffset(defaultNodeSize, defaultNodeSize, defaultNodeSize, defaultNodeSize);

            inPointStyle = new GUIStyle();
            inPointStyle.normal.background = EditorGUIUtility.Load(defaultInPointSkinPath) as Texture2D;
            inPointStyle.active.background = EditorGUIUtility.Load(defaultInPointActiveSkinPath) as Texture2D;
            inPointStyle.border = new RectOffset(defaultPointSize, defaultPointSize, defaultNodeSize, defaultNodeSize);

            outPointStyle = new GUIStyle();
            outPointStyle.normal.background = EditorGUIUtility.Load(defaultOutPointSkinPath) as Texture2D;
            outPointStyle.active.background = EditorGUIUtility.Load(defaultOutPointActiveSkinPath) as Texture2D;
            outPointStyle.border = new RectOffset(defaultPointSize, defaultPointSize, defaultNodeSize, defaultNodeSize);
        }

        private float smallGridSpacing = 20f;
        private float largeGridSpacing = 100f;
        private float smallGridOpacity = 0.2f;
        private float largeGridOpacity = 0.4f;

        private void OnGUI()
        {
            DrawGrid(smallGridSpacing,smallGridOpacity,Color.gray);
            DrawGrid(largeGridSpacing, largeGridOpacity, Color.gray);

            DrawNodes();
            DrawConnections();
            DrawConnectionLine(Event.current);

            ProcessNodeEvents(Event.current);
            ProcessEvents(Event.current);
            if (GUI.changed)
            {
                Repaint();
            }
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color color)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(color.r, color.g, color.b, gridOpacity);

            offset += drag * 0.5f;
            var newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for(int i = 0; i < widthDivs; ++i)
            {
                var beg = new Vector3(gridSpacing * i, -gridSpacing, 0f) + newOffset;
                var end = new Vector3(gridSpacing * i, position.height, 0f) + newOffset;
                Handles.DrawLine(beg, end);
            }

            for (int j = 0; j < heightDivs; ++j)
            {
                var beg = new Vector3(-gridSpacing, gridSpacing * j, 0f) + newOffset;
                var end = new Vector3(position.width,gridSpacing * j, 0f) + newOffset;
                Handles.DrawLine(beg, end);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private static Vector2 bezierOffset = Vector2.left * 50f;
        private const float defaultHandleWidth = 2f;

        private void DrawConnectionLine(Event e)
        {
            if(selectedInPoint != null && selectedOutPoint == null)
            {
                var selectedInCenter = selectedInPoint.rect.center;

                Handles.DrawBezier(selectedInCenter, e.mousePosition,
                    selectedInCenter + bezierOffset, e.mousePosition - bezierOffset,
                    Color.white, null, defaultHandleWidth);

                GUI.changed = true;
            }

            if (selectedOutPoint != null && selectedInPoint == null)
            {
                var selectedOutCenter = selectedOutPoint.rect.center;

                Handles.DrawBezier(selectedOutCenter, e.mousePosition,
                    selectedOutCenter - bezierOffset, e.mousePosition + bezierOffset,
                    Color.white, null, defaultHandleWidth);

                GUI.changed = true;
            }
        }

        private void DrawConnections()
        {
            if(connections != null)
            {
                for(int i = 0; i < connections.Count; ++i)
                {
                    connections[i].Draw();
                }
            }
        }

        private void ProcessNodeEvents(Event e)
        {
            if (nodes == null)
            {
                return;
            }

            for (int i = nodes.Count - 1; i >= 0; --i)
            {
                bool guiChanged = nodes[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }

        private void ProcessEvents(Event e)
        {
            drag = Vector2.zero;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if(e.button == 0)
                    {
                        ClearConnectionSelection();
                    }

                    if (e.button == 1)
                    {
                        ProcessContextMenu(e.mousePosition);
                    }
                    break;

                case EventType.MouseDrag:
                    if(e.button == 0)
                    {
                        OnDrag(e.delta);
                    }
                    break;
            }
        }

        private void OnDrag(Vector2 delta)
        {
            drag = delta;

            if(nodes != null)
            {
                for(int i = 0; i < nodes.Count; ++i)
                {
                    nodes[i].Drag(delta);
                }
            }

            GUI.changed = true;
        }

        private void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Add node"), false,
                () => OnClickAddNode(mousePosition));
            genericMenu.ShowAsContext();
        }

        private const float defaultNodeWidth = 200;
        private const float defaultNodeHeight = 100;

        private void OnClickAddNode(Vector2 mousePosition)
        {
            if (nodes == null)
            {
                nodes = new List<Node>();
            }

            nodes.Add(new Node(mousePosition, defaultNodeWidth, defaultNodeHeight, nodeStyle,
                selectedNodeStyle, inPointStyle,outPointStyle,OnClickInPoint,OnClickOutPoint,OnClickRemoveNode));
        }

        private void OnClickRemoveNode(Node node)
        {
           if(connections != null)
           {
                var connectionsToRemove = new List<Connection>();

                for (int i = 0; i < connections.Count; ++i)
                {
                    if (connections[i].inPoint == node.inPoint || connections[i].outPoint == node.outPoint)
                    {
                        connectionsToRemove.Add(connections[i]);
                    }
                }

                for (int i = 0; i < connectionsToRemove.Count; ++i)
                {
                    connections.Remove(connectionsToRemove[i]);
                }

                connectionsToRemove = null;
            }

           nodes.Remove(node);
        }

        private void OnClickInPoint(ConnectionPoint inPoint)
        {
            selectedInPoint = inPoint;

            if(selectedOutPoint != null)
            {
                if (selectedOutPoint.node != selectedInPoint.node)
                {
                    CreateConnection();
                    
                }
                ClearConnectionSelection();
            }
        }

        private void OnClickOutPoint(ConnectionPoint outPoint)
        {
            selectedOutPoint = outPoint;

            if (selectedInPoint != null)
            {
                if (selectedOutPoint.node != selectedInPoint.node)
                {
                    CreateConnection();

                }
                ClearConnectionSelection();
            }
        }

        private void ClearConnectionSelection()
        {
            selectedInPoint = null;
            selectedOutPoint = null;
        }

        private void CreateConnection()
        {
            if(connections == null)
            {
                connections = new List<Connection>();
            }

            connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
        }

        private void OnClickRemoveConnection(Connection connection)
        {
            connections.Remove(connection);
        }

        private void DrawNodes()
        {
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count; ++i)
                {
                    nodes[i].Draw();
                }
            }
        }
    }
}
