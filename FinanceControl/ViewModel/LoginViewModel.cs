using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using FinanceControl.Model;
using System.Security;
using System.Runtime.InteropServices;
using FinanceControl.View;

namespace FinanceControl.ViewModel
{

    public class LoginViewModel : ViewModelBase
    {
        // Поля
        private string _login; //Логин
        private SecureString _password; //Пароль
        private string _errorMessage;
        private FinanceControl_DB_Entities _dbContext = new FinanceControl_DB_Entities(); // Здесь  контекст базы данных

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

        private string UnsecuredString_userPassword //Дешифратор пароля
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

        // Команды

        public ICommand LoginCommand { get; }
        public ICommand RegistrationCommand { get; }
        public ICommand RecoverCommand { get; }
        public ICommand PictureCloseApplication { get; }
        public ICommand CursorOverrideIbeamCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new ViewModelCommand(Login, CanLogin);
            PictureCloseApplication = new ViewModelCommand(CloseApp_Click);
            RegistrationCommand = new ViewModelCommand(RegistrationWindow_Open);
            RecoverCommand = new ViewModelCommand(RecoveryWindow_Open);


        }

        // Проверка на заполненность полей
        private bool CanLogin(object parameter)
        {
            bool validData;
            if (string.IsNullOrWhiteSpace(userLogin) || userLogin.Length < 3 || userPassword == null || userPassword.Length < 3)
                validData = false;
            else
                validData = true;
            return validData;


        }

        //Закрытие приложения при нажатии на икноку X
        private void CloseApp_Click(object parameter)
        {
            Application.Current.MainWindow.Close();

        }

        //Метод проверки данных на правильность и последующая аутентификация
        private void Login(object parameter)
        {
            _dbContext = new FinanceControl_DB_Entities();
            var user = _dbContext.Users.FirstOrDefault(u => u.UserLogin == _login && u.UserPassword == UnsecuredString_userPassword);
            if (user != null)
            {
                // Успешная аутентификация
                MessageBox.Show("Вход выполнен успешно!");

                // Вызываем событие и передаем UserID в MainWindowViewModel
            }
            else
            {
                // Неправильный логин или пароль, выводим MessageBox
                MessageBox.Show("Неверный логин или пароль");
            }


            //Логика аутентификации
        }


        // Методы перехода по окнам регистрации и восстановления
        private void RegistrationWindow_Open(object parameter)
        {
            RegisterView registerView = new RegisterView();
            registerView.ShowDialog();
        }
        private void RecoveryWindow_Open(object parameter)
        { 
            RecoveryView recoveryView = new RecoveryView();
            recoveryView.ShowDialog();
        }

    }


}
