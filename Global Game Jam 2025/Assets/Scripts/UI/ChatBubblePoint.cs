using System;
using UnityEngine;

namespace UI
{
    public class ChatBubblePoint : MonoBehaviour
    {
        public bool isUsed = false;

        public Action<int> OnBubble;
        public Action OnBubbleRemove;

        public ChatBubble curChatBubble;
        
        private void Awake()
        {
            ChatBubblePointManager.Instance.AddPoint(this);
        }

        public void Bubble(ChatBubble chatBubble)
        {
            if (curChatBubble)
            {
                Release();
            }
            
            isUsed = true;
            curChatBubble = chatBubble;
            OnBubble?.Invoke(chatBubble.ID);
            Debug.Log($"Bubble!!! {curChatBubble.ID}");
        }
        
        public void Release()
        {
            isUsed = false;
            curChatBubble.SetFree();
            curChatBubble = null;
            OnBubbleRemove?.Invoke();
            Debug.Log("ReleaseBubble!!!");
        }

        private void OnDestroy()
        {
            curChatBubble?.SetFree();
            ChatBubblePointManager.Instance?.RemovePoint(this);
        }
    }
}
