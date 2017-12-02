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
using Pliant.Workbench.Ui.Controls.DialogWindow;
using Pliant.Workbench.Utils;
using RapidFx.Utils;

namespace Pliant.Workbench.Ui.Controls.Popover
{
    /// <summary>
    /// Interaction logic for PopoverLayer.xaml
    /// </summary>
    public partial class PopoverLayer : UserControl
    {
        public PopoverLayer()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty OverlayColorProperty = DependencyProperty.Register(
            "OverlayColor", typeof(Brush), typeof(PopoverLayer), new PropertyMetadata(new SolidColorBrush(Colors.Black) {Opacity = 0.5}));

        public Brush OverlayColor
        {
            get { return (Brush) GetValue(OverlayColorProperty); }
            set { SetValue(OverlayColorProperty, value); }
        }

        public static readonly DependencyProperty OverlayOpacityProperty = DependencyProperty.Register(
            "OverlayOpacity", typeof(double), typeof(PopoverLayer), new PropertyMetadata(0.5, OnOverlayOpacityChanged));

        private static void OnOverlayOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.ValueChanged() && ((PopoverLayer)d).OverlayColor != null)
            {
                ((PopoverLayer) d).OverlayColor.Opacity = (double)e.NewValue;
            }
        }

        public double OverlayOpacity
        {
            get { return (double) GetValue(OverlayOpacityProperty); }
            set { SetValue(OverlayOpacityProperty, value); }
        }
        
        public void Show()
        {
            // make visible
            this.popoverLayer.Visibility = Visibility.Visible;
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Hidden;
            this.popoverLayer.Visibility = Visibility.Hidden;
            this.overlayContent.Content = null;
        }

        public void ShowContent(object content)
        {
            this.overlayContent.Content = content;

            Show();
        }

        public PopoverLayer FitToContent()
        {
            if(overlayContent.VerticalAlignment == VerticalAlignment.Stretch)
                overlayContent.VerticalAlignment = VerticalAlignment.Top;

            if(overlayContent.HorizontalAlignment == HorizontalAlignment.Stretch)
                overlayContent.HorizontalAlignment = HorizontalAlignment.Left;

            return this;
        }

        public PopoverLayer FitToWindow()
        {
            overlayContent.VerticalAlignment = VerticalAlignment.Stretch;
            overlayContent.HorizontalAlignment = HorizontalAlignment.Stretch;

            return this;
        }

        public void AtLocation(WindowStartupLocation location)
        {
            if (location == WindowStartupLocation.CenterOwner || location == WindowStartupLocation.CenterScreen)
            {
                if (overlayContent.VerticalAlignment != VerticalAlignment.Stretch)
                    overlayContent.VerticalAlignment = VerticalAlignment.Center;

                if (overlayContent.HorizontalAlignment != HorizontalAlignment.Stretch)
                    overlayContent.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }
    }
}
