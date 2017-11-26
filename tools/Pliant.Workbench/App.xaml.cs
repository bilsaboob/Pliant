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
using RapidCSS.WPF;

namespace Pliant.Workbench
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
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
	        Css.Initialize();
		}

	    private void InitializeHotReloading()
        {
			StylesheetService.Instance.EnableHotReloadingForPath("*.scss", "Ui\\Styles", true);

			StylesheetService.Instance.ManageStyle(MainWindow);

			StylesheetService.Instance.HotReloadingEnabled = true;
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

	    public void Exit()
	    {
			MainWindow.Close();
	    }
    }
}
