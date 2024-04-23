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
    internal class DebtsPageVM : ViewModelBase
    {
        private int _loggedInUserId;
        private FinanceControl_DBEntities context;
        private Debts _selectedDebt;

        private string _comment;
        private decimal _amount;
        private string _toWho;
        private bool _status;

        public ObservableCollection<Debts> UserDebts { get; set; }

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

        public string ToWho
        {
            get => _toWho;
            set
            {
                if (_toWho != value)
                {
                    _toWho = value;
                    OnPropertyChanged(nameof(ToWho));
                }
            }
        }

        public bool Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public Debts IsDebtSelected
        {
            get { return _selectedDebt; }
            set
            {
                if (_selectedDebt != value)
                {
                    _selectedDebt = value;
                    OnPropertyChanged(nameof(IsDebtSelected));
                    LoadSelectedDebtDetails();

                }
            }

        }

        public ICommand DeleteSelectedTransactionCommand { get; }
        public ICommand UpdateSelectedTransactionCommand { get; }
        public ICommand AddNewTransactionCommand { get; }


        public DebtsPageVM(int loggedInUserId, FinanceControl_DBEntities transContext)
        {
            context = transContext;
            _loggedInUserId = loggedInUserId;
            UserDebts = new ObservableCollection<Debts>();
            LoadUserDebts();
            DeleteSelectedTransactionCommand = new ViewModelCommand(DeleteSelectedTransaction, CanDeleteSelectedTransaction);
            UpdateSelectedTransactionCommand = new ViewModelCommand(UpdateSelectedTransaction, CanUpdateSelectedTransactions);
            AddNewTransactionCommand = new ViewModelCommand(AddNewTransaction, CanAddNewTransaction);

        }

        private void DeleteSelectedTransaction(object parameter)
        {
            if (IsDebtSelected != null)
            {
                // Удаляем из базы данных
                context.Debts.Remove(IsDebtSelected);
                // Удаляем из коллекции
                var transactionsToRemove = context.DebtsTransactions.Where(debttx => debttx.DebtID == IsDebtSelected.DebtID).ToList();
                if (transactionsToRemove.Any())
                {
                    context.DebtsTransactions.RemoveRange(transactionsToRemove);
                    context.SaveChanges();
                }
                UserDebts.Remove(IsDebtSelected);
                EndOperation();
            }
        }
        private bool CanDeleteSelectedTransaction(object parameter)
        {
            return IsDebtSelected != null;
        }
        private void UpdateSelectedTransaction(object parameter)
        {
            if (IsDebtSelected != null)
            {
                // Обновляем в базе данных
                IsDebtSelected.Comment = Comment;
                IsDebtSelected.ToWho = ToWho;
                IsDebtSelected.Amount = Amount;
                if (IsDebtSelected.Amount <= 0)
                {
                    IsDebtSelected.DebtStatus = true;
                }
                EndOperation();
                LoadUserDebts();
            }
        }


        private bool CanUpdateSelectedTransactions(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || Amount == 0.0m || string.IsNullOrWhiteSpace(ToWho))
            {
                return false;
            }
            else
            {
                return IsDebtSelected != null;

            }
        }

        private void AddNewTransaction(object parameter)
        {
            Debts newDebt = new Debts
            {
                DebtID = GetNextDebtId(),
                Comment = Comment,
                DebtStatus = false,
                UserID = _loggedInUserId,
                Amount = Amount,
                ToWho = ToWho
            };
            context.Debts.Add(newDebt);
            // Добавляем в коллекцию
            UserDebts.Add(newDebt);
            EndOperation();
        }

        private bool CanAddNewTransaction(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || Amount == 0.0m || string.IsNullOrWhiteSpace(ToWho))
            {
                return false;
            }
            else
            {
                return true;
            }


        }

        private void LoadSelectedDebtDetails()
        {
            if (IsDebtSelected != null && context.Entry(IsDebtSelected).State != EntityState.Detached)
            {
                Amount = IsDebtSelected.Amount;
                Comment = IsDebtSelected.Comment; // Здесь Balance должен быть типа decimal
                ToWho = IsDebtSelected.ToWho;
                Status = IsDebtSelected.DebtStatus;
            }
        }

        private void LoadUserDebts()
        {
            UserDebts.Clear(); // Очищаем коллекцию перед загрузкой

            var debts = context.Debts.Where(debt => debt.Users.UserID == _loggedInUserId).ToList();

            foreach (var debt in debts)
            {
                UserDebts.Add(debt);
            }

            OnPropertyChanged(nameof(UserDebts)); // Уведомляем об изменении свойства для обновления привязки в UI
        }

        private int GetNextDebtId()
        {
            int maxDebtId = context.Debts.Any() ? context.Debts.Max(Debt => Debt.DebtID) : 0;

            // Возвращаем следующее значение, увеличенное на 1
            return maxDebtId + 1;
        }

        private void EndOperation()
        {
            context.SaveChanges();
            IsDebtSelected = null;
            Amount = 0.00m;
            Status = false;
            ToWho = null;
            Comment = null;
        }

        private bool IsValidInput(string input)
        {
            string pattern = @"^\d{1,18}(\.\d{2})?$";
            return Regex.IsMatch(input, pattern);
        }





    }
}
