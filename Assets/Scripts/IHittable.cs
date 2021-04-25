using UnityEngine;

namespace LD48
{
    public interface IHittable
    {
        public void Hit();
        public bool IsDead();
        public bool IsThreat();
    }
}