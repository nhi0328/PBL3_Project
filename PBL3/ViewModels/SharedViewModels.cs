using PBL3.Models;
using System;

namespace PBL3.ViewModels
{
    public class LicenseViewModel
    {
        public string LicenseType { get; set; } = string.Empty;
        public string StatusBackground { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string StatusIcon { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string PointsText { get; set; } = string.Empty;
        public string LastUpdateText { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string ExpiryDateText { get; set; } = string.Empty;
        public string IssueDateText { get; set; } = string.Empty;
        public string PlaceOfIssue { get; set; } = "Đà Nẵng";
    }

    public class CustomerViewModel
    {
        private readonly Customer _customer;
        public CustomerViewModel(Customer customer)
        {
            _customer = customer ?? new Customer();
            LastName = _customer.FullName?.Contains(" ") == true ? _customer.FullName.Substring(0, _customer.FullName.LastIndexOf(" ")) : _customer.FullName;
            FirstName = _customer.FullName?.Contains(" ") == true ? _customer.FullName.Substring(_customer.FullName.LastIndexOf(" ") + 1) : "";
            BirthDay = _customer.Dob?.Day.ToString("00") ?? "";
            BirthMonth = _customer.Dob?.Month.ToString("00") ?? "";
            BirthYear = _customer.Dob?.Year.ToString() ?? "";
            IsMale = _customer.Gender == "Nam";
            IsFemale = _customer.Gender == "Nữ";
            IsOther = _customer.Gender != "Nam" && _customer.Gender != "Nữ" && !string.IsNullOrEmpty(_customer.Gender);
            CCCD = _customer.Cccd;
            PhoneNumber = _customer.Phone;
            Email = _customer.Email;
        }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string BirthDay { get; set; }
        public string BirthMonth { get; set; }
        public string BirthYear { get; set; }
        public bool IsMale { get; set; }
        public bool IsFemale { get; set; }
        public bool IsOther { get; set; }
        public string CCCD { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
