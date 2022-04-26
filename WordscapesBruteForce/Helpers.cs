using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

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
            Console.WriteLine("Fetching real words ...");

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
            if (validWords.Count > 0)
                Console.WriteLine($"A total of {validWords.Count} real words were found.");

            Console.WriteLine();
        }

        internal static List<string> GetPermutations(string letters, string originalLetters, int minWordSize = 2, int size = 6)
        {
            var permutations = letters.ToCharArray().GetPermutations();
            var res = new List<string>();

            string message = !string.IsNullOrEmpty(originalLetters) && letters != originalLetters
                ? $"{originalLetters} => {letters}. ({minWordSize})"
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
            {
                AddWordsToConsole(words[i], ConsoleColor.Green,
                    comments: "]", ConsoleColor.Green,
                    prefix: "[", ConsoleColor.Green,
                    showHeaderLine: false,
                    inline: (i == 0 || i == words.Count - 1) ? true : (i % width) != 0); // exclude boundaries 
            }
        }

        internal static void AddWordsToConsole(string word, ConsoleColor wordColor,
            string comments = "",
            ConsoleColor? commentsColor = null,
            string prefix = "",
            ConsoleColor? prefixColor = null,
            bool showHeaderLine = true,
            bool inline = false)
        {
            string headerLine = showHeaderLine
                ? $"[{StopWatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}]"
                : string.Empty;

            if (!inline)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.WriteLine($"{headerLine} {prefix} {word} {comments}");
            }
            else
                Console.Write($"{headerLine} {prefix} {word} {comments}".PadLeft(3).PadRight(3));

            //Console.SetCursorPosition(0, Console.CursorTop);
            //TextConsoleColoring($"{headerLine}", ConsoleColor.White);
            //
            //Console.SetCursorPosition(20, Console.CursorTop);
            //TextConsoleColoring($" {prefix}", prefixColor ?? ConsoleColor.White);
            //
            //Console.SetCursorPosition(30, Console.CursorTop);
            //TextConsoleColoring($" {word}", wordColor);
            //
            //Console.SetCursorPosition(40, Console.CursorTop);
            //TextConsoleColoring($" {comments}", commentsColor ?? ConsoleColor.White);
            //
            //Console.WriteLine();
        }

        internal static void Start(out List<string> popularWords, out string letters, out string originalLetters, out int minWordSize,
            out List<string> validWords,
            out ConsoleKey? continueOption)
        {
            Init(out popularWords);
            ConsoleMenu.Q1(out letters, out originalLetters, out minWordSize, out continueOption);
            Console.Clear();
            StopWatch.Restart();
            StartHeavyWorkWithPermutations(out validWords, letters, originalLetters, minWordSize);
            StopWatch.Stop();
        }

        private static void TextConsoleColoring(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        #endregion
    }
}
