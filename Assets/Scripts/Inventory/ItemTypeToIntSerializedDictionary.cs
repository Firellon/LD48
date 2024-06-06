using System;
using LD48;
using Plugins.Sirenix.Odin_Inspector.Modules;

namespace Inventory
{
    [Serializable]
    public class ItemTypeToIntSerializedDictionary : UnitySerializedDictionary<ItemType, int>
    {
    }
}