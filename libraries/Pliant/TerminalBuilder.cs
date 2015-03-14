﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pliant
{
    public class TerminalBuilder : ITerminalBuilder
    {
        private IList<ITerminal> _terminals;
        public TerminalBuilder()
        {
            _terminals = new List<ITerminal>();
        }


        public IList<ITerminal> GetTerminals() 
        { 
            return _terminals; 
        }

        public ITerminalBuilder Range(char start, char end)
        {
            var rangeTerminal = new RangeTerminal(start, end);
            _terminals.Add(rangeTerminal);
            return this;
        }

        public ITerminalBuilder Digit()
        {
            var digitTerminal = new DigitTerminal();
            _terminals.Add(digitTerminal);
            return this;
        }

        public ITerminalBuilder WhiteSpace()
        {
            var whitespaceTerminal = new WhitespaceTerminal();
            _terminals.Add(whitespaceTerminal);
            return this;
        }
    }
}