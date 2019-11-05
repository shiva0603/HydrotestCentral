using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.ComponentModel;
//using HydrotestCentral.DatasetTableAdapters;

namespace HydrotestCentral.Model
{
    // Data object classes
    public class QuoteHeader
    {
        public string quoteno { get; set; }
        public string jobno { get; set; }
        public string qt_date { get; set; }
        public string cust { get; set; }
        public string cust_contact { get; set; }
        public string cust_phone { get; set; }
        public string cust_email { get; set; }
        public string loc { get; set; }
        public string salesman { get; set; }
        public int days_est { get; set; }
        public string status { get; set; }
        public string jobtype { get; set; }
        public string pipe_line_size { get; set; }
        public string pipe_length { get; set; }
        public string pressure { get; set; }
        public string endclient { get; set; }
        public string supervisor { get; set; }
        public string est_startdate { get; set; }
        public string est_enddate { get; set; }
        public double value { get; set; }
    }

    public class QuoteItem
    {
        public int qty { get; set; }
        public string item { get; set; }
        public double rate { get; set; }
        public string descr { get; set; }
        public int group { get; set; }
        public bool taxable { get; set; }
        public bool discountable { get; set; }
        public bool printable { get; set; }
        public string jobno { get; set; }
        public string quoteno { get; set; }
        public double line_total { get; set; }
        public double tax_total { get; set; }
        public int tab_index { get; set; }
        public int row_index { get; set; }
    }

    public class InventoryItem
    {
        public string item { get; set; }
        public string descr { get; set; }
        public double rate { get; set; }
    }

    // A Source of Quote Header Data from SQLite Database
    public class QuoteHeaderDataProvider
    {
        private SQLiteDataAdapter quoteheader_adapter;
        private SQLiteConnection connection;

        private DataTable quoteheader_dt;

        public QuoteHeaderDataProvider()
        {
            quoteheader_dt = new System.Data.DataTable(); 
            connection = new SQLiteConnection("DataSource=C:\\SQLite\\CentralDB.db");
            connection.Open();
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = string.Format("SELECT * FROM QTE_HDR");
            quoteheader_adapter = new SQLiteDataAdapter(cmd);
            //quoteheader_adapter.Fill(quoteheader_dataset);
            quoteheader_adapter.Fill(quoteheader_dt);

            connection.Close();
        }

        public DataView getQuoteHeaders()
        {
            return quoteheader_dt.DefaultView;
        }

        public DataTable getQuoteHeaderTableByJob(string jobno)
        {
            DataTable specific_dt = new System.Data.DataTable();
            connection = new SQLiteConnection("DataSource=C:\\SQLite\\CentralDB.db");
            connection.Open();
            SQLiteCommand cmd = connection.CreateCommand();
            Console.WriteLine("Jobno: " + jobno);
            cmd.CommandText = string.Format("SELECT * FROM QTE_HDR WHERE jobno='{0}'", jobno);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
            adapter.Fill(specific_dt);

            Console.WriteLine("Datatable: " + specific_dt.ToString());

            connection.Close();

            return specific_dt;
        }
    }

    // Source of Quote Items Data From SQLite Database
    public class QuoteItemsDataProvider
    {
        private SQLiteDataAdapter quoteitems_adapter;
        private SQLiteConnection connection;
        private SQLiteCommand cmd;
        private DataTable quoteitems_dt;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if(handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }

        }

        public QuoteItemsDataProvider()
        {
            quoteitems_dt = new System.Data.DataTable();
            connection = new SQLiteConnection("DataSource=C:\\SQLite\\CentralDB.db");
            connection.Open();
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = string.Format("SELECT * FROM QTE_ITEMS");
            quoteitems_adapter = new SQLiteDataAdapter(cmd);
            //quoteitems_adapter.Fill(quoteitems_dataset);
            quoteitems_adapter.Fill(quoteitems_dt);

            //quoteitems_dt.Columns["line_total"].Expression = "qty*rate";
            //quoteitems_dt.Columns["tax_total"].Expression = "qty*rate*taxable";

            connection.Close();
        }

        public DataView getQuoteItems()
        {
            //quoteitems_dt.ColumnChanged += new INotifyPropertyChanged
            return quoteitems_dt.DefaultView;
        }

        public DataView getQuoteItemsByJob(string jobno)
        {
            quoteitems_dt.DefaultView.RowFilter = string.Format("jobno='{0}'", jobno);
            return quoteitems_dt.DefaultView;
        }

