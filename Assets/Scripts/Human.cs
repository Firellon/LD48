using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LD48.CharacterController2D;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LD48
{
    public class Human : MonoBehaviour, IHittable
    {
        private PlayerMovement2D characterController;

        private Rigidbody2D body;
        private new SpriteRenderer renderer;
        private TerrainGenerator terrainGenerator;
        private DayNightCycle dayNightCycle;
        private Vector2 levelSize = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        
        public int woodAmount = 0;
        public int maxWoodAmount = 10;
        public float moveSpeed = 2.5f;
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
        public Sprite restingSprite;

        private bool isHit = false;
        private bool isDead = false;

        public float baseTimeToRecover = 5f;
        private float timeToRecover = 0f;

        public float baseTimeToReload = 0.5f;
        private float timeToReload = 0f;
        private bool isReloading = false;
        
        public float baseTimeToRest = 3f;
        private float timeToRest = 3f;
        private bool isResting = false;

        public new AudioSource audio;
        [CanBeNull] public AudioClip hitSound;
        [CanBeNull] public AudioClip deadSound;
        [CanBeNull] public AudioClip shootSound;
        [CanBeNull] public AudioClip itemPickupSound;

        private void Awake()
        {
            characterController = GetComponent<PlayerMovement2D>();
        }

        public bool IsDead()
        {
            return isDead;
        }

        public bool IsThreat()
        {
            return !isHit && !isDead && isReadyToShoot;
        }

        private void Start()
        {
            body = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();
            terrainGenerator = Camera.main.GetComponent<TerrainGenerator>();
            dayNightCycle = Camera.main.GetComponent<DayNightCycle>();
            levelSize = terrainGenerator.levelSize;
            if (!audio) audio = GetComponent<AudioSource>();
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

            if (!isReadyToShoot && characterController.IsMoving && !isHit && !isDead)
            {
                if (!isResting)
                {
                    isResting = true;
                    timeToRest = baseTimeToRest;    
                }
            }
            else
            {
                isResting = false;
            }

            if (isResting)
            {
                if (timeToRest > 0f)
                {
                    timeToRest -= Time.deltaTime;
                }
                else
                {
                    renderer.sprite = restingSprite;
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D hit)
        {
            if (hit.gameObject.CompareTag("Wall"))
            {
                if (!body) return;
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
            if (!body) return;

            renderer.sprite = GetRegularSprite();
            // body.velocity = moveDirection.normalized * moveSpeed;

            characterController.MoveSpeed = moveSpeed;
            characterController.Move(moveDirection);

            // playerMotor2D.movementDir = moveDirection;
            // characterController.MovePosition(moveDirection.normalized * moveSpeed * Time.fixedDeltaTime);

            if (moveDirection.x != 0)
            {
                var scale = transform.localScale;
                scale.x = moveDirection.x < 0 ? -1 : 1;
                transform.localScale = scale;

                // renderer.flipX = moveDirection.x < 0;
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
            return Physics2D.OverlapCircleAll(transform.position, fireTouchRadius, 1 << LayerMask.NameToLayer("Solid"))
                .Select(collider => collider.gameObject.GetComponent<Bonfire>())
                .Where(bonfire => bonfire != null);
        }
        

        private void CreateBonfire()
        {
            var bonfire = Instantiate(bonfirePrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
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
            if (shootSound) audio.PlayOneShot(shootSound);
        }

        private void PickUp(Item item)
        {
            if (isHit || isDead) return;
            // Debug.Log($"PickUp > {item.type}");
            switch (item.type)
            {
                case ItemType.Wood:
                    if (woodAmount < maxWoodAmount)
                    {
                        woodAmount++;
                        if (itemPickupSound) audio.PlayOneShot(itemPickupSound);
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
            body.velocity = Vector2.zero;
            if (!isHit && !isDead)
            {
                isHit = true;
                isReadyToShoot = false;
                renderer.sprite = hitSprite;
                timeToRecover = baseTimeToRecover;
                DropItems();
                // TODO: Update Collider on hit and on recover
                if (hitSound) audio.PlayOneShot(hitSound);
            }
            else
            {
                isDead = true;
                renderer.sprite = deadSprite;
                GetComponent<Collider2D>().enabled = false;
                terrainGenerator.AddDead(transform);
                if (deadSound) audio.PlayOneShot(deadSound);
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
            if (isDead)
            {
                return $"Alas, you have died after surviving for {dayNightCycle.GetCurrentDay()} days. Press R to restart.";
            }
            
            if (isHit)
            {
                return "You have been wounded, a few seconds needed to recover!";
            }

            if (IsCloseToMapBorder())
            {
                return "You are about to leave the Forest!\n To find what you seek, try going Deeper instead.";
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

        private bool IsCloseToMapBorder()
        {
            var minBorderDistance = 1f;
            if (transform.position.x < minBorderDistance ||
                transform.position.x > levelSize.x - minBorderDistance) return true;
            if (transform.position.y < minBorderDistance ||
                transform.position.y > levelSize.y - minBorderDistance) return true;
            return false;
        }

        public bool IsFacingTowards(Vector3 position)
        {
            return renderer.flipX ? transform.position.x - position.x > 0 : position.x - transform.position.x > 0;
        }
    }
}