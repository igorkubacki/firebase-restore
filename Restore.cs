using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace firebase_restore
{
    class Restore
    {
        public static void Start()
        {
            Console.WriteLine();
            Console.WriteLine();

            // Create an instance of the Firestore client if not already created.
            if(Program.db == null)
            {
                string? keyPath = Menu.SelectFile("Enter the path to your service account key JSON file");
                if(keyPath == null) 
                {
                    Menu.Clear(false);
                    return;
                }

                string? projectId = GetProjectId(keyPath);

                if(projectId == null) return;

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);
                Program.db = FirestoreDb.Create(projectId);
            }
            
            string? backupFilePath = Menu.SelectFile("Enter the path to your backup JSON file");

            if(backupFilePath == null)
            {
                Menu.Clear(false);
                return;
            }

            Console.WriteLine("Restoring started...");
            Console.WriteLine("Collections/documents found: 0/0");
            Console.SetCursorPosition(0, Console.CursorTop - 1);
                
            Task task = RestoreData(backupFilePath);
            task.Wait();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Done!");
            Console.WriteLine();
            Menu.Clear(true);
        }

        private static async Task RestoreData(string filePath)
        {
            // Get the data from the file and deserialize from JSON format.
            var data = JsonConvert.DeserializeObject<List<DataCollection>>(File.ReadAllText(filePath));

            if (data == null) return;

            // Restore data for each root collection.
            foreach(DataCollection collection in data)
            {
                await RestoreCollection(collection);
                Menu.UpdateCollections();
            }
        }
        
        private static async Task RestoreCollection(DataCollection collection)
        {
            foreach(DataDocument document in collection.Documents)
            {
                Dictionary<string, object> fields = new Dictionary<string, object>();

                foreach(var field in document.Fields)
                {
                    fields.Add(field.Key, field.Value);
                }

                await Program.db.Document(document.Path).SetAsync(fields);
                Menu.UpdateDocuments();

                // Restore data for each of the document collections.
                foreach(DataCollection subcollection in document.Collections)
                {
                    await RestoreCollection(subcollection);
                    Menu.UpdateCollections();
                }
            }
        }

        private static string? GetProjectId(string path)
        {
            try
            {
                var keyFile = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(path));

                return keyFile["project_id"].ToString();
            }
            catch(Exception e)
            {
                Console.WriteLine("An exception has occured: " + e.Message);
                Console.WriteLine("Make sure you entered the right path to the service key file.");
                Console.WriteLine();
            }
            
            return null;
        }
    }
}