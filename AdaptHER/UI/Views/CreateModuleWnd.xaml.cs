using AdaptHER.Class;
using AdaptHER.Model.Models;
using AdaptHER.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace AdaptHER.UI.Views
{
    /// <summary>
    /// The window for creating a request for the development of an adaptation module.
    /// </summary>
    /// <remarks>
    /// Allows you to:
    /// 1. Specify the code and general name of the module
    /// 2. Assign developers
    /// 3. Assign approvers
    /// 4. Select the position for the developed module
    /// 5. Specify the start and end dates of implementation
    /// 6. Save the adaptation module in the database
    /// </remarks>
  
    public partial class CreateModuleWnd : Window
    {
        private bool filteringDevelopers = false;
        private bool filteringAgreed = false;
        private bool filteringJob = false;

        private List<ComboBoxValueItem<int>> selectedDevelopers = new List<ComboBoxValueItem<int>>();
        private List<ComboBoxValueItem<int>> selectedAgreed = new List<ComboBoxValueItem<int>>();
        private List<ComboBoxValueItem<int>> selectedJob = new List<ComboBoxValueItem<int>>();

        public CreateModuleWnd()
        {
            InitializeComponent();
            Loaded += CreateModule_Loaded;

            developers.SelectionChanged += Developers_SelectionChanged;
            developersSearch.TextChanged += DevelopersSearch_TextChanged;

            agreed.SelectionChanged += Agreed_SelectionChanged;
            agreedSearch.TextChanged += AgreedSearch_TextChanged;

            job.SelectionChanged += Job_SelectionChanged;
            jobSearch.TextChanged += JobSearch_TextChanged;

            createButton.Click += CreateButton_Click;
        }
        private void CreateModule_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var develops = db.Persons
                        .Where(x => x.RoleId != null)
                        .Select(x => new ComboBoxValueItem<int>()
                        {
                            DisplayText = x.FirstName + " " + x.LastName + " " + x.Patronymic,
                            Value = x.Id
                        }).ToList();

                    var persons = db.Persons
                        .Where(x => x.RoleId != null)
                        .Select(x => new ComboBoxValueItem<int>()
                        {
                            DisplayText = x.FirstName + " " + x.LastName + " " + x.Patronymic,
                            Value = x.Id
                        }).ToList();

                    developers.ItemsSource = develops;
                    agreed.ItemsSource = persons;
                }
            }
            catch
            {
                FeedbackService.ShowFeedback("Failed to retrieve employee data, please verify the connection to the server!");
                return;
            }

            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var persons = db.Roles
                        .Select(x => new ComboBoxValueItem<int>()
                        {
                            DisplayText = x.Name,
                            Value = x.Id
                        }).ToList();

                    job.ItemsSource = persons;
                }
            }
            catch
            {
                FeedbackService.ShowFeedback("Failed to retrieve position data, please verify the connection to the server!");
                return;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(codeName.Text))
            {
                NotificationService.ShowNotification(this, "Enter the module code name", 2000);
                return;
            }
            if (string.IsNullOrEmpty(name.Text))
            {
                NotificationService.ShowNotification(this, "Enter the module name", 2000);
                return;
            }

            if (dateStart.SelectedDate == null)
            {
                NotificationService.ShowNotification(this, "Select the implementation period", 2000);
                return;
            }

            if (dateEnd.SelectedDate == null)
            {
                NotificationService.ShowNotification(this, "Select the implementation period", 2000);
                return;
            }

            if (dateStart.SelectedDate < DateTime.Now)
            {
                NotificationService.ShowNotification(this, "The implementation period cannot be earlier than the current day", 2000);
                return;
            }

            if (dateEnd.SelectedDate < DateTime.Now)
            {
                NotificationService.ShowNotification(this, "The implementation period cannot be earlier than the current day", 2000);
                return;
            }

            if (selectedDevelopers.Count == 0)
            {
                NotificationService.ShowNotification(this, "Select at least one developer", 2000);
                return;
            }

            if (selectedAgreed.Count == 0)
            {
                NotificationService.ShowNotification(this, "Select at least one approver", 2000);
                return;
            }

            if (selectedJob.Count == 0)
            {
                NotificationService.ShowNotification(this, "Select at least one position", 2000);
                return;
            }

            Modules module = new Modules()
            {
                Name = name.Text,
                OrgName = codeName.Text,
                DateStart = dateStart.SelectedDate.Value,
                DateEnd = dateEnd.SelectedDate.Value,
                StatusId = 2

            };

            using (var db = new ApplicationDbContext())
            {
                db.Modules.Add(module);
                db.SaveChanges();

                foreach (var dev in selectedDevelopers)
                {
                    db.Developers.Add(new Developers()
                    {
                        DevelopId = dev.Value,
                        ModuleId = module.Id,
                    });
                }

                foreach (var agreed in selectedAgreed)
                {

                    db.Agreeds.Add(new Agreeds()
                    {
                        AgreedId = agreed.Value,
                        ModuleId = module.Id,
                    });
                }

                foreach (var job in selectedJob)
                {
                    db.ModulesPositions.Add(new ModulesPositions()
                    {
                        PositionId = job.Value,
                        ModuleId = module.Id,
                    });
                }

                db.SaveChanges();
            }
            FeedbackService.ShowFeedback("The application has been successfully submitted!");
            Close();
        }

        private void Agreed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (filteringAgreed)
            {
                return;
            }

            foreach (var item in e.AddedItems.Cast<ComboBoxValueItem<int>>())
            {

                selectedAgreed.Add(item);
            }

            foreach (var item in e.RemovedItems.Cast<ComboBoxValueItem<int>>())
            {
                selectedAgreed.Remove(item);
            }
        }

        private void AgreedSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            filteringAgreed = true;

            CollectionView itemsViewOriginal = (CollectionView)CollectionViewSource.GetDefaultView(agreed.ItemsSource);

            bool skipFilter = String.IsNullOrEmpty(agreedSearch.Text);

            string text = agreedSearch.Text.ToLower();

            itemsViewOriginal.Filter = x =>
            {
                if (skipFilter)
                {
                    return true;
                }
                else
                {
                    if (((ComboBoxValueItem<int>)x).DisplayText.ToLower().Contains(text))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            };
            itemsViewOriginal.Refresh();

            agreed.SelectedItems.Clear();
            foreach (var item in selectedAgreed.Where(x => itemsViewOriginal.Contains(x)))
            {
                agreed.SelectedItems.Add(item);
            }

            filteringAgreed = false;
        }
        private void Developers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (filteringDevelopers)
            {
                return;
            }

            foreach (var item in e.AddedItems.Cast<ComboBoxValueItem<int>>())
            {
                selectedDevelopers.Add(item);
            }

            foreach (var item in e.RemovedItems.Cast<ComboBoxValueItem<int>>())
            {
                selectedDevelopers.Remove(item);
            }
        }

        private void DevelopersSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            filteringDevelopers = true;

            CollectionView itemsViewOriginal = (CollectionView)CollectionViewSource.GetDefaultView(developers.ItemsSource);

            bool skipFilter = String.IsNullOrEmpty(developersSearch.Text);

            string text = developersSearch.Text.ToLower();

            itemsViewOriginal.Filter = x =>
            {
                if (skipFilter)
                {
                    return true;
                }
                else
                {
                    if (((ComboBoxValueItem<int>)x).DisplayText.ToLower().Contains(text))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            };
            itemsViewOriginal.Refresh();

            developers.SelectedItems.Clear();
            foreach (var item in selectedDevelopers.Where(x => itemsViewOriginal.Contains(x)))
            {
                developers.SelectedItems.Add(item);
            }

            filteringDevelopers = false;
        }
        private void Job_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (filteringJob)
            {
                return;
            }

            foreach (var item in e.AddedItems.Cast<ComboBoxValueItem<int>>())
            {
                selectedJob.Add(item);
            }

            foreach (var item in e.RemovedItems.Cast<ComboBoxValueItem<int>>())
            {
                selectedJob.Remove(item);
            }
        }

        private void JobSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            filteringJob = true;

            CollectionView itemsViewOriginal = (CollectionView)CollectionViewSource.GetDefaultView(job.ItemsSource);

            bool skipFilter = String.IsNullOrEmpty(jobSearch.Text);

            string text = jobSearch.Text.ToLower();

            itemsViewOriginal.Filter = x =>
            {
                if (skipFilter)
                {
                    return true;
                }
                else
                {
                    if (((ComboBoxValueItem<int>)x).DisplayText.ToLower().Contains(text))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            };
            itemsViewOriginal.Refresh();

            job.SelectedItems.Clear();
            foreach (var item in selectedJob.Where(x => itemsViewOriginal.Contains(x)))
            {
                job.SelectedItems.Add(item);
            }

            filteringJob = false;
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
    }
}
