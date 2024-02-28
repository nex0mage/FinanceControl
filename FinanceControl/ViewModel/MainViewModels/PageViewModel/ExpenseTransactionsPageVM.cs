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
    internal class ExpenseTransactionsPageVM : ViewModelBase
    {
        private int _loggedInUserId;
        private FinanceControl_DBEntities context;
        private ExpensesTransactions _selectedExpenseTransaction;

        private Accounts _accountFrom;
        private string _comment;
        private ExpensesCategories _expenseCategory;
        private DateTime _expenseTransactionDate = new DateTime(2023, 01, 01);
        private decimal _amount;


        public ObservableCollection<ExpensesTransactions> UserExpenseTransactions { get; set; }

        private ObservableCollection<Accounts> _accounts;
        private ObservableCollection<ExpensesCategories> _expenseCategories;

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

        public Accounts AccountFrom
        {
            get { return _accountFrom; }
            set
            {
                if (_accountFrom != value)
                {
                    _accountFrom = value;
                    OnPropertyChanged(nameof(AccountFrom));
                }
            }
        }

        public ObservableCollection<ExpensesCategories> ExpenseCategories
        {
            get { return _expenseCategories; }
            set
            {
                if (_expenseCategories != value)
                {
                    _expenseCategories = value;
                    OnPropertyChanged(nameof(ExpenseCategories));
                }
            }

        }

        public ExpensesCategories ExpenseCategory
        {
            get { return _expenseCategory; }
            set
            {
                if (_expenseCategory != value)
                {
                    _expenseCategory = value;
                    OnPropertyChanged(nameof(ExpenseCategory));
                }
            }
        }

        private void LoadAccountsAndCategoriesFromContext()
        {
            var accountsFrom = context.Accounts.Where(accountFrom => accountFrom.Users.UserID == _loggedInUserId).ToList();
            var categories = context.ExpensesCategories.Where(category => category.Users.UserID == _loggedInUserId).ToList();


            // Создаем новые ObservableCollection и добавляем элементы из списков
            Accounts = new ObservableCollection<Accounts>(accountsFrom);
            ExpenseCategories = new ObservableCollection<ExpensesCategories>(categories);

            OnPropertyChanged(nameof(Accounts));
            OnPropertyChanged(nameof(ExpenseCategories) );
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

        public DateTime ExpenseTransactionDate
        {
            get => _expenseTransactionDate;
            set
            {
                if (_expenseTransactionDate != value)
                {
                    _expenseTransactionDate = value;
                    OnPropertyChanged(nameof(ExpenseTransactionDate));
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

        public ExpensesTransactions IsExpenseTransactionSelected
        {
            get { return _selectedExpenseTransaction; }
            set
            {
                if (_selectedExpenseTransaction != value)
                {
                    _selectedExpenseTransaction = value;
                    OnPropertyChanged(nameof(IsExpenseTransactionSelected));
                    LoadSelectedExpenseTransactionDetails();

                }
            }

        }

        public ICommand DeleteSelectedTransactionCommand { get; }
        public ICommand UpdateSelectedTransactionCommand { get; }
        public ICommand AddNewTransactionCommand { get; }


        public ExpenseTransactionsPageVM(int loggedInUserId, FinanceControl_DBEntities transContext)
        {
            context = transContext;
            _loggedInUserId = loggedInUserId;
            UserExpenseTransactions = new ObservableCollection<ExpensesTransactions>();
            LoadAccountsAndCategoriesFromContext();
            LoadUserExpenseTransactions();
            DeleteSelectedTransactionCommand = new ViewModelCommand(DeleteSelectedTransaction, CanDeleteSelectedTransaction);
            UpdateSelectedTransactionCommand = new ViewModelCommand(UpdateSelectedTransaction, CanUpdateSelectedTransactions);
            AddNewTransactionCommand = new ViewModelCommand(AddNewTransaction, CanAddNewTransaction);

        }

        private void DeleteSelectedTransaction(object parameter)
        {
            if (IsExpenseTransactionSelected != null)
            {
                int? oldAccountToID = IsExpenseTransactionSelected.AccountID;
                var oldAccountTo = context.Accounts.Find(oldAccountToID);
                oldAccountTo.Balance += IsExpenseTransactionSelected.Amount;

                // Удаляем из базы данных
                context.ExpensesTransactions.Remove(IsExpenseTransactionSelected);
                // Удаляем из коллекции
                UserExpenseTransactions.Remove(IsExpenseTransactionSelected);
                EndOperation();
            }
        }

        private bool CanDeleteSelectedTransaction(object parameter)
        {
            return IsExpenseTransactionSelected != null;
        }

        private void UpdateSelectedTransaction(object parameter)
        {
            if (IsExpenseTransactionSelected != null)
            {
                if (AccountFrom.Balance - Amount >= 0)
                {
                    // Обновляем в базе данных
                    IsExpenseTransactionSelected.Comment = Comment;
                    IsExpenseTransactionSelected.TransactionDate = ExpenseTransactionDate;
                    IsExpenseTransactionSelected.AccountID = AccountFrom.AccountID;
                    IsExpenseTransactionSelected.ExpenseCategoryID = ExpenseCategory.ExpenseCategoryID;

                    int? oldAccountFromID = IsExpenseTransactionSelected.AccountID;

                    UpdateAccountBalances(oldAccountFromID, AccountFrom.AccountID);

                }
                else
                {
                    MessageBox.Show("Транзакция не может быть проведена, т.к. на выбранном счете нет столько денег.", "Ошибка!");
                }
            }

        }

        private void UpdateAccountBalances(int? oldAccountFromID, int newAccountFromID)
        {
            if (oldAccountFromID != newAccountFromID)
            {
                // Уменьшаем баланс у старого кошелька получателя
                var oldAccountFrom = context.Accounts.Find(oldAccountFromID);
                oldAccountFrom.Balance += IsExpenseTransactionSelected.Amount;
            }

            decimal difference = Amount - IsExpenseTransactionSelected.Amount;
            AccountFrom.Balance -= difference;

            IsExpenseTransactionSelected.Amount = Amount;
            IsExpenseTransactionSelected.Accounts = AccountFrom;

            EndOperation();
            LoadUserExpenseTransactions();
        }

        private bool CanUpdateSelectedTransactions(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || Amount == 0.0m || AccountFrom == null || ExpenseCategory == null)
            {
                return false;
            }
            else
            {
                return IsExpenseTransactionSelected != null;

            }
        }

        private void AddNewTransaction(object parameter)
        {
            if (AccountFrom.Balance - Amount >=0)
            {
                ExpensesTransactions newExpenseTransaction = new ExpensesTransactions
                {
                    ExpenseTransactionID = GetNextTransferId(),
                    Comment = Comment,
                    TransactionDate = ExpenseTransactionDate,
                    AccountID = AccountFrom.AccountID,
                    Accounts = AccountFrom,
                    Amount = Amount,
                    ExpenseCategoryID = ExpenseCategory.ExpenseCategoryID
                };
                AccountFrom.Balance = AccountFrom.Balance - Amount;
                context.ExpensesTransactions.Add(newExpenseTransaction);
                // Добавляем в коллекцию
                UserExpenseTransactions.Add(newExpenseTransaction);

                EndOperation();

            }
            else
            {
                MessageBox.Show("Транзакция не может быть проведена, т.к. на выбранном счете нет столько денег.", "Ошибка!");
            }
        }

        private bool CanAddNewTransaction(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || Amount == 0.0m || AccountFrom == null)
            {
                return false;
            }
            else
            {
                return true;
            }


        }

        private void LoadSelectedExpenseTransactionDetails()
        {
            if (IsExpenseTransactionSelected != null && context.Entry(IsExpenseTransactionSelected).State != EntityState.Detached)
            {
                Amount = IsExpenseTransactionSelected.Amount;
                Comment = IsExpenseTransactionSelected.Comment; // Здесь Balance должен быть типа decimal
                ExpenseTransactionDate = IsExpenseTransactionSelected.TransactionDate;
                ExpenseCategory = IsExpenseTransactionSelected.ExpensesCategories;
                AccountFrom = IsExpenseTransactionSelected.Accounts; // Предположим, что Accounts относится к AccountFrom

            }
        }

        private void LoadUserExpenseTransactions()
        {
            UserExpenseTransactions.Clear(); // Очищаем коллекцию перед загрузкой

            var expensetransactions = context.ExpensesTransactions.Where(expensetransaction => expensetransaction.Accounts.Users.UserID == _loggedInUserId).ToList();

            foreach (var expensetransaction in expensetransactions)
            {
                UserExpenseTransactions.Add(expensetransaction);
            }

            OnPropertyChanged(nameof(UserExpenseTransactions)); // Уведомляем об изменении свойства для обновления привязки в UI
        }

        private int GetNextTransferId()
        {
            // Находим максимальное значение AccountID в коллекции
            int maxExpenseTransactionId = context.ExpensesTransactions.Any() ? context.ExpensesTransactions.Max(transaction => transaction.ExpenseTransactionID) : 0;


            // Возвращаем следующее значение, увеличенное на 1
            return maxExpenseTransactionId + 1;
        }

        private void EndOperation()
        {
            context.SaveChanges();
            IsExpenseTransactionSelected = null;
            Amount = 0.00m;
            ExpenseCategory = null;
            AccountFrom = null;
            ExpenseTransactionDate = new DateTime(2023, 01, 01);
            Comment = null;
        }

        private bool IsValidInput(string input)
        {
            string pattern = @"^\d{1,18}(\.\d{2})?$";
            return Regex.IsMatch(input, pattern);
        }



    }
}
