using System.Windows;
namespace PBL3
{
    public partial class ConfirmAddBox : Window
    {
        public bool IsConfirmed { get; private set; }
        public ConfirmAddBox(string catName)
        {
            InitializeComponent();
            txtMessage.Text = $"Loại phương tiện '{catName}' chưa tồn tại. Bạn có muốn thêm vào hệ thống không?";
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
        }
    }
}