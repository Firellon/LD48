using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pagination
{
    public class PaginationView : MonoBehaviour
    {
        [SerializeField] private Button previousPageButton;
        [SerializeField] private TextMeshProUGUI pageNumberText;
        [SerializeField] private Button nextPageButton;

        public Button PreviousPageButton => previousPageButton;
        public TextMeshProUGUI PageNumberText => pageNumberText;
        public Button NextPageButton => nextPageButton;
    }
}