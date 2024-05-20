using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Vulcanova.Uonet;
using Vulcanova.Features.Auth;
using Vulcanova.Uonet.Api.Auth;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth.Accounts;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Vulcanova.Features.Auth;

public class AuthenticationService : IAuthenticationService
{
    public readonly IApiClientFactory _apiClientFactory;

    public AuthenticationService(
        IApiClientFactory apiClientFactory)
    {
        _apiClientFactory = apiClientFactory;
    }

    public async Task<Account[]> AuthenticateAsync(string token, string pin, string instanceUrl)
    {
        var identity = await ClientIdentityProvider.CreateClientIdentityAsync();

        var x509Certificate2 = identity.Certificate;

        var device = $"Vulcanoid (mx2rel) – Windows: {System.Environment.MachineName}";

        var request = new RegisterClientRequest
        {
            OS = Constants.AppOs,
            DeviceModel = device,
            Certificate = Convert.ToBase64String(x509Certificate2.RawData),
            CertificateType = "X509",
            CertificateThumbprint = x509Certificate2.Thumbprint,
            PIN = pin,
            SecurityToken = token,
            SelfIdentifier = Guid.NewGuid().ToString()
        };

        var client = _apiClientFactory.GetAuthenticated(identity, instanceUrl);
        await client.PostAsync(RegisterClientRequest.ApiEndpoint, request);

        await ClientIdentityStore.SaveIdentityAsync(identity);

        var registerHebeResponse = await client.GetAsync(RegisterHebeClientQuery.ApiEndpoint, new RegisterHebeClientQuery());

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AccountMapperProfile>(); // Replace with your actual mapping profile class
        });

        IMapper mapper = mapperConfig.CreateMapper();

        var accounts = registerHebeResponse.Envelope
            .Where(a => a.Login != null && a.Periods is { Length: > 0 })
            .Select(mapper.Map<Account>)
            .ToArray();

        foreach (var account in accounts)
        {
            // in some rare cases, the data will contain duplicated periods
            account.Periods = account.Periods.GroupBy(p => p.Id).Select(g => g.First()).ToList();
            account.IdentityThumbprint = identity.Certificate.Thumbprint;
        }

        return accounts;
    }
}