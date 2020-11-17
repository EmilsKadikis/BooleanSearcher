using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BooleanSearcher;

namespace UnitTests
{
    [TestClass]
    public class CsvDocumentParserUnitTests
    {
        [TestMethod]
        public void Parse_EmptyText()
        {
            string text = "";
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                var documents = stream.ParseAsDocumentCsv();
                Assert.AreEqual(0, documents.Count);
            }
        }

        
        [TestMethod]
        public void Parse_MultipleRows()
        {
            string text = "id\turl\tpub_date\ttitle\tnews_text\n" +
                          "1\t\tMontag,4. November 2013\tTitle\tThis is the text of the article.\n" +
                          "2\t\tDienstag,5. November 2013\tTitle 2\tThis is the text of the second article.";
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                var documents = stream.ParseAsDocumentCsv(delimiter:"\t");
                Assert.AreEqual(2, documents.Count);
                Assert.AreEqual(2, documents[1].Id);
                Assert.AreEqual("Title 2", documents[1].Title);
            }
        }
    }
}
