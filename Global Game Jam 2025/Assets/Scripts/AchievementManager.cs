using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    private static readonly string AchievementStateKey = "AchievementState";
    
    private void Awake()
    {
        ComicManager.Instance.OnAchievementGot += OnAchievementGot;
        
        int achievementState = PlayerPrefs.GetInt(AchievementStateKey, 0);
        
        Debug.Log("当前成就解锁状态:" + Convert.ToString(achievementState,2));
        
        for (int i = 0; i < 12; i++)
        {
            transform.GetChild(i).gameObject.SetActive((achievementState & (1 << i)) !=0);
        }
    }
    
    [Button]
    private void OnAchievementGot(int id)
    {
        if (id is >= 0 and < 12)
        {
            int achievementState = PlayerPrefs.GetInt(AchievementStateKey, 0);
            if ((achievementState & (1 << id)) == 0)
            {
                achievementState = achievementState | 1 << id;
                PlayerPrefs.SetInt(AchievementStateKey, achievementState);
                GetAchievement(id);
            }
        }
    }

    private void GetAchievement(int id)
    {
        Debug.Log("解锁新成就: " + id);
        transform.GetChild(id).gameObject.SetActive(true);
    }

    [Button]
    private void ResetAchievement()
    {
        PlayerPrefs.SetInt(AchievementStateKey, 0);
        for (int i = 0; i < 12; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
