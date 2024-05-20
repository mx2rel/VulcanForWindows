using LiteDB.Async;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace VulcanTest.Vulcan
{
    public static class LiteDbManager
    {
        public static LiteDatabaseAsync database = new LiteDatabaseAsync($"Filename={Path.Combine(Preferences.folder, ("main.db"))};Connection=shared;");
    }
}
