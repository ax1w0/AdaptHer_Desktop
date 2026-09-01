using AdaptHER.Class;
using AdaptHER.UI.UC;
using AdaptHER.UI.Views;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdaptHER.UI.Pages
{
    /// <summary>
    /// The page for managing requests for the development of adaptation modules.
    /// </summary>
    /// <remarks>
    /// Provides functionality for:
    /// 1. Viewing adaptation modules with filtering by job titles
    /// 2. Searching for modules by name
    /// 3. Creating new adaptation modules
    /// </remarks>
    
    public partial class AdaptModulesPg : Page
    {
        private readonly IModulesService _modulesService;
        private readonly IFeedbackService _feedbackService;
        public ComboBox TestSelectJob => this.selectJob;
        public TextBox TestNameFilter => this.nameFilter;
        public StackPanel TestModulesContainer => this.modulesContainer;
        public Button TestCreateButton => this.createButton;
        public AdaptModulesPg(IModulesService modulesService = null, IFeedbackService feedbackService = null)
        {
            _modulesService = modulesService ?? new ModulesService();
            _feedbackService = feedbackService ?? new DefaultFeedbackService();
            InitializeComponent();
            Loaded += AdaptModulesPg_Loaded;

            selectJob.SelectionChanged += SelectJob_SelectionChanged;
            nameFilter.SelectionChanged += SearchButton_Click;
            nameFilter.TextChanged += SearchButton_Click;
            createButton.Click += CreateButton_Click;
        }
        public void RaiseLoadedEvent()
        {
            AdaptModulesPg_Loaded(this, new RoutedEventArgs());
        }
        private void AdaptModulesPg_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                selectJob.Items.Clear();
                selectJob.Items.Add(new ComboBoxValueItem<int>() { DisplayText = "Нет", Value = -1 });
                var roles = _modulesService.GetRoles();
                foreach (var role in roles)
                {
                    selectJob.Items.Add(new ComboBoxValueItem<int>()
                    {
                        DisplayText = role.Name,
                        Value = role.Id
                    });
                }
            }
            catch
            {
                _feedbackService.ShowFeedback("Failed to retrieve data on positions; please check the connection to the server!");
            }
        }
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            new CreateModuleWnd().ShowDialog();
            UpdateModules();
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateModules();
        }
        private void SelectJob_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateModules();
        }
        public void UpdateModules()
        {
            try
            {
                modulesContainer.Children.Clear();
                int? positionId = selectJob.SelectedValue == null || ((ComboBoxValueItem<int>)selectJob.SelectedValue).Value == -1
                    ? (int?)null
                    : ((ComboBoxValueItem<int>)selectJob.SelectedValue).Value;
                var modules = _modulesService.GetFilteredModules(positionId, nameFilter.Text).AsEnumerable()
            .OrderBy(m =>
            {
                if (m.StatusId == 2) return 1;
                if (m.StatusId == 1) return 2;
                if (m.StatusId == 3) return 3;
                if (m.StatusId == 4) return 4;
                return 5; 
            })
            .ToList();
                if (modules.Count == 0)
                {
                    modulesContainer.Children.Add(new TextBlock
                    {
                        Text = "Nothing was found for your query",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 16,
                        Margin = new Thickness(0, 20, 0, 0)
                    });
                }
                else
                {
                    foreach (var module in modules)
                    {
                        modulesContainer.Children.Add(new ModuleView(module));
                    }
                }
            }
            catch
            {
                _feedbackService.ShowFeedback("It was not possible to display the adaptation modules; please verify the connection to the server!");
            }
        }
    }
}
