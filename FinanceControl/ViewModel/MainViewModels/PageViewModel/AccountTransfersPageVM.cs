﻿using FinanceControl.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;


namespace FinanceControl.ViewModel.MainViewModels.PageViewModel
{
    internal class AccountTransfersPageVM : ViewModelBase
    {
        private int _loggedInUserId;
        private FinanceControl_DBEntities context;
        private Transfers _selectedTransfer;

        private Accounts _accountFrom;
        private Accounts _accountTo;
        private string _comment;
        private DateTime _transferDate = new DateTime(2024, 01, 01);
        private decimal _amount;

        public ObservableCollection<Transfers> UserTransfers { get; set; }

            private ObservableCollection<Accounts> _accounts;
            private ObservableCollection<Accounts> _accounts1;


            public ObservableCollection<Accounts> Accounts
            {
                get { return _accounts; }
                set
                {
                    if (_accounts != value)
                    {
                        _accounts = value;
                        OnPropertyChanged(nameof(Accounts));
                    }
                }
            }


            public ObservableCollection<Accounts> Accounts1
            {
                get { return _accounts1; }
                set
                {
                    if (_accounts1 != value)
                    {
                        _accounts1 = value;
                        OnPropertyChanged(nameof(Accounts1));
                    }
                }
            }

            public Accounts AccountFrom
            {
                get { return _accountFrom; }
                set
                {
                    if (_accountFrom != value)
                    {
                        _accountFrom = value;
                        OnPropertyChanged(nameof(AccountFrom));
                    }
                }
            }

            public Accounts AccountTo
            {
                get { return _accountTo; }
                set
                {
                    if (_accountTo != value)
                    {
                        _accountTo = value;
                        OnPropertyChanged(nameof(AccountTo));
                    }
                }
            }

        private void LoadAccountsFromContext()
        {
            var accountsFrom = context.Accounts.Where(accountFrom => accountFrom.Users.UserID == _loggedInUserId).ToList();
            var accountsTo = context.Accounts.Where(accountTo => accountTo.Users.UserID == _loggedInUserId).ToList();
            Accounts = new ObservableCollection<Accounts>(accountsFrom);
            Accounts1 = new ObservableCollection<Accounts>(accountsTo);
            OnPropertyChanged(nameof(Accounts));
            OnPropertyChanged(nameof(Accounts1));
        }

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

        public DateTime TransferDate
        {
            get => _transferDate;
            set
            {
                if (_transferDate != value)
                {
                    _transferDate = value;
                    OnPropertyChanged(nameof(TransferDate));
                }
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    string stringValue = value.ToString("0.##");
                    if (IsValidInput(stringValue))
                    {
                        _amount = value;
                        OnPropertyChanged(nameof(Amount));
                    }
                    else
                    {
                        MessageBox.Show("Ошибка вы можете ввести до 18 знаков перед запятой", "Ошибка");
                    }
                }
            }
        }


        public Transfers IsTransferSelected
        {
            get { return _selectedTransfer; }
            set
            {
                if (_selectedTransfer != value)
                {
                    _selectedTransfer = value;
                    OnPropertyChanged(nameof(IsTransferSelected));
                    LoadSelectedTransferDetails();
                    LoadAccountsFromContext();
                }
            }
        }

        public ICommand DeleteSelectedTransferCommand { get; }
        public ICommand UpdateSelectedTransferCommand { get; }
        public ICommand AddNewTransferCommand { get; }

        public AccountTransfersPageVM(int loggedInUserId, FinanceControl_DBEntities transContext)
        {
            context = transContext;
            _loggedInUserId = loggedInUserId;
            UserTransfers = new ObservableCollection<Transfers>();
            LoadAccountsFromContext();
            LoadUserTransfers();
            DeleteSelectedTransferCommand = new ViewModelCommand(DeleteSelectedTransfer, CanDeleteSelectedTransfer);
            UpdateSelectedTransferCommand = new ViewModelCommand(UpdateSelectedTransfer, CanUpdateSelectedTransfer);
            AddNewTransferCommand = new ViewModelCommand(AddNewTransfer, CanAddNewTransfer);
        }


        private void LoadSelectedTransferDetails()
        {
            if (IsTransferSelected != null && context.Entry(IsTransferSelected).State != EntityState.Detached)
            {
                Amount = IsTransferSelected.Amount;
                Comment = IsTransferSelected.Comment; 
                TransferDate = IsTransferSelected.TransferDate;
                AccountTo = IsTransferSelected.Accounts1; 
                AccountFrom = IsTransferSelected.Accounts;
            }
        }

        private void DeleteSelectedTransfer(object parameter)
        {
            if (IsTransferSelected != null)
            {
                int? oldAccountFromID = IsTransferSelected.AccountFromID;
                int? oldAccountToID = IsTransferSelected.AccountToID;
                var oldAccountFrom = context.Accounts.Find(oldAccountFromID);
                oldAccountFrom.Balance += IsTransferSelected.Amount;
                var oldAccountTo = context.Accounts.Find(oldAccountToID);
                oldAccountTo.Balance -= IsTransferSelected.Amount;
                context.Transfers.Remove(IsTransferSelected);
                UserTransfers.Remove(IsTransferSelected);
                EndOperation();
            }
        }

