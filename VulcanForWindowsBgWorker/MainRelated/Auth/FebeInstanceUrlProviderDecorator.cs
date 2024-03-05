using System.Threading.Tasks;
using Vulcanova.Uonet.Api.Common;

namespace Vulcanova.Core.Uonet;

public class FebeInstanceUrlProviderDecorator : IInstanceUrlProvider
{
    private readonly InstanceUrlProvider _instanceUrlProvider;

    public FebeInstanceUrlProviderDecorator(InstanceUrlProvider instanceUrlProvider)
    {
        _instanceUrlProvider = instanceUrlProvider;
    }

    public async Task<string> GetInstanceUrlAsync(string token, string symbol)
    {
        return await _instanceUrlProvider.GetInstanceUrlAsync(token, symbol);
    }

    public string ExtractInstanceUrlFromRequestUrl(string apiEndpointUrl)
        => _instanceUrlProvider.ExtractInstanceUrlFromRequestUrl(apiEndpointUrl);
}