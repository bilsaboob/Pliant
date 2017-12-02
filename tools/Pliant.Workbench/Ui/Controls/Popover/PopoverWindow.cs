using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Pliant.Workbench.Ui.Controls.DialogWindow;
using Pliant.Workbench.Utils;
using RapidFx.Utils;
using Window = Pliant.Workbench.Ui.Controls.DialogWindow.Window;

namespace Pliant.Workbench.Ui.Controls.Popover
{
    public class PopoverWindow : Window<PopoverWindow>
    {
        public static PopoverWindow Create(object content, Action<PopoverWindow> config = null)
        {
            // Prepare the window and the frame
            var wnd = new PopoverWindow();
            var frame = new PopoverFrame(wnd);

            wnd.Frame = frame;

            // Set the window content
            wnd.Content = content;

            // Additionally configure the window
            config?.Invoke(wnd);

            return wnd;
        }

        protected override void OnHeightPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.ValueChanged() && Frame != null && (double)e.NewValue >= 0)
            {
                Frame.Height = (double)e.NewValue;
            }
        }

        protected override void OpWidthPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.ValueChanged() && Frame != null && (double)e.NewValue >= 0)
            {
                Frame.Width = (double)e.NewValue;
            }
        }
    }

    public class PopoverFrame : IWindowFrame
    {
        private object _content;
        private bool _fitToContent;
        private WindowStartupLocation _location;
        private double _locationX;
        private double _locationY;

        private PopoverLayer _popoverLayer;

        public PopoverFrame(object content)
        {
            _content = content;
        }

        public string Title { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public void FitToContent()
        {
            _fitToContent = true;
        }

        public void AtLocation(double x, double y)
        {
            _locationX = x;
            _locationY = y;
        }

        public void AtLocation(WindowStartupLocation location)
        {
            _location = location;
        }

        public void Show(Action onShown)
        {
            //Trigger custom logic of finding the popver layer

            var activeWindow = Window.GetActiveWindow();
            if (activeWindow != null)
            {
                _popoverLayer = activeWindow.FindChild<PopoverLayer>();
                if (_popoverLayer != null)
                {
                    var elem = _content as FrameworkElement;

                    if (_fitToContent)
                    {
                        _popoverLayer.FitToContent();
                    }
                    else
                    {
                        _popoverLayer.FitToWindow();
                    }

                    if (Width > 0 && elem != null)
                    {
                        elem.Width = Width;
                    }

                    if (Height > 0 && elem != null)
                    {
                        elem.Height = Height;
                    }

                    _popoverLayer.AtLocation(_location);

                    _popoverLayer.ShowContent(_content);

                    onShown?.Invoke();
                }
            }
        }

        public void Close()
        {
            if(_popoverLayer != null)
                _popoverLayer.Hide();
        }
    }
}
