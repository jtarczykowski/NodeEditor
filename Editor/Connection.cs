using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AmazingNodeEditor
{
    public class Connection
    {
        public ConnectionPoint inPoint;
        public ConnectionPoint outPoint;
        public Action<Connection> OnClickRemoveConnection;

        public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, Action<Connection> onClickRemoveConnection)
        {
            this.inPoint = inPoint;
            this.outPoint = outPoint;
            this.OnClickRemoveConnection = onClickRemoveConnection;
        }

        private static Vector2 bezierOffset = Vector2.left * 50f;
        private const float defaultHandleWidth = 2f;

        public void Draw()
        {
            var inCenter = inPoint.rect.center;
            var outCenter = outPoint.rect.center;

            Handles.DrawBezier(inCenter, outCenter,
                inCenter + bezierOffset, outCenter - bezierOffset,
                Color.white, null, defaultHandleWidth);

            bool removeClicked = Handles.Button((inCenter + outCenter) * 0.5f,
                Quaternion.identity, 4, 8, Handles.RectangleHandleCap);

            if (removeClicked)
            {
                OnClickRemoveConnection?.Invoke(this);
            }
        }
    }
}
