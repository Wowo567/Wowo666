using System.Collections;
using System.Collections.Generic;
using Comic;
using UnityEngine;
using WowoFramework.Singleton;

public class ComicManager : MonoBehaviourSingleton<ComicManager>
{
    public GameObject firstComic;
    private ComicItem _curComic;

    public ComicItem curComic
    {
        get { return _curComic; }
        private set
        {
            if (_curComic != value)
            {
                curComic = value;
                CameraManager.Instance.ChangeCamera();
            }
        }
    }


    public ComicItem lastComic { get; private set; }
    
    private Dictionary<int, Datas.ComicData> _datasDic;
    private ComicItem[] _comicItems;
    private int _index = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        _datasDic = DatasManager.Instance.comicItemDatas.DatasDic;
        _comicItems = new ComicItem[5];
        curComic = Instantiate(firstComic, transform).GetComponent<ComicItem>();
        _comicItems[0] = curComic;
        _index += 1;
        CameraManager.Instance.ChangeView();
    }

    public void CreateComic(int id)
    {
        GameObject nextPrefab = _datasDic[id].prefab;
        lastComic = curComic;
        curComic = Instantiate(nextPrefab, transform).GetComponent<ComicItem>();
        _comicItems[_index] = curComic;
        _index += 1;
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
    
}
