using UnityEngine;
using Utilities.Monads;

namespace Hint
{
    public class HintView : MonoBehaviour
    {
        [SerializeField] private GameObject pressEHintView;
        [SerializeField] private Vector3 pressEHintButtonOffset = new(0, 1f);

        public GameObject PressEHintView => pressEHintView;
        public Vector3 PressEHintButtonOffset => pressEHintButtonOffset;

        private IMaybe<Transform> maybeTarget = Maybe.Empty<Transform>(); 

        public Transform PressEHintViewTarget
        {
            set
            {
                if (value == null)
                {
                    maybeTarget = Maybe.Empty<Transform>();
                    return;
                }

                maybeTarget = Maybe.Of(value);
            }
        }

        private void Update()
        {
            maybeTarget.IfPresent(target =>
            {
                transform.position = Camera.main.WorldToScreenPoint(target.position) + PressEHintButtonOffset;
            });
        }
    }
}