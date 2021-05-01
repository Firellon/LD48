using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LD48
{

    public enum GhostState
    {
        Idle,
        Attack,
        Flee
    }
    public class Ghost : MonoBehaviour, IHittable
    {
        private new SpriteRenderer renderer;
        private Rigidbody2D body;
        private AudioSource audio;
        
        public Sprite walkSprite;
        public Sprite attackSprite;
        public Sprite deadSprite;

        public AudioClip spawnSound;
        public AudioClip attackSound;

        public float walkSpeed = 4f;

        public float baseTimeToReload = 1f;
        private float timeToReload = 0f;
        public float attackDistance = 0.5f;

        public float baseTimeToDie = 10f;
        private float timeToDie = 0f;
        private bool isDead = false;

        public float detectionRadius = 10f;
        private GhostState state = GhostState.Idle;
        private Transform target;

        private float baseTimeToIdle = 2f;
        private float timeToIdle = 0f;
        private Vector2 idleDirection;

        public bool IsDead()
        {
            return isDead;
        }

        public bool IsThreat()
        {
            return !isDead && state != GhostState.Flee;
        }

        private void Start()
        {
            renderer = GetComponent<SpriteRenderer>();
            body = GetComponent<Rigidbody2D>();
            audio = GetComponent<AudioSource>();
            audio.PlayOneShot(spawnSound);
        }

        private void Update()
        {
            if (isDead)
            {
                if (timeToDie > 0f)
                {
                    timeToDie -= Time.deltaTime;
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
            }
            // Reduce amount of expensive calls
            if (Random.value > 0.9f)
            {
                state = GetCurrentState();    
            }

            switch (state)
            {
                case GhostState.Idle:
                    Idle();
                    break;
                case GhostState.Attack:
                    Attack();
                    break;
                case GhostState.Flee:
                    Flee();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Idle()
        {
            if (timeToIdle > 0f)
            {
                Move(idleDirection);
                timeToIdle -= Time.deltaTime;
            }
            else
            {
                timeToIdle = baseTimeToIdle;
                if (Random.value > 0.75f)
                {
                    idleDirection = new Vector2();
                }
                else
                {
                    idleDirection = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)).normalized;
                }
            }
        }

        private void Move(Vector2 direction)
        {
            body.velocity = direction * walkSpeed;
            if (direction.x != 0)
            {
                renderer.flipX = direction.x < 0;    
            }
        }

        private void Attack()
        {
            var targetDistance = Vector2.Distance(transform.position, target.position);
            if (targetDistance < attackDistance)
            {
                if (timeToReload > 0f)
                {
                    timeToReload -= Time.deltaTime;
                }
                else
                {
                    var human = target.gameObject.GetComponent<Human>();
                    if (human == null)
                    {
                        Debug.LogError("Ghost > Attack > no Human component found in target!");
                    }
                    human.Hit();
                    audio.PlayOneShot(attackSound);
                    renderer.sprite = attackSprite;
                    timeToReload = baseTimeToReload;
                }
            }
            else
            {
                renderer.sprite = walkSprite;
                var targetDirection = (target.position - transform.position).normalized;
                Move(targetDirection);
            }
        }

        private void Flee()
        {
            renderer.sprite = walkSprite;
            var targetDirection = (transform.position - target.position).normalized;
            Move(targetDirection);
        }

        private GhostState GetCurrentState()
        {
            var closestBonfires = Physics2D.OverlapCircleAll(transform.position, detectionRadius, 1 << LayerMask.NameToLayer("Solid"))
                .Select(collider => collider.gameObject.GetComponent<Bonfire>())
                .Where(bonfire => bonfire != null && bonfire.IsBurning())
                .OrderBy(bonfire => Vector2.Distance(transform.position, bonfire.transform.position))
                .ToList();
            if (closestBonfires.Any())
            {
                target = closestBonfires.First().transform;
                return GhostState.Flee;
            }
            
            var closestHumans = Physics2D.OverlapCircleAll(transform.position, detectionRadius, 1 << LayerMask.NameToLayer("Default"))
                .Select(collider => collider.gameObject.GetComponent<Human>())
                .Where(human => human != null && !human.IsDead())
                .OrderBy(human => Vector2.Distance(transform.position, human.transform.position))
                .ToList();
            if (closestHumans.Any())
            {
                target = closestHumans.First().transform;
                return GhostState.Attack;
            }

            return GhostState.Idle;
        }

        public void Hit()
        {
            isDead = true;
            renderer.sprite = deadSprite;
            timeToDie = baseTimeToDie;
            body.velocity = Vector2.zero;
            GetComponent<Collider2D>().enabled = false;
        }
    }
}