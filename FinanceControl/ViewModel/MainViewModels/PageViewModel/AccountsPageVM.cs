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
                    _balance = value;
                    ValidateInput(); // Передаем _balance для валидации
                    OnPropertyChanged(nameof(Balance));
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
            AddNewAccountCommand = new ViewModelCommand(AddNewAccount);


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
                var entry = context.Entry(IsAccountSelected);

                if (entry.State == EntityState.Detached)
                {
                    // Если объект не прикреплен, прикрепим его
                    context.Accounts.Attach(IsAccountSelected);
                }

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

        private void UpdateSelectedAccount(object parameter)
        {
            if (IsAccountSelected != null)
            {
                // Обновляем свойства аккаунта
                IsAccountSelected.Balance = Balance;
                IsAccountSelected.AccountName = AccountName;

                // Сохраняем изменения в базе данных
                context.SaveChanges();

                // Обновить коллекцию после изменений в базе данных
                LoadUserAccounts();
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
            context.SaveChanges();

            // Добавляем новый аккаунт в коллекцию
            UserAccounts.Add(newAccount);

            // Очищаем поля ввода
            AccountName = string.Empty;
            Balance = 0;
        }

        private bool CanUpdateSelectedAccount(object parameter)
        {
            return IsAccountSelected != null;
        }

        private int GetNextAccountId()
        {
            // Находим максимальное значение AccountID в коллекции
            int maxAccountId = UserAccounts.Max(account => account?.AccountID ?? 0);

            // Возвращаем следующее значение, увеличенное на 1
            return maxAccountId + 1;
        }

        private void ValidateInput()
        {
            // Паттерн для десятичного числа с не более чем 18 знаками перед точкой и не более чем 2 знаками после точки
            string pattern = @"^\d{0,18}([.,]\d{0,2})?$";
            if (!Regex.IsMatch(_balance.ToString(), pattern))
            {
                // Сбросить значение или выполнить другие действия при неверном вводе
                _balance = IsAccountSelected?.Balance ?? 0;
                MessageBox.Show("Некорректный ввод строки с десятичным значением. Вы можете ввести 18 знаков до запятой и 2 знака после.", "Ошибка");
                // Или можно использовать MessageBox.Show("Некорректный ввод");
            }
        }
    }
}