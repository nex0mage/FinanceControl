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
    internal class RegularExpensesPageVM : ViewModelBase
    {
        private int _loggedInUserId;
        private FinanceControl_DBEntities context;
        private RegularExpenses _selectedRegularExpense;

        private string _comment;
        private ExpensesCategories _expenseCategory;
        private string _freequency;
        private decimal _amount;


        public ObservableCollection<RegularExpenses> UserRegularExpenses { get; set; }

        private ObservableCollection<ExpensesCategories> _expenseCategories;


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

        private void LoadCategoriesFromContext()
        {
            var categories = context.ExpensesCategories.Where(category => category.Users.UserID == _loggedInUserId).ToList();


            // Создаем новые ObservableCollection и добавляем элементы из списков
            ExpenseCategories = new ObservableCollection<ExpensesCategories>(categories);

            OnPropertyChanged(nameof(ExpenseCategories));
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

        public string Freequency
        {
            get => _freequency;
            set
            {
                if (_freequency != value)
                {
                    _freequency = value;
                    OnPropertyChanged(nameof(Freequency));
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

        public RegularExpenses IsRegularExpenseSelected
        {
            get { return _selectedRegularExpense; }
            set
            {
                if (_selectedRegularExpense != value)
                {
                    _selectedRegularExpense = value;
                    OnPropertyChanged(nameof(IsRegularExpenseSelected));
                    LoadSelectedRegularExpenseDetails();

                }
            }

        }

        public ICommand DeleteSelectedRegularExpenseCommand { get; }
        public ICommand UpdateSelectedRegularExpenseCommand { get; }
        public ICommand AddNewRegularExpenseCommand { get; }


        public RegularExpensesPageVM(int loggedInUserId, FinanceControl_DBEntities transContext)
        {
            context = transContext;
            _loggedInUserId = loggedInUserId;
            UserRegularExpenses = new ObservableCollection<RegularExpenses>();
            LoadCategoriesFromContext();
            LoadUserExpenseTransactions();
            DeleteSelectedRegularExpenseCommand = new ViewModelCommand(DeleteSelectedTransaction, CanDeleteSelectedTransaction);
            UpdateSelectedRegularExpenseCommand = new ViewModelCommand(UpdateSelectedTransaction, CanUpdateSelectedTransactions);
            AddNewRegularExpenseCommand = new ViewModelCommand(AddNewTransaction, CanAddNewTransaction);

        }

        private void DeleteSelectedTransaction(object parameter)
        {
            if (IsRegularExpenseSelected != null)
            {

                // Удаляем из базы данных
                context.RegularExpenses.Remove(IsRegularExpenseSelected);

                // Удаляем из коллекции
                UserRegularExpenses.Remove(IsRegularExpenseSelected);

                EndOperation();
            }
        }

        private bool CanDeleteSelectedTransaction(object parameter)
        {
            return IsRegularExpenseSelected != null;
        }

        private void UpdateSelectedTransaction(object parameter)
        {
            if (IsRegularExpenseSelected != null)
            {
                // Обновляем в базе данных
                IsRegularExpenseSelected.Comment = Comment;
                IsRegularExpenseSelected.Frequency = Freequency;
                IsRegularExpenseSelected.ExpenseCategoryID = ExpenseCategory.ExpenseCategoryID;


            }
            EndOperation();

            LoadUserExpenseTransactions();


        }


        private bool CanUpdateSelectedTransactions(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || Amount == 0.0m || string.IsNullOrWhiteSpace(Freequency) || ExpenseCategory == null)
            {
                return false;
            }
            else
            {
                return IsRegularExpenseSelected != null;

            }
        }

        private void AddNewTransaction(object parameter)
        {
            RegularExpenses newRegularExpense = new RegularExpenses
            {
                RegularExpenseID = GetNextTransferId(),
                Comment = Comment,
                Frequency = Freequency,
                UserID = _loggedInUserId,
                ExpenseCategoryID = ExpenseCategory.ExpenseCategoryID,
                Amount = Amount
            };
            context.RegularExpenses.Add(newRegularExpense);
            // Добавляем в коллекцию
            UserRegularExpenses.Add(newRegularExpense);
            EndOperation();
        }

        private bool CanAddNewTransaction(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || Amount == 0.0m || string.IsNullOrWhiteSpace(Freequency) || ExpenseCategory == null)
            {
                return false;
            }
            else
            {
                return true;
            }


        }

        private void LoadSelectedRegularExpenseDetails()
        {
            if (IsRegularExpenseSelected != null && context.Entry(IsRegularExpenseSelected).State != EntityState.Detached)
            {
                Amount = IsRegularExpenseSelected.Amount;
                Comment = IsRegularExpenseSelected.Comment; // Здесь Balance должен быть типа decimal
                Freequency = IsRegularExpenseSelected.Frequency;
                ExpenseCategory = IsRegularExpenseSelected.ExpensesCategories;

            }
        }

        private void LoadUserExpenseTransactions()
        {
            UserRegularExpenses.Clear(); // Очищаем коллекцию перед загрузкой

            var regularexpenses = context.RegularExpenses.Where(regularexpense => regularexpense.Users.UserID == _loggedInUserId).ToList();

            foreach (var regularexpense in regularexpenses)
            {
                UserRegularExpenses.Add(regularexpense);
            }

            OnPropertyChanged(nameof(UserRegularExpenses)); // Уведомляем об изменении свойства для обновления привязки в UI
        }

        private int GetNextTransferId()
        {
            // Находим максимальное значение AccountID в коллекции
            int maxRegulareExpenseId = context.RegularExpenses.Any() ? context.RegularExpenses.Max(transaction => transaction.RegularExpenseID) : 0;


            // Возвращаем следующее значение, увеличенное на 1
            return maxRegulareExpenseId + 1;
        }

        private void EndOperation()
        {
            context.SaveChanges();
            IsRegularExpenseSelected = null;
            Comment = null;
            Amount = 0.00m;
            Freequency = null;
            ExpenseCategory = null;
        }

        private bool IsValidInput(string input)
        {
            string pattern = @"^\d{1,18}(\.\d{2})?$";
            return Regex.IsMatch(input, pattern);
        }



    }
}
