using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using VulcanForWindows.Preferences;

namespace Vulcanova.Features.Auth
{
    public static class ClientIdentityStore
    {

        private const string PemPrivateKeyItemKey = "PemPrivateKey";
        private const string FirebaseTokenItemKey = "FirebaseToken";
        private static string FolderPath => Path.Combine(PreferencesManager.folder, "CIS");
        public static async Task SaveIdentityAsync(ClientIdentity identity)
        {
            var (x509Certificate2, pemPrivateKey, firebaseToken) = identity;

            using var x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.ReadWrite);

            x509Store.Add(x509Certificate2);

            SaveSecureStorage($"{PemPrivateKeyItemKey}_{x509Certificate2.Thumbprint}", pemPrivateKey);
            SaveSecureStorage($"{FirebaseTokenItemKey}_{x509Certificate2.Thumbprint}", firebaseToken);
        }

        public static async Task<ClientIdentity> GetIdentityAsync(string thumbprint)
        {
            using var x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.ReadOnly);

            var certs = x509Store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

            if (certs.Count == 0)
            {
                return null;
            }

            var pemPrivateKey = GetSecureStorage($"{PemPrivateKeyItemKey}_{thumbprint}");
            var firebaseToken = GetSecureStorage($"{FirebaseTokenItemKey}_{thumbprint}");

            if (pemPrivateKey == null || firebaseToken == null)
            {
                return null;
            }

            return new ClientIdentity(certs[0], pemPrivateKey, firebaseToken);
        }

        public static async void RemoveClientIdentity(string thumbprint)
        {
            RemoveSecureStorage($"{PemPrivateKeyItemKey}_{thumbprint}");
            RemoveSecureStorage($"{FirebaseTokenItemKey}_{thumbprint}");
        }

        private static void SaveSecureStorage(string key, string value)
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            var filePath = $"{FolderPath}/{key}.txt";

            if (!File.Exists(filePath))
            {
                var s = File.Create(filePath);
                s.Close();
            }
            File.WriteAllText(filePath, value);
        }

        private static void RemoveSecureStorage(string key)
        {
            if (Directory.Exists(FolderPath))
            {

                var filePath = $"{FolderPath}/{key}.txt";

                if (File.Exists(filePath))
                    Directory.Delete(filePath, true);
            }
        }

        private static string GetSecureStorage(string key)
        {
            var filePath = $"{FolderPath}/{key}.txt";
            
            Console.WriteLine("Reading: " + filePath);
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            Console.WriteLine("Couldn't find file " + key);
            return null;
        }
    }

    public record ClientIdentity(X509Certificate2 Certificate, string PemPrivateKey, string FirebaseToken);
}
