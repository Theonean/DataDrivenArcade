using System.Collections.Generic;
using UnityEngine;


namespace Localization
{
    public class LocalizedData
    {
        // Key: id, Value: LocalizedTextEntry
        public Dictionary<string, LocalizedTextEntry> localizedTextEntries = new Dictionary<string, LocalizedTextEntry>();

        public void AddLocalizedTextEntry(string id, LocalizedTextEntry entry)
        {
            localizedTextEntries[id] = entry;
        }

        public string GetLocalizedTextForId(string id, string language)
        {
            if (localizedTextEntries.ContainsKey(id))
            {
                return localizedTextEntries[id].GetText(language);
            }
            else
            {
                Debug.LogError($"id {id} not found in localized data. For language {language}");
                return string.Empty;
            }
        }

        override public string ToString()
        {
            string result = "";
            foreach (var localizedTextEntry in localizedTextEntries)
            {
                result += $"{localizedTextEntry.Key}: {localizedTextEntry.Value}\n";
            }
            return result;
        }
    }
}