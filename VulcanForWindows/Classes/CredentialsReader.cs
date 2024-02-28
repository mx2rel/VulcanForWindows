using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace VulcanForWindows.Classes
{
    public class CredentialsReader
    {
        public static async Task<T> ReadCredentialAsync<T>(string credential)
        {
            try
            {
                var packageLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
                var assetsFolder = await packageLocation.GetFolderAsync("Assets");
                var credentialsFile = await assetsFolder.GetFileAsync("Credentials.json");
                var jsonText = await FileIO.ReadTextAsync(credentialsFile);
                var obj = JObject.Parse(jsonText);

                if (obj.TryGetValue(credential, out JToken value))
                {
                    return value.ToObject<T>();
                }
                else
                {
                    throw new KeyNotFoundException($"Credential '{credential}' not found in the JSON.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading credential: {ex.Message}");
                throw;
            }
        }

    }
}
