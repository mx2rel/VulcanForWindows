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
        Account GetByPupilIdAsync(int id);
        void UpdateAccountAsync(Account account);
        void UpdateAccountsAsync(IEnumerable<Account> accounts);
        void DeleteByIdAsync(int id);
    }
}