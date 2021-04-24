using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LD48
{
    public class Player : MonoBehaviour
    {
        public float runSpeed = 20.0f;
        
        private Rigidbody2D body;
        private Human human;

        private float horizontal;
        private float vertical;

        [SerializeField] private bool isReadyToShoot = false;

        private void Start()
        {
            body = GetComponent<Rigidbody2D>();
            human = GetComponent<Human>();
        }

        private void Update()
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            if (Input.GetButtonDown("Fire1"))
            {
                Debug.Log("Fire1");
                if (isReadyToShoot)
                {
                    human.Shoot();
                }
                else
                {
                    human.Fire();
                }
            }

            if (Input.GetButtonDown("Fire2"))
            {
                Debug.Log("Fire2");
                isReadyToShoot = !isReadyToShoot;
            }
        }

        private void FixedUpdate()
        {
            body.velocity = new Vector2(horizontal * runSpeed, vertical * runSpeed);
        }
    }
}
