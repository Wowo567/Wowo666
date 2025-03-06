using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using WowoFramework.Singleton;
using WowoFramework.TransformEx;

namespace UI
{
    public class PaperManager : MonoBehaviourSingleton<PaperManager>
    {
        [SerializeField, LabelText("纸页预制体")] private GameObject paperPrefab;
        [SerializeField, LabelText("已完成纸堆")] private Transform finishedStackTrans;
        [SerializeField, LabelText("绘制中纸堆")] private Transform paintingPapers;
        
        private Transform _curPaper;
        private Transform _comicsTrans;

        public Transform CurPaper => _curPaper;
        public Transform CurComicsTrans => _comicsTrans;
        
        [Button("换纸")]
        public void CreateNewPaper()
        {
            if (_curPaper)
            {
                Transform finishedPaper = _curPaper;
            
                SpriteRenderer[] spriteRenderers = finishedPaper.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                {
                    spriteRenderer.sortingOrder = 1;
                }
                ChatBubblePoint[] points = finishedPaper.GetComponentsInChildren<ChatBubblePoint>();
                foreach (ChatBubblePoint point in points)
                {
                    point.Release();
                }
                finishedPaper.SetParent(finishedStackTrans,true);
                finishedPaper.DOLocalMove(new Vector3(0, 0, 0), 1.0f).OnComplete(() =>
                {
                    if (finishedStackTrans.childCount > 1)
                    {
                        Destroy(finishedStackTrans.GetChild(0).gameObject);   
                    }
                    foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                    {
                        spriteRenderer.sortingOrder = 0;
                    }
                });   
            }

            _curPaper = Instantiate(paperPrefab, paintingPapers).transform;
            _curPaper.Find("continue").gameObject.SetActive(false);
            _comicsTrans = _curPaper.Find("Comics");
            _curPaper.ResetAllLocal();
        }

        [Button("显示继续按键")]
        public void ShowContinue()
        {
            _curPaper.Find("continue").gameObject.SetActive(true);
            _curPaper.Find("continue").GetComponent<SpriteRenderer>().color = new UnityEngine.Color(1, 1, 1, 0);
            _curPaper.Find("continue").GetComponent<SpriteRenderer>().DOFade(1, 0.5f);
        }
    }
}
