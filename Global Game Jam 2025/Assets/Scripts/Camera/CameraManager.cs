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
    private float lookView = 10;
    public float moveParameter = 0.3f;
    public float moveTime;
    [ReadOnly]
    private float _initView;
    private void Start()
    {
        _camera = GetComponent<Camera>();
        _initView = _camera.orthographicSize;
    }

    private void Update()
    {
        //Debug.Log("ChangeView"+_camera.orthographicSize);
    }

    //[Button("ChangeCamera")]
    public void ChangeView()
    {
        if(!enabled)return;
        DOTween.To(() => _camera.orthographicSize, x => _camera.orthographicSize = x, lookView, moveTime);
    }

    public void Recover()
    {
        if(!enabled)return;
        DOTween.To(() => _camera.orthographicSize, x => _camera.orthographicSize = x, _initView, moveTime);
        transform.DOMove(new Vector3(0,0,-10), moveTime);
    }
    
    public void ChangeCamera()
    {
      
        if(!enabled)return;
        Vector3 offest;
        if (ComicManager.Instance.lastComic != null)
        {
             offest =  ComicManager.Instance.curComic.transform.position - ComicManager.Instance.lastComic.transform.position;
        }
        else
        {
             offest =  ComicManager.Instance.curComic.transform.position - Vector3.zero;
        }
        Debug.Log("ChangeCamera lastComic"+ ComicManager.Instance.lastComic.transform.position);
        Debug.Log("ChangeCamera curComic"+ComicManager.Instance.curComic.transform.position);
        transform.DOMove(transform.position + offest * moveParameter, moveTime);
    }

    [Button]
    public void EndAnim()
    {
        SpriteRenderer spriteRenderer = transform.Find("Fade").GetComponent<SpriteRenderer>();
        spriteRenderer.DOFade(1, 2f).OnComplete(() =>
        {
            transform.position = new Vector3(0, 40, -10);
            spriteRenderer.DOFade(0, 2);
        });
    }
}
