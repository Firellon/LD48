using System;
using System.Collections.Generic;
using DG.Tweening;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace LD48.Cutscenes.SimpleCutscene
{
    [Serializable]
    public class CutsceneFrame
    {
        public Sprite Sprite;

        [TextArea]
        public string Text;
    }

    public interface ICutscene
    {
        void OnStart();
    }

    public class SimpleCutscene : MonoBehaviour, ICutscene
    {
        [SerializeField] private List<CutsceneFrame> frames;

        [SerializeField] private float textTypewriterSpeed = 50;

        [SerializeField] private Image picture;
        [SerializeField] private TMP_Text monologueText;

        private int currentFrameIndex = 0;

        public void SwitchToTheNextFrame()
        {
            currentFrameIndex++;

            if (currentFrameIndex >= frames.Count)
            {
                Debug.LogWarning("Can't switch to the next frame!");
                return;
            }

            UpdateImageAndText();
        }

        public void OnStart()
        {
            currentFrameIndex = 0;
            UpdateImageAndText();
        }

        public void OnEnd()
        {
            SignalsHub.DispatchAsync(new StopCutsceneSignal());
        }

        private void UpdateImageAndText()
        {
            var nextFrame = frames[currentFrameIndex];

            picture.sprite = nextFrame.Sprite;
            monologueText.text = nextFrame.Text;

            StartTextAnimation();
        }

        public void StartTextAnimation()
        {
            monologueText.DOKill();
            monologueText
                .DOTextFast(monologueText.text.Length / textTypewriterSpeed)
                .SetEase(Ease.Linear)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .SetUpdate(UpdateType.Normal, true)
                .OnComplete(OnTextAnimationEnd);
        }

        private void OnTextAnimationEnd()
        {
            
        }
    }
}