using Utilities.Monads;

namespace Stranger
{
    public interface ICharacterRegistry
    {
        IMaybe<Character> PlayerCharacter { get; }
    }
}