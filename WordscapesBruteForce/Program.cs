using System;
using System.Collections.Generic;
using System.Threading;

namespace WordscapesBruteForce
{
    class Program
    {
        private static int minWordSize = 2;
        private static string letters = string.Empty;
        private static string originalLetters = string.Empty;
        private static List<string> validWords = new List<string>();
        private static List<string> popularWords = new List<string>();

        static void Main(string[] args)
        {
            try
            {
                Helpers.Start(out popularWords, out letters, out originalLetters, out minWordSize, out validWords);
                while (true)
                {
                    if (validWords.Count > 0)
                    {
                        if (!ConsoleMenu.SelectionMenu(ref letters, ref originalLetters, minWordSize, popularWords, ref validWords))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid selection. Try again.");
                            Console.ResetColor();
                            continue;
                        }
                    }
                    else
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Nothing found!");
                        Console.ResetColor();
                        Thread.Sleep(1000);

                        // try again
                        popularWords.Clear();
                        Helpers.Start(out popularWords, out letters, out originalLetters, out minWordSize, out validWords);
                        originalLetters = letters;
                    }
                }
            }
            finally
            {
                Console.ResetColor();
            }

        }
    }
}
