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

	    public static readonly DependencyProperty ShowRootProperty = DependencyProperty.Register(
	        "ShowRoot", typeof(bool), typeof(FileTreeView), new PropertyMetadata(default(bool), ShowRootChanged));

	    private static void ShowRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	    {
	        if (e.NewValue != e.OldValue)
	        {
	            ((FileTreeView) (d)).treeView.ShowRootNode = (bool)e.NewValue;
	        }
	    }

	    public bool ShowRoot
	    {
	        get { return (bool) GetValue(ShowRootProperty); }
	        set { SetValue(ShowRootProperty, value); }
	    }

		public bool OpenPath(string rootPath)
		{
		    var rootInfo = new DirectoryInfo(rootPath);

            if (!rootInfo.Exists)
				return false;

            treeView.RootNode = new FolderTreeItemViewModel(null, rootInfo);
            
            return true;
		}
	}
}
