using System.Collections;
using System.IO;
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

            frameAnimation.PlayDrawingSound();
            yield return frameAnimation.AnimateFadeIn().WaitForCompletion();

            frameAnimation.PlayDrawingSound();
            yield return monologueText
                .DOTextFast(monologueText.text.Length / textTypewriterSpeed)
                .SetEase(Ease.Linear)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .SetUpdate(UpdateType.Normal, true)
                .WaitForCompletion();

            yield return new WaitForSecondsRealtime(2f);

            OnEnd();

            SignalsHub.DispatchAsync(new StartCutsceneSignal
            {
                Type = CutsceneType.Restart,
            });
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