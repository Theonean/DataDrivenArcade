using System.Collections.Generic;
using UnityEngine;

public class LocalizedTextEntry
{
    // Key: language, Value: text
    public Dictionary<string, string> localizedTexts = new Dictionary<string, string>(); 

    public void AddLocalizedText(string language, string text)
    {
        localizedTexts[language] = text;
    }

    public string GetText(string language)
    {
        if (localizedTexts.ContainsKey(language))
        {
            return localizedTexts[language];
        }
        else
        {
            Debug.LogError($"Language {language} not found in localized text entry.");
            return string.Empty;
        }
    }

    public override string ToString()
    {
        string result = "";
        foreach (var localizedText in localizedTexts)
        {
            result += $"  {localizedText.Key}: {localizedText.Value}\n";
        }
        return result;
    }
}
