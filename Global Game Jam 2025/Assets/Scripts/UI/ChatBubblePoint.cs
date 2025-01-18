using System;
using System.Collections.Generic;
using Comic;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class ChatBubblePoint : MonoBehaviour
    {
        public bool x = false;
        
        [NonSerialized]
        public bool isUsed = false;

        public Action<BubbleType> OnBubble;
        public Action OnBubbleRemove;

        [NonSerialized] private ChatBubble _curChatBubble;

        [NonSerialized]
        public List<BubbleType> canAcceptedBubbleIDs = new List<BubbleType>();
        
        private void Awake()
        {
            if (!x)
            {
                ChatBubblePointManager.Instance.AddPoint(this);   
            }
        }

        public void Init(List<BubbleType> validBubbleIDs)
        {
            canAcceptedBubbleIDs = validBubbleIDs;
        }
        
        public void Bubble(ChatBubble chatBubble)
        {
            if (_curChatBubble)
            {
                Release();
            }
            
            isUsed = true;
            _curChatBubble = chatBubble;
            OnBubble?.Invoke(chatBubble.type);
            Debug.Log($"Bubble!!! {_curChatBubble.type}");
            transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f);
        }
        
        public void Release(bool isHold = false)
        {
            isUsed = false;
            if (_curChatBubble)
            {
                if (isHold)
                {
                    _curChatBubble.SetHold();
                }
                else
                {
                    _curChatBubble.SetFree();
                }
            }
            _curChatBubble = null;
            OnBubbleRemove?.Invoke();
            Debug.Log("ReleaseBubble!!!");
        }

        public void OnBubbleHover(BubbleType type)
        {
            if (canAcceptedBubbleIDs.Contains(type))
            {
                transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f);
            }
        }

        public void OnBubbleExit()
        {
            transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f);
        }

        private void OnDestroy()
        {
            _curChatBubble?.SetFree();
            if (!x)
            {
                ChatBubblePointManager.Instance?.RemovePoint(this);   
            }
        }
    }
}
