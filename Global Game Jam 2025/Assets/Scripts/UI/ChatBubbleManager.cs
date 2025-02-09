using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WowoFramework.Singleton;

namespace UI
{
    public class ChatBubbleManager : MonoBehaviourSingleton<ChatBubbleManager>
    {
        public float height = 200;
        private int _bubbleIndex = -1;

        private readonly List<RectTransform> _bubbles = new List<RectTransform>();

        protected override void Awake()
        {
            base.Awake();
        }

        public void CreateChatBubble(ChatBubbleButton chatBubbleButton, GameObject chatBubblePrefab)
        {
            Instantiate(chatBubblePrefab).GetComponent<ChatBubble>().SetHome(chatBubbleButton);
        }

        [Button]
        public ChatBubbleButton UnlockBubbleButton(int index = -1)
        {
            if (index == -1)
            {
                _bubbleIndex++;
            }
            else
            {
                if (_bubbleIndex + 1 != index)
                {
                    Debug.Log($"{_bubbleIndex} xxxxx {index}");
                    return null;
                }
                else
                {
                    _bubbleIndex++;
                }
            }
            transform.GetChild(_bubbleIndex).gameObject.SetActive(true);
            _bubbles.Add(transform.GetChild(_bubbleIndex).GetComponent<RectTransform>());
            if (_bubbleIndex > 0)
            {
                transform.GetChild(_bubbleIndex).GetComponent<ChatBubbleButton>().Bubble();
            }
            RefreshLayout();
            return transform.GetChild(_bubbleIndex).GetComponent<ChatBubbleButton>();
        }

        private void RefreshLayout()
        {
            int count = _bubbles.Count;
            float i = 0;
            foreach (RectTransform rectTransform in _bubbles)
            {
                Vector2 pos = new Vector2(0, -i * height + (float)(count-1) / 2 * height);
                if (i < count - 1)
                {
                    rectTransform.DOAnchorPos(pos, 0.2f);
                }
                else
                {
                    rectTransform.anchoredPosition = pos;
                    rectTransform.DOScale(new Vector2(1, 1), 0.2f);
                }
                i++;
            }
        }
    }
}