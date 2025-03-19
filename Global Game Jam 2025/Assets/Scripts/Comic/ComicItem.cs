using System;
using System.Collections;
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
        private Dictionary<BubbleType, SpriteRenderer> _bubbleSprites;
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
        public BrushTextureSprite GreyBrush{ get; private set; }
        public BrushTextureSprite ColorBrush{ get; private set; }

        //为了最后的擦除合并图片
        private SpriteMerger _spriteMerger;

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
            _bubbleSprites = new Dictionary<BubbleType, SpriteRenderer>();

            //添加Grey/Color以及控制显示消除的脚本
            AddMask();
            GreyBrush =_greyTrans.GetComponentInChildren<BrushTextureSprite>();
            ColorBrush =_colorTrans.GetComponentInChildren<BrushTextureSprite>();
            
            AddComponents();
            _animator = GetComponent<Animator>();
            _spriteMerger = GetComponentInChildren<SpriteMerger>();
            Init();
        }

        private void AddMask()
        {
            if (_greyTrans.Find("Mask") == null) Instantiate(Resources.Load<GameObject>(MaskPrefabPath), _greyTrans);
            if (_colorTrans.Find("Mask") == null) Instantiate(Resources.Load<GameObject>(MaskPrefabPath), _colorTrans);
        }
        
        private void AddComponents()
        {      
            foreach (var item in transform.Find("Content").GetComponentsInChildren<SpriteRenderer>())
            {
                //不是蒙版则受影响
                if (!item.name.Contains("Mask"))
                {
                    item.AddComponent<WorldSpaceMaskController>();
                }
                
                if (item.name.Contains("Bubble"))
                {
                    SpriteRenderer spriteRenderer = item.GetComponent<SpriteRenderer>();
                    spriteRenderer.enabled = false;
                    _bubbleSprites.Add((BubbleType)char.GetNumericValue(item.gameObject.name.Last()), item);
                }
                
                //Mask的位置是Grey的背景的位置
                if(item.gameObject.name.ToUpper().Contains("BG")&&item.transform.parent.name.ToUpper().Contains("GREY"))
                {
                    Debug.Log("Mask的位置是Grey的背景的位置");
                    GreyBrush.transform.position = item.transform.position;
                    ColorBrush.transform.position = item.transform.position;
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
            CheckAnim();
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
            if (_spriteMerger != null)
            {
                Debug.Log("开始合并图片");
                DOTween.Sequence()
                    .AppendCallback(() => StartCoroutine(NextFrameAction(_spriteMerger.MergeAndReplace)));
            }
        }
        
        IEnumerator NextFrameAction(System.Action action)
        {
            yield return null; // 等待到下一帧
            action?.Invoke();
        }
        
        private void ClickTransition()
        {
            CheckAnim();
            GameManager.Instance.OnContinue += OnContinue;
            Sequence sequence = DOTween.Sequence();
            
            // 显示动画
            sequence.AppendCallback(ShowAnim)
                .AppendInterval(_animTime);
            
            //出现Continue
            sequence.AppendCallback(() => ShowContinue());

            sequence.AppendCallback(() =>CameraManager.Instance.Recover());
            sequence.AppendCallback(AnimFinish);
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
                    Debug.Log("clip.length"+clip.length);
                    // 返回动画的时长
                    return clip.length;
                }
            }

            // 如果找不到对应的状态，返回 0 或适当的默认值
            Debug.LogWarning("State with name " + stateName + " not found.");
            return 0f;
        }
        
        // 获取指定动画状态名的时长
        public float GetStateAnimationDuration(string stateName)
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
    
            if (stateInfo.IsName(stateName))
            {
                Debug.Log("stateInfo.length"+stateInfo.length);
                return stateInfo.length;
            }
    
            Debug.LogWarning("State with name " + stateName + " not found in Animator.");
            return 0f;
        }
        
        private void ShowContinue()
        {
            PaperManager.Instance.ShowContinue();
        }

        private void ShowAnim()
        {
           _animator.SetInteger("Type", (int)(_bubbleType+1));
        }

        private void Bubble()
        {
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
                Debug.Log("_animTime 0"+_animTime);
                return;
            }

           
            _animTime = GetAnimationDuration("Type" + (int)(_bubbleType + 1));
            Debug.Log("_animTime"+_animTime);
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

        public void Reset()
        {
            GreyBrush.ShowAll();
            animPlayFinish = false;
        }

        private void OnBubble(BubbleType type)
        {
            //如果有气泡了隐藏之前的气泡并把其他格子都删除
            if (_haveBubble)
            {
                _bubbleSprites[_bubbleType].enabled = false;
                ComicManager.Instance.RemoveComic(this);
                _bubbleType = type;
                return;
            }
            BubbleShow();
        }

        public void BubbleShow()
        {
            if (_haveBubble)
            { 
                Debug.Log("BubbleShow");
                ColorBrush.enabled = false;
                _colorTrans.gameObject.SetActive(true);
                ChatBubbleManager.Instance.HideUI();
                AudioManager.Instance.PlaySoundEffect("OnBubble");
                _haveBubble = true;
                _bubbleSprites[_bubbleType].enabled = true;
                // 显示彩色内容
                ColorBrush.enabled = true;
                ColorBrush.OnStart();
                return;
            }
           

            ChatBubbleManager.Instance.HideUI();
            AudioManager.Instance.PlaySoundEffect("OnBubble");
            _haveBubble = true;
            _bubbleSprites[_bubbleType].enabled = true;
            // 显示彩色内容
            ColorBrush.enabled = true;
            ColorBrush.OnStart();
        }

        private void OnBubbleColorBrushFinish()
        {
            CheckAnim();
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
            //ColorBrush.isBrush = false;
            if (ComicManager.Instance.curComic != this)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                GetComponentInChildren<SpriteMerger>(true).NewSpriteRenderer.enabled = false;
            }
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
                            ShowFinish();
                        }
                        else
                        {
                            ColorBrush.enabled = true;
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
                        GreyBrush.Clear();
                        CheckType();
                    }
                    break;
            }
        }

        public void OnSkip()
        {
            if (animPlayFinish) return;
            GreyBrush.Clear();
            ColorBrush.enabled = true;
            ColorBrush.ShowAll();
            _animator.SetInteger("Type", (int)(_bubbleType+1));
            _animator.Play("Type" + (int)(_bubbleType + 1), 0, 1.0f);
            if (type == ComicType.ClickTransition)
            {
                ShowContinue();
                CameraManager.Instance.Recover();
            }

            AnimFinish();
            //ShowFinish();
        }
    }
}