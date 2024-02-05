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
using FinanceControl.View.MainViews;
using FinanceControl.ViewModel.MainViewModels;
using FinanceControl;

namespace FinanceControl.ViewModel
{

    public class LoginViewModel : ViewModelBase
    {

        // Поля
        private string _login; //Логин
        private SecureString _password; //Пароль
        private EventAggregator _eventAggregator;
        private FinanceControl_DB_Entities _dbContext;
        private NavigationManager _navigationManager;
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
        // Метод дешифровки пароля
        private string UnsecuredString_userPassword //Дешифратор пароля
        {
            get { return userPassword.ToUnsecuredString(); }
        }
        // Команды
        public ICommand LoginCommand { get; }
        public ICommand RegistrationCommand { get; }
        public ICommand RecoverCommand { get; }
        public ICommand PictureCloseApplication { get; }
        public ICommand CursorOverrideIbeamCommand { get; }
        // Конструктор окна
        public LoginViewModel(EventAggregator eventAggregator, FinanceControl_DB_Entities dbContext, NavigationManager navigationManager)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
            LoginCommand = new ViewModelCommand(Login, CanLogin);
            PictureCloseApplication = new ViewModelCommand(CloseApp_Click);
            RegistrationCommand = new ViewModelCommand(RegistrationWindow_Open);
            RecoverCommand = new ViewModelCommand(RecoveryWindow_Open);
        }
        private int _loggedInUserId;

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
            Window currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this);
            if (currentWindow != null)
            {
                currentWindow.Close();
            }
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
                _loggedInUserId = user.UserID;

                // Проверка на null перед использованием _eventAggregator
                if (_eventAggregator != null)
                {
                    _eventAggregator.PublishUserLoggedIn(_loggedInUserId);
                }

                // Проверка на null перед использованием _navigationManager
                if (_navigationManager != null)
                {
                    _navigationManager.NavigateToMainView(_loggedInUserId);
                }
                else
                {
                    MessageBox.Show("nullexeption");
                }

                CloseApp_Click(1);
                // Вызываем событие и передаем UserID в MainWindowViewModel
            }
            else
            {
                // Неправильный логин или пароль, выводим MessageBox
                MessageBox.Show("Неверный логин или пароль");
            }
        }
        // Методы перехода по окнам регистрации и восстановления
        private void RegistrationWindow_Open(object parameter)
        {
            _navigationManager.OpenRegisterView();
            CloseApp_Click(1);

        }
        private void RecoveryWindow_Open(object parameter)
        { 
            RecoveryView recoveryView = new RecoveryView();
            recoveryView.ShowDialog();
        }
    }
}
