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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace HydrotestCentral
{
    /// <summary>
    /// Interaction logic for QuoteItemGrid.xaml
    /// </summary>
    public partial class QuoteItemGrid: UserControl
    {
        public SQLiteConnection connection;
        public SQLiteDataAdapter dataAdapter;
        public string item_placeholder;
        public QuoteItemsDataProvider quote_items;
        public static string jobno;
        public static int tab_index;

        #region ViewModel
        // TODO: Move this to a viewmodel

        public ObservableCollection<Item> Items { get; set; }
        #endregion

        public QuoteItemGrid(QuoteItemsDataProvider quote_items_in, string jobno_in, int tab_index_in)
        {
            InitializeComponent();
            quote_items = quote_items_in;
            jobno = jobno_in;
            tab_index = tab_index_in;

            Items = new ObservableCollection<Item>();
        }

        private void initializeItems()
        {
            using (connection = new SQLiteConnection("DataSource=C:\\Users\\SFWMD\\Aqua-Tech Hydro Services\\IT - Documents\\7.8 Databases\\CentralDB.db"))
            {
                connection.Open();

                string query = string.Format("SELECT ITEM, DESCR, RATE FROM Inventory ORDER BY ITEM ASC");

                using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //float.TryParse(reader.GetString(2), out float rate);

                            Item newItem = new Item(reader.GetString(0), reader.GetString(1), 0.0);
                            Console.WriteLine("Item: " + newItem.Itemname + "| " + newItem.Descr + " | " + newItem.Rate);
                            Items.Add(newItem);
                        }
                    }
                }
                connection.Close();
            }

            Console.WriteLine(Items.Count.ToString() + " items loaded...");
        }

        private void UpdateQuoteItemGrid()
        {
            Console.WriteLine("Cell edit\n");
        }

        private void QItems_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //e.Row.Item.Text.ToString();
            Console.WriteLine("CELL EDIT - Row: " + e.Row.GetIndex() + " edited\n");

            try
            {
                //
                //quote_items.saveItemsToDB();
                //UpdateCurrentQuoteDashboard();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
            }
        }

        private void QItems_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            //e.Row.Item.Text.ToString();
            Console.WriteLine("ROW EDIT - Row: " + e.Row.GetIndex() + " edited\n");

            try
            {
                //quote_items.UpdateLineTotals();
                //quote_items.saveItemsToDB();
                //UpdateCurrentQuoteDashboard();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
            }
        }

        private void ItemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get the ComboBox.
            var comboBox = sender as ComboBox;
            //txtBlock_item = 

            // ... Get the selected value out of the item box
            string value = comboBox.SelectedItem as string;
            //MessageBox.Show(parent.ToString());

            // ... Get the row selected
            DataGridRow dgr = GetSelectedRow(QItems);
            DataGridCell item_cell = GetCell(QItems, 1);
            int row_index = dgr.GetIndex();
            Console.WriteLine(string.Format("row_index: {0}", row_index));

            //MessageBox.Show(cell.Content as string);
            item_cell.Content = value;

            string item_cell_value = item_cell.Content as string;
            item_placeholder = value;

            // Get access to quoteitems_dt
            quote_items.updateQuoteItemsByJob_Tab_Row(item_cell_value, jobno, tab_index, row_index);
            Console.WriteLine(string.Format("RAN UPDATE QUERY: Item {0} update on Job {1}, Tab {2}, Row {3}", item_cell_value, jobno, tab_index, row_index));
            if(QItems.HasItems)
            {
                QItems.ItemsSource = null;
                QItems.Items.Clear();
                QItems.ItemsSource = quote_items.getQuoteItemsByJob_and_Tab(jobno, tab_index);
                QItems.Items.Refresh();
            }
            QItems.Items.Refresh();

        }

        private void ItemComboBox_DropDownClosed(object sender, EventArgs e)
        {
            /*
            var comboBox = sender as ComboBox;
            Console.WriteLine("item_placeholder: " + item_placeholder);
            comboBox.Text = item_placeholder;
            Console.WriteLine("comboBox.text: " + comboBox.Text.ToString());
            */

            //txtBlock_item.Text = item_placeholder;
            //this.QItems.SetValue(this., item_placeholder);
        }

        private void QuoteItemGridControl_Loaded(object sender, RoutedEventArgs e)
        {
            initializeItems();
        }

        public static DataGridRow GetSelectedRow(DataGrid grid)
        {
            return (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem);
        }

        public static DataGridCell GetCell(DataGrid dataGrid, int column)
        {
            DataGridRow rowContainer = GetSelectedRow(dataGrid);
            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                // try to get the cell but it may possibly be virtualized
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    // now try to bring into view and retreive the cell
                    dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);

                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }

                return cell;
            }

            return null;
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }


    }

    public class DataGridComboBoxColumnWithBindingHack : DataGridComboBoxColumn
    {
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            FrameworkElement element = base.GenerateEditingElement(cell, dataItem);
            CopyItemsSource(element);
            return element;
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            FrameworkElement element = base.GenerateElement(cell, dataItem);
            CopyItemsSource(element);
            return element;
        }

        private void CopyItemsSource(FrameworkElement element)
        {
            BindingOperations.SetBinding(element, ComboBox.ItemsSourceProperty,
              BindingOperations.GetBinding(this, ComboBox.ItemsSourceProperty));
        }
    }

    public class Item: INotifyPropertyChanged
    {
        string _itemname;
        string _descr;
        double _rate;

        public Item(string name, string descr, double rate)
        {
            _itemname = name;
            _descr = descr;
            _rate = rate;
        }

        public string Itemname
        {
            get
            {
                return _itemname;
            }
            set
            {
                if(_itemname != value)
                {
                    _itemname = value;
                    NotifyPropertyChanged("Itemname");
                }
            }
        }

        public string Descr
        {
            get
            {
                return _descr;
            }
            set
            {
                if (_descr != value)
                {
                    _descr = value;
                    NotifyPropertyChanged("Descr");
                }
            }
        }

        public double Rate
        {
            get
            {
                return _rate;
            }
            set
            {
                if(_rate != value)
                {
                    _rate = value;
                    NotifyPropertyChanged("Rate");
                }
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
