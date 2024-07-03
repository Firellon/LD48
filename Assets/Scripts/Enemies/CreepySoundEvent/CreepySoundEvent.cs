using System.Collections;
using DG.Tweening;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace Enemies.CreepySoundEvent
{
    public class CreepySoundEvent : MonoBehaviour
    {
        [SerializeField] private float soundTime;
        [SerializeField] private float fadeTime;
        [SerializeField] private AudioSource audioSource;

        [SerializeField] private bool loopedSound = true;

        [Inject] private IPrefabPool prefabPool;

        private float defaultVolume;

        private void Awake()
        {
            defaultVolume = audioSource.volume;
        }

        private void OnEnable()
        {
            StartCoroutine(nameof(SoundProcess));
        }

        private IEnumerator SoundProcess()
        {
            audioSource.loop = loopedSound;
            audioSource.Play();

            if (loopedSound)
            {
                audioSource.volume = 0;

                yield return audioSource.DOFade(defaultVolume, fadeTime).WaitForCompletion();
                yield return new WaitForSeconds(soundTime);
                yield return audioSource.DOFade(0f, fadeTime).WaitForCompletion();
            }
            else
            {
                yield return new WaitForSeconds(audioSource.clip.length);
            }

            prefabPool.Despawn(gameObject);
        }
    }
}