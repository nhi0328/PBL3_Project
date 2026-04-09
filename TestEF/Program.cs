Console.WriteLine("Hello, World!");

using System;
using System.Linq;
using PBL3.Models;
try {
    using var db = new TrafficSafetyDBContext();
    var count = db.Users.Count();
    Console.WriteLine("OK! Count: " + count);
} catch (Exception ex) {
    Console.WriteLine("ERROR: " + ex.Message);
}
