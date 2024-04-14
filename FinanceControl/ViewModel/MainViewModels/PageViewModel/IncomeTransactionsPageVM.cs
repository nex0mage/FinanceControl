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
        private DateTime _incomeTransactionDate = new DateTime(2023, 01, 01);
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


            // Создаем новые ObservableCollection и добавляем элементы из списков
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
                    string stringValue = value.ToString("0.##"); // Форматирование с двумя знаками после точки, но без лишних нулей
                    if (IsValidInput(stringValue))
                    {
                        _amount = value;
                        OnPropertyChanged(nameof(Amount));
                    }
                    else
                    {
                        // Обработка недопустимого ввода, например, выдача сообщения об ошибке.
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
                // Удаляем из базы данных
                context.IncomeTransactions.Remove(IsIncomeTransactionSelected);
                // Удаляем из коллекции
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
                // Обновляем в базе данных
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
                // Уменьшаем баланс у старого кошелька получателя
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
            // Добавляем в коллекцию
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
                Comment = IsIncomeTransactionSelected.Comment; // Здесь Balance должен быть типа decimal
                IncomeTransactionDate = IsIncomeTransactionSelected.TransactionDate;
                IncomeCategory = IsIncomeTransactionSelected.IncomeCategories;
                AccountTo = IsIncomeTransactionSelected.Accounts; // Предположим, что Accounts относится к AccountFrom

            }
        }

        private void LoadUserIncomeTransactions()
        {
            UserIncomeTransactions.Clear(); // Очищаем коллекцию перед загрузкой

            var incometransactions = context.IncomeTransactions.Where(incometransaction => incometransaction.Accounts.Users.UserID == _loggedInUserId).ToList();

            foreach (var incometransaction in incometransactions)
            {
                UserIncomeTransactions.Add(incometransaction);
            }

            OnPropertyChanged(nameof(UserIncomeTransactions)); // Уведомляем об изменении свойства для обновления привязки в UI
        }

        private int GetNextTransferId()
        {
            // Находим максимальное значение AccountID в коллекции
            int maxIncomeTransactionId = context.IncomeTransactions.Any() ? context.IncomeTransactions.Max(transaction => transaction.IncomeTransactionID) : 0;


            // Возвращаем следующее значение, увеличенное на 1
            return maxIncomeTransactionId + 1;
        }

        private void EndOperation()
        {
            context.SaveChanges();
            IsIncomeTransactionSelected = null;
            Amount = 0.00m;
            IncomeCategory = null;
            AccountTo = null;
            IncomeTransactionDate = new DateTime(2023, 01, 01);
            Comment = null;
        }

        private bool IsValidInput(string input)
        {
            string pattern = @"^\d{1,18}(\.\d{2})?$";
            return Regex.IsMatch(input, pattern);
        }


    }
}
