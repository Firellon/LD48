using System;
using System.Collections.Generic;
using System.Linq;
using AudioTools.Sound;
using Signals;
using UnityEngine;
using Utilities;
using Zenject;

namespace LD48.AudioTool
{
    public enum MusicType
    {
        DayMusic,
        NightMusic,
        FollowingMusic,
        Cutscene,
    }

    public struct PlayMusicSignal
    {
        public MusicType Type;
    }

    public struct PauseMusicSignal
    {
        
    }

    public struct StopMusicSignal
    {
        
    }

    public class MusicController : MonoBehaviour
    {
        [SerializeField] private SoundSample dayMusic;
        [SerializeField] private SoundSample nightMusic;
        [SerializeField] private SoundSample chasingMusic;
        [SerializeField] private SoundSample cutsceneMusic;

        [Inject] private ISoundManager<SoundType> soundManager;

        private Dictionary<MusicType, int> playingMusic = new();

        private int currentPlayingMusicId = -1;

        private void OnEnable()
        {
            SignalsHub.AddListener<PlayMusicSignal>(OnPlayMusicSignal);
            SignalsHub.AddListener<PauseMusicSignal>(OnPauseMusicSignal);
            SignalsHub.AddListener<StopMusicSignal>(OnStopMusicSignal);

            PlayMusic(MusicType.DayMusic);
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (!isPaused)
            {
                if (currentPlayingMusicId != -1)
                {
                    var playingMusicKVP = playingMusic.First(kvp => kvp.Value == currentPlayingMusicId);
                    var musicType = playingMusicKVP.Key;
                    playingMusic[playingMusicKVP.Key] = -1;

                    PlayMusic(musicType);
                }
            }
        }

        private void OnPlayMusicSignal(PlayMusicSignal signal)
        {
            var musicType = signal.Type;

            PlayMusic(musicType);
        }

        private void PlayMusic(MusicType musicType)
        {
            var soundId = playingMusic.SafeGet(musicType, -1);
            if (soundId != -1)
            {
                if (!soundManager.IsPlaying(soundId))
                {
                    soundManager.Resume(soundId);
                    currentPlayingMusicId = soundId;
                }
            }
            else
            {
                PauseAllPlayingMusic();

                var music = GetSampleByMusicType(musicType);
                playingMusic[musicType] = soundManager.PlayMusic(music, 1f);
                currentPlayingMusicId = playingMusic[musicType];
            }
        }

        private void PauseAllPlayingMusic()
        {
            var items = ListPool<KeyValuePair<MusicType, int>>.Instance.Spawn();

            foreach (var kvp in playingMusic)
            {
                items.Add(kvp);
            }

            foreach (var kvp in items)
            {
                var soundId = kvp.Value;
                if (soundId != -1 && soundManager.IsPlaying(soundId))
                {
                    soundManager.Pause(soundId);
                    playingMusic[kvp.Key] = -1;
                }
            }

            ListPool<KeyValuePair<MusicType, int>>.Instance.Despawn(items);

            currentPlayingMusicId = -1;
        }

        private SoundSample GetSampleByMusicType(MusicType signalType)
        {
            return signalType switch
            {
                MusicType.DayMusic => dayMusic,
                MusicType.NightMusic => nightMusic,
                MusicType.FollowingMusic => chasingMusic,
                MusicType.Cutscene => cutsceneMusic,
            };
        }

        private void OnPauseMusicSignal(PauseMusicSignal signal)
        {
            
        }

        private void OnStopMusicSignal(StopMusicSignal signal)
        {
            
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<PlayMusicSignal>(OnPlayMusicSignal);
            SignalsHub.RemoveListener<PauseMusicSignal>(OnPauseMusicSignal);
            SignalsHub.RemoveListener<StopMusicSignal>(OnStopMusicSignal);
        }
    }
}