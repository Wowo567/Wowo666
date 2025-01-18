using System;
using Comic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class ChatBubbleInComic : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler
    {
        public BubbleType type;
        
        private ChatBubblePoint chatBubblePoint;

        private void Awake()
        {
            chatBubblePoint = transform.GetComponentInParent<ComicItem>().GetComponentInChildren<ChatBubblePoint>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            gameObject.SetActive(false);
            chatBubblePoint.Release(true);
        }
    }
}
