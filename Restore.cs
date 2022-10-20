using Google.Cloud.Firestore;
using System.Text.Json;

namespace firebase_restore
{
    class Restore
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

            string? backupFilePath = SelectFile();

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

            Console.WriteLine("Done!");
            Menu.Clear(true);
        }

        private static string? SelectFile()
        {
            Console.WriteLine("Enter full path to the backup JSON file (or type 'r' to return): ");
            
            string? path = Console.ReadLine();

            while(string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                if(path != null && path.ToLower() == "r") return null;
                Console.WriteLine("Wrong path, try again");
                path = Console.ReadLine();
            }

            return path;
        }

        private static async Task RestoreData(string filePath)
        {
            // Get the data from the file and deserialize from JSON format.
            List<DataCollection>? data = JsonSerializer.Deserialize<List<DataCollection>>(File.ReadAllText(filePath));

            if(data == null) return;

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
                    fields.Add(field.Key, field.Value.ToString());
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

        private static string GetProjectId(string path)
        {
            Dictionary<string, object> keyFile = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(path));

            return keyFile["project_id"].ToString();
        }
    }
}