using System;
using System.Collections.Generic;
using System.Linq;
using Datas;
using DG.Tweening;
using Sirenix.OdinInspector;
using UI;
using UnityEditor;
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
        
        private ChatBubblePoint _point;
        private Grey[]  _grey;
        private Color[]  _color;

        private float fadeTime = 0.3f;

        // 用来存储物体的 transform.position
        [ShowInInspector]  // 让这个变量出现在Inspector中
        public Vector3 recordedPosition;
        
        //动画系统
        private Animator _animator;
        private float _animTime;

        public void SetPosition()
        {
            recordedPosition = transform.position;
            PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
            PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
        }

        private void Awake()
        {
            _grey = transform.GetComponentsInChildren<Grey>();
            _color = transform.GetComponentsInChildren<Color>();
            _animator = GetComponent<Animator>();
            //消失
            GreyShow(false);
            ColorShow(false);
        }

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
            Sequence sequence = DOTween.Sequence();
            // 显示线稿并等待完成
            sequence.AppendCallback(() => GreyShow(true,fadeTime))
                .AppendInterval(fadeTime);  // 确保 fadeTime 时间结束

            // 然后显示彩色内容
            sequence.AppendCallback(() => ColorShow(true,fadeTime))
                .AppendInterval(fadeTime);
            
            //出现Continue
            sequence.AppendCallback(() => ShowContinue());

            CameraManager.Instance.Recover();
        }

        // 获取指定动画状态名的时长
        public float GetAnimationDuration(string stateName)
        {
            // 获取 Animator 中的 Controller
            RuntimeAnimatorController controller = _animator.runtimeAnimatorController;

            // 获取所有的动画状态
            foreach (var clip in controller.animationClips)
            {
                // 查找与传入的状态名称匹配的动画
                if (clip.name == stateName)
                {
                    // 返回动画的时长
                    return clip.length;
                }
            }

            // 如果找不到对应的状态，返回 0 或适当的默认值
            Debug.LogWarning("State with name " + stateName + " not found.");
            return 0f;
        }
        private void ShowContinue()
        {
            //GameManager.Instance.ShowContinue();
            //_animTime = 
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
            transform.position = recordedPosition;// _comicData.position;


            CheckType();
            CheckBubble();
        }

        private void GreyShow(bool isShow,float fadeTime = 0) 
        {
            if (fadeTime == 0)
            {
                foreach (var item in _grey)
                {
                    item.GetComponent<SpriteRenderer>().color = new UnityEngine.Color(1, 1, 1, isShow ? 1 : 0);
                }
            }
            else
            {
                foreach (var item in _grey)
                {
                    item.GetComponent<SpriteRenderer>().DOFade(isShow ? 1 : 0, fadeTime);
                }   
            }
        }
        
        private void ColorShow(bool isShow,float fadeTime = 0)
        {
            if (fadeTime == 0)
            {
                foreach (var item in _color)
                {
                    item.GetComponent<SpriteRenderer>().color = new UnityEngine.Color(1, 1, 1, isShow ? 1 : 0);
                }
            }
            else
            {
                foreach (var item in _color )
                {
                    item.GetComponent<SpriteRenderer>().DOFade(isShow ? 1 : 0, fadeTime);
                }
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