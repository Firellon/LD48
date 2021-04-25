using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LD48
{
    public class Bullet : MonoBehaviour
    {
        public float moveSpeed = 20f;
        
        private  Rigidbody2D body;
        
        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void OnCollisionEnter2D(Collision2D hit)
        {
            if (hit.gameObject.CompareTag("Wall"))
            {
                Destroy();
            }
        }

        private void OnTriggerEnter2D(Collider2D hit)
        {
            if (hit.gameObject.CompareTag("Human") && transform.parent.gameObject != hit.gameObject)
            {
                var human = hit.transform.parent.gameObject.GetComponent<Human>();
                if (human == null) {
                    Debug.LogError($"OnTriggerEnter2D > {hit.gameObject} has Human tag and lacks Human component on its parent!");
                    return;
                }

                human.Hit();
                Destroy();
            }
        }

        private void Destroy()
        {
            Destroy(gameObject);
        }

        public void SetDirection(Vector2 bulletDirection)
        {
            body.velocity = bulletDirection * moveSpeed;
        }
    }
}
