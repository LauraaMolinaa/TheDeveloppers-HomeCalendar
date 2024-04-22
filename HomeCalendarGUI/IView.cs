using Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeCalendarGUI
{
    public interface IView
    {
        /// <summary>
        /// Displays all categories currently in the database on the drop-down button
        /// </summary>
        /// <param name="categories">A list of current categories</param>
        public void ShowCategoriesOnComboBox(List<Category> categories);

        /// <summary>
        /// Displays a message letting the user know the operation was successful.
        /// </summary>
        public void DisplayErrorMessage(string msg);

        /// <summary>
        /// Displays a message letting the user know the operation failed.
        /// </summary>
        public void DisplaySuccessfulMessage(string msg);

        /// <summary>
        /// Shows the neccessary information on a combobox for example, types of events. 
        /// </summary>
        public void ShowInformationOnCmb(List<Category> categories);
    }
}
