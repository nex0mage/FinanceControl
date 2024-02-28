using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using FinanceControl.Model;
using FinanceControl.View.MainViews.PageViews;
using FinanceControl.ViewModel.MainViewModels.PageViewModel;
using System.Windows.Threading;


namespace FinanceControl.ViewModel.MainViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        private FinanceControl_DBEntities context = new FinanceControl_DBEntities();
        private EventAggregator _eventAggregator;
        private NavigationManager _navigationManager;
        
        private ObservableCollection<Reminders> _userReminders;



        public ObservableCollection<Reminders> UserReminders
        {
            get { return _userReminders; }
            set
            {
                if (_userReminders != value)
                {
                        
                    _userReminders = value;
                    OnPropertyChanged(nameof(UserReminders));
                }

            }
        }






        private int _loggedInUserId;
        private void LoadUserReminders()
        {
                
            UserReminders.Clear(); // Очищаем коллекцию перед загрузкой

            var Reminders = context.Reminders
                .Where(Reminder =>
                    Reminder.Users.UserID == _loggedInUserId &&
                    !Reminder.IsCompleted &&
                    Reminder.ReminderDate <= DateTime.Now)
                .ToList();
            foreach (var Reminder in Reminders)
            {
                UserReminders.Add(Reminder);
            }

            OnPropertyChanged(nameof(UserReminders)); // Уведомляем об изменении свойства для обновления привязки в UI
        }

        private void DeleteMarkedReminders()
        {
            var remindersToUpdate = UserReminders.Where(r => r.IsCompleted == true).ToList();
            if (remindersToUpdate.Count > 0)
            {
                foreach (var reminder in remindersToUpdate)
                {
                    var reminderId = reminder.ReminderID; // Запоминаем ID напоминания
                    var reminderInContext = context.Reminders.FirstOrDefault(r => r.ReminderID == reminderId);

                    if (reminderInContext != null)
                    {
                        // Изменим свойство в контексте
                        reminderInContext.IsCompleted = true;
                    }
                    context.SaveChanges();
                    remindersToUpdate.ForEach(r => UserReminders.Remove(r));
                    OnPropertyChanged(nameof(UserReminders));
                }
            }
        }

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
            _loggedInUserId = loggedInUserId;
            UserReminders = new ObservableCollection<Reminders>();
            LoadUserReminders();
            Welcome = new WelcomePage();
            Accounts = new AccountsPage();
            AccountTransfers = new AccountTransfersPage();
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
            context = new FinanceControl_DBEntities();
            Accounts.DataContext = new AccountsPageVM(_loggedInUserId, context);
            CurrentPage = Accounts;
            DeleteMarkedReminders();

        }
        private void SetPageAccountTransfers(object parameter) 
        {
            context = new FinanceControl_DBEntities();
            AccountTransfers.DataContext = new AccountTransfersPageVM(_loggedInUserId, context);
            CurrentPage = AccountTransfers;
            DeleteMarkedReminders();
        }
        private void SetPageIncomeTransactions(object parameter)
        {
            context = new FinanceControl_DBEntities();
            IncomeTransactions.DataContext = new IncomeTransactionsPageVM(_loggedInUserId, context);
            CurrentPage = IncomeTransactions;
            DeleteMarkedReminders();
        }
        private void SetPageIncomeCategories(object parameter)
        {
            context = new FinanceControl_DBEntities();
            IncomeCategories.DataContext = new IncomeCategoriesPageVM(_loggedInUserId, context);
            CurrentPage = IncomeCategories;
            DeleteMarkedReminders();
        }
        private void SetPageExpenseTransactions(object parameter)
        {
            context = new FinanceControl_DBEntities();
            ExpenseTransactions.DataContext = new ExpenseTransactionsPageVM(_loggedInUserId, context);
            CurrentPage = ExpenseTransactions;
            DeleteMarkedReminders();
        }
        private void SetPageRegularExpenses(object parameter)
        {
            context = new FinanceControl_DBEntities();
            RegularExpenses.DataContext = new RegularExpensesPageVM(_loggedInUserId, context);
            CurrentPage = RegularExpenses;
            DeleteMarkedReminders();
        }
        private void SetPageExpenseCategories(object parameter)
        {
            context = new FinanceControl_DBEntities();
            ExpenseCategories.DataContext = new ExpenseCategoriesPageVM(_loggedInUserId, context);
            CurrentPage = ExpenseCategories;
            DeleteMarkedReminders();
        }
        private void SetPageDebts(object parameter)
        {
            context = new FinanceControl_DBEntities();
            Debts.DataContext = new DebtsPageVM(_loggedInUserId, context);
            CurrentPage = Debts;
            DeleteMarkedReminders();
        }
        private void SetPageDebtsTransactions(object parameter)
        {
            context = new FinanceControl_DBEntities();
            DebtsTransactions.DataContext = new DebtsTransactionsPageVM(_loggedInUserId, context);
            CurrentPage = DebtsTransactions;
            DeleteMarkedReminders();
        }
        private void SetPageGoals(object parameter)
        {
            context = new FinanceControl_DBEntities();
            Goals.DataContext = new GoalsPageVM(_loggedInUserId, context);
            CurrentPage = Goals;
            DeleteMarkedReminders();
        }
        private void SetPageGoalsTransactions(object parameter)
        {
            context = new FinanceControl_DBEntities();
            GoalsTransactions.DataContext = new GoalsTransactionsPageVM(_loggedInUserId, context);
            CurrentPage = GoalsTransactions;
            DeleteMarkedReminders();
        }
        private void SetPageReminders(object parameter)
        {
            context = new FinanceControl_DBEntities();
            Reminders.DataContext = new RemindersPageVM(_loggedInUserId, context);
            CurrentPage = Reminders;
            DeleteMarkedReminders();
        }
        private void SetPageCharts(object parameter)
        {
            Charts.DataContext = new ChartsPageVM(_loggedInUserId);
            CurrentPage = Charts;
            DeleteMarkedReminders();
        }

    }
}

