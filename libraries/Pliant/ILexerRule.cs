﻿using Pliant.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pliant
{
    public interface ILexerRule : ISymbol
    {
        IGrammar Grammar { get; }
        TokenType TokenType { get; }
    }
}
