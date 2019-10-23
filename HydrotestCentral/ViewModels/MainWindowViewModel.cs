using HydrotestCentral.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HydrotestCentral.ViewModels
{
    public partial class MainWindowViewModel: INotifyPropertyChanged
    {    
        public MainWindowViewModel()
        {
            InitializeComponent();
        }





        public void LoadData()
        {
            // Make a call to the repository class here
            // to set the properties of your viewmodel
        }







        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
