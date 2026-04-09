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

namespace PBL3
{
    
    public partial class Page1: Page
    {
        public Page1()
        {
            InitializeComponent();
        }

        private void BtnDangNhap_Click(object sender, RoutedEventArgs e)
        {
            // Lệnh chuyển sang Page2
            NavigationService.Navigate(new Page2());
        }

        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            // Lệnh chuyển sang Page10
            NavigationService.Navigate(new Page10());
        }

        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            // Lệnh chuyển sang Page11
            NavigationService.Navigate(new Page11());
        }

        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            new CustomMessageBox("Bạn cần đăng nhập để phản ánh vi phạm!").ShowDialog();
            // Lệnh chuyển sang Page2
            NavigationService.Navigate(new Page2());
        }
    }
}