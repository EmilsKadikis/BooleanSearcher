using System;
using System.Collections.Generic;
using System.Linq;

namespace BooleanSearcher
{
    class Program
    {
        static void Main(string[] args)
        {
            IIndex index = Index("postillon.csv");
            var queryRunner = new QueryRunner(index);

            var result1 = queryRunner.Query("*konferenz", "Presse"); 
            var result2 = queryRunner.Query("fahrrad", "*weg*");
            var result3 = queryRunner.Query("Frank*", "*stein");
            var result4 = queryRunner.Query("Welt*konferenz", "Paris");

            PrintQueryResult(index, "*konferenz AND Presse", result1);
            PrintQueryResult(index, "fahrrad AND *weg*", result2);
            PrintQueryResult(index, "Frank* AND *stein", result3);
            PrintQueryResult(index, "Welt*konferenz AND Paris", result4);
        }

        /// <summary>
        /// Outputs nicely formatted results of a query.
        /// </summary>
        /// <param name="index">The index which was used for the query. Used to obtain documents for displaying them.</param>
        /// <param name="query">String that describes the query</param>
        /// <param name="resultPostingsList">The posting list that was returned from the query.</param>
        static void PrintQueryResult(IIndex index, string query, List<int> resultPostingsList, bool showDocumentText = false)
        {
            Console.WriteLine($"Query '{query}' returned {resultPostingsList.Count} results: {string.Join(", ", resultPostingsList)} ");
            if(showDocumentText)
            {
                foreach (int documentId in resultPostingsList)
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
        }

        /// <summary>
        /// Required function. Loads documents from a specifically formatted csv file and creates a non-positional inverted index with the documents from it.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>The created index</returns>
        static IIndex Index(string fileName)
        {
            return PermutermIndex.FromFile(fileName);
            //return NonPositionalInvertedIndex.FromFile(fileName);
        }
    }
}
