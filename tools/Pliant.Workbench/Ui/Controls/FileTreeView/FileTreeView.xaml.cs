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

		public bool OpenPath(string rootPath)
		{
			if (!Directory.Exists(rootPath))
				return false;

			fileTreeView.Root = new FolderNode(rootPath);
			return true;
		}
	}
}
