using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue
{
    public class DialogueView : MonoBehaviour
    {
        [SerializeField] private Image characterPortraitImage;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI characterLineText;

        public Image CharacterPortraitImage => characterPortraitImage;
        public TextMeshProUGUI CharacterNameText => characterNameText;
        public TextMeshProUGUI CharacterLineText => characterLineText;
    }
}