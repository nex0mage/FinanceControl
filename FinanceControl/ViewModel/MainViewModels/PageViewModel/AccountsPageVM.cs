using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FinanceControl.Model;

namespace FinanceControl.ViewModel.MainViewModels.PageViewModel
{
    internal class AccountsPageVM : ViewModelBase
    {
        private int _loggedInUserId;

        private Accounts _selectedAccount;

        private string _accountName;
        private decimal _balance;


        FinanceControl_DB_Entities context = new FinanceControl_DB_Entities();

        public ObservableCollection<Accounts> UserAccounts { get; set; }


        public int LoggedInUserId
        {
            get { return _loggedInUserId; }
            set
            {
                if (_loggedInUserId != value)
                {
                    _loggedInUserId = value;
                    OnPropertyChanged(nameof(LoggedInUserId));
                }
            }
        }

        public Accounts IsAccountSelected
        {
            get { return _selectedAccount; }
            set
            {
                if (_selectedAccount != value)
                {
                    _selectedAccount = value;
                    OnPropertyChanged(nameof(IsAccountSelected));
                }
            }
        }

        public string AccountName
        {
            get { return _accountName; }
            set
            {
                if (_accountName != value)
                {
                    _accountName = value;
                    OnPropertyChanged(nameof(AccountName));
                }
            }
        }


        public decimal Balance
        {
            get { return _balance; }
            set
            {
                if (_balance != value)
                {
                    _balance = value;
                    OnPropertyChanged(nameof(Balance));
                }
            }
        }

        public ICommand DeleteSelectedAccountCommand { get; }

        public AccountsPageVM(int loggedInUserId) 
        {
            LoggedInUserId = loggedInUserId;
            LoadUserAccounts();
            DeleteSelectedAccountCommand = new ViewModelCommand(DeleteSelectedAccount, CanDeleteSelectedAccount);
        }

        private void LoadUserAccounts()
        {
            UserAccounts = new ObservableCollection<Accounts>(context.Accounts.Where(account => account.UserID == _loggedInUserId).ToList());
        }

        private void DeleteSelectedAccount(object parameter)
        {
            if (IsAccountSelected != null)
            {
                // Удалить из базы данных
                context.Accounts.Remove(IsAccountSelected);
                context.SaveChanges();

                // Удалить из коллекции
                UserAccounts.Remove(IsAccountSelected);

                // Сбросить выбор после удаления
                IsAccountSelected = null;
            }
        }

        private bool CanDeleteSelectedAccount(object parameter)
        {
            return IsAccountSelected != null;
        }







    }
}
