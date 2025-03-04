using System;
using System.Collections.Generic;
using System.Linq;
using Comic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D.Animation;

namespace UI
{
    public class ChatBubble : MonoBehaviour,IPointerDownHandler
    {
        public BubbleType type;
        public ChatBubbleState state = ChatBubbleState.Free;

        private Collider2D _collider2D;

        private ChatBubbleButton _curChatBubbleButton = null;
        
        private ChatBubblePoint _curHoveredPoint = null;
        private readonly List<ChatBubblePoint> _hoveredChatBubblePoints = new List<ChatBubblePoint>();
        
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

                List<Collider2D> results = new List<Collider2D>();
                _collider2D.OverlapCollider(new ContactFilter2D(), results);
                foreach (Collider2D result in results)
                {
                    ChatBubblePoint chatBubblePoint = result.GetComponent<ChatBubblePoint>();
                    if (chatBubblePoint)
                    {
                        if (chatBubblePoint.CanAcceptedBubbleIDs.Contains(type))
                        {
                            _hoveredChatBubblePoints.Add(result.GetComponent<ChatBubblePoint>());
                        }
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
            if (_curHoveredPoint)
            {
                TriggerBubble(_curHoveredPoint);
            }
            else
            {
                GoHome();   
            }
        }

        private void TriggerBubble(ChatBubblePoint chatBubblePoint)
        {
            state = ChatBubbleState.Chat;
            
            gameObject.SetActive(false);
            chatBubblePoint.Bubble((int)type-1);
            Destroy(gameObject);
        }

        private void GoHome()
        {
            state = ChatBubbleState.Free;
            if (_curChatBubbleButton != null)
            {
                RectTransform rectTransform = _curChatBubbleButton.GetComponent<RectTransform>();
                Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, rectTransform.position);

                if (Camera.main != null)
                {
                    Vector3 newPos = Camera.main.ScreenToWorldPoint(screenPosition);
                    newPos.z = 0;
                    
                    transform.DOMove(newPos, 0.3f).OnComplete(() =>
                    {
                        Destroy(gameObject);
                        _curChatBubbleButton.Reset();
                    });
                }
            }
            else
            {
                Destroy(gameObject);
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
            }
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }

    public enum ChatBubbleState
    {
        Free,
        Hold,
        Chat,
    }
}