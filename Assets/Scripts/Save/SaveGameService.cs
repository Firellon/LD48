using System;
using System.Collections.Generic;
using UnityEngine;

namespace LD48
{
    public class SaveGameService : ISaveGameService
    {
        public void Save(SaveGame saveGame, string name)
        {
            throw new NotImplementedException();
        }

        public bool TryLoad(string saveGameName, out SaveGame savegame)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, SaveGame> List()
        {
            throw new NotImplementedException();
        }
    }
}