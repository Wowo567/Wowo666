using System;
using UnityEngine;

namespace UI
{
    public class ChatBubbleFirstShow : MonoBehaviour
    {
        private ChatBubble _chatBubble;
        
        private void Start()
        {
            _chatBubble = GetComponent<ChatBubble>();
            
            if (transform.parent!=null && transform.parent.GetComponent<ChatBubblePoint>())
            {
                _chatBubble.SetChatBubbleButton(
                    ChatBubbleManager.Instance.UnlockBubbleButton((int)transform.GetComponent<ChatBubble>().type - 1));
                ChatBubblePoint chatBubblePoint = transform.parent.GetComponent<ChatBubblePoint>();
                _chatBubble.transform.SetParent(null);
                _chatBubble.LockToPoint(chatBubblePoint);
            }
            
            Destroy(this);
        }
    }
}
