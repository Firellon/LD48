using System;
using Signals;
using TMPro;
using UI.Signals;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Monads;

namespace UI
{
    public class TextInputPopupController : MonoBehaviour
    {
        [SerializeField] private GameObject popupGameObject;
        [SerializeField] private TextMeshProUGUI textInputLabel;
        [SerializeField] private TMP_InputField textInputField;
        [SerializeField] private Button closePopupButton;

        private IMaybe<Func<string, string>> maybeOnTextEntered = Maybe.Empty<Func<string, string>>();

        private void OnEnable()
        {
            SignalsHub.AddListener<ShowTextInputPopupCommand>(Show);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<ShowTextInputPopupCommand>(Show);
        }

        private void Start()
        {
            textInputLabel.text = string.Empty;
            textInputField.text = string.Empty;
            
            closePopupButton.onClick.RemoveAllListeners();
            closePopupButton.onClick.AddListener(OnClose);

            Hide();
        }

        private void Show(ShowTextInputPopupCommand command)
        {
            textInputLabel.text = command.Label;
            textInputField.text = command.CurrentText;
            maybeOnTextEntered = command.OnTextEntered.ToMaybe();
            popupGameObject.SetActive(true);
        }

        private void Hide()
        {
            textInputLabel.text = string.Empty;
            textInputField.text = string.Empty;
            popupGameObject.SetActive(false);
            maybeOnTextEntered = Maybe.Empty<Func<string, string>>();
        }
        
        private void OnClose()
        {
            maybeOnTextEntered.IfPresent(onTextEntered => onTextEntered.Invoke(textInputField.text));
            Hide();
        }
    }
}