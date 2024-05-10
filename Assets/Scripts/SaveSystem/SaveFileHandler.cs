using System;
using System.IO;
using MongoDB.Driver;
using MongoDB.Bson;
using UnityEngine;

namespace SaveSystem
{
    public enum DatabaseType
    {
        Local,
        MongoDB
    }
    public class SaveFileHandler
    {
        string filePath;
        string fileName;
        DatabaseType databaseType;
        string uri = "mongodb://ShapeShifters_poetrywar:56002aab95ab3ac337400c7136750b4a83c2962c@4uz.h.filess.io:27018/ShapeShifters_poetrywar";
        MongoClient dbClient;
        IMongoDatabase db;
        string collectionName = "PlayersData";

        public SaveFileHandler(string fileName)
        {
            this.fileName = fileName;
            filePath = Path.Combine(Application.persistentDataPath, fileName);

            databaseType = GameManager.instance.arcadeMode ? DatabaseType.Local : DatabaseType.MongoDB;

            try
            {
                if (databaseType == DatabaseType.MongoDB)
                {
                    Debug.Log("Connecting to MongoDB");
                    dbClient = new MongoClient(uri);
                    db = dbClient.GetDatabase("ShapeShifters_poetrywar"); // Assuming 'Players' is the database name.
                }
                else
                    fileName += ".txt";
            }
            catch (Exception ex)
            {
                Debug.LogError("Error accessing MongoDB: " + ex.Message + "\nSwitching to Local Database");
                databaseType = DatabaseType.Local;
            }
        }

        public void Save(SaveData saveData)
        {
            if (databaseType == DatabaseType.MongoDB)
            {
                Debug.Log("Saving to MongoDB");
                var collection = db.GetCollection<BsonDocument>(collectionName); // Ensure this is the correct collection name

                // Convert SaveData to a BsonDocument
                var document = saveData.ToBsonDocument();

                // Define a filter to check if the document already exists
                var filter = Builders<BsonDocument>.Filter.Eq("playerName", saveData.playerName); // Replace 'PlayerName' with the actual unique field

                // Define an update operation (set each field, or replace the entire document)
                var update = new BsonDocument("$set", document);

                // Upsert option
                var options = new UpdateOptions { IsUpsert = true };

                // Perform the update or insert operation
                collection.UpdateOne(filter, update, options);

                Debug.Log("Data saved or updated in MongoDB");
            }
            else if (databaseType == DatabaseType.Local)
            {
                string data = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(filePath, data);
                Debug.Log("Game Saved to: " + filePath);
            }
        }



        public SaveData Load()
        {
            if (databaseType == DatabaseType.MongoDB)
            {
                Debug.Log("Loading from MongoDB");
                var collection = db.GetCollection<BsonDocument>(collectionName);

                var filter = Builders<BsonDocument>.Filter.Eq("playerName", fileName); // Adjust based on your SaveData structure.
                var document = collection.Find(filter).FirstOrDefault();
                if (document != null)
                {
                    document.Remove("_id"); // Remove the _id field before converting to JSON
                    var json = document.ToJson();
                    Debug.Log("loading Json for player: " + fileName + " with content: " + json);

                    SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                    Debug.Log("Data loaded from MongoDB for player: " + fileName);
                    return saveData;
                }
                else
                {
                    Debug.LogWarning("No data found for player: " + fileName);
                    return null;
                }
            }
            else if (File.Exists(filePath) && databaseType == DatabaseType.Local)
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

