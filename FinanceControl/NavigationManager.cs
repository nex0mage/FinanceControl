using FinanceControl.Model;
using FinanceControl.View.MainViews;
using FinanceControl.View;
using FinanceControl.ViewModel.MainViewModels;
using FinanceControl.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace FinanceControl
{
    public class NavigationManager
    {
        private EventAggregator _eventAggregator;
        private FinanceControl_DB_Entities _dbContext;

        public NavigationManager(EventAggregator eventAggregator, FinanceControl_DB_Entities dbContext)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public void StartAtLoginView()
        {
            LoginViewModel loginViewModel = new LoginViewModel(_eventAggregator, _dbContext, this);  // Передача ссылки на NavigationManager
            LoginView loginView = new LoginView();
            loginView.DataContext = loginViewModel;
            loginView.Show();
        }

        public void NavigateToMainView(int loggedInUserId)
        {
            MainViewModel mainViewModel = new MainViewModel(loggedInUserId, _eventAggregator, this); // Передача EventAggregator
            MainView mainView = new MainView();
            mainView.DataContext = mainViewModel;
            mainView.Show();
        }

        public void OpenRegisterView()
        {
            RegisterViewModel registerViewModel = new RegisterViewModel(_eventAggregator, _dbContext, this);
            RegisterView registerView = new RegisterView();
            registerView.DataContext = registerViewModel;
            registerView.Show();
        }
    }
}
