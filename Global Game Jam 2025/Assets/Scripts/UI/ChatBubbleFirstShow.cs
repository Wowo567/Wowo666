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
                int index = (int)transform.GetComponent<ChatBubble>().type - 1;
                Debug.Log($"待解锁 {index}");
                _chatBubble.SetChatBubbleButton(
                    ChatBubbleManager.Instance.UnlockBubbleButton(index));
                ChatBubblePoint chatBubblePoint = transform.parent.GetComponent<ChatBubblePoint>();
                _chatBubble.transform.SetParent(null);
                _chatBubble.LockToPoint(chatBubblePoint);
            }
            
            Destroy(this);
        }
    }
}
