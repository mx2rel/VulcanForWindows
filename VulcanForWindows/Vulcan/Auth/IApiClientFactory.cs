using System.Threading.Tasks;
using Vulcanova.Features.Auth;
using Vulcanova.Uonet.Api;

namespace Vulcanova.Core.Uonet;

public interface IApiClientFactory
{
    IApiClient GetAuthenticated(ClientIdentity identity, string apiInstanceUrl,
        string accountContext = null);

    Task<IApiClient> GetAuthenticatedAsync(string identityThumbprint, string apiInstanceUrl,
        string accountContext = null);
}