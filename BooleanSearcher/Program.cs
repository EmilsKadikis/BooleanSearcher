using System;
using System.Collections.Generic;

namespace BooleanSearcher
{
    class Program
    {
        static void Main(string[] args)
        {
            IIndex index = Index("postillon.csv");
            var queryRunner = new QueryRunner(index);

            var result1 = queryRunner.Query("weiß", "maße");
            var result2 = queryRunner.Query("weiß", "masse");
            var result3 = queryRunner.Query("weiss", "maße");
            var result4 = queryRunner.Query("weiss", "masse");

            var document = index.Document(-1);

            PrintQueryResult(index, "weiß AND maße", result1);
            PrintQueryResult(index, "weiß AND masse", result2);
            PrintQueryResult(index, "weiss AND maße", result3);
            PrintQueryResult(index, "weiss AND masse", result4);

        }

        /// <summary>
        /// Outputs nicely formatted results of a query.
        /// </summary>
        /// <param name="index">The index which was used for the query. Used to obtain documents for displaying them.</param>
        /// <param name="query">String that describes the query</param>
        /// <param name="resultPostingsList">The posting list that was returned from the query.</param>
        static void PrintQueryResult(IIndex index, string query, List<int> resultPostingsList)
        {
            Console.WriteLine($"Query '{query}' returned {resultPostingsList.Count} results: {string.Join(", ", resultPostingsList)} ");
            foreach(int documentId in resultPostingsList)
            {
                var document = index.Document(documentId);
                Console.WriteLine($"---------------------- Document {documentId} ----------------------");
                Console.WriteLine($"{document.Title}");
                Console.WriteLine($"----------------------------------------------------------");
                Console.WriteLine(document.Text);
                Console.WriteLine($"------------------ End of Document {documentId} -------------------");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Required function. Loads documents from a specifically formatted csv file and creates a non-positional inverted index with the documents from it.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>The created index</returns>
        static IIndex Index(string fileName)
        {
            return NonPositionalInvertedIndex.FromFile(fileName);
        }
    }
}
