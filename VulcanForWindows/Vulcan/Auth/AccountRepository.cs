using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using VulcanForWindows;
using VulcanForWindows.Preferences;
using Vulcanova.Features.Auth.Accounts;

namespace Vulcanova.Features.Auth
{
    public class AccountRepository : IAccountRepository
    {


        public delegate void ChangedActiveAccountEvent(Account newActiveAccount);
        public delegate void AccountsChangedEvent();

        public static ChangedActiveAccountEvent OnActiveAccountChanged = delegate { };
        public static AccountsChangedEvent OnAccountsChanged = delegate { };

        private List<Account> accounts;
        private static string dataFilePath
            => Path.Combine(PreferencesManager.folder, "accounts.txt");


        public AccountRepository(string dataFilePath)
        {
            LoadData();

            OnActiveAccountChanged += (Account a) =>
                OnAccountsChanged.Invoke();
        }
        public AccountRepository()
        {
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
            var filteredAccountsToAdd = newAccounts.Where(r => !GetAccounts().Select(j => j.Pupil.Id).Contains(r.Pupil.Id));
            accounts.AddRange(filteredAccountsToAdd);
            SaveData();
            OnAccountsChanged.Invoke();
        }

        public bool SetActiveByPupilId(int accountId)
        {
            var accountToActivate = GetByPupilId(accountId);
            if (accountToActivate != null)
                if (accountToActivate.Pupil.Id != ((GetActiveAccount() == null) ? (0) : (GetActiveAccount().Pupil.Id)))
                {
                    var a = new List<Account>(accounts);
                    foreach (var account in a)
                    {
                        account.IsActive = false;
                    }
                    UpdateAccounts(a, false);
                    accountToActivate.IsActive = true;
                    UpdateAccount(accountToActivate, false);
                    OnActiveAccountChanged.Invoke(accountToActivate);
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

        public void UpdateAccount(Account account, bool invokeEvent = true)
        {
            int index = accounts.FindIndex(a => a.Pupil.Id == account.Pupil.Id);
            if (index != -1)
            {
                accounts[index] = account;
                if (invokeEvent) OnAccountsChanged.Invoke();
                SaveData();
            }
        }

        public void UpdateAccounts(IEnumerable<Account> updatedAccounts, bool invokeEvent = true)
        {
            foreach (var updatedAccount in updatedAccounts)
            {
                int index = accounts.FindIndex(a => a.Pupil.Id == updatedAccount.Pupil.Id);
                if (index != -1)
                {
                    accounts[index] = updatedAccount;
                }
            }
            if (invokeEvent) OnAccountsChanged.Invoke();
            SaveData();
        }

        public void DeleteById(int id)
        {
            int index = accounts.FindIndex(a => a.Id == id);
            if (index == -1) return;
            var pupilId = accounts[index].Pupil.Id;
            DeleteByPupilId(pupilId);
        }

        public void DeleteByPupilId(int id)
        {
            int index = accounts.FindIndex(a => a.Pupil.Id == id);
            if (index == -1) return;

            var account = accounts[index];
            bool updateActiveAccountLaterOn = GetActiveAccount() == account;

            accounts.RemoveAt(index);

            if (updateActiveAccountLaterOn)
            {
                if (accounts.Count != 0)
                    SetActiveByPupilId(accounts[0].Pupil.Id);
            }

            SaveData();
            OnAccountsChanged.Invoke();

            if (accounts.Count == 0)
            {
                MainWindow.Instance.Logout();
            }
            else
            {
                MainWindow.Instance.LoadMainPage();
            }
        }

        public void Logout()
        {
            accounts = new List<Account>();
            SaveData();
        }
    }
}
