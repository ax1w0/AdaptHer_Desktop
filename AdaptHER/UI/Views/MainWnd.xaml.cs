using AdaptHER.UI.Views;
using System.Windows;
using System.Windows.Input;

namespace AdaptHER
{
    /// <summary>
    /// The main application window, which displays a welcome message.
    /// </summary>
    /// <remarks>
    /// This window is the starting point after successful user authentication.
    /// It displays basic system information and access status.
    /// </remarks>
   
    public partial class MainWnd : Window
    {
        public MainWnd()
        {
            InitializeComponent();
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void RestoreBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }
        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void btnGoBack_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWnd goBackWnd = new AuthorizationWnd();
            goBackWnd.Show();
            Close();
        }
    }
}
