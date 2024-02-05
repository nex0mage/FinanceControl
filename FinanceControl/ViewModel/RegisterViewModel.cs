using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceControl.Model;
using System.Security;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Windows;
using System.Runtime.Remoting.Contexts;
using FinanceControl.View;
using System.Runtime.InteropServices;



namespace FinanceControl.ViewModel
{
    internal class RegisterViewModel : ViewModelBase
    {
        private string _email;
        private string _login;
        private SecureString _password;
        private string _errorMessage;
        private EventAggregator _eventAggregator;
        private FinanceControl_DB_Entities _dbContext;
        private NavigationManager _navigationManager;

        public string userEmail
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(userEmail));
                }
            }
        }

        public string userLogin
        {
            get { return _login; }
            set
            {
                if (_login != value)
                {
                    _login = value;
                    OnPropertyChanged(nameof(userLogin));
                }
            }
        }

        public SecureString userPassword
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(userPassword));
                }
            }
        }

        private string UnsecuredString_userPassword
        {
            get { return userPassword.ToUnsecuredString(); }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        public RegisterViewModel(EventAggregator eventAggregator, FinanceControl_DB_Entities dbContext, NavigationManager navigationManager)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
            RegisterCommand = new ViewModelCommand(Register, CanRegister);
            CancelCommand = new ViewModelCommand(Cancel);
        }

        public bool CanRegister(object parameter)
        {
            bool validData;
            if (string.IsNullOrWhiteSpace(_email) || string.IsNullOrWhiteSpace(_login)|| _login.Length < 3 || _password == null || _password.Length < 3 || _email.Length <3)
                validData = false;
            else
                validData = true;
            return validData;
        }

        private static bool IsValidEmail(string email)
        {
            // Регулярное выражение для проверки адреса электронной почты
            string emailPattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";
            // Используем Regex.IsMatch для проверки соответствия
            return Regex.IsMatch(email, emailPattern);
        }

        private void Register(object parameter)
        {
            _dbContext = new FinanceControl_DB_Entities();
            bool userExists = _dbContext.Users.Any(u => u.UserLogin == _login || u.Email == _email);
            if (IsValidEmail(_email) == true)
            {
                if (userExists)
                {
                    // Вывод MessageBox о том, что учетная запись уже существует
                    MessageBox.Show("Учетная запись с таким логином или почтой уже существует.");
                }
                else
                {
                    int maxUserId = _dbContext.Users.Max(u => (int?)u.UserID) ?? 0;
                    int newUserId = maxUserId + 1;
                    _dbContext.Users.Add(new Users 
                    {
                        UserID = newUserId,
                        UserLogin = _login,
                        UserPassword = UnsecuredString_userPassword,
                        Email = _email
                    });
                    _dbContext.SaveChanges();
                    MessageBox.Show("Регистрация выполнена успешно. Добро пожаловать!");
                    _eventAggregator.PublishUserLoggedIn(newUserId);
                    _navigationManager.NavigateToMainView(newUserId);
                    Cancel(1);
                }
            }
            else
            {
                MessageBox.Show("Такой электронной почты не может существоввать");
            }
        }

        private void Cancel(object parameter) 
        {
            Window currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this);
            if (currentWindow != null)
            {
                _navigationManager.StartAtLoginView();
                currentWindow.Close();
            }
        }
    }
}
