using UnityEngine;
using UnityEngine.UI;

namespace Journal.ToggleButton
{
    public class JournalToggleButtonView : MonoBehaviour
    {
        [SerializeField] private Button journalButton;

        public Button JournalButton => journalButton;
    }
}