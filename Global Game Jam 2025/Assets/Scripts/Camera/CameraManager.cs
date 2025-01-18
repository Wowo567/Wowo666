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
    private ComicItem _lastComic, _curComic;
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

    [Button("ChangeCamera")]
    public void ChangeCamera(ComicItem item)
    {
        _curComic = item;
        if (_lastComic != null)
        {
            Vector3 offest = _curComic.transform.position - _lastComic.transform.position;
            transform.DOMove(transform.position + offest * moveParameter, moveTime);
        }

        _lastComic = _curComic;
    }
}
