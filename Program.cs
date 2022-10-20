using Google.Cloud.Firestore;

namespace firebase_restore
{
    static class Program
    {
        public static FirestoreDb db;
        private static void Main()
        {
            Menu.Show();
        }
    }
}