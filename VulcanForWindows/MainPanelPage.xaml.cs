using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VulcanForWindows.Classes;
using VulcanForWindows.Vulcan.Grades;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Grades;
using VulcanTest.Vulcan;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPanelPage : Page
    {
        public GradesResponseEnvelope env;

        public ObservableCollection<SubjectGrades> sg;

        public MainPanelPage()
        {
            sg = new ObservableCollection<SubjectGrades>();
            this.InitializeComponent();
            Fetch();
        }

        public async void Fetch()
        {
            var acc = new AccountRepository().GetActiveAccountAsync();
            env = await new GradesService().GetPeriodGrades(acc, acc.CurrentPeriod.Id);
            env.Updated += Env_Updated;
        }

        private void Env_Updated(object sender, IEnumerable<Grade> e)
        {
            sg.ReplaceAll(SubjectGrades.CreateRecent(env));
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //(e.ClickedItem as SubjectGrades)
            //TODO: LOAD SUBJECT
        }
    }
}
