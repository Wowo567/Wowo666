using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class ContinueButton : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler
    {
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
            transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f);
            GameManager.Instance.Continue();
        }
    }
}
