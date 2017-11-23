using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using Pliant.Workbench.Common;
using Pliant.Workbench.Editor;
using Pliant.Workbench.Events;
using Pliant.Workbench.Parsing;
using XamlCSS;
using XamlCSS.WPF;

namespace Pliant.Workbench
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private StyleHotReloading _hotReloading;

        public App()
        {
            InitializeComponent();

            Css.Initialize();

            InitializeStyles();
            InitializeHotReloading();

            GrammarParsing = new GrammarParsing();

            InitializeEvents();
        }

        public GrammarParsing GrammarParsing { get; set; }
        
        private void InitializeStyles()
        {
            var appStyleText = File.ReadAllText("Ui\\Styles\\app.scss");
            (Resources["InternalStyle"] as StyleSheet).Content = appStyleText;
        }

        private void InitializeHotReloading()
        {
            _hotReloading = new StyleHotReloading();
            _hotReloading.Start();
        }

        private void InitializeEvents()
        {
            GrammarTextChangedEvent.On(e => {
                var parseContext = new ParseContext() {Input = e.Document.Text};
                GrammarParsing.Parse(parseContext, (context) => {
                    // parse was finished
                    new ParseErrorsColorizer(e.Editor.Colorizing).Colorize(context);
                    new GrammarColorizer(e.Editor.Colorizing).Colorize(context);
                });
            });
        }
    }
}
