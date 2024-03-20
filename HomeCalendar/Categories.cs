using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Data.Common;
using System.Data.SQLite;

using static Calendar.Category;
using System.Net.Http.Headers;
using System.Configuration;
using System.Security.Cryptography;
using System.Globalization;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Calendar
{
    // ====================================================================
    // CLASS: categories
    //        - A collection of category items,
    //        - Read / write to file
    //        - etc
    // ====================================================================
    /// <summary>
    /// A division of different activies which have a name, an unique number and details about it.
    /// </summary>
    public class Categories
    {
        private SQLiteConnection _dbConnection;

        // ====================================================================
        // Properties
        // ====================================================================
        public SQLiteConnection Connection { get { return _dbConnection; } }

        // ====================================================================
        // Constructor
        // ====================================================================
        public Categories(SQLiteConnection dbConnection, bool newDB)
        {
            this._dbConnection = dbConnection;
            if (newDB)
            {
                PopulateCategoriesTypeTable();
                SetCategoriesToDefaults();
            }
        }
      
        public CategoryType GetCategoryTypeFromTypeId(int typeId)
        {
            if (typeId == 1)
            {
                return CategoryType.Event;
            }
            if (typeId == 2)
            {
                return CategoryType.Availability;
            }
            if (typeId == 3)
            {
                return CategoryType.AllDayEvent;
            }

            return CategoryType.Holiday;
        }

        // ====================================================================
        // set categories to default
        // ====================================================================
        /// <summary>
        /// Resets the list of group of activities to a default when providing a new database. 
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Categories c = new Categories(dbConnection, true);
        /// ]]>
        /// </code>
        /// </example>
        public void SetCategoriesToDefaults()
        {
            // ---------------------------------------------------------------
            // Add Defaults
            // ---------------------------------------------------------------
            string query = "DELETE FROM categories";
            SQLiteCommand cmd = new SQLiteCommand(query, this._dbConnection);
            cmd.ExecuteNonQuery();

            Add("School", Category.CategoryType.Event);
            Add("Personal", Category.CategoryType.Event);
            Add("VideoGames", Category.CategoryType.Event);
            Add("Medical", Category.CategoryType.Event);
            Add("Sleep", Category.CategoryType.Event);
            Add("Vacation", Category.CategoryType.AllDayEvent);
            Add("Travel days", Category.CategoryType.AllDayEvent);
            Add("Canadian Holidays", Category.CategoryType.Holiday);
            Add("US Holidays", Category.CategoryType.Holiday);
        }

        /// <summary>
        /// Adds a new group of activities to this database. 
        /// </summary>
        /// <param name="desc"> A text that specifies details about a category. </param>
        /// <param name="type"> An unique name that represents what kind of category it is. </param>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Categories c1 = new Categories();
        /// c1.Add("Pottery class", Category.CategoryType.Event);
        /// ]]>
        /// </code>
        /// </example>
        public void Add(String desc, Category.CategoryType type)
        {
            var pragmaOff = new SQLiteCommand("PRAGMA foreign_keys=OFF", this._dbConnection);
            pragmaOff.ExecuteNonQuery();
            // use the database to insert the Category into the database
            string query = "INSERT INTO categories(Description, TypeId) VALUES(@Description, @TypeId)";
            using var cmd = new SQLiteCommand(query, _dbConnection);
            cmd.Parameters.AddWithValue("@Description", desc);
            int x = ((int)type);
            cmd.Parameters.AddWithValue("@TypeId", x);
            cmd.ExecuteNonQuery();

        }

        /// <summary>
        /// Populates the category types to a default. 
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Categories c1 = new Categories();
        /// c1.PopulateCategoriestable();
        /// ]]>
        /// </code>
        /// </example>
        public void PopulateCategoriesTypeTable()
        {
            SQLiteCommand cmd = new SQLiteCommand(this._dbConnection);
            cmd.CommandText = "INSERT INTO categoryTypes(Description) VALUES(@Description);";
            cmd.Parameters.AddWithValue("@Description", Category.CategoryType.Event.ToString());
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO categoryTypes(Description) VALUES(@Description);";
            cmd.Parameters.AddWithValue("@Description", Category.CategoryType.Availability.ToString());
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO categoryTypes(Description) VALUES(@Description);";
            cmd.Parameters.AddWithValue("@Description", Category.CategoryType.AllDayEvent.ToString());
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO categoryTypes(Description) VALUES(@Description);";
            cmd.Parameters.AddWithValue("@Description", Category.CategoryType.Holiday.ToString());
            cmd.Prepare();
            cmd.ExecuteNonQuery();

        }

        /// <summary>
        /// This method allows to change the one of the activities. 
        /// </summary>
        /// <param name="id"> the unique identifier for an activity. </param>
        /// <param name="categoryId"> the identifier of the activitie's categoryId. </param>
        /// <param name="startDateTime"> the beginning of the activity. </param>
        /// <param name="duration"> The time this activity will take. </param>
        /// <param name="details"> The description of the activity. </param>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Categories c1e = new Categories(dbConnection, true);
        /// c1.UpdateEvent(1, 3, "2020-01-01", 57.0, "updated details");
        /// ]]>
        /// </code>
        /// </example>
        public void UpdateCategory(int id, string newDescription, Category.CategoryType newType)
        {
            SQLiteCommand cmd = new SQLiteCommand(this._dbConnection);
            cmd.CommandText = "UPDATE categories SET Description = @newDescription, TypeId = @newType WHERE Id = @Id";
            cmd.Parameters.AddWithValue("@newDescription", newDescription);
            int x = ((int)newType);
            cmd.Parameters.AddWithValue("@newType", x);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        // ====================================================================
        // Delete category
        // ====================================================================
        /// <summary>
        /// Removes the activity from the database.
        /// </summary>
        /// <param name="Id"> An unique number that represents an type of activity. </param>
        /// <exception cref="Exception"> Throw an exception if the Id could not be found in the database. </exception>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Categories c1 = new Categories(dbConnection, true);
        /// c1.Delete(1);
        /// ]]>
        /// </code>
        /// </example>
        public void Delete(int Id)
        {
            // delete from database
            try
            {
                var pragmaOff = new SQLiteCommand("PRAGMA foreign_keys=OFF", this._dbConnection);
                pragmaOff.ExecuteNonQuery();

                using var cmd = new SQLiteCommand(this._dbConnection);
                cmd.CommandText = "DELETE FROM categories WHERE Id = @Id";
                cmd.Parameters.AddWithValue("@Id", Id);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }

            catch (Exception e)
            {
                throw new Exception("An error occurred when deleting the category from the database.", e);
            }



        }

        // ====================================================================
        // Return list of categories
        // Note:  make new copy of list, so user cannot modify what is part of
        //        this instance
        // ====================================================================
        /// <summary>
        /// This method retrieves all types of activities in our database and puts them inside a list. 
        /// </summary>
        /// <returns> A list containing all types of activities. </returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Categories c1 = new Categories(dbConnection, true);
        /// List<Category> categoriesList = c1.List();
        /// ]]>
        /// </code>
        /// </example>
        public List<Category> List()
        {
            List<Category> categoriesList = new List<Category>();

            string query = "SELECT * FROM categories ";
            SQLiteCommand cmd = new SQLiteCommand(query, this._dbConnection);
            using SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string description = reader.GetString(1);
                int typeId = reader.GetInt32(2);
                Category.CategoryType type = GetCategoryTypeFromTypeId(typeId);

                categoriesList.Add(new Category(id, description, type));
            }


            return categoriesList;

            // query database and return results



        }



    }
}
