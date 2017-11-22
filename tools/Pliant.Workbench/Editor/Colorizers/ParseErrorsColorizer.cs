using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;
using Pliant.Workbench.Common;
using Pliant.Workbench.Editor.Services;
using Pliant.Workbench.Parsing;

namespace Pliant.Workbench.Editor
{
    public class ParseErrorsColorizer : ColorizerBase
    {
        public ParseErrorsColorizer(ITextMarkerService textMarkerService)
            : base(textMarkerService)
        {
        }

        public void Colorize(ParseContext context)
        {
            ClearMarkers();

            foreach (var error in context.Errors)
            {
                MarkError(error)
                    .Type(TextMarkerTypes.SquigglyUnderline)
                    .ForegroundColor(Color.FromRgb(255, 0, 0))
                    .MarkerColor(Color.FromRgb(255, 0, 0));
            }
        }
    }
}
