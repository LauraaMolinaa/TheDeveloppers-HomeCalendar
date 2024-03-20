using Calendar;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
namespace Calendar
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Events e1 = new Events();

            //conecting to db 
            Database.newDatabase("C:\\Users\\rania\\Downloads\\Sprint2-Milestone\\Milestone3_tests\\testDBInput.db");
        }
    }
}