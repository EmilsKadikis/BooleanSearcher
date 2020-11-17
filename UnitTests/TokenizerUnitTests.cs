using System;
using System.Collections.Generic;
using System.Text;
using BooleanSearcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class TokenizerUnitTests
    {
        [TestMethod]
        public void Tokenize_OneWord()
        {
            var tokenizer = new Tokenizer((char c) => !char.IsLetterOrDigit(c), (char c) => char.IsWhiteSpace(c));

            var tokens = tokenizer.Tokenize("One");

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("One", tokens[0]);
        }

        [TestMethod]
        public void Tokenize_SimpleText()
        {
            var tokenizer = new Tokenizer((char c) => !char.IsLetterOrDigit(c), (char c) => char.IsWhiteSpace(c));

            var tokens = tokenizer.Tokenize("One two three. Four five, six and seven.");

            Assert.AreEqual(11, tokens.Count);
            Assert.AreEqual("One", tokens[0]);
            Assert.AreEqual("Four", tokens[4]);
        }


        [TestMethod]
        public void Tokenize_TextWithQuotes()
        {
            var tokenizer = new Tokenizer((char c) => !char.IsLetterOrDigit(c), (char c) => char.IsWhiteSpace(c));

            var tokens = tokenizer.Tokenize("O’Kennedy hasn’t gone to S.");

            Assert.AreEqual(8, tokens.Count);
            Assert.AreEqual("O", tokens[0]);
            Assert.AreEqual("gone", tokens[4]);
        }

    }
}
