using Google.Cloud.Firestore;
using System.Text.Json;

namespace firebase_restore
{
    static class Backup
    {
        private static FirestoreDb db;
        public static async void Start()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Enter the path to your service account key JSON file, then press enter: ");
            string? path = Console.ReadLine();

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            await BackupData(path);
        }

        private async static Task BackupData(string path)
        {
            db = FirestoreDb.Create("availability-monitor-7231f");

            // Getting all collections ID.
            var collections = db.ListRootCollectionsAsync();

            List<DataCollection> data = new List<DataCollection>();

            // Adding root collections data.            
            await foreach(var coll in collections)
            {
                data.Add(new DataCollection(coll.Id, coll.Id));
            }

            // Adding documents for each collection.
            foreach(DataCollection collection in data)
            {
                collection.Documents = await GetChildData(collection);                
            }

            // Saving data to a file in JSON format.
            string jsonData = JsonSerializer.Serialize(data);

            File.WriteAllText("firestore_backup_" + DateTime.Now.ToShortDateString() + ".json", jsonData);
        }

        // Gets child documents with their subcollections and fields with values.
        private static async Task<List<DataDocument>> GetChildData(DataCollection collection)
        {
            QuerySnapshot snapshot = await db.Collection(collection.Path).GetSnapshotAsync();
            
            List<DataDocument> documents = new List<DataDocument>();

            foreach (DocumentSnapshot documentSnapshot in snapshot.Documents)
            {
                // Converting document to a dictionary.
                Dictionary<string, object> document = documentSnapshot.ToDictionary();

                DataDocument documentData = new DataDocument(documentSnapshot.Id, collection.Path + "/" + documentSnapshot.Id);
                
                // Adding each field with value.
                foreach (var key in document.Keys)
                {
                    documentData.Fields.Add(key, document[key]);
                }

                IAsyncEnumerable<CollectionReference> subcollectionsReferences = db.Collection(collection.Path).Document(documentData.Id).ListCollectionsAsync();

                // Adding each subcollection.
                await foreach(var subcollectionReference in subcollectionsReferences)
                {
                    // Create collection.
                    DataCollection collectionData = new DataCollection(subcollectionReference.Id, documentData.Path + "/" + subcollectionReference.Id);
                    // Add collection documents and subcollections.
                    collectionData.Documents = await GetChildData(collectionData);
                    
                    documentData.Collections.Add(collectionData);
                }

                documents.Add(documentData);
            }

            return documents;
        }
    }    
}
