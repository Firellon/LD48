using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Day;
using JetBrains.Annotations;
using LD48.CharacterController2D;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;
using Random = UnityEngine.Random;

namespace LD48
{
    [RequireComponent(typeof(PlayerMovement2D))]
    public class Human : MonoBehaviour, IHittable
    {
        [Inject] private IPrefabPool prefabPool;

        private PlayerMovement2D characterController;

        private Rigidbody2D body;
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

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator humanAnimator;
        [SerializeField] private Collider2D collider2d;

        private bool isHit = false;
        private bool isDead = false;
        private bool isAiming = false;

        public float baseTimeToRecover = 5f;
        private float timeToRecover = 0f;

        public float baseTimeToReload = 0.5f;
        [SerializeField] private float shootAnimationLengthSeconds = 0.5f;
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

        private static readonly int MovementSpeedAnimation = Animator.StringToHash("MovementSpeed");
        private static readonly int IsRestingAnimation = Animator.StringToHash("IsResting");
        private static readonly int IsReloadingAnimation = Animator.StringToHash("IsReloading");
        private static readonly int ShootingAnimation = Animator.StringToHash("Shoot");
        private static readonly int IsAimingAnimation = Animator.StringToHash("IsAiming");
        private static readonly int IsHitAnimation = Animator.StringToHash("IsHit");
        private static readonly int IsDeadAnimation = Animator.StringToHash("IsDead");

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
            return !isHit && !isDead && isAiming;
        }

        private void Start()
        {
            body = GetComponent<Rigidbody2D>();
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
                    humanAnimator.SetBool(IsReloadingAnimation, false);
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
                    humanAnimator.SetBool(IsHitAnimation, false);
                }
            }

            if (!isAiming && !characterController.IsMoving && !isHit && !isDead)
            {
                if (!isResting)
                {
                    isResting = true;
                    humanAnimator.SetFloat(MovementSpeedAnimation, 0f);
                    timeToRest = baseTimeToRest;
                }
            }
            else
            {
                isResting = false;
                humanAnimator.SetBool(IsRestingAnimation, false);
            }

            if (isResting && !humanAnimator.GetBool(IsRestingAnimation))
            {
                if (timeToRest > 0f)
                {
                    timeToRest -= Time.deltaTime;
                }
                else
                {
                    humanAnimator.SetBool(IsRestingAnimation, true);
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
            if (isHit || isDead || isReloading)
            {
                StopMovement();
                return;
            }

            if (!body) return;

            humanAnimator.SetFloat(MovementSpeedAnimation, moveSpeed * moveDirection.magnitude);
            characterController.MoveSpeed = moveSpeed;
            characterController.Move(moveDirection);

            if (moveDirection.x != 0)
            {
                spriteRenderer.flipX = moveDirection.x < 0;
            }
        }

        public void LightAFire()
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

        private IList<Bonfire> GetClosestBonfires()
        {
            return Physics2D.OverlapCircleAll(transform.position, fireTouchRadius, 1 << LayerMask.NameToLayer("Solid"))
                .Select(collider => collider.gameObject.GetComponent<Bonfire>())
                .Where(bonfire => bonfire != null)
                .ToList();
        }


        private void CreateBonfire()
        {
            var bonfire = prefabPool.Spawn(bonfirePrefab, transform.position + Vector3.down * 0.5f,
                Quaternion.identity);
        }

        private void Shoot()
        {
            if (isReloading || isHit || isDead) return;
            Debug.Log("Shoot");
            StopMovement();
            isReloading = true;
            timeToReload = baseTimeToReload;
            humanAnimator.SetTrigger(ShootingAnimation);
            StartCoroutine(ShootCoroutine());
        }

        private IEnumerator ShootCoroutine()
        {
            yield return new WaitForSeconds(shootAnimationLengthSeconds);

            var bulletObject = prefabPool.Spawn(bulletPrefab, transform);
            var xDirection = spriteRenderer.flipX ? -1 : 1;
            var bullet = bulletObject.GetComponent<Bullet>();
            bullet.SetDirection(new Vector2(xDirection, 0));

            bulletObject.transform.localPosition = bulletPosition * new Vector2(xDirection, 1);
            humanAnimator.SetBool(IsReloadingAnimation, true);
            timeToReload = baseTimeToReload;
            isReloading = true;
            if (shootSound) audio.PlayOneShot(shootSound);
        }

        private void StopMovement()
        {
            body.velocity = Vector2.zero;
            characterController.MoveSpeed = 0;
            humanAnimator.SetFloat(MovementSpeedAnimation, 0);
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

        public void ToggleIsAiming()
        {
            if (isHit || isDead) return;
            isAiming = !isAiming;
            humanAnimator.SetBool(IsAimingAnimation, isAiming);
        }

        public void Act()
        {
            if (isHit || isDead) return;
            if (isAiming)
            {
                Shoot();
            }
            else
            {
                LightAFire();
            }
        }

        public void Hit()
        {
            StopMovement();
            if (!isHit && !isDead)
            {
                isHit = true;
                isAiming = false;
                humanAnimator.SetBool(IsHitAnimation, true);
                timeToRecover = baseTimeToRecover;
                DropItems();
                // TODO: Update Collider on hit and on recover
                if (hitSound) audio.PlayOneShot(hitSound);
            }
            else
            {
                isDead = true;
                humanAnimator.SetBool(IsDeadAnimation, true);
                collider2d.enabled = false;
                terrainGenerator.AddDead(transform);
                if (deadSound) audio.PlayOneShot(deadSound);
            }
        }

        private void DropItems()
        {
            while (woodAmount > 0)
            {
                woodAmount--;
                prefabPool.Spawn(woodPrefab, transform.position + new Vector3(Random.value, Random.value),
                    Quaternion.identity);
            }
        }

        public string GetTipMessageText()
        {
            if (isDead)
            {
                return
                    $"Alas, you have died after surviving for {dayNightCycle.GetCurrentDay()} days. Press R to restart.";
            }

            if (isHit)
            {
                return "You have been wounded, a few seconds needed to recover!";
            }

            if (IsCloseToMapBorder())
            {
                return "You are about to leave the Forest!\n To find what you seek, try going Deeper instead.";
            }

            if (isAiming)
            {
                return "Press LMB to Shoot";
            }

            if (woodAmount <= 0) return "Gather some Wood to survive through the Night";
            var bonfires = GetClosestBonfires();
            return bonfires.Any() ? "Press LMB to add Wood to the bonfire" : "Press LMB to start a new Bonfire";
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
            return spriteRenderer.flipX ? transform.position.x - position.x > 0 : position.x - transform.position.x > 0;
        }
    }
}