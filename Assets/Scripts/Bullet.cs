using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LD48
{
    public class Bullet : MonoBehaviour
    {
        public float timeToLive = 1f;
        public float moveSpeed = 20f;
        
        private  Rigidbody2D body;
        
        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }
        
        void Update()
        {
            if (timeToLive > 0)
            {
                timeToLive -= Time.deltaTime;
            }
            else
            {
                Destroy();
            }
        }

        private void OnCollisionEnter2D(Collision2D hit)
        {
            if (hit.gameObject.CompareTag("Wall"))
            {
                Destroy();
            }

            if (hit.gameObject.CompareTag("Human") && transform.parent.gameObject != hit.gameObject)
            {
                var human = hit.gameObject.GetComponent<Human>();
                if (human == null) {
                    Debug.LogError($"OnCollisionEnter2D > {hit.gameObject} has Human tag and lacks Human component!");
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
