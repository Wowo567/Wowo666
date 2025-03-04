using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class ChatBubbleButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
    {
        private bool _isUnlock = false;
        private bool _isUsed = false;
        
        private Image img;
        private Image lockTag;
        
        public GameObject chatBubblePrefab;

        private void Awake()
        {
            img = GetComponent<Image>();
            lockTag = transform.GetChild(0).GetComponent<Image>();
            
            img.raycastTarget = false;
            lockTag.color = new UnityEngine.Color(1, 1, 1, 1);
            img.color = new UnityEngine.Color(1, 1, 1, 0);
        }

        public void Unlock(bool isImmediate)
        {
            if (isImmediate)
            {
                img.raycastTarget = true;
                lockTag.color = new UnityEngine.Color(1, 1, 1, 0);
                img.color = new UnityEngine.Color(1, 1, 1, 1);
            }
            else
            {
                lockTag.DOFade(0, 0.5f).OnComplete(() =>
                {
                    img.raycastTarget = true;
                });
                img.DOFade(1, 0.5f);
            }
        }

    public void OnPointerEnter(PointerEventData eventData)
        {
            if(_isUsed) return;
            transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(_isUsed) return;
            ChatBubbleManager.Instance.CreateChatBubble(this,chatBubblePrefab);
            transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f);
            Bubble();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(_isUsed) return;
            transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f);
        }

        private void Bubble()
        {
            img.color = UnityEngine.Color.gray;
            _isUsed = true;
        }
        
        public void Reset()
        {
            img.color = UnityEngine.Color.white;
            _isUsed = false;
        }
    }
}
