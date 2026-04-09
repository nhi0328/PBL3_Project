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

namespace PBL3
{
    /// <summary>
    /// Interaction logic for ConfirmDeleteBox.xaml
    /// </summary>
    public partial class ConfirmDeleteBox : Window
    {
        public ConfirmDeleteBox()
        {
            InitializeComponent();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Trả về true
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Trả về false
            this.Close();
        }
    }
}
