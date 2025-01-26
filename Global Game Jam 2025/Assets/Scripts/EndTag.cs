using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTag : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(nameof(WaitToOver));
        GameManager.Instance.HideUI();
    }

    IEnumerator WaitToOver()
    {
        yield return new WaitForSeconds(5);
        CameraManager.Instance.EndAnim();
        AudioManager.Instance.PlayMusic(MusicType.Weird);
    }
}
