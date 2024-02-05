using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceControl.ViewModel
{
    public class EventAggregator
    {
        public event EventHandler<UserLoggedInEventArgs> UserLoggedIn;

        public void PublishUserLoggedIn(int userId)
        {
            UserLoggedIn?.Invoke(this, new UserLoggedInEventArgs(userId));
        }
    }

    public class UserLoggedInEventArgs : EventArgs
    {
        public int UserId { get; }

        public UserLoggedInEventArgs(int userId)
        {
            UserId = userId;
        }
    }
}
