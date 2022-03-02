using System;
using System.Collections.Generic;
using System.Threading;

namespace WordscapesBruteForce
{
    class Program
    {
        private static int minWordSize = 2;
        private static string letters = string.Empty;
        private static List<string> validWords = new List<string>();
        private static List<string> popularWords = new List<string>();

        static void Main(string[] args)
        {
            try
            {
                Helpers.Init(out popularWords);
                Helpers.Q1(out letters, out minWordSize);
                Console.Clear();
                Helpers.StopWatch.Restart();
                Helpers.StartHeavyWorkWithPermutations(out validWords, letters, minWordSize);
                Helpers.StopWatch.Stop();

                while (true)
                {

                    if (validWords.Count > 0)
                    {
                        if (!ConsoleMenu.SelectionMenu(letters, minWordSize, popularWords, ref validWords))
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
