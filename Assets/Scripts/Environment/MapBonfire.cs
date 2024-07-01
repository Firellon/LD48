using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Prefabs;
using Zenject;

namespace Environment
{
    public class MapBonfire : MonoBehaviour
    {
        [Inject] private IPrefabPool prefabPool;

        public float burnTimePerWood = 20f;
        [FormerlySerializedAs("audio")] public AudioSource fireSound;
        [SerializeField] private List<GameObject> visualEffects = new();
        [SerializeField] private ParticleSystem fireEffect;

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
            timeToBurn += burnTimePerWood;
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
                SetFireStrength();
                Burn();
            }
            else
            {
                isBurning = false;
                SetFireStrength();
                fireSound.Stop();
            }
        }

        public void AddWood()
        {
            timeToBurn += burnTimePerWood;
            Debug.Log($"AddWood > timeToBurn {timeToBurn}");
            isBurning = true;
        }

        private void Burn()
        {
            if (Random.value > (1 - 0.025 * timeToBurn))
            {
                fireSound.Play();
            }
        }

        private void SetFireStrength()
        {
            visualEffects.ForEach(effect => effect.SetActive(isBurning));
            var fireEmission = fireEffect.emission;
            fireEmission.rateOverTime = timeToBurn * 4;
        }
    }
}