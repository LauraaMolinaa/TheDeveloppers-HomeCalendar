using Calendar;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
namespace HomeCalendar
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Events e1 = new Events();

            //conecting to db 
            Database.newDatabase("C:\\Users\\2232607\\Documents\\Sprint2_Homecalendar\\Milestone3_tests\\testDBInput.db");
        }
    }
}