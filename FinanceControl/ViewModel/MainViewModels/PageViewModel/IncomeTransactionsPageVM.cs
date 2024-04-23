using FinanceControl.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FinanceControl.ViewModel.MainViewModels.PageViewModel
{
    internal class IncomeTransactionsPageVM : ViewModelBase
    {
        private int _loggedInUserId;
        private FinanceControl_DBEntities context;
        private IncomeTransactions _selectedIncomeTransaction;

        private Accounts _accountTo;
        private string _comment;
        private IncomeCategories _incomeCategory;
        private DateTime _incomeTransactionDate = new DateTime(2024, 01, 01);
        private decimal _amount;

        public ObservableCollection<IncomeTransactions> UserIncomeTransactions { get; set; }

        private ObservableCollection<Accounts> _accounts;
        private ObservableCollection<IncomeCategories> _incomeCategories;

        public ObservableCollection<Accounts> Accounts
        {
            get { return _accounts; }
            set
            {
                if (_accounts != value)
                {
                    _accounts = value;
                    OnPropertyChanged(nameof(Accounts));
                }
            }
        }

        public Accounts AccountTo
        {
            get { return _accountTo; }
            set
            {
                if (_accountTo != value)
                {
                    _accountTo = value;
                    OnPropertyChanged(nameof(AccountTo));
                }
            }
        }

        public ObservableCollection<IncomeCategories> IncomeCategories
        {
            get { return _incomeCategories; }
            set
            {
                if (_incomeCategories != value)
                {
                    _incomeCategories = value;
                    OnPropertyChanged(nameof(IncomeCategories));
                }
            }

        }

        public IncomeCategories IncomeCategory
        {
            get { return _incomeCategory; }
            set
            {
                if (_incomeCategory != value)
                {
                    _incomeCategory = value;
                    OnPropertyChanged(nameof(IncomeCategory));
                }
            }
        }

        private void LoadAccountsAndCategoriesFromContext()
        {
            var accountsTo = context.Accounts.Where(accountTo => accountTo.Users.UserID == _loggedInUserId).ToList();
            var categories = context.IncomeCategories.Where(category => category.Users.UserID == _loggedInUserId).ToList();
            Accounts = new ObservableCollection<Accounts>(accountsTo);
            IncomeCategories = new ObservableCollection<IncomeCategories>(categories);
            OnPropertyChanged(nameof(Accounts));
            OnPropertyChanged(nameof(IncomeCategories));
        }

        public string Comment
        {
            get => _comment;
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    OnPropertyChanged(nameof(Comment));
                }
            }
        }

        public DateTime IncomeTransactionDate
        {
            get => _incomeTransactionDate;
            set
            {
                if (_incomeTransactionDate != value)
                {
                    _incomeTransactionDate = value;
                    OnPropertyChanged(nameof(IncomeTransactionDate));
                }
            }
        }
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    string stringValue = value.ToString("0.##");
                    if (IsValidInput(stringValue))
                    {
                        _amount = value;
                        OnPropertyChanged(nameof(Amount));
                    }
                    else
                    {
                        MessageBox.Show("Ошибка вы можете ввести до 18 знаков перед запятой", "Ошибка");
                    }
                }
            }
        }

        public IncomeTransactions IsIncomeTransactionSelected
        {
            get { return _selectedIncomeTransaction; }
            set
            {
                if (_selectedIncomeTransaction != value)
                {
                    _selectedIncomeTransaction = value;
                    OnPropertyChanged(nameof(IsIncomeTransactionSelected));
                    LoadSelectedIncomeTransactionDetails();
                }
            }
        }
        public ICommand DeleteSelectedTransactionCommand { get; }
        public ICommand UpdateSelectedTransactionCommand { get; }
        public ICommand AddNewTransactionCommand { get; }
        public IncomeTransactionsPageVM(int loggedInUserId, FinanceControl_DBEntities transContext)
        {
            context = transContext;
            _loggedInUserId = loggedInUserId;
            UserIncomeTransactions = new ObservableCollection<IncomeTransactions>();
            LoadAccountsAndCategoriesFromContext();
            LoadUserIncomeTransactions();
            DeleteSelectedTransactionCommand = new ViewModelCommand(DeleteSelectedTransaction, CanDeleteSelectedTransaction);
            UpdateSelectedTransactionCommand = new ViewModelCommand(UpdateSelectedTransaction, CanUpdateSelectedTransactions);
            AddNewTransactionCommand = new ViewModelCommand(AddNewTransaction, CanAddNewTransaction);
        }
        private void DeleteSelectedTransaction(object parameter)
        {
            if (IsIncomeTransactionSelected != null)
            {
                int? oldAccountToID = IsIncomeTransactionSelected.AccountID;
                var oldAccountTo = context.Accounts.Find(oldAccountToID);
                oldAccountTo.Balance -= IsIncomeTransactionSelected.Amount;
                context.IncomeTransactions.Remove(IsIncomeTransactionSelected);
                UserIncomeTransactions.Remove(IsIncomeTransactionSelected);
                EndOperation();
            }
        }
        private bool CanDeleteSelectedTransaction(object parameter)
        {
            return IsIncomeTransactionSelected != null;
        }
        private void UpdateSelectedTransaction(object parameter)
        {
            if (IsIncomeTransactionSelected != null)
            {
                IsIncomeTransactionSelected.Comment = Comment;
                IsIncomeTransactionSelected.TransactionDate = IncomeTransactionDate;
                IsIncomeTransactionSelected.AccountID = AccountTo.AccountID;
                IsIncomeTransactionSelected.IncomeCategoryID = IncomeCategory.IncomeCategoryID;
                int? oldAccountToID = IsIncomeTransactionSelected.AccountID;
                UpdateAccountBalances( oldAccountToID, AccountTo.AccountID);
            }
        }
        private void UpdateAccountBalances ( int? oldAccountToID, int newAccountToID)
        {
            if (oldAccountToID != newAccountToID)
            {
                var oldAccountTo = context.Accounts.Find(oldAccountToID);
                oldAccountTo.Balance -= IsIncomeTransactionSelected.Amount;
            }
            decimal difference = Amount - IsIncomeTransactionSelected.Amount;
            AccountTo.Balance += difference;
            IsIncomeTransactionSelected.Amount = Amount;
            IsIncomeTransactionSelected.Accounts = AccountTo;
            EndOperation();
            LoadUserIncomeTransactions();
        }
        private bool CanUpdateSelectedTransactions(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || Amount == 0.0m || AccountTo == null || IncomeCategory == null)
            {
                return false;
            }
            else
            {
                return IsIncomeTransactionSelected != null;
            }
        }
        private void AddNewTransaction(object parameter)
        {
            IncomeTransactions newIncomeTransaction = new IncomeTransactions
            {
                IncomeTransactionID = GetNextTransferId(),
                Comment = Comment,
                TransactionDate = IncomeTransactionDate,
                AccountID = AccountTo.AccountID,
                Accounts = AccountTo,
                Amount = Amount
            };
            AccountTo.Balance = AccountTo.Balance + Amount;
            context.IncomeTransactions.Add(newIncomeTransaction);
            UserIncomeTransactions.Add(newIncomeTransaction);
            EndOperation();
        }
        private bool CanAddNewTransaction(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || Amount == 0.0m || AccountTo == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void LoadSelectedIncomeTransactionDetails()
        {
            if (IsIncomeTransactionSelected != null && context.Entry(IsIncomeTransactionSelected).State != EntityState.Detached)
            {
                Amount = IsIncomeTransactionSelected.Amount;
                Comment = IsIncomeTransactionSelected.Comment;
                IncomeTransactionDate = IsIncomeTransactionSelected.TransactionDate;
                IncomeCategory = IsIncomeTransactionSelected.IncomeCategories;
                AccountTo = IsIncomeTransactionSelected.Accounts; 
            }
        }

        private void LoadUserIncomeTransactions()
        {
            UserIncomeTransactions.Clear(); 
            var incometransactions = context.IncomeTransactions
                .Where(incometransaction => incometransaction.Accounts.Users.UserID == _loggedInUserId)
                .OrderByDescending(incometransaction => incometransaction.TransactionDate)
                .ToList();
            foreach (var incometransaction in incometransactions)
            {
                UserIncomeTransactions.Add(incometransaction);
            }
            OnPropertyChanged(nameof(UserIncomeTransactions)); 
        }

        private int GetNextTransferId()
        {
            int maxIncomeTransactionId = context.IncomeTransactions.Any() ? context.IncomeTransactions.Max(transaction => transaction.IncomeTransactionID) : 0;
            return maxIncomeTransactionId + 1;
        }

        private void EndOperation()
        {
            context.SaveChanges();
            IsIncomeTransactionSelected = null;
            Amount = 0.00m;
            IncomeCategory = null;
            AccountTo = null;
            IncomeTransactionDate = new DateTime(2024, 01, 01);
            Comment = null;
            LoadUserIncomeTransactions();
        }

        private bool IsValidInput(string input)
        {
            string pattern = @"^\d{1,18}(\.\d{2})?$";
            return Regex.IsMatch(input, pattern);
        }
    }
}
