using System;
using UnityEngine;

namespace UI
{
    public class ChatBubbleFirstShow : MonoBehaviour
    {
        private ChatBubble _chatBubble;
        
        private void Awake()
        {
            _chatBubble = GetComponent<ChatBubble>();
            
            _chatBubble.SetChatBubbleButton(ChatBubbleManager.Instance.UnlockBubbleButton());
            
            if (transform.parent!=null && transform.parent.GetComponent<ChatBubblePoint>())
            {
                ChatBubblePoint chatBubblePoint = transform.parent.GetComponent<ChatBubblePoint>();
                _chatBubble.transform.SetParent(null);
                _chatBubble.LockToPoint(chatBubblePoint);
            }
        }
    }
}
