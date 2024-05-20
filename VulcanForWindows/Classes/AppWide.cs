using Windows.ApplicationModel;

namespace VulcanTest.Vulcan
{
    public static class AppWide
    {
        public static string AppVersion => string.Format("{0}.{1}.{2}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build);
    }
}
