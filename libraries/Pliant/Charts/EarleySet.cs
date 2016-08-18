﻿using Pliant.Grammars;
using System.Collections.Generic;

namespace Pliant.Charts
{
    public class EarleySet : IEarleySet
    {
        private StateQueue<INormalState> _predictions;
        private StateQueue<INormalState> _scans;
        private StateQueue<INormalState> _completions;
        private StateQueue<ITransitionState> _transitions;

        public IReadOnlyList<INormalState> Predictions { get { return _predictions; } }

        public IReadOnlyList<INormalState> Scans { get { return _scans; } }

        public IReadOnlyList<INormalState> Completions { get { return _completions; } }

        public IReadOnlyList<ITransitionState> Transitions { get { return _transitions; } }

        public int Location { get; private set; }

        public EarleySet(int location)
        {
            _predictions = new StateQueue<INormalState>();
            _scans = new StateQueue<INormalState>();
            _completions = new StateQueue<INormalState>();
            _transitions = new StateQueue<ITransitionState>();
            Location = location;
        }

        public bool Enqueue(IState state)
        {
            if (state.StateType == StateType.Transitive)
                return EnqueueTransition(state as ITransitionState);
            
            return EnqueueNormal(state, state as INormalState);
        }

        private bool EnqueueNormal(IState state, INormalState normalState)
        {
            if (!state.IsComplete)
            {
                var currentSymbol = state.PostDotSymbol;
                if (currentSymbol.SymbolType == SymbolType.NonTerminal)
                    return _predictions.Enqueue(normalState);
                return _scans.Enqueue(normalState);
            }

            return _completions.Enqueue(normalState);
        }

        private bool EnqueueTransition(ITransitionState transitionState)
        {
            return _transitions.Enqueue(transitionState);
        }

        public ITransitionState FindTransitionState(ISymbol searchSymbol)
        {
            for (int t = 0; t < Transitions.Count; t++)
            {
                var transitionState = Transitions[t] as TransitionState;
                if (transitionState.Recognized.Equals(searchSymbol))
                    return transitionState;
            }
            return null;
        }

        public INormalState FindSourceState(ISymbol searchSymbol)
        {
            // TODO: speed up by using a index lookup
            var sourceItemCount = 0;
            INormalState sourceItem = null;

            for (int s = 0; s < Predictions.Count; s++)
            {
                var state = Predictions[s];
                if (state.IsSource(searchSymbol))
                {
                    var moreThanOneSourceItemExists = sourceItemCount > 0;
                    if (moreThanOneSourceItemExists)
                        return null;
                    sourceItemCount++;
                    sourceItem = state;
                }
            }
            return sourceItem;
        }        
    }
}