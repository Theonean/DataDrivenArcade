using UnityEngine;

namespace Localization
{
    [RequireComponent(typeof(TMPro.TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;
        private TMPro.TextMeshProUGUI textComponent;

        private void Awake()
        {
            textComponent = GetComponent<TMPro.TextMeshProUGUI>();
        }

        private void Start()
        {
            UpdateText();
        }

        public void SetKey(string newKey, bool UpdateNow = true)
        {
            key = newKey;
            if (UpdateNow)
                UpdateText();

        }

        public string GetValue()
        {
            return LocalizationManager.Instance.GetLocalizedText(key);
        }

        private void UpdateText()
        {
            textComponent.text = LocalizationManager.Instance.GetLocalizedText(key);
        }

        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += UpdateText;
        }

        private void OnDestroy()
        {
            LocalizationManager.OnLanguageChanged -= UpdateText;
        }
    }
}