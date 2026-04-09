using System;
using System.Linq;
using PBL3.Models;

namespace PBL3_Test {
    class Program {
        static void Main() {
            try {
                using var db = new TrafficSafetyDBContext();
                var count = db.Users.Count();
                Console.WriteLine(""OK! Count: "" + count);
            } catch (Exception ex) {
                Console.WriteLine(""ERROR: "" + ex.Message);
            }
        }
    }
}
