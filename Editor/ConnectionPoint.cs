using System;
using System.Xml.Serialization;
using UnityEngine;

namespace AmazingNodeEditor
{
    public enum ConnectionPointType { In, Out }

    public class ConnectionPoint
    {
        public string id;

        [XmlIgnore]
        public Rect rect;
        [XmlIgnore]
        public ConnectionPointType type;
        [XmlIgnore]
        public Node node;

        [XmlIgnore]
        public Action<ConnectionPoint> OnClickConnectionPoint;

        [XmlIgnore]
        public GUIStyle style;

        public ConnectionPoint() { }

        public ConnectionPoint(Node node, ConnectionPointType type,GUIStyle style, 
            Action<ConnectionPoint> onClickConnectionPoint, string id = null)
        {
            this.node = node;
            this.type = type;
            this.style = style;
            this.OnClickConnectionPoint = onClickConnectionPoint;
            rect = new Rect(0, 0, 10f, 20f);

            this.id = id ?? Guid.NewGuid().ToString();
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
