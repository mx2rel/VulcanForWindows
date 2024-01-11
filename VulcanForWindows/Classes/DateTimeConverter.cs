using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using Microsoft.UI.Xaml.Data;

namespace Converters;

public class DateTimeConverter : IValueConverter
{
    static string FirstLetterToUpper(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Convert the first letter to uppercase and concatenate the rest of the string
        return char.ToUpper(input[0]) + input.Substring(1);
    }


    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            int daysAgo = (DateTime.Now - dateTime).Days;
            // Format the DateTime value as "Day, DD.MM"
            string formattedString = $"{FirstLetterToUpper(dateTime.ToString("dddd"))}, {dateTime.ToString("dd.MM")} ({daysAgo} {((daysAgo == 1) ? "dzień" : "dni")} temu)";
            return formattedString;
        }

        // Return null if the value is not a DateTime
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // ConvertBack is not used in this example
        throw new NotImplementedException();
    }
}

