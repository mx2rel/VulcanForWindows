using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Data;

namespace Converters;

public class BoolToColor : IValueConverter
{

    static IDictionary<string, string> prefabs = new Dictionary<string, string>
    {
    };

    static string[] customs = new string[]
    {
    };

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string param = parameter as string;
        if (value is bool b)
        {
            if (prefabs.ContainsKey(param))
            {
                return Convert(value, targetType, prefabs[param], language);
            }
            else if (customs.Contains(param))
            {
                return CustomHandler(b, param);
            }
            else
            {
                string s = param;


                if (s.Contains("!"))
                {
                    var split = s.Split("!");
                    string colorString = b ? split[0] : split[1];
                    return new Microsoft.UI.Xaml.Media.
                        SolidColorBrush(
                        (Windows.UI.Color)(typeof(Microsoft.UI.Colors).GetProperty(colorString).GetValue(null)));

                }
            }
        }

        Debug.WriteLine("Could not convert color. Parameter: " + param);
        // Return null if the value is not a DateTime
        return new Microsoft.UI.Xaml.Media.
                        SolidColorBrush(Microsoft.UI.Colors.Indigo);
    }

    public string CustomHandler(bool b, string param)
    {
        switch (param)
        {
            default:
                return b ? "Green" : "Red";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // ConvertBack is not used in this example
        throw new NotImplementedException();
    }
}

