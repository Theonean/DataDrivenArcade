using System;
using System.IO;
using UnityEngine;

namespace Localization
{
    public class LocalizationImporter
    {
        private static string filePath;
        private static string missingEntries = "";

        public static LocalizedData LoadLocalizationData()
        {
            LocalizedData localizationData = new();

            // Define file path in StreamingAssets
            filePath = Path.Combine(Application.streamingAssetsPath, "Localization.csv");

            // Ensure file exists
            if (!File.Exists(filePath))
            {
                Debug.LogError("Localization file not found: " + filePath);
                return localizationData;
            }

            // Read all lines from CSV file
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length < 2)  // Ensure file has at least headers + one row
                return localizationData;

            // Read header for language keys
            string[] headers = lines[0].Split(',');

            // Read the rest of the CSV file
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split(',');

                if (columns.Length < headers.Length)
                    missingEntries += $"Missing entries for key {columns[0]}. {columns.Length}/{headers.Length} entries." + Environment.NewLine;

                string id = columns[0]; // First column is the localization ID
                LocalizedTextEntry entry = new();

                // Loop through language columns
                for (int j = 1; j < headers.Length; j++)
                {
                    string language = headers[j]; // "English", "French", etc.
                    string text = columns.Length > j ? columns[j] : string.Empty; // read text from column or empty string

                    entry.AddLocalizedText(language, text);
                }

                localizationData.AddLocalizedTextEntry(id, entry);
            }

            Debug.Log($"Localization data loaded successfully. \n {localizationData.ToString()} \n Missing Entries: \n {missingEntries}");

            return localizationData;
        }
    }
}