using AdaptHER.Class;
using AdaptHER.Model.Models;
using AdaptHER.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MiniExcelLibs;
using System.IO;
using System.ComponentModel;

namespace AdaptHER.UI.Pages
{
    /// <summary>
    /// Page for creating a new employee adaptation program.
    /// </summary>
    /// <remarks>
    /// Allows you to:
    /// 1. Select the employee to be trained
    /// 2. Assign a department and position
    /// 3. Select adaptation modules
    /// 4. Assign mentor(s)
    /// 5. Generate the employee adaptation program in Excel format
    /// </remarks>
    
    public partial class AdaptProgramsPg : Page, INotifyPropertyChanged
    {
        private int _fileCounter = 1;
        private string _employeeSearchText;
        private string _mentorSearchText;
        private bool _noEmployeeResults = false;
        private bool _noMentorResults = false;
        public event PropertyChangedEventHandler PropertyChanged;
        public string EmployeeSearchText
        {
            get => _employeeSearchText;
            set
            {
                _employeeSearchText = value;
                OnPropertyChanged(nameof(EmployeeSearchText));
                SearchEmployees();
            }
        }
        public string MentorSearchText
        {
            get => _mentorSearchText;
            set
            {
                _mentorSearchText = value;
                OnPropertyChanged(nameof(MentorSearchText));
                SearchMentors();
            }
        }
        public bool NoEmployeeResults
        {
            get => _noEmployeeResults;
            set
            {
                _noEmployeeResults = value;
                OnPropertyChanged(nameof(NoEmployeeResults));
            }
        }
        public bool NoMentorResults
        {
            get => _noMentorResults;
            set
            {
                _noMentorResults = value;
                OnPropertyChanged(nameof(NoMentorResults));
            }
        }
        public bool sortingTeaches = false;
        public Dictionary<int, List<ComboBoxValueItem<int>>> selectedTeaches = new Dictionary<int, List<ComboBoxValueItem<int>>>();
        public int? selectedModule = null;
        private readonly IFeedbackService _feedbackService;
        private readonly INotificationService _notificationService;
        private readonly IModulesProgService _modulesService;
        private readonly IEmployeeService _employeeService;
        public AdaptProgramsPg(
            IFeedbackService feedbackService = null,
            INotificationService notificationService = null,
            IModulesProgService modulesService = null,
            IEmployeeService employeeService = null)
        {
            _feedbackService = feedbackService ?? new DefaultFeedbackService();
            _notificationService = notificationService ?? new DefaultNotificationService();
            _modulesService = modulesService ?? new DefaultModulesService();
            _employeeService = employeeService ?? new DefaultEmployeeService();
            InitializeComponent();
            Loaded += ConstructorPg_Loaded;

            selectGroup.SelectionChanged += SelectGroup_SelectionChanged;
            selectJob.SelectionChanged += SelectJob_SelectionChanged;
            selectModules.SelectionChanged += SelectModules_SelectionChanged;
            selectTeach.SelectionChanged += SelectTeach_SelectionChanged;
            newEmployeeSearch.TextChanged += (s, e) => EmployeeSearchText = newEmployeeSearch.Text;
            mentorsSearch.TextChanged += (s, e) => MentorSearchText = mentorsSearch.Text;
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void SelectTeach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sortingTeaches)
            {
                return;
            }
            if (selectedModule == null)
            {
                return;
            }
            foreach (var item in e.RemovedItems)
            {
                selectedTeaches[selectedModule.Value].Remove((ComboBoxValueItem<int>)item);
            }
            foreach (var item in e.AddedItems)
            {
                selectedTeaches[selectedModule.Value].Add((ComboBoxValueItem<int>)item);
            }
        }
        public void SelectModules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sortingTeaches = true;
            selectTeach.SelectedItem = null;
            foreach (var item in e.RemovedItems)
            {
                selectedTeaches.Remove(((ComboBoxValueItem<int>)item).Value);
                selectedModule = null;
            }
            foreach (var item in e.AddedItems)
            {
                selectedTeaches.Add(((ComboBoxValueItem<int>)item).Value, new List<ComboBoxValueItem<int>>());
                selectedModule = ((ComboBoxValueItem<int>)item).Value;
            }
            sortingTeaches = false;
        }
        public void SelectJob_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.RemovedItems.Count != 0)
                {
                    selectModules.ItemsSource = new List<ComboBoxValueItem<int>>
                    {
                        new ComboBoxValueItem<int>
                        {
                            DisplayText = "Choose a position",
                            Value = -1
                        }
                    };
                    selectModules.IsEnabled = false;
                }

                foreach (var item in e.AddedItems)
                {
                    using (var db = new ApplicationDbContext())
                    {
                        int selectedJobId = ((ComboBoxValueItem<int>)item).Value;
                        string jobName = ((ComboBoxValueItem<int>)item).DisplayText;

                        var modules = db.Modules
                            .Include(x => x.ModulesPositions)
                            .Where(x => x.ModulesPositions
                            .Any(y => y.PositionId == selectedJobId))
                            .Select(x => new ComboBoxValueItem<int>()
                            {
                                DisplayText = x.Name,
                                Value = x.Id
                            }).ToList();

                        if (modules.Count == 0)
                        {
                            selectModules.ItemsSource = new List<ComboBoxValueItem<int>>
                            {
                                new ComboBoxValueItem<int>
                                {
                                    DisplayText = "There are no adaptation modules for the position",
                                    Value = -1
                                }
                            };
                            selectModules.IsEnabled = false;
                        }
                        else
                        {
                            selectModules.ItemsSource = modules;
                            selectModules.IsEnabled = true;
                        }
                    }
                }
                selectedTeaches.Clear();
            }
            catch
            {
                FeedbackService.ShowFeedback("Failed to retrieve data about the modules; please verify the connection to the server!");
                return;
            }
        }
        public void SelectGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.RemovedItems.Count != 0)
                {
                    selectJob.ItemsSource = new List<object>();
                }

                foreach (var item in e.AddedItems)
                {
                    using (var db = new ApplicationDbContext())
                    {
                        var roles = db.Roles
                            .Include(x => x.RolesDepartments)
                            .Where(x => x.RolesDepartments != null && x.RolesDepartments
                            .Any(y => y.DepartmentId == ((ComboBoxValueItem<int>)item).Value))
                            .Select(x => new ComboBoxValueItem<int>()
                            {
                                DisplayText = x.ToString(),
                                Value = x.Id
                            }).ToList();
                        selectJob.ItemsSource = roles;
                    }
                }
            }
            catch
            {
                FeedbackService.ShowFeedback("Failed to retrieve data on positions; please verify the connection to the server!");
                return;
            }
        }
        public void SearchEmployees()
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var persons = db.Persons
                        .Where(x => x.LastName.Contains(EmployeeSearchText) ||
                                   x.FirstName.Contains(EmployeeSearchText) ||
                                   x.Patronymic.Contains(EmployeeSearchText))
                        .Select(x => new
                        {
                            Person = x,
                            HasRole = x.RoleId.HasValue,
                            FullName = (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName + " ") +
                                       (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName + " ") +
                                       (string.IsNullOrEmpty(x.Patronymic) ? "" : x.Patronymic)
                        })
                        .ToList()
                        .Where(x => !x.HasRole)
                        .Select(x => new ComboBoxValueItem<int>()
                        {
                            DisplayText = x.FullName,
                            Value = x.Person.Id
                        })
                        .ToList();
                    NoEmployeeResults = !persons.Any();
                    if (persons.Count == 1 && !string.IsNullOrWhiteSpace(EmployeeSearchText))
                    {
                        selectEmploy.SelectedItem = persons.First();
                    }
                    else if (persons.Count == 0)
                    {
                        selectEmploy.ItemsSource = new List<ComboBoxValueItem<int>>
                        {
                            new ComboBoxValueItem<int>
                            {
                                DisplayText = "Nothing was found for your query",
                                Value = -1
                            }
                        };
                    }
                    else
                    {
                        selectEmploy.ItemsSource = persons;
                    }
                }
            }
            catch
            {
                FeedbackService.ShowFeedback("Unable to retrieve data about employees; please check the connection to the server!");
            }
        }
        public void SearchMentors()
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var teaches = db.Persons
                        .Where(x => x.RoleId.HasValue &&
                                    (x.LastName.Contains(MentorSearchText) ||
                                    x.FirstName.Contains(MentorSearchText) ||
                                    x.Patronymic.Contains(MentorSearchText)))
                        .Select(x => new ComboBoxValueItem<int>()
                        {
                            DisplayText = (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName + " ") +
                                          (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName + " ") +
                                          (string.IsNullOrEmpty(x.Patronymic) ? "" : x.Patronymic),
                            Value = x.Id
                        })
                        .ToList();
                    NoMentorResults = !teaches.Any();
                    if (teaches.Count == 1 && !string.IsNullOrWhiteSpace(MentorSearchText))
                    {
                        selectTeach.SelectedItem = teaches.First();
                    }
                    else if (teaches.Count == 0)
                    {
                        selectTeach.ItemsSource = new List<ComboBoxValueItem<int>>
                        {
                            new ComboBoxValueItem<int>
                            {
                                DisplayText = "Nothing was found for your query",
                                Value = -1
                            }
                        };
                        selectTeach.IsEnabled = false;
                    }
                    else
                    {
                        selectTeach.ItemsSource = teaches;
                        selectTeach.IsEnabled = true;
                    }
                }
            }
            catch
            {
                FeedbackService.ShowFeedback("Unable to retrieve data about mentors; please check the connection to the server!");
            }
        }
        public void ConstructorPg_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var persons = db.Persons
                        .Where(x => x.LastName.Contains(newEmployeeSearch.Text) || x.FirstName.Contains(newEmployeeSearch.Text) || x.Patronymic.Contains(newEmployeeSearch.Text))
                        .Select(x => new
                        {
                            Person = x,
                            HasRole = x.RoleId.HasValue,
                            FullName = (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName + " ") + 
                                       (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName + " ") +
                                       (string.IsNullOrEmpty(x.Patronymic) ? "" : x.Patronymic)
                        })
                        .ToList()
                        .Where(x => !x.HasRole)
                        .Select(x => new ComboBoxValueItem<int>()
                        {
                            DisplayText = x.FullName,
                            Value = x.Person.Id
                        })
                        .ToList();
                    selectEmploy.ItemsSource = persons;
                    var teaches = db.Persons
                        .Where(x => x.RoleId.HasValue && x.LastName.Contains(mentorsSearch.Text) || 
                                    x.FirstName.Contains(mentorsSearch.Text) || 
                                    x.Patronymic.Contains(mentorsSearch.Text))
                        .Select(x => new ComboBoxValueItem<int>()
                        {
                            DisplayText = (string.IsNullOrEmpty(x.LastName) ? "" : x.LastName + " ") +
                                          (string.IsNullOrEmpty(x.FirstName) ? "" : x.FirstName + " ") +
                                          (string.IsNullOrEmpty(x.Patronymic) ? "" : x.Patronymic),
                            Value = x.Id
                        }).ToList();
                    selectTeach.ItemsSource = teaches;
                }
            }
            catch
            {
                FeedbackService.ShowFeedback("Failed to retrieve data about employees; please verify the connection to the server!");
                return;
            }
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var departments = db.Departments
                        .Select(x => new ComboBoxValueItem<int>()
                        {
                            DisplayText = x.Name,
                            Value = x.Id
                        }).ToList();
                    selectGroup.ItemsSource = departments;
                }
            }
            catch
            {
                FeedbackService.ShowFeedback("Unable to retrieve data about the departments; please verify the connection to the server!");
                return;
            }
        }
        public void FormProg_ClickBtn(object sender, RoutedEventArgs e)
        {
            var currentPage = this;
            Window ownerWindow = Window.GetWindow(currentPage);
            if (selectEmploy.SelectedItem == null)
            {
                NotificationService.ShowNotification(ownerWindow, "Select a trainee", 2000);
                return;
            }
            if (selectTeach.SelectedIndex == -1)
            {
                NotificationService.ShowNotification(ownerWindow, "Select at least one mentor", 2000);
                return;
            }
            if (selectGroup.SelectedIndex == -1)
            {
                NotificationService.ShowNotification(ownerWindow, "Select a department", 2000);
                return;
            }
            if (selectJob.SelectedIndex == -1)
            {
                NotificationService.ShowNotification(ownerWindow, "Select a position", 2000);
                return;
            }
            if (selectModules.SelectedIndex == -1)
            {
                NotificationService.ShowNotification(ownerWindow, "Select at least one training module", 2000);
                return;
            }
            var selectedDepartment = (ComboBoxValueItem<int>)selectGroup.SelectedItem;
            var selectedRole = (ComboBoxValueItem<int>)selectJob.SelectedItem;
            var selectedPerson = (ComboBoxValueItem<int>)selectEmploy.SelectedItem;
            string fileName;
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            do
            {
                fileName = $"Adaptation_program_{_fileCounter}_{selectedPerson.DisplayText}_{DateTime.Now:yyyy-MM-dd}.xlsx"
                    .Replace(" ", "_");
                _fileCounter++;
            }
            while (File.Exists(Path.Combine(basePath, fileName)));
            List<dynamic> rows = new List<dynamic>
            {
                new {
                    ФИО = selectedPerson.DisplayText,
                    Отдел = selectedDepartment.DisplayText,
                    Должность = selectedRole.DisplayText,
                    Модуль = "",
                    Наставник = "",
                    Дата_начала = DateTime.Now.ToString("yyyy-MM-dd")
                }
            };
            using (var db = new ApplicationDbContext())
            {
                Programs program = new Programs()
                {
                    Name = basePath,
                };
                Analytics analytics = new Analytics()
                {
                    IsWorking = true,
                    CountExercise = 0,
                    CountExeCorrect = 0,
                };
                db.Programs.Add(program);
                db.SaveChanges();
                foreach (var item in selectedTeaches)
                {
                    Modules module = db.Modules.FirstOrDefault(x => x.Id == item.Key);
                    string moduleName = module?.Name ?? item.Key.ToString();
                    rows.Add(new
                    {
                        ФИО = "",
                        Отдел = "",
                        Должность = "",
                        Модуль = moduleName,
                        Наставник = item.Value.FirstOrDefault()?.DisplayText ?? "",
                        Дата_начала = ""
                    });
                    for (int i = 1; i < item.Value.Count; i++)
                    {
                        rows.Add(new
                        {
                            ФИО = "",
                            Отдел = "",
                            Должность = "",
                            Модуль = "",
                            Наставник = item.Value[i].DisplayText,
                            Дата_начала = ""
                        });
                    }
                    db.ModulesPrograms.Add(new ModulesPrograms()
                    {
                        ModuleId = item.Key,
                        ProgramId = program.Id
                    });
                }
                db.SaveChanges();
            }
            try
            {
                MiniExcel.SaveAs(Path.Combine(basePath, fileName), rows, overwriteFile: true);
                FeedbackService.ShowFeedback($"Program saved as: {fileName}");
            }
            catch (Exception ex)
            {
                FeedbackService.ShowFeedback($"Error while saving: {ex.Message}");
            }
            selectGroup.SelectedItem = null;
        }
    }
}
