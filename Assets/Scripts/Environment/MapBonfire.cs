using System.Collections.Generic;
using Dialogue;
using Dialogue.Entry;
using Human;
using Inventory;
using LD48;
using Map;
using Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Utilities.Monads;
using Zenject;

namespace Environment
{
    public class MapBonfire : MonoBehaviour, IInteractable, IClickDialogueTarget
    {
        [SerializeField] private float burnTimePerWood = 20f;
        [FormerlySerializedAs("audio")] public AudioSource fireSound;
        [SerializeField] private List<GameObject> visualEffects = new();
        [SerializeField] private ParticleSystem fireEffect;
        
        [Inject] private VisualsConfig visualsConfig;
        [Inject] private SpriteRenderer spriteRenderer; 
        [Inject] private MapObjectController mapObjectController;

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
                UpdateDialogueEntry();
            }
            else
            {
                isBurning = false;
                SetFireStrength();
                fireSound.Stop();
                UpdateDialogueEntry();
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
        public IMaybe<Item> MaybeItem => Maybe.Empty<Item>();
        public IMaybe<MapObject> MaybeMapObject => mapObjectController.MapObject.ToMaybe();
        public GameObject GameObject => gameObject;
        public void SetHighlight(bool isLit)
        {
            spriteRenderer.material = isLit ? visualsConfig.HighlightedInteractableShader : visualsConfig.RegularInteractableShader;
        }

        public bool CanInteract()
        {
            return false; // TODO: See if we should keep this way
        }

        public void Interact(HumanController humanController)
        {
            // TODO: Add a burnable item to the fire if possible
        }

        public void Remove()
        {
            SignalsHub.DispatchAsync(new MapObjectRemovedEvent(GameObject, mapObjectController.MapObject.ObjectType));
        }
        
        #endregion

        #region Dialogue
        
        private void UpdateDialogueEntry()
        {
            DialogueEntry = new SerializedDialogueEntry
            {
                EntryDescription = isBurning
                    ? timeToBurn < burnTimePerWood
                        ? "Looks like the fire's dying down. Won’t last much longer."
                        : "This fire's putting on quite the show!"
                    : "Only the embers are left here."
            };
        }

        public IDialogueEntry DialogueEntry { get; set; } = new SerializedDialogueEntry();
        public void OnPointerClick(PointerEventData eventData)
        {
            SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(DialogueEntry));
        }
        
        #endregion
    }
}