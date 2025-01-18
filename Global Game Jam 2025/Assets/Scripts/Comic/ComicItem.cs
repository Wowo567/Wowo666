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
        Wait = 4,
    }

    public enum ComicType
    {
        Bubble = 1,
        Transition = 2,
        ClickTransition = 3
    }

    public class ComicItem : MonoBehaviour
    {
        public int id;
        public ComicType type;
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

        private void CheckType()
        {
            switch (type)
            {
                case ComicType.Transition:
                    Transition();
                    break;
                case ComicType.ClickTransition:
                    ClickTransition();
                    break;
                case ComicType.Bubble:
                    Bubble();
                    break;
            }
        }

        private void Transition()
        {
            Sequence sequence = DOTween.Sequence();

            // 显示线稿并等待完成
            sequence.AppendCallback(() => GreyShow(true,fadeTime))
                .AppendInterval(fadeTime);  // 确保 fadeTime 时间结束

            // 然后显示彩色内容
            sequence.AppendCallback(() => ColorShow(true,fadeTime))
                .AppendInterval(fadeTime);
            
            //出现下一个漫画
            sequence.AppendCallback(() => ShowNext(BubbleType.Happy));
        }
        
        private void ClickTransition()
        {
            
        }

        private void Bubble()
        {
            //显示线稿
            GreyShow(true,fadeTime);
        
            _point = GetComponentInChildren<ChatBubblePoint>();
            _point.Init(_comicData.nextComics.Keys.ToList());
            _point.OnBubble+= OnBubble;
            _point.OnBubbleRemove+= OnBubbleRemove;
        }

        private void Init()
        {
            //初始位置
            _comicData = DatasManager.Instance.comicItemDatas.DatasDic[id];
            transform.position = _comicData.position;
            _grey = transform.Find("Content/Grey").GetComponentsInChildren<SpriteRenderer>();
            _color = transform.Find("Content/Color").GetComponentsInChildren<SpriteRenderer>();
            //消失
            GreyShow(false);
            ColorShow(false);
            CheckType();
            CheckBubble();
        }

        private void GreyShow(bool isShow,float fadeTime = 0)
        {
            foreach (var item in _grey)
            {
                item.DOFade(isShow ? 1 : 0, fadeTime);
            }
        }
        
        private void ColorShow(bool isShow,float fadeTime = 0)
        {
            foreach (var item in _color )
            {
                item.DOFade(isShow ? 1 : 0, fadeTime);
            }
        }
        
        private void OnBubble(BubbleType type)
        {
            ColorShow(true,fadeTime);
            ShowNext(type);
        }

        private void ShowNext(BubbleType type)
        {
            int next = _comicData.nextComics[type];
            if (next == 0)
            {
                //没有故事线
            }
            else
            {
                ComicManager.Instance.CreateComic(next);
            }
        }

        private void OnBubbleRemove()
        {
            Sequence sequence = DOTween.Sequence();
            // 消失
            sequence.AppendCallback(() => ColorShow(false,fadeTime))
                .AppendInterval(fadeTime);
            //移除之后的漫画
            sequence.AppendCallback(() => ComicManager.Instance.RemoveComic(this));
        }

        public void Remove()
        {
            ColorShow(false,fadeTime);
            GreyShow(false,fadeTime);
            Sequence sequence = DOTween.Sequence();
            // 消失
            sequence.AppendInterval(fadeTime);
            //移除这个漫画
            sequence.AppendCallback(() => DestroyImmediate(gameObject));
        }

        private void CheckBubble()
        {
            //GetComponent(bubble)
        }
    }
}