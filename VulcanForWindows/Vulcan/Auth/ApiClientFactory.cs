using System.Collections.Concurrent;
using System.Threading.Tasks;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Uonet.Api;
using Vulcanova.Uonet.Signing;

namespace Vulcanova.Core.Uonet;

public class ApiClientFactory : IApiClientFactory
{
    private readonly ConcurrentDictionary<string, IApiClient> _reusableClients = new();

    private static string GetCacheKey(string thumbprint, string instanceUrl, string accountContext) =>
        $"{thumbprint}+{instanceUrl}+{accountContext}";

    public IApiClient GetAuthenticated(ClientIdentity identity, string apiInstanceUrl, string accountContext = null)
        => _reusableClients.GetOrAdd(
            GetCacheKey(identity.Certificate.Thumbprint, apiInstanceUrl, accountContext),
            static (_, values) => CreateApiClient(values.identity, values.apiInstanceUrl, values.accountContext),
            (identity, apiInstanceUrl, accountContext));

    public async Task<IApiClient> GetAuthenticatedAsync(string identityThumbprint, string apiInstanceUrl,
        string accountContext = null)
    {
        var cacheKey = GetCacheKey(identityThumbprint, apiInstanceUrl, accountContext);

        if (_reusableClients.TryGetValue(cacheKey, out var value))
        {
            return value;
        }

        var identity = await ClientIdentityStore.GetIdentityAsync(identityThumbprint);

        var apiClient = CreateApiClient(identity, apiInstanceUrl, accountContext);

        _reusableClients.TryAdd(cacheKey, apiClient);

        return apiClient;
    }

    private static IApiClient CreateApiClient(ClientIdentity identity, string apiInstanceUrl, string accountContext)
    {
        var (cert, privateKey, firebaseToken) = identity;

        var signer =
            accountContext is not null
                ? new ContextualRequestSigner(cert.Thumbprint, privateKey, firebaseToken, accountContext)
                : new RequestSigner(cert.Thumbprint, privateKey, firebaseToken);

        return new ApiClient(signer, apiInstanceUrl);
    }
}

public static class ApiClientFactoryExtensions
{
    public static async Task<IApiClient> GetAuthenticatedAsync(this IApiClientFactory factory, Account account)
    {
        return await factory.GetAuthenticatedAsync(account.IdentityThumbprint, account.Unit.RestUrl.ToString(), account.Context);
    }
}