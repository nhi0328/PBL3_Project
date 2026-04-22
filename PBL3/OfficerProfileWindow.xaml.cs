using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using PBL3.Models; // Chú ý: Đảm bảo using đúng thư mục chứa model Officer của Nhi

namespace PBL3
{
    public partial class OfficerProfileWindow : Window
    {
        public OfficerProfileWindow(Officer officer) // Nhận vào đối tượng Officer
        {
            InitializeComponent();

            if (officer != null)
            {
                txtOfficerId.Text = string.IsNullOrEmpty(officer.OfficerId) ? "Không có ID" : officer.OfficerId;
                txtCCCD.Text = string.IsNullOrEmpty(officer.Cccd) ? "Không xác định" : officer.Cccd;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}