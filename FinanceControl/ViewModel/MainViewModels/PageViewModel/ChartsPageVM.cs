using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FinanceControl.Model;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Windows;
using OxyPlot.Wpf;
using OxyPlot;
using System.Windows.Forms;
using OxyPlot.Series;
using System.Runtime.Remoting.Contexts;
using Xceed.Words.NET;
using OxyPlot.Legends;


namespace FinanceControl.ViewModel.MainViewModels.PageViewModel
{
    internal class ChartsPageVM : ViewModelBase
    {
        FinanceControl_DBEntities _context;
        private int _loggedInUserId; // Добавляем переменную для идентификатора пользователя
        private DateTime _startDate = new DateTime(2023, 1, 1);
        private DateTime _endDate = new DateTime(2024, 12, 31);

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged(nameof(StartDate));
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                }
            }
        }


        private SeriesCollection _totalPieSeries;

        public SeriesCollection TotalPieSeries
        {
            get { return _totalPieSeries; }
            set
            {
                _totalPieSeries = value;
                OnPropertyChanged(nameof(TotalPieSeries));
            }
        }

        public ICommand Save { get; }
        public ICommand UpdateChartPie { get; }

        public ChartsPageVM(int loggedInUserId)
        {
            _loggedInUserId = loggedInUserId; // Присваиваем идентификатор пользователя
            // Инициализация данных
            // Загрузка данных из базы данных
            LoadData();
            Save = new ViewModelCommand(SaveInFile);
            UpdateChartPie = new ViewModelCommand(UpdateChartByNewDate);
        }



        private void SaveInFile(object parameter)
        {
            _context = new FinanceControl_DBEntities();
            // Создаем объект PlotModel для графика
            var plotModel = new PlotModel { Title = "Отчет", TextColor = OxyColors.Black, TitleFontSize = 20 };

            // Получаем данные из базы данных с учетом выбранного временного диапазона
            var incomeTotal = _context.IncomeTransactions
                .Where(t => t.Accounts.UserID == _loggedInUserId && t.TransactionDate >= StartDate && t.TransactionDate <= EndDate)
                .Sum(t => (double?)t.Amount) ?? 0;

            var goalsTotal = _context.GoalsTransactions
                .Where(t => t.Accounts.UserID == _loggedInUserId && t.TransactionDate >= StartDate && t.TransactionDate <= EndDate)
                .Sum(t => (double?)t.Amount) ?? 0;

            var debtsTotal = _context.DebtsTransactions
                .Where(t => t.Accounts.UserID == _loggedInUserId && t.TransactionDate >= StartDate && t.TransactionDate <= EndDate)
                .Sum(t => (double?)t.Amount) ?? 0;

            var expensesTotal = _context.ExpensesTransactions
                .Where(t => t.Accounts.UserID == _loggedInUserId && t.TransactionDate >= StartDate && t.TransactionDate <= EndDate)
                .Sum(t => (double?)t.Amount) ?? 0;

            // Добавляем сектора данных для круговой диаграммы
            var pieSeries = new OxyPlot.Series.PieSeries
            {
                Background = OxyColors.Black,
                TextColor = OxyColors.White,
                FontSize = 17,
                Diameter = 1,
                InnerDiameter = 0,
                StartAngle = 0,
                Stroke = OxyColors.White,
                StrokeThickness = 2,
                Slices =
        {
            new PieSlice("Доходы", incomeTotal) { Fill = OxyColor.FromRgb(0, 128, 0) },
            new PieSlice("Цели", goalsTotal) { Fill = OxyColor.FromRgb(255, 165, 0) },
            new PieSlice("Долги", debtsTotal) { Fill = OxyColor.FromRgb(0, 0, 128) },
            new PieSlice("Расходы", expensesTotal) { Fill = OxyColor.FromRgb(139, 0, 0) },
        }
            };

            plotModel.Series.Add(pieSeries);

            // Вызываем диалоговое окно для сохранения файла Word
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Word files (*.docx)|*.docx|All files (*.*)|*.*",
                Title = "Выберите место для сохранения файла"
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Сохраняем график в изображение
                var imageFileName = Path.Combine(Path.GetTempPath(), "chart_image.png");
                SaveGraphImage(plotModel, imageFileName);

                // Создаем новый документ Word
                using (var doc = DocX.Create(saveFileDialog.FileName))
                {
                    // Вставляем изображение в документ
                    var image = doc.AddImage(imageFileName);
                    var picture = image.CreatePicture();
                    var paragraph = doc.InsertParagraph();
                    paragraph.InsertPicture(picture);

                    // Добавляем легенду к изображению в документе Word
                    foreach (var slice in pieSeries.Slices)
                    {
                        paragraph.AppendLine($"{slice.Label} - {slice.Value}");
                    }

                    // Сохраняем документ Word
                    doc.Save();
                }
            }
        }

        private void SaveGraphImage(PlotModel plotModel, string fileName)
        {
            // Сохраняем график в изображение PNG
            using (var stream = File.Create(fileName))
            {
                var exporter = new PngExporter { Width = 650, Height = 594 };
                exporter.Export(plotModel, stream);
            }
        }


        private void UpdateChartByNewDate(object parameter)
        {
            LoadData();
        }

        private void LoadData()
        {
            _context = new FinanceControl_DBEntities();
            TotalPieSeries = new SeriesCollection();
            using (var context = _context)
            {
                var incomeTransactions = context.IncomeTransactions
                    .Where(t => t.Accounts.UserID == _loggedInUserId)
                    .ToList();

                var goalsTransactions = context.GoalsTransactions
                    .Where(t => t.Accounts.UserID == _loggedInUserId)
                    .ToList();

                var debtsTransactions = context.DebtsTransactions
                    .Where(t => t.Accounts.UserID == _loggedInUserId)
                    .ToList();

                var expensesTransactions = context.ExpensesTransactions
                    .Where(t => t.Accounts.UserID == _loggedInUserId)
                    .ToList();

                // Расчет суммы транзакций по дате

                var incomeData = CalculateTotalAmountByDate(incomeTransactions, _startDate, _endDate);
                var goalsData = CalculateTotalAmountByDate(goalsTransactions, _startDate, _endDate);
                var debtsData = CalculateTotalAmountByDate(debtsTransactions, _startDate, _endDate);
                var expensesData = CalculateTotalAmountByDate(expensesTransactions, _startDate, _endDate);

                // Добавление данных в коллекции PieSeries
                AddDataToSeries(TotalPieSeries, incomeData, "Доходы", "#597B55");
                AddDataToSeries(TotalPieSeries, goalsData, "Цели", "#e3a832");
                AddDataToSeries(TotalPieSeries, debtsData, "Долги", "#497280");
                AddDataToSeries(TotalPieSeries, expensesData, "Расходы", "#754B42");
            }
        }



        private Dictionary<DateTime, decimal> CalculateTotalAmountByDate<T>(List<T> transactions, DateTime _startDate, DateTime _endDate)
            where T : class
        {
            return transactions
                .Where(t => t.GetType().GetProperty("TransactionDate") != null)
                .Where(t => (DateTime)t.GetType().GetProperty("TransactionDate").GetValue(t, null) >= _startDate &&
                            (DateTime)t.GetType().GetProperty("TransactionDate").GetValue(t, null) <= _endDate)
                .GroupBy(t => (DateTime)t.GetType().GetProperty("TransactionDate").GetValue(t, null))
                .ToDictionary(group => group.Key, group => group.Sum(t => (decimal)t.GetType().GetProperty("Amount").GetValue(t, null)));
        }

        private void AddDataToSeries(SeriesCollection series, Dictionary<DateTime, decimal> data, string seriesTitle, string hexColor)
        {
            System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);

            var pieSeries = new LiveCharts.Wpf.PieSeries
            {
                Title = seriesTitle,
                Values = new ChartValues<double>(data.Values.Select(value => (double)value)),
                DataLabels = true,
                Fill = new SolidColorBrush(color)
            };

            series.Add(pieSeries);
        }
    }
}
