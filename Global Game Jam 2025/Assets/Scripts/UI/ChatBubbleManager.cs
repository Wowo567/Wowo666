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
        private static readonly string UnlockedBubbleStateKey = "UnlockedBubbleState";

        private ChatBubbleButton[] _bubbleButtons;
        private CanvasGroup _canvasGroup;

        protected override void Awake()
        {
            base.Awake();

            _bubbleButtons = GetComponentsInChildren<ChatBubbleButton>();
            _canvasGroup = GetComponent<CanvasGroup>();
            
            ComicManager.Instance.OnBubbleUnlock += OnBubbleUnlock;
            
            int unlockedBubbleState = PlayerPrefs.GetInt(UnlockedBubbleStateKey, 0);
            Debug.Log("当前气泡解锁状态:" + Convert.ToString(unlockedBubbleState,2));
            for (int i = 0; i < 4; i++)
            {
                if ((unlockedBubbleState & (1 << i)) != 0)
                {
                    _bubbleButtons[i].Unlock(true);
                }
            }
        }

        public void CreateChatBubble(ChatBubbleButton chatBubbleButton, GameObject chatBubblePrefab)
        {
            Instantiate(chatBubblePrefab, chatBubbleButton.transform.position, chatBubbleButton.transform.rotation)
                .GetComponent<ChatBubble>().SetHome(chatBubbleButton);
        }

        [Button]
        private void OnBubbleUnlock(int id)
        {
            if (id is >= 0 and < 4)
            {
                int unlockedBubbleState = PlayerPrefs.GetInt(UnlockedBubbleStateKey, 0);
                if ((unlockedBubbleState & (1 << id)) == 0)
                {
                    unlockedBubbleState = unlockedBubbleState | 1 << id;
                    PlayerPrefs.SetInt(UnlockedBubbleStateKey, unlockedBubbleState);
                    _bubbleButtons[id].Unlock(false);
                }
            }
        }

        [Button]
        private void ResetUnlockState()
        {
            PlayerPrefs.SetInt(UnlockedBubbleStateKey, 0);
        }
        
        public void ResetBubble(int curChatBubbleID)
        {
            _bubbleButtons[curChatBubbleID].Reset();
        }

        public void ShowUI()
        {
            _canvasGroup.DOFade(1, 0.5f);
        }
        
        public void HideUI()
        {
            _canvasGroup.DOFade(0, 0.5f);
        }
    }
}