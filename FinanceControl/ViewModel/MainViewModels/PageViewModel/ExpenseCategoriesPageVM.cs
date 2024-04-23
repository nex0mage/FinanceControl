using FinanceControl.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinanceControl.ViewModel.MainViewModels.PageViewModel
{
    internal class ExpenseCategoriesPageVM : ViewModelBase
    {
        private int _loggedInUserId;
        private FinanceControl_DBEntities context;
        private ExpensesCategories _selectedExpenseCategory;

        private string _categoryName;

        public ObservableCollection<ExpensesCategories> UserExpenseCategories { get; set; }

        public string CategoryName
        {
            get { return _categoryName; }
            set
            {
                if (_categoryName != value)
                {
                    _categoryName = value;
                    OnPropertyChanged(nameof(CategoryName));

                }
            }
        }


        public ExpensesCategories IsExpenseCategorySelected
        {
            get { return _selectedExpenseCategory; }
            set
            {
                if (_selectedExpenseCategory != value)
                {
                    _selectedExpenseCategory = value;
                    OnPropertyChanged(nameof(IsExpenseCategorySelected));
                    LoadSelectedExpenseCategorynDetails();
                }
            }
        }

        public ICommand DeleteSelectedCategoryCommand { get; }
        public ICommand UpdateSelectedCategoryCommand { get; }
        public ICommand AddNewCategoryCommand { get; }


        public ExpenseCategoriesPageVM(int loggedInUserId, FinanceControl_DBEntities transContext)
        {
            context = transContext;
            _loggedInUserId = loggedInUserId;
            UserExpenseCategories = new ObservableCollection<ExpensesCategories>();
            LoadUserExpenseCategories();
            DeleteSelectedCategoryCommand = new ViewModelCommand(DeleteSelectedCategory, CanDeleteSelectedCategory);
            UpdateSelectedCategoryCommand = new ViewModelCommand(UpdateSelectedCategory, CanUpdateSelectedCategory);
            AddNewCategoryCommand = new ViewModelCommand(AddNewCategory, CanAddNewCategory);

        }

        private void DeleteSelectedCategory(object parameter)
        {
            if (IsExpenseCategorySelected != null)
            {
                // Удаление из базы данных приближенных записей
                context.ExpensesCategories.Remove(IsExpenseCategorySelected);
                var transactionsToRemove = context.ExpensesTransactions.Where(tx => tx.ExpenseCategoryID == IsExpenseCategorySelected.ExpenseCategoryID).ToList();
                var regularTransactionsToRemove = context.RegularExpenses.Where(tx => tx.ExpenseCategoryID == IsExpenseCategorySelected.ExpenseCategoryID).ToList();
                if (transactionsToRemove.Any() || regularTransactionsToRemove.Any())
                {
                    context.ExpensesTransactions.RemoveRange(transactionsToRemove);
                    context.RegularExpenses.RemoveRange(regularTransactionsToRemove);
                    context.SaveChanges();
                }
                // Удаление из коллекции
                UserExpenseCategories.Remove(IsExpenseCategorySelected);
                EndOperation();
            }
        }

        private bool CanDeleteSelectedCategory(object parameter)
        {
            return IsExpenseCategorySelected != null;
        }

        private void UpdateSelectedCategory(object parameter)
        {
            if (IsExpenseCategorySelected != null)
            {
                // Обновление в базе данных
                IsExpenseCategorySelected.CategoryName = CategoryName;
                var transactionsToUpdate = context.ExpensesTransactions.Where(tx => tx.ExpenseCategoryID == IsExpenseCategorySelected.ExpenseCategoryID).ToList();
                var regularTransactionsToUpdate = context.RegularExpenses.Where(tx => tx.ExpenseCategoryID == IsExpenseCategorySelected.ExpenseCategoryID).ToList();
                foreach (var transaction in transactionsToUpdate)
                {
                    transaction.ExpensesCategories = IsExpenseCategorySelected;
                }
                foreach (var regularTransaction in regularTransactionsToUpdate)
                {
                    regularTransaction.ExpensesCategories = IsExpenseCategorySelected;
                }
                LoadUserExpenseCategories();
                EndOperation();
            }
        }

        private bool CanUpdateSelectedCategory(object parameter)
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                return false;
            }
            else
            {
                return IsExpenseCategorySelected != null;

            }
        }

        private void AddNewCategory(object parameter)
        {
            ExpensesCategories newExpenseCategory = new ExpensesCategories
            {
                ExpenseCategoryID = GetNextCategoryId(),
                CategoryName = CategoryName,
                UserID = _loggedInUserId
            };
            context.ExpensesCategories.Add(newExpenseCategory);
            // Добавление в коллекцию
            UserExpenseCategories.Add(newExpenseCategory);
            EndOperation();
        }

        private bool CanAddNewCategory(object parameter)
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                return false;
            }
            else
            {
                return true;

            }

        }

        private void LoadSelectedExpenseCategorynDetails()
        {
            if (IsExpenseCategorySelected != null && context.Entry(IsExpenseCategorySelected).State != EntityState.Detached)
            {
                CategoryName = IsExpenseCategorySelected.CategoryName;
            }
        }

        private void LoadUserExpenseCategories()
        {
            UserExpenseCategories.Clear(); // Очищаем коллекцию перед загрузкой

            var ExpenseCategories = context.ExpensesCategories.Where(Expensecategory => Expensecategory.Users.UserID == _loggedInUserId).ToList();

            foreach (var Expensecategory in ExpenseCategories)
            {
                UserExpenseCategories.Add(Expensecategory);
            }

            OnPropertyChanged(nameof(UserExpenseCategories)); // Уведомляем об изменении свойства для обновления привязки в UI
        }
        private int GetNextCategoryId()
        {

            // Находим максимальное значение AccountID в коллекции
            int maxExpenseCategoryId = context.ExpensesCategories.Any() ? context.ExpensesCategories.Max(expensecategory => expensecategory.ExpenseCategoryID) : 0;


            // Возвращаем следующее значение, увеличенное на 1
            return maxExpenseCategoryId + 1;
        }
        private void EndOperation()
        {
            context.SaveChanges();
            IsExpenseCategorySelected = null;
            CategoryName = null;
        }
    }
}
