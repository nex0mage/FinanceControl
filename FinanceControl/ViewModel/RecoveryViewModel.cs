using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FinanceControl.Model;
using System.Security;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;
using System.Data.Entity;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Runtime.Remoting.Contexts;

namespace FinanceControl.ViewModel
{
    internal class RecoveryViewModel : ViewModelBase
    {
        private string _email;
        private SecureString _newPassword;
        private string _recoveryCode;
        private string randomCode;
         // Здесь  контекст базы данных
        private FinanceControl_DB_Entities _dbContext = new FinanceControl_DB_Entities();

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


        public SecureString newPassword
        { 
            get { return _newPassword; }
            set
            {
                if (_newPassword != value)
                {
                    _newPassword = value;
                    OnPropertyChanged(nameof(_newPassword));
                }
            }
        }

        public string recoveryCode
        {
            get { return _recoveryCode; }
            set
            {
                if (_recoveryCode != value)
                {
                    _recoveryCode = value;
                    OnPropertyChanged(nameof(_newPassword));
                }
            }
        }

        private string UnsecuredString_newPassword
        {
            get { return _newPassword.ToUnsecuredString(); }
        }

        public ICommand RecoverCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SendRecoveryCodeCommand { get; }

        public RecoveryViewModel()
        {

            RecoverCommand = new ViewModelCommand(RecoverByCode, CanRecover_ByCode);
            SendRecoveryCodeCommand = new ViewModelCommand(SendRecoveryCode, CanSend_RecoveryCode);
           // CancelCommand = new ViewModelCommand();
        }


        // Проверки возможности выполнения комманд отправки кода восстановления и восстановления с помощью полученного когда
        public bool CanSend_RecoveryCode(object parameter)
        {
            bool validData;
            if (string.IsNullOrWhiteSpace(_email) || _newPassword == null || _newPassword.Length < 3 || _email.Length < 3)
                validData = false;
            else
                validData = true;
            return validData;

        }
        public bool CanRecover_ByCode(object parameter) 
        {
            bool validData;
            if (string.IsNullOrWhiteSpace(recoveryCode) || recoveryCode.Length < 6)
                validData = false; 
            else
                validData = true;
            return validData;
        
        }

        private void SendRecoveryCode(object parameter)
        {
            if (IsValidEmail(_email))
            {
                if (_dbContext.Users.FirstOrDefault(u => u.Email == _email) != null)
                {
                    randomCode = GenerateRandomCode();
                    SendEmail(_email, "Finapp: Ваш код восстановления", "Здравствуйте, Ваш код восстановления для аккаунта приложения для контроля финансов:   " + randomCode);
                    MessageBox.Show("Код для смены пароля успешно отправлен на почту.");
                }
                else
                {
                    MessageBox.Show("Пользователь с такой почтой не зарегистрирован");
                }

            }
            else
            {
                MessageBox.Show("Такой электронной почты не может существовать");
            }
        }

        private void RecoverByCode(object parameter)
        {
            if (recoveryCode == randomCode)
            {

                var userToUpdate = _dbContext.Users.FirstOrDefault(u => u.Email == _email);
                if (userToUpdate != null)
                {
                    // Найден пользователь, обновляем пароль
                    userToUpdate.UserPassword = UnsecuredString_newPassword;
                    _dbContext.SaveChanges();
                    MessageBox.Show("Пароль изменен успешно. Теперь используйте его для входа в ваш аккаунт.");
                    Window currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this);
                    if (currentWindow != null)
                    {
                        currentWindow.Close();
                    }

                }
                else
                {
                    MessageBox.Show("Возникла ошибка");
                }


            }
            else
            {
                MessageBox.Show("Неравильный код восстановления");
            }
        }


        private string GenerateRandomCode()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }



        private bool IsValidEmail(string email)
        {
            // Регулярное выражение для проверки адреса электронной почты
            string emailPattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";

            // Используем Regex.IsMatch для проверки соответствия
            return Regex.IsMatch(email, emailPattern);
        }


        private void SendEmail(string to, string subject, string body)
        {
            using (SmtpClient smtpClient = new SmtpClient("smtp.mail.ru", 587))
            {
                smtpClient.Credentials = new NetworkCredential("finance_control_application@mail.ru", "CJN7HxugeYBuNvg5MM64");
                smtpClient.EnableSsl = true;
                string senderEmail = "finance_control_application@mail.ru";

                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(senderEmail);
                    mailMessage.To.Add(to);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;

                    smtpClient.Send(mailMessage);
                }
            }
        }



    }
}
