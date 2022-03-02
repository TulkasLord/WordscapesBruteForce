using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordscapesBruteForce
{
    internal class ConsoleMenu
    {
        internal static bool SelectionMenu(string letters, int minWordSize, List<string> popularWords, ref List<string> validWords)
        {
            bool validSelection = true;

            ShowMenu();

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.H:
                    Console.WriteLine();
                    H(validWords);
                    break;

                case ConsoleKey.N:

                    // clean
                    validWords = new List<string>();
                    letters = string.Empty;
                    minWordSize = 2;

                    // try again
                    Helpers.Q1(out letters, out minWordSize);
                    Console.WriteLine();
                    Console.Clear();
                    Helpers.StopWatch.Restart();
                    Helpers.StartHeavyWorkWithPermutations(out validWords, letters, minWordSize);
                    Helpers.StopWatch.Stop();
                    break;

                case ConsoleKey.S:
                    Console.WriteLine();
                    Console.Clear();
                    Helpers.StopWatch.Restart();
                    Helpers.StartHeavyWorkWithPermutations(out validWords, string.Join("", letters.Shuffle()), minWordSize);
                    Helpers.StopWatch.Stop();
                    break;

                case ConsoleKey.F:
                    Console.WriteLine();
                    F(validWords, Console.ReadLine());
                    break;

                case ConsoleKey.L:
                    Console.WriteLine();
                    L(validWords);
                    break;

                case ConsoleKey.P:
                    Console.WriteLine();
                    P(validWords, popularWords);
                    break;

                default:
                    Console.WriteLine();
                    validSelection = false;
                    break;
            }

            return validSelection;
        }

        internal static void ShowMenu()
        {
            #region Menu
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("---------------------------------------- O P T I O N S --------------------------------------");
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  (H) ");
            Console.ResetColor();
            Console.WriteLine("- Hints: Type letter + position + word Length. (e.g: 'H1, E4, =5' Result: HOMES)");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  (N) ");
            Console.ResetColor();
            Console.WriteLine("- New set of letters.");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  (S) ");
            Console.ResetColor();
            Console.WriteLine("- Shuffle the current letters.");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  (F) ");
            Console.ResetColor();
            Console.WriteLine("- Find words with the same length.");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  (L) ");
            Console.ResetColor();
            Console.WriteLine("- List all words found.");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  (P) ");
            Console.ResetColor();
            Console.WriteLine("- List all popular words.");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("---------------------------------------- O P T I O N S --------------------------------------");
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.ResetColor();
            Console.WriteLine();
            Console.Write("Selection: ");

            #endregion
        }

        internal static void H(List<string> validWords)
        {
            var hints = Console.ReadLine().Split(',', StringSplitOptions.RemoveEmptyEntries);
            Dictionary<int, char> mapping = new Dictionary<int, char>();
            if (hints.Length > 0)
            {
                int length = 0;
                var filtered = new List<string>();
                foreach (var hint in hints)
                {
                    int intValue = 0;
                    if (hint.Count() > 1)
                    {
                        int.TryParse(hint.Substring(1, hint.Length - 1), out intValue);
                    }
                    else
                        continue;

                    char letter = ' ';
                    if (hint.Count() > 0 && char.IsLetter(hint[0]))
                    {
                        letter = hint[0];
                    }
                    else if (hint.Count() > 0 && char.IsSymbol(hint[0]) && hint[0] == '=')
                    {
                        length = intValue;
                    }
                    else
                        continue;

                    if (length == 0)
                        mapping.Add(intValue, letter);
                }

                string foundIt = string.Empty;
                foreach (var mapped in mapping)
                {
                    int idx = mapped.Key - 1;
                    var matches = validWords.Where(x => x.Length > idx && x.IndexOf(mapped.Value, idx) == idx && (length == 0 ? true : x.Length == length));
                    if (matches.Count() > 0)
                        filtered.AddRange(matches);
                }

                // group the matches the highest is the winner!!
                var groups = filtered.GroupBy(n => n)
                          .Select(n => new
                          {
                              key = n.Key,
                              count = n.Count()
                          })
                          .OrderBy(n => n.count);

                filtered = new List<string>(groups.Where(x => x.count == mapping.Count()).Select(x => x.key));
                if (filtered.Count > 0)
                {
                    Console.WriteLine($"Try this:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(string.Join(", ", filtered));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Sorry, nothing found!");
                }

                Console.ResetColor();
            }

            Console.WriteLine(hints);
        }

        internal static void P(List<string> validWords, List<string> popularWords)
        {
            var matches = validWords.Where(x => popularWords.Contains(x)).ToList();
            if (matches.Count > 0)
                Helpers.AddWordsHorizontallyToConsole(matches);
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Sorry, can't find any popular words.");
                Console.ResetColor();
            }
        }

        internal static void L(List<string> validWords)
        {
            Helpers.AddWordsHorizontallyToConsole(validWords);
        }

        internal static void F(List<string> validWords, string value)
        {
            int length = 0;
            if (!string.IsNullOrEmpty(value))
                int.TryParse(value, out length);
            else
            {
                return;
            }

            var filtered = validWords.Where(x => x.Length == length);
            if (filtered.Any())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Join(", ", filtered));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Sorry, nothing found that matches length '{length}'.");
            }

            Console.ResetColor();
        }

    }
}
