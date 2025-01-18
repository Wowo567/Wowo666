using System;
using System.Collections.Generic;
using Comic;
using UnityEngine;

namespace Datas
{
    [Serializable]
    public class BubbleData
    {
        public int nextId;
        public int faceId;

        public BubbleData(int nextId, int faceId)
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
        public string spriteName;
        public Sprite sprite;
        public Vector3 position;
        public Dictionary<BubbleType, BubbleData> nextComics;
    }

    [ExcelAsset]
    public class ComicItemDatas : ScriptableObject
    {
        private const string imagePath = "Images/ComicItems/";
        public List<ComicData> datas;

        private Dictionary<int, ComicData> datasDic;

        public Dictionary<int, ComicData> DatasDic => datasDic;

        // 将字符串转换为 Vector3
        private Vector3 GetPosition(string str)
        {
            string[] parts = str.Split(';');
            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);
            float z = float.Parse(parts[2]);
            return new Vector3(x, y, z);
        }

        // 初始化数据
        public void Init()
        {
            datasDic = new Dictionary<int, ComicData>();

            foreach (var item in datas)
            {
                item.sprite = Resources.Load<Sprite>(imagePath + item.spriteName);
                item.position = GetPosition(item.positionStr);

                // 创建 Bubble 对象
                BubbleData bubble1 = CreateBubble(item.bubble1);
                BubbleData bubble2 = CreateBubble(item.bubble2);
                BubbleData bubble3 = CreateBubble(item.bubble3);

                // 初始化 nextComics 字典
                item.nextComics = new Dictionary<BubbleType, BubbleData>
                {
                    { (BubbleType)1, bubble1 },
                    { (BubbleType)2, bubble2 },
                    { (BubbleType)3, bubble3 }
                };

                // 将 item 添加到 datasDic 中
                datasDic.Add(item.id, item);
                Debug.Log($"？？？ {item.id}");
            }
        }

        // 辅助方法：从字符串创建 Bubble 对象
        private BubbleData CreateBubble(string bubbleStr)
        {
            string[] parts = bubbleStr.Split(';');
            int nextId = int.Parse(parts[0]);
            int faceId = int.Parse(parts[1]);
            return new BubbleData(nextId, faceId);
        }
    }
}