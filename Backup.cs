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

            await foreach(var coll in collections)
            {
                data.Add(new DataCollection(coll.Id, coll.Path));
            }

            foreach(DataCollection collection in data)
            {
                collection.Documents = await GetChildDocuments(collection);
                
            }


            string json = JsonSerializer.Serialize(data);

            File.WriteAllText("firestore_backup_" + DateTime.Now.ToShortDateString() + ".json", json);
        }
        private static async Task<List<DataDocument>> GetChildDocuments(DataCollection collection)
        {
            QuerySnapshot snapshot = await db.Collection(collection.Id).GetSnapshotAsync();

            List<DataDocument> documents = new List<DataDocument>();

            foreach (var documentSnapshot in snapshot)
            {
                Dictionary<string, object> document = documentSnapshot.ToDictionary();
                DataDocument documentData = new DataDocument(documentSnapshot.Id, documentSnapshot.Reference.Path);

                foreach (var key in document.Keys)
                {
                    documentData.Fields.Add(key, document[key]);
                }

                documents.Add(documentData);
            }

            return documents;
        }
        private static async Task GetDocumentCollections()
        {
            
        }
    }

    class DataCollection
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public List<DataDocument> Documents { get; set; }

        public DataCollection(string id, string path)
        {
            Id = id;
            Path = path;
            Documents = new List<DataDocument>();
        }
    }

    class DataDocument
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public Dictionary<string, object> Fields { get; set; }
        public List<DataCollection> Collections { get; set; }

        public DataDocument(string id, string path)
        {
            Id = id;
            Path = path;
            Fields = new Dictionary<string, object>();
            Collections = new List<DataCollection>();
        }
    }
}
