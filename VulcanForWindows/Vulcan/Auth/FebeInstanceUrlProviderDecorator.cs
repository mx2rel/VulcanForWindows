using System.Threading.Tasks;
using VulcanForWindows.Properties;
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
        if (symbol.StartsWith("!FEBE"))
        {
            return Resources.FebeInstanceUrl;
        }

        return await _instanceUrlProvider.GetInstanceUrlAsync(token, symbol);
    }

    public string ExtractInstanceUrlFromRequestUrl(string apiEndpointUrl)
        => _instanceUrlProvider.ExtractInstanceUrlFromRequestUrl(apiEndpointUrl);
}