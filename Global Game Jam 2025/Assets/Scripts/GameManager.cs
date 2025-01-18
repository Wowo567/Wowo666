using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using WowoFramework.Singleton;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    public ContinueButton continueButton;

    public Action OnContinue;
    
    public void ShowContinue()
    {
        continueButton.gameObject.SetActive(true);
    }

    public void Continue()
    {
        PaperManager.Instance.ChangePaper();
        OnContinue?.Invoke();
    }
}
