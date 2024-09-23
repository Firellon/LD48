using System.Collections;
using DG.Tweening;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace LD48.Cutscenes.SimpleCutscene
{
    public class OneFrameCutscene : MonoBehaviour, ICutscene
    {
        [SerializeField] private Image picture;
        [SerializeField] private TMP_Text monologueText;
        [SerializeField] private FrameAnimation frameAnimation;

        [SerializeField] private float textTypewriterSpeed = 25;

        public void OnStart()
        {
            StartCoroutine(nameof(CutsceneProcess));
            // SignalsHub.DispatchAsync(new PlayMusicSignal { Type = MusicType.Cutscene });
        }

        public IEnumerator CutsceneProcess()
        {
            ResetImageAndText();

            yield return monologueText
                .DOTextFast(monologueText.text.Length / textTypewriterSpeed)
                .SetEase(Ease.Linear)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .SetUpdate(UpdateType.Normal, true)
                .WaitForCompletion();

            OnEnd();
        }

        private void ResetImageAndText()
        {
            frameAnimation.ResetAnimation();
            monologueText.DOKill();
            monologueText.maxVisibleCharacters = 0;
        }

        public void OnEnd()
        {
            SignalsHub.DispatchAsync(new StopCutsceneSignal());
        }
    }
}