﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Data.SQLite;
using System.Globalization;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Calendar
{
    // ====================================================================
    // CLASS: Events
    //        - A collection of Event items,
    //        - Read / write to file
    //        - etc
    // ====================================================================
    /// <summary>
    /// Multiple activities that are retrieved, created, updated and deleted from a database
    /// </summary>
    public class Events
    {
        private List<Event> _Events = new List<Event>();

        private SQLiteConnection _dbConnection;
        // ====================================================================
        // Properties
        // ====================================================================
        public SQLiteConnection Connection { get { return _dbConnection; } }

        // ====================================================================
        // populate categories from a file
        // if filepath is not specified, read/save in AppData file
        // Throws System.IO.FileNotFoundException if file does not exist
        // Throws System.Exception if cannot read the file correctly (parsing XML)
        // ====================================================================

        public Events()
        {

        }
        public Events(SQLiteConnection dbConnection)
        {
            this._dbConnection = dbConnection;
        }


        /// <summary>
        /// Adds a new activity (new 'Event' object) to the database. 
        /// </summary>
        /// <param name="date"> The beginnin of the event. </param>
        /// <param name="categoryId"> A name that is assocaited with a type of activity </param>
        /// <param name="duration"> The time that has been reserved for this event. </param>
        /// <param name="details"> A description of the activity. </param>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Events e = new Events(dbConnection);
        /// e.Add("2018-01-01", 2, 45.5, "new Event");
        /// ]]>
        /// </code>
        /// </example>
        public void Add(string date, int categoryId, double duration, string details)
        {
            var pragmaOff = new SQLiteCommand("PRAGMA foreign_keys=OFF", this._dbConnection);
            pragmaOff.ExecuteNonQuery();

            string query = "INSERT INTO events(CategoryId, StartDateTime, DurationInMinutes, Details) VALUES(@CategoryId, @StartDateTime, @DurationInMinutes, @Details)";
            using var cmd = new SQLiteCommand(query, _dbConnection);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            cmd.Parameters.AddWithValue("@StartDateTime", date);
            cmd.Parameters.AddWithValue("@DurationInMinutes", duration);
            cmd.Parameters.AddWithValue("@Details", details);
            cmd.ExecuteNonQuery();
        }

        // ====================================================================
        // Delete Event
        // ====================================================================
        /// <summary>
        /// Removes the activity from the database.
        /// </summary>
        /// <param name="eventId"> An unique number that represents an activity. </param>
        /// <exception cref="Exception"> Throw an exception if the Id could not be found in the database. </exception>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Events e = new Events(dbConnection);
        /// e.Delete(1);
        /// ]]>
        /// </code>
        /// </example>
        public void Delete(int eventId)
        {
            try
            {
                var pragmaOff = new SQLiteCommand("PRAGMA foreign_keys=OFF", this._dbConnection);
                pragmaOff.ExecuteNonQuery();

                using var cmd = new SQLiteCommand(this._dbConnection);
                cmd.CommandText = "DELETE FROM events WHERE Id = @eventId;";
                cmd.Parameters.AddWithValue("@eventId", eventId);
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    Console.WriteLine("No event found with the provided ID.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while deleting an event from the database", e);
            }
        }

        /// <summary>
        /// This method allows to change the one of the activities. 
        /// </summary>
        /// <param name="id"> the unique identifier for an activity. </param>
        /// <param name="categoryId"> the identifier of the activitie's categoryId. </param>
        /// <param name="startDateTime"> the beginning of the activity. </param>
        /// <param name="duration"> The time this activity will take. </param>
        /// <param name="details"> The description of the activity. </param>
        /// <exception cref="Exception"> Throw exception if there was an issue with updating the event. </exception>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Event e = new Event(dbConnection);
        /// e.UpdateEvent(1, 3, "2020-01-01", 57.0, "updated details");
        /// ]]>
        /// </code>
        /// </example>
        public void UpdateEvent(int id, int categoryId, DateTime startDateTime, double duration, String details)
        {
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(this._dbConnection);
                cmd.CommandText = "UPDATE events SET CategoryId = @categoryId, StartDateTime = @startDateTime, DurationInMinutes = @duration, Details = @details WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@CategoryId", categoryId);
                cmd.Parameters.AddWithValue("@StartDateTime", startDateTime);
                cmd.Parameters.AddWithValue("@DurationInMinutes", duration);
                cmd.Parameters.AddWithValue("@Details", details);
                cmd.Parameters.AddWithValue("@EventId", id);

                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while updating an event from the database.", e);
            }          
        }

        /// <summary>
        /// This method retrieves all activities in our database and puts them inside a list. 
        /// </summary>
        /// <returns> A list containing all activities. </returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Event e = new Event(dbConnection);
        /// List<Event> eventsList = e.List();
        /// ]]>
        /// </code>
        /// </example>
        public List<Event> List()
        {
            List<Event> eventsList = new List<Event>();

            string query = "SELECT Id, StartDateTime, DurationInMinutes, Details, CategoryId FROM events ORDER BY Id";
            SQLiteCommand cmd = new SQLiteCommand(query, this._dbConnection);
            using SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                int categoryId = reader.GetInt32(1);
                string startDateTime = reader.GetString(2);
                float durationInMinutes = reader.GetFloat(3);
                string details = reader.GetString(4);
                //DateTime startDate = DateTime.ParseExact(startDateTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                //Console.WriteLine($"{id}, {description}, {typeId}");
                // should be added to the list, eventsList.Add
                Add(startDateTime, categoryId, durationInMinutes, details); 
            }


            return eventsList;
        }


        

        

    }
}

