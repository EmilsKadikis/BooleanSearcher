using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace BooleanSearcher
{
    /// <summary>
    /// Class that applies normalization rules on tokens. Used for both index construction and normalizing query terms.
    /// </summary>
    public class TokenNormalizer
    {
        /// <summary>
        /// List of stop words to not include in the dictionary. There are many, many more that could be included here.
        /// </summary>
        private string[] stopWords = new string[] { "der", "die", "das", "den", "dem", "des", "ein", "eine" };
        
        /// <summary>
        /// Takes a list of tokens and normalizes them
        /// </summary>
        /// <param name="tokens">List of tokens</param>
        /// <returns>List with all the terms.</returns>
        public List<string> Normalize(List<string> tokens)
        {
            var normalizedTokens = new List<string>(tokens.Count);
            // Normalization = remove whitespace from start and end of the token (just in case) -> change everything to lower case -> normalize German-specific characters
            tokens.ForEach(token => normalizedTokens.Add(token.Trim().ToLowerInvariant().NormalizeGermanCharacters()));

            // Remove tokens that are empty, contain no letters or digits or is one of the stop words.
            normalizedTokens.RemoveAll(token => string.IsNullOrWhiteSpace(token) || token.HasNoAlphaNumericSymbols() || stopWords.Contains(token));

            // Eliminate duplicate entries
            return normalizedTokens.Distinct().ToList();
        }
    }

    public static class TokenNormalizationExtensions
    {
        public static string NormalizeGermanCharacters(this string token)
        {
            return token.Replace("ü", "ue")
                .Replace("ä", "ae")
                .Replace("ö", "oe")
                .Replace("ß", "ss");
        }

        public static bool HasNoAlphaNumericSymbols(this string token)
        {
            return token.All(c => !char.IsLetterOrDigit(c));
        }
    }
}
