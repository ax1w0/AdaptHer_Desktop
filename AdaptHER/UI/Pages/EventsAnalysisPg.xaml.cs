using AdaptHER.Class;
using AdaptHER.Model.Models;
using AdaptHER.Model;
using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

namespace AdaptHER.UI.Pages
{
    /// <summary>
    /// The page for analyzing adaptation activities.
    /// </summary>
    /// <remarks>
    /// Provides functionality for:
    /// 1. Visualizing the results of adaptation activities (graphs/charts)
    /// 2. Filtering results by departments and positions
    /// 3. Exporting reports in PDF and CSV formats
    /// 4. Printing reports
    /// </remarks>

    public partial class EventsAnalysisPg : Page, INotifyPropertyChanged
    {
        private int _reportCounter = 1;
        private SeriesCollection _seriesCollection;
        private string[] _labels;
        private Func<double, string> _formatter;
        private readonly IFeedbackService _feedbackService;
        public Action UpdateModulesAction { get; set; }
        public SeriesCollection SeriesCollection
        {
            get { return _seriesCollection; }
            set
            {
                _seriesCollection = value;
                OnPropertyChanged("SeriesCollection");
            }
        }
        public string[] Labels
        {
            get { return _labels; }
            set
            {
                _labels = value;
                OnPropertyChanged("Labels");
            }
        }
        public Func<double, string> Formatter
        {
            get { return _formatter; }
            set
            {
                _formatter = value;
                OnPropertyChanged("Formatter");
            }
        }
        public EventsAnalysisPg(IFeedbackService feedbackService = null)
        {
            _feedbackService = feedbackService ?? new DefaultFeedbackService();
            InitializeComponent();
            Loaded += EventsAnalysisPg_Loaded;

            selectJob.SelectionChanged += SelectJob_SelectionChanged;
            selectGroup.SelectionChanged += SelectGroup_SelectionChanged;
            view.SelectionChanged += View_SelectionChanged;
        }
        public void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    var printContainer = new Grid
                    {
                        Width = printDialog.PrintableAreaWidth,
                        Height = printDialog.PrintableAreaHeight,
                        Background = Brushes.White
                    };
                    printContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    printContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    printContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    var header = new TextBlock
                    {
                        Text = $"Report on adaptation measures №{_reportCounter}",
                        FontSize = 18,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 30, 0, 30),
                        Foreground = Brushes.Black
                    };
                    Grid.SetRow(header, 0);
                    printContainer.Children.Add(header);
                    var metaPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 15)
                    };
                    metaPanel.Children.Add(new TextBlock
                    {
                        Text = $"Date of formation: {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}",
                        FontSize = 12,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = Brushes.Gray
                    });
                    metaPanel.Children.Add(new Border
                    {
                        BorderThickness = new Thickness(0, 1, 0, 0),
                        BorderBrush = Brushes.LightGray,
                        Width = 300,
                        Margin = new Thickness(0, 10, 0, 10),
                        HorizontalAlignment = HorizontalAlignment.Center
                    });
                    Grid.SetRow(metaPanel, 1);
                    printContainer.Children.Add(metaPanel);
                    var contentGrid = LogicalTreeHelper.FindLogicalNode(this, "contentGrid") as FrameworkElement;
                    if (contentGrid != null)
                    {
                        var bitmap = new RenderTargetBitmap(
                            (int)contentGrid.ActualWidth,
                            (int)contentGrid.ActualHeight,
                            96, 96, PixelFormats.Pbgra32);
                        bitmap.Render(contentGrid);

                        var image = new System.Windows.Controls.Image
                        {
                            Source = bitmap,
                            Stretch = Stretch.Uniform,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(50, 90, 50, 50)
                        };
                        printContainer.Children.Add(image);
                    }
                    printDialog.PrintVisual(printContainer, $"Adaptation Report №{_reportCounter}");
                    FeedbackService.ShowFeedback("Report printed successfully!");
                    _reportCounter++;
                }
            }
            catch (Exception ex)
            {
                FeedbackService.ShowFeedback($"Print error: {ex.Message}");
            }
        }
        private IQueryable<Analytics> ApplyFilters(IQueryable<Analytics> query)
        {
            int selectedJobId = (selectJob.SelectedItem as ComboBoxValueItem<int>)?.Value ?? -1;
            int selectedGroupId = (selectGroup.SelectedItem as ComboBoxValueItem<int>)?.Value ?? -1;
            if (selectedJobId != -1)
                query = query.Where(a => a.RoleId == selectedJobId);
            if (selectedGroupId != -1)
                query = query.Where(a => a.DepartmentId == selectedGroupId);
            return query;
        }
        public void BtnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    Title = "Export to PDF",
                    FileName = $"Report_on_adaptation_measures_№{_reportCounter}_{DateTime.Now:dd.MM.yyyy_HH.mm.ss}.pdf"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    using (var fs = new FileStream(saveDialog.FileName, FileMode.Create))
                    {
                        Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                        PdfWriter writer = PdfWriter.GetInstance(document, fs);
                        document.Open();
                        string fontPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                        BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        Font titleFont = new Font(baseFont, 18, Font.BOLD, BaseColor.DARK_GRAY);
                        Paragraph title = new Paragraph("REPORT ON ADAPTATION MEASURES", titleFont);
                        title.Alignment = Element.ALIGN_CENTER;
                        title.SpacingAfter = 15;
                        document.Add(title);
                        Font subtitleFont = new Font(baseFont, 12, Font.NORMAL, BaseColor.GRAY);
                        Paragraph subtitle = new Paragraph(
                            $"№ {_reportCounter}\nDate of formation: {DateTime.Now:dd.MM.yyyy HH:mm}",
                            subtitleFont);
                        subtitle.Alignment = Element.ALIGN_CENTER;
                        subtitle.SpacingAfter = 20;
                        document.Add(subtitle);
                        document.Add(new Chunk(new LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_CENTER, -1)));
                        document.Add(new Paragraph(" "));
                        using (var db = new ApplicationDbContext())
                        {
                            var query = ApplyFilters(db.Analytics.AsQueryable());
                            var data = query.ToList();
                            if (data.Any())
                            {
                                PdfPTable table = new PdfPTable(5);
                                table.WidthPercentage = 100;
                                table.SpacingBefore = 15;
                                table.SpacingAfter = 20;
                                float[] columnWidths = { 1.5f, 2f, 2f, 1.5f, 1.5f };
                                table.SetWidths(columnWidths);
                                Font headerFont = new Font(baseFont, 10, Font.BOLD, BaseColor.WHITE);
                                AddPdfCell(table, "Program", headerFont, new BaseColor(70, 130, 180));
                                AddPdfCell(table, "Start Date", headerFont, new BaseColor(70, 130, 180));
                                AddPdfCell(table, "End Date", headerFont, new BaseColor(70, 130, 180));
                                AddPdfCell(table, "Tasks", headerFont, new BaseColor(70, 130, 180));
                                AddPdfCell(table, "Result (%)", headerFont, new BaseColor(70, 130, 180));
                                Font dataFont = new Font(baseFont, 10, Font.NORMAL);
                                foreach (var item in data)
                                {
                                    AddPdfCell(table, item.ProgramId.ToString(), dataFont);
                                    AddPdfCell(table, item.DateStart.ToString("dd.MM.yyyy"), dataFont);
                                    AddPdfCell(table, item.DateEnd.ToString("dd.MM.yyyy") ?? "-", dataFont);
                                    AddPdfCell(table, item.CountExercise.ToString(), dataFont);
                                    AddPdfCell(table, ((double)item.CountExeCorrect / item.CountExercise * 100).ToString("0.00"), dataFont);
                                }
                                document.Add(table);
                            }
                            else
                            {
                                Font noDataFont = new Font(baseFont, 14, Font.ITALIC, BaseColor.RED);
                                document.Add(new Paragraph("There is no data to display", noDataFont));
                            }
                        }
                        Font footerFont = new Font(baseFont, 10, Font.ITALIC, BaseColor.GRAY);
                        Paragraph footer = new Paragraph($"AdaptHER - {DateTime.Now:yyyy}", footerFont);
                        footer.Alignment = Element.ALIGN_RIGHT;
                        document.Add(footer);
                        document.Close();
                    }

                    FeedbackService.ShowFeedback("PDF report created successfully!");
                    _reportCounter++;
                }
            }
            catch (Exception ex)
            {
                FeedbackService.ShowFeedback($"Error creating PDF: {ex.Message}");
            }
        }
        private void AddPdfCell(PdfPTable table, string text, Font font, BaseColor bgColor = null)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.Padding = 5;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            if (bgColor != null)
            {
                cell.BackgroundColor = BaseColor.GRAY;
            }
            table.AddCell(cell);
        }
        public void BtnExportCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    Title = "Export to CSV",
                    FileName = $"Adaptation_Activities_Statement_#{_reportCounter}_{DateTime.Now:dd.MM.yyyy_HH.mm.ss}.csv"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    using (var writer = new StreamWriter(saveDialog.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("Program;Start Date;End Date;Total Tasks;Result (%)");

                        using (var db = new ApplicationDbContext())
                        {
                            var query = ApplyFilters(db.Analytics.AsQueryable());

                            foreach (var item in query.ToList())
                            {
                                writer.WriteLine(
                                    $"{item.ProgramId};" +
                                    $"{item.DateStart:dd.MM.yyyy};" +
                                    $"{item.DateEnd.ToString("dd.MM.yyyy") ?? "-"};" +
                                    $"{item.CountExercise};" +
                                    $"{((double)item.CountExeCorrect / item.CountExercise * 100).ToString("0.00", CultureInfo.InvariantCulture)}");
                            }
                        }
                    }
                    FeedbackService.ShowFeedback("Data successfully exported to CSV!");
                    _reportCounter++;
                }
            }
            catch (Exception ex)
            {
                FeedbackService.ShowFeedback($"Error exporting to CSV: {ex.Message}");
            }
        }
        public void View_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateModules();
        }
        public void SelectGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateModules();
        }
        public void SelectJob_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateModules();
        }
        public void InitializeComboBoxes()
        {
            selectJob.Items.Clear();
            selectJob.Items.Add(new ComboBoxValueItem<int> { DisplayText = "No", Value = -1 });
            selectGroup.Items.Clear();
            selectGroup.Items.Add(new ComboBoxValueItem<int> { DisplayText = "No", Value = -1 });
        }
        public void EventsAnalysisPg_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeComboBoxes();
            try
            {
                selectJob.Items.Clear();
                selectJob.Items.Add(new ComboBoxValueItem<int>()
                {
                    DisplayText = "No",
                    Value = -1
                });
                using (var db = new ApplicationDbContext())
                {
                    var roles = db.Roles.ToList();
                    foreach (var role in roles)
                    {
                        selectJob.Items.Add(new ComboBoxValueItem<int>()
                        {
                            DisplayText = role.Name,
                            Value = role.Id
                        });
                    }
                }
            }
            catch
            {
                FeedbackService.ShowFeedback("Failed to retrieve data on positions; please verify the connection to the server!");
                return;
            }
            try
            {
                selectGroup.Items.Clear();
                selectGroup.Items.Add(new ComboBoxValueItem<int>()
                {
                    DisplayText = "No",
                    Value = -1
                });
                using (var db = new ApplicationDbContext())
                {
                    var groups = db.Departments.ToList();
                    foreach (var group in groups)
                    {
                        selectGroup.Items.Add(new ComboBoxValueItem<int>()
                        {
                            DisplayText = group.Name,
                            Value = group.Id
                        });
                    }
                }
            }
            catch
            {
                FeedbackService.ShowFeedback("Failed to retrieve data on departments; please verify the connection to the server!");
                return;
            }
            view.SelectedIndex = 0;
        }
        public void UpdateModules()
        {
            if (UpdateModulesAction != null)
            {
                UpdateModulesAction();
                return;
            }
            if (selectJob == null || selectGroup == null || view == null)
                return;
            int selectedJobId = (selectJob.SelectedItem as ComboBoxValueItem<int>)?.Value ?? -1;
            int selectedGroupId = (selectGroup.SelectedItem as ComboBoxValueItem<int>)?.Value ?? -1;
            int viewType = view.SelectedIndex;
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var query = db.Analytics.AsQueryable();
                    if (selectedJobId != -1)
                    {
                        query = query.Where(a => a.RoleId == selectedJobId);
                    }
                    if (selectedGroupId != -1)
                    {
                        query = query.Where(a => a.DepartmentId == selectedGroupId);
                    }
                    var analyticsData = query.ToList();
                    if (viewType == 0)
                    {
                        ShowDataAsLineChart(analyticsData);
                    }
                    else
                    {
                        ShowDataAsColumnChart(analyticsData);
                    }
                }
            }
            catch (Exception)
            {
                FeedbackService.ShowFeedback("Error loading data; please verify the connection to the server!");
            }
        }
        public void ShowDataAsLineChart(List<Analytics> data)
        {
            var grid = LogicalTreeHelper.FindLogicalNode(this, "contentGrid") as Grid;
            if (grid == null)
            {
                grid = new Grid();
                grid.Name = "contentGrid";
                grid.Margin = new Thickness(20, 10, 20, 20);
                grid.Background = (Brush)FindResource("SecundaryBackgroundColor");
                var parentGrid = this.Content as Grid;
                if (parentGrid != null && parentGrid.RowDefinitions.Count > 2)
                {
                    parentGrid.Children.Remove(parentGrid.Children
                        .OfType<Grid>()
                        .FirstOrDefault(g => g.Name == "contentGrid"));

                    parentGrid.Children.Add(grid);
                    Grid.SetRow(grid, 2);
                }
            }
            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            if (!data.Any())
            {
                var noDataText = new TextBlock
                {
                    Text = "No data to display",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    Foreground = (Brush)FindResource("TextFourthColor")
                };
                grid.Children.Add(noDataText);
                return;
            }
            var chart = new CartesianChart();
            chart.Series = new SeriesCollection();
            var groupedData = data.GroupBy(a => a.ProgramId)
                .Select(g => new
                {
                    ProgramId = g.Key,
                    AvgCorrect = g.Average(a => (double)a.CountExeCorrect / a.CountExercise * 100),
                    Count = g.Count()
                })
                .OrderBy(x => x.ProgramId)
                .ToList();
            chart.Series.Add(new LineSeries
            {
                Title = "Percentage of correct answers",
                Values = new ChartValues<double>(groupedData.Select(x => x.AvgCorrect)),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10,
                DataLabels = true,
                LabelPoint = point => $"{point.Y:0.0}%",
                Stroke = (Brush)FindResource("PrimaryHomingTwoColor"),
                Fill = Brushes.Transparent
            });
            chart.AxisX.Add(new Axis
            {
                Title = "Adaptation Programs",
                Labels = groupedData.Select(x => $"{x.ProgramId}").ToArray(),
                Separator = new LiveCharts.Wpf.Separator { Step = 1 },
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("PrimaryHomingColor")
            });
            chart.AxisY.Add(new Axis
            {
                Title = "Percentage of correct answers",
                LabelFormatter = value => $"{value:0}%",
                MinValue = 0,
                MaxValue = 100,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("PrimaryHomingColor")
            });
            grid.Children.Add(chart);
        }
        public void ShowDataAsColumnChart(List<Analytics> data)
        {
            var grid = LogicalTreeHelper.FindLogicalNode(this, "contentGrid") as Grid;
            if (grid == null)
            {
                grid = new Grid();
                grid.Name = "contentGrid";
                grid.Margin = new Thickness(20, 10, 20, 20);
                grid.Background = (Brush)FindResource("SecundaryBackgroundColor");

                var parentGrid = this.Content as Grid;
                if (parentGrid != null && parentGrid.RowDefinitions.Count > 2)
                {
                    parentGrid.Children.Remove(parentGrid.Children
                        .OfType<Grid>()
                        .FirstOrDefault(g => g.Name == "contentGrid"));

                    parentGrid.Children.Add(grid);
                    Grid.SetRow(grid, 2);
                }
            }
            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            if (!data.Any())
            {
                var noDataText = new TextBlock
                {
                    Text = "No data to display",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    Foreground = (Brush)FindResource("TextFourthColor")
                };
                grid.Children.Add(noDataText);
                return;
            }
            var chart = new CartesianChart();
            chart.Series = new SeriesCollection();
            var groupedData = data.GroupBy(a => a.ProgramId)
                .Select(g => new
                {
                    ProgramId = g.Key,
                    StudentCount = g.Count(),
                    ProgramName = $"Program {g.Key}"
                })
                .OrderBy(x => x.ProgramId)
                .ToList();
            chart.Series.Add(new ColumnSeries
            {
                Title = "Number of trainees",
                Values = new ChartValues<int>(groupedData.Select(x => x.StudentCount)),
                DataLabels = true,
                LabelPoint = point => $"{point.Y} persons",
                Fill = (Brush)FindResource("PrimaryHomingTwoColor")
            });
            chart.AxisX.Add(new Axis
            {
                Title = "Adaptation Programs",
                Labels = groupedData.Select(x => $"{x.ProgramId}").ToArray(),
                Separator = new LiveCharts.Wpf.Separator { Step = 1 },
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("PrimaryHomingColor")
            });
            chart.AxisY.Add(new Axis
            {
                Title = "Number of trainees",
                LabelFormatter = value => $"{value}",
                MinValue = 0,
                MaxValue = groupedData.Max(x => x.StudentCount) * 1.1,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("PrimaryHomingColor")
            });

            grid.Children.Add(chart);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
