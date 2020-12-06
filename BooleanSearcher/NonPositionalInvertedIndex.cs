using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BooleanSearcher
{
    public class NonPositionalInvertedIndex : IndexBase
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
        /// <returns>A NonPositionalInvertedIndex from the documents contained in the given csv file.</returns>
        public static NonPositionalInvertedIndex FromFile(string fileName)
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
                return new NonPositionalInvertedIndex(documents);
            }
        }

        public NonPositionalInvertedIndex(List<Document> documents) : base(documents)
        {
           
        }

        /// <summary>
        /// Adds a term to the index.
        /// </summary>
        /// <param name="term">Term to add</param>
        /// <param name="documentIds">The posting list for this term</param>
        protected override void AddTerm(string term, List<int> documentIds)
        {
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
                // Add the new posting list.
                postingLists.Add(nextFreePostingListId, documentIds.OrderBy(id => id).ToList());

                // Add the new term.
                termDictionary.Add(
                    term,
                    new DictionaryEntry
                    {
                        PostingListId = nextFreePostingListId,
                        PostingListSize = documentIds.Count
                    }
                );

                nextFreePostingListId++;
            }
        }

        /// <summary>
        /// Normalizes passed term and returns its posting list, if it's in the index.
        /// </summary>
        /// <param name="term">Term by which to get the posting list. Does not support wildcard queries.</param>
        /// <returns>Posting list, or empty list if the term is not in the dictionary.</returns>
        public override List<PostingResult> PostingsLists(string term)
        {
            // Use the same normalizer that was used to create the index to normalize this term.
            var normalizedTerm = normalizer.Normalize(new List<string> { term }).FirstOrDefault();
            if (!termDictionary.ContainsKey(normalizedTerm))
                return new List<PostingResult>();

            var dictionaryEntry = termDictionary[normalizedTerm];
            return new List<PostingResult> 
            { 
                new PostingResult
                {
                    Term = normalizedTerm,
                    PostingsList = postingLists[dictionaryEntry.PostingListId]
                }
            };
        }
    }

    public struct DictionaryEntry
    {
        public int PostingListSize { get; set; }
        public int PostingListId { get; set; }
    }
}
