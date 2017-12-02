using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Pliant.Workbench
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            mainMenu.MainMenu.ShowOpenFolder();
        }
    }
}
