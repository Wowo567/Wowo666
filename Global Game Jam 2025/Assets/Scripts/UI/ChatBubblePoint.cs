using System;
using System.Collections.Generic;
using Comic;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class ChatBubblePoint : MonoBehaviour
    {
        public Action<BubbleType> OnBubble;
        public Action OnBubbleRemove;

         private int _curChatBubbleID = -1;

        [NonSerialized]
        public List<BubbleType> CanAcceptedBubbleIDs = new List<BubbleType>();
        
        private void Awake()
        {
            ChatBubblePointManager.Instance.AddPoint(this);   
        }

        public void Init(List<BubbleType> validBubbleIDs)
        {
            CanAcceptedBubbleIDs = validBubbleIDs;
        }
        
        public void Bubble(int chatBubbleID)
        {
            if (_curChatBubbleID>=0)
            {
                Release();
            }
            
            _curChatBubbleID = chatBubbleID;
            OnBubble?.Invoke((BubbleType)chatBubbleID);
            
            Debug.Log($"触发气泡类型 {(BubbleType)chatBubbleID}");
        }
        
        public void Release()
        {
            ChatBubbleManager.Instance.ResetBubble(_curChatBubbleID);
            Debug.Log($"移除气泡类型 {(BubbleType)_curChatBubbleID}");
            _curChatBubbleID = -1;
            OnBubbleRemove?.Invoke();
        }

        public void OnBubbleHover(BubbleType type)
        {
            if (CanAcceptedBubbleIDs.Contains(type))
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
            ChatBubblePointManager.Instance?.RemovePoint(this);   
        }
    }
}