        public DataView getQuoteItemsByJob_and_Tab(string jobno, int tab_index)
        {
            quoteitems_dt.DefaultView.RowFilter = string.Format("jobno='{0}' AND tab_index={1}", jobno, tab_index);
            return quoteitems_dt.DefaultView;
        }
        
        public double getSumOfLineTotals(string jobno)
        {
            object sumObject;
            double proj_daily_total = 0.0;

            //quoteitems_dt.Columns["line_total"].Expression = "qty*rate";
            //quoteitems_dt.Columns["tax_total"].Expression = "qty*rate*taxable";

            string filter = string.Format("jobno='{0}'", jobno);
            sumObject = quoteitems_dt.Compute("Sum(line_total)", filter);

            try
            {
                Double.TryParse(sumObject.ToString(), out proj_daily_total);
            }
            catch(Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }
            return proj_daily_total;
        }

        public int getCountOfTabItems(string jobno)
        {
            int num = 0;
            int maxIndex = 0;
            foreach(DataRow dr in quoteitems_dt.Select("jobno='" + jobno + "'"))
            {
                object value = dr["tab_index"];

                if(value == DBNull.Value)
                {
                    num = 0;
                }
                else
                {
                    num = Convert.ToInt32(value) + 1;
                    //Console.WriteLine(value.ToString());
                    //Console.WriteLine("DB Tab Count Converted to Int32: " + num);

                    maxIndex = Math.Max(maxIndex, num);

                    //Console.WriteLine("Max is: " + maxIndex);
                }
            }

            Console.WriteLine("Database Tab Count: " + num);

            return num;
        }

        public void UpdateLineTotals()
        {
            foreach (DataRow row in quoteitems_dt.Rows)
            {
                int row_index = (int)quoteitems_dt.Rows.IndexOf(row);
                int line_total_index = row.Table.Columns["line_total"].Ordinal;

                double rate = 0;
                double qty = 1;

                //DataRow newRow = row.

                try
                {
                    Double.TryParse(row["qty"].ToString(), out qty);
                    Double.TryParse(row["rate"].ToString(), out rate);
                }
                catch(Exception Ex)
                {
                    Console.WriteLine(Ex.ToString());
                }

                //quoteitems_dt.Compute(")
                //    .SetField<Double>(line_total_index, rate * qty);

                //quoteitems_dt.ImportRow(row)
            }

            quoteitems_dt.AcceptChanges();
        }

        public void saveItemsToDB()
        {
            quoteitems_dt.AcceptChanges();

            try
            {
                quoteitems_dt.Columns["line_total"].Expression = "qty*rate";
                quoteitems_dt.Columns["tax_total"].Expression = "qty*rate*taxable";
                // ... Is this redundant?
                connection = new SQLiteConnection("DataSource=C:\\SQLite\\CentralDB.db");
                connection.Open();
                cmd = connection.CreateCommand();
                cmd.CommandText = string.Format("SELECT * FROM QTE_ITEMS");
                quoteitems_adapter = new SQLiteDataAdapter(cmd);
                SQLiteCommandBuilder builder = new SQLiteCommandBuilder(quoteitems_adapter);
                quoteitems_adapter.Update(quoteitems_dt);
                connection.Close();
            }
            catch (Exception Ex)
            {
                System.Windows.MessageBox.Show(Ex.Message);
            }
        }

        public void updateQuoteItemsByJob_Tab_Row(string item, string jobno, int tab_index, int row_index)
        {
            try
            {
                connection = new SQLiteConnection("DataSource=C:\\SQLite\\CentralDB.db");
                connection.Open();
                cmd = connection.CreateCommand();
                cmd.CommandText = string.Format("UPDATE QTE_ITEMS SET item='{0}' WHERE jobno='{1}' AND tab_index='{2}' AND row_index='{3}'", item, jobno, tab_index, row_index);
                cmd.ExecuteNonQuery();

                cmd.CommandText = string.Format("SELECT * FROM QTE_ITEMS");
                quoteitems_adapter = new SQLiteDataAdapter(cmd);
                quoteitems_adapter.Fill(quoteitems_dt);
                quoteitems_dt.AcceptChanges();

                connection.Close();
            }
            catch (Exception Ex)
            {
                System.Windows.MessageBox.Show(Ex.Message);
            }


        }
    }

}
