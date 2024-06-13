using UnityEngine;

namespace Player
{
    public class HumanState : MonoBehaviour
    {
        public const int K_MaxSanity = 100;
        
        public int Sanity { get; set; } = K_MaxSanity;
    }
}