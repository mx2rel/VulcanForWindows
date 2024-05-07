using System.Collections.Generic;
using System.Threading.Tasks;
using Vulcanova.Features.Auth.Accounts;

namespace Vulcanova.Features.Auth
{
    public interface IAccountRepository
    {
        void AddAccounts(IEnumerable<Account> accounts);
        Account GetActiveAccount();
        IReadOnlyCollection<Account> GetAccounts();
        Account GetByPupilId(int id);
        void UpdateAccount(Account account);
        void UpdateAccounts(IEnumerable<Account> accounts);
        void DeleteById(int id);
    }
}