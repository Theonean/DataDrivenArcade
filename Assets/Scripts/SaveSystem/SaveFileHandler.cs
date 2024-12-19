using System.IO;
using UnityEngine;

namespace SaveSystem
{
    public enum DatabaseType
    {
        Local
    }
    public class SaveFileHandler
    {
        string filePath;
        string fileName;
        DatabaseType databaseType;

        public SaveFileHandler(string fileName)
        {
            this.fileName = fileName + ".txt";

            databaseType = DatabaseType.Local;

            filePath = Path.Combine(Application.persistentDataPath, this.fileName);
        }

        public void Save(SaveData saveData)
        {
            string data = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(filePath, data);
            Debug.Log("Game Saved to: " + filePath);
        }


        public SaveData Load()
        {
            if (File.Exists(filePath) && databaseType == DatabaseType.Local)
            {
                string retrievedData = File.ReadAllText(filePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(retrievedData);
                Debug.Log("Save Data Loaded for player: " + fileName + " with content: " + saveData.ToString());
                return saveData;
            }
            else
            {
                Debug.LogWarning("No save file named '" + filePath + "' found");
                return null;
            }
        }

    }
}

