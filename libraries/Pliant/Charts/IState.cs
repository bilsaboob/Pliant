﻿using Pliant.Forest;
using Pliant.Grammars;
using System.Collections.Generic;

namespace Pliant.Charts
{
    public interface IState
    {
        IProduction Production { get; }

        int Origin { get; }

        StateType StateType { get; }

        ISymbol PreDotSymbol { get; }

        ISymbol PostDotSymbol { get; }

        int Position { get; }

        bool IsComplete { get; }
                
        IForestNode ParseNode { get; set; }
    }
}