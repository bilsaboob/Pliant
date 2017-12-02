using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AngleSharp.Css;
using Pliant.Workbench.Utils;
using RapidFx.Utils;

namespace Pliant.Workbench.Ui.Controls.DialogWindow
{
    public interface IWindowFrame
    {
        string Title { get; set; }
        double Width { get; set; }
        double Height { get; set; }

        void FitToContent();

        void AtLocation(double x, double y);
        void AtLocation(WindowStartupLocation location);

        void Show();
        void Close();
    }

    public enum WindowFitMode
    {
        Window,
        Content
    }

    public class Window : System.Windows.Controls.ContentControl
    {
        public static System.Windows.Window GetActiveWindow()
        {
            return Application.Current.Windows.OfType<System.Windows.Window>().SingleOrDefault(w => w.IsActive);
        }

        public Window()
        {
        }

        protected void InitContent()
        {
            var elem = Content as FrameworkElement;
            if (elem != null)
            {
                if (Fit == WindowFitMode.Window)
                {
                    elem.Width = double.NaN;
                    elem.Height = double.NaN;
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == TitleProperty)
            {
                if (e.ValueChanged() && Frame != null)
                    Frame.Title = (string)e.NewValue ?? "";

                return;
            }

            if (e.Property == WidthProperty)
            {
                OpWidthPropertyChanged(e);

                return;
            }

            

            if (e.Property == HeightProperty)
            {
                OnHeightPropertyChanged(e);
                
                return;
            }
        }

        protected virtual void OnHeightPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.ValueChanged() && Frame != null && (double)e.NewValue >= 0)
            {
                Frame.Height = (double)e.NewValue;
                Height = double.NaN;
            }
        }

        protected virtual void OpWidthPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.ValueChanged() && Frame != null && (double)e.NewValue >= 0)
            {
                Frame.Width = (double)e.NewValue;
                Width = double.NaN;
            }
        }

        public static readonly DependencyProperty FrameProperty = DependencyProperty.Register(
            "Frame", typeof(IWindowFrame), typeof(Window), new PropertyMetadata(default(IWindowFrame)));

        public IWindowFrame Frame
        {
            get { return (IWindowFrame) GetValue(FrameProperty); }
            set { SetValue(FrameProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(Window), new PropertyMetadata(default(string)));

        public string Title
        {
            get { return (string) GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public WindowFitMode Fit { get; set; }
        
        public void Size(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public void FitToWindow()
        {
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;

            VerticalContentAlignment = VerticalAlignment.Stretch;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;

            Fit = WindowFitMode.Window;
        }

        public void FitToContent()
        {
            if (Frame != null)
            {
                Frame.FitToContent();
            }

            Fit = WindowFitMode.Content;
        }

        protected void AtLocation(double x, double y)
        {
            if (Frame != null)
            {
                Frame.AtLocation(x, y);
            }
        }

        protected void AtLocation(WindowStartupLocation location)
        {
            if (Frame != null)
            {
                Frame.AtLocation(location);
            }
        }

        public void Show()
        {
            if (Frame != null)
            {
                InitContent();
                Frame.Show();
            }
        }

        public void Close()
        {
            if (Frame != null)
            {
                Frame.Close();
            }
        }
    }

    public class Window<TWindow> : Window
    {
        public new TWindow FitToWindow()
        {
            base.FitToWindow();
            return (TWindow)(object)this;
        }

        public new TWindow FitToContent()
        {
            base.FitToContent();
            return (TWindow)(object)this;
        }

        public new TWindow Size(double width, double height)
        {
            base.Size(width, height);
            return (TWindow)(object)this;
        }

        public new TWindow AtLocation(double x, double y)
        {
            base.AtLocation(x, y);
            return (TWindow)(object)this;
        }

        public new TWindow AtLocation(WindowStartupLocation location)
        {
            base.AtLocation(location);
            return (TWindow)(object)this;
        }

        public new TWindow Show()
        {
            base.Show();
            return (TWindow)(object)this;
        }
    }

    public class WindowFrame : System.Windows.Window
    {
        public WindowFrame(object content, System.Windows.Window owner)
        {
            Content = content;
            Owner = owner;
        }
    }

    public class DialogWindow : Window<DialogWindow>
    {
        public static DialogWindow Create(object content, Action<DialogWindow> config = null)
        {
            // Prepare the window and the frame
            var wnd = new DialogWindow();

            var frame = new DialogWindowFrame(wnd, Window.GetActiveWindow());

            /*
            frame.WindowStyle = WindowStyle.None;
            frame.AllowsTransparency = true;
            frame.ResizeMode = ResizeMode.CanResizeWithGrip;
            */

            wnd.Frame = frame;

            // Set the window content
            wnd.Content = content;
            
            // Additionally configure the window
            config?.Invoke(wnd);
            
            return wnd;
        }
    }
    
    public class DialogWindowFrame : WindowFrame, IWindowFrame
    {
        public DialogWindowFrame(object content, System.Windows.Window owner)
            : base(content, owner)
        {
        }

        public void FitToContent()
        {
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        public void AtLocation(double x, double y)
        {
            Left = x;
            Top = y;
        }

        public void AtLocation(WindowStartupLocation location)
        {
            WindowStartupLocation = location;
        }

        public new void Show()
        {
            base.ShowDialog();
        }

        public new void Close()
        {
            base.Close();
        }
    }
}
