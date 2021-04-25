using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LD48
{
    public enum StrangerState
    {
        Wander,
        Gather,
        SeekBonfire,
        StartBonfire,
        Fight,
        Flee,
    }
    
    public class Stranger: MonoBehaviour
    {
        // How many enemies can I handle at once?
        public int bravery = 3;
        // How far can I see threats?
        public float threatRadius = 10f;
        public float bonfireRadius = 10f;
        public float gatherRadius = 15f;
        
        private Human human;
        private DayNightCycle dayNightCycle;
        
        [SerializeField]
        private StrangerState state;
        private Transform target;

        public float baseTimeToWander = 3f;
        private float timeToWander = 0f;
        private Vector2 wanderDirection = Vector2.zero;


        private void Start()
        {
            human = GetComponent<Human>();
            dayNightCycle = Camera.main.GetComponent<DayNightCycle>();
        }
        
        private void Update()
        {
            if (human.IsDead()) return;
            // Reduce amount of expensive calls
            if (Random.value > 0.9f)
            {
                state = GetCurrentState();    
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
                    human.Fire();
                    state = GetCurrentState();
                    break;
                case StrangerState.Fight:
                    Fight();
                    break;
                case StrangerState.Flee:
                    Flee();
                    break;
                case StrangerState.Wander:
                    Wander();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private StrangerState GetCurrentState()
        {
            var closestThreats = Physics2D.OverlapCircleAll(transform.position, threatRadius, 1 << LayerMask.NameToLayer("Default"))
                .Select(collider => collider.gameObject)
                .Where(threat =>
                {
                    var hittable = threat.GetComponent<IHittable>();
                    return hittable != null && hittable.IsThreat();
                })
                .OrderBy(threat => Vector2.Distance(transform.position, threat.transform.position))
                .ToList();
            if (closestThreats.Any())
            {
                target = closestThreats.First().transform;
                return closestThreats.Count() > bravery ? StrangerState.Flee : StrangerState.Fight;
            }

            var currentCycle = dayNightCycle.GetCurrentCycle();
            if (currentCycle == DayTime.Evening || currentCycle == DayTime.Night)
            {
                
                var closestBonfires = Physics2D.OverlapCircleAll(transform.position, bonfireRadius, 1 << LayerMask.NameToLayer("Solid"))
                    .Select(collider => collider.gameObject.GetComponent<Bonfire>())
                    .Where(bonfire => bonfire != null && bonfire.IsBurning())
                    .OrderBy(bonfire => Vector2.Distance(transform.position, bonfire.transform.position))
                    .ToList();
                if (closestBonfires.Any())
                {
                    target = closestBonfires.First().transform;
                    return StrangerState.SeekBonfire;
                }

                return human.woodAmount > 0 ? StrangerState.StartBonfire : StrangerState.Wander;
            }
            
            if (human.woodAmount >= human.maxWoodAmount) return StrangerState.Wander;

            var closestWood = Physics2D.OverlapCircleAll(transform.position, gatherRadius, 1 << LayerMask.NameToLayer("Item"))
                .Select(collider => collider.gameObject.GetComponent<Item>())
                .Where(wood => wood != null && wood.type == ItemType.Wood)
                .OrderBy(wood => Vector2.Distance(transform.position, wood.transform.position))
                .ToList();

            if (!closestWood.Any()) return StrangerState.Wander;
            
            target = closestWood.First().transform;
            return StrangerState.Gather;

        }

        private void SeekBonfire()
        {
            SetReadyToShoot(false);
            
            var bonfireDistance = Vector2.Distance(target.transform.position, transform.position);
            if (bonfireDistance > human.fireTouchRadius)
            {
                human.Move(target.transform.position - transform.position);
                return;
            }

            if (human.woodAmount == 0) return;
            
            var bonfire = target.GetComponent<Bonfire>();
            if (Random.Range(1, 10) > bonfire.GetTimeToBurn())
            {
                human.Act();
            }
        }

        private void Wander()
        {
            SetReadyToShoot(false);
            
            if (timeToWander > 0f)
            {
                human.Move(wanderDirection);
                timeToWander -= Time.deltaTime;
            }
            else
            {
                timeToWander = baseTimeToWander;
                wanderDirection = Random.value > 0.9f 
                    ? new Vector2() 
                    : new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)).normalized;
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
            
            if (Mathf.Abs( target.position.y - transform.position.y) > 0.05f)
            {
                var targetPosition = target.position;
                var transformPosition = transform.position;
                human.Move(new Vector2(
                    Mathf.Sign(targetPosition.x - transformPosition.x) * 0.1f,
                    Mathf.Sign(targetPosition.y - transformPosition.y)
                    ));
                return;
            }

            human.Act();
        }

        private void Gather()
        {
            SetReadyToShoot(false);
            if (!target)
            {
                state = StrangerState.Wander;
                return;
            }
            human.Move(target.position - transform.position);
        }

        private void Flee()
        {
            SetReadyToShoot(false);
            human.Move(transform.position - target.transform.position);
        }

        private void SetReadyToShoot(bool ready)
        {
            var isThreat = human.IsThreat();
            if ((ready && !isThreat) || (!ready && isThreat))
            {
                human.SwitchReadyToShoot();
            }
        }
    }
}