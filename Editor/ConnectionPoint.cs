using System;
using UnityEngine;

namespace AmazingNodeEditor
{
    public enum ConnectionPointType { In, Out }

    public class ConnectionPoint
    {
        public Rect rect;
        public ConnectionPointType type;
        public Node node;
        public Action<ConnectionPoint> OnClickConnectionPoint;

        public GUIStyle style;

        public ConnectionPoint(Node node, ConnectionPointType type,GUIStyle style, 
            Action<ConnectionPoint> onClickConnectionPoint)
        {
            this.node = node;
            this.type = type;
            this.style = style;
            this.OnClickConnectionPoint = onClickConnectionPoint;
            rect = new Rect(0, 0, 10f, 20f);
        }

        private float connectionDistanceFromBox = 8f;
        public void Draw()
        {
            rect.y = node.rect.y + (node.rect.height * 0.5f) - (rect.height * 0.5f);

            switch(type)
            {
                case ConnectionPointType.In:
                    rect.x = node.rect.x - rect.width + connectionDistanceFromBox;
                    break;

                case ConnectionPointType.Out:
                    rect.x = node.rect.x + node.rect.width - connectionDistanceFromBox;
                    break;
            }

            if(GUI.Button(rect, "", style))
            {
                OnClickConnectionPoint?.Invoke(this);
            }
        }
    }
}
