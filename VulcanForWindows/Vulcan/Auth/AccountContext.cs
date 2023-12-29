using Vulcanova.Features.Auth.Accounts;

namespace Vulcanova.Features.Shared;

public class AccountContext
{
    public AccountContext(Account account)
    {
        Account = account;
    }


    public Account Account { get; set; }
}