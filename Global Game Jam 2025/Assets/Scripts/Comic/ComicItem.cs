using System.Collections.Generic;
using System.Linq;
using Datas;
using UI;
using UnityEngine;

namespace Comic
{
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
        private ComicItem _nextComic;
        private Dictionary<BubbleType, ComicItem> _nextComicItems = new Dictionary<BubbleType, ComicItem>();

        private SpriteRenderer _face;
        private ChatBubblePoint _point;
        
        private void Start()
        {
            Init();
        }

        private void Init()
        {
            //初始位置
            _comicData = DatasManager.Instance.comicItemDatas.DatasDic[id];
            transform.position = _comicData.position;
            //_face = transform.Find("Face").GetComponent<SpriteRenderer>();
        
            _point = GetComponentInChildren<ChatBubblePoint>();
            _point.Init(_comicData.nextComics.Keys.ToList());
            _point.OnBubble+= OnBubble;
            _point.OnBubbleRemove+= OnBubbleRemove;
        }
        
        private void OnBubble(BubbleType type)
        {
            BubbleData bubbleData = _comicData.nextComics[type];
            
            //更改表情
            //Sprite faceSprite = DatasManager.Instance.faceDatas.DatasDic[bubbleData.faceId].sprite;
            // _face.sprite = faceSprite;
            //实例下一个漫画
            // _nextComic = bubbleData.nextId;
            
            //更改相机
            CameraManager.Instance.ChangeCamera();
        }

        private void OnBubbleRemove()
        {
        
        }
    }
}