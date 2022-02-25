using System;
using System.Threading.Tasks;

namespace WordscapesBruteForce
{
    public static class Constants
    {
        public const string LOCAL_DICTIONARY_FILE = "dictionary.json";
        public static readonly string[] EXCLUDED_WORDS =
            {
                "(abbreviation)",
                "Sorry, no definitions found.",
                "Sorry, no etymologies found."
            };
    }
}
