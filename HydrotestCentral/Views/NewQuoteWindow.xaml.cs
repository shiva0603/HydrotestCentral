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
using System.Data;
using System.Data.SQLite;

namespace HydrotestCentral
{
    /// <summary>
    /// Interaction logic for NewQuoteWindow.xaml
    /// </summary>
    public partial class NewQuoteWindow : Window
    {
        public SQLiteConnection connection;
        public SQLiteDataAdapter dataAdapter;

        public NewQuoteWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_jobno.Text = getNextJobNumber(getLastJobNumber());
            txt_qtdate.Text = DateTime.Now.ToString("MM/dd/yyyy");
            txt_status.Text = "QUOTE";
        }

        public string getLastJobNumber()
        {
            connection = new SQLiteConnection("DataSource=C:\\CentralDB.db");
            connection.Open();
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = string.Format("SELECT jobno FROM QTE_HDR ORDER BY jobno DESC LIMIT 1");
            string returnString = cmd.ExecuteScalar().ToString();
            connection.Close();

            return returnString;
        }

        public string getNextJobNumber(string lastJobNo)
        {
            char[] remChars = { 'A', 'T', 'H', 'S', '-' };
            string returnString = lastJobNo.TrimStart(remChars);
            Console.WriteLine("removed chars from lastJobNo: " + lastJobNo);
            int num = 0;
            if(Int32.TryParse(returnString, out num))
            {
                num += 1;
                returnString = "ATHS-" + num.ToString();
            }
            else
            {
                MessageBox.Show("Error Getting Job Number!");
            }
            Console.WriteLine("new job no created: " + returnString);

            return returnString;
        }

        private void Btn_AddQuote_Click(object sender, RoutedEventArgs e)
        {
            /*
            connection = new SQLiteConnection("DataSource=C:\\CentralDB.db");
            connection.Open();
            SQLiteCommand cmd1 = connection.CreateCommand();
            cmd1.CommandText = string.Format("SELECT * FROM QTE_HDR");
            */
            MessageBox.Show("Last Job Number is " + getLastJobNumber());
        }

        private void Btn_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
