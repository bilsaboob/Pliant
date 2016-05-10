﻿using Pliant.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pliant.Grammars
{
    public class Production : IProduction
    {
        public INonTerminal LeftHandSide { get; private set; }

        private ReadWriteList<ISymbol> _rightHandSide;

        public IReadOnlyList<ISymbol> RightHandSide { get { return _rightHandSide; } }

        public bool IsEmpty { get { return _rightHandSide.Count == 0; } }
        
        public Production(INonTerminal leftHandSide, IEnumerable<ISymbol> rightHandSide)
        {
            Assert.IsNotNull(leftHandSide, nameof(leftHandSide));
            Assert.IsNotNull(rightHandSide, nameof(rightHandSide));
            LeftHandSide = leftHandSide;
            _rightHandSide = new ReadWriteList<ISymbol>(new List<ISymbol>(rightHandSide));
            _hashCode = ComputeHashCode();
        }

        public Production(INonTerminal leftHandSide, params ISymbol[] rightHandSide)
        {
            Assert.IsNotNull(leftHandSide, nameof(leftHandSide));
            Assert.IsNotNull(rightHandSide, nameof(rightHandSide));
            LeftHandSide = leftHandSide;
            _rightHandSide = new ReadWriteList<ISymbol>(new List<ISymbol>(rightHandSide));
            _hashCode = ComputeHashCode();
        }
                
        public override bool Equals(object obj)
        {
            if ((object)obj == null)
                return false;
            var production = obj as Production;
            if ((object)production == null)
                return false;
            if (!LeftHandSide.Equals(production.LeftHandSide))
                return false;
            if (RightHandSide.Count != production.RightHandSide.Count)
                return false;
            for (int i = 0; i < RightHandSide.Count; i++)
                if (!RightHandSide[i].Equals(production.RightHandSide[i]))
                    return false;
            return true;
        }

        // PERF: Cache Costly Hash Code Computation
        private readonly int _hashCode;

        public override int GetHashCode()
        {
            return _hashCode;
        }

        private int ComputeHashCode()
        {
            var hash = HashUtil.ComputeIncrementalHash(LeftHandSide.GetHashCode(), 0, true);
            foreach (var symbol in RightHandSide)
                hash = HashUtil.ComputeIncrementalHash(symbol.GetHashCode(), hash);
            return hash;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0} ->", LeftHandSide.Value);
#pragma warning disable CC0006 // Use foreach
            for (int p = 0; p < RightHandSide.Count; p++)
#pragma warning restore CC0006 // Use foreach
            {
                var symbol = RightHandSide[p];
                stringBuilder.AppendFormat(" {0}", symbol);
            }
            return stringBuilder.ToString();
        }
    }
}