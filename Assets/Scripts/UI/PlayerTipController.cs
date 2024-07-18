using System.Collections;
using DG.Tweening;
using Signals;
using TMPro;
using UI.Signals;
using UnityEngine;

namespace UI
{
    public class PlayerTipController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerTipText;


        private void OnEnable()
        {
            SignalsHub.AddListener<SetPlayerTipCommand>(OnSetPlayerTip);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<SetPlayerTipCommand>(OnSetPlayerTip);
        }

        private void Start()
        {
            playerTipText.text = string.Empty;
        }

        private void OnSetPlayerTip(SetPlayerTipCommand command)
        {
            // playerTipText.text = command.PlayerTipText;
            StartCoroutine(SetPlayerTipTextCoroutine(command.PlayerTipText));
        }

        private IEnumerator SetPlayerTipTextCoroutine(string text)
        {
            playerTipText.alpha = 0f;
            
            playerTipText.text = text;

            yield return playerTipText.DOFade(1f, 0.5f);

            yield return new WaitForSeconds(5f);

            yield return playerTipText.DOFade(0f, 0.5f);
        }
    }
}