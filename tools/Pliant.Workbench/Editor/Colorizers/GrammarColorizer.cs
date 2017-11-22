using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Forest;
using Pliant.Grammars;
using Pliant.Tokens;
using Pliant.Workbench.Editor.Services;
using Pliant.Workbench.Parsing;

namespace Pliant.Workbench.Editor
{
    public class GrammarColorizer : ColorizerBase
    {
        public GrammarColorizer(ITextMarkerService textMarkerService)
            : base(textMarkerService)
        {
        }

        public void Colorize(ParseContext context)
        {
            var rootNode = context.Result;
            if(rootNode == null) return;

            var colorizingVisitor = new GrammarColorizingVisitor(this, new SelectFirstChildDisambiguationAlgorithm());
            rootNode.Accept(colorizingVisitor);
        }
    }

    public class GrammarColorizingVisitor : DisambiguatingForestNodeVisitorBase
    {
        private ColorizerBase _colorizer;

        public GrammarColorizingVisitor(ColorizerBase colorizer, IForestDisambiguationAlgorithm stateManager)
            : base(stateManager)
        {
            _colorizer = colorizer;
        }

        private void ColorizeToken(IToken token)
        {
            // Colorize token?
        }

        private void ColorizeSymbol(ISymbolForestNode symbolNode)
        {
            var t = symbolNode.Symbol.ToString();
            if (t == "Ebnf.Definition")
            {
                // Colorize rule definition name
                //_colorizer.Mark(symbolNode.)
                return;
            }

            if (t == "Ebnf.LexerRegex")
            {
                // Colorize regex symbol
                return;
            }
        }

        #region Visit helpers
        public override void Visit(IIntermediateForestNode intermediateNode)
        {
            foreach (var child in intermediateNode.Children)
                Visit(child);
        }

        public override void Visit(ITokenForestNode tokenNode)
        {
            ColorizeToken(tokenNode.Token);
        }
        
        public override void Visit(ISymbolForestNode symbolNode)
        {
            ColorizeSymbol(symbolNode);

            foreach (var child in symbolNode.Children)
                Visit(child);
        }
        
        public override void Visit(ITerminalForestNode terminalNode)
        {
        }

        public override void Visit(IAndForestNode andNode)
        {
            foreach (var child in andNode.Children)
                child.Accept(this);
        }
        #endregion
    }
}
