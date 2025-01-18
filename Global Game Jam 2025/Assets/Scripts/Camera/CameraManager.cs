using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Comic;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using WowoFramework.Singleton;

public class CameraManager : MonoBehaviourSingleton<CameraManager>
{
    private Camera _camera;
    public float lookView;
    public float moveParameter = 0.3f;
    public float moveTime;
    [ReadOnly]
    private float _initView;
    private void Start()
    {
        _camera = GetComponent<Camera>();
        _initView = _camera.orthographicSize;
    }

    //[Button("ChangeCamera")]
    public void ChangeView()
    {
        DOTween.To(() => _camera.orthographicSize, x => _camera.orthographicSize = x, lookView, moveTime);
    }

    public void Recover()
    {
        DOTween.To(() => _camera.orthographicSize, x => _camera.orthographicSize = x, _initView, moveTime);
        transform.DOMove(new Vector3(0,0,-10), moveTime);
    }
    
    public void ChangeCamera()
    {
        Vector3 offest;
        if (ComicManager.Instance.lastComic != null)
        {
             offest =  ComicManager.Instance.curComic.transform.position - ComicManager.Instance.lastComic.transform.position;
        }
        else
        {
             offest =  ComicManager.Instance.curComic.transform.position - Vector3.zero;
        }
        
        transform.DOMove(transform.position + offest * moveParameter, moveTime);
    }
}
