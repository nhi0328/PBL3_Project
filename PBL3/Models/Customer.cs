using System;

namespace PBL3.Models;

public class Customer : User
{
    public Customer() : base() { }

    public Customer(string cccd, string n = "", string p = "", string email = "", DateTime dob = default, string gender = "", string pass = "")
        : base(cccd, n, p, email, (dob == default ? new DateTime(2000, 1, 1) : dob), gender, pass)
    {
    }

    public override string GetRole()
    {
        return "CUSTOMER";
    }

    public override string Display()
    {
        return $"[Công dân] {FullName} - CCCD: {Cccd}";
    }
}