using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AdaptHER.UI.UC
{
    /// <summary>
    /// The tooltip window
    /// </summary>
    public partial class NotifivationControl : UserControl
    {
        public NotifivationControl()
        {
            InitializeComponent();
            NotificationBorder.Opacity = 1;
        }
        public void Show(string message, int displayTime = 2000)
        {
            MessageText.Text = message;
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                BeginTime = TimeSpan.FromMilliseconds(displayTime)
            };
            fadeOut.Completed += (s, e) =>
            {
                if (Parent is Panel panel)
                {
                    panel.Children.Remove(this);
                }
            };
            NotificationBorder.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
