using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using WowoFramework.Singleton;

namespace UI
{
    public class ChatBubblePointManager : MonoBehaviourSingleton<ChatBubblePointManager>
    {
        [NonSerialized]
        public List<ChatBubblePoint> points = new List<ChatBubblePoint>();
        
        public void AddPoint(ChatBubblePoint point)
        {
            points.Add(point);
        }
        
        public void RemovePoint(ChatBubblePoint point)
        {
            points.Remove(point);
        }
    }
}
