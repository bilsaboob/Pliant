using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Pliant.Workbench
{
    public partial class MainWindow : Window
    {
        public bool HasOpened { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!HasOpened)
            {
                HasOpened = true;
                mainMenu.MainMenu.ShowOpenFolder();
            }
        }
    }
}
