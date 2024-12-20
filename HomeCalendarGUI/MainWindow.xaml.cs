﻿
using Calendar;
using System.Data.SQLite;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Data.Entity.Core.Objects;
using System.Xml.Linq;
using System.Windows.Interop;
using static Calendar.Category;
using System.Windows.Markup;
using System.CodeDom;
using System.Reflection.PortableExecutable;
using System.Linq;


namespace HomeCalendarGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IView
    {
        private readonly Presenter presenter;
        private UpdateEventsWindow updateEventsWindow;
        private List<Category> categories; 

        private OpenFolderDialog openFolderDialog;
        
        private string fileDirectoryToStore;

        /// <summary>
        /// Main Menu of Home Calendar App
        /// </summary>
        /// <param name="useDefaultDb">If true, an empty database will automatically be created for the user</param>
        /// <param name="filePath">File path to the database</param>
        public MainWindow(bool useDefaultDb, string filePath = null)
        {
            InitializeComponent();
            
            //Create Calendar directory if it doesn't exist
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Calendar"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Calendar");
            }

            //Open Folder Dialog properties
            openFolderDialog = new OpenFolderDialog
            {
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Calendar",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Calendar",
            };

            //Validate if user is using the default database or specified database
            if (useDefaultDb)
            {
                presenter = new Presenter(this);
            }
            else
            {
                presenter = new Presenter(this, filePath);

            }

            presenter.GetCategoriesForAllCatsComboBoxes();
            presenter.GetCategoriesTypeInList();
            presenter.GetHomeCalendarItems(null, null, 0, false, false, false, false);
            SetTodaysDateOnDatePicker();
        }

        #region IView
        public async void DisplayErrorMessage(string msg)
        {
            message.Foreground = Brushes.PaleVioletRed;
            message.FontWeight = FontWeights.ExtraBold;
            message.Text = msg;
            await Task.Delay(10000);
            message.Text = null;
        }

        public async void DisplaySuccessfulMessage(string msg)
        {
            message.Foreground = Brushes.LightGreen;
            message.FontWeight = FontWeights.ExtraBold;
            message.Text = msg;
            await Task.Delay(10000);
            message.Text = null;
        }

        public void PopulateCategoriesInAllCatsComboBox(List<Category> categories)
        {
            this.categories = categories;
            catsComboBox.Items.Clear();
            CategoryFilterCmb.Items.Clear();
            
            // Sort categories alphabetically by their Description
            var sortedCategories = categories.OrderBy(c => c.Description).ToList();

            const int DEFAULT = 0;
            sortedCategories.ForEach(c => {
                catsComboBox.Items.Add(c);
                CategoryFilterCmb.Items.Add(c);
            });
            catsComboBox.SelectedIndex = DEFAULT;
            CategoryFilterCmb.SelectedIndex = DEFAULT;
        }      

        public void PopulateCategoryTypesComboBox(List<Category> categories)
        {
            foreach (var category in categories)
            {
                if (categoryTypecmbBox.Items.Contains(category.Type))
                {
                    //ignoring event types that have already been added because we do not want duplicates
                    continue;
                }
                categoryTypecmbBox.Items.Add(category.Type);
            }
        }

        public void ShowCalendarItems(List<CalendarItem> items)
        {   
            SetDataGridColumnsToDefault();
            CalendarItemsDataGrid.ItemsSource = items;
        }        
        
        public void ShowTotalBusyTimeByMonth(List<CalendarItemsByMonth> itemsByMonth)
        {
            SetDataGridColumnsToSummaryByMonth();
            CalendarItemsDataGrid.ItemsSource = itemsByMonth;
        }

        public void ShowTotalBusyTimeByCategory(List<CalendarItemsByCategory> itemsByCategory)
        {
            SetDataGridColumnsToSummaryByCategory();
            CalendarItemsDataGrid.ItemsSource = itemsByCategory;
        }

        public void ShowTotalBusyTimeByMonthAndCategory(List<Dictionary<string, object>> itemsByCategoryAndMonth)
        {

            CalendarItemsDataGrid.ItemsSource = itemsByCategoryAndMonth;
            CalendarItemsDataGrid.Columns.Clear();

            // get list of column name from first dictionary in the list
            // and create column and bind to dictionary element

            bool skipKey = false;

            //Go through each section of the dictionary
            for (int i = 0; i < itemsByCategoryAndMonth.Count; i++)
            {   
                //Go through each key
                foreach (string key in itemsByCategoryAndMonth[i].Keys)
                {
                    List<DataGridColumn> currentTextColumns = CalendarItemsDataGrid.Columns.ToList();
                    skipKey = false;

                    //Ommit the ones that have "items:"
                    if (key.Contains("items:"))
                    {
                        continue;
                    }

                    //Checks if a column header from dictionary already exists
                    foreach (var currentDGData in currentTextColumns)
                    {
                        if (currentDGData.Header.Equals(key))
                        {
                            skipKey = true;
                        }
                    }

                    if (skipKey)
                    {
                        continue;
                    }

                    var column = new DataGridTextColumn();
                    column.Header = key;
                    column.Binding = new Binding($"[{key}]");
                    CalendarItemsDataGrid.Columns.Add(column);
                }
            }
        }
        #endregion

        private void RefreshMainView()
        {
            presenter.GetCategoriesTypeInList();
            presenter.GetCategoriesForAllCatsComboBoxes();            
            
            try
            {
                //Check if the user is filtering by category and/ or month
                bool summaryByMonth = (bool)SummaryByMonthCheckBox.IsChecked;
                bool summaryByCategory = (bool)SummaryByCategoryCheckBox.IsChecked;
                bool filterByCategory = (bool)FilterByCategoryCheckBox.IsChecked;

                if (DateFilterCheckBox.IsChecked == true)
                {
                    DateTime? start = Start.SelectedDate;
                    DateTime? end = End.SelectedDate;
                    bool filterByDate = (bool)DateFilterCheckBox.IsChecked;
                    Category cat = CategoryFilterCmb.SelectedItem as Category;
                    int categoryId = cat.Id;

                    presenter.GetHomeCalendarItems(start, end, categoryId, filterByDate, summaryByCategory, summaryByMonth, filterByCategory);
                    
                }
                else
                {
                    bool filterByDate = (bool)DateFilterCheckBox.IsChecked;
                    Category cat = CategoryFilterCmb.SelectedItem as Category;
                    int categoryId = cat.Id;
                    presenter.GetHomeCalendarItems(null, null, categoryId, filterByDate, summaryByCategory, summaryByMonth, filterByCategory);                    
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"An unknown error occured: {ex.Message}","Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        private void SetTodaysDateOnDatePicker()
        {
            StartDate.DisplayDateStart = DateTime.Now;
        }

        private void SetDataGridColumnsToDefault()
        {
            const int StartDate = 0, StartTime = 1;

            List<DataGridTextColumn> textColumns = new List<DataGridTextColumn>
            {
                new DataGridTextColumn {Header ="Start Date", Binding = new Binding("StartDateTime")},
                new DataGridTextColumn {Header ="Start Time", Binding = new Binding("StartDateTime")},
                new DataGridTextColumn {Header ="Category", Binding = new Binding("Category")},
                new DataGridTextColumn {Header ="Description", Binding = new Binding("ShortDescription")},
                new DataGridTextColumn {Header ="Duration", Binding = new Binding("DurationInMinutes")},
                new DataGridTextColumn {Header ="Busy Time", Binding = new Binding("BusyTime")},
            };
            textColumns[StartDate].Binding.StringFormat = "yyyy/MM/dd";
            textColumns[StartTime].Binding.StringFormat = "hh:mm:ss";


            CalendarItemsDataGrid.Columns.Clear();     // Clear all existing columns on the DataGrid control.                                                                   
            textColumns.ForEach(CalendarItemsDataGrid.Columns.Add);
        }

        private void SetDataGridColumnsToSummaryByMonth()
        {

            List<DataGridTextColumn> textColumns = new List<DataGridTextColumn>
            {
                new DataGridTextColumn {Header ="Month", Binding = new Binding("Month")},
                new DataGridTextColumn {Header ="Total Busy Time", Binding = new Binding("TotalBusyTime")},
            };

            CalendarItemsDataGrid.Columns.Clear();     // Clear all existing columns on the DataGrid control.                                                                   
            textColumns.ForEach(CalendarItemsDataGrid.Columns.Add);
        }

        private void SetDataGridColumnsToSummaryByCategory()
        {
            List<DataGridTextColumn> textColumns = new List<DataGridTextColumn>
            {
                new DataGridTextColumn {Header ="Category", Binding = new Binding("Category")},
                new DataGridTextColumn {Header ="Total Busy Time", Binding = new Binding("TotalBusyTime")},
            };

            CalendarItemsDataGrid.Columns.Clear();     // Clear all existing columns on the DataGrid control.                                                                   
            textColumns.ForEach(CalendarItemsDataGrid.Columns.Add);
        }

        private bool IsValidDescription(string desc)
        {
            if (string.IsNullOrEmpty(desc))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(desc))
            {
                return false;
            }
            if (float.TryParse(desc, out float s))
            {
                return false;
            }
            return true;
        }

        #region Btn Operations
        private void Btn_SaveCalendarFileTo(object sender, RoutedEventArgs e)
        {
            if (openFolderDialog.ShowDialog() == true)
            {
                fileDirectoryToStore = openFolderDialog.FolderName;
                openFolderDialog.InitialDirectory = fileDirectoryToStore;
                openFolderDialog.FolderName = "";
            }
        }

        private void Button_ClickAddCategory(object sender, RoutedEventArgs e)
        {
            var eventTypeChoice = categoryTypecmbBox.SelectedItem;
            string desc = DescriptionBox.Text;

            if (eventTypeChoice != null && !string.IsNullOrEmpty(desc))
            {
                CategoryType type = (CategoryType)eventTypeChoice;

                //checking if description is filled of numbers which would be unvalid 
                if (!IsValidDescription(desc))
                {
                    DisplayErrorMessage("Please enter a valid description.");
                    return;
                }
                
                presenter.AddNewCategory(desc, type);                
                DescriptionBox.Clear();
                categoryTypecmbBox.SelectedIndex = -1;
                RefreshMainView();                
            }
            else
            {
                DisplayErrorMessage("You cannot leave any fields empty.");
            }
        }
        
        private void Button_ClickAddEvent(object sender, RoutedEventArgs e)
        {
            #region Validation
            //verifying input data for events 
            if (StartDate.SelectedDate == null)
            {
                DisplayErrorMessage("Please select a start date for the event.");
                return;
            }
            if (!DateTime.TryParse(StartTime.Text, out DateTime time))
            {
                DisplayErrorMessage("Time isn't provided in the right format");
                return;
            }
            if (catsComboBox.SelectedItem == null)
            {
                DisplayErrorMessage("Please select a category for the event.");
                return;
            }
            if (!IsValidDescription(EventDescriptionBox.Text))
            {
                DisplayErrorMessage("Please enter a description for the event.");
                return;
            }
            if (DurationInMinutes.Text == null)
            {
                DisplayErrorMessage("Please choose a duration in minutes for your event.");
                return;
            }            


            //Add the time
            DateTime startDate = StartDate.SelectedDate.Value;            
            startDate = startDate.AddHours(time.Hour);
            startDate = startDate.AddMinutes(time.Minute);
            startDate = startDate.AddSeconds(time.Second);

            //getting duration in minutes
            if (!double.TryParse(DurationInMinutes.Text, out double endTimeInMinutes))
            {
                DisplayErrorMessage("Please enter a number for your duration in minutes.");
                return;
            }

            if (endTimeInMinutes <= 0)
            {
                DisplayErrorMessage("The duration of your event must not be negative.");
                return;
            }
            #endregion

            try
            {
                Category selectedCategory = (Category)catsComboBox.SelectedItem;
                string description = EventDescriptionBox.Text;

                presenter.AddNewEvent(startDate, selectedCategory.Id, description, endTimeInMinutes);
            }
            catch
            {
                DisplayErrorMessage("There was an error. Please try again.");                
            }

            // Clear the input fields 
            StartDate.SelectedDate = null;
            catsComboBox.SelectedIndex = -1;
            StartTime.Clear();
            DurationInMinutes.Clear();
            EventDescriptionBox.Clear();
            RefreshMainView();
        }

        private void Button_ClickCancelEvent(object sender, RoutedEventArgs e)
        {
            EventDescriptionBox.Clear();
        }

        private void Btn_DeleteEvent(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to permanently delete this event?", "Deleting an Event", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                CalendarItem item = CalendarItemsDataGrid.SelectedItem as CalendarItem;
                try
                {
                    if(item == null)
                    {
                        throw new Exception("This event does not exist");
                    }

                    presenter.DeleteAnEvent(item.EventID);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to delete this calendar item: "+ex.Message,"Error",MessageBoxButton.OK,MessageBoxImage.Hand);
                }
                RefreshMainView();
            }
        }

        private void Btn_UpdateEvent(object sender, RoutedEventArgs e)
        {
            CalendarItem item = CalendarItemsDataGrid.SelectedItem as CalendarItem;

            updateEventsWindow = new UpdateEventsWindow(presenter, categories, item, this);
            this.Hide();

            DateFilterCheckBox.IsChecked = false;
            SummaryByMonthCheckBox.IsChecked = false;
            SummaryByCategoryCheckBox.IsChecked = false;

            updateEventsWindow.Show();
        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Filters CheckBox Operations
        private void DateFilterCheckBoxClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check if the user is filtering by category and/ or month
                bool summaryByMonth = (bool)SummaryByMonthCheckBox.IsChecked;
                bool summaryByCategory = (bool)SummaryByCategoryCheckBox.IsChecked;
                bool filterByCategory = (bool)FilterByCategoryCheckBox.IsChecked;

                if (DateFilterCheckBox.IsChecked == true)
                {
                    DateTime? start = Start.SelectedDate;
                    DateTime? end = End.SelectedDate;
                    bool filterByDate = (bool)DateFilterCheckBox.IsChecked;
                    Category cat = CategoryFilterCmb.SelectedItem as Category;
                    int categoryId = cat.Id;

                    presenter.GetHomeCalendarItems(start, end, categoryId, filterByDate, summaryByCategory, summaryByMonth, filterByCategory);                    
                }
                else
                {
                    bool filterByDate = (bool)DateFilterCheckBox.IsChecked;
                    Category cat = CategoryFilterCmb.SelectedItem as Category;
                    int categoryId = cat.Id;
                    presenter.GetHomeCalendarItems(null, null, categoryId, filterByDate, summaryByCategory, summaryByMonth, filterByCategory);                   
                }
            }
            catch (InvalidOperationException ex)
            {
                if (ex is InvalidOperationException)
                {
                    MessageBox.Show($"Date Filter Error: {ex.Message}", "DateTime Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Unknown Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                DateFilterCheckBox.IsChecked = false;
                Start.SelectedDate = null;
                End.SelectedDate = null;
            }
        }

        private void FilterByCategoryCheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check if the user is filtering by category and/ or month
                bool summaryByMonth = (bool)SummaryByMonthCheckBox.IsChecked;
                bool summaryByCategory = (bool)SummaryByCategoryCheckBox.IsChecked;
                bool filterByCategory = (bool)FilterByCategoryCheckBox.IsChecked;

                if (DateFilterCheckBox.IsChecked == true)
                {
                    DateTime? start = Start.SelectedDate;
                    DateTime? end = End.SelectedDate;
                    bool filterByDate = (bool)DateFilterCheckBox.IsChecked;
                    Category cat = CategoryFilterCmb.SelectedItem as Category;
                    int categoryId = cat.Id;

                    presenter.GetHomeCalendarItems(start, end, categoryId, filterByDate, summaryByCategory, summaryByMonth, filterByCategory);
                }
                else
                {
                    bool filterByDate = (bool)DateFilterCheckBox.IsChecked;
                    Category cat = CategoryFilterCmb.SelectedItem as Category;
                    int categoryId = cat.Id;
                    presenter.GetHomeCalendarItems(null, null, categoryId, filterByDate, summaryByCategory, summaryByMonth, filterByCategory);
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                {
                    MessageBox.Show($"Date Filter Error: {ex.Message}", "DateTime Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Unknown Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                SummaryByCategoryCheckBox.IsChecked = false;
            }
        }

        private void SummaryByMonthCheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check if the user is filtering by category and/ or month
                bool summaryByMonth = (bool)SummaryByMonthCheckBox.IsChecked;
                bool summaryByCategory = (bool)SummaryByCategoryCheckBox.IsChecked;
                bool filterByCategory = (bool)FilterByCategoryCheckBox.IsChecked;

                if (DateFilterCheckBox.IsChecked == true)
                {
                    DateTime? start = Start.SelectedDate;
                    DateTime? end = End.SelectedDate;
                    bool filterByDate = (bool)DateFilterCheckBox.IsChecked;
                    Category cat = CategoryFilterCmb.SelectedItem as Category;
                    int categoryId = cat.Id;

                    presenter.GetHomeCalendarItems(start, end, categoryId, filterByDate, summaryByCategory, summaryByMonth, filterByCategory);
                }
                else
                {
                    bool filterByDate = (bool)DateFilterCheckBox.IsChecked;
                    Category cat = CategoryFilterCmb.SelectedItem as Category;
                    int categoryId = cat.Id;
                    presenter.GetHomeCalendarItems(null, null, categoryId, filterByDate, summaryByCategory, summaryByMonth, filterByCategory);
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                {
                    MessageBox.Show($"Date Filter Error: {ex.Message}", "DateTime Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Unknown Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                SummaryByMonthCheckBox.IsChecked = false;
            }
        }

        private void SummaryByCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check if the user is filtering by category and/ or month
                bool summaryByMonth = (bool)SummaryByMonthCheckBox.IsChecked;
                bool summaryByCategory = (bool)SummaryByCategoryCheckBox.IsChecked;
                bool filterByCategory = (bool)FilterByCategoryCheckBox.IsChecked;

                if (DateFilterCheckBox.IsChecked == true)
                {
                    DateTime? start = Start.SelectedDate;
                    DateTime? end = End.SelectedDate;
                    bool filterByDate = (bool)DateFilterCheckBox.IsChecked;
                    Category cat = CategoryFilterCmb.SelectedItem as Category;
                    int categoryId = cat.Id;

                    presenter.GetHomeCalendarItems(start, end, categoryId, filterByDate, summaryByCategory, summaryByMonth, filterByCategory);
                }
                else
                {
                    bool filterByDate = (bool)DateFilterCheckBox.IsChecked;
                    Category cat = CategoryFilterCmb.SelectedItem as Category;
                    int categoryId = cat.Id;
                    presenter.GetHomeCalendarItems(null, null, categoryId, filterByDate, summaryByCategory, summaryByMonth, filterByCategory);
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                {
                    MessageBox.Show($"Date Filter Error: {ex.Message}", "DateTime Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Unknown Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                SummaryByCategoryCheckBox.IsChecked = false;
            }
        }

        private void CategoryFilterCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //Check if the user is filtering by category and/ or month
                bool summaryByMonth = (bool)SummaryByMonthCheckBox.IsChecked;
                bool summaryByCategory = (bool)SummaryByCategoryCheckBox.IsChecked;
                bool filterByCategory = (bool)FilterByCategoryCheckBox.IsChecked;

                if (DateFilterCheckBox.IsChecked == true)
                {
                    DateTime? start = Start.SelectedDate;
                    DateTime? end = End.SelectedDate;
                    bool filterByDate = (bool)DateFilterCheckBox.IsChecked;
                    Category cat = CategoryFilterCmb.SelectedItem as Category;
                    int categoryId = cat == null ? 0 : cat.Id;

                    presenter.GetHomeCalendarItems(start, end, categoryId, filterByDate, summaryByCategory, summaryByMonth, filterByCategory);
                }
                else
                {
                    bool filterByDate = (bool)DateFilterCheckBox.IsChecked;
                    Category cat = CategoryFilterCmb.SelectedItem as Category;
                    int categoryId = cat == null ? 0 : cat.Id;
                    presenter.GetHomeCalendarItems(null, null, categoryId, filterByDate, summaryByCategory, summaryByMonth, filterByCategory);
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                {
                    MessageBox.Show($"Date Filter Error: {ex.Message}", "DateTime Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Unknown Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                SummaryByCategoryCheckBox.IsChecked = false;
            }
        }
        #endregion
    }
}
