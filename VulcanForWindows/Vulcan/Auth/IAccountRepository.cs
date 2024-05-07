using System.Collections.Generic;
using System.Threading.Tasks;
using Vulcanova.Features.Auth.Accounts;

namespace Vulcanova.Features.Auth
{
    public interface IAccountRepository
    {
        void AddAccountsAsync(IEnumerable<Account> accounts);
        Account GetActiveAccountAsync();
        IReadOnlyCollection<Account> GetAccountsAsync();
        Account GetByPupilId(int id);
        void UpdateAccount(Account account);
        void UpdateAccounts(IEnumerable<Account> accounts);
        void DeleteById(int id);
    }
}