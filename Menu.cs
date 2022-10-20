namespace firebase_restore
{
    static class Menu
    {
        static int collectionsCount;
        static int documentsCount;
        public static void Show()
        {
            while(true)
            {
                Console.WriteLine("1. Make a backup.");
                Console.WriteLine("2. Restore a backup.");
                Console.WriteLine("3. Exit.");
                Console.Write("What would you like to do? [1-3]:");

                ConsoleKey input = Console.ReadKey().Key;

                HandleInput(input);
            }
        }

        private static void HandleInput(ConsoleKey input)
        {
            switch (input)
            {
                case ConsoleKey.D1:
                    collectionsCount = 0;
                    documentsCount = 0;
                    Backup.Start();
                    break;
                case ConsoleKey.D2:
                    collectionsCount = 0;
                    documentsCount = 0;
                    Restore.Start();
                    break;
                case ConsoleKey.D3:
                    Environment.Exit(0);
                    break;
                default:
                    Menu.Clear(false);
                    break;
            }
        }

        public static void UpdateCollections()
        {
            collectionsCount++;
            
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write("Collections/documents found: " + collectionsCount + "/" + documentsCount);
        }

        public static void UpdateDocuments() 
        {
            documentsCount++;

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write("Collections/documents found: " + collectionsCount + "/" + documentsCount);        
        }

        public static void Clear(bool keyPressNeeded)
        {
            if(keyPressNeeded)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to continue");
                Console.ReadKey(true);
            }
            Console.Clear();
        }
    }
}