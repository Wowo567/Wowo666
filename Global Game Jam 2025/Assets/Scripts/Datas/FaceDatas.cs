using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Face
{
    public int id;
    public string spriteName;
    public Sprite sprite;
}

[ExcelAsset]
public class FaceDatas : ScriptableObject
{
    private const string imagePath = "Images/ComicItems/Face/";
    public List<Face> Datas;

    public Dictionary<int,Face> DatasDic => datasDic;
    public Dictionary<int,Face> datasDic;
    

    public void Init()
    {
        datasDic = new Dictionary<int,Face>();
        foreach (var item in Datas)
        {
            item.sprite = Resources.Load(imagePath + item.spriteName) as Sprite;
            datasDic.Add(item.id, item);
        }
    }
    
}