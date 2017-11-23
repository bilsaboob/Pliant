using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using Pliant.Workbench.Editor.Services;
using Pliant.Workbench.Events;

namespace Pliant.Workbench.Ui.Controls
{
    /// <summary>
    /// Interaction logic for GrammarEditor.xaml
    /// </summary>
    public partial class GrammarEditor : UserControl
    {
        private TextMarkerService _textMarkerService;
        private TextDocument _document;

        public GrammarEditor()
        {
            InitializeComponent();

            InitDocument(grammarTextEditor.Document);

            InitGrammarEditor(grammarTextEditor);
        }

        public TextMarkerService Colorizing => _textMarkerService;

        private void InitGrammarEditor(TextEditor editor)
        {
            var lineNoMargin = editor.TextArea.LeftMargins.FirstOrDefault(m => m is LineNumberMargin) as LineNumberMargin;
            if (lineNoMargin != null)
                InitEditorLineNumberMargin(lineNoMargin);

            InitEditorTextView(editor.TextArea.TextView);
        }
        
        private void InitEditorLineNumberMargin(LineNumberMargin lineNoMargin)
        {
            lineNoMargin.MinWidth = 30;
            lineNoMargin.Margin = new Thickness(5, 0, 5, 0);
        }

        private void InitEditorTextView(TextView textView)
        {
            textView.Margin = new Thickness(10, 0, 0, 5);
        }
        
        private void InitDocument(TextDocument document)
        {
            if (_document != document)
            {
                // Clear old text marker service from the transformers on text view
                if (_textMarkerService != null)
                {
                    grammarTextEditor.TextArea.TextView.LineTransformers.Remove(_textMarkerService);
                    grammarTextEditor.TextArea.TextView.BackgroundRenderers.Remove(_textMarkerService);
                }

                if (_document != null)
                    document.Changed -= OnDocumentTextChanged;

                // set the new document and init the new text marker service
                _document = document;
                _textMarkerService = new TextMarkerService(_document);

                // set the text marker service
                grammarTextEditor.TextArea.TextView.LineTransformers.Add(_textMarkerService);
                grammarTextEditor.TextArea.TextView.BackgroundRenderers.Add(_textMarkerService);

                document.Changed += OnDocumentTextChanged;
            }
        }

        #region Document event handlers
        private void OnDocumentTextChanged(object sender, DocumentChangeEventArgs e)
        {
            //Trigger a reparse of the application
            new GrammarTextChangedEvent() {Editor = this, Change = e, Document = grammarTextEditor.Document}.Invoke();
        }
        #endregion

        #region Gui event handlers
        private void GrammarTextEditor_OnDocumentChanged(object sender, EventArgs e)
        {
            InitDocument(grammarTextEditor.Document);
        }
        #endregion
    }
}
