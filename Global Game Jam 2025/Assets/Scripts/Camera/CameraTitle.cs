using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using WowoFramework.Singleton;

public class TitleCamera : MonoBehaviourSingleton<TitleCamera>
{
   private Camera _camera;
   public Transform firstPosition,secondPosition;
   private float _moveTime = 0.5f;
   private void Awake()
   {
      //之后统一做摄像机系统
      return;
      _camera = GetComponent<Camera>();
      Sequence sequence = DOTween.Sequence();
      sequence.AppendInterval(2);
      
      sequence.AppendCallback(() => FirstChange())
         .AppendInterval(0.5f);
      
      sequence.AppendInterval(_moveTime);
      sequence.AppendCallback(() => SecondChange())
         .AppendInterval(1);
     
      sequence.AppendCallback(() =>  ShowCamera());
   }

   public void FirstChange()
   {
      _camera.transform.DOMove(firstPosition.localPosition, _moveTime);
      DOTween.To(() => _camera.orthographicSize, x => _camera.orthographicSize = x, 8, _moveTime);
   }
   
   public void SecondChange()
   {
      _camera.transform.DOMove(secondPosition.localPosition, _moveTime);
      DOTween.To(() => _camera.orthographicSize, x => _camera.orthographicSize = x, 12, _moveTime);
   }

   private void ShowCamera()
   {
      GameManager.Instance.ShowTitle();
      CameraManager.Instance.enabled = true;
   }
}
