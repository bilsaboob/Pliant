using System;
using System.Text;
using System.Threading.Tasks;
using Pliant.Ebnf;
using Pliant.Runtime;

namespace Pliant.Workbench.Parsing
{
    public class GrammarParsing
    {
        private EbnfGrammar _grammar;

        public GrammarParsing()
        {
            _grammar = new EbnfGrammar();
        }

        public void Parse(ParseContext context, Action<ParseContext> onParsedAction)
        {
            return;

            var engine = new ParseEngine(
                _grammar,
                new ParseEngineOptions(loggingEnabled: false)
            );

            // parse the grammar text
            var parseRunner = new ParseRunner(engine, context.Input);
            while (!parseRunner.EndOfStream())
                if (!parseRunner.Read())
                    context.Error(parseRunner.Line, parseRunner.Column);

            if (parseRunner.ParseEngine.IsAccepted())
            {
                // set the result
                context.Result = parseRunner.ParseEngine.GetParseForestRootNode();
            }

            onParsedAction(context);
        }
    }
}
