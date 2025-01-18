using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble
{
    private int nextId;
    private int faceId;

    public Bubble(int nextId, int faceId)
    {
        this.nextId = nextId;
        this.faceId = faceId;
    }
}

[Serializable]
public class ComicData
{
    public int id;
    public string bubble1;
    public string bubble2;
    public string bubble3;
    public string positionStr;
   // public string facePositionStr;
    public string spriteName;
    public Sprite sprite;
    public Vector3 position;
   // public Vector3 facePosition;
    public Dictionary<BubbleType, Bubble> nextComics;
}

[ExcelAsset]
public class ComicItemDatas : ScriptableObject
{
    private const string imagePath = "Images/ComicItems/";
    public List<ComicData> Datas;

    public Dictionary<int, ComicData> DatasDic => datasDic;
    public Dictionary<int, ComicData> datasDic;

    private Vector3 GetPosition(string str)
    {
        // 拆分字符串
        string[] parts = str.Split(';');

        // 转换为 float 类型
        float x = float.Parse(parts[0]);
        float y = float.Parse(parts[1]);
        float z = float.Parse(parts[2]);

        return new Vector3(x, y, z);

    }

    public void Init()
    {
        datasDic = new Dictionary<int, ComicData>();
        foreach (var item in Datas)
        {
            item.sprite = Resources.Load(imagePath + item.spriteName) as Sprite;

            // 创建 Vector3 对象
            item.position = GetPosition(item.positionStr);
            //item.facePosition = GetPosition(item.facePositionStr);

            Bubble bubble_1 = new Bubble(Int32.Parse(item.bubble1.Split(";")[0]),
                Int32.Parse(item.bubble1.Split(";")[1]));
            Bubble bubble_2 = new Bubble(Int32.Parse(item.bubble2.Split(";")[0]),
                Int32.Parse(item.bubble2.Split(";")[1]));
            Bubble bubble_3 = new Bubble(Int32.Parse(item.bubble3.Split(";")[0]),
                Int32.Parse(item.bubble3.Split(";")[1]));

            item.nextComics = new Dictionary<BubbleType, Bubble>();
            item.nextComics.Add((BubbleType)1, bubble_1);
            item.nextComics.Add((BubbleType)2, bubble_2);
            item.nextComics.Add((BubbleType)3, bubble_3);

            datasDic.Add(item.id, item);
        }
    }

}