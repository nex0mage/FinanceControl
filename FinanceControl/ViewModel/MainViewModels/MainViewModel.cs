using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceControl.ViewModel.MainViewModels
{
    namespace FinanceControl.ViewModel.MainViewModels
    {
        internal class MainViewModel : ViewModelBase
        {
            private int _loggedInUserId;
            private EventAggregator _eventAggregator;
            private NavigationManager _navigationManager;


            public int LoggedInUserId
            {
                get { return _loggedInUserId; }
                set
                {
                    if (_loggedInUserId != value)
                    {
                        _loggedInUserId = value;
                        OnPropertyChanged(nameof(LoggedInUserId));
                    }
                }
            }

            public MainViewModel(int loggedInUserId, EventAggregator eventAggregator, NavigationManager navigationManager)
            {
                _loggedInUserId = loggedInUserId;
                _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
                _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

                // Остальной код инициализации...
            }
        }
    }
}
