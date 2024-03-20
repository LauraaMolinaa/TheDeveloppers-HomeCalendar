using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Calendar
{
    // ====================================================================
    // CLASS: Category
    //        - An individual category for Calendar program
    //        - Valid category types: Event,AllDayEvent,Holiday
    // ====================================================================
    public class Category
    {
        // ====================================================================
        // Properties
        // ====================================================================

        /// <summary>
        /// Gets/sets the specific number that will represent the category. 
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets/sets details about a specific category. 
        /// </summary>
        public String Description { get; }

        /// <summary>
        /// Gets/sets the kind of category the event (such as Holiday, AllDayEvent, Event).
        /// </summary>
        public CategoryType Type { get; }

        /// <summary>
        /// A set of strings that represent the groups of categories. 
        /// </summary>
        /// <returns>
        /// Three different kinds of categories: event, all day event and holiday.
        /// </returns>
        public enum CategoryType
        {
            Event,
            AllDayEvent,
            Holiday,
            Availability,
        };

        // ====================================================================
        // Constructor
        // ====================================================================
        /// <summary>
        /// Initializes three fields of this category according to what was passed as parameters.
        /// </summary>
        /// <param name="id"> Unique integer number that represents a category. </param>
        /// <param name="description"> A text (string) giving details about a category in particular. </param>
        /// <param name="type"> An unique name that represents what kind of category it is. </param>
        public Category(int id, String description, CategoryType type = CategoryType.Event)
        {
            this.Id = id;
            this.Description = description;
            this.Type = type;
        }

        // ====================================================================
        // Copy Constructor
        // ====================================================================
        /// <summary>
        /// Initializes three fields according to the values of the Category object that was passed.
        /// </summary>
        /// <param name="category"> An object that specifies an unique number to represent a category, a name and details. </param>
        public Category(Category category)
        {
            this.Id = category.Id; ;
            this.Description = category.Description;
            this.Type = category.Type;
        }

        // ====================================================================
        // String version of object
        // ====================================================================
        /// <summary>
        /// Customizes the way this object will be represented as a string format.  
        /// </summary>
        /// <returns> A sentence that displays details about the current object. </returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Category vacation = new Category(1, "Traveling to Europe", Category.CategoryType.Holiday);
        /// Console.Write(vacation); 
        /// ]]>
        /// </code>
        /// </example>
        public override string ToString()
        {
            return Description;
        }

    }
}    

