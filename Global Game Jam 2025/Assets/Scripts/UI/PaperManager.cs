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
        public GameObject paperPrefab;

        public Transform curPaper;

        public Transform comicsTrans;

        [Button]
        public void ChangePaper()
        {
            Transform abandonPaper = curPaper;
            
            curPaper = Instantiate(paperPrefab).transform;
            comicsTrans = curPaper.Find("Comics");
            curPaper.ResetAllLocal();
            curPaper.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            
            ChatBubblePoint[] points = abandonPaper.GetComponentsInChildren<ChatBubblePoint>();
            foreach (ChatBubblePoint point in points)
            {
                point.Release();
            }
            abandonPaper.GetComponent<SpriteRenderer>().sortingOrder++;
            abandonPaper.DOMove(new Vector3(30, 30, 0), 1.0f).OnComplete(() =>
            {
                Destroy(abandonPaper.gameObject);
            });
        }
        
        [Button]
        public void ShowContinue()
        {
            curPaper.Find("continue").gameObject.SetActive(true);
        }
    }
}
