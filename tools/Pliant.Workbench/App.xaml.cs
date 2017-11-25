using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
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
			
            InitializeStyles();

            GrammarParsing = new GrammarParsing();

            InitializeEvents();
        }

        public GrammarParsing GrammarParsing { get; set; }

	    private void App_OnStartup(object sender, StartupEventArgs e)
	    {
		    var window = new MainWindow();
		    window.Width = 1024;
		    window.Height = 900;

		    InitializeHotReloading();

		    window.Show();
		}

		private void InitializeStyles()
        {
	        XamlCSS.WPF.Css.Initialize();
		}

	    private void InitializeHotReloading()
        {
			_hotReloading = new StyleHotReloading(MainWindow);
	        _hotReloading.TryLoadAndApplyStyle("Ui\\Styles\\app.scss");

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
