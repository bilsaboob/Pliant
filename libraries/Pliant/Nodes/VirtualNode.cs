﻿using Pliant.Charts;
using Pliant.Collections;
using System.Collections.Generic;
using Pliant.Grammars;
using System;

namespace Pliant.Nodes
{
    public class VirtualNode : ISymbolNode
    {
        private ITransitionState _transitionState;
        private IState _completed;
        private ReadWriteList<IAndNode> _children;
        /// <summary>
        /// A single AND node. Virtual nodes are leo nodes and by nature don't have ambiguity.
        /// </summary>
        private AndNode _andNode;

        public int Location { get; private set; }
        
        public VirtualNode(int location, ITransitionState transitionState, IState completed)
        {
            _transitionState = transitionState;
            _completed = completed;
            _children = new ReadWriteList<IAndNode>();
            Location = location;
        }

        public int Origin
        {
            get { return _transitionState.Origin; }
        }
             
        public NodeType NodeType
        {
            get { return NodeType.Virtual; }
        }
        
        public IReadOnlyList<IAndNode> Children
        {
            get 
            {
                if(!ResultCached())
                    LazyLoadChildren();
                return _children; 
            }
        }

        public ISymbol Symbol
        {
            get
            {
                return _transitionState.Recognized;
            }
        }

        private void LazyLoadChildren()
        {
            if (_transitionState.NextTransition != null)
            {
                var virtualNode = new VirtualNode(Location, _transitionState.NextTransition, _completed);
                if (_transitionState.Reduction.ParseNode == null)
                    AddUniqueFamily(virtualNode);
                else
                    AddUniqueFamily(_transitionState.Reduction.ParseNode, virtualNode);
            }
            else if (_transitionState.Reduction.ParseNode != null)
            {
                AddUniqueFamily(_transitionState.Reduction.ParseNode, _completed.ParseNode);
            }
            else
            {
                AddUniqueFamily(_completed.ParseNode);
            }            
        }

        private bool ResultCached()
        {
            return _andNode != null;
        }

        public void AddUniqueFamily(INode trigger)
        {
            if (_andNode != null)
                return;
            _andNode = new AndNode();
            _andNode.AddChild(trigger);
            _children.Add(_andNode);
        }

        public void AddUniqueFamily(INode source, INode trigger)
        {
            if (_andNode != null)
                return;
            _andNode = new AndNode();
            _andNode.AddChild(source);
            _andNode.AddChild(trigger);
            _children.Add(_andNode);
        }
        
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", Symbol, Origin, Location);
        }
    }
}
