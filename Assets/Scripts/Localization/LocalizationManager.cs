using UnityEngine;
using System;

namespace Localization
{
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [SerializeField] private LanguageConfig languageConfig;  // Reference to LanguageConfig ScriptableObject
        [SerializeField] private LocalizedData localizationData;

        public static event Action OnLanguageChanged;
        private string currentLanguage = "English";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            LoadLocalizationData();
            LoadSavedLanguage();
        }

        private void LoadLocalizationData()
        {
            // Load CSV or JSON-based localization data into memory
            localizationData = LocalizationImporter.LoadLocalizationData();
        }

        private void LoadSavedLanguage()
        {
            // Load last used language or fallback to default
            string savedLanguage = PlayerPrefs.GetString("SelectedLanguage", "English");
            if (!languageConfig.IsLanguageSupported(savedLanguage))
            {
                savedLanguage = "English";  // Fallback to default
            }

            SetLanguage(savedLanguage);
        }

        public void SetLanguage(string newLanguage)
        {
            if (languageConfig.IsLanguageSupported(newLanguage))
            {
                currentLanguage = newLanguage;
                PlayerPrefs.SetString("SelectedLanguage", newLanguage);
                OnLanguageChanged?.Invoke();
            }
            else
            {
                Debug.LogWarning($"Language {newLanguage} is not supported!");
            }
        }

        public string GetLocalizedText(string localizationID)
        {
            if (localizationData.GetLocalizedTextForId(localizationID, currentLanguage) is string text && !string.IsNullOrEmpty(text))
            {
                return text;
            }

            Debug.LogWarning($"Missing translation for {localizationID} in {currentLanguage}");
            return localizationData.localizedTextEntries.ContainsKey(localizationID) ? localizationData.GetLocalizedTextForId(localizationID, "English") : "[Missing Translation]";
        }
    }
}