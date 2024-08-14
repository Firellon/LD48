using System;
using Environment;
using Human;
using Inventory;
using LD48;
using Stranger.AI;
using UnityEditor;
using UnityEngine;
using Utilities.Monads;
using Utilities.RandomService;
using Zenject;
using Random = UnityEngine.Random;

namespace Stranger
{
    public class StrangerController : MonoBehaviour
    {
        private const float K_positionEpsilon = 0.1f;

        [Inject] private IItemContainer inventory;
        [Inject] private IItemRegistry itemRegistry;
        [Inject] private IStrangerBehaviorTree behaviorTree;
        [Inject] private StrangerAiConfig config;
        [Inject] private IRandomService randomService;

        private HumanController humanController;
        private TerrainGenerator terrainGenerator;

        [SerializeField] private StrangerState state;
        [SerializeField] private Transform target;
        [Obsolete]
        [SerializeField] private float freeMoveCheckDistance = 1f;
        [SerializeField] private LayerMask solidLayerMask;
        
        [Obsolete]
        public float baseTimeToWander = 3f;
        private float timeToWander = 0f;
        private Vector2 wanderDirection = Vector2.zero;
        private Vector3 moveDirection = Vector3.zero;

        private void Start()
        {
            humanController = GetComponent<HumanController>();
            terrainGenerator = Camera.main.GetComponent<TerrainGenerator>(); // TODO: Inject

            SetUpInitialInventory();
        }

        private void SetUpInitialInventory()
        {
            // TODO: Set up a random inventory? Move into config?
            foreach (var itemSpawnConfig in config.InitialInventory)
            {
                var itemAmount = randomService.Sample(itemSpawnConfig.SpawnAmounts);
                if (itemAmount <= 0) continue;
                itemRegistry.GetItemOrEmpty(itemSpawnConfig.ItemType).IfPresent(item =>
                {
                    Debug.Log($"SetUpInitialInventory > adding {itemAmount} of {itemSpawnConfig.ItemType}");
                    for (var i = 0; i < itemAmount; i++)
                    {
                        humanController.Inventory.AddItem(item);
                    }
                });
            }
        }

