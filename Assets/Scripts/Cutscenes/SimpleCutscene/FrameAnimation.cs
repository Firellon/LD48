using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace LD48.Cutscenes.SimpleCutscene
{
    public class FrameAnimation : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private AnimationCurve fadeInCurve;

        [SerializeField] private float drawEffectStroke = 0.2f;

        private string materialFieldName1 = "_Progress";
        private string materialFieldName2 = "_DrawEffectStroke";

        public Tween AnimateFadeIn()
        {
            var sequence = DOTween.Sequence();

            var duration = 1f;

            sequence
                .Append(DOVirtual.Float(0f, 1f, duration, (value) =>
                    {
                        image.material.SetFloat(materialFieldName1, value);
                    })
                    .SetEase(Ease.Linear)
                )
                .Append(DOVirtual.Float(1f, 0f, duration, (value) =>
                    {
                        image.material.SetFloat(materialFieldName2, value);
                    })
                    .SetEase(Ease.Linear)
                )
                .SetUpdate(UpdateType.Normal, true)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .SetTarget(image);

            return sequence;

            // return image.material
            //     .DOFloat(1f, materialFieldName, 2f)
            //     .SetUpdate(UpdateType.Normal, true)
            //     .SetLink(gameObject, LinkBehaviour.KillOnDisable)
            //     .SetTarget(image);
        }

        public void ResetAnimation()
        {
            image.DOKill();
            image.material.SetFloat(materialFieldName2, 1f);
            image.material.SetFloat(materialFieldName1, 0f);
        }
    }
}