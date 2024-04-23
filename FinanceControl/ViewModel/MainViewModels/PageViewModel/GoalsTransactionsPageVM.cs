using FinanceControl.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FinanceControl.ViewModel.MainViewModels.PageViewModel
{
    internal class GoalsTransactionsPageVM : ViewModelBase
    {
        private int _loggedInUserId;
        private FinanceControl_DBEntities context;
        private GoalsTransactions _selectedGoalsTransaction;

        private Accounts _accountFrom;
        private Goals _goalTo;
        private DateTime _GoalsTransactionDate = new DateTime(2024, 01, 01);
        private decimal _amount;

        public ObservableCollection<GoalsTransactions> UserGoalsTransactions { get; set; }

        private ObservableCollection<Accounts> _accounts;
        private ObservableCollection<Goals> _Goals;


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


        public ObservableCollection<Goals> Goals
        {
            get { return _Goals; }
            set
            {
                if (_Goals != value)
                {
                    _Goals = value;
                    OnPropertyChanged(nameof(Goals));
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

        public Goals GoalTo
        {
            get { return _goalTo; }
            set
            {
                if (_goalTo != value)
                {
                    _goalTo = value;
                    OnPropertyChanged(nameof(GoalTo));
                }
            }
        }


        private void LoadAccountsFromContext()
        {
            var accountsFrom = context.Accounts.Where(accountFrom => accountFrom.Users.UserID == _loggedInUserId).ToList();

            var goalTo = context.Goals.Where(accountTo => accountTo.Users.UserID == _loggedInUserId && accountTo.GoalStatus == false).ToList();



            // Создаем новые ObservableCollection и добавляем элементы из списков
            Accounts = new ObservableCollection<Accounts>(accountsFrom);
            Goals = new ObservableCollection<Goals>(goalTo);

            OnPropertyChanged(nameof(Accounts));
            OnPropertyChanged(nameof(Goals));
        }


        public DateTime GoalsTransactionDate
        {
            get => _GoalsTransactionDate;
            set
            {
                if (_GoalsTransactionDate != value)
                {
                    _GoalsTransactionDate = value;
                    OnPropertyChanged(nameof(GoalsTransactionDate));
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
                    _amount = value;
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }


        public GoalsTransactions IsGoalsTransactionSelected
        {
            get { return _selectedGoalsTransaction; }
            set
            {
                if (_selectedGoalsTransaction != value)
                {
                    _selectedGoalsTransaction = value;
                    OnPropertyChanged(nameof(_Goals));
                    LoadSelectedGoalsTransactionDetails(); // Загрузка деталей выбранного аккаунта
                    LoadAccountsFromContext();
                }
            }
        }

        public ICommand DeleteSelectedGoalsTransactionCommand { get; }
        public ICommand UpdateSelectedGoalsTransactionCommand { get; }
        public ICommand AddNewGoalsTransactionCommand { get; }

        public GoalsTransactionsPageVM(int loggedInUserId, FinanceControl_DBEntities transContext)
        {
            context = transContext;
            _loggedInUserId = loggedInUserId;
            UserGoalsTransactions = new ObservableCollection<GoalsTransactions>();
            LoadAccountsFromContext();
            LoadUserGoalsTransactions();
            DeleteSelectedGoalsTransactionCommand = new ViewModelCommand(DeleteSelectedGoalsTransaction, CanDeleteSelectedGoalsTransaction);
            UpdateSelectedGoalsTransactionCommand = new ViewModelCommand(UpdateSelectedGoalsTransaction, CanUpdateSelectedGoalsTransaction);
            AddNewGoalsTransactionCommand = new ViewModelCommand(AddNewGoalsTransaction, CanAddNewGoalsTransaction);



        }


        private void LoadSelectedGoalsTransactionDetails()
        {
            if (IsGoalsTransactionSelected != null && context.Entry(IsGoalsTransactionSelected).State != EntityState.Detached)
            {
                Amount = IsGoalsTransactionSelected.Amount;
                GoalsTransactionDate = IsGoalsTransactionSelected.TransactionDate;
                GoalTo = IsGoalsTransactionSelected.Goals; // Предположим, что Accounts1 относится к AccountTo
                AccountFrom = IsGoalsTransactionSelected.Accounts; // Предположим, что Accounts относится к AccountFrom

            }
        }

        private void DeleteSelectedGoalsTransaction(object parameter)
        {
            if (IsGoalsTransactionSelected != null)
            {
                int? oldAccountFromID = IsGoalsTransactionSelected.AccountID;
                int? oldAccountToID = IsGoalsTransactionSelected.GoalTransactionID;
                var oldAccountFrom = context.Accounts.Find(oldAccountFromID);
                oldAccountFrom.Balance += IsGoalsTransactionSelected.Amount;
                var oldAccountTo = context.Goals.Find(oldAccountToID);
                oldAccountTo.Ammount += IsGoalsTransactionSelected.Amount;
                if (oldAccountTo.GoalStatus == true && oldAccountTo.Ammount > 0)
                {
                    oldAccountTo.GoalStatus = false;
                }
                // Удаление из базы данных
                context.GoalsTransactions.Remove(IsGoalsTransactionSelected);
                // Удаление из коллекции
                UserGoalsTransactions.Remove(IsGoalsTransactionSelected);
                EndOperation();
            }
        }

        private bool CanDeleteSelectedGoalsTransaction(object parameter)
        {
            return IsGoalsTransactionSelected != null;
        }

        private void UpdateSelectedGoalsTransaction(object parameter)
        {
            if (IsGoalsTransactionSelected != null)
            {
                IsGoalsTransactionSelected.TransactionDate = GoalsTransactionDate;
                IsGoalsTransactionSelected.AccountID = AccountFrom.AccountID;
                IsGoalsTransactionSelected.GoalID = GoalTo.GoalID;
                int? oldAccountFromID = IsGoalsTransactionSelected.AccountID;
                int? oldAccountToID = IsGoalsTransactionSelected.GoalID;
                UpdateAccountBalances(oldAccountFromID, AccountFrom.AccountID, oldAccountToID, GoalTo.GoalID);
            }
        }

        private void UpdateAccountBalances(int? oldAccountFromID, int newAccountFromID, int? oldAccountToID, int newAccountToID)
        {
            if (oldAccountFromID != newAccountFromID)
            {
                var oldAccountFrom = context.Accounts.Find(oldAccountFromID);
                oldAccountFrom.Balance += IsGoalsTransactionSelected.Amount;
            }
            if (oldAccountToID != newAccountToID)
            {
                var oldAccountTo = context.Accounts.Find(oldAccountToID);
                oldAccountTo.Balance -= IsGoalsTransactionSelected.Amount;
            }
            UpdateFinal();
        }

        private void UpdateFinal()
        {
            decimal difference = Amount - IsGoalsTransactionSelected.Amount;
            if (difference > 0 && (GoalTo.Ammount - difference) < 0)
            {
                MessageBox.Show("Кошелек получателя приобретает значение меньше нуля. Измените значение и повторите попытку.", "Операция прервана");
                return;
            }
            if (difference > 0 && (AccountFrom.Balance - difference) < 0)
            {
                MessageBox.Show("Кошелек отправителя приобретает значение меньше нуля. Измените значение и повторите попытку.", "Операция прервана");
                return;
            }
            AccountFrom.Balance -= difference;
            GoalTo.Ammount -= difference;
            if (GoalTo.Ammount == 0)
            {
                GoalTo.GoalStatus = true;
                IsGoalsTransactionSelected.Amount = Amount;
                IsGoalsTransactionSelected.Accounts = AccountFrom;
                IsGoalsTransactionSelected.Goals = GoalTo;
            }
            else if (GoalTo.Ammount > 0)
            {
                IsGoalsTransactionSelected.Amount = Amount;
                IsGoalsTransactionSelected.Accounts = AccountFrom;
                IsGoalsTransactionSelected.Goals = GoalTo;
            }
            EndOperation();
            LoadUserGoalsTransactions();
        }

        private bool CanUpdateSelectedGoalsTransaction(object parameter)
        {
            if (Amount == 0.0m || AccountFrom == null || GoalTo == null)
            {
                return false;
            }
            else
            {
                return IsGoalsTransactionSelected != null;
            }
        }

        private void AddNewGoalsTransaction(object parameter)
        {
            if ((AccountFrom.Balance - Amount) >= 0)
            {
                GoalsTransactions newGoalsTransaction = new GoalsTransactions
                {
                    GoalTransactionID = GetNextGoalsTransactionId(),
                    TransactionDate = GoalsTransactionDate,
                    AccountID = AccountFrom.AccountID,
                    GoalID = GoalTo.GoalID,
                    Accounts = AccountFrom,
                    Goals = GoalTo,
                    Amount = Amount
                };
                if (GoalTo.Ammount - Amount > 0)
                {
                    AccountFrom.Balance = AccountFrom.Balance - Amount;
                    GoalTo.Ammount = GoalTo.Ammount - Amount;
                    context.GoalsTransactions.Add(newGoalsTransaction);
                    UserGoalsTransactions.Add(newGoalsTransaction);
                }
                else if (GoalTo.Ammount - Amount == 0)
                {
                    AccountFrom.Balance = AccountFrom.Balance - Amount;
                    GoalTo.Ammount = GoalTo.Ammount - Amount;
                    GoalTo.GoalStatus = true;
                    context.GoalsTransactions.Add(newGoalsTransaction);
                    UserGoalsTransactions.Add(newGoalsTransaction);
                }
                else
                {
                    MessageBox.Show("Перевод не может быть осуществлен, так как баланс цели станет отрицательным. Пожалуйста скорректируйте сумму операции.", "Ошибка");
                }
                EndOperation();
            }
            else
            {
                MessageBox.Show("Перевод не может быть осуществлен, так как на счету отправителя нет такого количества средств", "Ошибка");
            }
        }

        private bool CanAddNewGoalsTransaction(object parameter)
        {
            if (Amount == 0.0m || AccountFrom == null || GoalTo == null)
            {
                return false;
            }
            else
            {
                return true;
            }


        }

        private void LoadUserGoalsTransactions()
        {
            UserGoalsTransactions.Clear();
            var goalsTransactions = context.GoalsTransactions
                .Where(goalsTransaction => goalsTransaction.Accounts.Users.UserID == _loggedInUserId)
                .OrderByDescending(goalsTransaction => goalsTransaction.TransactionDate)
                .ToList();
            foreach (var goalsTransaction in goalsTransactions)
            {
                UserGoalsTransactions.Add(goalsTransaction);
            }
            OnPropertyChanged(nameof(UserGoalsTransactions)); 
        }

        private int GetNextGoalsTransactionId()
        {
            int maxGoalsTransactionId = context.GoalsTransactions.Max(GoalsTransactions => (int?)GoalsTransactions.GoalTransactionID) ?? 0;
            return maxGoalsTransactionId + 1;
        }

        private void EndOperation()
        {
            context.SaveChanges();
            IsGoalsTransactionSelected = null;
            Amount = 0.00m;
            AccountFrom = null;
            GoalTo = null;
            GoalsTransactionDate = new DateTime(2024, 01, 01);
            LoadUserGoalsTransactions();
        }
    }
}
