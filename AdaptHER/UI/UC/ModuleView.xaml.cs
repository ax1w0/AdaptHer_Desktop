using AdaptHER.Class;
using AdaptHER.Model;
using AdaptHER.Model.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AdaptHER.UI.UC
{
    /// <summary>
    /// Element of the adaptation module.
    /// </summary>
    /// <remarks>
    /// Provides data about the adaptation module.
    /// Provides the ability to edit the development status of the adaptation module.
    /// </remarks>
    public partial class ModuleView : UserControl
    {
        bool canChange = false;
        private Modules _currentModule;
        public ModuleView(Modules module)
        {
            InitializeComponent();
            _currentModule = module;
            Loaded += Status_Loaded;
            body.Visibility = Visibility.Collapsed;

            if (module.Developers != null)
            {
                foreach (var dev in module.Developers.Select(x => x.Develops))
                {
                    if (dev != null)
                    {
                        developers.Items.Add(dev.ToString());
                    }
                }
            }
            if (module.Agreeds != null)
            {
                foreach (var personAgreed in module.Agreeds.Select(x => x.PersonsAgreeds))
                {
                    if (agreed != null)
                    {
                        agreed.Items.Add(personAgreed.ToString());
                    }
                }
            }
            if (module.ModulesPositions != null)
            {
                foreach (var position in module.ModulesPositions.Select(x => x.Positions))
                {
                    if (position != null)
                    {
                        roles.Items.Add(position.ToString());
                    }
                }
            }
            OrgModuleName.Text = module.OrgName;
            moduleName.Text = module.Name;
            moduleStatus.Text = module.Statuses.Name;
            header.MouseLeftButtonUp += Header_MouseLeftButtonUp;
        }
        private void Status_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                moduleStatus.Items.Clear();
                using (var db = new ApplicationDbContext())
                {
                    var statuses = db.Statuses.ToList();
                    foreach (var status in statuses)
                    {
                        moduleStatus.Items.Add(new ComboBoxValueItem<int>()
                        {
                            DisplayText = status.Name,
                            Value = status.Id
                        });
                        if (status.Id == _currentModule.Statuses.Id)
                        {
                            moduleStatus.SelectedIndex = moduleStatus.Items.Count - 1;
                        }
                    }
                }
            }
            catch
            {
                FeedbackService.ShowFeedback("Failed to retrieve data on module statuses; please verify the connection to the server!");
                return;
            }
        }
       
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (canChange == false)
            {
                canChange = true;
                moduleStatus.IsEnabled = true;
                SaveButton.IsEnabled = true;
            }
            else
            {
                canChange = false;
                moduleStatus.IsEnabled = false;
                SaveButton.IsEnabled = false;
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var selectedItem = moduleStatus.SelectedItem as ComboBoxValueItem<int>;

                    if (selectedItem != null && selectedItem.Value != -1)
                    {
                        var moduleToUpdate = db.Modules.Find(_currentModule.Id);
                        if (moduleToUpdate != null)
                        {
                            moduleToUpdate.Statuses = db.Statuses.Find(selectedItem.Value);
                            db.SaveChanges();

                            _currentModule.Statuses = moduleToUpdate.Statuses;
                            moduleStatus.Text = moduleToUpdate.Statuses.Name;
                        }
                    }

                    canChange = false;
                    moduleStatus.IsEnabled = false;
                    SaveButton.IsEnabled = false;

                    FeedbackService.ShowFeedback("Changes saved successfully!");
                }
            }
            catch
            {
                FeedbackService.ShowFeedback("Error saving changes!");
            }
        }
        private void Header_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            collapsedButton.IsChecked = !collapsedButton.IsChecked;
            body.Visibility = body.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
