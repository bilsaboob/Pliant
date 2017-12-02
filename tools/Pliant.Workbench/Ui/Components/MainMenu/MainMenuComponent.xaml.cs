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
using Pliant.Workbench.Ui.Components.MainMenu.MenuContent;

namespace Pliant.Workbench.Ui.Components.MainMenu
{
    /// <summary>
    /// Interaction logic for MainMenuComponent.xaml
    /// </summary>
    public partial class MainMenuComponent : UserControl
    {
        private List<UserControl> _menuContents;

        public MainMenuComponent()
        {
            InitializeComponent();

            _menuContents = new List<UserControl>();

            // ActivateFileExplorer();
			ActivateMainMenu();
        }

        public MainMenuContent MainMenu => _menuContents.FirstOrDefault(c => c is MainMenuContent) as MainMenuContent;

        private void MainMenuItem_OnClick(object sender, RoutedEventArgs e)
	    {
		    ActivateMainMenu();
		}

		private void ActivateMainMenu()
        {
            var menuContent = _menuContents.FirstOrDefault(c => c is MainMenuContent) as MainMenuContent;
            if (menuContent == null)
            {
                menuContent = new MainMenuContent();
                _menuContents.Add(menuContent);
            }
            SetMenuContent(menuContent);
        }

	    private void ExplorerMenuItem_OnClick(object sender, RoutedEventArgs e)
	    {
			ActivateFileExplorer();
		}

		private void ActivateFileExplorer()
        {
            var menuContent = _menuContents.FirstOrDefault(c => c is ExplorerMenuConent) as ExplorerMenuConent;
            if (menuContent == null)
            {
                menuContent = new ExplorerMenuConent();
                _menuContents.Add(menuContent);
            }
            SetMenuContent(menuContent);
        }

	    private void SettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
	    {
		    ActivateSettings();
		}

		private void ActivateSettings()
        {
            var menuContent = _menuContents.FirstOrDefault(c => c is SettingsMenuContent) as SettingsMenuContent;
            if (menuContent == null)
            {
                menuContent = new SettingsMenuContent();
                _menuContents.Add(menuContent);
            }
            SetMenuContent(menuContent);
        }

        private void SetMenuContent(UserControl content)
        {
            content.HorizontalAlignment = HorizontalAlignment.Stretch;
            content.VerticalContentAlignment = VerticalAlignment.Stretch;
            this.menuContentContainer.Content = content;
        }
		
    }
}
