using Babu;
using DG.Tweening;
using PathologicalGames;
using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using Task = System.Threading.Tasks.Task;

namespace BigBang
{
    public class AudioManager : BabuSingleton<AudioManager>
    {
        private AudioSource musicAudio;

        private SpawnPool soundPool;

        private HashSet<string> _loadingSet = new HashSet<string>();
        private Dictionary<string, AudioClip> m_soundDic = new Dictionary<string, AudioClip>();

        private Tweener bgmExitTweener;
        private Tweener bgmEnterTweener;

        private float toMinValue = 0f;
        private float enterValue = 0.5f;

        private AudioClip bgmClip;

        private bool enableMusic;
        private bool enableSound;

        public bool IsMusicEnable
        {
            get
            {
                return enableMusic;
            }
        }

        public bool IsSoundEnable
        {
            get
            {
                return enableSound;
            }
        }

        public float MusicVolume
        {
            get => musicAudio.volume;
            set
            {
                musicAudio.volume = value;
                PlayerPrefs.SetFloat(PlayerPrefsKeys.BGM, value);
            }
        }

        public override void Awake()
        {
            base.Awake();
            musicAudio = GetComponent<AudioSource>();
            soundPool = GetComponent<SpawnPool>();
            MusicVolume = PlayerPrefs.GetFloat(PlayerPrefsKeys.BGM, 0.5f);
            enableMusic = PlayerPrefs.GetInt(PlayerPrefsKeys.EnableMusic, 1) > 0;
            enableSound = PlayerPrefs.GetInt(PlayerPrefsKeys.EnableSound, 1) > 0;
            EventManager.Instance.Register(EventManager.CanNotHotFixId.AVOID_GAME, AvoidGame);
        }
        private void AvoidGame(object[] objects)
        {
            StopMusicImmediately();
        }

