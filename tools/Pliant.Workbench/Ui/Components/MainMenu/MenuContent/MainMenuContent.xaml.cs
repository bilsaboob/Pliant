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
using Pliant.Workbench.Ui.Components.OpenFileDialog;

namespace Pliant.Workbench.Ui.Components.MainMenu.MenuContent
{
    /// <summary>
    /// Interaction logic for MainMenuContent.xaml
    /// </summary>
    public partial class MainMenuContent : UserControl
    {
        public MainMenuContent()
        {
            InitializeComponent();
        }

	    private void OpenFolderMenuItem_OnClick(object sender, RoutedEventArgs e)
	    {
		    var rootPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		    var dlg = OpenFileDialogComponent.Open(rootPath);
		    if (Directory.Exists(dlg.SelectedPath))
		    {
			    // we have a path to open
		    }
		}

	    private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e)
	    {
			((App)Application.Current).Exit();
		}
    }
}
