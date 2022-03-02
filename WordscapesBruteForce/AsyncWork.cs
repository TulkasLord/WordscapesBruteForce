using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WordscapesBruteForce
{
    internal static class AsyncWork
    {
        #region Fields
        private static int realWordIndex = 0;
        #endregion

        #region Methods
        internal static async Task<IEnumerable<string>> GetRealWords(IEnumerable<string> words, List<WordDictionaryModel> localDictionary)
        {
            // some cleansing
            words = words.Where(x => !string.IsNullOrEmpty(x));

            // Collect multiple task for each iteration.
            List<Task<string>> taskList = new List<Task<string>>();

            // Fetch words into the LOCAL or ONLINE dictionaries
            foreach (string word in words)
                taskList.Add(IsValidWord(Constants.ONLINE_DICTIONARY_URLS, localDictionary, word));

            return (await Task.WhenAll(taskList)).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        }

        internal static async Task<IEnumerable<string>> GetRealWordsInParallelInWithBatches(IEnumerable<string> words, int batchSize, List<WordDictionaryModel> localDictionary)
        {
            realWordIndex = 0;
            var tasks = new List<Task<IEnumerable<string>>>();
            int numberOfBatches = (int)Math.Ceiling((double)words.Count() / batchSize);

            for (int i = 0; i < numberOfBatches; i++)
            {
                var w = words.Skip(i * batchSize).Take(batchSize);
                tasks.Add(GetRealWords(w, localDictionary));
            }

            return (await Task.WhenAll(tasks)).SelectMany(u => u);
        }

        internal static async Task<string> IsValidWord(string[] urls, List<WordDictionaryModel> localDictionary, string word)
        {
            bool foundIt = false;
            bool isAPI = false;
            string source = string.Empty;
            HttpWebRequest httpRequest;
            WebResponse httpResponse = null;

            // We first look into our Local Dictionary (dictionary.json)
            if (!localDictionary?.Any(a => a.Word == word) ?? true)
            {
                foreach (string url in urls)
                {
                    Stream stream = null;
                    isAPI = url.StartsWith("https://api.");
                    source = isAPI ? "(API)" : "(HTML)";

                    try
                    {
                        // REQUEST
                        httpRequest = (HttpWebRequest)WebRequest.Create(string.Concat(url, HttpUtility.UrlEncode(word)));

                        if (isAPI)
                        {
                            // Read ONLINE dictionary with real words (API)
                            httpRequest.Accept = "application/json";
                        }
                        else
                        {
                            // Read ONLINE dictionary with real words (HTML)
                            httpRequest.AllowAutoRedirect = false;
                            httpRequest.Method = "GET";
                            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
                            httpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                            httpRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
                        }

                        // RESPONSE
                        httpResponse = await httpRequest?.GetResponseAsync();

                        //-----------------------------------
                        // found on Online Dictionaries
                        //-----------------------------------
                        stream = httpResponse?.GetResponseStream();
                        if (stream != null)
                        {
                            using (var streamReader = new StreamReader(stream))
                            {
                                string content = streamReader.ReadToEnd();
                                foreach (var excludedWord in Constants.EXCLUDED_WORDS)
                                {
                                    if (content.Contains(excludedWord))
                                        throw new WebException($"(WIKI) - Word: {word}, excluded.");
                                }

                                if (!isAPI && url.Contains("en.wiktionary") && !content.Contains("ref=\"#English\""))
                                    throw new WebException($"(WIKI) - Word: {word}, excluded. Can't be found on english");

                                Interlocked.Increment(ref realWordIndex);
                                Helpers.AddWordsToConsole(word, source, $"({realWordIndex.ToString("D3")})");

                                // We found the word in at least one source.
                                foundIt = true;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ERROR: {source}. MSG: {ex}");
                        continue;
                    };
                }
            }
            else
            {
                source = "(LOCAL)";

                Interlocked.Increment(ref realWordIndex);
                Helpers.AddWordsToConsole(word, source, $"({realWordIndex.ToString("D3")})");

                // We found the word in at least one source.
                foundIt = true;
            }

            return foundIt ? word : string.Empty;
        }
        #endregion
    }
}
