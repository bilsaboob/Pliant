using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using XamlCSS;

namespace Pliant.Workbench.Common
{
    public class StyleHotReloading : IDisposable
    {
        private FileSystemWatcher _watcher;
        
        public StyleHotReloading([CallerFilePath] string appRootPath = null)
        {
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
                    TryLoadAndSetAppStyle(e.FullPath);
                }
            }
        }

        private void TryLoadAndSetAppStyle(string path)
        {
            var maxRetries = 3;
            var retries = 0;
            while (retries < maxRetries)
            {
                try
                {
                    var appStyleText = File.ReadAllText(path);
                    (Application.Current.FindResource("InternalStyle") as StyleSheet).Content = appStyleText;
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
