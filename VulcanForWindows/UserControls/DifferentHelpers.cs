using Microsoft.UI.Xaml;

namespace VulcanForWindows.UserControls
{
    internal static class DifferentHelpers
    {

        public static Visibility ToVisibility(this bool t)
        {
            return t ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}