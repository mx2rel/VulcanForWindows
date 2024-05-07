using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Shared;
using Vulcanova.Uonet.Api.Auth;
using Period = Vulcanova.Features.Shared.Period;

namespace Vulcanova.Features.Auth;

public class AccountSyncService : UonetResourceProvider, IAccountSyncService
{

    private const string ResourceKey = "AccountsSync";

    public async Task SyncAccountsIfRequiredAsync()
    {
        if (!ShouldSync(ResourceKey)) return;

        var accountsGroupedByLoginId = (new AccountRepository().GetAccountsAsync())
            .GroupBy(x => x.Login.Id);

        foreach (var accountsGroup in accountsGroupedByLoginId)
        {
            var client = await new ApiClientFactory().GetAuthenticatedAsync(accountsGroup.First());
            
            var newAccounts = await client.GetAsync(
                // when querying the unit API contrary to the instance API,
                // the /api prefix has to be omitted, thus [4..] to omit the "api/" prefix
                RegisterHebeClientQuery.ApiEndpoint[4..],
                new RegisterHebeClientQuery());
            
            foreach (var acc in accountsGroup)
            {
                var newAccount = newAccounts.Envelope.SingleOrDefault(x => x.Login.Id == acc.Login.Id);

                if (newAccount == null) continue;

                var currentPeriodsIds = acc.Periods.Select(y => y.Id);

                // in some rare cases, the data will contain duplicated periods
                var deduplicatedNewPeriods = newAccount.Periods.GroupBy(p => p.Id).Select(g => g.First()).ToArray();

                var periodsChanged = deduplicatedNewPeriods.Any(x => !currentPeriodsIds.Contains(x.Id));

                if (!periodsChanged)
                {
                    var newCurrentPeriod = deduplicatedNewPeriods.Single(x => x.Current);
                    var oldCurrentPeriod = acc.Periods.Single(x => x.Current);

                    periodsChanged = newCurrentPeriod.Id != oldCurrentPeriod.Id;
                }

                if (periodsChanged)
                {

                    var mapperConfig = new MapperConfiguration(cfg =>
                    {
                        cfg.AddProfile<AccountMapperProfile>(); // Replace with your actual mapping profile class
                    });

                    IMapper mapper = mapperConfig.CreateMapper();

                    acc.Periods = deduplicatedNewPeriods.Select(mapper.Map<Period>).ToList();
                }

                acc.Capabilities = newAccount.Capabilities;

                new AccountRepository().UpdateAccount(acc);

                // is it the active account?
                //if (_accountContext.Account.Id == acc.Id)
                //{
                //    _accountContext.Account = acc;
                //}
            }
        }
        Console.Write("synced");
        SetJustSynced(ResourceKey);
    }

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromDays(1);
}