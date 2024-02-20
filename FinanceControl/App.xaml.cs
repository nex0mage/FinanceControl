using FinanceControl.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FinanceControl;
using FinanceControl.Model;

namespace FinanceControl
{
    public partial class App : Application
    {
        public static EventAggregator AppEventAggregator { get; } = new EventAggregator();
        public static FinanceControl_DBEntities AppDbContext { get; } = new FinanceControl_DBEntities();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            NavigationManager navigationManager = new NavigationManager(AppEventAggregator, AppDbContext);
            navigationManager.StartAtLoginView();
        }
    }
}
