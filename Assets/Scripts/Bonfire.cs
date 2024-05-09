using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Prefabs;
using Zenject;

namespace LD48
{
    public class Bonfire : MonoBehaviour
    {
        [Inject] private IPrefabPool prefabPool;
        
        public float burnTimePerWood = 20f;
        public GameObject fireParticlePrefab;
        [FormerlySerializedAs("audio")] public AudioSource fireSound;
        
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
            timeToBurn += burnTimePerWood / 2;
            isBurning = true;
            fireSound.Play();
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
                fireSound.Stop();
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
                var fireParticle = prefabPool.Spawn(fireParticlePrefab, transform);
                fireParticle.transform.localPosition = new Vector2(Random.Range(-0.15f, 0.15f), 0.3f + Random.Range(-0.1f, 0.1f));
                fireParticle.GetComponent<Rigidbody2D>().velocity = new Vector2(0, Random.Range(0.1f, 2f));
                fireParticle.GetComponent<Temporary>().SetTimeToLive(0.1f + Random.Range(0f, 0.5f));
                fireSound.Play();
            }
        }
    }
}
