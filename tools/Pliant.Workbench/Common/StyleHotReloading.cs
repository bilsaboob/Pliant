using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using XamlCSS;
using XamlCSS.WPF;

namespace Pliant.Workbench.Common
{
    public class StyleHotReloading : IDisposable
    {
        private FileSystemWatcher _watcher;
	    private UIElement _uiElement;
        
        public StyleHotReloading(UIElement uiElement, [CallerFilePath] string appRootPath = null)
        {
	        _uiElement = uiElement;

			AppRootPath = Path.GetDirectoryName(appRootPath);

            InitializeHotReloading();
        }

        public string AppRootPath { get; set; }

        private void InitializeHotReloading()
        {
            _watcher = new FileSystemWatcher();
            _watcher.Path = AppRootPath;
            _watcher.IncludeSubdirectories = true;
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _watcher.Filter = "*.scss";

            // Add event handlers.
            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
            _watcher.Deleted += OnChanged;

            // Begin watching.
            _watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            var fileName = Path.GetFileName(e.FullPath);
            if (fileName == "app.scss")
            {
                if (File.Exists(e.FullPath))
                {
                    TryLoadAndApplyStyle(e.FullPath);
                }
            }
        }
		
        public void TryLoadAndApplyStyle(string path)
        {
            var maxRetries = 3;
            var retries = 0;
            while (retries < maxRetries)
            {
                try
                {
                    var appStyleText = File.ReadAllText(path);

					// apply the style to the window
	                if (_uiElement != null)
	                {
		                _uiElement.Dispatcher.Invoke(() => {
			                var stylesheet = XamlCSS.CssParsing.CssParser.Parse(appStyleText);
			                Css.SetStyleSheet(_uiElement, stylesheet);
			                //Css.instance.UpdateElement(_uiElement);
			                var contentControl = _uiElement as ContentControl;
			                if (contentControl != null)
			                {
				                var content = contentControl.Content;
				                contentControl.Content = null;
				                contentControl.Content = content;
			                }
		                });
	                }

					// also update the internal style if such exists
	                /*var globalStyle = Application.Current.FindResource("InternalStyle") as StyleSheet;
	                if (globalStyle != null)
						globalStyle.Content = appStyleText;*/
					
                    break;
                }
                catch (Exception) { }
                retries++;
                Thread.Sleep(50);
            }
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            _watcher = null;
        }
    }
}
