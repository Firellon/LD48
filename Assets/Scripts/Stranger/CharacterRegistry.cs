using System.Collections.Generic;
using UnityEngine;
using Utilities.Monads;

namespace Stranger
{
    public class CharacterRegistry : MonoBehaviour, ICharacterRegistry
    {
        [SerializeField] private Character playerCharacter;
        [SerializeField] private List<Character> characters = new();

        public IMaybe<Character> PlayerCharacter => playerCharacter.ToMaybe();
    }
}

    