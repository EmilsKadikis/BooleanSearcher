using System;
using System.Collections.Generic;
using System.Text;
using BooleanSearcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class PrefixTreeUnitTests
    {
        [TestMethod]
        public void PrexiTree_Find_Exact()
        {
            var tree = new CompactPrefixTree();
            tree.Add("test$", 1);
            tree.Add("testing$", 2);
            tree.Add("example$", 3);
            tree.Add("examples$", 4);

            var result = tree.FindIdsWithPrefix("example$");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result[0].PostingListId);
        }

        [TestMethod]
        public void PrexiTree_Find_Prefix()
        {
            var tree = new CompactPrefixTree();
            tree.Add("test$", 1);
            tree.Add("testing$", 2);
            tree.Add("example$", 3);
            tree.Add("examples$", 4);

            var result = tree.FindIdsWithPrefix("test");

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].PostingListId);
            Assert.AreEqual(2, result[1].PostingListId);
        }

        [TestMethod]
        public void PrexiTree_Find_NonExistant()
        {
            var tree = new CompactPrefixTree();
            tree.Add("test$", 1);
            tree.Add("testing$", 2);
            tree.Add("example$", 3);
            tree.Add("examples$", 4);

            var result = tree.FindIdsWithPrefix("hello");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void PrexiTree_Find_CorrectTerms()
        {
            var tree = new CompactPrefixTree();
            tree.Add("test$", 1);
            tree.Add("testing$", 2);
            tree.Add("tesserect$", 5);
            tree.Add("example$", 3);
            tree.Add("examples$", 4);

            var result = tree.FindIdsWithPrefix("test");

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("test$", result[0].Term);
            Assert.AreEqual("testing$", result[1].Term);
        }
    }
}
