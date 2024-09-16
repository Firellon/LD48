using UnityEngine;

namespace Stranger
{
    [CreateAssetMenu(menuName = "LD48/Create Character SO", fileName = "New Character", order = 0)]
    public class Character : ScriptableObject
    {
        [SerializeField] private string characterName;
        [SerializeField] private Sprite portrait;

        public string CharacterName => characterName;
        public Sprite Portrait => portrait;
    }
}