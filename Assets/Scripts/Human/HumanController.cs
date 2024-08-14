using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Day;
using DG.Tweening;
using Environment;
using Human.Signal;
using Inventory;
using Inventory.Signals;
using JetBrains.Annotations;
using LD48;
using LD48.CharacterController2D;
using Map;
using Signals;
using UnityEngine;
using Utilities.Monads;
using Utilities.Prefabs;
using Zenject;
using Random = UnityEngine.Random;

namespace Human
{
    [RequireComponent(typeof(PlayerMovement2D))]
    public class HumanController : MonoBehaviour, IHittable
    {
        [Inject] private IPrefabPool prefabPool;
        [Inject] private IInventory inventory;
        [Inject] private IMapObjectRegistry mapObjectRegistry;
        [Inject] private VisualsConfig visualsConfig;

        public IInventory Inventory => inventory;
        public HumanState State { get; } = new();

        private PlayerMovement2D characterController;
        private Rigidbody2D body;
        private TerrainGenerator terrainGenerator;
        private DayNightCycle dayNightCycle;
        private Vector2 levelSize = new(float.PositiveInfinity, float.PositiveInfinity);

        public float moveSpeed = 2.5f;
        public Vector2 bulletPosition;
        public GameObject bulletPrefab;
        public float fireTouchRadius = 1f;
        public float mapObjectTouchRadius = 1f;

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator humanAnimator;
        [SerializeField] private Collider2D collider2d;
        [SerializeField] private HumanGender humanGender;

        private bool isHit = false;
        private bool isAiming = false;
        private bool isSurrendering = false;
        
        public bool IsSurrendering => isSurrendering;

        public float baseTimeToRecover = 5f;
        private float timeToRecover = 0f;

        public float baseTimeToReload = 0.5f;
        [SerializeField] private float shootAnimationLengthSeconds = 0.5f;
        private float timeToReload = 0f;
        private bool isReloading = false;


        public float baseTimeToRest = 3f;
        private float timeToRest = 3f;

        private bool isResting = false;
        public bool IsResting => isResting;

        private readonly List<IInteractable> interactableObjects = new();

        #region Audio

        public AudioSource audio;
        [CanBeNull] public AudioClip hitSound;
        [CanBeNull] public AudioClip deadSound;
        [CanBeNull] public AudioClip shootSound;
        [CanBeNull] public AudioClip itemPickupSound;

        public AudioSource walkAudio;
        [CanBeNull] public AudioClip walkSound;

        #endregion

        #region Animations

        private static readonly int MovementSpeedAnimation = Animator.StringToHash("MovementSpeed");
        private static readonly int IsRestingAnimation = Animator.StringToHash("IsResting");
        private static readonly int IsReloadingAnimation = Animator.StringToHash("IsReloading");
        private static readonly int ShootingAnimation = Animator.StringToHash("Shoot");
        private static readonly int IsAimingAnimation = Animator.StringToHash("IsAiming");
        private static readonly int IsHitAnimation = Animator.StringToHash("IsHit");
        private static readonly int IsDeadAnimation = Animator.StringToHash("IsDead");
        private static readonly int IsPickingUpAnimation = Animator.StringToHash("IsPickingUp");
        private static readonly int IsInteractingAnimation = Animator.StringToHash("IsInteracting");
        private static readonly int HasBookAnimation = Animator.StringToHash("HasBook");
        private static readonly int IsSurrenderingAnimation = Animator.StringToHash("IsSurrendering");

        #endregion

        private void OnEnable()
        {
            SignalsHub.AddListener<PlayerHandItemUpdatedEvent>(OnPlayerItemUpdated);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<PlayerHandItemUpdatedEvent>(OnPlayerItemUpdated);
        }

        private void OnPlayerItemUpdated(PlayerHandItemUpdatedEvent evt)
        {
            evt.MaybeItem
                .IfPresent(item => { humanAnimator.SetBool(HasBookAnimation, item.ItemType == ItemType.Book); })
                .IfNotPresent(() => { humanAnimator.SetBool(HasBookAnimation, false); });
        }

        private void Awake()
        {
            characterController = GetComponent<PlayerMovement2D>();
        }

        public bool IsDead => State.IsDead;
        public bool HasWon => State.HasWon;

        bool IHittable.IsDead()
        {
            return IsDead;
        }

        public bool IsThreat()
        {
            return !HasWon && !isHit && !IsDead && isAiming;
        }

