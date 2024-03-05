using Microsoft.Win32;
using Newtonsoft.Json;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Grades;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;
using VulcanTest.Vulcan;
using VulcanForWindows.Classes.VulcanGradesDb;

namespace VulcanForWindowsBgWorker;

static class Program
{
    /// <summary>
    /// G³ówny punkt wejœcia dla aplikacji.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        AddToStartupRegistry();


        // Keep the main thread alive
        // This will ensure that the application doesn't exit immediately
        // Otherwise, the background task will also terminate
        while (true)
        {
            Thread.Sleep(15000); // Sleep for a short interval to avoid high CPU usage
            TimerCallback(null);
        }
    }
    async static void TimerCallback(object state)
    {
        // Display the message box
        var acc = new AccountRepository().GetActiveAccountAsync();
        var res = await new GradesService().FetchGradesFromCurrentPeriodAsync(acc);
        ClassmateGradesUploader.UpsyncGrades(res, acc.CurrentPeriod.Id);
        List<Grade> newGrades = new List<Grade>();
        foreach (var grade in res)
        {
            if (Preferences.TryGet<DateTime>($"Grade_{grade.Id}_ShowedNotification", out var on))
            {
                if (on < grade.DateModify)
                {
                    newGrades.Add(grade);
                    Preferences.Set<DateTime>($"Grade_{grade.Id}_ShowedNotification", DateTime.Now);
                }
            }
            else
            {
                newGrades.Add(grade);
                Preferences.Set<DateTime>($"Grade_{grade.Id}_ShowedNotification", DateTime.Now);
            }
        }


        new ToastContentBuilder()
    .AddArgument("action", "viewConversation")
    .AddArgument("conversationId", 9813)
    .AddText($"({newGrades.Count}) nowe oceny!")
    .AddText(JsonConvert.SerializeObject(res).Substring(0, 20))
    .Show();
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
            Console.WriteLine("Application added to startup.");
            MessageBox.Show("Application added to startup!");

        }
        else
        {
            Console.WriteLine("Application is already added to startup.");
            MessageBox.Show("Application is already added to startup.");
        }
    }
}