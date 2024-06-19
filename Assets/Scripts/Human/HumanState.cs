using UnityEngine;

namespace Human
{
    public class HumanState : MonoBehaviour
    {
        public const int K_MaxSanity = 100;
        public const int K_MinSanity = 0;

        private int sanity = (K_MaxSanity - K_MinSanity) / 2;

        public int Sanity
        {
            get => sanity;
            set => sanity = Mathf.Clamp(value, K_MinSanity, K_MaxSanity);
        }
        public int MinSanity => K_MinSanity;
        public int MaxSanity => K_MaxSanity;

        public bool IsDead { get; private set; } = false;

        public void Die()
        {
            if (IsDead) return;

            IsDead = true;
        }
    }
}