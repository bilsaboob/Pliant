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
using RapidCSS;
using RapidCSS.CssParsing;
using RapidCSS.WPF;

namespace Pliant.Workbench.Common
{
	public class StylesheetService : IDisposable
	{
		public static readonly StylesheetService Instance = new StylesheetService();

		private bool _hotReloadingEnabled;
		private Dictionary<string, HotReloadingConfig> _hotReloadingConfig;
		private Dictionary<string, StyleSheet> _styleSheets;
		private List<WeakReference<UIElement>> _elements;


		private StylesheetService()
		{
			_hotReloadingConfig = new Dictionary<string, HotReloadingConfig>();
			_elements = new List<WeakReference<UIElement>>();
			_styleSheets = new Dictionary<string, StyleSheet>();
		}

		private IEnumerable<UIElement> ManagedElements
		{
			get
			{
				var elements = _elements.ToArray();
				foreach (var elemRef in elements)
				{
					if (!elemRef.TryGetTarget(out var elem))
					{
						_elements.Remove(elemRef);
						continue;
					}

					yield return elem;
				}
			}
		}

		public bool HotReloadingEnabled
		{
			get => _hotReloadingEnabled;
			set
			{
				_hotReloadingEnabled = true;
				if (value)
				{
					StartAllListeners();
				}
				else
				{
					StopAllListeners();
				}
			}
		}

		private void StartAllListeners()
		{
			foreach (var config in _hotReloadingConfig.Values.ToArray())
			{
				config.StartHotReloading();
			}
		}

		private void StopAllListeners()
		{
			foreach (var config in _hotReloadingConfig.Values.ToArray())
			{
				config.StopHotReloading();
			}
		}

		public void EnableHotReloadingForPath(string pattern = "*.scss", string rootPath = null, bool recursive = false, [CallerFilePath] string basePath = null)
		{
			var path = rootPath;
			if (basePath != null && !rootPath.StartsWith("."))
				path = Path.Combine(Path.GetDirectoryName(basePath), rootPath);

			if (!_hotReloadingConfig.TryGetValue(path, out var config))
			{
				config = new HotReloadingConfig(this);
				_hotReloadingConfig[path] = config;
			}

			config.Path = path;
			config.Pattern = pattern;
			config.Recursive = recursive;

			if (_hotReloadingEnabled)
				config.StartHotReloading();
		}

		public void DisableHotReloadingForPath(string rootPath)
		{
			if (_hotReloadingConfig.TryGetValue(rootPath, out var config))
			{
				config.StopHotReloading();
				_hotReloadingConfig[rootPath] = null;
			}
		}

		public void ManageStyle(UIElement element)
		{
			if(ManagedElements.ToArray().Contains(element))
				return;
			
			_elements.Add(new WeakReference<UIElement>(element));

			TryApplyStyle(element, _styleSheets.Values.FirstOrDefault());
		}

		public void TryLoadAndApplyStyle(string stylesheetPath)
		{
			var styleSheetText = LoadStyle(stylesheetPath);
			if (styleSheetText == null) return;

			var elements = ManagedElements;
			foreach (var elem in elements)
			{
				TryApplyStyle(elem, styleSheetText);
			}
		}

		private StyleSheet LoadStyle(string stylesheetPath)
		{
			var maxRetries = 3;
			var retries = 0;
			while (retries < maxRetries)
			{
				try
				{
					var styleSheetText = File.ReadAllText(stylesheetPath);
					var styleSheet = CssParser.Parse(styleSheetText);
					_styleSheets[stylesheetPath] = styleSheet;
					return styleSheet;
				}
				catch (Exception)
				{
					retries++;
					Thread.Sleep(50);
				}
			}

			return null;
		}

		private void TryApplyStyle(UIElement elem, StyleSheet styleSheet)
		{
			if(elem == null || styleSheet == null)
				return;
			// apply the style to the window
			elem.Dispatcher.Invoke(() => {
				Css.SetStyleSheet(elem, styleSheet);

				//Refresh the element by replacing the content
				var contentControl = elem as ContentControl;
				if (contentControl != null)
				{
					var content = contentControl.Content;
					contentControl.Content = null;
					contentControl.Content = content;
				}
			});
		}

		public void Dispose()
		{
			foreach (var hotReloadingConfig in _hotReloadingConfig.Values)
				hotReloadingConfig.StopHotReloading();

			_elements.Clear();
		}

		class HotReloadingConfig
		{
			private bool _isFirstStart;
			private PathListener _listener;
			private StylesheetService _stylesheetService;

			public HotReloadingConfig(StylesheetService stylesheetService)
			{
				_stylesheetService = stylesheetService;
				_isFirstStart = true;
			}

			public string Path { get; set; }
			public string Pattern { get; set; }
			public bool Recursive { get; set; }
			
			public void StartHotReloading()
			{
				if (_isFirstStart)
				{
					var styleFiles = Directory.GetFiles(Path, Pattern, Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
					foreach (var styleFile in styleFiles)
					{
						if (File.Exists(styleFile))
						{
							_stylesheetService.TryLoadAndApplyStyle(styleFile);
						}
					}
				}

				StopHotReloading();

				_listener = new PathListener(Path, Pattern, true);
				_listener.Change += OnChange;
				_listener.Start();

				_isFirstStart = false;
			}

			public void StopHotReloading()
			{
				if (_listener != null)
				{
					_listener.Stop();
					_listener.Change -= OnChange;
					_listener.Dispose();
					_listener = null;
				}
			}

			private void OnChange(object sender, FileSystemEventArgs e)
			{
				if (File.Exists(e.FullPath))
				{
					_stylesheetService.TryLoadAndApplyStyle(e.FullPath);
				}
			}
		}
	}

	public class PathListener : IDisposable
	{
		private FileSystemWatcher _watcher;

		public PathListener(string path, string pattern = "*.scss", bool recursive = false)
		{
			_watcher = new FileSystemWatcher();
			_watcher.Path = path;
			_watcher.IncludeSubdirectories = recursive;
			_watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
			_watcher.Filter = pattern;

			// Add event handlers.
			_watcher.Changed += OnChanged;
			_watcher.Created += OnChanged;
			_watcher.Deleted += OnChanged;
		}

		public event FileSystemEventHandler Change;

		public void Start()
		{
			_watcher.EnableRaisingEvents = true;
		}

		public void Stop()
		{
			_watcher.EnableRaisingEvents = false;
		}

		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			Change?.Invoke(sender, e);
		}

		public void Dispose()
		{
			_watcher?.Dispose();
		}
	}
}
