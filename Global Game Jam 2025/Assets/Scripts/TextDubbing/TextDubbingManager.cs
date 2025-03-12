using System;
using System.Collections;
using UnityEngine;
using WowoFramework.Singleton;

namespace TextDubbing
{
    public class TextDubbingManager : MonoBehaviourSingleton<TextDubbingManager>
    {
        private const string Path = "TextDubbing/";
        private AudioSource _audioSource;
        public string curClipName { get; private set; }
        public bool curClipFinish { get; private set; }


        private void Awake()
        {
            _audioSource = GameObject.Find("TextDubbing").GetComponent<AudioSource>();
        }
        

        public void Play(string name)
        {
            curClipFinish = false;
            AudioClip clip = Resources.Load<AudioClip>(Path + name);
            if (clip == null)
            {
                Debug.LogError("NO TextDubbing CLIP:" + name);
            }
            else
            {
                _audioSource.clip = clip;
                _audioSource.PlayOneShot(clip);
                StartCoroutine(WaitForAudioEnd());
            }
        }

        //播放完是出现下一格漫画
        IEnumerator WaitForAudioEnd()
        {
            yield return new WaitWhile(() => _audioSource.isPlaying);
            curClipName = _audioSource.clip.name;
            curClipFinish = true;
            Debug.Log("配音播放完毕");
        }
    }
}
