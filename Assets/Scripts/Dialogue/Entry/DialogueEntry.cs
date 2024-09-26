using System.Collections.Generic;
using UnityEngine;

namespace Dialogue.Entry
{
    [CreateAssetMenu(menuName = "LD48/Create DialogueEntry SO", fileName = "New Dialogue Entry", order = 0)]
    public class DialogueEntry : ScriptableObject, IDialogueEntry
    {
        [SerializeField] private string entryKey = string.Empty;

        [SerializeField] private List<DialogueEntryReplica> replicas = new();

        public string EntryKey => entryKey;
        public List<DialogueEntryReplica> Replicas => replicas;
    }
}