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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PBL3
{
    public partial class Page11 : Page
    {
        // Constructor mặc định
        public Page11()
        {
            InitializeComponent();
        }

        // Xử lý sự kiện nút Quay lại
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        // Xử lý sự kiện nút Tìm kiếm
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtIdentifier.Text;
            MessageBox.Show($"Đang tìm kiếm luật với từ khóa: {keyword}");
            // Viết logic tìm kiếm SQL ở đây...
        }
    }
}
