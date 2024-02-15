using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FinanceControl.View.MainViews.PageViews
{
    /// <summary>
    /// Логика взаимодействия для AccountsPage.xaml
    /// </summary>
    public partial class AccountsPage : Page
    {
        public AccountsPage()
        {
            InitializeComponent();
        }
        private void NumericUpDown_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsTextNumeric(e.Text))
            {
                e.Handled = true;
            }
        }

        private bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }
    }
}
