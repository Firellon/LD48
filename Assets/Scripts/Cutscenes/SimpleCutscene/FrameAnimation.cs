
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace LD48.Cutscenes.SimpleCutscene
{
    public class FrameAnimation : MonoBehaviour
    {
        [SerializeField] private Image image;

        public Tween AnimateFadeIn()
        {
            return image.material
                .DOFloat(1f, "_MaskOffset", 1f)
                .SetUpdate(UpdateType.Normal, true)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .SetTarget(image);
        }

        public void ResetAnimation()
        {
            image.DOKill();
            image.material.SetFloat("_MaskOffset", -1f);
        }
    }
}