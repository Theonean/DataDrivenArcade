using System.IO;
using UnityEngine;

namespace SaveSystem
{
    public class SaveFileHandler
    {
        string filePath;
        string fileName;

        public SaveFileHandler(string fileName)
        {
            this.fileName = fileName;
            filePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        public void Save(SaveData saveData)
        {
            // Save your saveData object to a json file.
            string data = JsonUtility.ToJson(saveData, true); // 'true' makes it look pretty
            File.WriteAllText(filePath, data);
            Debug.Log("Game Saved to: " + filePath);
        }

        public SaveData Load()
        {
            if (File.Exists(filePath))
            {
                // read the object data from file
                string retrievedData = File.ReadAllText(filePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(retrievedData);

                //Output the number of save files in the filepath to the console and their names
                //TODO Finish This Debug
                string[] saveFiles = Directory.GetFiles(Path.GetDirectoryName(filePath), "*.txt");
                //Debug.Log("Number of save files: " + saveFiles.Length + " at: " + filePath);
                foreach (string saveFile in saveFiles)
                {
                    //Debug.Log("Save file name: " + Path.GetFileName(saveFile));
                }

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

