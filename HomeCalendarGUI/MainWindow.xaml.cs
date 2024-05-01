﻿using Calendar;
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


namespace HomeCalendarGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window, IView
    {
        private readonly Presenter presenter;

        private OpenFolderDialog openFolderDialog;
        private string fileDirectoryToStore;

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

            presenter.GetCategoriesForComboBox();
            presenter.GetCategoriesTypeInList();
            SetTodaysDateOnDatePicker();
        }

        public void SetTodaysDateOnDatePicker()
        {
            StartDate.DisplayDateStart = DateTime.Now;
            EndDate.DisplayDateStart = DateTime.Now;
        }

        public void DisplayErrorMessage(string msg)
        {
            message.Foreground = Brushes.Red;
            message.Text = msg;
        }

        public void DisplaySuccessfulMessage(string msg)
        {
            message.Foreground = Brushes.Green;
            message.Text = msg;
        }

        public void ShowCategoriesOnComboBox(List<Category> categories)
        {
            catsComboBox.Items.Clear();
            // Sort categories alphabetically by their Description
            var sortedCategories = categories.OrderBy(c => c.Description).ToList();

            const int DEFAULT = 0;
            sortedCategories.ForEach(c => {
                catsComboBox.Items.Add(c);
            });
            catsComboBox.SelectedIndex = DEFAULT;
        }

        public void ShowInformationOnCmb(List<Category> categories)
        {
            foreach (var category in categories)
            {
                if (cmbEventTypes.Items.Contains(category.Type))
                {
                    //ignoring event types that have already been added because we do not want duplicates
                    continue;
                }
                cmbEventTypes.Items.Add(category.Type);
            }
        }

        private void Btn_SaveCalendarFileTo(object sender, RoutedEventArgs e)
        {
            if (openFolderDialog.ShowDialog() == true)
            {
                fileDirectoryToStore = openFolderDialog.FolderName;
                openFolderDialog.InitialDirectory = fileDirectoryToStore;
                openFolderDialog.FolderName = "";                                             
            }
        }

        private void RefreshMainView()
        {
            presenter.GetCategoriesForComboBox();
        }

        private void Button_ClickAddCategory(object sender, RoutedEventArgs e)
        {
            var eventTypeChoice = cmbEventTypes.SelectedItem;
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
                cmbEventTypes.SelectedIndex = -1;
                RefreshMainView();
            }
            else
            {
                DisplayErrorMessage("You cannot leave any fields empty.");
            }
        }

        private void Button_ClickAddEvent(object sender, RoutedEventArgs e)
        {
            //verifying input data for events 
            if (StartDate.SelectedDate == null)
            {
                DisplayErrorMessage("Please select a start date for the event.");
                return;
            }
            if (EndDate.SelectedDate == null)
            {
                DisplayErrorMessage("Please select an end date for the event.");
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
            if (EndTime.Text == null)
            {
                DisplayErrorMessage("Please choose a duration in minutes for your event.");
                return;
            }

            DateTime startDate = StartDate.SelectedDate.Value;
            DateTime endDate = EndDate.SelectedDate.Value;

            if (endDate < startDate)
            {
                DisplayErrorMessage("The end date cannot be before the start date.");
                return;
            }

            //getting duration in minutes
            if (!double.TryParse(EndTime.Text, out double endTimeInMinutes))
            {
                DisplayErrorMessage("Please enter a number for your duration in minutes.");
                return;
            }

            if (endTimeInMinutes <= 0)
            {
                DisplayErrorMessage("The duration of your event must not be negative.");
                return;
            }

            Category selectedCategory = (Category)catsComboBox.SelectedItem;
            string description = EventDescriptionBox.Text;

            presenter.AddNewEvent(startDate, endDate, selectedCategory.Id, description, endTimeInMinutes);

            // Clear the input fields
            StartDate.SelectedDate = null;
            EndDate.SelectedDate = null;
            catsComboBox.SelectedIndex = -1;
            EventDescriptionBox.Clear();

            DisplaySuccessfulMessage("Event added successfully.");
        }

        private void Button_ClickCancelEvent(object sender, RoutedEventArgs e)
        {
            EventDescriptionBox.Clear();
        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public bool IsValidDescription(string desc)
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

        public void ShowCalendarItemsFilteredByMonth(List<string> months, List<Double> totalBusyTimes)
        {
            //Console.WriteLine();
            //Events.ItemsSource = months;
                 
        }

        private void BtnClick_Sort(object sender, RoutedEventArgs e)
        {
            DateTime start = StartMonth.SelectedDate.Value;
            DateTime end = EndMonth.SelectedDate.Value; 

            presenter.GetCalendarItemsFilteredByMonth(start, end); 
        }

    }
}