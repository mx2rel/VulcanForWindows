using Microsoft.Win32;
using Newtonsoft.Json;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Grades;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;
using VulcanTest.Vulcan;
using VulcanForWindows.Classes.VulcanGradesDb;
using System.Diagnostics;
using System.IO.Pipes;

namespace VulcanForWindowsBgWorker;

static class Program
{

    public static int GradesUpdateInterval = 2;
    public static bool SendGradesNotifications = true;
    public static bool DebugMode = false;

    /// <summary>
    /// G³ówny punkt wejœcia dla aplikacji.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        AddToStartupRegistry();
        Syncer();
        LoadPreferences();

        // Keep the main thread alive
        // This will ensure that the application doesn't exit immediately
        // Otherwise, the background task will also terminate
        while (true)
        {

            MinutePassed();
            //Thread.Sleep(15000); // Wait for minute
            Thread.Sleep(60000); // Wait for minute


        }
    }

    private static void LoadPreferences()
    {
        GradesUpdateInterval = Preferences.Get<int>("GradesUpdateInterval", 2);
        SendGradesNotifications = GradesUpdateInterval != -1;
        if (GradesUpdateInterval == -1)
            GradesUpdateInterval = 5;

        DebugMode = Preferences.Get<int>("DebugMode", 2) != -1;
    }

    public static async void Syncer()
    {
        while (true)
        {
            var server = new NamedPipeServerStream("VulcanForWindowsInterAppSync", PipeDirection.In);

            // Wait asynchronously for connection
            await server.WaitForConnectionAsync();

            // Read data from the client asynchronously
            var reader = new StreamReader(server);
            string data = await reader.ReadToEndAsync();
            if (DebugMode)
                new ToastContentBuilder()
                    .AddText("Syncer").AddText(data)
                .Show();

            var splitted = data.Split("|");
            Preferences.Set<int>(splitted[0], int.Parse(splitted[1]));
            LoadPreferences();
            server.Close();
        }

    }

    public static void MinutePassed()
    {
        var currentMinute = Math.Floor(DateTime.Now.TimeOfDay.TotalMinutes);
        if (currentMinute % GradesUpdateInterval == 0) GradesUpdate();
    }

    async static void GradesUpdate()
    {
        var acc = new AccountRepository().GetActiveAccountAsync();
        var res = await new GradesService().FetchGradesFromCurrentPeriodAsync(acc);
        List<Grade> newGrades = new List<Grade>();
        foreach (var grade in res)
        {
            if (Preferences.TryGet<DateTime>($"Grade_{grade.Id}_ShowedNotification", out var on))
            {
                if (on < grade.DateModify)
                {
                    newGrades.Add(grade);
                    //Preferences.Set<DateTime>($"Grade_{grade.Id}_ShowedNotification", DateTime.Now);
                    //toAdd.Add($"Grade_{grade.Id}_ShowedNotification");
                }
            }
            else
            {
                newGrades.Add(grade);
                //Preferences.Set<DateTime>($"Grade_{grade.Id}_ShowedNotification", DateTime.Now);
                //toAdd.Add($"Grade_{grade.Id}_ShowedNotification");
            }
        }
        Preferences.SetGroup<DateTime>(newGrades.Select(r => ($"Grade_{r.Id}_ShowedNotification", DateTime.Now as object)).ToArray());

        if (newGrades.Count == 0) return;


        //if (GradesUpdateInterval == -1) return;

        newGrades = newGrades.OrderByDescending(r => r.DateModify).ToList();
        ClassmateGradesUploader.UpsyncGrades(newGrades.ToArray(), acc.CurrentPeriod.Id);

        if (SendGradesNotifications)
            if (newGrades.Count > 0)
            {
                if (newGrades.Count == 1)
                {
                    foreach (var v in newGrades)
                        new ToastContentBuilder()
                    .AddText($"Nowa ocena - {v.Column.Subject.Name}")
                    .AddText($"{v.Content} ({v.Column.Name})")
                    .Show();
                }
                else
                {
                    new ToastContentBuilder()
            .AddText($"({newGrades.Count}) nowe oceny").AddAppLogoOverride(new Uri(Path.Combine(Application.StartupPath, "GradesIcon.png")))
            .AddText(string.Join("\n", newGrades.Select(r => ($"{r.Column.Subject.Name} ({((r.Column.Name.Length > 22) ? (r.Column.Name.Substring(0, 22) + "...") : r.Column.Name)}): {r.Content}")).Take(3).ToArray())
            + ((newGrades.Count > 3) ? ($"\n+ {newGrades.Count - 3} innych") : ""))
            .Show();
                }
            }
    }


    static void AddToStartupRegistry()
    {

        string appName = "VulcanForWindowsBgWorker";
        string appPath = Path.Combine(Environment.CurrentDirectory, "VulcanForWindowsBgWorker.exe");
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        registryKey.SetValue(appName, appPath);

        if (registryKey.GetValue(appName) == null)
        {
            // Add the application to startup
            registryKey.SetValue(appName, appPath);
            //Console.WriteLine("Application added to startup.");
            //MessageBox.Show("Application added to startup!");
            //new ToastContentBuilder()
            //    .AddText($"VulcanForWindowsBgWorker has been added to startup.")
            //    .Show();

        }
        else
        {
            //new ToastContentBuilder()
            //    .AddText($"VulcanForWindowsBgWorker is running.")
            //    .Show();
        }
    }
}