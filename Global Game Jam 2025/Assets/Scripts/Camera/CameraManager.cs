 using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using WowoFramework.Singleton;

public class CameraManager : MonoBehaviourSingleton<CameraManager>
{
    private Transform _lastComic, _curComic;
    private Camera _camera;
    public float _lookView;
    [ReadOnly]
    public float _initView;
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _initView = _camera.orthographicSize;
    }

    [Button("ChangeCamera")]
    public void ChangeCamera()
    {
  
    }


   
}
