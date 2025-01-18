using System.Collections;
using System.Collections.Generic;
using Comic;
using UnityEngine;
using WowoFramework.Singleton;

public class ComicManager : MonoBehaviourSingleton<ComicManager>
{
    public GameObject firstComic;
    public ComicItem curComic { get; private set; }
    public ComicItem lastComic { get; private set; }
    
    private Dictionary<int, Datas.ComicData> _datasDic;
    
    // Start is called before the first frame update
    void Start()
    {
        _datasDic = DatasManager.Instance.comicItemDatas.DatasDic;
        curComic = Instantiate(firstComic, transform).GetComponent<ComicItem>();
        CameraManager.Instance.ChangeView();
        CameraManager.Instance.ChangeCamera();
    }

    public void CreateComic(int id)
    {
        GameObject nextPrefab = _datasDic[id].prefab;
        lastComic = curComic;
        curComic = Instantiate(nextPrefab, transform).GetComponent<ComicItem>();
        CameraManager.Instance.ChangeCamera();
    }
}
