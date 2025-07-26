using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace UiControls.DynamicGrid.View;

public class HasVisibleItemsMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is ItemCollection items)
        {
            return items.OfType<MenuItem>().Any(item => item.Visibility == Visibility.Visible);
        }
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}