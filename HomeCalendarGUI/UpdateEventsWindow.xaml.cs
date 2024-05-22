﻿using Calendar;
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
using Xceed.Wpf.Toolkit;

namespace HomeCalendarGUI
{
    /// <summary>
    /// Interaction logic for UpdateEventsWindow.xaml
    /// </summary>
    public partial class UpdateEventsWindow : Window, IView
    {
        private Presenter presenter;

        public UpdateEventsWindow()
        {
            presenter = new Presenter(this);
            InitializeComponent();
            
            presenter.GetCategoriesForAllCatsComboBoxes();
            CreateTimePicker();
            StartDate.DisplayDate = DateTime.Now;
        }

        /// <summary>
        /// Initializes the timepicker and adds it to the window
        /// </summary>
        public void CreateTimePicker()
        {
            TimePicker startTimePicker = new TimePicker();
            startTimePicker.AllowTextInput = false;
            startTimePicker.Name = "startTime";
            startTimePicker.Margin = new Thickness(0, 5, 0, 0);

            StartTimeGrid.Children.Add(startTimePicker);

            TimePicker EndTimePicker = new TimePicker();
            EndTimePicker.AllowTextInput = false;
            EndTimePicker.Name = "endTime";
            EndTimePicker.Margin = new Thickness(0, 5, 0, 0);

            EndTimeGrid.Children.Add(EndTimePicker);
        }

        public void DisplayErrorMessage(string msg)
        {
            throw new NotImplementedException();
        }

        public void DisplaySuccessfulMessage(string msg)
        {
            throw new NotImplementedException();
        }

        public void PopulateAllCategoriesComboBox(List<Category> categories)
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

        public void PopulateCategoryTypesComboBox(List<Category> categories)
        {
            throw new NotImplementedException();
        }

        public void ShowCalendarItems(List<CalendarItem> items)
        {
            throw new NotImplementedException();
        }

        public void ShowTotalBusyTimeByCategory(List<CalendarItemsByCategory> itemsByCategory)
        {
            throw new NotImplementedException();
        }

        public void ShowTotalBusyTimeByMonth(List<CalendarItemsByMonth> itemsByMonth)
        {
            throw new NotImplementedException();
        }

        public void ShowTotalBusyTimeByMonthAndCategory(List<Dictionary<string, object>> itemsByCategoryAndMonth)
        {
            throw new NotImplementedException();
        }

        private void Btn_UpdateEvent(object sender, RoutedEventArgs e)
        {
            //duration of the event
            
        }
    }
}