        private void Start()
        {
            body = GetComponent<Rigidbody2D>();
            terrainGenerator = Camera.main.GetComponent<TerrainGenerator>();
            dayNightCycle = Camera.main.GetComponent<DayNightCycle>();
            levelSize = terrainGenerator.levelSize;
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

            if (isHit && !IsDead)
            {
                if (timeToRecover > 0f)
                {
                    timeToRecover -= Time.deltaTime;
                }
                else
                {
                    isHit = false;
                    UpdateInventoryIsHelpless();
                    humanAnimator.SetBool(IsHitAnimation, false);
                    foreach (var interactableObject in interactableObjects)
                    {
                        interactableObject.SetHighlight(true);
                    }
                }
            }

            if (!isAiming && !characterController.IsMoving && !isHit && !IsDead)
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

        private void OnCollisionEnter2D(Collision2D other)
        {
            // FIXME: Do We need this at all?
            if (other.gameObject.CompareTag("Wall"))
            {
                if (!body) return;
                body.velocity = Vector2.zero;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Item") || other.gameObject.CompareTag("MapObject"))
            {
                var interactable = other.gameObject.GetComponent<IInteractable>();
                if (interactable == null)
                {
                    Debug.LogError(
                        $"OnCollisionEnter2D > {other.gameObject} has Item/MapObject tag and lacks IInteractable component!");
                    return;
                }

                interactable.SetHighlight(true);
                interactableObjects.Add(interactable);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Item") || other.gameObject.CompareTag("MapObject"))
            {
                var interactable = other.gameObject.GetComponent<IInteractable>();
                if (interactable == null)
                {
                    Debug.LogError(
                        $"OnCollisionExit2D > {other.gameObject} has Item tag and lacks IInteractable component!");
                    return;
                }

                interactable.SetHighlight(false);
                interactableObjects.Remove(interactable);
                SignalsHub.DispatchAsync(new InteractableExitEvent(this, interactable));
            }
        }

        public void Move(Vector2 moveDirection)
        {
            if (isHit || IsDead || isReloading)
            {
                StopMovement();
                return;
            }

            if (!body) return;

            var moveLength = moveDirection.magnitude;

            humanAnimator.SetFloat(MovementSpeedAnimation, moveSpeed * moveLength);
            characterController.MoveSpeed = Mathf.Min(moveLength * moveSpeed, moveSpeed);
            characterController.Move(moveDirection);

            if (moveLength > float.Epsilon)
            {
                if (walkAudio != null && !walkAudio.isPlaying)
                    walkAudio.Play();
            }
            else
            {
                if (walkAudio != null && walkAudio.isPlaying)
                    walkAudio.Stop();
            }

            if (moveDirection.x != 0)
            {
                spriteRenderer.flipX = moveDirection.x < 0;
            }
        }

        private void UseItem(Item item)
        {
            switch (item.ItemType)
            {
                case ItemType.Wood:
                    AddToFire(item);
                    return;
                case ItemType.Bonfire:
                    LightAFire(item);
                    return;
                case ItemType.Pistol:
                    Shoot(item);
                    return;
                case ItemType.Book:
                    AddToFire(item);
                    return;
                case ItemType.Key:
                    OpenExit(item);
                    return;
                case ItemType.Crate:
                    PlaceCrate(item);
                    return;
                case ItemType.GuidePost:
                    PlaceGuidePost(item);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void AddToFire(Item burnableItem)
        {
            if (isHit || IsDead) return;

            var bonfires = GetClosestMapObjects<MapBonfire>();
            if (bonfires.Any())
            {
                inventory.SetHandItem(Maybe.Empty<Item>());
                StartCoroutine(AddToFireCoroutine(bonfires.First(), burnableItem));
            }
        }

        private IEnumerator AddToFireCoroutine(MapBonfire bonfire, Item burnableItem)
        {
            var burnableItemObject = prefabPool.Spawn(visualsConfig.TemporaryItemPrefab, transform);
            burnableItemObject.GetComponent<SpriteRenderer>().sprite = burnableItem.ItemSprite;
            var bonfirePosition = bonfire.transform.position;
            var itemThrowDurationSeconds = 0.5f;
            yield return burnableItemObject.transform.DOJump(bonfirePosition, 1f, 1, itemThrowDurationSeconds).WaitForCompletion();

            prefabPool.Despawn(burnableItemObject);

            bonfire.AddBurnableItem(burnableItem);
        }

        public void LightAFire(Item bonfireItem)
        {
            if (isHit || IsDead) return;

            var bonfires = GetClosestMapObjects<MapBonfire>();
            if (bonfires.None())
            {
                inventory.SetHandItem(Maybe.Empty<Item>());
                CreateBonfire();
            }
        }

        private void PlaceCrate(Item item)
        {
            if (isHit || IsDead) return;
            if (item.ItemType != ItemType.Crate)
            {
                Debug.LogError($"{nameof(PlaceCrate)} > cannot place {item.ItemType} instead!");
                return;
            }

            var crates = GetClosestMapObjects<MapCrate>();
            if (crates.None())
            {
                inventory.SetHandItem(Maybe.Empty<Item>());
                var cratePrefab = mapObjectRegistry.GetMapObject(MapObjectType.Crate).Prefab;
                var crate = prefabPool.Spawn(cratePrefab, transform.position + Vector3.down * 0.5f,
                    Quaternion.identity);
                SignalsHub.DispatchAsync(new MapObjectAddedEvent(crate, MapObjectType.Crate));
            }
        }

        private void PlaceGuidePost(Item item)
        {
            if (isHit || IsDead) return;
            if (item.ItemType != ItemType.GuidePost)
            {
                Debug.LogError($"{nameof(PlaceGuidePost)} > cannot place {item.ItemType} instead!");
                return;
            }

            var guidePosts = GetClosestMapObjects<MapGuidePost>();
            if (guidePosts.None())
            {
                inventory.SetHandItem(Maybe.Empty<Item>());
                var guidePostPrefab = mapObjectRegistry.GetMapObject(MapObjectType.GuidePost).Prefab;
                var guidePost = prefabPool.Spawn(guidePostPrefab, transform.position + Vector3.down * 0.5f,
                    Quaternion.identity);
                SignalsHub.DispatchAsync(new MapObjectAddedEvent(guidePost, MapObjectType.GuidePost));
            }
        }

        private void OpenExit(Item keyItem)
        {
            if (isHit || IsDead) return;

            var exits = GetClosestMapObjects<MapExit>();
            if (exits.Any())
            {
                inventory.SetHandItem(Maybe.Empty<Item>());
                Debug.Log("Successfully exited the forest!");
                State.Win();
            }
        }

        private IList<T> GetClosestMapObjects<T>() where T : MonoBehaviour
        {
            return Physics2D.OverlapCircleAll(transform.position, mapObjectTouchRadius,
                    1 << LayerMask.NameToLayer("Solid"))
                .Select(mapObjectCollider => mapObjectCollider.gameObject.GetComponent<T>())
                .Where(bonfire => bonfire != null)
                .ToList();
        }

        private void CreateBonfire()
        {
            var bonfirePrefab = mapObjectRegistry.GetMapObject(MapObjectType.Bonfire).Prefab;
            var bonfire = prefabPool.Spawn(bonfirePrefab, transform.position + Vector3.down * 0.5f,
                Quaternion.identity);
        }

        private void Shoot(Item item)
        {
            if (isReloading || isHit || IsDead) return;

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
            var bullet = bulletObject.GetComponent<BulletController>();
            bullet.SetDirection(new Vector2(xDirection, 0));

            bulletObject.transform.localPosition = bulletPosition * new Vector2(xDirection, 1);
            humanAnimator.SetBool(IsReloadingAnimation, true);
            timeToReload = baseTimeToReload;
            isReloading = true;
            if (shootSound) audio.PlayOneShot(shootSound);
        }

        public void StopMovement()
        {
            body.velocity = Vector2.zero;
            characterController.MoveSpeed = 0;
            humanAnimator.SetFloat(MovementSpeedAnimation, 0);
            walkAudio.Stop();
        }

        public void PickUp(IInteractable interactable)
        {
            if (isHit || IsDead || !interactable.CanBePickedUp) return;

            humanAnimator.SetTrigger(IsPickingUpAnimation);

            if (!inventory.CanAddItem()) return;

            if (itemPickupSound)
                audio.PlayOneShot(itemPickupSound);

            interactable.MaybeItem.IfPresent(item => inventory.AddItem(item));
            interactableObjects.Remove(interactable);
            interactable.Remove();
        }

        public void SetIsSurrendering(bool newIsSurrendering)
        {
            if (isHit || IsDead) return;

            isSurrendering = newIsSurrendering;
            UpdateInventoryIsHelpless();
            StopMovement();
            humanAnimator.SetBool(IsSurrenderingAnimation, newIsSurrendering);
        }

        public void SetIsAiming(bool newIsAiming)
        {
            if (isHit || IsDead)
            {
                humanAnimator.SetBool(IsAimingAnimation, false);
                return;
            }

            isAiming = newIsAiming;
            humanAnimator.SetBool(IsAimingAnimation, isAiming);
        }

        public void Act()
        {
            if (isHit || IsDead) return;

            inventory.HandItem.IfPresent(UseItem);
        }

        public void Interact()
        {
            if (interactableObjects.Any())
            {
                var firstInteractableObject = interactableObjects.First();
                // TODO: Implement other ways to interact with objects
                if (firstInteractableObject.CanBePickedUp)
                {
                    PickUp(firstInteractableObject);
                    return;
                }

                firstInteractableObject.Interact(this);
            }
            else
            {
                // TODO: Check if has an item in his hand and can use it
            }
        }

        public void Hit()
        {
            if (HasWon) return;

            StopMovement();
            if (!isHit && !IsDead)
            {
                isHit = true;
                isAiming = false;
                isSurrendering = false;
                isResting = false;
                
                humanAnimator.SetBool(IsHitAnimation, true);
                timeToRecover = baseTimeToRecover;
                // DropItems();
                // TODO: Update Collider on other and on recover
                if (hitSound) audio.PlayOneShot(hitSound);
                foreach (var interactableObject in interactableObjects)
                {
                    interactableObject.SetHighlight(false);
                }

                if (!inventory.MoveHandItemToInventory())
                {
                    inventory.HandItem.IfPresent(handItem =>
                    {
                        TryDropItem(handItem);
                        inventory.SetHandItem(Maybe.Empty<Item>());
                    }).IfNotPresent(() =>
                    {
                        Debug.LogError("Impossible situation: MoveHandItemToInventory returned false while no hand item is present!");
                    });
                }
                UpdateInventoryIsHelpless();
            }
            else
            {
                Die();
            }
        }

        private void UpdateInventoryIsHelpless()
        {
            inventory.IsHelpless = isHit && isSurrendering;
        }

        public void Die()
        {
            if (HasWon) return;

            StopMovement();
            State.Die();
            humanAnimator.SetBool(IsDeadAnimation, true);
            collider2d.enabled = false;
            terrainGenerator.AddDead(transform);
            if (deadSound) audio.PlayOneShot(deadSound);

            var corpseMapObject = mapObjectRegistry.GetMapObject(MapObjectType.Corpse);
            var corpse = prefabPool.Spawn(
                corpseMapObject.Prefab,
                transform.position,
                Quaternion.identity);
            corpse.GetComponent<MapObjectController>().SetMapObject(corpseMapObject);
            var mapCorpse = corpse.GetComponent<MapCorpse>();
            mapCorpse.SetHumanGender(humanGender);
            mapCorpse.SetItems(inventory.Items);
            // TODO: Drop the Hand item
            SignalsHub.DispatchAsync(new MapObjectAddedEvent(corpse, MapObjectType.Corpse));
            
            SignalsHub.DispatchAsync(new HumanDiedEvent(this));
            // Destroy(gameObject);
        }

        private void DropItems()
        {
            inventory.HandItem.IfPresent(item =>
            {
                TryDropItem(item);
                inventory.SetHandItem(Maybe.Empty<Item>());
            });

            while (inventory.Items.Any())
            {
                var firstItem = inventory.Items.First();
                inventory.RemoveItem(firstItem);
                TryDropItem(firstItem);
            }
        }

        private void TryDropItem(Item item)

        {
            if (!item.CanBeDropped) return;

            var itemObject = prefabPool.Spawn(item.ItemPrefab,
                transform.position + new Vector3(Random.value, Random.value),
                Quaternion.identity);
            itemObject.GetComponent<ItemController>().SetItem(item);
        }

        public bool IsFacingTowards(Vector3 position)
        {
            return spriteRenderer.flipX ? transform.position.x - position.x > 0 : position.x - transform.position.x > 0;
        }

        public bool CanPickUp(out IInteractable item)
        {
            item = interactableObjects.FirstOrDefault();

            return item is {CanBePickedUp: true};
        }

        public bool CanTakeItemFromContainer(ItemType itemType, out IItemContainer itemContainer)
        {
            itemContainer = interactableObjects.OfType<IItemContainer>()
                .FirstOrDefault(container => container.CanTakeItem() && container.HasItem(itemType));

            return itemContainer != null;


        }
    }
}