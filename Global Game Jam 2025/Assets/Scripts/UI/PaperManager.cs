using System;
using DG.Tweening;
using UnityEngine;
using WowoFramework.Singleton;
using WowoFramework.TransformEx;

namespace UI
{
    public class PaperManager : MonoBehaviourSingleton<PaperManager>
    {
        public GameObject paperPrefab;

        public Transform curPaper;
        
        protected override void Awake()
        {
            base.Awake();
            
            
        }

        public void ChangePaper()
        {
            curPaper.DOMove(new Vector3(5, 5, 0), 1.0f);
            curPaper = Instantiate(paperPrefab).transform;
            curPaper.ResetAllLocal();
            curPaper.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }
    }
}
