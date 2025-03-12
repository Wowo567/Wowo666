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
        public string bubble1;
        public string bubble2;
        public string bubble3;
        public string bubble4;
        public string prefabName;
        public int achievement;
        public int unlockBubble;
        public string textDubbing;
        public GameObject prefab;
        public Dictionary<BubbleType, int> nextComics;
        public Dictionary<BubbleType, string> bubbleTextDubbing;
    }

    [ExcelAsset]
    public class ComicItemDatas : ScriptableObject
    {
        private const string prefabPath = "Prefabs/ComicItems/";
        public List<ComicData> datas;

        private Dictionary<int, ComicData> datasDic;

        public Dictionary<int, ComicData> DatasDic => datasDic;

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
                    { (BubbleType)0, int.Parse(item.bubble1.Split(';')[0]) },
                    { (BubbleType)1, int.Parse(item.bubble2.Split(';')[0]) },
                    { (BubbleType)2, int.Parse(item.bubble3.Split(';')[0]) },
                    { (BubbleType)3, int.Parse(item.bubble4.Split(';')[0]) }
                };

                item.bubbleTextDubbing = new Dictionary<BubbleType, string>
                {
                    { (BubbleType)0, GetBubbleDubbingString(item.bubble1) },
                    { (BubbleType)1, GetBubbleDubbingString(item.bubble2) },
                    { (BubbleType)2, GetBubbleDubbingString(item.bubble3) },
                    { (BubbleType)3, GetBubbleDubbingString(item.bubble4) }
                };

                // 将 item 添加到 datasDic 中
                datasDic.Add(item.id, item);
            }
        }

        private string GetBubbleDubbingString(string str)
        {
            string[] strs = str.Split(';');
            if (strs.Length > 1)
            {
                return strs[1];
            }
            else
            {
                return "";
            }
        }
            
        
    }
}