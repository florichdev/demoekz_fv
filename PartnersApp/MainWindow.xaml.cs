using PartnersApp.Models;
using PartnersApp.Services;
using System.ComponentModel;
using System.Windows;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Threading;
using System;

namespace PartnersApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Service _dataService = new Service();
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private Partner _selectedPartner;
        private string _currentUser;

        public bool IsAdmin { get; private set; }
        public string StatusText { get; private set; }
        public string CurrentTime { get; private set; }

        public MainWindow(bool isAdmin, string username = "")
        {
            InitializeComponent();
            IsAdmin = isAdmin;
            _currentUser = username;
            DataContext = this;
            
            InitializeTimer();
            UpdateStatusText();
            SetButtonStates();
            
            Loaded += async (s, e) => await LoadDataAsync();
        }

        private void InitializeTimer()
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => UpdateTime();
            _timer.Start();
            UpdateTime();
        }

        private void UpdateTime()
        {
            var moscowTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Russian Standard Time");
            CurrentTime = $"МСК: {moscowTime:HH:mm:ss dd.MM.yyyy}";
            OnPropertyChanged(nameof(CurrentTime));
        }

        private void UpdateStatusText()
        {
            StatusText = $"Пользователь: {_currentUser} ({(IsAdmin ? "Администратор" : "Пользователь")})";
            OnPropertyChanged(nameof(StatusText));
        }

        private void SetButtonStates()
        {
            var opacity = IsAdmin ? 1.0 : 0.5;
            AddButton.IsEnabled = EditButton.IsEnabled = DeleteButton.IsEnabled = IsAdmin;
            AddButton.Opacity = EditButton.Opacity = DeleteButton.Opacity = opacity;
        }

        private async Task LoadDataAsync()
        {
            await Task.Run(() =>
            {
                var partners = _dataService.GetPartners();
                Dispatcher.Invoke(() => PartnersGrid.ItemsSource = partners);
            });
        }

        private void LoadData() => PartnersGrid.ItemsSource = _dataService.GetPartners();

        private void AddPartner_Click(object sender, RoutedEventArgs e)
        {
            if (IsAdmin && new PartnerWindow().ShowDialog() == true) LoadData();
        }

        private void EditPartner_Click(object sender, RoutedEventArgs e)
        {
            if (IsAdmin && _selectedPartner != null && new PartnerWindow(_selectedPartner).ShowDialog() == true) LoadData();
        }

        private void DeletePartner_Click(object sender, RoutedEventArgs e)
        {
            if (IsAdmin && _selectedPartner != null &&
                MessageBox.Show("Удалить партнера?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _dataService.DeletePartner(_selectedPartner.Id);
                LoadData();
            }
        }

        private void ShowSales_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPartner != null)
            {
                new SalesWindow(_selectedPartner.Id, IsAdmin).ShowDialog();
                LoadData();
            }
        }

        private void ExportToCsv_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"Партнеры_{DateTime.Now:yyyy-MM-dd}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var partners = _dataService.GetPartners();
                    var csv = new StringBuilder("ID;Тип;Наименование;Директор;Телефон;Email;Адрес;Рейтинг\n");
                    
                    foreach (var p in partners)
                        csv.AppendLine($"{p.Id};{p.TypeName};{p.Name};{p.Director};{p.Phone};{p.Email};{p.Address};{p.Rating}");
                    
                    File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Данные экспортированы:\n{saveDialog.FileName}", "Экспорт завершен");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка");
                }
            }
        }

        private void SwitchUser_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            new AuthWindow().Show();
            Close();
        }

        private void PartnersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => _selectedPartner = PartnersGrid.SelectedItem as Partner;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            _dataService?.Dispose();
            base.OnClosed(e);
        }
    }
}