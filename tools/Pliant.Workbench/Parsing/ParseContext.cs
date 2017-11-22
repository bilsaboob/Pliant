using System.Collections.Generic;
using System.Linq;
using Pliant.Forest;
using Pliant.Workbench.Common;

namespace Pliant.Workbench.Parsing
{
    public class ParseContext
    {
        private Dictionary<string, ParseError> _errors;

        public ParseContext()
        {
            _errors = new Dictionary<string, ParseError>();
        }

        public string Input { get; set; }

        public List<ParseError> Errors => _errors.Values.ToList();

        public IInternalForestNode Result { get; set; }

        public void ClearErrors()
        {
            _errors.Clear();
        }

        public void Error(int line, int col, string message = null)
        {
            var error = new ParseError(line, col, message);
            var errorStr = error.ToString();
            _errors[errorStr] = error;
        }
    }

    public class ParseError
    {
        public static implicit operator Position(ParseError error)
        {
            return new Position(error.Line, error.Col);
        }

        public ParseError(int line, int col, string message)
        {
            Line = line;
            Col = col;
            Message = message;
        }

        public int Line { get; set; }
        public int Col { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            var msg = Message ?? "Parse error";
            return $"{Line}:{Col} - {msg}";
        }
    }
}