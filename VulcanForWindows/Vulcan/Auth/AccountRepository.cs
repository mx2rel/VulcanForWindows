using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Vulcanova.Features.Auth.Accounts;

namespace Vulcanova.Features.Auth
{
    public class AccountRepository : IAccountRepository
    {
        private List<Account> accounts;
        private readonly string dataFilePath;

        public AccountRepository(string dataFilePath)
        {
            this.dataFilePath = dataFilePath;
            LoadData();
        }
        public AccountRepository()
        {
            this.dataFilePath = "C:/Vulcan/accounts.txt";
            LoadData();
        }

        private void LoadData()
        {
            if (File.Exists(dataFilePath))
            {
                string jsonData = File.ReadAllText(dataFilePath);
                accounts = JsonSerializer.Deserialize<List<Account>>(jsonData);
            }
            else
            {
                accounts = new List<Account>();
            }
        }

        private void SaveData()
        {
            string jsonData = JsonSerializer.Serialize(accounts);
            File.WriteAllText(dataFilePath, jsonData);
        }

        public void AddAccounts(IEnumerable<Account> newAccounts)
        {
            accounts.AddRange(newAccounts.Where(r=>!GetAccounts().Select(j=>j.Pupil.Id).Contains( r.Pupil.Id)));
            SaveData();
        }

        public bool SetActiveByPupilId(int accountId)
        {
            var accountToActivate = GetByPupilId(accountId);
            if (accountToActivate != null)
                if (accountToActivate.Pupil.Id != ((GetActiveAccount() == null) ? (0) :(GetActiveAccount().Pupil.Id)))
                {
                    var a = new List<Account>(accounts);
                    foreach (var account in a)
                    {
                        account.IsActive = false;
                    }
                    UpdateAccounts(a);
                    accountToActivate.IsActive = true;
                    UpdateAccount(accountToActivate);
                    return true;
                }
            return false;
        }

        public Account GetActiveAccount()
        {
            return accounts.FirstOrDefault(a => a.IsActive);
        }

        public IReadOnlyCollection<Account> GetAccounts()
        {
            return accounts.AsReadOnly();
        }

        public Account GetById(int id)
        {
            return accounts.FirstOrDefault(a => a.Id == id);
        }


        public Account GetByPupilId(int id)
        {
            return accounts.FirstOrDefault(a => a.Pupil.Id == id);
        }

        public void UpdateAccount(Account account)
        {
            int index = accounts.FindIndex(a => a.Pupil.Id == account.Pupil.Id);
            if (index != -1)
            {
                accounts[index] = account;
                SaveData();
            }
        }

        public void UpdateAccounts(IEnumerable<Account> updatedAccounts)
        {
            foreach (var updatedAccount in updatedAccounts)
            {
                int index = accounts.FindIndex(a => a.Pupil.Id == updatedAccount.Pupil.Id);
                if (index != -1)
                {
                    accounts[index] = updatedAccount;
                }
            }
            SaveData();
        }

        public void DeleteById(int id)
        {
            int index = accounts.FindIndex(a => a.Id == id);
            if (index != -1)
            {
                accounts.RemoveAt(index);
                SaveData();
            }
        }

        public void DeleteByPupilId(int id)
        {
            int index = accounts.FindIndex(a => a.Pupil.Id == id);
            if (index != -1)
            {
                accounts.RemoveAt(index);
                SaveData();
            }
        }
        public void Logout()
        {
            accounts = new List<Account>();
            SaveData();
        }
    }
}
