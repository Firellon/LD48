using UnityEngine;

namespace LD48
{
    public class Human : MonoBehaviour
    {
        private Rigidbody2D body;
        
        public int woodAmount = 0;
        public int maxWoodAmount = 10;
        
        private void Start()
        {
            body = GetComponent<Rigidbody2D>();
        }
        private void OnCollisionEnter2D(Collision2D hit)
        {
            if (hit.gameObject.CompareTag("Wall"))
            {
                body.velocity = Vector2.zero;
            }

            if (hit.gameObject.CompareTag("Item"))
            {
                var item = hit.gameObject.GetComponent<Item>();
                if (item == null)
                {
                    Debug.LogError($"OnCollisionEnter2D > {hit.gameObject} has Item tag and lacks Item component!");
                    return;
                }

                PickUp(item);
            }
        }
        
        public void Fire()
        {
            Debug.Log("Fire");
        }

        public void Shoot()
        {
            Debug.Log("Shoot");
        }
        
        private void PickUp(Item item)
        {
            Debug.Log($"PickUp > {item.type}");
            switch (item.type)
            {
                case ItemType.Wood:
                    if (woodAmount < maxWoodAmount)
                    {
                        woodAmount++;
                        Destroy(item.gameObject);
                    }
                    break;
            }
        }
    }
}