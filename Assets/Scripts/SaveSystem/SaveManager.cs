using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager singleton;

        public SaveData[] playersData;

        public string[] saveFiles;

        public List<ISaveable> saveables;

        public SaveFileHandler[] saveFileHandlers;
        private bool[] playersInitialized;
        private bool activated = false;

        void Awake()
        {
            if (singleton == null)
            {
                singleton = this;
            }
            else if (singleton != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);

            //  BRIAN: if gm.instance.singlePlayer initialize arrays to 1 else 2
        }

        //This function is outdated, it used to be the case that players could play solo, I changed it so that in singleplayers they play against henryAI, which also has a savefile as any normal player
        public void SetPlayerCount(int playerCount)
        {
            saveFiles = new string[playerCount];
            playersData = new SaveData[playerCount];
            saveFileHandlers = new SaveFileHandler[playerCount];
            playersInitialized = new bool[playerCount];
        }

        public void Initiate(string fileName, int playerNum)
        {
            int playerNumIndex = playerNum - 1;
            saveFiles[playerNumIndex] = fileName;
            print("Player " + playerNum + " is using file " + saveFiles[playerNumIndex]);
            saveFileHandlers[playerNumIndex] = new SaveFileHandler(saveFiles[playerNumIndex]);
            playersInitialized[playerNumIndex] = true;

            // If all players are initialized, activate the save system
            if (playersInitialized.All(x => x))
            {
                activated = true;
            }
        }

        /// <summary>
        /// Call this function when a player enteres the scene where they select or setup the amount of players playing
        /// </summary>
        public void DeInitiate()
        {
            activated = false;
            playersInitialized = new bool[2] { false, false };
            playersData = new SaveData[2];
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            //SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            //SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (activated)
            {
                LoadData(1);
                LoadData(2);
            }
        }

        /*void OnSceneUnloaded(Scene current)
        {
            if (activated)
            {
                SaveData(1);
                SaveData(2);
            }
        }*/

        public void SaveData()
        {
            SaveData(1);
            
            SaveData(2);
        }

        //SaveData is Managed by the Gamamanger, I had problems with it not finding the saveable stuff so I just made it work whatever way I could 
        public void SaveData(int playerNum)
        {
            int playerNumIndex = playerNum - 1;

            saveables = GetSaveables();
            Debug.Log("P" + playerNum + " Saving Data: " + saveables.Count);
            foreach (ISaveable s in saveables)
            {
                playersData[playerNumIndex] = s.SaveData(playersData[playerNumIndex], playerNum);
            }

            saveFileHandlers[playerNumIndex].Save(playersData[playerNumIndex]);
        }

        public void LoadData(int playerNum)
        {
            int playerNumIndex = playerNum - 1;
            playersData[playerNumIndex] = saveFileHandlers[playerNumIndex].Load();
            if (playersData[playerNumIndex] == null)
            {
                playersData[playerNumIndex] = new SaveData();
                playersData[playerNumIndex].playerName = saveFiles[playerNumIndex];
                SaveData(playerNum);
            }
            else
            {
                List<ISaveable> saveables = GetSaveables();
                Debug.Log("P" + playerNum + "Loading Data: " + saveables.Count);
                foreach (ISaveable s in saveables)
                {
                    s.LoadData(playersData[playerNumIndex], playerNum);
                }
            }
        }

        List<ISaveable> GetSaveables()
        {
            IEnumerable<ISaveable> saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
            return new List<ISaveable>(saveables);
        }

    }
}


