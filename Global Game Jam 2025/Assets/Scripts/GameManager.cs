using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WowoFramework.Singleton;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    public Action OnContinue;

    public Action OnGameOver;

    [SerializeField] private Transform title;

    [SerializeField] private TextMesh textMesh;

    [SerializeField] private Image bubbleBar;

    private bool _isFirstContinue = true;

    protected override void Awake()
    {
        base.Awake();

        title.localScale = new Vector3(0, 0, 0);
        textMesh.color = new UnityEngine.Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 0);
        bubbleBar.color = new UnityEngine.Color(bubbleBar.color.r, bubbleBar.color.g, bubbleBar.color.b, 0);
        
        ShowText();
    }

    [Button]
    public void ShowText()
    {
        DOTween.To(() => textMesh.color, x => textMesh.color = x,
            new UnityEngine.Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 1), 1f);
    }
    
    [Button]
    public void ShowTitle()
    {
        DOTween.To(() => title.localScale, x => title.localScale = x,
            new Vector3(1, 1, 1), 0.5f);
    }
    
    [Button]
    public void ShowBubbleBar()
    {
        DOTween.To(() => bubbleBar.color, x => bubbleBar.color = x,
            new UnityEngine.Color(bubbleBar.color.r, bubbleBar.color.g, bubbleBar.color.b, 1), 0.5f);
    }
    
    [Button]
    public void Continue()
    {
        if (_isFirstContinue)
        {
            _isFirstContinue = false;
            ShowBubbleBar();
            ChatBubbleManager.Instance.UnlockBubbleButton();
        }
        PaperManager.Instance.CreateNewPaper();
        OnContinue?.Invoke();
    }

    public void GameOver()
    {
        OnGameOver?.Invoke();
    }

    public void HideUI()
    {
        Destroy(bubbleBar.gameObject);
    }
}
