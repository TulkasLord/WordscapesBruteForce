using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace WordscapesBruteForce
{
    internal class Helpers
    {
        #region Fields
        internal static readonly Stopwatch StopWatch = new Stopwatch();
        #endregion

        #region Methods

        internal static void Init(out List<string> popularWords)
        {
            popularWords = new List<string>();
            using (StreamReader r = new StreamReader(Constants.LOCAL_POPULAR_WORDS_FILE))
                popularWords.AddRange(r.ReadToEnd().Split(Environment.NewLine));

            popularWords = popularWords.Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();
        }

        internal static void StartHeavyWorkWithPermutations(out List<string> validWords, string letters, string originalLetters, int minWordSize)
        {
            var res = GetPermutations(letters, originalLetters, minWordSize, letters.Length);
            Console.WriteLine();

            // Load content from Local Dictionary with real words
            List<WordDictionaryModel> localDictionary = null;
            using (StreamReader r = new StreamReader(Constants.LOCAL_DICTIONARY_FILE))
                localDictionary = JsonConvert.DeserializeObject<List<WordDictionaryModel>>(r.ReadToEnd()) ?? new List<WordDictionaryModel>();

            // Do the heavy job here ...
            var realWords = AsyncWork.GetRealWordsInParallelInWithBatches(res, res.Count(), localDictionary);
            validWords = new List<string>(realWords.Result.ToList());

            // Let's update the local Dictionary with the Online Dictionary words
            if (validWords.Count > 0)
            {
                try
                {
                    localDictionary.AddRange(validWords.Select(s => new WordDictionaryModel() { Word = s }));
                    string json = JsonConvert.SerializeObject(localDictionary.ToList(), Formatting.Indented);
                    File.WriteAllText(Constants.LOCAL_DICTIONARY_FILE, json);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            Console.ResetColor();
            Console.WriteLine();
        }

        internal static List<string> GetPermutations(string letters, string originalLetters, int minWordSize = 2, int size = 6)
        {
            var permutations = letters.ToCharArray().GetPermutations();
            var res = new List<string>();

            string message = !string.IsNullOrEmpty(originalLetters) && letters != originalLetters
                ? $"{originalLetters} => {letters}"
                : letters;

            Console.SetCursorPosition((Console.WindowWidth / 2) - 5, 0);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"'{message}'");
            Console.ResetColor();

            for (int i = 0; i < permutations.Count(); i++)
            {
                Console.SetCursorPosition(0, 0);

                char[] chars = (char[])permutations.ElementAt(i);
                string str = string.Join(string.Empty, chars);

                var results = from e in Enumerable.Range(0, (int)BigInteger.Pow(2, size))
                              let p =
                              from b in Enumerable.Range(1, size)
                              select (e & (int)BigInteger.Pow(2, b - 1)) == 0
                                    ? (char?)null
                                    : str[b - 1]
                              select string.Join(string.Empty, p);

                res.AddRange(results.Where(x => x.Length > minWordSize));
                res = res.Distinct().ToList();

                Console.Write($"Best permutations: {res.Count()}");
            }
            return res;
        }

        internal static void AddWordsHorizontallyToConsole(List<string> words, int width = 10)
        {
            for (int i = 0; i < words.Count; i++)
                AddWordsToConsole(words[i], "]", "[", false, i == 0 ? true : (i % width) != 0);

        }

        internal static void AddWordsToConsole(string word, string comments = "", string prefix = "", bool showHeaderLine = true, bool inline = false)
        {
            string headerLine = showHeaderLine
                ? $"[{StopWatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}]"
                : string.Empty;

            Console.ForegroundColor = ConsoleColor.Green;

            if (!inline)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.WriteLine($"{headerLine} {prefix} {word} {comments}");
            }
            else
                Console.Write($"{headerLine} {prefix} {word} {comments}".PadLeft(3).PadRight(3));

        }

        internal static void Q1(out string letters, out string originalLetters, out int minWordSize)
        {
            letters = string.Empty;

            Console.Clear();
            Console.Write("Give me some letters: ");
            letters = Console.ReadLine();
            if (string.IsNullOrEmpty(letters))
            {
                Q1(out letters, out originalLetters, out minWordSize); // try again 
                return;
            }

            if (letters.Any(a => !char.IsLetter(a)))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Only letters are allowed.");
                Console.ResetColor();
                Thread.Sleep(1000);
                Q1(out letters, out originalLetters, out minWordSize); // try again
                return;
            }

            Console.Write("Min. of letters per word (default is 2): ");
            string min = Console.ReadLine();
            if (!string.IsNullOrEmpty(min))
            {
                int.TryParse(min, out minWordSize);
            }
            else
            {
                minWordSize = 2;
            }

            Console.WriteLine();
            Console.WriteLine("All set!.");
            Console.Write($"We are about to generate words from this set of letters:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($" '{letters}'");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"The new words generated will have min. lenght of");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($" '{minWordSize}'");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("press any key to continue ...");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();
            Console.Clear();

            originalLetters = letters;
        }

        internal static void Start(out List<string> popularWords, out string letters, out string originalLetters, out int minWordSize, out List<string> validWords)
        {
            Init(out popularWords);
            Q1(out letters, out originalLetters, out minWordSize);
            Console.Clear();
            StopWatch.Restart();
            StartHeavyWorkWithPermutations(out validWords, letters, originalLetters, minWordSize);
            StopWatch.Stop();
        }

        #endregion
    }
}
