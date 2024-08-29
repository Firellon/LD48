using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Journal
{
    [CreateAssetMenu(menuName = "LD48/Create JournalEntry SO", fileName = "JournalEntry", order = 0)]
    public class JournalEntry : ScriptableObject
    {
        [SerializeField] private string entryKey;
        [SerializeField] private int entryOrder;
        [FormerlySerializedAs("entryName")] [SerializeField] private string entryTitle;
        [TextArea(minLines: 5, maxLines: 20), SerializeField, InspectorTextArea] private string entryDescription;

        public string EntryKey => entryKey;
        public int EntryOrder => entryOrder;
       [ShowInInspector,ReadOnly] public string EntryShortName => $"No.{entryOrder}";
        public string EntryTitle => entryTitle;
        public string EntryDescription => entryDescription;
        public List<string> EntryDescriptions => Split(entryDescription);

        private List<string> Split(string text)
        {
            const int maxCharactersPerText = 200;
            const char sentenceSeparator = '.';
            
            var results = new List<string>();
            var segmentStartIndex = 0;
            var previousSeparatorIndex = 0;
            var currentCharIndex = 0;

            while (currentCharIndex < text.Length)
            {
                var currentChar = text[currentCharIndex];
                if (currentChar == sentenceSeparator)
                    previousSeparatorIndex = currentCharIndex+1;
                
                if (currentCharIndex - segmentStartIndex >= maxCharactersPerText)
                {
                    var textSegment = text.Substring(segmentStartIndex, previousSeparatorIndex - segmentStartIndex);
                    results.Add(textSegment);
                    segmentStartIndex = previousSeparatorIndex;
                }

                currentCharIndex++;
            }

            var lastSegmentLength = currentCharIndex - segmentStartIndex;
            if (lastSegmentLength > 0)
            {
                results.Add(text.Substring(segmentStartIndex, lastSegmentLength));
            }

            return results; 
        }

        public bool Equals(JournalEntry entry)
        {
            return entry.EntryKey == EntryKey;
        }
    }
}