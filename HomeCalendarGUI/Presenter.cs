
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calendar;
using static System.Net.WebRequestMethods;
using System.Data.Entity.Migrations.Model;
using System.Printing;
using TeamHeavyWeight_HomeCalendarApp;
using static Calendar.Category;
using System.Collections;
using System.Data;
using System.Windows;
using System.ComponentModel.DataAnnotations;

namespace HomeCalendarGUI
{
    /// <summary>
    /// Presenter of the HomeCalendar App
    /// </summary>
    public class Presenter
    {
        private readonly HomeCalendar model;
        private readonly IView view;       

        /// <summary>
        /// Instantiates presenter with default settings and default database
        /// </summary>
        /// <param name="v">IView interface implemented class</param>
        public Presenter(IView v)
        {
            model = new HomeCalendar();
            view = v;
        }

        /// <summary>
        /// Instantiates presenter with an existing database
        /// </summary>
        /// <param name="v">IView interface implemented class</param>
        /// <param name="dbFile">File path to the database</param>
        public Presenter(IView v, string dbFile)
        {
            view = v;
            try
            {
                model = new HomeCalendar(dbFile, false);
            }
            catch (FileNotFoundException ex)
            {
                if (ex is FileNotFoundException)
                {
                    // Notify the view to display an error message
                    view.DisplayErrorMessage("Database file could not be loaded. Please check the database path.");
                    model = null;
                }
                else
                {
                    // Handle other unexpected exceptions
                    view.DisplayErrorMessage("An unexpected error occurred during initialization.");
                    model = null;
                }
            }
        }

        /// <summary>
        /// Adds a new category to the database with the details and the type the user provided.
        /// </summary>
        /// <param name="desc"> A string that holds the details about this category. </param>
        /// <param name="type"> An enum that contains the kind of activity. </param>
        public void AddNewCategory(string desc, CategoryType type)
        {
            if (desc == null || type == null)
            {
                view.DisplayErrorMessage("You can not leave any empty boxes.");
            }
            else
            {
                try
                {
                    model.categories.Add(desc, type);
                    view.DisplaySuccessfulMessage("Category has been successfully added!");
                }
                catch
                {
                    view.DisplayErrorMessage("There was an error. Please try again.");
                }                
            }     
        }

        /// <summary>
        /// Adds a new event to database
        /// </summary>
        /// <param name="startDate">Start date of the event</param>        
        /// <param name="categoryId">Category Id of the event</param>
        /// <param name="description">Description of the event</param>
        /// <param name="duration">Duration of the event</param>
        public void AddNewEvent(DateTime startDate, int categoryId, string description, double duration)
        {
            try
            {
                // Here we call the Add method of the Events class from your model
                model.events.Add(startDate, categoryId, duration, description);
                
                // You might want to call a method to update the UI or a list of events here as well
                view.DisplaySuccessfulMessage("Event added successfully.");
            }
            catch
            {
                view.DisplayErrorMessage("There was an error. Please try again.");
            }
        }

        /// <summary>
        /// Gets all category types 
        /// </summary>
        public void GetCategoriesTypeInList()
        {
            view.PopulateCategoryTypesComboBox(model.categories.List());
        }

        /// <summary>
        /// Gets all categories listed in the database
        /// </summary>
        public void GetCategoriesForAllCatsComboBoxes()
        {
            List<Category> categories = model.categories.List();
            view.PopulateCategoriesInAllCatsComboBox(categories);
        }

        /// <summary>
        /// Gets calendar items with corresponding filters
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="categoryId">Category Id</param>
        /// <param name="dateFilter">If true, it filters by specified date</param>
        /// <param name="summaryByCategory">If true, it filters by category</param>
        /// <param name="summaryByMonth">If true,filters by month</param>        
        public void GetHomeCalendarItems(DateTime? startDate, DateTime? endDate, int categoryId, bool dateFilter ,bool summaryByCategory, bool summaryByMonth,bool filterDataByCategory)
        {
            //if dateFilter is set to true, throw if the start and end dates values are null
            // or when the end date is before the start date.
            if (dateFilter)
            {
                if(startDate == null || endDate == null)
                {                   
                    throw new InvalidOperationException("Must provide a start and end date");
                }
                else if(endDate < startDate)
                {
                    throw new InvalidOperationException("End date cannot be set before the starting date");
                }
            }

            //If the user wants to filter by category and month, get dictionary while considering the date filter flag
            if (summaryByCategory && summaryByMonth)
            {
                List<Dictionary<string, object>> itemsByCategoryAndMonth;
                if (dateFilter)
                {
                    itemsByCategoryAndMonth = model.GetCalendarDictionaryByCategoryAndMonth(startDate, endDate, filterDataByCategory, categoryId);                    
                }
                else
                {
                    itemsByCategoryAndMonth = model.GetCalendarDictionaryByCategoryAndMonth(null, null, filterDataByCategory, categoryId);
                }
                view.ShowTotalBusyTimeByMonthAndCategory(itemsByCategoryAndMonth);
            }
            else if (summaryByMonth)
            {
                //If the user wants to filter by month, get a list of calendar items by month while considering the date filter flag
                List<CalendarItemsByMonth> itemsByMonth;
                if (dateFilter)
                {
                    itemsByMonth = model.GetCalendarItemsByMonth(startDate, endDate, filterDataByCategory, categoryId);                   
                }
                else
                {
                    itemsByMonth = model.GetCalendarItemsByMonth(null, null, filterDataByCategory, categoryId);                    
                }
                view.ShowTotalBusyTimeByMonth(itemsByMonth);
            }
            else if (summaryByCategory)
            {
                List<CalendarItemsByCategory> itemsByCategory;
                //If the user wants to filter by category, get a list of calendar items by category while considering the date filter flag
                if (dateFilter)
                {
                    itemsByCategory = model.GetCalendarItemsByCategory(startDate, endDate, filterDataByCategory, categoryId);                    
                }
                else
                {
                    itemsByCategory = model.GetCalendarItemsByCategory(null, null, filterDataByCategory, categoryId);                   
                }
                view.ShowTotalBusyTimeByCategory(itemsByCategory);
            }
            else
            {
                //If the user doesn't apply filters, get a list of calendar items while considering the date filter flag

                List<CalendarItem> items;
                if (dateFilter)
                {
                    items = model.GetCalendarItems(startDate, endDate, filterDataByCategory, categoryId);
                }
                else
                {
                    items = model.GetCalendarItems(null, null, filterDataByCategory, categoryId);
                }                
                
                view.ShowCalendarItems(items);              
            }
        }

        /// <summary>
        /// Deletes an event from the database
        /// </summary>
        public void DeleteAnEvent(int eventId)
        {
            model.events.DeleteEvent(eventId);
        }

        /// <summary>
        /// Updates an event on the database
        /// </summary>
        /// <param name="eventId">Event Id</param>
        /// <param name="startDate">Start Date of the event</param>
        /// <param name="duration">Duration of the event</param>
        /// <param name="desc">Description of the event</param>
        /// <param name="category">Category of the event</param>
        public void UpdateEvent(int eventId, DateTime? startDate, double? duration, string? desc, int? category)
        {
            try
            {
                model.events.Update(eventId, startDate, category, duration, desc);
                view.DisplaySuccessfulMessage("Event has been updated successfully!");
                GetHomeCalendarItems(null, null, 0, false, false, false, false);
            }
            catch (Exception ex) 
            {
                view.DisplayErrorMessage($"Something went wrong while updating: {ex}");
            }
        }
    }
}
