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
    internal class RemindersPageVM : ViewModelBase
    {
        private int _loggedInUserId;
        private FinanceControl_DBEntities context;
        private Reminders _selectedReminder;

        private string _comment;
        private DateTime _reminderDate = new DateTime(2023, 01, 01);
        private bool _status;

        public ObservableCollection<Reminders> UserReminders { get; set; }

        public string Comment
        {
            get => _comment;
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    OnPropertyChanged(nameof(Comment));
                }
            }
        }


        public bool Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public DateTime ReminderDate
        {
            get => _reminderDate;
            set
            {
                if (_reminderDate != value)
                {
                    _reminderDate = value;
                    OnPropertyChanged(nameof(ReminderDate));
                }
            }
        }

        public Reminders IsReminderSelected
        {
            get { return _selectedReminder; }
            set
            {
                if (_selectedReminder != value)
                {
                    _selectedReminder = value;
                    OnPropertyChanged(nameof(IsReminderSelected));
                    LoadSelectedReminderDetails();

                }
            }

        }

        public ICommand DeleteSelectedReminderCommand { get; }
        public ICommand UpdateSelectedReminderCommand { get; }
        public ICommand AddNewReminderCommand { get; }


        public RemindersPageVM(int loggedInUserId, FinanceControl_DBEntities transContext)
        {
            context = transContext;
            _loggedInUserId = loggedInUserId;
            UserReminders = new ObservableCollection<Reminders>();
            LoadUserReminders();
            DeleteSelectedReminderCommand = new ViewModelCommand(DeleteSelectedReminder, CanDeleteSelectedReminder);
            UpdateSelectedReminderCommand = new ViewModelCommand(UpdateSelectedReminder, CanUpdateSelectedReminders);
            AddNewReminderCommand = new ViewModelCommand(AddNewReminder, CanAddNewReminder);

        }

        private void DeleteSelectedReminder(object parameter)
        {
            if (IsReminderSelected != null)
            {
                // Удаляем из базы данных
                context.Reminders.Remove(IsReminderSelected);
                // Удаляем из коллекции
                var RemindersToRemove = context.Reminders.Where(Reminder => Reminder.ReminderID == IsReminderSelected.ReminderID).ToList();

                if (RemindersToRemove.Any())
                {
                    context.Reminders.RemoveRange(RemindersToRemove);
                    context.SaveChanges();
                }
                UserReminders.Remove(IsReminderSelected);
                EndOperation();
            }
        }

        private bool CanDeleteSelectedReminder(object parameter)
        {
            return IsReminderSelected != null;
        }

        private void UpdateSelectedReminder(object parameter)
        {
            if (IsReminderSelected != null)
            {
                // Обновляем в базе данных
                IsReminderSelected.ReminderDescription = Comment;
                IsReminderSelected.ReminderDate = ReminderDate;
                IsReminderSelected.IsCompleted = Status;
                EndOperation();
                LoadUserReminders();
            }

        }


        private bool CanUpdateSelectedReminders(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || ReminderDate == null)
            {
                return false;
            }
            else
            {
                return IsReminderSelected != null;

            }
        }

        private void AddNewReminder(object parameter)
        {
            Reminders newReminder = new Reminders
            {
                ReminderID = GetNextReminderId(),
                ReminderDescription = Comment,
                IsCompleted = Status,
                UserID = _loggedInUserId,
                ReminderDate = ReminderDate
            };
            context.Reminders.Add(newReminder);
            // Добавляем в коллекцию
            UserReminders.Add(newReminder);

            EndOperation();

        }

        private bool CanAddNewReminder(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || ReminderDate == null)
            {
                return false;
            }
            else
            {
                return true;
            }


        }

        private void LoadSelectedReminderDetails()
        {
            if (IsReminderSelected != null && context.Entry(IsReminderSelected).State != EntityState.Detached)
            {
                Status = IsReminderSelected.IsCompleted;
                Comment = IsReminderSelected.ReminderDescription;
                ReminderDate = IsReminderSelected.ReminderDate;

            }
        }

        private void LoadUserReminders()
        {
            UserReminders.Clear(); // Очищаем коллекцию перед загрузкой

            var Reminders = context.Reminders.Where(Reminder => Reminder.Users.UserID == _loggedInUserId).ToList();

            foreach (var Reminder in Reminders)
            {
                UserReminders.Add(Reminder);
            }

            OnPropertyChanged(nameof(UserReminders)); // Уведомляем об изменении свойства для обновления привязки в UI
        }

        private int GetNextReminderId()
        {
            // Находим максимальное значение AccountID в коллекции
            int maxReminderId = context.Reminders.Max(Reminder => (int?)Reminder.ReminderID) ?? 0;

            // Возвращаем следующее значение, увеличенное на 1
            return maxReminderId + 1;
        }

        private void EndOperation()
        {
            context.SaveChanges();
            IsReminderSelected = null;
            ReminderDate = new DateTime(2023, 01, 01);
            Status = false;
            Comment = null;
        }



    }
}
