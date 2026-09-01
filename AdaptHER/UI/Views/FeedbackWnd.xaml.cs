using System.Windows;
using System.Windows.Input;

namespace AdaptHER.UI.Views
{
    /// <summary>
    /// Feedback message window.
    /// </summary>
    public partial class FeedbackWnd : Window
    {
        public FeedbackWnd(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