        private bool CanDeleteSelectedTransfer(object parameter)
        {
            return IsTransferSelected != null;
        }

        private void UpdateSelectedTransfer(object parameter)
        {
            if (IsTransferSelected != null)
            {
                IsTransferSelected.Comment = Comment;
                IsTransferSelected.TransferDate = TransferDate;
                IsTransferSelected.AccountFromID = AccountFrom.AccountID;
                IsTransferSelected.AccountToID = AccountTo.AccountID;
                int? oldAccountFromID = IsTransferSelected.AccountFromID;
                int? oldAccountToID = IsTransferSelected.AccountToID;
                UpdateAccountBalances(oldAccountFromID, AccountFrom.AccountID, oldAccountToID, AccountTo.AccountID);
            }
        }

        private void UpdateAccountBalances(int? oldAccountFromID, int newAccountFromID, int? oldAccountToID, int newAccountToID)
        {
            if (oldAccountFromID != newAccountFromID)
            {
                var oldAccountFrom = context.Accounts.Find(oldAccountFromID);
                oldAccountFrom.Balance += IsTransferSelected.Amount;
            }

            if (oldAccountToID != newAccountToID)
            {
                var oldAccountTo = context.Accounts.Find(oldAccountToID);
                oldAccountTo.Balance -= IsTransferSelected.Amount;
            }
            UpdateFinal();
        }

        private void UpdateFinal()
        {
            decimal difference = Amount - IsTransferSelected.Amount;
            if (difference > 0 && (AccountTo.Balance - difference) < 0)
            {
                MessageBox.Show("Кошелек получателя приобретает значение меньше нуля. Измените значение и повторите попытку.", "Операция прервана");
                return;
            }
            if (difference > 0 && (AccountFrom.Balance - difference) < 0)
            {
                MessageBox.Show("Кошелек отправителя приобретает значение меньше нуля. Измените значение и повторите попытку.", "Операция прервана");
                return;
            }
            AccountFrom.Balance -= difference;
            AccountTo.Balance += difference;
            IsTransferSelected.Amount = Amount;
            IsTransferSelected.Accounts = AccountFrom;
            IsTransferSelected.Accounts1 = AccountTo;
            EndOperation();
            LoadUserTransfers();

        }

        private bool CanUpdateSelectedTransfer(object parameter)
        {
            if(string.IsNullOrWhiteSpace(Comment)|| Amount == 0.0m || AccountFrom == null || AccountTo == null || AccountFrom == AccountTo)
            {
                return false;
            } 
            else
            {
                return IsTransferSelected != null;
            }
        }

        private void AddNewTransfer(object parameter)
        {
            if ((AccountFrom.Balance - Amount)>=0)
            {
                Transfers newTransfer = new Transfers
                {
                    TransferID = GetNextTransferId(),
                    Comment = Comment,
                    TransferDate = TransferDate,
                    AccountFromID = AccountFrom.AccountID,
                    AccountToID = AccountTo.AccountID,
                    Accounts = AccountFrom,
                    Accounts1 = AccountTo,
                    Amount = Amount
                };
                AccountFrom.Balance = AccountFrom.Balance - Amount;
                AccountTo.Balance = AccountTo.Balance + Amount;
                context.Transfers.Add(newTransfer);
                UserTransfers.Add(newTransfer);
                EndOperation();
            }
            else
            {
                MessageBox.Show("Перевод не может быть осуществлен, так как на счету отправителя нет такого количества средств", "Ошибка");
            }
        }

        private bool CanAddNewTransfer(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Comment) || Amount == 0.0m || AccountFrom == null || AccountTo == null || AccountFrom == AccountTo)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void LoadUserTransfers()
        {
            UserTransfers.Clear();
            var transfers = context.Transfers
                .Where(transfer => transfer.Accounts.Users.UserID == _loggedInUserId)
                .OrderByDescending(transfer => transfer.TransferDate)
                .ToList();
            foreach (var transfer in transfers)
            {
                UserTransfers.Add(transfer);
            }
            OnPropertyChanged(nameof(UserTransfers));
        }

        private int GetNextTransferId()
        {
            int maxTransferId = context.Transfers.Any() ? context.Transfers.Max(transfer => transfer.TransferID) : 0;
            return maxTransferId + 1;
        }

        private void EndOperation()
        {
            context.SaveChanges();
            IsTransferSelected = null;
            Amount = 0.00m;
            AccountFrom = null;
            AccountTo = null;
            TransferDate = new DateTime(2024, 01, 01);
            Comment = null;
            LoadUserTransfers();    
        }

        private bool IsValidInput(string input)
        {
            string pattern = @"^\d{1,18}(\.\d{2})?$";
            return Regex.IsMatch(input, pattern);
        }
    }
}
