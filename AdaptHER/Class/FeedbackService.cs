using System.Windows;
using AdaptHER.UI.Views;

namespace AdaptHER.Class
{
    public static class FeedbackService
    {
        public static void ShowFeedback(string message)
        {
            var feedbackWindow = new FeedbackWnd(message);
            if (Application.Current?.MainWindow != null)
            {
                feedbackWindow.Owner = Application.Current.MainWindow;
                feedbackWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                feedbackWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            feedbackWindow.ShowDialog();
        }
    }
}
