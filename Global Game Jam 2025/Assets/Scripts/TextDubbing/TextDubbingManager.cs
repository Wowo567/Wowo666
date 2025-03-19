using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using WowoFramework.Singleton;

public enum TextDubbingType
{
    NoText,
    HaveText
}

namespace TextDubbing
{
    public class TextDubbingManager : MonoBehaviourSingleton<TextDubbingManager>
    {
        private const string Path = "TextDubbing/";
        private AudioSource _audioSource;
        public string curClipName { get; private set; }
        public bool curClipFinish { get; private set; }

        public TextDubbingType type;

        private void Awake()
        {
            _audioSource = GameObject.Find("TextDubbing").GetComponent<AudioSource>();
        }
        

        public void Play(string name)
        {
            curClipFinish = false;
            AudioClip clip = Resources.Load<AudioClip>(Path + type + "/" + name);
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

        public void OnSkip()
        {
            _audioSource.Stop();
            curClipFinish = true;
            Debug.Log("配音跳过");
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
