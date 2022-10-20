using Google.Cloud.Firestore;
using System.Text.Json;

namespace firebase_restore
{
    static class Backup
    {
        public static void Start()
        {
            Console.WriteLine();
            Console.WriteLine();

            if(Program.db == null)
            {
                Console.WriteLine("Enter the path to your service account key JSON file, then press enter: ");
                string? keyPath = Console.ReadLine();

                while(string.IsNullOrEmpty(keyPath) || !File.Exists(keyPath))
                {
                    Console.WriteLine("Please enter a valid path.");
                    keyPath = Console.ReadLine();
                }

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);

                string projectId = GetProjectId(keyPath);

                Program.db = FirestoreDb.Create(projectId);
            }

            Console.WriteLine("Backup started...");
            Console.WriteLine("Collections/documents found: 0/0");
            Console.SetCursorPosition(0, Console.CursorTop - 1);

            string savePath = BackupData().Result;
            
            Console.WriteLine();
            Console.WriteLine("Done! File saved at: " + savePath);
            Menu.Clear(true);
        }

        private async static Task<string> BackupData()
        {
            // Getting all collections ID.
            var collections = Program.db.ListRootCollectionsAsync();

            List<DataCollection> data = new List<DataCollection>();

            // Adding root collections data.            
            await foreach(var coll in collections)
            {
                Menu.UpdateCollections();
                data.Add(new DataCollection(coll.Id, coll.Id));
            }

            // Adding documents for each collection.
            foreach(DataCollection collection in data)
            {
                collection.Documents = await GetChildData(collection);
            }

            // Saving data to a file in JSON format.
            string jsonData = JsonSerializer.Serialize(data);

            Directory.CreateDirectory(@"\Backup");

            string savePath = @"\Backup\firestore_backup_" + DateTime.Now.ToShortDateString() + ".json";

            File.WriteAllText(savePath, jsonData);

            return savePath;
        }

        // Gets child documents with their subcollections and fields with values.
        private static async Task<List<DataDocument>> GetChildData(DataCollection collection)
        {
            QuerySnapshot snapshot = await Program.db.Collection(collection.Path).GetSnapshotAsync();
            
            List<DataDocument> documents = new List<DataDocument>();

            foreach (DocumentSnapshot documentSnapshot in snapshot.Documents)
            {
                Menu.UpdateDocuments();
                // Converting document to a dictionary.
                Dictionary<string, object> document = documentSnapshot.ToDictionary();

                DataDocument documentData = new DataDocument(documentSnapshot.Id, collection.Path + "/" + documentSnapshot.Id);
                
                // Adding each field with value.
                foreach (var key in document.Keys)
                {
                    documentData.Fields.Add(key, document[key]);
                }

                IAsyncEnumerable<CollectionReference> subcollectionsReferences = Program.db.Collection(collection.Path).Document(documentData.Id).ListCollectionsAsync();

                // Adding each subcollection.
                await foreach(var subcollectionReference in subcollectionsReferences)
                {
                    Menu.UpdateCollections();
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

        private static string GetProjectId(string path)
        {
            Dictionary<string, object> keyFile = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(path));

            return keyFile["project_id"].ToString();
        }
    }    
}
