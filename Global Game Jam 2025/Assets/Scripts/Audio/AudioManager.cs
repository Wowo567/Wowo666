using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using WowoFramework.Singleton;

public enum MusicType
{
    Normal,
    Weird
}


public class AudioManager : MonoBehaviourSingleton<AudioManager>
{

    private static string SoundEffectPath = "Audio/SFX/SFX_";

   public AudioSource musicAudioSource, sfxAudioSource;
    public AudioClip normal, weird;

    private float volumeFadeTime = 1;

   
    private Tweener tweener;
    public void PlayMusic(MusicType type)
    {
        AudioClip clip = type == MusicType.Normal ? normal : weird;
        if (tweener != null)tweener.Kill();
        tweener = musicAudioSource.DOFade(0, volumeFadeTime).OnComplete(delegate
        {
            musicAudioSource.clip = clip;
            musicAudioSource.Play();
            musicAudioSource.DOFade(1, volumeFadeTime);
        });
    }
    
    public void PlaySoundEffect(string name)
    {
        AudioClip clip = Resources.Load<AudioClip>(SoundEffectPath + name);
        if (clip == null)
        {
            Debug.LogError("NO AUDIO CLIP:" + name);
        }
        else
        {
            Debug.Log("PlaySoundEffect");
            sfxAudioSource.PlayOneShot(clip);
        }
    }
}
