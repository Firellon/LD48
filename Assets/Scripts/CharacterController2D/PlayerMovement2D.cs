using Sirenix.OdinInspector;
using UnityEngine;

namespace LD48.CharacterController2D
{
    public class PlayerMovement2D : MonoBehaviour
    {
        //I recommend 7 for the move speed, and 1.2 for the force damping
        public Rigidbody2D rb;
        [ShowInInspector, ReadOnly] public float forceDamping { get; set; } = 1.2f;
        [ShowInInspector, ReadOnly] public float MoveSpeed { get; set; } = 5f;

        [ShowInInspector, ReadOnly] public bool IsMoving { get; private set; }

        [ShowInInspector, ReadOnly] private Vector2 playerInput;

        private Vector2 forceToApply;

        public void Move(Vector2 direction)
        {
            playerInput = direction.normalized;
        }

        public void AddForce(Vector2 force)
        {
            forceToApply += force;
        }

        private void FixedUpdate()
        {
            var moveForce = playerInput * MoveSpeed;
            moveForce += forceToApply;

            forceToApply /= forceDamping;

            if (Mathf.Abs(forceToApply.x) <= float.Epsilon && Mathf.Abs(forceToApply.y) <= float.Epsilon)
            {
                forceToApply = Vector2.zero;
            }

            rb.velocity = moveForce;
            IsMoving = moveForce.magnitude >= 0.001f;
        }
    }
}