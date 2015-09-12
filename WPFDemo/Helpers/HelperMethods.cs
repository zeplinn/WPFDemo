using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WPFDemo
{
    public class HelperMethods
    {
        public static T FindParentOfType<T>(DependencyObject o) where T :DependencyObject
        {
            if (o == null)
                return null;
            dynamic parent = VisualTreeHelper.GetParent(o);
            return parent.GetType().IsAssignableFrom(typeof(T)) ? parent : FindParentOfType<T>(parent);
        }
    }
}
