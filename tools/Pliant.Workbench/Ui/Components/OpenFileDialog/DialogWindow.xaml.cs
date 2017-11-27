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
using Pliant.Workbench.Common;

namespace Pliant.Workbench.Ui.Components.OpenFileDialog
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public static DialogWindow OpenCentered(FrameworkElement content, string title = "")
        {
            var activeWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            var parentWidth = activeWindow.Width;
            var parentHeight = activeWindow.Height;

            var wnd = CreateDialogWindow(content, title);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wnd.SizeToContent = SizeToContent.WidthAndHeight;

            wnd.MaxWidth = parentWidth;
            wnd.MaxHeight = parentHeight;

            wnd.ShowDialog();

            return wnd;
        }
        

        public static DialogWindow Open(FrameworkElement content, string title = "")
        {
            var wnd = CreateDialogWindow(content, title);

			wnd.ShowDialog();

            return wnd;
        }

        private static DialogWindow CreateDialogWindow(FrameworkElement content, string title)
        {
            var wnd = new DialogWindow();
            wnd.Owner = Application.Current.MainWindow;

            wnd.Title = title ?? "";

            content.HorizontalAlignment = HorizontalAlignment.Stretch;
            content.VerticalAlignment = VerticalAlignment.Stretch;

            wnd.dialogContent.Content = content;

            StylesheetService.Instance.ManageStyle(wnd);

            return wnd;
        }

        public DialogWindow()
        {
            InitializeComponent();
		}
    }
}
