namespace firebase_restore
{
    static class Firebase_Restore
    {
        private static void Main()
        {
            while (true)
            {
                Console.WriteLine("1. Make a backup");
                Console.WriteLine("2. Restore a backup");
                Console.WriteLine("3. Exit");
                Console.Write("What would you like to do [1-3]:");
                ConsoleKey input = Console.ReadKey().Key;

                switch (input)
                {
                    case ConsoleKey.D1:
                        Backup.Start();
                        break;
                    case ConsoleKey.D2:
                        break;
                    case ConsoleKey.D3:
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine();
                        Console.WriteLine("Invalid input, try again.");
                        break;
                }
                Console.WriteLine();
            }
        }
    }
}