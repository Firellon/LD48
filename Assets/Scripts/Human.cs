using System.Collections.Generic;
using System.Linq;
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
        public float fireTouchRadius = 2f;
        public GameObject bonfirePrefab;

        public GameObject woodPrefab;
        
        [SerializeField] private bool isReadyToShoot = false;

        public Sprite notReadyToShootSprite;
        public Sprite readyToShootSprite;
        public Sprite hitSprite;
        public Sprite deadSprite;

        private bool isHit = false;
        private bool isDead = false;

        public float baseTimeToRecover = 5f;
        private float timeToRecover = 0f;

        public float baseTimeToReload = 0.5f;
        private float timeToReload = 0f;
        private bool isReloading = false;
        
        private void Start()
        {
            body = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (isReloading)
            {
                if (timeToReload > 0f)
                {
                    timeToReload -= Time.deltaTime;
                }
                else
                {
                    isReloading = false;
                }
            }
            
            if (isHit && !isDead)
            {
                if (timeToRecover > 0f)
                {
                    timeToRecover -= Time.deltaTime;
                }
                else
                {
                    isHit = false;
                    renderer.sprite = GetRegularSprite();
                }
            }
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
            if (isHit || isDead) return;
            body.velocity = moveDirection * moveSpeed;
            if (moveDirection.x != 0)
            {
                renderer.flipX = moveDirection.x < 0;    
            }
        }
        
        public void Fire()
        {
            if (isHit || isDead) return;
            
            if (woodAmount == 0)
            {
                // TODO: Show a proper message
                Debug.Log("No Wood to burn!");
                return;
            }

            var bonfires = GetClosestBonfires();
            if (bonfires.Any())
            {
                woodAmount--;
                bonfires.First().AddWood();
            }
            else
            {
                woodAmount--;
                CreateBonfire();
            }
        }

        private IEnumerable<Bonfire> GetClosestBonfires()
        {
            return Physics2D.OverlapCircleAll(transform.position, fireTouchRadius, 1 << LayerMask.NameToLayer("Default"))
                .Select(collider => collider.gameObject.GetComponent<Bonfire>())
                .Where(bonfire => bonfire != null);
        }
        

        private void CreateBonfire()
        {
            var bonfire = Instantiate(bonfirePrefab, transform.position, Quaternion.identity);
        }

        private void Shoot()
        {
            if (isReloading || isHit || isDead) return;
            Debug.Log("Shoot");
            var bullet = Instantiate(bulletPrefab, transform);

            var xDirection = renderer.flipX ? -1 : 1;
            bullet.GetComponent<Bullet>().SetDirection(new Vector2(xDirection, 0));
            
            bullet.transform.localPosition = bulletPosition * new Vector2(xDirection, 1);
            isReloading = true;
            timeToReload = baseTimeToReload;
        }
        
        private void PickUp(Item item)
        {
            if (isHit || isDead) return;
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
            if (isHit || isDead) return;
            isReadyToShoot = !isReadyToShoot;
            renderer.sprite = GetRegularSprite();
        }

        private Sprite GetRegularSprite()
        {
            return isReadyToShoot ? readyToShootSprite : notReadyToShootSprite;
        }
        
        public void Act()
        {
            if (isHit || isDead) return;
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
            if (!isHit && !isDead)
            {
                isHit = true;
                renderer.sprite = hitSprite;
                timeToRecover = 5f;
            }
            else
            {
                isDead = true;
                renderer.sprite = deadSprite;
                DropItems();
            }
        }

        private void DropItems()
        {
            while (woodAmount > 0)
            {
                woodAmount--;
                Instantiate(woodPrefab, transform.position + new Vector3(Random.value, Random.value), Quaternion.identity);
            }
        }

        public string GetTipMessageText()
        {
            if (isHit)
            {
                return "You have been wounded, a few seconds needed to recover!";
            }
            
            if (isDead)
            {
                return "Alas, you have died. Press R to restart.";
            }
            
            if (isReadyToShoot)
            {
                return "Press LMB to Shoot";
            }
            else
            {
                if (woodAmount <= 0) return "Gather some Wood to survive through the Night";
                var bonfires = GetClosestBonfires();
                return bonfires.Any() ? "Press LMB to add Wood to the bonfire" : "Press LMB to start a new Bonfire";
            }
        }
    }
}