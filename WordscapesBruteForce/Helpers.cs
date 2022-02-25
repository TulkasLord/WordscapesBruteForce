using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using System.Web;

namespace WordscapesBruteForce
{
    [Serializable]
    internal class WordDictionaryModel
    {
        public string Word { get; set; }
    }

    internal class Helpers
    {
        #region Fields
        internal static readonly Stopwatch StopWatch = new Stopwatch();
        #endregion

        #region Methods
        internal static void LoadQuestions1(out string letters, out int minWordSize)
        {
            Console.Clear();
            Console.Write("Give me some letters: ");
            letters = Console.ReadLine();
            if (string.IsNullOrEmpty(letters))
                LoadQuestions1(out letters, out minWordSize); // try again 

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
        }

        internal static string[] LoadQuestions2(List<string> validWords)
        {
            Console.WriteLine();
            Console.WriteLine("Any hints? Type letter + position. e.g: E1, R4, etc ...");
            var hints = Console.ReadLine().Split(',', StringSplitOptions.RemoveEmptyEntries);
            Dictionary<int, char> mapping = new Dictionary<int, char>();
            if (hints.Length > 0)
            {
                var filtered = new List<string>();
                foreach (var hint in hints)
                {
                    int position = 0;
                    if (hint.Count() > 1)
                    {
                        int.TryParse(hint.Substring(1, hint.Length - 1), out position);
                    }
                    else
                        continue;

                    char letter = ' ';
                    if (hint.Count() > 0 && char.IsLetter(hint[0]))
                    {
                        letter = hint[0];
                    }
                    else
                        continue;

                    mapping.Add(position, letter);
                }


                string foundIt = string.Empty;
                foreach (var mapped in mapping)
                {
                    var matches = validWords.Where(x => x.IndexOf(mapped.Value) == mapped.Key - 1);
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
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Sorry, nothing found!");
                    Console.ForegroundColor = ConsoleColor.White;
                }

            }

            return hints;
        }

        internal static ConsoleKeyInfo LoadQuestions3()
        {
            Console.WriteLine("Press 'N' to try a [N]ew set of letters.");
            Console.WriteLine("Press 'S' to [S]huffle and the same letters");
            return Console.ReadKey();

        }

        internal static void LoadQuestions4(List<string> validWords)
        {
            int length = 0;

            Console.WriteLine();
            Console.Write("Find words with the same length, type the number and press Enter: ");
            string val = Console.ReadLine();

            if (!string.IsNullOrEmpty(val))
                int.TryParse(val, out length);
            else
            {
                return;
            }

            var filtered = validWords.Where(x => x.Length == length);
            if (filtered.Any())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Join(", ", filtered));
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Sorry, nothing found that matches length '{length}'.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        internal static void StartHeavyWorkWithPermutations(out List<string> validWords, string letters, int minWordSize)
        {
            var res = GetPermutations(letters, minWordSize, letters.Length);
            Console.WriteLine();

            var realWords = GetRealWordsInParallelInWithBatches(res, res.Count());
            validWords = new List<string>(realWords.Result.ToList());

            Console.WriteLine();
            Console.WriteLine("Done!");
        }

        internal static List<string> GetPermutations(string letters, int minWordSize = 2, int size = 6)
        {
            var permutations = letters.ToCharArray().GetPermutations();
            var res = new List<string>();

            Console.SetCursorPosition((Console.WindowWidth / 2) - 5, 0);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"'{letters}'");
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