        // 开启音效
        public void EnableSound()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.EnableSound, 1);
            enableSound = true;
        }

        // 关闭音效
        public void DisableSound()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.EnableSound, 0);
            enableSound = false;
        }

        // 开启背景音乐
        public void EnableMusic()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.EnableMusic, 1);
            enableMusic = true;
            PlayMusic(AudioNames.BGM_HOME);
        }

        // 关闭背景音乐
        public void DisableMusic()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.EnableMusic, 0);
            enableMusic = false;
            StopMusicImmediately();
        }


        public void PlayMusic(string _name)
        {
            if (!enableMusic) return;
            if (string.IsNullOrEmpty(_name)) return;
            if (m_soundDic.ContainsKey(_name))
            {
                PlayMusicInternal(_name);
            }
            else
            {
#if !UNITY_WEBGL
                LoadAudio(_name, () => PlayMusicInternal(_name));
#else
                LoadAudioAsync(_name, () => PlayMusicInternal(_name));
#endif
            }
        }

        void PlayMusicInternal(string _name)
        {
            // 循环播放
            musicAudio.loop = true;
            // 第一次播放BGM,直接淡入播放
            if (bgmClip == null)
            {
                bgmClip = m_soundDic[_name];
                OnBGMEnter();
                return;
            }
            // 相同的BGM不从头开始播放
            if (bgmClip == m_soundDic[_name]) return;
            // 设置播放的背景音乐
            bgmClip = m_soundDic[_name];
            // 退出过程不可打断,进入过程可打断
            if (bgmExitTweener == null)
            {
                // 打断进入音乐
                bgmEnterTweener?.Kill();
                bgmEnterTweener = null;
                // 降低的目标值
                float targetValue = PlayerPrefs.GetFloat(PlayerPrefsKeys.BGM, 0.5f) * toMinValue;
                // 音量降低
                bgmExitTweener = DOTween.To(value => musicAudio.volume = value, musicAudio.volume,
                   targetValue, 3f).SetEase(Ease.Linear).OnComplete(OnBGMEnter);
            }
        }

        private void OnBGMEnter()
        {
            bgmExitTweener?.Kill();
            bgmExitTweener = null;
            // 设置背景音乐
            musicAudio.clip = bgmClip;
            // 播放音乐
            musicAudio.Play();
            // 设置进入时的起始音量
            musicAudio.volume = PlayerPrefs.GetFloat(PlayerPrefsKeys.BGM, 0.5f) * enterValue;
            // 音量提高
            bgmEnterTweener = DOTween.To(value => musicAudio.volume = value, musicAudio.volume, PlayerPrefs.GetFloat(PlayerPrefsKeys.BGM, 1), 3).SetEase(Ease.Linear);
        }

        // 淡出关闭背景音乐
        public void StopMusic()
        {
            bgmExitTweener?.Kill();
            bgmExitTweener = null;
            bgmExitTweener = DOTween.To(value => musicAudio.volume = value, musicAudio.volume, 0, 3).SetEase(Ease.Linear).OnComplete(() =>
            {
                musicAudio.Stop();
                bgmClip = null;
            });
        }

        public void StopMusicImmediately()
        {
            bgmExitTweener?.Kill();
            bgmExitTweener = null;
            musicAudio.volume = 0;
            musicAudio.Stop();
            bgmClip = null;
        }

        public bool IsPlayingMusic()
        {
            return musicAudio.isPlaying;
        }

        public async Task LoadAllAsync()
        {
            var type = typeof(AudioNames);
            foreach (var item in type.GetFields())
            {
                //Debug.Log("加载音频：" + item.GetValue(null));
                string key = item.GetValue(null) as string;
                var h = YooAssets.LoadAssetAsync<AudioClip>(ResourcePath.AudioPath + key);
                await h.Task;
                m_soundDic.Add(key, h.AssetObject as AudioClip);
            }
        }

        public void LoadAudio(string name, Action callback)
        {
            //Debug.Log("加载音频：" + name);
            var clip = YooAssets.LoadAssetSync<AudioClip>(ResourcePath.AudioPath + name).AssetObject as AudioClip;
            m_soundDic.Add(name, clip);
            callback();
        }

        void LoadAudioAsync(string name, Action callback)
        {
            if (_loadingSet.Contains(name))
            {
                return;
            }
            _loadingSet.Add(name);
            var h = YooAssets.LoadAssetAsync<AudioClip>(ResourcePath.AudioPath + name);
            h.Completed += _ =>
            {
                _loadingSet.Remove(name);
                m_soundDic.Add(name, h.AssetObject as AudioClip);
                callback();
            };
        }

        // 播放音效
        public void PlaySound(string name, float volume = 1)
        {
            Debug.Log("PlaySound name = " + name);
            if (!enableSound) return;
            if (string.IsNullOrEmpty(name)) return;
            if (m_soundDic.ContainsKey(name))
            {
                PlaySoundInternal(name, volume);
            }
            else
            {
#if !UNITY_WEBGL
                LoadAudio(name, () => PlaySoundInternal(name, volume));
#else
                LoadAudioAsync(name, () => PlaySoundInternal(name, volume));
#endif
            }
        }

        void PlaySoundInternal(string name, float volume = 1)
        {
            Transform soundSource = soundPool.Spawn("Sound");
            AudioSource sound = soundSource.GetComponent<AudioSource>();
            // 设置音量
            sound.loop = false;
            sound.volume = volume;
            sound.clip = m_soundDic[name];
            sound.Play();
            soundPool.Despawn(soundSource, sound.clip.length * 1.05f);
        }

        // 播放循环音效
        //public AudioSource PlayLoopSound(string name, float volume = 1)
        //{
        //    if (!enableSound) return null;
        //    if (string.IsNullOrEmpty(name)) return null;
        //    if (!m_soundDic.ContainsKey(name)) LoadAudio(name);
        //    Transform soundSource = soundPool.Spawn("Sound");
        //    AudioSource sound = soundSource.GetComponent<AudioSource>();
        //    sound.loop = true;
        //    sound.volume = volume;
        //    sound.clip = m_soundDic[name];
        //    sound.Play();
        //    return sound;
        //}

        public void StopSound(AudioSource source)
        {
            if (source == null) return;
            if (source.gameObject.activeInHierarchy)
            {
                soundPool.Despawn(source.transform);
            }
        }
    }
}