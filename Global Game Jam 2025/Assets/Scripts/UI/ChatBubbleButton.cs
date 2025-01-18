using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class ChatBubbleButton : MonoBehaviour,IPointerEnterHandler,IPointerDownHandler,IPointerExitHandler
    {
        private bool _isUsed = false;
        
        public GameObject chatBubblePrefab;

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

        public void Bubble()
        {
            transform.GetComponent<Image>().color = UnityEngine.Color.gray;
            _isUsed = true;
        }
        
        public void Release()
        {
            transform.GetComponent<Image>().color = UnityEngine.Color.white;
            _isUsed = false;
        }
    }
}
