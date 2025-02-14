using System.Collections.Generic;
using UnityEngine;

namespace Localization
{
    /// <summary>
    /// Configuration for supported languages in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "LanguageConfig", menuName = "Localization/Language Config")]
    public class LanguageConfig : ScriptableObject
    {
        [Tooltip("List of available languages for localization.")]
        public List<string> supportedLanguages = new List<string>();

        /// <summary>
        /// Checks if a given language is supported.
        /// </summary>
        public bool IsLanguageSupported(string language)
        {
            return supportedLanguages.Contains(language);
        }
    }
}