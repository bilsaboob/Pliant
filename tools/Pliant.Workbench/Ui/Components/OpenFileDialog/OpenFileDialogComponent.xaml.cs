using System;
using System.Collections;
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
using Demo.ViewModel;
using Pliant.Workbench.Ui.Controls.DialogWindow;
using Pliant.Workbench.Ui.Controls.FileTreeView;
using Pliant.Workbench.Ui.Controls.Popover;
using Pliant.Workbench.Utils;
using RapidFx.Utils;

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

            dlg.Focusable = true;
            FocusHelper.FocusCore(dlg);

            return dlg;
        }

        private FileSystemTreeItemViewModel _lastSelectedPathItem;

        public OpenFileDialogComponent()
        {
            InitializeComponent();

            InitializeKeyBindings();
        }

        private void InitializeKeyBindings()
        {
            this.AddKeyBinding("Esc", ()=> Canceled?.Invoke(this, EventArgs.Empty));
            //this.AddKeyBinding("Up", OnKey_Up);
            //this.AddKeyBinding("Down", OnKey_Down);
            //this.AddKeyBinding("Shift+Tab", OnKey_ShiftTab);
            //this.AddKeyBinding("Tab", OnKey_Tab);
        }

        private void OnKey_Up()
        {
            NavigateTreeView(node => {
                fileTreeView.treeView.Selection.SelectPreviousFromItem(node);
            });
        }
        
        private void OnKey_Down()
        {
            NavigateTreeView(node => {
                fileTreeView.treeView.Selection.SelectNextFromItem(node);
            });
        }

        private void OnKey_Tab()
        {
            NavigateTreeView(node => {
                if (node.CanExpand)
                {
                    if (!node.IsExpanded)
                    {
                        var selectNode = node;
                        node.IsExpanded = true;
                        var model = node.DataContext as TreeItemViewModel;
                        if (model != null)
                        {
                            var child = model.Children.FirstOrDefault();
                            if (child != null && child.TreeViewItem != null)
                            {
                                selectNode = child.TreeViewItem;
                            }
                        }
                        
                        fileTreeView.treeView.Selection.Select(selectNode);
                    }
                }
            });
        }

        private void OnKey_ShiftTab()
        {
            NavigateTreeView(node => {
                var parentNode = node.Parent as MultiSelectTreeViewItem;
                if (parentNode != null)
                {
                    if (parentNode.IsExpanded)
                    {
                        parentNode.IsExpanded = false;
                        fileTreeView.treeView.Selection.Select(parentNode);
                    }
                }
            });
        }

        private void NavigateTreeView(Action<MultiSelectTreeViewItem> navigateAction)
        {
            var refocus = pathTextBox.IsFocused || pathTextBox.textBox.IsFocused;

            if (!this.fileTreeView.IsFocused)
            {
                MultiSelectTreeViewItem selectedNode = null;
                if (_lastSelectedPathItem != null)
                {
                    selectedNode = _lastSelectedPathItem.TreeViewItem;
                }

                if (selectedNode == null)
                {
                    var selectionCount = this.fileTreeView.treeView.SelectedItems.Count;
                    if (selectionCount > 0)
                    {
                        var selectedItem = this.fileTreeView.treeView.SelectedItems[selectionCount - 1] as ITreeItemViewModel;
                        if (selectedItem != null)
                        {
                            selectedNode = selectedItem.TreeViewItem;
                        }
                    }
                }

                navigateAction(selectedNode);
            }

            if (refocus)
            {
                FocusHelper.FocusCore(pathTextBox.textBox);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == RootPathProperty)
            {
                var newRootPath = (string)e.NewValue;

                if (e.ValueChanged())
                {
                    if (Directory.Exists(newRootPath))
                    {
                        fileTreeView.RootPath(newRootPath);

                        // new root, so let's expand it immediately
                        fileTreeView.RootNode.IsExpanded = true;

                        // Update the selected path if typed path is set
                        /*if(!string.IsNullOrEmpty(TypedPath))
                            TryUpdateSelectedPath(TypedPath);*/
                    }
                }
                return;
            }

            if (e.Property == TypedPathProperty)
            {
                var typedText = e.NewValue as string;
                if (typedText != null)
                {
                    var displayText = typedText;
                    if (typedText.StartsWithDiskPath(out var diskPath))
                    {
                        //Update the root
                        var newRootPath = diskPath.AsDiskPath();
                        if (newRootPath != RootPath)
                            RootPath = newRootPath;

                        displayText = typedText.TrimDiskPath();
                    }

                    TryUpdateSelectedPath(displayText);
                }
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
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(RootPath))
                return;

            // prep the paths
            var rootPath = RootPath;
            var cleanRootPath = rootPath.CleanPath();
            var cleanPath = path.CleanPath(); // relative to the root
            
            // split the paht into parts
            var pathParts = cleanPath.SplitPath();
            if(pathParts.Count == 0)
                return;
            
            // collect the nodes for the path
            var pathNodes = CollectPathNodes(pathParts, out var isPathComplete);

            // expand the full path
            ExpandPath(pathNodes);

            // Update the selected path only if the path is complete
            SetSelectedPath(cleanRootPath.CombinePath(path), pathNodes.LastOrDefault());
        }

        private void SetSelectedPath(string path, FileSystemTreeItemViewModel newSelectedItem)
        {
            SelectedPath = path;
            
            var selectedItemChanged = _lastSelectedPathItem != newSelectedItem;

            // deselect the previous item
            if (_lastSelectedPathItem != null && selectedItemChanged)
            {
                _lastSelectedPathItem.IsSelected = false;

                // deselect / collapse the previously opened path (that doesn't overlap the new path)
                DeselectAndCollapsePreviousPath(
                    _lastSelectedPathItem.Root as FileSystemTreeItemViewModel,
                    _lastSelectedPathItem.FullPath.CleanPath(),
                    path.CleanPath()
                );
            }
            
            _lastSelectedPathItem = newSelectedItem;

            // select the new item
            if (_lastSelectedPathItem != null)
            {
                _lastSelectedPathItem.IsSelected = true;
                fileTreeView.BringIntoView(_lastSelectedPathItem);
            }
        }

        private void ExpandPath(IEnumerable<FileSystemTreeItemViewModel> pathNodes)
        {
            foreach (var pathTreeElem in pathNodes)
            {
                pathTreeElem.IsExpanded = true;
            }
        }

        private List<FileSystemTreeItemViewModel> CollectPathNodes(List<string> pathParts, out bool isPathComplete)
        {
            isPathComplete = false;
            var nodes = new List<FileSystemTreeItemViewModel>();

            var currentNode = fileTreeView.RootNode;
            if (currentNode == null)
                return nodes;

            isPathComplete = true;

            foreach (var pathPart in pathParts)
            {
                var pathChild = currentNode.Children.OfType<FileSystemTreeItemViewModel>().FirstOrDefault(c => string.Equals(c.Info.Name, pathPart));
                if (pathChild == null)
                {
                    isPathComplete = false;
                    break;
                }

                currentNode = pathChild as FolderTreeItemViewModel;
                if (currentNode == null)
                    break;

                nodes.Add(pathChild);
            }

            return nodes;
        }

        private void DeselectAndCollapsePreviousPath(FileSystemTreeItemViewModel root, string prevPath, string newPath)
        {
            var newPathParts = newPath.SplitPath();
            var prevPathParts = prevPath.SplitPath();

            var node = root;

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

                if (i == 0)
                {
                    // check directly with the node name
                    var nodeName = node.Name;
                    if (node.Name.StartsWithDiskPath(out var diskPath))
                        nodeName = diskPath;

                    if (!string.Equals(newPartName, nodeName))
                        break;
                }
                else
                {
                    // check for a child
                    node = node.Children.OfType<FileSystemTreeItemViewModel>().FirstOrDefault(c => string.Equals(newPartName, c.Name));
                    if (node == null)
                        break;
                }
                
                i++;
            }

            if(node == null)
                return;

            // The following paths should be cleaned
            for (; i < prevPathParts.Count; ++i)
            {
                var prevPart = prevPathParts[i];
                var nextElem = node.Children.OfType<FileSystemTreeItemViewModel>().FirstOrDefault(c => string.Equals(prevPart, c.Name));

                if(nextElem == null)
                    break;

                nextElem.IsExpanded = false;
                nextElem.IsSelected = false;

                node = nextElem;
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
