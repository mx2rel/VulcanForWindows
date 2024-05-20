using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Data;

namespace Converters;

public class NumberToTwoDecimals : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        //string param = parameter as string;
        if (value is double d)
        {
            return d.ToString("0.00");
        }
        return "?.??";
    }


    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // ConvertBack is not used in this example
        throw new NotImplementedException();
    }
}

