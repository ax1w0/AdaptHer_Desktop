using System.Windows;
using System.Windows.Controls;
using AdaptHER.UI.UC;

namespace AdaptHER.Class
{
    public static class NotificationService
    {
        public static void ShowNotification(Window window, string message, int displayTime = 2000)
        {
            if (window == null) return;
            window.Dispatcher.Invoke(() =>
            {
                var notification = new NotifivationControl();
                var container = window.FindName("NotificationContainer") as Panel;
                if (container == null)
                {
                    var grid = new Grid();
                    grid.HorizontalAlignment = HorizontalAlignment.Stretch;
                    grid.VerticalAlignment = VerticalAlignment.Bottom;
                    grid.Margin = new Thickness(0, 0, 0, 20);

                    if (window.Content is UIElement content)
                    {
                        window.Content = null;
                        var hostGrid = new Grid();
                        hostGrid.Children.Add(content);
                        hostGrid.Children.Add(grid);
                        window.Content = hostGrid;
                    }
                    else
                    {
                        window.Content = grid;
                    }

                    container = grid;
                }
                container.Children.Add(notification);
                notification.Show(message, displayTime);
            });
        }
    }
}
