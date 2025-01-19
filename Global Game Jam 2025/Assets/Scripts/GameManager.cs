using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using WowoFramework.Singleton;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    public Action OnContinue;

    [Button]
    public void Continue()
    {
        PaperManager.Instance.ChangePaper();
        OnContinue?.Invoke();
    }
}
