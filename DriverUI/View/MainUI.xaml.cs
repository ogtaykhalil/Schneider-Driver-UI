using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DriverUI.ViewModel;
using System.Threading.Tasks;

namespace DriverUI.View
{
    /// <summary>
    /// Interaction logic for MainUI.xaml
    /// </summary>
    public partial class MainUI : Window
    {
        viewModel vm;
        public MainUI()
        {
            InitializeComponent();
            vm = new viewModel();
            this.DataContext = vm;

            Task.Run(vm.Run);



        }        

      
    }
}
