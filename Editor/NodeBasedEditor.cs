using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AmazingNodeEditor
{
    public class NodeBasedEditor : EditorWindow
    {
        protected List<Node> nodes;
        protected List<Connection> connections;

        protected NodeStyleInfo defaultNodeStyle;

        protected NodeConnectionPoint selectedInPoint;
        protected NodeConnectionPoint selectedOutPoint;

        protected Vector2 offset;
        protected Vector2 drag;

        protected float menuBarHeight = 20f;
        protected Rect menuBar;

        [MenuItem("Window/NodeBasedEditor")]
        protected static void OpenWindow()
        {
            NodeBasedEditor window = GetWindow<NodeBasedEditor>();
            window.titleContent = new GUIContent("Node Based Editor");
        }

        protected const string defaultSkinPath = "builtin skins/darkskin/images/node1.png";
        protected const string defaultActiveSkinPath = "builtin skins/darkskin/images/node1 on.png";
        protected const string defaultInPointSkinPath = "builtin skins/darkskin/images/btn left.png";
        protected const string defaultInPointActiveSkinPath = "builtin skins/darkskin/images/btn left on.png";
        protected const string defaultOutPointSkinPath = "builtin skins/darkskin/images/btn right.png";
        protected const string defaultOutPointActiveSkinPath = "builtin skins/darkskin/images/btn right on.png";
        protected const int defaultNodeSize = 12;
        protected const int defaultPointSize = 4;

        protected virtual void OnEnable()
        {
            defaultNodeStyle = new NodeStyleInfo();

            defaultNodeStyle.defaultNodeStyle = new GUIStyle();
            defaultNodeStyle.defaultNodeStyle.normal.background = EditorGUIUtility.Load(defaultSkinPath) as Texture2D;
            defaultNodeStyle.defaultNodeStyle.border = new RectOffset(defaultNodeSize, defaultNodeSize, defaultNodeSize, defaultNodeSize);

            defaultNodeStyle.selectedNodeStyle = new GUIStyle();
            defaultNodeStyle.selectedNodeStyle.normal.background = EditorGUIUtility.Load(defaultSkinPath) as Texture2D;
            defaultNodeStyle.selectedNodeStyle.border = new RectOffset(defaultNodeSize, defaultNodeSize, defaultNodeSize, defaultNodeSize);

            defaultNodeStyle.inPointStyle = new GUIStyle();
            defaultNodeStyle.inPointStyle.normal.background = EditorGUIUtility.Load(defaultInPointSkinPath) as Texture2D;
            defaultNodeStyle.inPointStyle.active.background = EditorGUIUtility.Load(defaultInPointActiveSkinPath) as Texture2D;
            defaultNodeStyle.inPointStyle.border = new RectOffset(defaultPointSize, defaultPointSize, defaultNodeSize, defaultNodeSize);

            defaultNodeStyle.outPointStyle = new GUIStyle();
            defaultNodeStyle.outPointStyle.normal.background = EditorGUIUtility.Load(defaultOutPointSkinPath) as Texture2D;
            defaultNodeStyle.outPointStyle.active.background = EditorGUIUtility.Load(defaultOutPointActiveSkinPath) as Texture2D;
            defaultNodeStyle.outPointStyle.border = new RectOffset(defaultPointSize, defaultPointSize, defaultNodeSize, defaultNodeSize);
        }

        protected float smallGridSpacing = 20f;
        protected float largeGridSpacing = 100f;
        protected float smallGridOpacity = 0.2f;
        protected float largeGridOpacity = 0.4f;

        protected void OnGUI()
        {
            DrawGrid(smallGridSpacing,smallGridOpacity,Color.gray);
            DrawGrid(largeGridSpacing, largeGridOpacity, Color.gray);
            DrawMenuBar();

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

        protected void DrawGrid(float gridSpacing, float gridOpacity, Color color)
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

        protected static Vector2 bezierOffset = Vector2.left * 50f;
        protected const float defaultHandleWidth = 2f;

        protected void DrawConnectionLine(Event e)
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

        protected void DrawConnections()
        {
            if(connections != null)
            {
                for(int i = 0; i < connections.Count; ++i)
                {
                    connections[i].Draw();
                }
            }
        }

        protected void ProcessNodeEvents(Event e)
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

        protected void ProcessEvents(Event e)
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

        protected void OnDrag(Vector2 delta)
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

        protected virtual void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Add node"), false,
                () => OnClickAddNode(mousePosition));
            genericMenu.ShowAsContext();
        }

        protected Vector2 defaultNodeDimensions = new Vector2(400, 200);

        protected Node CreateNode(Vector2 mousePosition)
        {
            return new Node(mousePosition,
                defaultNodeDimensions,
                defaultNodeStyle,
                OnClickInPoint,
                OnClickOutPoint,
                OnClickRemoveNode);
        }

        protected void OnClickAddNode(Vector2 mousePosition)
        {
            if (nodes == null)
            {
                nodes = new List<Node>();
            }

            nodes.Add(CreateNode(mousePosition));
        }

        protected void OnClickRemoveNode(Node node)
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

        protected void OnClickInPoint(ConnectionPointBase inPoint)
        {
            selectedInPoint = inPoint as NodeConnectionPoint;

            if(selectedOutPoint != null)
            {
                if (selectedOutPoint.node != selectedInPoint.node)
                {
                    CreateConnection();
                    
                }
                ClearConnectionSelection();
            }
        }

        protected void OnClickOutPoint(ConnectionPointBase outPoint)
        {
            selectedOutPoint = outPoint as NodeConnectionPoint;

            if (selectedInPoint != null)
            {
                if (selectedOutPoint.node != selectedInPoint.node)
                {
                    CreateConnection();

                }
                ClearConnectionSelection();
            }
        }

        protected void ClearConnectionSelection()
        {
            selectedInPoint = null;
            selectedOutPoint = null;
        }

        protected void CreateConnection()
        {
            if(connections == null)
            {
                connections = new List<Connection>();
            }

            connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
        }

        protected void OnClickRemoveConnection(Connection connection)
        {
            connections.Remove(connection);
        }

        protected void DrawNodes()
        {
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count; ++i)
                {
                    nodes[i].Draw();
                }
            }
        }

        protected void DrawMenuBar()
        {
            menuBar = new Rect(0, 0, position.width, menuBarHeight);

            GUILayout.BeginArea(menuBar, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button(new GUIContent("Save"), EditorStyles.toolbarButton, GUILayout.Width(35)))
            {
                Save();
            }
            
            GUILayout.Space(5);

            if(GUILayout.Button(new GUIContent("Load"), EditorStyles.toolbarButton, GUILayout.Width(35)))
            {
                Load();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        protected string nodesPath = $"{Application.streamingAssetsPath}/nodes.xml";
        protected string connectionsPath = $"{Application.streamingAssetsPath}/connections.xml";

        protected void Save()
        {
            XMLSaver.Serialize(nodes, nodesPath);
            XMLSaver.Serialize(connections, connectionsPath);
        }

        protected void Load()
        {
            var nodesDeserialized = XMLSaver.Deserialize<List<Node>>(nodesPath);
            var connectionsDeserialized = XMLSaver.Deserialize<List<Connection>>(connectionsPath);

            nodes = new List<Node>();
            connections = new List<Connection>();

            foreach (var node in nodesDeserialized)
            {
                var dim = new Vector2(node.rect.width, node.rect.height);

                nodes.Add(new Node(
                node.rect.position,
                dim,
                defaultNodeStyle,
                OnClickInPoint,
                OnClickOutPoint,
                OnClickRemoveNode,
                node.inPoint.id,
                node.outPoint.id
                ));
            }

            foreach(var connection in connectionsDeserialized)
            {
                var inPoint = nodes.First(n => n.inPoint.id == connection.inPoint.id).inPoint;
                var outPoint = nodes.First(n => n.outPoint.id == connection.outPoint.id).outPoint;
                connections.Add(new Connection(inPoint, outPoint, OnClickRemoveConnection));
            }
        }
    }
}
