using System.Collections.Generic;
using Inventory;
using LD48;
using Map;
using Signals;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Monads;

namespace Environment
{
    public class MapBonfire : MonoBehaviour, IInteractable
    {
        [SerializeField] private SpriteRenderer spriteRenderer; // TODO: Inject
        [SerializeField] private MapObjectController mapObjectController; // TODO: Inject
        [SerializeField] private Material regularShader;
        [SerializeField] private Material highlightShader;
        
        [Space]
        [SerializeField] private float burnTimePerWood = 20f;
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
        
        public void AddBurnableItem(Item burnableItem)
        {
            AddWood();
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

        #region IInteractable

        public bool CanBePickedUp => false;
        public bool IsItemContainer => false;
        public IMaybe<Item> MaybeItem => Maybe.Empty<Item>();
        public IMaybe<MapObject> MaybeMapObject => mapObjectController.MapObject.ToMaybe();
        public GameObject GameObject => gameObject;
        public void SetHighlight(bool isLit)
        {
            spriteRenderer.material = isLit ? highlightShader : regularShader;
        }

        public void Remove()
        {
            SignalsHub.DispatchAsync(new MapObjectRemovedEvent(GameObject, mapObjectController.MapObject.ObjectType));
        }
        
        #endregion
    }
}