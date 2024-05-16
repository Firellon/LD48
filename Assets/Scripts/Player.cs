using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace LD48
{
    public class Player : MonoBehaviour
    {
        public TMP_Text woodAmountText;
        [FormerlySerializedAs("tipMessage")] public TMP_Text tipMessageText;
        private Human human;

        private float horizontal;
        private float vertical;

        private void Start()
        {
            human = GetComponent<Human>();
        }

        private void Update()
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            if (Input.GetButtonDown("Fire1"))
            {
                human.Act();
            }

            if (Input.GetButtonDown("Fire2"))
            {
                human.ToggleIsAiming();
            }

            if (woodAmountText && tipMessageText)
            {
                woodAmountText.text = $"Wood: {human.woodAmount} / {human.maxWoodAmount}";
                tipMessageText.text = human.GetTipMessageText();   
            }
        }

        private void FixedUpdate()
        {
            var moveVector = new Vector2(horizontal, vertical);
            human.Move(moveVector);
        }
    }
}
