using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using VulcanTest.Vulcan;
using System.Diagnostics;
using VulcanForWindows.Classes.VulcanGradesDb;
using Newtonsoft.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage1 : Page
    {
        public BlankPage1()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in await LiteDbManager.database.GetCollectionNamesAsync())
                await LiteDbManager.database.DropCollectionAsync(col);
            Preferences.Clear();
            b1.Content = "Done!";
        }

        private void b2_Click(object sender, RoutedEventArgs e)
        {
            LiteDbManager.database.GetCollection<Classes.VulcanGradesDb.ClassmateGradesSyncObject>().DeleteAllAsync();
        }
        private async void GetColumn(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(JsonConvert.SerializeObject(await ClassmateGradesService.GetSingleClassmateColumn(2361146)));
        }
    }
}
