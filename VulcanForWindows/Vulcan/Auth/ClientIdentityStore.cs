using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Vulcanova.Features.Auth
{
    public static class ClientIdentityStore
    {

        public static async Task SaveIdentityAsync(ClientIdentity identity)
        {
            var (x509Certificate2, pemPrivateKey, firebaseToken) = identity;

            using var x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.ReadWrite);

            x509Store.Add(x509Certificate2);

            await SaveSecureStorage($"pem", pemPrivateKey);
            await SaveSecureStorage($"firebase", firebaseToken);
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

            var pemPrivateKey = await GetSecureStorage($"pem");
            var firebaseToken = await GetSecureStorage($"firebase");

            if (pemPrivateKey == null || firebaseToken == null)
            {
                return null;
            }

            return new ClientIdentity(certs[0], pemPrivateKey, firebaseToken);
        }

        private static async Task SaveSecureStorage(string key, string value)
        {
            // Implement your secure storage mechanism for console apps, e.g., file storage.
            // Here, I'm using a simple file-based approach.
            if (!Directory.Exists("C:/Vulcan"))
                Directory.CreateDirectory("C:/Vulcan");
            if (!File.Exists("C:/Vulcan/" + key + ".txt"))
            {
                var s = File.Create("C:/Vulcan/" + key + ".txt");
                s.Close();
            }
            File.WriteAllText("C:/Vulcan/"+key+".txt", value);
        }

        private static async Task<string> GetSecureStorage(string key)
        {
            // Implement your secure storage mechanism for console apps, e.g., file storage.
            // Here, I'm using a simple file-based approach.
            Console.WriteLine("Reading: " + "C:/Vulcan/" + key + ".txt");
            if (File.Exists("C:/Vulcan/" + key + ".txt"))
            {
                return File.ReadAllText("C:/Vulcan/" + key + ".txt");
            }
            Console.WriteLine("Couldn't find file " + key);
            return null;
        }
    }

    public record ClientIdentity(X509Certificate2 Certificate, string PemPrivateKey, string FirebaseToken);
}
