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
using System.Windows.Shapes;

namespace Pliant.Workbench.Ui.Components.OpenFileDialog
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public static DialogWindow Open(FrameworkElement content)
        {
            var wnd = new DialogWindow();

            content.HorizontalAlignment = HorizontalAlignment.Stretch;
            content.VerticalAlignment = VerticalAlignment.Stretch;

            wnd.dialogContent.Content = content;

            wnd.ShowDialog();

            return wnd;
        }

        public DialogWindow()
        {
            InitializeComponent();
        }
    }
}
