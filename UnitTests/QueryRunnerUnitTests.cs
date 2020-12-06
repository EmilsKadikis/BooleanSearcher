using System;
using System.Collections.Generic;
using System.Text;
using BooleanSearcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTests
{
    [TestClass]
    public class QueryRunnerUnitTests
    {
        [TestMethod]
        public void QueryRunner_SingleTerm()
        {
            Mock<IIndex> indexMock = new Mock<IIndex>();
            indexMock.Setup(m => m.PostingsLists(It.Is<string>(x => x == "test"))).Returns(PostingListResult("test", new List<int> { 1, 2, 4, 5, 6, 8, 9 }));
            indexMock.Setup(m => m.PostingsLists(It.Is<string>(x => x == "example"))).Returns(PostingListResult("example", new List<int> { 2, 4, 6, 7, 9, 10 }));

            var queryRunner = new QueryRunner(indexMock.Object);

            var result = queryRunner.Query("example");

            Assert.AreEqual(6, result.Count);
            Assert.AreEqual(7, result[3]);
        }

        private static List<PostingResult> PostingListResult(string term, List<int> postingList)
        {
            return new List<PostingResult> { new PostingResult { Term = term, PostingsList = postingList } };
        }

        [TestMethod]
        public void QueryRunner_TwoTerms()
        {
            Mock<IIndex> indexMock = new Mock<IIndex>();
            indexMock.Setup(m => m.PostingsLists(It.Is<string>(x => x == "test"))).Returns(PostingListResult("test", new List<int> { 1, 2, 4, 5, 6, 8, 9 }));
            indexMock.Setup(m => m.PostingsLists(It.Is<string>(x => x == "example"))).Returns(PostingListResult("example", new List<int> { 2, 4, 6, 7, 9, 10 }));
            indexMock.Setup(m => m.PostingsLists(It.Is<string>(x => x == "document"))).Returns(PostingListResult("document", new List<int> { 1, 3, 7, 9, 10 }));

            var queryRunner = new QueryRunner(indexMock.Object);

            var result = queryRunner.Query("example", "test");

            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(9, result[3]);
        }

        [TestMethod]
        public void QueryRunner_TwoTerms_NoDocumentsInCommon()
        {
            Mock<IIndex> indexMock = new Mock<IIndex>();
            indexMock.Setup(m => m.PostingsLists(It.Is<string>(x => x == "test"))).Returns(PostingListResult("test", new List<int> { 1, 2, 3, 5 }));
            indexMock.Setup(m => m.PostingsLists(It.Is<string>(x => x == "example"))).Returns(PostingListResult("example", new List<int> { 6, 7, 9, 10 }));
            indexMock.Setup(m => m.PostingsLists(It.Is<string>(x => x == "document"))).Returns(PostingListResult("document", new List<int> { 1, 3, 7, 9, 10 }));

            var queryRunner = new QueryRunner(indexMock.Object);

            var result = queryRunner.Query("example", "test");

            Assert.AreEqual(0, result.Count);
        }
    }
}
