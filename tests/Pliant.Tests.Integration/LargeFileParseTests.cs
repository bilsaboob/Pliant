﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pliant.Automata;
using Pliant.Grammars;
using Pliant.Json;
using Pliant.RegularExpressions;
using Pliant.Runtime;
using Pliant.Tests.Common;
using System.IO;

namespace Pliant.Tests.Integration.Runtime
{
    [TestClass]
    public class LargeFileParseTests
    { 
        public TestContext TestContext { get; set; }

        private static IGrammar _grammar;

        private ParseTester _parseTester;
        private ParseTester _compressedParseTester;
        
        [ClassInitialize]
#pragma warning disable CC0057 // Unused parameters
        public static void Initialize(TestContext testContext)
#pragma warning restore CC0057 // Unused parameters
        {
            _grammar = new JsonGrammar();
        }

        [TestInitialize]
        public void InitializeTest()
        {
            _parseTester = new ParseTester(_grammar);
            _compressedParseTester = new ParseTester(
                new DeterministicParseEngine(
                    new PreComputedGrammar(_grammar)));
        }

        [TestMethod]
        public void TestCanParseJsonArray()
        {
            var json = @"[""one"", ""two""]";
            _parseTester.RunParse(json);
        }

        [TestMethod]
        public void TestCanParseJsonObject()
        {
            var json = @"
            {
                ""firstName"":""Patrick"", 
                ""lastName"": ""Huber"",
                ""id"": 12345
            }";
            _parseTester.RunParse(json);
        }

        [TestMethod]
        [DeploymentItem(@"Runtime\10000.json", "Runtime")]
        public void TestCanParseLargeJsonFile()
        {
            var path = Path.Combine(TestContext.TestDeploymentDir, "Runtime", "10000.json");
            using (var stream = File.OpenRead(path))
            using (var reader = new StreamReader(stream))
            {
                _parseTester.RunParse(reader);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Runtime\10000.json", "Runtime")]
        public void TestCanParseLargeJsonFileWithCompression()
        {
            var path = Path.Combine(TestContext.TestDeploymentDir, "Runtime", "10000.json");
            using (var stream = File.OpenRead(path))
            using (var reader = new StreamReader(stream))
            {
                _compressedParseTester.RunParse(reader);
            }
        }
        
        [TestMethod]
        public void TestCanParseJsonArrayWithCompression()
        {
            var json = @"[""one"", ""two""]";
            _compressedParseTester.RunParse(json);
        }

        [TestMethod]
        public void TestCanParseJsonObjectWithCompression()
        {
            var json = @"
            {
                ""firstName"":""Patrick"", 
                ""lastName"": ""Huber"",
                ""id"": 12345
            }";
            _compressedParseTester.RunParse(json);
        }

        private static ILexerRule Whitespace()
        {
            var start = new DfaState();
            var end = new DfaState(isFinal: true);
            var transition = new DfaTransition(
                new WhitespaceTerminal(),
                end);
            start.AddTransition(transition);
            end.AddTransition(transition);
            return new DfaLexerRule(start, "\\w+");
        }
        
        private static BaseLexerRule String()
        {
            // ["][^"]+["]
            const string pattern = "[\"][^\"]+[\"]";
            return CreateRegexDfa(pattern);
        }

        private static BaseLexerRule CreateRegexDfa(string pattern)
        {
            var regexParser = new RegexParser();
            var regex = regexParser.Parse(pattern);
            var regexCompiler = new RegexCompiler();
            var dfa = regexCompiler.Compile(regex);
            return new DfaLexerRule(dfa, pattern);
        }

    }
}
