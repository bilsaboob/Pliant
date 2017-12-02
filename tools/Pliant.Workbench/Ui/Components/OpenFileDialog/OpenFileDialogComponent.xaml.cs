using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Pliant.Workbench.Ui.Controls.DialogWindow;
using Pliant.Workbench.Ui.Controls.FileTreeView;
using Pliant.Workbench.Ui.Controls.Popover;
using Pliant.Workbench.Utils;

namespace Pliant.Workbench.Ui.Components.OpenFileDialog
{
    /// <summary>
    /// Interaction logic for OpenFileDialogComponent.xaml
    /// </summary>
    public partial class OpenFileDialogComponent : UserControl
    {
        public static OpenFileDialogComponent Open(string rootPath, string title)
        {
            var dlg = new OpenFileDialogComponent();
            dlg.RootPath = rootPath;
            dlg.TypedPath = rootPath;
            dlg.Height = 300;

            var popverWindow = PopoverWindow.Create(dlg, wnd => wnd.Title = title)
                .Size(800, 600)
                .FitToWindow()
                .AtLocation(WindowStartupLocation.CenterOwner)
                .Show();

            dlg.Opened += (sender, args) => {
                popverWindow.Close();
            };

            dlg.Canceled += (sender, args) => {
                popverWindow.Close();
            };

            return dlg;
        }

        private FileSystemTreeItemViewModel _lastSelectedPathItem;

        public OpenFileDialogComponent()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == RootPathProperty)
            {
                if (Directory.Exists(RootPath))
                {
                    fileTreeView.RootPath(RootPath);

                    // Update the selected path if typed path is set
                    if(!string.IsNullOrEmpty(TypedPath))
                        TryUpdateSelectedPath(TypedPath);
                }
                return;
            }

            if (e.Property == TypedPathProperty)
            {
                TryUpdateSelectedPath(e.NewValue as string);
            }
        }

        public event EventHandler Opened;

        public event EventHandler Canceled;

        public static readonly DependencyProperty RootPathProperty = DependencyProperty.Register(
            "RootPath", typeof(string), typeof(OpenFileDialogComponent), new PropertyMetadata(default(string)));

        public string RootPath
        {
            get { return (string)GetValue(RootPathProperty); }
            set { SetValue(RootPathProperty, value); }
        }

        public static readonly DependencyProperty TypedPathProperty = DependencyProperty.Register(
            "TypedPath", typeof(string), typeof(OpenFileDialogComponent), new PropertyMetadata(default(string)));

        public string TypedPath
        {
            get { return (string) GetValue(TypedPathProperty); }
            set { SetValue(TypedPathProperty, value); }
        }

        public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register(
            "SelectedPath", typeof(string), typeof(OpenFileDialogComponent), new PropertyMetadata(default(string)));

        public string SelectedPath
        {
            get { return (string) GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        private void TryUpdateSelectedPath(string path)
        {
            if(string.IsNullOrEmpty(path))
                return;

            var rootPath = RootPath;
            if(string.IsNullOrEmpty(rootPath))
                return;

            var cleanPath = path.CleanPath();
            var cleanRootPath = (rootPath + "\\").CleanPath();

            if (!cleanPath.StartsWith(cleanRootPath))
                return;

            var startPath = cleanPath.Substring(cleanRootPath.Length);

            var paths = startPath.Trim('/', '\\').SplitPath();
            if(paths.Count == 0)
                return;

            paths.Reverse();
            
            var currentTreeElem = fileTreeView.RootNode;
            if(currentTreeElem == null)
                return;

            var isPathComplete = true;

            var pathTreeElements = new List<FileSystemTreeItemViewModel>();
            foreach (var pathPart in paths)
            {
                var pathChild = currentTreeElem.Children.OfType<FileSystemTreeItemViewModel>().FirstOrDefault(c => string.Equals(c.Info.Name, pathPart));
                if (pathChild == null)
                {
                    isPathComplete = false;
                    break;
                }
                
                currentTreeElem = pathChild as FolderTreeItemViewModel;
                if(currentTreeElem == null)
                    break;

                pathTreeElements.Add(pathChild);
            }

            // Expand the complete path
            foreach (var pathTreeElem in pathTreeElements)
            {
                pathTreeElem.IsExpanded = true;
            }

            // Update the selected path only if the path is complete
            if (isPathComplete)
            {
                SelectedPath = path;

                var newSelectedItem = pathTreeElements.Last();
                var selectedItemChanged = _lastSelectedPathItem != newSelectedItem;

                // deselect the previous item
                if (_lastSelectedPathItem != null && selectedItemChanged)
                {
                    _lastSelectedPathItem.IsSelected = false;

                    DeselectAndCollapsePreviousPath(
                        _lastSelectedPathItem.Root as FileSystemTreeItemViewModel, 
                        _lastSelectedPathItem.FullPath.CleanPath().Replace(cleanRootPath, ""), 
                        cleanPath.Replace(cleanRootPath, ""));
                }

                _lastSelectedPathItem = newSelectedItem;

                // select the new item
                if (_lastSelectedPathItem != null)
                {
                    _lastSelectedPathItem.IsSelected = true;
                }
            }
        }

        private void DeselectAndCollapsePreviousPath(FileSystemTreeItemViewModel root, string prevPath, string newPath)
        {
            var newPathParts = newPath.SplitPath();
            newPathParts.Reverse();

            var prevPathParts = prevPath.SplitPath();
            prevPathParts.Reverse();

            var prevElem = root;

            var i = 0;
            while (true)
            {
                if (i >= prevPathParts.Count)
                    break;

                if (i >= newPathParts.Count)
                    break;

                var newPartName = newPathParts[i];
                if (!string.Equals(newPartName, prevPathParts[i]))
                    break;

                prevElem = prevElem.Children.OfType<FileSystemTreeItemViewModel>().FirstOrDefault(c => string.Equals(newPartName, c.Name));
                if(prevElem == null)
                    break;

                i++;
            }

            if(prevElem == null)
                return;

            // The following paths should be cleaned
            for (; i < prevPathParts.Count; ++i)
            {
                var prevPart = prevPathParts[i];
                var nextElem = prevElem.Children.OfType<FileSystemTreeItemViewModel>().FirstOrDefault(c => string.Equals(prevPart, c.Name));

                if(nextElem == null)
                    break;

                nextElem.IsExpanded = false;
                nextElem.IsSelected = false;

                prevElem = nextElem;
            }
        }

        private void Open_OnClick(object sender, RoutedEventArgs e)
        {
            Opened?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Canceled?.Invoke(this, EventArgs.Empty);
        }
    }
}
