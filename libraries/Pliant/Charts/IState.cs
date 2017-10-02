﻿using Pliant.Forest;
using Pliant.Grammars;
using System.Collections.Generic;

namespace Pliant.Charts
{
    public interface IState
    {
        IDottedRule DottedRule { get; }

        int Origin { get; }

        StateType StateType { get; }        
                
        IForestNode ParseNode { get; set; }
    }
}