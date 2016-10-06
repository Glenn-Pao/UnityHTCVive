﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : SingletonMonoBehavior<AudioManager>
{
    #region [ VARIABLES ]
    //The path of audio files
    private const string BGM_PATH = "Audio/BGM";
    private const string SE_PATH = "Audio/SE";

    // The key for save volume level
    private const string BGM_VOLUME_KEY = "BGM_VOLUME_KEY";
    private const string SE_VOLUME_KEY = "SE_VOLUME_KEY";

    // The default value of above key;
    private const float BGM_VOLUME_DEFAULT = 1.0f;
    private const float SE_VOLUME_DEFAULT = 1.0f;

    // Audio name to flow next
    private string _nextBGMName;
    private string _nextSEName;

    private bool _isFadeOut = false;

    private Dictionary<string, AudioClip> _bgmDic;
    private Dictionary<string, AudioClip> _seDic;

    private AudioSource _bgmSource;
    private List<AudioSource> _seSourceList;
    private const int SE_SOURCE_NUM = 10;

    // Time to take though BGM performs fading.
    public const float BGM_FADE_SPEED_RATE_HIGH = 0.9f;
    public const float BGM_FADE_SPEED_RATE_LOW = 0.3f;
    private float _bgmFadeSpeedRate = BGM_FADE_SPEED_RATE_HIGH;

    //public AudioSource attachBGMSource;
    //public AudioSource attachSESource;
    #endregion

    private void Awake()
    {
        #region [ INITIALIZE ]
        if (this != instance)
        {
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(this.gameObject);

        #endregion

        #region [ ADD_COMPONENTS ]
        // Making Audiolistener and AudioSource.
        gameObject.AddComponent<AudioListener>();
        for (int i = 0; i < SE_SOURCE_NUM + 1; i++)
            gameObject.AddComponent<AudioSource>();

        #endregion

        // Getting audio source and setting to each variable, then setting that volume
        AudioSource[] audioSourceArray = GetComponents<AudioSource>();
        _seSourceList = new List<AudioSource>();

        for (int i = 0; i < audioSourceArray.Length; i++)
        {
            audioSourceArray[i].playOnAwake = false;

            if (i == 0)
            {
                audioSourceArray[i].loop = true;
                _bgmSource = audioSourceArray[i];
                _bgmSource.volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, BGM_VOLUME_DEFAULT);
            }
            else
            {
                _seSourceList.Add(audioSourceArray[i]);
                audioSourceArray[i].volume = PlayerPrefs.GetFloat(SE_VOLUME_KEY, SE_VOLUME_DEFAULT);
            }
        }

        //Setting and loading all SE and BGM files from resources folder.
        _bgmDic = new Dictionary<string, AudioClip>();
        _seDic = new Dictionary<string, AudioClip>();

        object[] bgmList = Resources.LoadAll(BGM_PATH);
        object[] seList = Resources.LoadAll(SE_PATH);

        foreach (AudioClip bgm in bgmList)
            _bgmDic[bgm.name] = bgm;

        foreach (AudioClip se in seList)
            _seDic[se.name] = se;
    }

    private void Update()
    {
        if (!_isFadeOut)
            return;

        // Its volume downs gradually and its volume restores defaults if it becomes 0 then flow next bgm

        _bgmSource.volume -= Time.deltaTime * _bgmFadeSpeedRate;
        if (_bgmSource.volume <= 0)
        {
            _bgmSource.Stop();
            _bgmSource.volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, BGM_VOLUME_DEFAULT);
            _isFadeOut = false;

            if (!string.IsNullOrEmpty(_nextBGMName))
                PlayBGM(_nextSEName);
        }
    }

    #region [ FOR_SE ]

    public void PlaySE (string seName, float delay = 0f)
    {
        if(!_seDic.ContainsKey(seName))
        {
            Debug.LogWarning(seName + "is nothing.");
            return;
        }

        _nextSEName = seName;
        Invoke("DelayPlaySE", delay);
    }

    private void DelayPlaySE()
    {
        foreach(AudioSource seSources in _seSourceList)
        {
            if(!seSources.isPlaying)
            {
                seSources.PlayOneShot(_seDic[_nextSEName] as AudioClip);
                return;
            }
        }
    }

    #endregion

    #region [ FOR_BGM ]

    public void PlayBGM(string bgmName, float fadeSpeedRate = BGM_FADE_SPEED_RATE_HIGH)
    {
        if(!_bgmDic.ContainsKey(bgmName))
        {
            Debug.LogWarning(bgmName + "is nothing.");
            return;
        }

        //  It flows when bgm doesn't flow.
        if(!_bgmSource.isPlaying)
        {
            _nextBGMName = "";
            _bgmSource.clip = _bgmDic[bgmName] as AudioClip;
            _bgmSource.Play();
        }

        // if any bgm flows already, flow next bgm after it fade out.(If next bgm and currently bgm are same, it is through.)
        else if (_bgmSource.clip.name != bgmName)
        {
            _nextBGMName = bgmName;
            FadeOutBGM(fadeSpeedRate);
        }
    }

    public void StopBGM()
    {
        _bgmSource.Stop();
    }

    public void FadeOutBGM(float fadeSpeedRate = BGM_FADE_SPEED_RATE_LOW)
    {
        _bgmFadeSpeedRate = fadeSpeedRate;
        _isFadeOut = true;
    }

    #endregion
    //;agjhjah
    #region [ FOR_VOLUME ]

    public void ChangeBothVolume(float BGMVolume, float SEVolume)
    {
        _bgmSource.volume = BGMVolume;
        foreach(AudioSource seSources in _seSourceList)
        {
            seSources.volume = SEVolume;
        }

        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BGMVolume);
        PlayerPrefs.SetFloat(SE_VOLUME_KEY, SEVolume);
    }

    public void ChangeBGMVolume(float BGMVolume)
    {
        _bgmSource.volume = BGMVolume;

        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BGMVolume);
    }

    public void ChangeSEVolume(float SEVolume)
    {
        foreach (AudioSource seSources in _seSourceList)
        {
            seSources.volume = SEVolume;
        }

        PlayerPrefs.SetFloat(SE_VOLUME_KEY, SEVolume);
    }

    #endregion
}
