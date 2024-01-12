using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    static IDictionary<string, string> prefabs = new Dictionary<string, string>
    {
        {"defaultWithAgo", "DTF_dddd!_dd.MM_DTF_(AGO__)" },
        {"WeekdayDateAgo", "DTF_dddd!_dd.MM_DTF_(AGO__)" },
        {"default", "DTF_dddd!_dd.MM_DTF" },
        {"WeekdayDate", "DTF_dddd!_dd.MM_DTF" },
        {"Date", "DTF_dd.MM_DTF" },
        {"DateWithYear", "DTF_dd.MM.YY_DTF" },
        {"DayMonthName", "DTF_dd_MMMM_DTF" },
        {"DayMonthShort", "DTF_dd_MMM_DTF" },
        {"Weekday", "DTF_dddd_DTF" },
        {"MonthName", "DTF_MMMM_DTF" },
        {"MonthShort", "DTF_MMM_DTF" },
        {"MonthNumber", "DTF_MM_DTF" },
    };

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string param = (string)parameter;

        if (value is DateTime dateTime)
        {
            if (string.IsNullOrEmpty(param))
            {
                return Convert(value, targetType, "defaultWithAgo", language);
            }
            else if(prefabs.ContainsKey(param))
            {
                return Convert(value, targetType, prefabs[param], language);
            }else
            {
                string s = param ;
                if(param.Contains("DTF_") && param.Contains("_DTF"))
                {
                    string inside;
                    inside = param.Split("DTF_")[1].Split("_DTF")[0];

                    s = s.Replace($"{inside}", dateTime.ToString(inside));
                    s = s.Replace("DTF_", "");
                    s= s.Replace("_DTF", "");
                }
                if(param.Contains("AGO__"))
                {

                   s= s.Replace("AGO__", HumanLikeAgoAndDays(dateTime));
                }
                return FirstLetterToUpper(s.Replace("_", " ").Replace("!", ","));
            }
        }

        // Return null if the value is not a DateTime
        return null;
    }
    public string DateTimeToString(DateTime dateTime, string arg)
    {
        return FirstLetterToUpper(dateTime.ToString(arg));
    }

    public string HumanLikeAgoAndDays(DateTime dateTime)
    {
        int daysAgo = (DateTime.Now.Date - dateTime.Date).Days;
        string v;
        switch (daysAgo)
        {
            case 0:
                v = "Dzisiaj";
                break;
            case 1:
                v = "Wczoraj";
                break;
            case 2:
                v = "Przedwczoraj";
                break;
            case -1:
                v = "Jutro";
                break;
            case -2:
                v = "Pojutrze";
                break;
            default:
                if (daysAgo > 0)
                    v = $"{daysAgo} {((daysAgo == 1) ? "dzień" : "dni")} temu";
                else
                    v = $"Za {-daysAgo} dni";
                break;
        }
        return v;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // ConvertBack is not used in this example
        throw new NotImplementedException();
    }
}

