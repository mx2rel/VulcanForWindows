using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Text;

namespace Converters;

public class BoolToStrings : IValueConverter
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
                    var weightString = b ? split[0] : split[1];
                    return
                        (FontWeight)(typeof(FontWeights).GetProperty(weightString).GetValue(null));
                }
            }
        }

        Debug.WriteLine("Could not convert bool to font weight. Parameter: " + param);
        // Return null if the value is not a DateTime
        return FontWeights.Normal;
    }

    public string CustomHandler(bool b, string param)
    {
        switch (param)
        {
            default:
                return b ? "Normal" : "Normal";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // ConvertBack is not used in this example
        throw new NotImplementedException();
    }
}

