using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PBL3.Models;
using PBL3.ViewModels;

namespace PBL3
{
    public class NotificationViewModel
    {
        public string Content { get; set; }
        public string Date { get; set; }
        public bool IsUnread { get; set; }
        public Visibility UnreadVisibility => IsUnread ? Visibility.Visible : Visibility.Collapsed;
    }
}