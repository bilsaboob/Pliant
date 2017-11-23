using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;
using Pliant.Workbench.Common;
using GrammarEditor = Pliant.Workbench.Ui.Controls.GrammarEditor;

namespace Pliant.Workbench.Events
{
    public class GrammarTextChangedEvent : EventBase<GrammarTextChangedEvent>
    {
        public GrammarEditor Editor { get; set; }
        public DocumentChangeEventArgs Change { get; set; }
        public TextDocument Document { get; set; }
    }
}
