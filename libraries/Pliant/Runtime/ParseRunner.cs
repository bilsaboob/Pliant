﻿using Pliant.Automata;
using Pliant.Collections;
using Pliant.Grammars;
using Pliant.Lexemes;
using Pliant.Tokens;
using Pliant.Utilities;
using System.Collections.Generic;
using System.IO;

namespace Pliant.Runtime
{
    public class ParseRunner : IParseRunner
    {
        private List<ILexeme> _existingLexemes;
        private List<ILexeme> _ignoreLexemes;
        private readonly ILexemeFactoryRegistry _lexemeFactoryRegistry;

        private readonly TextReader _textReader;

        public IParseEngine ParseEngine { get; private set; }

        public int Position { get; private set; }

        public ParseRunner(IParseEngine parseEngine, string input)
                            : this(parseEngine, new StringReader(input))
        {
        }

        public ParseRunner(IParseEngine parseEngine, TextReader input)
        {
            _textReader = input;

            _lexemeFactoryRegistry = new LexemeFactoryRegistry();
            RegisterDefaultLexemeFactories(_lexemeFactoryRegistry);

            _ignoreLexemes = new List<ILexeme>();
            _existingLexemes = new List<ILexeme>();

            Position = 0;
            ParseEngine = parseEngine;
        }

        public bool EndOfStream()
        {
            return _textReader.Peek() == -1;
        }

        public bool Read()
        {
            if (EndOfStream())
                return false;

            var character = ReadCharacter();

            if (MatchesExistingIncompleteIgnoreLexemes(character))
                return true;
            
            if (MatchesExistingLexemes(character))
            {
                if (!EndOfStream())
                    return true;
                return TryParseExistingToken();
            }

            if (AnyExistingLexemes())
                if (!TryParseExistingToken())
                    return false;

            if (MatchesNewLexemes(character))
            {
                if (!EndOfStream())
                    return true;
                return TryParseExistingToken();
            }

            if (MatchesExistingIgnoreLexemes(character))
                return true;

            ClearExistingIngoreLexemes();

            return MatchesNewIgnoreLexemes(character);
        }

        private static void RegisterDefaultLexemeFactories(ILexemeFactoryRegistry lexemeFactoryRegistry)
        {
            lexemeFactoryRegistry.Register(new TerminalLexemeFactory());
            lexemeFactoryRegistry.Register(new ParseEngineLexemeFactory());
            lexemeFactoryRegistry.Register(new StringLiteralLexemeFactory());
            lexemeFactoryRegistry.Register(new DfaLexemeFactory());
        }
        private bool AnyExistingLexemes()
        {
            return _existingLexemes.Count > 0;
        }

        private void ClearExistingIngoreLexemes()
        {
            _ignoreLexemes.Clear();
        }
        
        private ILexemeFactory GetLexemeFactory(ILexerRule lexerRule)
        {
            return _lexemeFactoryRegistry
                .Get(lexerRule.LexerRuleType);
        }

        private bool MatchesExistingIgnoreLexemes(char character)
        {
            if (!AnyExistingIngoreLexemes())
                return false;

            var anyMatchedIgnoreLexemes = false;
            foreach (var existingLexeme in _ignoreLexemes)
            {
                if (existingLexeme.Scan(character))
                {
                    anyMatchedIgnoreLexemes = true;
                }
            }
            return anyMatchedIgnoreLexemes;
        }

        private bool AnyExistingIngoreLexemes()
        {
            return _ignoreLexemes.Count != 0;
        }

        private bool MatchesExistingIncompleteIgnoreLexemes(char character)
        {
            if (!AnyExistingIngoreLexemes())
                return false;

            var anyMatchedIgnoreLexemes = false;
            foreach (var existingLexeme in _ignoreLexemes)
            {
                if (!existingLexeme.IsAccepted() && existingLexeme.Scan(character))
                {
                    anyMatchedIgnoreLexemes = true;
                }
            }
            return anyMatchedIgnoreLexemes;
        }

