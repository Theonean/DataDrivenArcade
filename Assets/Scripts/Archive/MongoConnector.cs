using MongoDB.Driver;
using UnityEngine;

public class MongoConnector : MonoBehaviour
{
    // Example of a serialized field that should not be conditionally compiled
    public bool uploadLocalFiles = false;
    public static bool isOnline = true;

    private static string uri = "...";
    private static MongoClient dbClient;

    private void Awake()
    {
        InitializeDBClient();
    }

    private void InitializeDBClient()
    {
        // This ensures that dbClient is always initialized in the same way regardless of the platform
        if (dbClient == null)
        {
            dbClient = new MongoClient(uri);
        }
    }

    // Ensure no serialized fields are within conditional directives
}
