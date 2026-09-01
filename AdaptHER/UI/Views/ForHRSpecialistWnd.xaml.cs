using AdaptHER.Class;
using AdaptHER.UI.Pages;
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
using System.Windows.Shapes;

namespace AdaptHER.UI.Views
{
    /// <summary>
    /// The main window of the HR specialist application, which displays a welcome message.
    /// </summary>
    /// <remarks>
    /// This window is the starting point after successful user authorization.
    /// It displays basic information about the system and the HR specialist’s access status.
    /// </remarks>
   
    public partial class ForHRSpecialistWnd : Window
    {
        public ForHRSpecialistWnd()
        {
            InitializeComponent();
            Frames.MainFrame = MainFrm;
            Frames.MainFrame.Navigate(new MainPg());
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
        private void btnAdaptModules_Click(object sender, RoutedEventArgs e)
        {
            Frames.MainFrame.Navigate(new AdaptModulesPg());
        }
        private void btnConstructor_Click(object sender, RoutedEventArgs e)
        {
            Frames.MainFrame.Navigate(new AdaptProgramsPg());
        }
        private void btnEventsAnalysis_Click(object sender, RoutedEventArgs e)
        {
            Frames.MainFrame.Navigate(new EventsAnalysisPg());
        }
        private void btnGoBack_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWnd goBackWnd = new AuthorizationWnd();
            goBackWnd.Show();
            Close();
        }
    }
}
