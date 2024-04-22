using Calendar;
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
            const int DEFAULT = 0;                        
            categories.ForEach(c => {
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
                presenter.AddNewCategory(desc, type);
            }
            else
            {
                DisplayErrorMessage("You cannot leave any fields empty.");
            }
        }

        private void Button_ClickAddEvent(object sender, RoutedEventArgs e)
        {

        }

        private void Button_ClickCancelEvent(object sender, RoutedEventArgs e)
        {
            EventDescriptionBox.Clear();
        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}