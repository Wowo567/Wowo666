using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WowoFramework.Singleton;

public class DatasManager :MonoBehaviourSingleton<DatasManager>
{
    public ComicItemDatas comicItemDatas;
    public FaceDatas faceDatas;

    
    //这些在Awake中自动获取
    private void Awake()
    {
        comicItemDatas.Init();
        faceDatas.Init();
    }
}
