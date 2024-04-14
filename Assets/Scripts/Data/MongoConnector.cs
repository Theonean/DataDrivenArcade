using System;
using UnityEngine;
using MongoDB.Driver;
using MongoDB.Bson;

public class MongoConnector : MonoBehaviour
{
    private void Start()
    {
        string uri = "mongodb://mongoTest_centralyet:0353ddff02f663ab8cd13921505ded30d3c64a94@ab9.h.filess.io:27018/mongoTest_centralyet";

        MongoClient dbClient = new MongoClient(uri);

        try
        {
            var database = dbClient.GetDatabase("mongoTest_centralyet"); // The database name where your 'data' collection exists.
            var collection = database.GetCollection<BsonDocument>("data"); // Assuming 'data' is the name of your collection.

            var documents = collection.Find(new BsonDocument()).ToList(); // Retrieves all documents from the 'data' collection.

            foreach (var document in documents)
            {
                Debug.Log(document.ToJson()); // Prints each document in the Unity Console.
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error accessing MongoDB: " + ex.Message);
        }

        Debug.Log("Done.");
    }
}
