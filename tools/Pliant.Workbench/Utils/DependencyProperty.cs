using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pliant.Workbench.Utils
{
    public static class DP
    {
        public static DependencyProperty Register<TControl, TValue>(string name, Action<TControl, TValue> onChangeAction)
        {
            return DependencyProperty.Register(name, typeof(TValue), typeof(TControl), new RfxUIPropertyMetadata<TControl, TValue>(onChangeAction));
        }

        public static DependencyProperty Register<TControl, TValue>(string name, TValue defaultValue, Action<TControl, TValue> onChangeAction)
        {
            return DependencyProperty.Register(name, typeof(TValue), typeof(TControl), new RfxUIPropertyMetadata<TControl, TValue>(defaultValue, onChangeAction));
        }
    }

    public class RfxUIPropertyMetadata<TControl, TValue> : UIPropertyMetadata
    {
        public RfxUIPropertyMetadata(Action<TControl, TValue> onChangeAction)
        {
            OnChangeAction = onChangeAction;

            ConfigureChangeAction();
        }

        public RfxUIPropertyMetadata(TValue defaultValue, Action<TControl, TValue> onChangeAction)
        {
            DefaultValue = defaultValue;
            OnChangeAction = onChangeAction;

            ConfigureChangeAction();
        }

        private void ConfigureChangeAction()
        {
            if (OnChangeAction != null)
            {
                PropertyChangedCallback = (o, args) => {
                    if (InvokeOnValueEquals || args.NewValue != args.OldValue)
                    {
                        OnChangeAction((TControl)(object)o, (TValue)args.NewValue);
                    }
                };
            }
        }

        public bool InvokeOnValueEquals { get; set; }
        public Action<TControl, TValue> OnChangeAction { get; set; }
    }
}
