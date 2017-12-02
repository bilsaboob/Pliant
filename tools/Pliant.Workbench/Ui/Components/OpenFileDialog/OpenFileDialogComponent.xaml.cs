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
using Pliant.Workbench.Ui.Controls.Popover;

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

            return dlg;
        }
        
        public OpenFileDialogComponent()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == RootPathProperty)
            {
                if(Directory.Exists(RootPath))
                    fileTreeView.OpenPath(RootPath);
            }
        }

        public event EventHandler Opened;

        public event EventHandler Canceled;

        public string SelectedPath { get; set; }

        public static readonly DependencyProperty RootPathProperty = DependencyProperty.Register(
            "RootPath", typeof(string), typeof(OpenFileDialogComponent), new PropertyMetadata(default(string)));

        public string RootPath
        {
            get { return (string)GetValue(RootPathProperty); }
            set { SetValue(RootPathProperty, value); }
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
