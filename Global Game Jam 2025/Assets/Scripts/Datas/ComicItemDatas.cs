using System;
using System.Collections.Generic;
using Comic;
using UnityEngine;

namespace Datas
{

    [Serializable]
    public class ComicData
    {
        public int id;
        public int bubble1;
        public int bubble2;
        public int bubble3;
        public int bubble4;
        public string prefabName;
        public GameObject prefab;
        public Dictionary<BubbleType, int> nextComics;
    }

    [ExcelAsset]
    public class ComicItemDatas : ScriptableObject
    {
        private const string prefabPath = "Prefabs/ComicItems/";
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
                item.prefab = Resources.Load<GameObject>(prefabPath + item.prefabName);

                // 初始化 nextComics 字典
                item.nextComics = new Dictionary<BubbleType, int>
                {
                    { (BubbleType)1, item.bubble1 },
                    { (BubbleType)2, item.bubble2 },
                    { (BubbleType)3, item.bubble3 },
                    { (BubbleType)4, item.bubble4 }
                };

                // 将 item 添加到 datasDic 中
                datasDic.Add(item.id, item);
            }
        }
        
    }
}