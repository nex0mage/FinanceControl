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
    internal class DebtsTransactionsPageVM : ViewModelBase
    {
        private int _loggedInUserId;
        private FinanceControl_DBEntities context;
        private DebtsTransactions _selectedDebtsTransaction;

        private Accounts _accountFrom;
        private Debts _debtTo;
        private DateTime _DebtsTransactionDate = new DateTime(2023, 01, 01);
        private decimal _amount;

        public ObservableCollection<DebtsTransactions> UserDebtsTransactions { get; set; }

        private ObservableCollection<Accounts> _accounts;
        private ObservableCollection<Debts> _debts;


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


        public ObservableCollection<Debts> Debts
        {
            get { return _debts; }
            set
            {
                if (_debts != value)
                {
                    _debts = value;
                    OnPropertyChanged(nameof(Debts));
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

        public Debts DebtTo
        {
            get { return _debtTo; }
            set
            {
                if (_debtTo != value)
                {
                    _debtTo = value;
                    OnPropertyChanged(nameof(DebtTo));
                }
            }
        }


        private void LoadAccountsFromContext()
        {
            var accountsFrom = context.Accounts.Where(accountFrom => accountFrom.Users.UserID == _loggedInUserId).ToList();

            var debtTo = context.Debts.Where(accountTo => accountTo.Users.UserID == _loggedInUserId && accountTo.DebtStatus == false).ToList();



            // Создаем новые ObservableCollection и добавляем элементы из списков
            Accounts = new ObservableCollection<Accounts>(accountsFrom);
            Debts = new ObservableCollection<Debts>(debtTo);

            OnPropertyChanged(nameof(Accounts));
            OnPropertyChanged(nameof(Debts));
        }


        public DateTime DebtsTransactionDate
        {
            get => _DebtsTransactionDate;
            set
            {
                if (_DebtsTransactionDate != value)
                {
                    _DebtsTransactionDate = value;
                    OnPropertyChanged(nameof(DebtsTransactionDate));
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

        public DebtsTransactions IsDebtsTransactionSelected
        {
            get { return _selectedDebtsTransaction; }
            set
            {
                if (_selectedDebtsTransaction != value)
                {
                    _selectedDebtsTransaction = value;
                    OnPropertyChanged(nameof(_debts));
                    LoadSelectedDebtsTransactionDetails(); // Загрузка деталей выбранного аккаунта
                    LoadAccountsFromContext();
                }
            }
        }

        public ICommand DeleteSelectedDebtsTransactionCommand { get; }
        public ICommand UpdateSelectedDebtsTransactionCommand { get; }
        public ICommand AddNewDebtsTransactionCommand { get; }

        public DebtsTransactionsPageVM(int loggedInUserId, FinanceControl_DBEntities transContext)
        {
            context = transContext;
            _loggedInUserId = loggedInUserId;
            UserDebtsTransactions = new ObservableCollection<DebtsTransactions>();
            LoadAccountsFromContext();
            LoadUserDebtsTransactions();
            DeleteSelectedDebtsTransactionCommand = new ViewModelCommand(DeleteSelectedDebtsTransaction, CanDeleteSelectedDebtsTransaction);
            UpdateSelectedDebtsTransactionCommand = new ViewModelCommand(UpdateSelectedDebtsTransaction, CanUpdateSelectedDebtsTransaction);
            AddNewDebtsTransactionCommand = new ViewModelCommand(AddNewDebtsTransaction, CanAddNewDebtsTransaction);



        }


        private void LoadSelectedDebtsTransactionDetails()
        {
            if (IsDebtsTransactionSelected != null && context.Entry(IsDebtsTransactionSelected).State != EntityState.Detached)
            {
                Amount = IsDebtsTransactionSelected.Amount;
                DebtsTransactionDate = IsDebtsTransactionSelected.TransactionDate;
                DebtTo = IsDebtsTransactionSelected.Debts; // Предположим, что Accounts1 относится к AccountTo
                AccountFrom = IsDebtsTransactionSelected.Accounts; // Предположим, что Accounts относится к AccountFrom

            }
        }

        private void DeleteSelectedDebtsTransaction(object parameter)
        {
            if (IsDebtsTransactionSelected != null)
            {
                int? oldAccountFromID = IsDebtsTransactionSelected.AccountID;
                int? oldAccountToID = IsDebtsTransactionSelected.DebtTransactionID;
                var oldAccountFrom = context.Accounts.Find(oldAccountFromID);
                oldAccountFrom.Balance += IsDebtsTransactionSelected.Amount;
                var oldAccountTo = context.Debts.Find(oldAccountToID);
                oldAccountTo.Amount += IsDebtsTransactionSelected.Amount;

                // Удаляем из базы данных
                context.DebtsTransactions.Remove(IsDebtsTransactionSelected);
                // Удаляем из коллекции
                UserDebtsTransactions.Remove(IsDebtsTransactionSelected);
                EndOperation();

            }
        }

        private bool CanDeleteSelectedDebtsTransaction(object parameter)
        {
            return IsDebtsTransactionSelected != null;
        }

        private void UpdateSelectedDebtsTransaction(object parameter)
        {
            if (IsDebtsTransactionSelected != null)
            {
                // Обновляем в базе данных
                IsDebtsTransactionSelected.TransactionDate = DebtsTransactionDate;
                IsDebtsTransactionSelected.AccountID = AccountFrom.AccountID;
                IsDebtsTransactionSelected.DebtID = DebtTo.DebtID;

                int? oldAccountFromID = IsDebtsTransactionSelected.AccountID;
                int? oldAccountToID = IsDebtsTransactionSelected.DebtID;

                UpdateAccountBalances(oldAccountFromID, AccountFrom.AccountID, oldAccountToID, DebtTo.DebtID);
            }
        }

        private void UpdateAccountBalances(int? oldAccountFromID, int newAccountFromID, int? oldAccountToID, int newAccountToID)
        {
            if (oldAccountFromID != newAccountFromID)
            {
                // Уменьшаем баланс у старого кошелька отправителя
                var oldAccountFrom = context.Accounts.Find(oldAccountFromID);
                oldAccountFrom.Balance += IsDebtsTransactionSelected.Amount;
            }

            if (oldAccountToID != newAccountToID)
            {
                // Поднимаем баланс у старого кошелька получателя
                var oldAccountTo = context.Debts.Find(oldAccountToID);
                oldAccountTo.Amount -= IsDebtsTransactionSelected.Amount;
            }

            UpdateFinal();
        }

        private void UpdateFinal()
        {
            decimal difference = Amount - IsDebtsTransactionSelected.Amount;

            if (difference > 0 && (DebtTo.Amount - difference) < 0)
            {
                MessageBox.Show("Долг меньше введеной вами суммы. Измените ее и повторите попытку", "Операция прервана");
                return;
            }

            if (difference > 0 && (AccountFrom.Balance - difference) < 0)
            {
                MessageBox.Show("Кошелек отправителя приобретает значение меньше нуля. Измените значение и повторите попытку.", "Операция прервана");
                return;
            }

            AccountFrom.Balance -= difference;
            DebtTo.Amount -= difference;

            if(DebtTo.Amount == 0)
            {
                DebtTo.DebtStatus = true;
            }
            IsDebtsTransactionSelected.Amount = Amount;
            IsDebtsTransactionSelected.Accounts = AccountFrom;
            IsDebtsTransactionSelected.Debts = DebtTo;

            // Обновить коллекцию после изменений в базе данных
            EndOperation();
            LoadUserDebtsTransactions();

        }

        private bool CanUpdateSelectedDebtsTransaction(object parameter)
        {
            if (Amount == 0.0m || AccountFrom == null || DebtTo == null)
            {
                return false;
            }
            else
            {
                return IsDebtsTransactionSelected != null;

            }

        }

        private void AddNewDebtsTransaction(object parameter)
        {
            if ((AccountFrom.Balance - Amount) >= 0)
            {
                DebtsTransactions newDebtsTransaction = new DebtsTransactions
                {
                    DebtTransactionID = GetNextDebtsTransactionId(),
                    TransactionDate = DebtsTransactionDate,
                    AccountID = AccountFrom.AccountID,
                    DebtID = DebtTo.DebtID,
                    Accounts = AccountFrom,
                    Debts = DebtTo,
                    Amount = Amount
                };
                AccountFrom.Balance = AccountFrom.Balance - Amount;
                DebtTo.Amount = DebtTo.Amount - Amount;
                context.DebtsTransactions.Add(newDebtsTransaction);

                // Добавляем в коллекцию
                UserDebtsTransactions.Add(newDebtsTransaction);
                EndOperation();
            }
            else
            {
                MessageBox.Show("Перевод не может быть осуществлен, так как на счету отправителя нет такого количества средств", "Ошибка");
            }
            // Создаем новую транзакцию

            // Добавляем в базу данных
        }

        private bool CanAddNewDebtsTransaction(object parameter)
        {
            if (Amount == 0.0m || AccountFrom == null || DebtTo == null)
            {
                return false;
            }
            else
            {
                return true;
            }


        }

        private void LoadUserDebtsTransactions()
        {
            UserDebtsTransactions.Clear(); // Очищаем коллекцию перед загрузкой

            var DebtsTransactions = context.DebtsTransactions.Where(DebtsTransaction => DebtsTransaction.Accounts.Users.UserID == _loggedInUserId).ToList();

            foreach (var DebtsTransaction in DebtsTransactions)
            {
                UserDebtsTransactions.Add(DebtsTransaction);
            }

            OnPropertyChanged(nameof(UserDebtsTransactions)); // Уведомляем об изменении свойства для обновления привязки в UI
        }


        private int GetNextDebtsTransactionId()
        {
            // Находим максимальное значение AccountID в коллекции
            int maxDebtsTransactionId = context.DebtsTransactions.Any() ? context.DebtsTransactions.Max(debttx => debttx.DebtTransactionID) : 0;


            // Возвращаем следующее значение, увеличенное на 1
            return maxDebtsTransactionId + 1;
        }

        private void EndOperation()
        {
            context.SaveChanges();
            IsDebtsTransactionSelected = null;
            Amount = 0.00m;
            AccountFrom = null;
            DebtTo = null;
            DebtsTransactionDate = new DateTime(2023, 01, 01);
        }

        private bool IsValidInput(string input)
        {
            string pattern = @"^\d{1,18}(\.\d{2})?$";
            return Regex.IsMatch(input, pattern);
        }


    }
}
