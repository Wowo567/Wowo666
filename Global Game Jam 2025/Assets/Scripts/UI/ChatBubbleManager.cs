using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WowoFramework.Singleton;

namespace UI
{
    public class ChatBubbleManager : MonoBehaviourSingleton<ChatBubbleManager>
    {
        public float space;
        
        public List<ChatBubble> chatBubbles = new List<ChatBubble>();
        public List<ChatBubbleButton> chatBubbleButtons = new List<ChatBubbleButton>();

        public void CreateChatBubble(ChatBubbleButton chatBubbleButton, GameObject chatBubblePrefab)
        {
            Instantiate(chatBubblePrefab).GetComponent<ChatBubble>().SetHome(chatBubbleButton);
        }
    }
}
