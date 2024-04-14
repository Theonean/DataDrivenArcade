using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystem {
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager singleton;

        public SaveData saveData;

        public string saveFile = "gamesave2.txt";

        public List<ISaveable> saveables;

        public SaveFileHandler saveFileHandler;

        void Awake() {
            if (singleton == null) {
                singleton = this;
            } else if (singleton!= this) {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);

            // Save the data to a file
            saveFileHandler = new SaveFileHandler(saveFile);
        }

        void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            LoadData();          
        }

        void OnSceneUnloaded(Scene current) {
            SaveData();
        }

        public void SaveData() {
            saveables = GetSaveables();
            Debug.Log("Saving Data: " + saveables.Count);
            foreach (ISaveable s in saveables) {
                s.SaveData(saveData);
            }
            if (saveables.Count > 0)
                saveFileHandler.Save(saveData);
                
        }

        public void LoadData() {
            saveData = saveFileHandler.Load();
            if (saveData == null) {
                saveData = new SaveData();
                SaveData();
            }
            else {
                List<ISaveable> saveables = GetSaveables();
                Debug.Log("Loading Data: " + saveables.Count);            
                foreach (ISaveable s in saveables) {
                    s.LoadData(saveData);
                }                
            }
        }

        List<ISaveable> GetSaveables() {
            IEnumerable<ISaveable> saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
            return new List<ISaveable>(saveables);
        }

    }
}


