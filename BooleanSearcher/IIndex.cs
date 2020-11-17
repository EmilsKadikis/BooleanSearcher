using System.Collections.Generic;

namespace BooleanSearcher
{
    /// <summary>
    /// Represents any kind of index, so that other parts of boolean queries can be developed without caring about the exact implementation details of the index.
    /// </summary>
    public interface IIndex
    {
        /// <summary>
        /// Normalizes the passed term and returns its posting list, if it exists in the index.
        /// </summary>
        /// <param name="term">Search term without any normalization</param>
        /// <returns>Posting list, if this term is in the index, empty list otherwise.</returns>
        List<int> PostingsList(string term);

        /// <summary>
        /// Gets the document by id. Can be used after obtaining the document id's from a query to get the actual document text.
        /// </summary>
        /// <param name="id">Document id, same as the one stored inb posting lists</param>
        /// <returns>Full details of the document if it exists, throws exception otherwise.</returns>
        Document Document(int id);
    }
}