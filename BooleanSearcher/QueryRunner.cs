using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BooleanSearcher
{
    /// <summary>
    /// Works with multiple posting lists in order to do more complex queries.
    /// </summary>
    public class QueryRunner
    {
        private IIndex index;
        public QueryRunner(IIndex index)
        {
            this.index = index;
        }

        /// <summary>
        /// Query by one term.
        /// </summary>
        /// <param name="term">Term to look for.</param>
        /// <returns>Posting list of passed term, empty list if it's not in the index (and hence not in any of the documents).</returns>
        public List<int> Query(string term)
        {
            return index.PostingsList(term);
        }

        /// <summary>
        /// Query for the conjunction of two terms.
        /// </summary>
        /// <param name="firstTerm"></param>
        /// <param name="secondTerm"></param>
        /// <returns>Posting list of the result.</returns>
        public List<int> Query(string firstTerm, string secondTerm)
        {
            var firstPostingList = index.PostingsList(firstTerm);
            var secondPostingList = index.PostingsList(secondTerm);

            return Intersect(firstPostingList, secondPostingList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstPostingList"></param>
        /// <param name="secondPostingList"></param>
        /// <returns></returns>
        private List<int> Intersect(List<int> firstPostingList, List<int> secondPostingList)
        {
            var intersection = new List<int>();

            var firstEnumerator = firstPostingList.GetEnumerator();
            var secondEnumerator = secondPostingList.GetEnumerator();

            // The enumerators start pointing to before the first element, so we need to move both onto the first element to start it off.
            bool hasMoreEntries = firstEnumerator.MoveNext() && secondEnumerator.MoveNext();

            // If moving even one of the enumerators forwards failed, that means that there can be no further intersecting elements, so we can stop.
            while (hasMoreEntries) 
            {
                int firstId = firstEnumerator.Current;
                int secondId = secondEnumerator.Current;
                if (firstId == secondId) 
                {
                    // Intersection found, advance both enumerators.
                    intersection.Add(firstId);
                    hasMoreEntries = firstEnumerator.MoveNext() && secondEnumerator.MoveNext();
                }
                else
                {
                    // Intersection not found, advance the enumerator which returned the lowest id.
                    hasMoreEntries = firstId < secondId ? firstEnumerator.MoveNext() : secondEnumerator.MoveNext();
                }
            }
            
            return intersection;
        }
    }
}
