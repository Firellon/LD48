﻿using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LD48.AudioTool;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace LD48.Cutscenes.SimpleCutscene
{
    public class SimpleCutscene : MonoBehaviour, ICutscene
    {
        [SerializeField] private List<CutsceneFrame> frames;

        [SerializeField] private float textTypewriterSpeed = 25;

        [SerializeField] private Image picture;
        [SerializeField] private TMP_Text monologueText;
        [SerializeField] private FrameAnimation frameAnimation;

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
            StartCoroutine(nameof(CutsceneProcess));
            SignalsHub.DispatchAsync(new PlayMusicSignal { Type = MusicType.Cutscene });
        }

        private IEnumerator CutsceneProcess()
        {
            currentFrameIndex = 0;
            UpdateImageAndText();

            foreach (var frame in frames)
            {
                // 1. animate image
                // 2. animate text
                ResetImageAndText(frame);

                if (frame.Sprite != null)
                {
                    frameAnimation.PlayDrawingSound();
                    yield return frameAnimation.AnimateFadeIn().WaitForCompletion();
                }

                if (!string.IsNullOrWhiteSpace(frame.Text))
                {
                    frameAnimation.PlayDrawingSound();
                    yield return monologueText
                        .DOTextFast(monologueText.text.Length / textTypewriterSpeed)
                        .SetEase(Ease.Linear)
                        .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                        .SetUpdate(UpdateType.Normal, true)
                        .WaitForCompletion();
                }

                yield return new WaitForSecondsRealtime(2f);

                currentFrameIndex++;
                UpdateImageAndText();
            }

            OnEnd();
        }

        private void ResetImageAndText(CutsceneFrame currentFrame)
        {
            if (currentFrame.Sprite != null)
                frameAnimation.ResetAnimation();

            if (!string.IsNullOrWhiteSpace(currentFrame.Text))
            {
                monologueText.DOKill();
                monologueText.maxVisibleCharacters = 0;
            }
        }

        public void OnEnd()
        {
            SignalsHub.DispatchAsync(new StopCutsceneSignal());
        }

        private void UpdateImageAndText()
        {
            if (currentFrameIndex >= frames.Count)
                return;

            var nextFrame = frames[currentFrameIndex];

            if (nextFrame.Sprite != null)
                picture.sprite = nextFrame.Sprite;

            if (!string.IsNullOrWhiteSpace(nextFrame.Text))
                monologueText.text = nextFrame.Text;
        }
    }
}