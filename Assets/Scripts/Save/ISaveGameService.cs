using System.Collections.Generic;

namespace LD48
{
    public interface ISaveGameService
    {
        public void Save(SaveGame saveGame, string name);
        public bool TryLoad(string saveGameName, out SaveGame savegame);
        public Dictionary<string, SaveGame> List();
    }
}