using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LD48
{
    public class Bonfire : MonoBehaviour
    {
        public float burnTimePerWood = 20f;
        public GameObject fireParticlePrefab;
        
        private float timeToBurn = 0f;
        private bool isBurning;

        public bool IsBurning()
        {
            return isBurning;
        }

        public float GetTimeToBurn()
        {
            return timeToBurn;
        }
        
        void Start()
        {
            AddWood();
        }

        // Update is called once per frame
        void Update()
        {
            if (timeToBurn > 0)
            {
                isBurning = true;
                timeToBurn -= Time.deltaTime;
                Burn();
            }
            else
            {
                isBurning = false;
            }
        }
        public void AddWood()
       {
           timeToBurn += burnTimePerWood;
           isBurning = true;
       }

        private void Burn()
        {
            if (Random.value > (1 - 0.025 * timeToBurn))
            {
                var fireParticle = Instantiate(fireParticlePrefab, transform);
                fireParticle.transform.localPosition = new Vector2(Random.Range(-0.15f, 0.15f), 0.3f + Random.Range(-0.1f, 0.1f));
                fireParticle.GetComponent<Rigidbody2D>().velocity = new Vector2(0, Random.Range(0.1f, 2f));
                fireParticle.GetComponent<Temporary>().SetTimeToLive(0.1f + Random.Range(0f, 0.5f));
            }
        }
    }
}
