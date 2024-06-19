using System;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sanity
{
    public class SanityCounterController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI sanityText;
        [SerializeField] private Image sanityImage;

        private void OnEnable()
        {
            SignalsHub.AddListener<PlayerSanityUpdatedEvent>(OnPlayerSanityUpdated);
        }

        private void OnPlayerSanityUpdated(PlayerSanityUpdatedEvent evt)
        {
            sanityText.text = evt.Sanity.ToString();
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<PlayerSanityUpdatedEvent>(OnPlayerSanityUpdated);
        }
    }
}