        private void Update()
        {
            if (humanController.IsDead) return;
            // Reduce amount of expensive calls
            if (Random.value > config.StateCalculationProbability)
            {
                state = GetCurrentState();
            }

            if (state != StrangerState.Surrender && humanController.IsSurrendering)
            {
                humanController.SetIsSurrendering(false);
            }

            switch (state)
            {
                case StrangerState.Gather:
                    Gather();
                    break;
                case StrangerState.SeekBonfire:
                    SeekBonfire();
                    break;
                case StrangerState.StartBonfire:
                    StartBonfire();
                    break;
                case StrangerState.Fight:
                    Fight();
                    break;
                case StrangerState.Rob:
                    Rob();
                    break;
                case StrangerState.Flee:
                    Flee();
                    break;
                case StrangerState.Wander:
                    Wander();
                    break;
                case StrangerState.Surrender:
                    Surrender();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StartBonfire()
        {
            var maybeBonfireItem = itemRegistry.GetItemOrEmpty(ItemType.Bonfire);
            maybeBonfireItem.IfPresent(bonfireItem =>
            {
                humanController.LightAFire(bonfireItem);
                state = GetCurrentState();
            });
        }

        private StrangerState GetCurrentState()
        {
            behaviorTree.Evaluate();

            target = behaviorTree.State.MaybeTarget.Match(aTarget => aTarget, () => null);

            return behaviorTree.State.TargetAction;
        }

        private bool DoesHumanHaveWoodILack(HumanController otherHumanController)
        {
            var currentWoodAmount = inventory.GetItemAmount(ItemType.Wood);
            return currentWoodAmount < config.MinWoodToSurvive &&
                   otherHumanController.Inventory.GetItemAmount(ItemType.Wood) > currentWoodAmount + 2;
        }

        private void SeekBonfire()
        {
            SetReadyToShoot(false);

            var bonfireDistance = Vector2.Distance(target.transform.position, transform.position);
            if (bonfireDistance > humanController.fireTouchRadius)
            {
                Vector2 bonfireDirection = target.transform.position - transform.position;
                moveDirection = GetFreeMoveDirection(bonfireDirection.SkewDirection(5));
                humanController.Move(moveDirection);
                return;
            }

            moveDirection = Vector3.zero;
            humanController.StopMovement();

            if (inventory.GetItemAmount(ItemType.Wood) == 0) return;

            var bonfire = target.GetComponent<MapBonfire>();
            // TODO: Check if we have that wood item?
            if (Random.Range(1, 10) > bonfire.GetTimeToBurn())
            {
                var woodItem = itemRegistry.GetItem(ItemType.Wood);
                humanController.AddToFire(woodItem);
            }
        }

        private void Wander()
        {
            SetReadyToShoot(false);

            if (timeToWander > 0f)
            {
                moveDirection = GetFreeMoveDirection(wanderDirection);
                humanController.Move(moveDirection);
                timeToWander -= Time.deltaTime;
            }
            else
            {
                timeToWander = baseTimeToWander;
                wanderDirection = Random.value > 0.9f
                    ? new Vector2()
                    : new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                var centerPosition = new Vector3(terrainGenerator.levelSize.x / 2f, terrainGenerator.levelSize.y / 2f);
                var centerDirection = new Vector2(centerPosition.x - transform.position.x,
                    centerPosition.y - transform.position.y);
                wanderDirection += centerDirection / 1000f;
            }
        }

        private void Fight()
        {
            if (!target)
            {
                state = StrangerState.Wander;
                return;
            }

            var hittable = target.GetComponent<IHittable>();
            if (hittable == null || !hittable.IsThreat())
            {
                state = StrangerState.Wander;
                return;
            }

            SetReadyToShoot(true);

            var targetPositionDiff = Mathf.Abs(target.position.y - transform.position.y);
            if (targetPositionDiff > K_positionEpsilon)
            {
                var targetPosition = target.position;
                var transformPosition = transform.position;
                moveDirection = GetFreeMoveDirection(new Vector2(
                    Mathf.Sign(targetPosition.x - transformPosition.x) * 0.1f,
                    Mathf.Sign(targetPosition.y - transformPosition.y)));
                humanController.Move(moveDirection);
                return;
            }

            humanController.Act();
        }

        private void Rob()
        {
            if (!target)
            {
                state = StrangerState.Wander;
                return;
            }

            var targetHuman = target.GetComponent<HumanController>();
            if (targetHuman == null || !DoesHumanHaveWoodILack(targetHuman))
            {
                state = StrangerState.Wander;
                return;
            }

            SetReadyToShoot(true);

            if (Mathf.Abs(target.position.y - transform.position.y) > 0.05f)
            {
                var targetPosition = target.position;
                var transformPosition = transform.position;
                moveDirection = GetFreeMoveDirection(new Vector2(
                    Mathf.Sign(targetPosition.x - transformPosition.x) * 0.1f,
                    Mathf.Sign(targetPosition.y - transformPosition.y)
                ));
                humanController.Move(moveDirection);
                return;
            }

            humanController.Act();
        }
        
        private void Surrender()
        {
            SetReadyToShoot(false);
            humanController.StopMovement();
            humanController.SetIsSurrendering(true);
        }

        private void Gather()
        {
            SetReadyToShoot(false);
            if (!target || !inventory.CanAddItem())
            {
                state = StrangerState.Wander;
                return;
            }

            Vector2 gatherDirection = target.position - transform.position;
            moveDirection = GetFreeMoveDirection(gatherDirection.SkewDirection(5));
            humanController.Move(moveDirection);
            if (humanController.CanPickUp(out var item))
            {
                humanController.PickUp(item);
            }
            else if (humanController.CanTakeItemFromContainer(ItemType.Wood, out var itemContainer))
            {
                if (itemContainer.GetItem(ItemType.Wood, out var woodItem))
                {
                    itemContainer.RemoveItem(woodItem);
                    inventory.AddItem(woodItem);
                    // TODO: Animation of the item getting taken?
                }
            }
        }

        private void Flee()
        {
            SetReadyToShoot(false);
            if (!target)
            {
                state = StrangerState.Wander;
                return;
            }
            Vector2 fleeDirection = transform.position - target.transform.position;
            moveDirection = GetFreeMoveDirection(fleeDirection.SkewDirection(10));
            humanController.Move(moveDirection);
        }

        private void SetReadyToShoot(bool ready)
        {
            var isThreat = humanController.IsThreat();
            if (ready && !isThreat)
            {
                // TODO: Check if the hand item is shootable instead
                if (humanController.Inventory.HandItem.Match(item => item.ItemType == ItemType.Pistol, false))
                {
                    humanController.SetIsAiming(true);
                    return;
                }

                // TODO: Check if the hand item is shootable instead
                var maybeGunItem =
                    humanController.Inventory.Items.FirstOrEmpty(item => item.ItemType == ItemType.Pistol);
                humanController.SetIsAiming(maybeGunItem.IsPresent);
                humanController.Inventory.SetHandItem(maybeGunItem);
                maybeGunItem.IfPresent(gunItem => humanController.Inventory.RemoveItem(gunItem));
                return;
            }

            if (!ready && isThreat)
            {
                humanController.SetIsAiming(false);
                humanController.Inventory.HandItem.IfPresent(handItem =>
                {
                    if (handItem.ItemType == ItemType.Pistol)
                    {
                        humanController.Inventory.AddItem(handItem);
                        humanController.Inventory.SetHandItem(Maybe.Empty<Item>());
                    }
                });
            }
        }

        private Vector3 GetFreeMoveDirection(Vector2 direction)
        {
            var moveObstacle = Physics2D.Raycast(transform.position, direction, freeMoveCheckDistance, solidLayerMask);
            var rotationAngle = 0f;
            while (moveObstacle.collider != null && rotationAngle < 360)
            {
                rotationAngle += 5f;
                moveObstacle = Physics2D.Raycast(transform.position,
                    Quaternion.Euler(0f, 0f, rotationAngle) * direction, freeMoveCheckDistance, solidLayerMask);
            }

            return Quaternion.Euler(0f, 0f, rotationAngle) * direction;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            var transformPosition = transform.position;
            Gizmos.DrawLine(transformPosition, transformPosition + moveDirection.normalized);
            if (behaviorTree != null && behaviorTree.State != null)
            {
                Handles.Label(transformPosition, behaviorTree.State.TargetAction.ToString());   
            }
        }
    }
}