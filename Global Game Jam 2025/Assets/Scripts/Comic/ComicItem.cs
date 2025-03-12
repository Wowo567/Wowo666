using System;
using System.Collections.Generic;
using System.Linq;
using Datas;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using TextDubbing;
using UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace Comic
{
    public enum BubbleType
    {
        Tell = 0,
        Run = 1,
        Shock = 2,
        Love = 3,
    }

    public enum ComicType
    {
        Bubble = 1,
        Transition = 2,
        ClickTransition = 3,
        ClickTransitionBubble = 4
    }

    public class ComicItem : MonoBehaviour
    {
        private const string MaskPrefabPath = "Prefabs/ComicItemParts/Mask";
        
        public int id;
        public ComicType type;
        public ComicData ComicData{ get; private set; }
        private ComicItem _nextComic;
        
        private ChatBubblePoint _point;
        //private Grey[]  _grey;
        //private Color[]  _color;
        private List<SpriteRenderer> _bubbleParts;
        private Transform _greyTrans, _colorTrans;

        private float _fadeTime = 0.5f;

        // 用来存储物体的 transform.position
        [ShowInInspector]  // 让这个变量出现在Inspector中
        public Vector3 recordedPosition;
        
        //动画系统
        private Animator _animator;
        private float _animTime;


        private BubbleType _bubbleType = BubbleType.Tell;
        private bool _haveBubble = false;

        //判断动画是否完成
        public bool animPlayFinish { get; private set; }
        //显示/擦除
        private BrushTextureSprite _greyBrush,_colorBrush;

        public void SetPosition()
        {
            recordedPosition = transform.localPosition;
            PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
            PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
        }

        private void Awake()
        {
             transform.localPosition = recordedPosition;
            _greyTrans = transform.Find("Content/Grey");
            _colorTrans = transform.Find("Content/Color");
            _bubbleParts = new List<SpriteRenderer>();

            //添加Grey/Color以及控制显示消除的脚本
            AddMask();
            AddComponents();

            _animator = GetComponent<Animator>();
            _greyBrush =_greyTrans.GetComponentInChildren<BrushTextureSprite>();
            _colorBrush =_colorTrans.GetComponentInChildren<BrushTextureSprite>();

            Init();
        }

        private void AddMask()
        {
            if (_greyTrans.Find("Mask") == null) Instantiate(Resources.Load<GameObject>(MaskPrefabPath), _greyTrans);
            if (_colorTrans.Find("Mask") == null) Instantiate(Resources.Load<GameObject>(MaskPrefabPath), _colorTrans);
        }
        
        private void AddComponents()
        {
            AddComponentToAllChildren<Grey>(_greyTrans);
            AddComponentToAllChildren<Color>(_colorTrans);

            foreach (var item in transform.Find("Content").GetComponentsInChildren<SpriteRenderer>())
            {
                //不是蒙版和气泡则受影响
                if (!item.name.Contains("Bubble")&&!item.name.Contains("Mask"))
                {
                    item.AddComponent<WorldSpaceMaskController>();
                }
                else if (item.name.Contains("Bubble") && type == ComicType.Bubble)
                {
                    SpriteRenderer spriteRenderer = item.GetComponent<SpriteRenderer>();
                    spriteRenderer.color = new UnityEngine.Color(1, 1, 1, 0);
                    _bubbleParts.Add(spriteRenderer);
                }

            }
        }
        public void AddComponentToAllChildren<T>(Transform parent) where T : Component
        {
            if (parent == null) return;

            // 递归遍历所有子节点添加组件（如果没有）
            foreach (Transform child in parent)
            {
                if (child.GetComponent<T>() == null)
                {
                    child.gameObject.AddComponent<T>();
                }
            }
        }
        

        private void CheckType()
        {
            switch (type)
            {
                case ComicType.Transition:
                    Transition();
                    break;
                case ComicType.Bubble:
                    OnBubbleColorBrushFinish();
                    break;
                case ComicType.ClickTransition:
                    ClickTransition();
                    break;
                case ComicType.ClickTransitionBubble:
                    ClickTransitionBubble();
                    break;
            }
        }

        private void Transition()
        {
         
            Sequence sequence = DOTween.Sequence();
            
            // 显示动画
            sequence.AppendCallback(ShowAnim)
                .AppendInterval(_animTime);
            
            sequence.AppendCallback(AnimFinish);
            //配音播放完成再出现
        }

        private void AnimFinish()
        {   
            animPlayFinish = true;
        }
        
        private void ClickTransition()
        {
            GameManager.Instance.OnContinue += OnContinue;
            Sequence sequence = DOTween.Sequence();
            
            // 显示动画
            sequence.AppendCallback(ShowAnim)
                .AppendInterval(_animTime);
            
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
            PaperManager.Instance.ShowContinue();
        }

        private void ShowAnim()
        {
           _animator.SetInteger("Type", (int)(_bubbleType+1));
           CheckAnim();
        }

        private void ShowBubble(float fadeTime)
        {
            foreach (var item in _bubbleParts)
            {
                item.DOFade(1, fadeTime);
            }   
        }
        
        private void Bubble()
        {
            Sequence sequence = DOTween.Sequence();
            // 显示气泡相关图片并等待完成
            sequence.AppendCallback(() => ShowBubble(_fadeTime)).AppendInterval(_fadeTime);
            sequence.AppendCallback(ShowFinish);
        
            _point = GetComponentInChildren<ChatBubblePoint>();
            _point.Init(ComicData.nextComics.Keys.ToList());
            _point.OnBubble+= OnBubble;
            _point.OnBubbleRemove+= OnBubbleRemove;
        }
        
        private void ClickTransitionBubble()
        {
            GameManager.Instance.OnContinue += OnContinue;
            Sequence sequence = DOTween.Sequence();
            
            //出现Continue
            sequence.AppendCallback(() => ShowContinue());

            CameraManager.Instance.Recover();
        
            _point = GetComponentInChildren<ChatBubblePoint>();
            if (_point)
            {
                _point.Init(ComicData.nextComics.Keys.ToList());
                _point.OnBubble += OnBubble;
                _point.OnBubbleRemove += OnBubbleRemove;
            }
        }

        private void CheckAnim()
        {
            if (_animator == null)
            {
                _animTime = 0;
                return;
            }

            Debug.Log("_bubbleType"+_bubbleType);
            _animTime = GetAnimationDuration("Type" + (int)(_bubbleType + 1));
        }

        private void Init()
        {
            //初始位置
            ComicData = DatasManager.Instance.comicItemDatas.DatasDic[id];
            CameraManager.Instance.ChangeCamera();

            animPlayFinish = false;
            
            //隐藏UI
            ChatBubbleManager.Instance.HideUI();
        }
        
        private void OnBubble(BubbleType type)
        {
            ChatBubbleManager.Instance.HideUI();
            AudioManager.Instance.PlaySoundEffect("OnBubble");
        
            _haveBubble = true;
            _bubbleType = type;
            // 显示彩色内容
            _colorBrush.enabled = true;
        }

        private void OnBubbleColorBrushFinish()
        {
            Sequence sequence = DOTween.Sequence();
            // 显示动画
            sequence.AppendCallback(ShowAnim)
                .AppendInterval(_animTime);
            
            sequence.AppendCallback(AnimFinish);
        }

        private void ShowNext()
        {
            int next = ComicData.nextComics[_bubbleType];
            if (next == 0)
            {
                //没有故事线
            }
            else
            {
                ComicManager.Instance.CreateComic(next);
            }
        }

        public void ShowFinish()
        {
            if (type == ComicType.Transition || (type == ComicType.Bubble && _haveBubble))
            {
                Debug.Log("ShowNext1");
                ShowNext();
            }
            else
            {
                ChatBubbleManager.Instance.ShowUI();
                animPlayFinish = false;
            }
        }
        
        private void OnContinue()
        {
            AudioManager.Instance.PlaySoundEffect("Remove");
            int next = ComicData.nextComics[BubbleType.Tell];
            ComicManager.Instance.NextPage();
            ComicManager.Instance.CreateComic(next);
            CameraManager.Instance.ChangeView();
        }

        private void OnBubbleRemove()
        {
            AudioManager.Instance.PlaySoundEffect("Remove");
            Sequence sequence = DOTween.Sequence();
            
            //移除之后的漫画
            sequence.AppendCallback(() => ComicManager.Instance.RemoveComic(this));
        }

        public void Remove()
        {
            //_greyBrush.isBrush = false;
            _colorBrush.isBrush = false;
        }
        
        private void OnDestroy()
        {
            RemoveAction();
        }

        private void RemoveAction()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnContinue -= OnContinue;
            if (type == ComicType.Bubble && _point != null)
            {
                _point.OnBubble -= OnBubble;
                _point.OnBubbleRemove -= OnBubbleRemove;
            }
        }

        public int GetAchievementID()
        {
            return ComicData.achievement;
        }

        public int GetUnlockBubbleID()
        {
            return ComicData.unlockBubble;
        }
        
        [Button("记录位置")]
        public void RecordPosition()
        {
            recordedPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        }

        public void BrushFinish(BrushType brushType, bool isBrush)
        {
            switch (brushType)
            {
                case BrushType.Grey:
                    if (isBrush)
                    {
                        if (type == ComicType.Bubble)
                        {
                            Bubble();
                        }
                        else
                        {
                            _colorBrush.enabled = true;
                            AudioManager.Instance.PlaySoundEffect("ShowColor");
                        }
                    }
                    break;
                case BrushType.Color:
                    if (isBrush)
                    {
                        //播放配音
                        string dubbingName = type == ComicType.Bubble
                            ? ComicData.bubbleTextDubbing[_bubbleType]
                            : ComicData.textDubbing;
                        TextDubbingManager.Instance.Play(dubbingName);
                        _greyBrush.Clear();
                        CheckType();
                    }
                    else
                    {
                        //DestroyImmediate(gameObject);
                    }
                    break;
            }
        }
    }
}