using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VulcanTest.Vulcan.Settings;

namespace Vulcanova.Features.Grades.Summary;

public static class AverageCalculator
{
    public static decimal? Average(this IEnumerable<Grade> grades, ModifiersSettings modifiers)
    {
        var nonNullGrades = grades
            .Where(g => g.VulcanValue != null)
            .SelectMany(g => Enumerable.Repeat(TryGetValueFromContentRaw(g.ContentRaw, modifiers), g.Column.Weight))
            .Where(g => g != null)
            .ToList();

        if (!nonNullGrades.Any())
        {
            return null;
        }

        return nonNullGrades.Sum() / nonNullGrades.Count;
    }
    public static (decimal average, int sumOfWeights)? AverageRaw(this IEnumerable<Grade> grades, ModifiersSettings modifiers)
    {
        var nonNullGrades = grades
            .Where(g => g.VulcanValue != null)
            .SelectMany(g => Enumerable.Repeat(TryGetValueFromContentRaw(g.ContentRaw, modifiers), g.Column.Weight))
            .Where(g => g != null)
            .ToList();

        if (!nonNullGrades.Any())
        {
            return null;
        }

        return ((decimal)nonNullGrades.Sum() / nonNullGrades.Count, nonNullGrades.Count);

    }

    private static readonly Regex ValueRegex = new("(\\d+)([+|-])?");

    public static bool TryGetValueFromContentRaw(string contentRaw, out decimal d)
        => TryGetValueFromContentRaw(contentRaw, new ModifiersSettings(), out d);
    public static bool TryGetValueFromContentRaw(string contentRaw, ModifiersSettings modifiers, out decimal d)
    {
        var value = TryGetValueFromContentRaw(contentRaw, modifiers);

        if (value == null)
        {
            d = 0;
            return false;
        }

        d = value.Value;
        return true;
    }

    private static decimal? TryGetValueFromContentRaw(string contentRaw, ModifiersSettings modifiers)
    {
        var match = ValueRegex.Match(contentRaw);

        if (!match.Success)
        {
            return null;
        }

        if (!int.TryParse(match.Groups[1].Value, out var numericValue))
        {
            return null;
        }

        if (!match.Groups[2].Success)
        {
            return numericValue;
        }

        var modifier = GradeModifier.FromString(match.Groups[2].Value, modifiers);
        return modifier.ApplyTo(numericValue);
    }

    public static bool TryGetRawContentFromValue(decimal value, ModifiersSettings modifiers, out string d)
    {
        d = string.Empty;
        decimal decimals = value % 1;

        if (value < 1 || value > 6) return false;

        if (decimals == 0)
        {
            d = ((int)value).ToString();
            return true;
        }
        else
        if (decimals == modifiers.PlusSettings.SelectedValue)
        {
            d = value.ToString()[0] + "+";
            return true;
        }
        else
        if (decimals == (1 - modifiers.MinusSettings.SelectedValue))
        {
            d = value.ToString()[0] + "-";
            return true;
        }
        else
        {
            return false;
        }
    }
}