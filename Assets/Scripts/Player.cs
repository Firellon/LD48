using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LD48
{
    public class Player : MonoBehaviour
    {
        public TMP_Text woodAmountText;
        public TMP_Text tipMessage;
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
                human.SwitchReadyToShoot();
            }

            woodAmountText.text = $"Wood: {human.woodAmount} / {human.maxWoodAmount}";
            tipMessage.text = human.GetTipMessageText();
        }

        private void FixedUpdate()
        {
            var moveVector = new Vector2(horizontal, vertical); 
            human.Move(moveVector);
        }
    }
}
