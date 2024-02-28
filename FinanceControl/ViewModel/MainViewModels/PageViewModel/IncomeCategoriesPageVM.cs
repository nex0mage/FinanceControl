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
    internal class IncomeCategoriesPageVM : ViewModelBase
    {
        private int _loggedInUserId;
        private FinanceControl_DBEntities context;
        private IncomeCategories _selectedIncomeCategory;

        private string _categoryName;

        public ObservableCollection<IncomeCategories> UserIncomeCategories { get; set; }

        public string CategoryName
        { get { return _categoryName; }
            set
            {
                if (_categoryName != value)
                {
                    _categoryName = value;
                    OnPropertyChanged(nameof(CategoryName));

                }
            }
        }


        public IncomeCategories IsIncomeCategorySelected
        {
            get { return _selectedIncomeCategory; }
            set
            {
                if (_selectedIncomeCategory != value)
                {
                    _selectedIncomeCategory = value;
                    OnPropertyChanged(nameof(IsIncomeCategorySelected));
                    LoadSelectedIncomeCategorynDetails();
                }
            }
        }

        public ICommand ClearSelection { get; }
        public ICommand DeleteSelectedCategoryCommand { get; }
        public ICommand UpdateSelectedCategoryCommand { get; }
        public ICommand AddNewCategoryCommand { get; }


        public IncomeCategoriesPageVM(int loggedInUserId, FinanceControl_DBEntities transContext)
        {
            context = transContext;
            _loggedInUserId = loggedInUserId;
            UserIncomeCategories = new ObservableCollection<IncomeCategories>();
            LoadUserIncomeCategories();
            DeleteSelectedCategoryCommand = new ViewModelCommand(DeleteSelectedCategory, CanDeleteSelectedCategory);
            UpdateSelectedCategoryCommand = new ViewModelCommand(UpdateSelectedCategory, CanUpdateSelectedCategory);
            AddNewCategoryCommand = new ViewModelCommand(AddNewCategory, CanAddNewCategory);
            ClearSelection = new ViewModelCommand(ClearSelected);
        }

        private void ClearSelected(object parameter)
        {
            IsIncomeCategorySelected = null;
        }

        private void DeleteSelectedCategory(object parameter)
        {
            if (IsIncomeCategorySelected != null)
            {

                // Удаляем из базы данных
                context.IncomeCategories.Remove(IsIncomeCategorySelected);
                // Удаляем из коллекции
                UserIncomeCategories.Remove(IsIncomeCategorySelected);
                EndOperation();

            }

        }

        private bool CanDeleteSelectedCategory(object parameter)
        {
            return IsIncomeCategorySelected != null;
        }

        private void UpdateSelectedCategory(object parameter)
        {
            if (IsIncomeCategorySelected != null)
            {
                // Обновляем в базе данных
                IsIncomeCategorySelected.CategoryName = CategoryName;
                LoadUserIncomeCategories();
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
                return IsIncomeCategorySelected != null;

            }
        }

        private void AddNewCategory(object parameter)
        {
            IncomeCategories newIncomeCategory = new IncomeCategories
            {
                IncomeCategoryID = GetNextCategoryId(),
                CategoryName = CategoryName,
                UserID = _loggedInUserId

            };
            context.IncomeCategories.Add(newIncomeCategory);
            // Добавляем в коллекцию
            UserIncomeCategories.Add(newIncomeCategory);
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


        private void LoadSelectedIncomeCategorynDetails()
        {
            if (IsIncomeCategorySelected != null && context.Entry(IsIncomeCategorySelected).State != EntityState.Detached)
            {
                CategoryName = IsIncomeCategorySelected.CategoryName;


            }
        }

        private void LoadUserIncomeCategories()
        {
            UserIncomeCategories.Clear(); // Очищаем коллекцию перед загрузкой

            var incomecategories = context.IncomeCategories.Where(incomecategory => incomecategory.Users.UserID == _loggedInUserId).ToList();

            foreach (var incomecategory in incomecategories)
            {
                UserIncomeCategories.Add(incomecategory);
            }

            OnPropertyChanged(nameof(UserIncomeCategories)); // Уведомляем об изменении свойства для обновления привязки в UI
        }
        private int GetNextCategoryId()
        {

            // Находим максимальное значение AccountID в коллекции
            int maxIncomeCategoryId = context.IncomeCategories.Any() ? context.IncomeCategories.Max(incomecategory => incomecategory.IncomeCategoryID) : 0;


            // Возвращаем следующее значение, увеличенное на 1
            return maxIncomeCategoryId + 1;
        }

        private void EndOperation()
        {
            context.SaveChanges();
            IsIncomeCategorySelected = null;
            CategoryName = null;
        }


    }
}
