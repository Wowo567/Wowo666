using System;
using System.Collections.Generic;
using Comic;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;
using WowoFramework.Singleton;

public class ComicManager : MonoBehaviourSingleton<ComicManager>
{
    public GameObject firstComic;
    private ComicItem _curComic;

    public Action<int> OnComicCreated;
    public Action<int> OnAchievementGot;
    public Action<int> OnBubbleUnlock;

    public ComicItem curComic
    {
        get { return _curComic; }
        private set
        {
            if (_curComic != value)
            {
                _curComic = value;
            }
        }
    }


    public ComicItem lastComic { get; private set; }
    
    private Dictionary<int, Datas.ComicData> _datasDic;
    private ComicItem[] _comicItems;
    private int _index = 0;
    
    [Title("Position Recorder")]
    [InfoBox("Click the button to record the object's position.")]


    // 记录当前物体的位置
    [Button("Record Position")]
    private void RecordPosition()
    {
        foreach (var item in PaperManager.Instance.CurComicsTrans.GetComponentsInChildren<ComicItem>(true))
        {
            item.SetPosition();
            //item.recordedPosition = item.transform.position;
        }
        Debug.Log("Position recorded");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _datasDic = DatasManager.Instance.comicItemDatas.DatasDic;
        _comicItems = new ComicItem[10];
        curComic = Instantiate(firstComic, PaperManager.Instance.CurComicsTrans).GetComponent<ComicItem>();
        _comicItems[0] = curComic;
        _index += 1;
        Debug.Log($"CreateComic {!curComic} {curComic.name} ");
        OnAchievementGot?.Invoke(curComic.GetAchievementID());
        OnBubbleUnlock?.Invoke(curComic.GetUnlockBubbleID());
    }

    public void CreateComic(int id)
    {
        Debug.Log("CreateComic"+id);
        GameObject nextPrefab = _datasDic[id].prefab;
        lastComic = curComic;
        curComic = Instantiate(nextPrefab, PaperManager.Instance.CurComicsTrans).GetComponent<ComicItem>();
        _comicItems[_index] = curComic;
        _index += 1;
        OnComicCreated?.Invoke(id);
        OnAchievementGot?.Invoke(curComic.GetAchievementID());
        OnBubbleUnlock?.Invoke(curComic.GetUnlockBubbleID());
    }

    public void RemoveComic(ComicItem comicItem)
    {
        int comicIndex = -1;
        for (int i = 0; i < _comicItems.Length; i++)
        {
            if (_comicItems[i] == comicItem)
            {
                comicIndex = i;
                _index = i + 1;
                break;
            }
        }

        if (comicIndex >= 0)
        {
            curComic = _comicItems[comicIndex];
            for (int i = comicIndex+1; i < _comicItems.Length; i++)
            {
                if (_comicItems[i] != null)
                {
                    _comicItems[i].Remove();
                    _comicItems[i] = null;
                }
            }
        }
    }

    public void NextPage()
    {
        _index = 0;
        for (int i = 0; i < _comicItems.Length; i++)
        {
            _comicItems[i] = null;
        }
    }
}
