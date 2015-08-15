﻿using Pliant.Automata;
using Pliant.Grammars;
using Pliant.Tokens;
using System.Collections.Generic;

namespace Pliant.Bnf
{
    public class BnfGrammar : IGrammar
    {
        private static readonly IGrammar _bnfGrammar;

        static BnfGrammar()
        {
            /* 
             *  <grammar>        ::= <rule> | <rule> <grammar>
             *  <rule>           ::= "<" <rule-name> ">" "::=" <expression>
             *  <expression>     ::= <list> | <list> "|" <expression>
             *  <line-end>       ::= <EOL> | <line-end> <line-end>
             *  <list>           ::= <term> | <term> <list>
             *  <term>           ::= <literal> | "<" <rule-name> ">"
             *  <literal>        ::= '"' <text> '"' | "'" <text> "'"
             */
            var whitespace = CreateWhitespaceLexerRule();
            var ruleName = CreateRuleNameLexerRule();
            var implements = CreateImplementsLexerRule();
            var eol = CreateEndOfLineLexerRule();
            var notDoubleQuote = CreateNotDoubleQuoteLexerRule();
            var notSingleQuuote = CreateNotSingleQuoteLexerRule();

            var grammar = new NonTerminal("grammar");
            var rule = new NonTerminal("rule");
            var identifier = new NonTerminal("identifier");
            var expression = new NonTerminal("expression");
            var lineEnd = new NonTerminal("line-end");
            var list = new NonTerminal("list");
            var term = new NonTerminal("term");
            var literal = new NonTerminal("literal");
            var doubleQuoteText = new NonTerminal("doubleQuoteText");
            var singleQuoteText = new NonTerminal("singleQuoteText");

            var productions = new[]
            {
                new Production(grammar, rule),
                new Production(grammar, rule, grammar),
                new Production(rule, identifier, implements, expression),
                new Production(expression, list),
                new Production(expression, list, new TerminalLexerRule('|'), expression),
                new Production(lineEnd, eol),
                new Production(lineEnd, lineEnd, lineEnd),
                new Production(list, term),
                new Production(list, term, list),
                new Production(term, literal),
                new Production(term, identifier),
                new Production(identifier, new TerminalLexerRule('<'), ruleName, new TerminalLexerRule('>')),
                new Production(literal, new TerminalLexerRule('"'), notDoubleQuote, new TerminalLexerRule('"')),
                new Production(literal, new TerminalLexerRule('\''), notSingleQuuote, new TerminalLexerRule('\''))
            };

            var ignore = new[]
            {
                whitespace
            };

            _bnfGrammar = new Grammar(grammar, productions, ignore);
        }

        private static ILexerRule CreateNotSingleQuoteLexerRule()
        {
            var start = new DfaState();
            var final = new DfaState(true);
            var terminal = new NegationTerminal(new Terminal('\''));
            var edge = new DfaTransition(terminal, final);
            start.AddTransition(edge);
            final.AddTransition(edge);
            return new DfaLexerRule(start, new TokenType("not-single-quote"));
        }

        private static ILexerRule CreateNotDoubleQuoteLexerRule()
        {
            // ( [^"\\] | (\\ .) ) +
            var start = new DfaState();
            var escape = new DfaState();
            var final = new DfaState(true);

            var notQuoteTerminal = new NegationTerminal(
                new SetTerminal('"', '\\'));
            var escapeTerminal = new Terminal('\\');
            var anyTerminal = new AnyTerminal();

            var notQuoteEdge = new DfaTransition(notQuoteTerminal, final);
            start.AddTransition(notQuoteEdge);
            final.AddTransition(notQuoteEdge);

            var escapeEdge = new DfaTransition(escapeTerminal, escape);
            start.AddTransition(escapeEdge);
            final.AddTransition(escapeEdge);

            var anyEdge = new DfaTransition(anyTerminal, final);
            escape.AddTransition(anyEdge);

            return new DfaLexerRule(start, new TokenType("not-double-quote"));            
        }

        private static ILexerRule CreateEndOfLineLexerRule()
        {
            return new StringLiteralLexerRule("\r\n", new TokenType("eol"));
        }

        private static ILexerRule CreateImplementsLexerRule()
        {
            return new StringLiteralLexerRule("::=", new TokenType("implements"));
        }

        private static ILexerRule CreateRuleNameLexerRule()
        {
            var ruleNameState = new DfaState();
            var zeroOrMoreLetterOrDigit = new DfaState(true);
            ruleNameState.AddTransition(
                new DfaTransition(
                    new CharacterClassTerminal(
                        new RangeTerminal('a', 'z'),
                        new RangeTerminal('A', 'Z')),
                    zeroOrMoreLetterOrDigit));
            zeroOrMoreLetterOrDigit.AddTransition(
                new DfaTransition(
                    new CharacterClassTerminal(
                        new RangeTerminal('a', 'z'),
                        new RangeTerminal('A', 'Z'),
                        new DigitTerminal(),
                        new SetTerminal('-', '_')),
                    zeroOrMoreLetterOrDigit));
            var ruleName = new DfaLexerRule(ruleNameState, new TokenType("rule-name"));
            return ruleName;
        }

        private static ILexerRule CreateWhitespaceLexerRule()
        {
            var whitespaceTerminal = new WhitespaceTerminal();
            var startWhitespace = new DfaState();
            var finalWhitespace = new DfaState(true);
            startWhitespace.AddTransition(new DfaTransition(whitespaceTerminal, finalWhitespace));
            finalWhitespace.AddTransition(new DfaTransition(whitespaceTerminal, finalWhitespace));
            var whitespace = new DfaLexerRule(startWhitespace, new TokenType("whitespace"));
            return whitespace;
        }

        public IReadOnlyList<IProduction> Productions
        {
            get { return _bnfGrammar.Productions; }
        }

        public INonTerminal Start
        {
            get { return _bnfGrammar.Start; }
        }

        public IReadOnlyList<ILexerRule> Ignores
        {
            get { return _bnfGrammar.Ignores; }
        }
        
        public IEnumerable<IProduction> RulesFor(INonTerminal nonTerminal)
        {
            return _bnfGrammar.RulesFor(nonTerminal);
        }
        
        public IEnumerable<IProduction> StartProductions()
        {
            return _bnfGrammar.StartProductions();
        }
    }
}