        internal static async Task<IEnumerable<string>> GetRealWords(IEnumerable<string> words)
        {
            int counter = 0;
            words = words.Where(x => !string.IsNullOrEmpty(x));

            // ONLINE dictionaries with real words
            string[] urls = { @"https://www.wordnik.com/words/", @"https://api.dictionaryapi.dev/api/v2/entries/en/" };

            // LOCAL dictionary with real words
            List<WordDictionaryModel> localDictionary = null;
            using (StreamReader r = new StreamReader(Constants.LOCAL_DICTIONARY_FILE))
                localDictionary = JsonConvert.DeserializeObject<List<WordDictionaryModel>>(r.ReadToEnd()) ?? new List<WordDictionaryModel>();

            // collect multiple task for each iteration.
            List<Task<string>> taskList = new List<Task<string>>();

            // Fetch words into the LOCAL or ONLINE dictionaries
            foreach (string word in words)
            {
                counter++;
                Console.ResetColor();
                taskList.Add(IsValidWord(urls, localDictionary, word));
            }

            return (await Task.WhenAll(taskList)).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        internal static async Task<IEnumerable<string>> GetRealWordsInParallelInWithBatches(IEnumerable<string> words, int batchSize)
        {
            var tasks = new List<Task<IEnumerable<string>>>();
            int numberOfBatches = (int)Math.Ceiling((double)words.Count() / batchSize);

            for (int i = 0; i < numberOfBatches; i++)
            {
                var w = words.Skip(i * batchSize).Take(batchSize);
                tasks.Add(GetRealWords(w));
            }

            return (await Task.WhenAll(tasks)).SelectMany(u => u);
        }

        private static async Task<string> IsValidWord(string[] urls, List<WordDictionaryModel> localDictionary, string word)
        {
            WebResponse httpResponse = null;
            bool isAPI = false;

            var foundItOnLocal = localDictionary != null
               ? localDictionary.FirstOrDefault(x => x.Word == word)
               : null;

            if (foundItOnLocal == null)
            {
                foreach (string url in urls)
                {
                    try
                    {
                        Stream stream = null;
                        isAPI = url.StartsWith("https://api.");

                        if (isAPI)
                        {
                            // Read ONLINE dictionary with real words (API)
                            var httpRequest = (HttpWebRequest)WebRequest.Create(string.Concat(url, HttpUtility.UrlEncode(word)));
                            httpRequest.Accept = "application/json";
                            httpResponse = await httpRequest.GetResponseAsync();
                            stream = httpResponse.GetResponseStream();
                        }
                        else
                        {
                            // Read ONLINE dictionary with real words (HTML)
                            var httpRequest = HttpWebRequest.Create(string.Concat(url, HttpUtility.UrlEncode(word))) as HttpWebRequest;
                            httpRequest.Method = "GET";
                            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
                            httpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                            httpRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
                            httpResponse = await httpRequest.GetResponseAsync();
                            stream = httpResponse.GetResponseStream();
                        }

                        //-----------------------------------
                        // found on Online Dictionaries
                        //-----------------------------------

                        using (var streamReader = new StreamReader(stream))
                        {
                            string content = streamReader.ReadToEnd();
                            bool excluded = false;

                            foreach (var excludedWord in Constants.EXCLUDED_WORDS)
                            {
                                if (content.Contains(excludedWord))
                                {
                                    excluded = true;
                                    break;
                                }
                            }

                            if (!isAPI && excluded)
                                throw new WebException($"Word: {word}, excluded.");

                            //---------------------------------------------------------------------
                            // let's update the local Dictionary with the online Dictionary word
                            //--------------------------------------------------------------------
                            localDictionary.Add(new WordDictionaryModel() { Word = word });
                            string json = JsonConvert.SerializeObject(localDictionary, Formatting.Indented);
                            File.WriteAllText(Constants.LOCAL_DICTIONARY_FILE, json);
                        }
                    }
                    catch
                    {
                        word = string.Empty;
                        continue;
                    };
                }
            }

            if (!string.IsNullOrEmpty(word))
                AddWordToConsole(word, isAPI ? "(API)" : "(HTML)" );

            return word;
        }

        internal static void AddWordToConsole(string word, string comments = "")
        {
            string headerLine = $"[{StopWatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}]";
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{headerLine} :) {word} {comments}");
            Console.ForegroundColor = ConsoleColor.White;
        }
     
        #endregion
    }
}
