using AudioTools.Sound;
using Signals;
using UnityEngine;
using Zenject;

namespace LD48.AudioTool
{
    public struct PlayMusicSignal
    {
        
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

        [Inject] private ISoundManager<SoundType> soundManager;

        private void OnEnable()
        {
            SignalsHub.AddListener<PlayMusicSignal>(OnPlayMusicSignal);
            SignalsHub.AddListener<PauseMusicSignal>(OnPauseMusicSignal);
            SignalsHub.AddListener<StopMusicSignal>(OnStopMusicSignal);
        }

        private void OnPlayMusicSignal(PlayMusicSignal signal)
        {
            
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