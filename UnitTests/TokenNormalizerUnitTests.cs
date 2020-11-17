using System;
using System.Collections.Generic;
using System.Text;
using BooleanSearcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{ 
    [TestClass]
    public class TokenNormalizerUnitTests
    {
        [TestMethod]
        public void Normalize_SimpleSentence()
        {
            var normalizer = new TokenNormalizer();

            var tokens = normalizer.Normalize(new List<string> { "This", "is", "a", "sentence", ".", " "});

            Assert.AreEqual(4, tokens.Count);
            Assert.AreEqual("this", tokens[0]);
        }

        [TestMethod]
        public void Normalize_GermanCharacters()
        {
            var normalizer = new TokenNormalizer();

            var tokens = normalizer.Normalize(new List<string> { "Straße", "München", ".", "Fußgängerübergänge", "Größenmaßstäbe" });

            Assert.AreEqual(4, tokens.Count);
            Assert.AreEqual("strasse", tokens[0]);
            Assert.AreEqual("muenchen", tokens[1]);
            Assert.AreEqual("fussgaengeruebergaenge", tokens[2]);
            Assert.AreEqual("groessenmassstaebe", tokens[3]);
        }
    }
}
