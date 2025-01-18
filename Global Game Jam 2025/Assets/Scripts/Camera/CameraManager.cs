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
    public GameObject piecesParent;
    private Transform[] _pieces;
    private CinemachineVirtualCamera[] _virtualCameras;
    private int _cameraIndex = 0;
    private CinemachineVirtualCamera _curCamrea;

    private float _surroundingParameters = 1.5f;//显示周围的范围比例 1.5为比目前漫画面积大1.5倍

    private float _curHeight;
    private Vector2 _curCenter;

    private void Awake()
    {
        _virtualCameras = GameObject.Find("Virtual Camera").GetComponentsInChildren<CinemachineVirtualCamera>(true);
        _curCamrea = _virtualCameras[_cameraIndex];
        _cameraIndex = (_cameraIndex + 1) % 2;
        
    }

    [Button("ChangeCamera")]
    public void ChangeCamera()
    {
        GetSize();
        // 根据图形的高度调整摄像机的 OrthographicSize
        AdjustCameraOrthographicSize();
    }


    private void GetSize()
    {
        //获取漫画块
        _pieces = piecesParent.GetComponentsInChildren<Transform>();
        // 初始化最小和最大值
        float _minX, _minY, _maxX, _maxY;
        _minX = _maxX = _pieces[0].position.x;
        _minY = _maxY = _pieces[0].position.y;

        // 遍历所有的pieces，更新最小和最大值
        foreach (var item in _pieces)
        {
            foreach (var piece in item.GetComponentsInChildren<Transform>())
            {
                float pieceX = piece.position.x;
                float pieceY = piece.position.y;

                // 更新X的最小值和最大值
                if (pieceX < _minX) _minX = pieceX;
                if (pieceX > _maxX) _maxX = pieceX;

                // 更新Y的最小值和最大值
                if (pieceY < _minY) _minY = pieceY;
                if (pieceY > _maxY) _maxY = pieceY;
            }
        }
        
        // 计算矩形的宽度和高度
        float width = _maxX - _minX;
        _curHeight = _maxY - _minY;
        
        // 计算矩形的中心点
        float centerX = (_minX + _maxX) / 2;
        float centerY = (_minY + _maxY) / 2;
        _curCenter = new Vector2(centerX, centerY);
       
    }

    private void AdjustCameraOrthographicSize()
    {
        Debug.Log(_virtualCameras.Length);
        _virtualCameras[_cameraIndex].transform.position =
            new Vector3(_curCenter.x, _curCenter.y, _virtualCameras[_cameraIndex].transform.position.z);
        // 获取摄像机的纵横比
        float aspectRatio = (float)Screen.width / Screen.height;

        // 根据图形高度和纵横比计算新的 OrthographicSize
        float targetOrthographicSize = _curHeight / 2;

        // 考虑屏幕的纵横比，保证在宽度上也能适配
        if (aspectRatio > 1) // 宽屏
        {
            targetOrthographicSize /= aspectRatio;
        }

        // 设置新的 OrthographicSize
        _virtualCameras[_cameraIndex].m_Lens.OrthographicSize = _surroundingParameters * targetOrthographicSize;
        _virtualCameras[_cameraIndex].gameObject.SetActive(true);
        _cameraIndex = (_cameraIndex + 1) % 2;
        _virtualCameras[_cameraIndex].gameObject.SetActive(false);
        Debug.Log($"新的 OrthographicSize: {targetOrthographicSize}");


    }
}
