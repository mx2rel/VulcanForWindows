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

            string s = param;
            if (s.Contains("!"))
            {
                var split = s.Split("!");
                var result = b ? split[0] : split[1];

                if (targetType == typeof(TextDecorations) || targetType == typeof(FontWeight))
                {



                    if (targetType == typeof(FontWeight))
                        return
                            (FontWeight)(typeof(FontWeights).GetProperty(result).GetValue(null));
                    if (targetType == typeof(TextDecorations))
                        return
                        (TextDecorations)Enum.Parse(typeof(TextDecorations), result);
                }
                if (targetType == typeof(string))
                {
                    return result;
                }
        }
        }

        Debug.WriteLine($"Could not convert bool to string. Parameter: {param}, TargetType: {targetType.ToString()})");
        // Return null if the value is not a DateTime
        return null;
    }

    public string CustomHandler(bool b, string param)
    {
        switch (param)
        {
            default:
                return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // ConvertBack is not used in this example
        throw new NotImplementedException();
    }
}

