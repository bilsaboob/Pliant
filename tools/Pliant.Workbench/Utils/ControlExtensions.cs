using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Pliant.Workbench.Utils
{
    public static class ControlExtensions
    {
        public static bool ValueChanged(this DependencyPropertyChangedEventArgs args)
        {
            return args.NewValue != args.OldValue;
        }

        public static T FindChild<T>(this DependencyObject parent, string childName = null)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T typedChild = child as T;
                if (typedChild == null)
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null)
                        return foundChild;
                }
                else
                {
                    if (string.IsNullOrEmpty(childName))
                        return typedChild;

                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                        return (T)child;

                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null)
                        return foundChild;
                }
            }

            return null;
        }
    }
}
