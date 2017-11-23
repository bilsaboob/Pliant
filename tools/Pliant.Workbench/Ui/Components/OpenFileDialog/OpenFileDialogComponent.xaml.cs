using System;
using System.Collections.Generic;
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

namespace Pliant.Workbench.Ui.Components.OpenFileDialog
{
    /// <summary>
    /// Interaction logic for OpenFileDialogComponent.xaml
    /// </summary>
    public partial class OpenFileDialogComponent : UserControl
    {
        public static OpenFileDialogComponent Open(string rootPath = null)
        {
            var dlg = new OpenFileDialogComponent();
            dlg.RootPath = rootPath;

            var wnd = DialogWindow.Open(dlg);

            return dlg;
        }

        private string _rootPath;

        public OpenFileDialogComponent()
        {
            InitializeComponent();
        }

        public string SelectedPath { get; set; }

        public string RootPath
        {
            get => _rootPath;
            set => fileTreeView.OpenPath(value);
        }
    }
}
