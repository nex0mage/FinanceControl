using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using FinanceControl.Model;
using System.Security;

namespace FinanceControl.ViewModel
{

    public class LoginViewModel : ViewModelBase
    {
        // Поля
        private string _login;
        private SecureString _password;
        private string _errorMessage;
        private bool _isViewVisible;
        private FinanceControl_DB_Entities _dbContext; // Здесь ваш контекст базы данных
        public event EventHandler<int> LoginSuccess; // Событие для передачи UserID в MainWindowViewModel

        //Свойства
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

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));

            }
        }

        public bool IsViewVisible
        {
            get { return _isViewVisible; }
            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        // Команды

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand RecoverCommand { get; }

        public LoginViewModel(FinanceControl_DB_Entities dbContext)
        {
            _dbContext = dbContext;

            // Команда для кнопки входа
            LoginCommand = new ViewModelCommand(Login, CanLogin);
        }

        // Проверка на пустые поля
        public bool CanLogin(object parameter)
        {
            bool validData;
            if(string.IsNullOrWhiteSpace(userLogin) || userLogin.Length < 3 || userPassword == null || userPassword.Length < 3)
                validData = false;
            else 
                validData = true;
            return validData;

            //    var user = _dbContext.Users.FirstOrDefault(u => u.UserLogin == userLogin && u.UserPassword == userPassword);

            //    if (user != null)
            //    {
            //        // Успешная аутентификация
            //        MessageBox.Show("Вход выполнен успешно!");

            //        // Вызываем событие и передаем UserID в MainWindowViewModel
            //        LoginSuccess?.Invoke(this, user.UserID);
            //    }
            //    else
            //    {
            //        // Неправильный логин или пароль, выводим MessageBox
            //        MessageBox.Show("Неверный логин или пароль");
            //    }

        }

        private void Login(object parameter)
        {
            // Реализуйте вашу логику аутентификации
        }

        // Метод для преобразования SecureString в string
        private string SecureStringToString(SecureString secureString)
        {
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(secureString);
            try
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
        }

        public LoginViewModel()
        {
            // Инициализация вашей ViewModel
        }
    }

}
