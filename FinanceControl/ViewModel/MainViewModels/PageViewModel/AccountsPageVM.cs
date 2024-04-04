using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using FinanceControl.Model;

namespace FinanceControl.ViewModel.MainViewModels.PageViewModel
{
    internal class AccountsPageVM : ViewModelBase
    {
        private int _loggedInUserId;
        private Accounts _selectedAccount;
        private string _accountName;
        private decimal _balance;

        FinanceControl_DBEntities context;

        public ObservableCollection<Accounts> UserAccounts { get; set; }


        public Accounts IsAccountSelected
        {
            get { return _selectedAccount; }
            set
            {
                if (_selectedAccount != value)
                {
                    _selectedAccount = value;
                    OnPropertyChanged(nameof(IsAccountSelected));
                    LoadSelectedAccountDetails(); // Загрузка деталей выбранного аккаунта
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
                    string stringValue = value.ToString("0.##"); // Форматирование с двумя знаками после точки, но без лишних нулей
                    if (IsValidInput(stringValue))
                    {
                        _balance = value;
                        OnPropertyChanged(nameof(Balance));
                    }
                    else
                    {
                        // Обработка недопустимого ввода, например, выдача сообщения об ошибке.
                        MessageBox.Show("Ошибка вы можете ввести до 18 знаков перед запятой", "Ошибка");
                    }
                }
            }
        }

        public ICommand DeleteSelectedAccountCommand { get; }
        public ICommand UpdateSelectedAccountCommand { get; }
        public ICommand AddNewAccountCommand { get; }

        public AccountsPageVM(int loggedInUserId, FinanceControl_DBEntities transContext)
        {
            context = transContext;
            _loggedInUserId = loggedInUserId;
            UserAccounts = new ObservableCollection<Accounts>();
            LoadUserAccounts();
            DeleteSelectedAccountCommand = new ViewModelCommand(DeleteSelectedAccount, CanDeleteSelectedAccount);
            UpdateSelectedAccountCommand = new ViewModelCommand(UpdateSelectedAccount, CanUpdateSelectedAccount);
            AddNewAccountCommand = new ViewModelCommand(AddNewAccount, CanAddNewAccount);


        }

        private void LoadUserAccounts()
        {
            UserAccounts.Clear(); // Очищаем коллекцию перед загрузкой
            var accounts = context.Accounts.Where(account => account.UserID == _loggedInUserId).ToList();
            foreach (var account in accounts)
            {
                UserAccounts.Add(account);
            }

            OnPropertyChanged(nameof(UserAccounts)); // Уведомляем об изменении свойства для обновления привязки в UI
        }

        private void LoadSelectedAccountDetails()
        {
            if (IsAccountSelected != null && context.Entry(IsAccountSelected).State != EntityState.Detached)
            {
                AccountName = IsAccountSelected.AccountName;
                Balance = IsAccountSelected.Balance; // Здесь Balance должен быть типа decimal
            }
        }

        private void DeleteSelectedAccount(object parameter)
        {
            if (IsAccountSelected != null)
            {
                // Получаем ID аккаунта, который будем удалять
                var accountIdToDelete = IsAccountSelected.AccountID;

                // Удаляем все зависимые записи из таблицы ExpensesTransactions, связанные с выбранным аккаунтом
                var expenses = context.ExpensesTransactions.Where(e => e.AccountID == accountIdToDelete);
                context.ExpensesTransactions.RemoveRange(expenses);

                // Удаляем все зависимые записи из таблицы IncomeTransactions, связанные с выбранным аккаунтом
                var incomes = context.IncomeTransactions.Where(i => i.AccountID == accountIdToDelete);
                context.IncomeTransactions.RemoveRange(incomes);

                // Удаляем все зависимые записи из таблицы DebtTransactions, связанные с выбранным аккаунтом
                var debts = context.DebtsTransactions.Where(d => d.AccountID == accountIdToDelete);
                context.DebtsTransactions.RemoveRange(debts);

                // Удаляем все зависимые записи из таблицы GoalTransactions, связанные с выбранным аккаунтом
                var goals = context.GoalsTransactions.Where(g => g.AccountID == accountIdToDelete);
                context.GoalsTransactions.RemoveRange(goals);

                // Получаем выбранный аккаунт из базы данных
                var accountToDelete = context.Accounts.SingleOrDefault(a => a.AccountID == accountIdToDelete);

                // Если аккаунт найден
                if (accountToDelete != null)
                {
                    // Удаляем выбранный аккаунт из базы данных
                    context.Accounts.Remove(accountToDelete);

                    // Удаляем выбранный аккаунт из коллекции пользовательских аккаунтов
                    UserAccounts.Remove(IsAccountSelected);

                    // Сохраняем изменения
                    context.SaveChanges();

                    // Сбросить выбор после удаления
                    EndOperation();
                }
            }
        }
        private bool CanDeleteSelectedAccount(object parameter)
        {
            return IsAccountSelected != null;
        }

        private void UpdateSelectedAccount(object parameter)
        {
            if (IsAccountSelected != null)
            {
                // Обновляем свойства аккаунта
                IsAccountSelected.Balance = Balance;
                IsAccountSelected.AccountName = AccountName;
                // Обновить коллекцию после изменений в базе данных
                LoadUserAccounts();
                EndOperation();
            }
        }

        private void AddNewAccount(object parameter)
        {
            // Создаем новый аккаунт
            var newAccount = new Accounts
            {
                UserID = _loggedInUserId, // Присваиваем значение UserID
                AccountID = GetNextAccountId(), // Получаем следующее значение для AccountID
                AccountName = AccountName,
                Balance = Balance

            };
            // Добавляем новый аккаунт в базу данных
            context.Accounts.Add(newAccount);
            // Добавляем новый аккаунт в коллекцию
            UserAccounts.Add(newAccount);
            EndOperation();
        }

        private bool CanUpdateSelectedAccount(object parameter)
        {
            return IsAccountSelected != null;
        }

        private bool CanAddNewAccount(object parameter)
        {
            if (string.IsNullOrWhiteSpace(AccountName) || Balance == 0.0m)
            {
                return false;
            }
            else
            {
                return true;
            }


        }


        private int GetNextAccountId()
        {
            // Находим максимальное значение AccountID в коллекции
            int maxAccountId = context.Accounts.Any() ? context.Accounts.Max(account => account.AccountID) : 0;
            // Возвращаем следующее значение, увеличенное на 1
            return maxAccountId + 1;
        }

        private void EndOperation()
        {
            context.SaveChanges();
            IsAccountSelected = null;
            Balance = 0.00m;
            AccountName = null;
        }

        private bool IsValidInput(string input)
        {
            string pattern = @"^\d{1,18}(\.\d{2})?$";
            return Regex.IsMatch(input, pattern);
        }


    }
}