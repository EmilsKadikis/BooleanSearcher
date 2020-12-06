using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BooleanSearcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class PermutermIndexTests
    {
        [TestMethod]
        public void PermutermIndex_NoWildCard()
        {

            var documents = new List<Document>
            {
                new Document
                {
                    Id = 1,
                    Text = "Test document number 1. Ut enim ad minim veniam."
                },
                new Document
                {
                    Id = 2,
                    Text = "Test document number 2. Lorem ipsum."
                },
                new Document
                {
                    Id = 3,
                    Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
                },
            };

            IIndex index = new PermutermIndex(documents);
            var result = index.PostingsLists("Lorem").First().PostingsList;

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(3, result[1]);
        }

        [TestMethod]
        public void PermutermIndex_Rotate()
        {
            PermutermIndex index = new PermutermIndex(new List<Document>());

            Assert.AreEqual("X$", index.Rotate("X"));
            Assert.AreEqual("$X*", index.Rotate("X*"));
            Assert.AreEqual("X$*", index.Rotate("*X"));
            Assert.AreEqual("X*", index.Rotate("*X*"));
            Assert.AreEqual("Y$X*", index.Rotate("X*Y"));


            Assert.AreEqual("test$", index.Rotate("test"));
            Assert.AreEqual("$test*", index.Rotate("test*"));
            Assert.AreEqual("test$*", index.Rotate("*test"));
            Assert.AreEqual("test*", index.Rotate("*test*"));
            Assert.AreEqual("example$test*", index.Rotate("test*example"));
        }

        [TestMethod]
        public void PermutermIndex_WildCard()
        {

            var documents = new List<Document>
            {
                new Document
                {
                    Id = 1,
                    Text = "Test document number 1. Ut enim ad minim veniam. Look."
                },
                new Document
                {
                    Id = 2,
                    Text = "Test document number 2. Lorem ipsum."
                },
                new Document
                {
                    Id = 3,
                    Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
                },
            };

            IIndex index = new PermutermIndex(documents);
            var result = index.PostingsLists("Lo*");

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(1, result[0].PostingsList.Count);
            Assert.AreEqual(1, result[0].PostingsList[0]);

            Assert.AreEqual(2, result[1].PostingsList.Count);
            Assert.AreEqual(2, result[1].PostingsList[0]);
            Assert.AreEqual(3, result[1].PostingsList[1]);
        }

    }
}
