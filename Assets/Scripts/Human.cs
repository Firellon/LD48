using UnityEngine;

namespace LD48
{
    public class Human : MonoBehaviour
    {
        private Rigidbody2D body;
        private new SpriteRenderer renderer;
        
        public int woodAmount = 0;
        public int maxWoodAmount = 10;
        public float moveSpeed = 5f;
        public Vector2 bulletPosition;
        public GameObject bulletPrefab;
        
        [SerializeField] private bool isReadyToShoot = false;

        public Sprite notReadyToShootSprite;
        public Sprite readyToShootSprite;
        
        private void Start()
        {
            body = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();
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

        public void Move(Vector2 moveDirection)
        {
            body.velocity = moveDirection * moveSpeed;
            if (moveDirection.x != 0)
            {
                renderer.flipX = moveDirection.x < 0;    
            }
        }
        
        public void Fire()
        {
            Debug.Log("Fire");
            
        }

        public void Shoot()
        {
            Debug.Log("Shoot");
            var bullet = Instantiate(bulletPrefab, transform);

            var xDirection = renderer.flipX ? -1 : 1;
            bullet.GetComponent<Bullet>().SetDirection(new Vector2(xDirection, 0));
            
            bullet.transform.localPosition = bulletPosition * new Vector2(xDirection, 1);
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

        public void SwitchReadyToShoot()
        {
            isReadyToShoot = !isReadyToShoot;
            renderer.sprite = isReadyToShoot ? readyToShootSprite : notReadyToShootSprite;
        }

        public void Act()
        {
            if (isReadyToShoot)
            {
                Shoot();
            }
            else
            {
                Fire();
            }
        }

        public void Hit()
        {
            Debug.Log("Hit!");
        }
    }
}