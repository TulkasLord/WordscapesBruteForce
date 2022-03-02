namespace WordscapesBruteForce
{
    public static class Constants
    {
        public const string LOCAL_DICTIONARY_FILE = "dictionary.json";
        public const string LOCAL_POPULAR_WORDS_FILE = "popular.txt";
        public static readonly string[] EXCLUDED_WORDS =
            {
                "(abbreviation)",
                "Wiktionary does not yet have an entry",
                "Sorry, no definitions found.",
                "Cambridge dictionaries of English"
            };

        public static readonly string[] ONLINE_DICTIONARY_URLS =
            {
                @"https://www.wordnik.com/words/",
                @"https://en.wiktionary.org/wiki/",                         // only english
                @"https://api.dictionaryapi.dev/api/v2/entries/en/",        // only english
                @"https://dictionary.cambridge.org/us/dictionary/english/"  // only english
            };
    }
}
