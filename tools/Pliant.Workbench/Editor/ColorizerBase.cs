using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Workbench.Editor.Services;
using Pliant.Workbench.Parsing;

namespace Pliant.Workbench.Editor
{
    public abstract class ColorizerBase
    {
        public ColorizerBase(ITextMarkerService textMarkerService)
        {
            TextMarkerService = textMarkerService;
        }

        public ITextMarkerService TextMarkerService { get; private set; }

        public ITextMarker MarkError(ParseError error)
        {
            return Mark(error.Line+1, error.Col, 1);
        }

        public ITextMarker Mark(int line, int col, int length)
        {
            return TextMarkerService.Create(line, col, length);
        }

        public void ClearMarkers()
        {
            TextMarkerService.Clear();
        }
    }
}
