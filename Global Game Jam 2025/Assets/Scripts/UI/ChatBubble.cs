using System;
using System.Collections.Generic;
using System.Linq;
using Comic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class ChatBubble : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
    {
        public BubbleType type;
        public ChatBubbleState state = ChatBubbleState.Free;

        private Collider2D _collider2D;

        private ChatBubbleButton _curChatBubbleButton = null;
        private ChatBubblePoint _curLockedPoint = null;
        
        private ChatBubblePoint _curHoveredPoint = null;
        private List<ChatBubblePoint> _hoveredChatBubblePoints = new List<ChatBubblePoint>();
        
        private void Awake()
        {
            _collider2D = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (state == ChatBubbleState.Hold)
            {
                Vector3 mousePosition = Input.mousePosition;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                transform.position = new Vector3(worldPosition.x, worldPosition.y, 0);
                
                _hoveredChatBubblePoints.Clear();
                foreach (ChatBubblePoint chatBubblePoint in ChatBubblePointManager.Instance.points)
                {
                    if (chatBubblePoint.canAcceptedBubbleIDs.Contains(type) && _collider2D.OverlapPoint(chatBubblePoint.transform.position))
                    {
                        _hoveredChatBubblePoints.Add(chatBubblePoint);
                    }
                }
                float minDis = float.MaxValue;
                ChatBubblePoint _hoverPoint = null;
                foreach (ChatBubblePoint chatBubblePoint in _hoveredChatBubblePoints)
                {
                    float dis = Vector3.Distance(chatBubblePoint.transform.position, transform.position);
                    if (dis < minDis)
                    {
                        minDis = dis;
                        _hoverPoint = chatBubblePoint;
                    }
                }

                if (_curHoveredPoint != _hoverPoint)
                {
                    if (_curHoveredPoint != null)
                    {
                        _curHoveredPoint.OnBubbleExit();   
                    }
                    _curHoveredPoint = _hoverPoint;
                    if (_curHoveredPoint != null)
                    {
                        _curHoveredPoint.OnBubbleHover(type);
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    ReleaseBubble();
                }
            }
        }

        private void ReleaseBubble()
        {
            if (_curHoveredPoint != null)
            {
                LockToPoint(_curHoveredPoint);
            }
            else
            {
                GoHome();   
            }
        }

        public void LockToPoint(ChatBubblePoint chatBubblePoint)
        {
            state = ChatBubbleState.Chat;
            
            gameObject.SetActive(false);
            chatBubblePoint.Bubble(this);
            _curLockedPoint = chatBubblePoint;
            transform.DOMove(chatBubblePoint.transform.position, 0.2f);
        }

        private void GoHome()
        {
            state = ChatBubbleState.Free;
            if (_curChatBubbleButton != null)
            {
                RectTransform rectTransform = _curChatBubbleButton.GetComponent<RectTransform>();
                Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, rectTransform.anchoredPosition);

                if (Camera.main != null)
                {
                    Vector3 newPos = Camera.main.ScreenToWorldPoint(screenPosition);
                    newPos.z = 0;
                    
                    transform.DOMove(newPos, 0.3f).OnComplete(() =>
                    {
                        Destroy(gameObject);
                        _curChatBubbleButton.Release();
                    });
                }
            }
        }

        public void SetHome(ChatBubbleButton chatBubbleButton)
        {
            state = ChatBubbleState.Hold;
            _curChatBubbleButton = chatBubbleButton;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (state == ChatBubbleState.Chat)
            {
                state = ChatBubbleState.Hold;
                _curLockedPoint.Release();
                _curLockedPoint = null;
            }
        }
        
        public void SetFree()
        {
            if (state == ChatBubbleState.Chat)
            {
                gameObject.SetActive(true);
                GoHome();
            }
        }
        
        public void SetHold()
        {
            if (state == ChatBubbleState.Chat)
            {
                gameObject.SetActive(true);
                state = ChatBubbleState.Hold;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {

        }

        private void OnDestroy()
        {
            transform.DOKill();
        }

        public void SetChatBubbleButton(ChatBubbleButton unlockBubbleButton)
        {
            _curChatBubbleButton = unlockBubbleButton;
        }
    }

    public enum ChatBubbleState
    {
        Free,
        Hold,
        Chat,
    }
}