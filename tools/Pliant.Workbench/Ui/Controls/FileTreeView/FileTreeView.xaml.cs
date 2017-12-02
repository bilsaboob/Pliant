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
using Demo.ViewModel;
using RapidFx.Utils;

namespace Pliant.Workbench.Ui.Controls.FileTreeView
{
	/// <summary>
	/// Interaction logic for FileTreeView.xaml
	/// </summary>
	public partial class FileTreeView : UserControl
	{
		public FileTreeView()
		{
			InitializeComponent();
        }

	    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	    {
	        base.OnPropertyChanged(e);

	        if (e.Property == ShowRootProperty)
	        {
	            if (e.NewValue != e.OldValue)
	            {
	                this.treeView.ShowRootNode = (bool)e.NewValue;
	            }
                return;
            }

	        if (e.Property == RootNodeProperty)
	        {
                if (e.NewValue != e.OldValue)
	            {
	                this.treeView.RootNode = e.NewValue as FolderTreeItemViewModel;
	            }
	            return;
	        }
        }

	    public static readonly DependencyProperty ShowRootProperty = DependencyProperty.Register(
	        "ShowRoot", typeof(bool), typeof(FileTreeView), new PropertyMetadata(default(bool)));

	    public bool ShowRoot
	    {
	        get { return (bool) GetValue(ShowRootProperty); }
	        set { SetValue(ShowRootProperty, value); }
	    }

	    public static readonly DependencyProperty RootNodeProperty = DependencyProperty.Register(
	        "RootNode", typeof(FolderTreeItemViewModel), typeof(FileTreeView), new PropertyMetadata(default(FolderTreeItemViewModel)));

	    public FolderTreeItemViewModel RootNode
	    {
	        get { return (FolderTreeItemViewModel) GetValue(RootNodeProperty); }
	        set { SetValue(RootNodeProperty, value); }
	    }

		public bool RootPath(string rootPath)
		{
		    var rootInfo = new DirectoryInfo(rootPath);

            if (!rootInfo.Exists)
				return false;

            RootNode = new FolderTreeItemViewModel(null, rootInfo);
            
            return true;
		}

        public void BringIntoView(TreeItemViewModel node)
        {
            var nodeView = node.TreeViewItem;
            if (nodeView != null)
            {
                var treeView = nodeView.ParentTreeView;
                if (treeView != null)
                {
                    var scrollViewer = treeView.FindChild<ScrollViewer>();
                    if (scrollViewer != null)
                    {
                        scrollViewer.ScrollToBottom();
                    }
                }

                nodeView.BringIntoView();
            }
        }
    }
}
