using DevExpress.WinUI.Drawing.Internal;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Converters
{
    public class UintToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(Brush))
            {
                if (language == "acrylic")
                    return new AcrylicBrush()
                    {
                        TintColor = GetColor(value, parameter, false).Value,
                        TintOpacity = 0.9,
                        TintLuminosityOpacity = 0.6,
                    };
                return new SolidColorBrush(GetColor(value, parameter, false).Value);
            }
            if (targetType == typeof(Color)) return GetColor(value, parameter, false).Value;
            return null;
        }

        Color? GetColor(object value, object parameter, bool returnNull = false)
        {
            if (value is uint uintColor)
            {
                byte a = 255;
                byte r = (byte)((uintColor >> 16) & 0xFF);
                byte g = (byte)((uintColor >> 8) & 0xFF);
                byte b = (byte)(uintColor & 0xFF);

                if (r == 0 && g == 0 && b == 0)
                {
                    if (parameter is string def)
                    {
                        var p = typeof(Colors).GetProperty(def);
                        if (p != null)
                        {
                            return (Color)p.GetValue(null, null);
                        }

                    }
                    else
                    {
                        if (returnNull) return null;
                        return Colors.Transparent;
                    }
                }


                return ColorHelper.FromArgb(a, r, g, b);
            }
            if (returnNull) return null;
            return Colors.Transparent;
        }


        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // ConvertBack is not used in this example
            throw new NotImplementedException();
        }
    }
}
