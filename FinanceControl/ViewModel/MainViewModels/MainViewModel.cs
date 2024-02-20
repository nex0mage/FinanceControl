using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using FinanceControl.Model;
using FinanceControl.View.MainViews.PageViews;
using FinanceControl.ViewModel.MainViewModels.PageViewModel;


    namespace FinanceControl.ViewModel.MainViewModels
    {
        internal class MainViewModel : ViewModelBase
        {
            private FinanceControl_DBEntities context = new FinanceControl_DBEntities();
            private EventAggregator _eventAggregator;
            private NavigationManager _navigationManager;

            private Page Welcome;
            private Page Accounts;
            private Page AccountTransfers;
            private Page IncomeTransactions;
            private Page IncomeCategories;
            private Page ExpenseTransactions;
            private Page RegularExpenses;
            private Page ExpenseCategories;
            private Page Debts;
            private Page DebtsTransactions;
            private Page Goals;
            private Page GoalsTransactions;
            private Page Reminders;
            private Page Charts;

            private Page _currentPage;
            public Page CurrentPage
            {
                get { return _currentPage; }
                set
                {
                    if (_currentPage != value)
                    {
                        _currentPage = value;
                        OnPropertyChanged(nameof(CurrentPage));

                    }
                }
            }

            public ICommand CurrentPageAccounts { get; }
            public ICommand CurrentPageAccountTransfers {  get; }
            public ICommand CurrentPageIncomeTransactions { get; }
            public ICommand CurrentPageIncomeCategories { get; }
            public ICommand CurrentPageExpenseTransactions { get; }
            public ICommand CurrentPageRegularExpenses { get; }
            public ICommand CurrentPageExpenseCategories {  get; }
            public ICommand CurrentPageDebts {  get; }
            public ICommand CurrentPageDebtsTransaction { get; }
            public ICommand CurrentPageGoals { get; }
            public ICommand CurrentPageGoalsTransactions { get; }
            public ICommand CurrentPageReminders { get; }
            public ICommand CurrentPageCharts { get; }



            public MainViewModel(int loggedInUserId, EventAggregator eventAggregator, NavigationManager navigationManager)
            {
                Welcome = new WelcomePage();
                Accounts = new AccountsPage();
                Accounts.DataContext = new AccountsPageVM(loggedInUserId, context);
                AccountTransfers = new AccountTransfersPage();
                AccountTransfers.DataContext = new AccountTransfersPageVM(loggedInUserId, context);
                IncomeTransactions = new IncomeTransactionsPage();
                IncomeCategories = new IncomeCategoriesPage();
                ExpenseTransactions = new ExpenseTransactionsPage();
                RegularExpenses = new RegularExpensesPage();
                ExpenseCategories = new ExpenseCategoriesPage();
                Debts = new DebtsPage();
                DebtsTransactions = new DebtsTransactionsPage();
                Goals = new GoalsPage();
                GoalsTransactions = new GoalsTransactionsPage();
                Reminders = new RemindersPage();
                Charts = new ChartsPage();

                CurrentPage = Welcome;

                CurrentPageAccounts = new ViewModelCommand(SetPageAccounts);
                CurrentPageAccountTransfers = new ViewModelCommand(SetPageAccountTransfers);
                CurrentPageIncomeTransactions = new ViewModelCommand(SetPageIncomeTransactions);
                CurrentPageIncomeCategories = new ViewModelCommand(SetPageIncomeCategories);
                CurrentPageExpenseTransactions = new ViewModelCommand(SetPageExpenseTransactions);
                CurrentPageRegularExpenses = new ViewModelCommand(SetPageRegularExpenses);
                CurrentPageExpenseCategories = new ViewModelCommand(SetPageExpenseCategories);
                CurrentPageDebts = new ViewModelCommand(SetPageDebts);
                CurrentPageDebtsTransaction = new ViewModelCommand(SetPageDebtsTransactions);
                CurrentPageGoals = new ViewModelCommand(SetPageGoals);
                CurrentPageGoalsTransactions = new ViewModelCommand(SetPageGoalsTransactions);
                CurrentPageReminders = new ViewModelCommand(SetPageReminders);
                CurrentPageCharts = new ViewModelCommand(SetPageCharts);


                _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
                _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

                // Остальной код инициализации...
            }
            private void SetPageAccounts(object parameter)
            {
                CurrentPage = Accounts;
            }
            private void SetPageAccountTransfers(object parameter) 
            {
                CurrentPage = AccountTransfers;
            }
            private void SetPageIncomeTransactions(object parameter)
            {
                CurrentPage = IncomeTransactions;
            }
            private void SetPageIncomeCategories(object parameter)
            {
                CurrentPage = IncomeCategories;
            }
            private void SetPageExpenseTransactions(object parameter)
            {
                CurrentPage = ExpenseTransactions;
            }
            private void SetPageRegularExpenses(object parameter)
            {
                CurrentPage = RegularExpenses;
            }
            private void SetPageExpenseCategories(object parameter)
            {
                CurrentPage = ExpenseCategories;
            }
            private void SetPageDebts(object parameter)
            {
                CurrentPage = Debts;
            }
            private void SetPageDebtsTransactions(object parameter)
            {
                CurrentPage = DebtsTransactions;
            }
            private void SetPageGoals(object parameter)
            {
                CurrentPage = Goals;
            }
            private void SetPageGoalsTransactions(object parameter)
            {
                CurrentPage = GoalsTransactions;
            }
            private void SetPageReminders(object parameter)
            {
                CurrentPage = Reminders;
            }
            private void SetPageCharts(object parameter)
            {
                CurrentPage = Charts;
            }

        }
    }

