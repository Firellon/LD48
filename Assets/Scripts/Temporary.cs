using UnityEngine;

namespace LD48
{
    public class Temporary : MonoBehaviour
    {
        public float timeToLive = 1f;

        public void SetTimeToLive(float newTimeToLive)
        {
            timeToLive = newTimeToLive;
        }
        void Update()
        {
            if (timeToLive > 0)
            {
                timeToLive -= Time.deltaTime;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}