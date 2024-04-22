using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calendar;
using static System.Net.WebRequestMethods;
﻿using Calendar;
using System.Data.Entity.Migrations.Model;
using System.Printing;
using TeamHeavyWeight_HomeCalendarApp;
using static Calendar.Category;

namespace HomeCalendarGUI
{
    public class Presenter
    {
        private readonly HomeCalendar model;
        private readonly IView view;
        

        /// <summary>
        /// Initiates presenter with default settings
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
        public Presenter(IView v,string dbFile)
        {
            model = new HomeCalendar(dbFile,false);
            view = v;
        }

        /// <summary>
        /// Gets all categories listed in the database
        /// </summary>
        public void GetCategoriesForComboBox()
        {
            List<Category> categories = model.categories.List();
            view.ShowCategoriesOnComboBox(categories);
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
                model.categories.Add(desc, type);
                view.DisplaySuccessfulMessage("Category has been successfully added!");
            }
            
        }

        public void AddNewEvent(DateTime startDate, DateTime endDate, int categoryId, string description)
        {
            // Calculate the duration of the event
            double duration = (endDate - startDate).TotalMinutes;

            if (duration <= 0)
            {
                view.DisplayErrorMessage("End date must be later than start date.");
                return;
            }

            // Here we call the Add method of the Events class from your model
            model.events.Add(startDate, categoryId, duration, description);

            // You might want to call a method to update the UI or a list of events here as well
            view.DisplaySuccessfulMessage("Event added successfully.");
        }

        /// <summary>
        /// Gets the all the types of activity to display it. 
        /// </summary>
        public void GetCategoriesTypeInList() 
        {
            view.ShowInformationOnCmb(model.categories.List());
        }


    }
}
