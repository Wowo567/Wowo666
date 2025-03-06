 using System;
using System.Collections.Generic;
using Comic;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class ChatBubblePoint : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer_Grey;
        [SerializeField]
        private SpriteRenderer spriteRenderer_Color;
        
        
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
            spriteRenderer_Grey.enabled = false;
            spriteRenderer_Color.enabled = false;
            
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
                if (_curChatBubbleID > -1)
                {
                    spriteRenderer_Grey.enabled = false;
                    spriteRenderer_Color.enabled = true;
                }
                else
                {
                    spriteRenderer_Grey.enabled = true;
                    spriteRenderer_Color.enabled = false;
                }
            }
        }

        public void OnBubbleExit()
        {
            spriteRenderer_Grey.enabled = false;
            spriteRenderer_Color.enabled = false;
        }

        private void OnDestroy()
        {
            ChatBubblePointManager.Instance?.RemovePoint(this);   
        }
    }
}
