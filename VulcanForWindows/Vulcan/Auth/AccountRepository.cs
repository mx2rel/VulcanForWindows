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

        public void AddAccountsAsync(IEnumerable<Account> newAccounts)
        {
            accounts.AddRange(newAccounts);
            SaveData();
        }

        public Account GetActiveAccountAsync()
        {
            return accounts.FirstOrDefault(a => a.IsActive);
        }

        public IReadOnlyCollection<Account> GetAccountsAsync()
        {
            return accounts.AsReadOnly();
        }

        public Account GetByIdAsync(int id)
        {
            return accounts.FirstOrDefault(a => a.Id == id);
        }


        public Account GetByPupilIdAsync(int id)
        {
            return accounts.FirstOrDefault(a => a.Pupil.Id == id);
        }

        public void UpdateAccountAsync(Account account)
        {
            int index = accounts.FindIndex(a => a.Id == account.Id);
            if (index != -1)
            {
                accounts[index] = account;
                SaveData();
            }
        }

        public void UpdateAccountsAsync(IEnumerable<Account> updatedAccounts)
        {
            foreach (var updatedAccount in updatedAccounts)
            {
                int index = accounts.FindIndex(a => a.Id == updatedAccount.Id);
                if (index != -1)
                {
                    accounts[index] = updatedAccount;
                }
            }
            SaveData();
        }

        public void DeleteByIdAsync(int id)
        {
            int index = accounts.FindIndex(a => a.Id == id);
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
