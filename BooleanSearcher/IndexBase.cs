using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BooleanSearcher
{
    /// <summary>
    /// Base class that implements document tokenization and document storage, since they are the same between both index types.
    /// </summary>
    public abstract class IndexBase : IIndex
    {
        // Store all the documents so that we can easily retrieve the text of a specific document.
        protected List<Document> documents = new List<Document>();

        // This tokenizer accepts two functions as configurable rules on how to tokenize.
        protected Tokenizer tokenizer = new Tokenizer(
            tokenSplittingRule: (char c) => !char.IsLetterOrDigit(c),   // Split off new token if any non-alphanumeric character found
            characterErasingRule: (char c) => char.IsWhiteSpace(c)      // Don't include whitespaces in tokens.
        );

        protected TokenNormalizer normalizer = new TokenNormalizer();


        public IndexBase(List<Document> documents)
        {
            this.documents = documents;

            var termOccurances = new List<TermOccurance>();
            // Go through all the documents and create a TermOccurance struct for each term in each document.
            foreach (var document in documents)
            {
                // Tokenize document and normalize all the tokens
                var normalizedTerms = normalizer.Normalize(tokenizer.Tokenize(document.Text));

                // Add a term occurance for each of the normalized terms.
                termOccurances.AddRange(
                    normalizedTerms.Select(term => new TermOccurance
                    {
                        Term = term,
                        DocumentId = document.Id
                    })
                );
            }

            // Now that we have a list that contains all the term : document id pairs, we can process them
            termOccurances
                .GroupBy(x => x.Term)   // Group all the occurances by the term, so that we get a list of <term, list of document ids>
                .ToList()
                .ForEach(               // Iterate through all the terms
                    group => AddTerm(   // and add a new term to the dictionary with all of its postings.
                        group.Key,                               // Term
                        group.Select(g => g.DocumentId).ToList() // Posting list for this term
                    )
                );
        }

        /// <summary>
        /// Used while constructing index to remember which terms occured in which documents.
        /// </summary>
        private struct TermOccurance
        {
            public string Term { get; set; }
            public int DocumentId { get; set; }
        }

        /// <summary>
        /// Called for each term that's found in the documents. This method is abstract, since its implementation depends
        /// on how the specific type of index stores terms and postings.
        /// </summary>
        /// <param name="term">Term to add to the index</param>
        /// <param name="documentIds">Ids of all the documents that this term appeared in</param>
        protected abstract void AddTerm(string term, List<int> documentIds);

        /// <summary>
        /// Gets document data by its id.
        /// </summary>
        /// <param name="id">Document Id</param>
        /// <returns>Details of the document. Throws exception if document with this id doesn't exist.</returns>
        public Document Document(int id)
        {
            return documents.First(d => d.Id == id);
        }

        /// <summary>
        /// Returns postings lists that matched the term. Implementation depends on type of index. For example, a wildcard query would only work
        /// on the permuterm index.
        /// </summary>
        /// <param name="term">Term to search for.</param>
        /// <returns>A list of term/posting list pairs that matched the query.</returns>
        public abstract List<PostingResult> PostingsLists(string term);
    }
}
