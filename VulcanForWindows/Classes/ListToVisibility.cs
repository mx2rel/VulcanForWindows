using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Converters;

public class ListToVisibility : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return Get(value, parameter) ? Visibility.Visible : Visibility.Collapsed;
    }

    public bool Get(object value, object parameter)
    {
        bool swap = false;
        if(parameter != null)
        if (bool.TryParse( parameter.ToString(),out var s) )
            swap = s;

        if (value is System.Collections.IEnumerable ie)
        {
            if (!swap)
                return ie.Cast<object>().Count() > 0;
            else
                return ie.Cast<object>().Count() == 0;

        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // ConvertBack is not used in this example
        throw new NotImplementedException();
    }
}

