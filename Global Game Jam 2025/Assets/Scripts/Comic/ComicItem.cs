using System.Collections.Generic;
using System.Linq;
using Datas;
using DG.Tweening;
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
        
        private ChatBubblePoint _point;
        private SpriteRenderer[] _grey, _color;
        private float fadeTime = 0.3f;
        
        private void Start()
        {
            Init();
        }

        private void Init()
        {
            //初始位置
            _comicData = DatasManager.Instance.comicItemDatas.DatasDic[id];
            transform.position = _comicData.position;
            _grey = transform.Find("Grey").GetComponentsInChildren<SpriteRenderer>();
            _color = transform.Find("Color").GetComponentsInChildren<SpriteRenderer>();
            
            //更改相机
            CameraManager.Instance.ChangeCamera(this);
            //显示线稿
            GreyShow(true);
        
            _point = GetComponentInChildren<ChatBubblePoint>();
            _point.Init(_comicData.nextComics.Keys.ToList());
            _point.OnBubble+= OnBubble;
            _point.OnBubbleRemove+= OnBubbleRemove;
        }

        private void GreyShow(bool isShow)
        {
            foreach (var item in _grey)
            {
                item.DOFade(isShow ? 1 : 0, fadeTime);
            }
        }
        
        private void ColorShow(bool isShow)
        {
            foreach (var item in _color )
            {
                item.DOFade(isShow ? 1 : 0, fadeTime);
            }
        }

        
        private void OnBubble(BubbleType type)
        {
            ColorShow(true);
            int next = _comicData.nextComics[type];
            
            //实例下一个漫画
            // _nextComic = bubbleData.nextId;
            
        }

        private void OnBubbleRemove()
        {
        
        }
    }
}