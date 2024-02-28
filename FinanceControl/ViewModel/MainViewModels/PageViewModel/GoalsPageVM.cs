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
    internal class GoalsPageVM : ViewModelBase
    {
        private int _loggedInUserId;
        private FinanceControl_DBEntities context;
        private Goals _selectedGoal;

        private string _comment;
        private decimal _amount;
        private bool _status;

        public ObservableCollection<Goals> UserGoals { get; set; }

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

        public Goals IsGoalSelected
        {
            get { return _selectedGoal; }
            set
            {
                if (_selectedGoal != value)
                {
                    _selectedGoal = value;
                    OnPropertyChanged(nameof(IsGoalSelected));
                    LoadSelectedGoalDetails();

                }
            }

        }

        public ICommand DeleteSelectedTransactionCommand { get; }
        public ICommand UpdateSelectedTransactionCommand { get; }
        public ICommand AddNewTransactionCommand { get; }


        public GoalsPageVM(int loggedInUserId, FinanceControl_DBEntities transContext)
        {
            context = transContext;
            _loggedInUserId = loggedInUserId;
            UserGoals = new ObservableCollection<Goals>();
            LoadUserGoals();
            DeleteSelectedTransactionCommand = new ViewModelCommand(DeleteSelectedTransaction, CanDeleteSelectedTransaction);
            UpdateSelectedTransactionCommand = new ViewModelCommand(UpdateSelectedTransaction, CanUpdateSelectedTransactions);
            AddNewTransactionCommand = new ViewModelCommand(AddNewTransaction, CanAddNewTransaction);

        }

        private void DeleteSelectedTransaction(object parameter)
        {
            if (IsGoalSelected != null)
            {
                // Удаляем из базы данных
                context.Goals.Remove(IsGoalSelected);
                // Удаляем из коллекции
                var transactionsToRemove = context.GoalsTransactions.Where(Goaltx => Goaltx.GoalID == IsGoalSelected.GoalID).ToList();

                if (transactionsToRemove.Any())
                {
                    context.GoalsTransactions.RemoveRange(transactionsToRemove);
                    context.SaveChanges();
                }
                UserGoals.Remove(IsGoalSelected);
                EndOperation();
            }
        }

        private bool CanDeleteSelectedTransaction(object parameter)
        {
            return IsGoalSelected != null;
        }

        private void UpdateSelectedTransaction(object parameter)
        {
            if (IsGoalSelected != null)
            {
                // Обновляем в базе данных
                IsGoalSelected.Title = Comment;
                IsGoalSelected.Ammount = Amount;
                if (IsGoalSelected.Ammount <= 0)
                {
                    IsGoalSelected.GoalStatus = true;
                }
                EndOperation();
                LoadUserGoals();
            }

        }


        private bool CanUpdateSelectedTransactions(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || Amount == 0.0m )
            {
                return false;
            }
            else
            {
                return IsGoalSelected != null;

            }
        }

        private void AddNewTransaction(object parameter)
        {
            Goals newGoal = new Goals
            {
                GoalID = GetNextGoalId(),
                Title = Comment,
                GoalStatus = false,
                UserID = _loggedInUserId,
                Ammount = Amount,
            };
            context.Goals.Add(newGoal);
            // Добавляем в коллекцию
            UserGoals.Add(newGoal);

            EndOperation();

        }

        private bool CanAddNewTransaction(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || Amount == 0.0m)
            {
                return false;
            }
            else
            {
                return true;
            }


        }

        private void LoadSelectedGoalDetails()
        {
            if (IsGoalSelected != null && context.Entry(IsGoalSelected).State != EntityState.Detached)
            {
                Amount = IsGoalSelected.Ammount;
                Comment = IsGoalSelected.Title; // Здесь Balance должен быть типа decimal
                Status = IsGoalSelected.GoalStatus;
            }
        }

        private void LoadUserGoals()
        {
            UserGoals.Clear(); // Очищаем коллекцию перед загрузкой

            var Goals = context.Goals.Where(Goal => Goal.Users.UserID == _loggedInUserId).ToList();

            foreach (var Goal in Goals)
            {
                UserGoals.Add(Goal);
            }

            OnPropertyChanged(nameof(UserGoals)); // Уведомляем об изменении свойства для обновления привязки в UI
        }

        private int GetNextGoalId()
        {
            // Находим максимальное значение AccountID в коллекции
           int maxGoalId = context.Goals.Any() ? context.Goals.Max(Goal => Goal.GoalID) : 0;


            // Возвращаем следующее значение, увеличенное на 1
            return maxGoalId + 1;
        }

        private void EndOperation()
        {
            context.SaveChanges();
            IsGoalSelected = null;
            Amount = 0.00m;
            Status = false;
            Comment = null;
        }

        private bool IsValidInput(string input)
        {
            string pattern = @"^\d{1,18}(\.\d{2})?$";
            return Regex.IsMatch(input, pattern);
        }




    }
}
