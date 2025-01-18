using System;
using System.Collections.Generic;
using Comic;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class ChatBubblePoint : MonoBehaviour
    {
        [NonSerialized]
        public bool isUsed = false;

        public Action<BubbleType> OnBubble;
        public Action OnBubbleRemove;

        [NonSerialized] private ChatBubble _curChatBubble;

        [NonSerialized]
        public List<BubbleType> canAcceptedBubbleIDs = new List<BubbleType>();
        
        private void Awake()
        {
            ChatBubblePointManager.Instance.AddPoint(this);
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
        }
        
        public void Release()
        {
            isUsed = false;
            _curChatBubble.SetFree();
            _curChatBubble = null;
            OnBubbleRemove?.Invoke();
            Debug.Log("ReleaseBubble!!!");
        }

        public void OnBubbleHover(BubbleType type)
        {
            if (canAcceptedBubbleIDs.Contains(type))
            {
                transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f);
                transform.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        public void OnBubbleExit()
        {
            transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f);
            transform.GetComponent<SpriteRenderer>().color = Color.gray;
        }

        private void OnDestroy()
        {
            _curChatBubble?.SetFree();
            ChatBubblePointManager.Instance?.RemovePoint(this);
        }
    }
}
