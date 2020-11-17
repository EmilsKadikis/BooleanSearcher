using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.VisualBasic.FileIO;

namespace BooleanSearcher
{
    public static class CsvDocumentParsingExtensions
    {
        /// <summary>
        /// Helper function that parses the postillon.csv file and returns a list of Documents.
        /// </summary>
        /// <param name="stream"> The fact that this parameter is marked with "this" signifies that it can be called on a Stream type object, like so: file.ParseAsDocumentCsv(delimiter:"\t")</param>
        /// <param name="delimiter">The character that's used to separate fields in the CSV file.</param>
        /// <returns>A list of Document structs that contain every column from the file.</returns>
        public static List<Document> ParseAsDocumentCsv(this Stream stream, string delimiter=",")
        {
            var result = new List<Document>();
            CsvReader csv = new CsvReader(
                new StreamReader(stream), 
                new CsvConfiguration(CultureInfo.GetCultureInfo("de-DE")) 
                { 
                    Delimiter = delimiter,
                    IgnoreQuotes = true,
                    BadDataFound = ctx => { Console.WriteLine(string.Join("", ctx.RawRecord.Take(20).ToList())); } 
                }
            );
            csv.Configuration.RegisterClassMap<DocumentMap>();

            return csv.GetRecords<Document>().ToList();
        }

        /// <summary>
        /// Used by CsvReader to know which columns match with which fields in the struct.
        /// </summary>
        public sealed class DocumentMap : ClassMap<Document>
        {
            public DocumentMap()
            {
                Map(m => m.Id).Name("id");
                Map(m => m.Source).Name("url");
                Map(m => m.Date).Name("pub_date");
                Map(m => m.Title).Name("title");
                Map(m => m.Text).Name("news_text");
            }
        }
    }
}
