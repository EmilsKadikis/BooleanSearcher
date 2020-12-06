using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BooleanSearcher
{
    public class PermutermIndex : IndexBase
    {
        // Dictionaries in C# are implemented as hash tables and provide a O(1) lookup time. This one uses the normalized term as the key, and holds the posting list size and posting list id.
        protected Dictionary<string, DictionaryEntry> termDictionary = new Dictionary<string, DictionaryEntry>();

        // With an integer key, this dictionary effectively functions like an array (but one where we don't have to manually worry about allocating enough space beforehand)
        // since the hash of an integer is equal to the integer itself.
        protected Dictionary<int, List<int>> postingLists = new Dictionary<int, List<int>>();

        // When we need to create a new posting list for a new term, we use this as the id for the posting list and advance it by one.
        // Otherwise we'd need to go through the postingLists keys to find the next free id.
        protected int nextFreePostingListId = 1;

        /// <summary>
        /// Load's file with given file name, parses it as CSV and creates an index.
        /// </summary>
        /// <param name="fileName">Name or path of the file to load. Can be either local or absolute.</param>
        /// <returns>A PermutermIndex from the documents contained in the given csv file.</returns>
        public static PermutermIndex FromFile(string fileName)
        {
            // Get full file path
            string filePath;
            if (Directory.Exists(fileName))
                filePath = fileName;
            else
                filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            // Open file and parse it into a list of documents, then create the index.
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var documents = stream.ParseAsDocumentCsv(delimiter: "\t");
                return new PermutermIndex(documents);
            }
        }

        public PermutermIndex(List<Document> documents) : base(documents)
        {

        }

        /// <summary>
        /// Add term to dictionary. If it already exists, the new documentIds are added into the existing posting list,
        /// but if it's a new term, one posting list is added and dictionary entries for the term in all possible rotations.
        /// </summary>
        /// <param name="term">Term to add</param>
        /// <param name="documentIds">List of document ids where the term appears in</param>
        protected override void AddTerm(string term, List<int> documentIds)
        {
            term += "$";
            // If term is already in the dictionary, add documentIds to the already existing posting list.
            if (termDictionary.ContainsKey(term))
            {
                var dictionaryEntry = termDictionary[term];
                var postingsListId = dictionaryEntry.PostingListId;
                var postingList = postingLists[postingsListId];

                // Add new postings, eliminate duplicates and order it again to be in ascending order.
                postingList.AddRange(documentIds);
                postingList = postingList.Distinct().OrderBy(id => id).ToList();

                // Update posting list and posting list size that's stored in dictionary.
                postingLists[postingsListId] = postingList;
                termDictionary[term] = new DictionaryEntry
                {
                    PostingListId = postingsListId,
                    PostingListSize = postingList.Count
                };
            }
            else // else, if the term doesn't yet exist in the dictionary, add it.
            {
                // Add the new posting list
                postingLists.Add(nextFreePostingListId, documentIds.OrderBy(id => id).ToList());

                for (int i = 0; i < term.Length; i++)
                {
                    // Add the new term
                    termDictionary.Add(
                        term,
                        new DictionaryEntry
                        {
                            PostingListId = nextFreePostingListId,
                            PostingListSize = documentIds.Count
                        }
                    );

                    // Rotate term by taking all the character starting from the second character and appending the first character to the end
                    term = term.Substring(1) + term[0];
                }

                nextFreePostingListId++;
            }
        }

        /// <summary>
        /// Rotates the term so that any wildcards are at the end, then either finds one posting list if there is no wildcard,
        /// or all postings lists for terms that start with the rotated term.
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public override List<PostingResult> PostingsLists(string term)
        {
            // Use the same normalizer that was used to create the index to normalize this term.
            var normalizedTerm = normalizer.Normalize(new List<string> { term }).FirstOrDefault();
            normalizedTerm = Rotate(normalizedTerm);
            if (normalizedTerm.EndsWith("*")) // If the term had a wildcard, after rotating it will be at the end
            {
                normalizedTerm = normalizedTerm.TrimEnd('*');
                return termDictionary
                    .Keys.Where(k => k.StartsWith(normalizedTerm))
                    .Select(k =>
                        new PostingResult
                        {
                            Term = k,
                            PostingsList = postingLists[termDictionary[k].PostingListId]
                        }
                    ).ToList();
            }
            else
            {
                if (!termDictionary.ContainsKey(normalizedTerm))
                    return new List<PostingResult>();

                var dictionaryEntry = termDictionary[normalizedTerm];
                return new List<PostingResult>()
                {
                    new PostingResult
                    {
                        Term = normalizedTerm,
                        PostingsList = postingLists[dictionaryEntry.PostingListId]
                    }
                };
            }
        }

        /// <summary>
        /// Rotate a term so that it has $ at the end of the word and so that the wildcard is at the end,
        /// so that it can be used with a prefix tree.
        /// </summary>
        /// <param name="term">Normalized term with or without a wildcard.</param>
        /// <returns>Rotated term with $ and the wildcard at the end.</returns>
        public string Rotate(string term)
        {
            int wildcardCount = term.Count(x => x == '*');
            bool startsWithWildcard = term.StartsWith('*');
            bool endsWithWildcard = term.EndsWith('*');
            switch (wildcardCount)
            {
                case var _ when wildcardCount > 2:
                    throw new ArgumentException($"Too many wildcards in term {term}. Such searches are not supported.");
                case var _ when wildcardCount == 2 && !(startsWithWildcard && endsWithWildcard):
                    throw new ArgumentException($"Two wildcards only supported if they're at the start and the end of the term {term}");
                case var _ when wildcardCount == 2: 
                    return term.TrimStart('*');             // *X* -> X*
                case var _ when wildcardCount == 1 && startsWithWildcard:
                    return term.TrimStart('*') + "$*";      // *X -> X$*
                case var _ when wildcardCount == 1 && endsWithWildcard:
                    return "$" + term;                      // X* -> $X*
                case var _ when wildcardCount == 1:
                    int wildcardIndex = term.IndexOf("*");  // X*Y -> Y$X*
                    return term.Substring(wildcardIndex + 1) + "$" + term.Substring(0, wildcardIndex) + "*";
                default:
                    return term + "$";                      // X -> X$
            }
        }
    }

}
