using TMPro;
using UnityEngine;

namespace Localization
{
    public class LanguageDropdown : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private LanguageConfig languageConfig;

        private void Start()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(languageConfig.supportedLanguages);
            dropdown.onValueChanged.AddListener(OnLanguageSelected);
        }

        private void OnLanguageSelected(int index)
        {
            LocalizationManager.Instance.SetLanguage(languageConfig.supportedLanguages[index]);
        }
    }
}