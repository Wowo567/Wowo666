using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public enum BubbleType
{
    Happy = 1,
    Angry = 2,
    Sad = 3,
}
    
public class ComicItem : MonoBehaviour
{
    public int id;
    private ComicData _comicData;
    private Dictionary<int, ComicItem> _nextComicItems = new Dictionary<int, ComicItem>();

    private ChatBubblePoint _point;
    private void Awake()
    {
       Init();
    }

    private void Init()
    {
        //初始位置
        _comicData = DatasManager.Instance.comicItemDatas.DatasDic[id];
        transform.position = _comicData.position;
        
        _point = GetComponentInChildren<ChatBubblePoint>();
        _point.OnBubble+= OnBubble;
        _point.OnBubbleRemove+= OnBubbleRemove;
    }

    private void OnBubble(int type)
    {
        
    }
    
    private void OnBubbleRemove()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
