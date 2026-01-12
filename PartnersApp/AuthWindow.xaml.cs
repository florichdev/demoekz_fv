using PartnersApp.Services;
using System.Windows;

namespace PartnersApp
{
    public partial class AuthWindow : Window
    {
        private Service _dataService;

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (_dataService == null)
                _dataService = new Service();

            if (_dataService.Authenticate(LoginTextBox.Text, PasswordBox.Password, out bool isAdmin))
            {
                var mainWindow = new MainWindow(isAdmin, LoginTextBox.Text);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("❌ Неверные данные\n\nТестовые данные:\n• admin / admin123\n• user / user123", "Ошибка входа");
            }
        }
    }
}