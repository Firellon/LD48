using System;
using System.Linq;
using Day;
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
        Rob,
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
        private TerrainGenerator terrainGenerator;
        
        [SerializeField]
        private StrangerState state;
        [SerializeField]
        private Transform target;

        public float baseTimeToWander = 3f;
        private float timeToWander = 0f;
        private Vector2 wanderDirection = Vector2.zero;
        public int minWoodToSurvive = 3;


        private void Start()
        {
            human = GetComponent<Human>();
            dayNightCycle = Camera.main.GetComponent<DayNightCycle>();
            terrainGenerator = Camera.main.GetComponent<TerrainGenerator>();

            human.woodAmount = Random.Range(1, 4);
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
                    human.LightAFire();
                    state = GetCurrentState();
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
                    if (threat.gameObject == gameObject) return false;
                    var hittable = threat.GetComponent<IHittable>();
                    if (hittable == null) return false;
                    var otherHuman = threat.GetComponent<Human>();
                    if (otherHuman == null && hittable.IsThreat()) return true;
                    if (otherHuman == null) return false;
                    var humanVerticalDistance = Mathf.Abs(otherHuman.transform.position.y - transform.position.y);
                    var isHumanCloseAndPointingAtMe = otherHuman.IsThreat() && otherHuman.IsFacingTowards(transform.position) && humanVerticalDistance < threatRadius / 3;
                    // if (isHumanCloseAndPointingAtMe) Debug.Log($"Other Human {otherHuman.name} is trying to attack me!");

                    return isHumanCloseAndPointingAtMe;
                })
                .OrderBy(threat => Vector2.Distance(transform.position, threat.transform.position))
                .ToList();
            if (closestThreats.Any())
            {
                target = closestThreats.First().transform;
                // if (closestThreats.Count() > bravery) Debug.Log($"Too dangerous, I need to Flee! ({closestThreats.Count()})");
                return closestThreats.Count() > bravery ? StrangerState.Flee : StrangerState.Fight;
            }

            if (human.woodAmount < minWoodToSurvive)
            {
                var closestPeopleToRob = Physics2D.OverlapCircleAll(transform.position, threatRadius, 1 << LayerMask.NameToLayer("Default"))
                    .Select(collider => collider.gameObject)
                    .Where(other =>
                    {
                        if (other.gameObject == gameObject) return false;
                        var otherHuman = other.GetComponent<Human>();
                        if (otherHuman == null) return false;

                        return DoesHumanHaveWoodILack(otherHuman);
                    })
                    .OrderBy(threat => Vector2.Distance(transform.position, threat.transform.position))
                    .ToList();

                if (closestPeopleToRob.Any())
                {
                    target = closestPeopleToRob.First().transform;
                    return StrangerState.Rob;
                }
            }


            var currentCycle = dayNightCycle.GetCurrentCycle();
            if (currentCycle == DayTime.NightComing || currentCycle == DayTime.Night)
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

        private bool DoesHumanHaveWoodILack(Human otherHuman)
        {
            return human.woodAmount < minWoodToSurvive && otherHuman.woodAmount > human.woodAmount + 2;
        }

        private void SeekBonfire()
        {
            SetReadyToShoot(false);
            
            var bonfireDistance = Vector2.Distance(target.transform.position, transform.position);
            if (bonfireDistance > human.fireTouchRadius)
            {
                Vector2 bonfireDirection = target.transform.position - transform.position; 
                human.Move(bonfireDirection.SkewDirection(5));
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

        private void Rob()
        {
            if (!target)
            {
                state = StrangerState.Wander;
                return;
            }

            var targetHuman = target.GetComponent<Human>(); 
            if (targetHuman == null || !DoesHumanHaveWoodILack(targetHuman))
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

            Vector2 gatherDirection = target.position - transform.position; 
            human.Move(gatherDirection.SkewDirection(5));
        }

        private void Flee()
        {
            SetReadyToShoot(false);
            Vector2 fleeDirection = transform.position - target.transform.position;
            human.Move(fleeDirection.SkewDirection(10));
        }

        private void SetReadyToShoot(bool ready)
        {
            var isThreat = human.IsThreat();
            if ((ready && !isThreat) || (!ready && isThreat))
            {
                human.ToggleIsAiming();
            }
        }
    }
}