using HydrotestCentral.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Collections.ObjectModel;
using HydrotestCentral.Model;
using System.Data;
using System.Windows;

namespace HydrotestCentral.ViewModels
{
    public partial class MainWindowViewModel: INotifyPropertyChanged
    {
        static string connectionString = Properties.Settings.Default.connString;
        SQLiteConnection connection;
        SQLiteCommand cmd;
        SQLiteDataAdapter adapter;
        DataSet ds;

        public ObservableCollection<QuoteHeader> quote_headers { get; set; }
        public ObservableCollection<QuoteItem> quote_items { get; set; }
        public ObservableCollection<InventoryItem> inventory_items { get; set; }


        public MainWindowViewModel()
        {
            InitializeComponent();

            quote_headers = new ObservableCollection<QuoteHeader>();
            quote_headers = LoadQuoteHeaderData();
            quote_items = new ObservableCollection<QuoteItem>();
            quote_items = LoadQuoteItemData();
        }

        public ObservableCollection<QuoteHeader> LoadQuoteHeaderData()
        {
            var headers = new ObservableCollection<QuoteHeader>();

            try
            {
                connection = new SQLiteConnection(@"DataSource=C:\\Users\\SFWMD\\Aqua-Tech Hydro Services\\IT - Documents\\7.8 Databases\\CentralDB.db");
                connection.Open();
                cmd = connection.CreateCommand();
                cmd.CommandText = string.Format("SELECT * FROM QTE_HDR");
                adapter = new SQLiteDataAdapter(cmd);

                ds = new DataSet();

                adapter.Fill(ds, "QTE_HDR");


                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int cleaned_days = 0;
                    double cleaned_value = 0.00;

                    if (Int32.TryParse(dr[8].ToString(), out cleaned_days)) { }

                    if (Double.TryParse(dr[17].ToString(), out cleaned_value)) { }

                    headers.Add(new QuoteHeader
                    {
                            quoteno = dr[0].ToString(),
                            jobno = dr[0].ToString(),
                            qt_date = dr[1].ToString(),
                            cust = dr[2].ToString(),
                            cust_contact = dr[3].ToString(),
                            cust_phone = dr[4].ToString(),
                            cust_email = dr[5].ToString(),
                            loc = dr[6].ToString(),
                            salesman = dr[7].ToString(),
                            days_est = cleaned_days,
                            status = dr[9].ToString(),
                            pipe_line_size = dr[10].ToString(),
                            pipe_length = dr[11].ToString(),
                            pressure = dr[12].ToString(),
                            endclient = dr[13].ToString(),
                            supervisor = dr[14].ToString(),
                            est_startdate = dr[15].ToString(),
                            est_enddate = dr[16].ToString(),
                            value = cleaned_value
                    });
                     Console.WriteLine(dr[0].ToString() + " created in quote_headers");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                ds = null;
                adapter.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return headers;
        }

        public ObservableCollection<QuoteItem> LoadQuoteItemData()
        {
            var items = new ObservableCollection<QuoteItem>();

            try
            {
                connection = new SQLiteConnection(connectionString);
                connection.Open();
                cmd = connection.CreateCommand();
                cmd.CommandText = string.Format("SELECT * FROM QTE_ITEMS");
                adapter = new SQLiteDataAdapter(cmd);

                ds = new DataSet();

                adapter.Fill(ds, "QTE_ITEM");


                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int cleaned_qty = 0;
                        double cleaned_rate = 0.00;
                        int cleaned_group = 1;
                        bool cleaned_taxable = false;
                        bool cleaned_discountable = false;
                        bool cleaned_printable = false;
                        double cleaned_line_total = 0.00;
                        double cleaned_tax_total = 0.00;
                        int cleaned_tab_index = 0;
                        int cleaned_row_index = 0;

                        if (Int32.TryParse(dr[0].ToString(), out cleaned_qty)) { }
                        if (Double.TryParse(dr[2].ToString(), out cleaned_rate)) { }
                        if (Int32.TryParse(dr[4].ToString(), out cleaned_group)) { }
                        if (Boolean.TryParse(dr[5].ToString(), out cleaned_taxable)) { }
                        if (Boolean.TryParse(dr[6].ToString(), out cleaned_discountable)) { }
                        if (Boolean.TryParse(dr[7].ToString(), out cleaned_printable)) { }
                        if (Double.TryParse(dr[9].ToString(), out cleaned_line_total)) { }
                        if (Double.TryParse(dr[10].ToString(), out cleaned_tax_total)) { }
                        if (Int32.TryParse(dr[11].ToString(), out cleaned_tab_index)) { }
                        if (Int32.TryParse(dr[12].ToString(), out cleaned_row_index)) { }

                        items.Add(new QuoteItem
                        {
                            qty = cleaned_qty,
                            item = dr[1].ToString(),
                            rate = cleaned_rate,
                            descr = dr[3].ToString(),
                            group = cleaned_group,
                            taxable = cleaned_taxable,
                            discountable = cleaned_discountable,
                            printable = cleaned_printable,
                            jobno = dr[8].ToString(),
                            line_total = cleaned_line_total,
                            tax_total = cleaned_tax_total,
                            tab_index = cleaned_tab_index,
                            row_index = cleaned_row_index
                        });
                        //Console.WriteLine(dr[1].ToString() + " created in quote_items");
                    }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                ds = null;
                adapter.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return items;
        }

        public ObservableCollection<QuoteItem> getQuoteItems()
        {
            return quote_items;
        }

        public void updateQuoteItemsByJob(string jobno)
        {
            //MessageBox.Show("updateQuoteItemsByJob called...");
            var start_collection = new ObservableCollection<QuoteItem>();
            var new_collection = new ObservableCollection<QuoteItem>();
            start_collection = LoadQuoteItemData();

            IEnumerable<QuoteItem> items = start_collection.Where(c => c.jobno == jobno);
            Console.WriteLine("Adding New Collection for Quote Items updated to only show Job: " + jobno);
            foreach (QuoteItem i in items)
            {
                new_collection.Add(i);
                Console.WriteLine("--->" + i.jobno + " | " + i.item);
            }

            quote_items = new_collection;
        }

        public void updateQuoteItemsByJob_And_Tab(string jobno, int tab_index)
        {
            var start_collection = new ObservableCollection<QuoteItem>();
            var new_collection = new ObservableCollection<QuoteItem>();
            start_collection = LoadQuoteItemData();

            IEnumerable<QuoteItem> items = start_collection.Where(c => c.jobno == jobno && c.tab_index == tab_index);

            foreach (QuoteItem i in items)
            {
                new_collection.Add(i);
            }

            quote_items = new_collection;
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