        private bool MatchesExistingLexemes(char character)
        {
            if (!AnyExistingLexemes())
                return false;
            var matchedLexemes = SharedPools.Default<List<ILexeme>>().AllocateAndClear();
            var anyMatchedLexemes = false;
            foreach (var existingLexeme in _existingLexemes)
            {
                if (existingLexeme.Scan(character))
                {
                    matchedLexemes.Add(existingLexeme);
                    anyMatchedLexemes = true;
                }
            }
            if (!anyMatchedLexemes)
                return false;
            SharedPools.Default<List<ILexeme>>().Free(_existingLexemes);
            _existingLexemes = matchedLexemes;
            return true;
        }
        
        private bool MatchesNewIgnoreLexemes(char character)
        {
            if (ParseEngine.Grammar.Ignores.Count == 0)
                return false;

            var ignoreLexerRules = SharedPools.Default<List<ILexerRule>>().AllocateAndClear();
            // PERF: Avoid IEnumerable<T> boxing by calling AddRange
            // PERF: Avoid foreach loop due to non struct boxing
            for (int i = 0; i < ParseEngine.Grammar.Ignores.Count; i++)
            {
                var ignore = ParseEngine.Grammar.Ignores[i];
                ignoreLexerRules.Add(ignore);
            }

            var matchingIgnoreLexemes = SharedPools.Default<List<ILexeme>>().AllocateAndClear();
            var anyMatchingIgnoreLexemes = false;
            foreach (var ignoreLexerRule in ignoreLexerRules)
            {
                var lexemeFactory = GetLexemeFactory(ignoreLexerRule);
                var lexeme = lexemeFactory.Create(ignoreLexerRule);
                if (!lexeme.Scan(character))
                {
                    lexemeFactory.Free(lexeme);
                    continue;
                }
                matchingIgnoreLexemes.Add(lexeme);
                anyMatchingIgnoreLexemes = true;                
            }
            SharedPools.Default<List<ILexerRule>>().Free(ignoreLexerRules);

            if (anyMatchingIgnoreLexemes)
            {
                SharedPools.Default<List<ILexeme>>().Free(_ignoreLexemes);
                _ignoreLexemes = matchingIgnoreLexemes;
                return true;
            }
            return false;
        }

        private bool MatchesNewLexemes(char character)
        {
            var newLexemes = SharedPools.Default<List<ILexeme>>().AllocateAndClear();
            var anyLexemeScanned = false;
            
            var expectedLexerRules = ParseEngine.GetExpectedLexerRules();
            // PERF: Avoid foreach due to boxing IEnumerable<T>

            for (var l = 0; l< expectedLexerRules.Count; l++)
            {
                var lexerRule = expectedLexerRules[l];
                var lexemeFactory = GetLexemeFactory(lexerRule);
                var lexeme = lexemeFactory.Create(lexerRule);
                if (!lexeme.Scan(character))
                {
                    lexemeFactory.Free(lexeme);
                    continue;
                }
                anyLexemeScanned = true;
                newLexemes.Add(lexeme);                
            }

            SharedPools.Default<List<ILexerRule>>().Free(expectedLexerRules);
            expectedLexerRules = null;

            if (!anyLexemeScanned)
                return false;
            SharedPools.Default<List<ILexeme>>().Free(_existingLexemes);
            _existingLexemes = newLexemes;
            return true;
        }
        private char ReadCharacter()
        {
            var character = (char)_textReader.Read();
            Position++;
            return character;
        }

        private bool TryParseExistingToken()
        {
            // PERF: Avoid Linq FirstOrDefault due to lambda allocation
            ILexeme longestAcceptedMatch = null;
            foreach (var lexeme in _existingLexemes)
                if (lexeme.IsAccepted())
                {
                    longestAcceptedMatch = lexeme;
                    break;
                }

            if (longestAcceptedMatch == null)
                return false;

            var token = CreateTokenFromLexeme(longestAcceptedMatch);
            if (token == null)
                return false;

            if (!ParseEngine.Pulse(token))
                return false;

            ClearExistingLexemes();
            return true;
        }
        
        private IToken CreateTokenFromLexeme(ILexeme lexeme)
        {
            var capture = lexeme.Capture;
            return new Token(
                capture,
                Position - capture.Length - 1,
                lexeme.TokenType);
        }
        
        private void ClearExistingLexemes()
        {
            _existingLexemes.Clear();
        }
    }
}