using System;
using Microsoft.Data.SqlClient;

class Program {
    static void Main() {
        string connStr = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TrafficSafetyDB;Integrated Security=True";
        using var conn = new SqlConnection(connStr);
        conn.Open();
        using var cmd = new SqlCommand("SELECT t.name as TableName, c.name as ColName FROM sys.tables t JOIN sys.columns c ON t.object_id = c.object_id WHERE t.name LIKE '%Driving%' OR t.name LIKE '%DRIVING%'", conn);
        using var reader = cmd.ExecuteReader();
        while(reader.Read()) {
            Console.WriteLine($"{reader[0]} - {reader[1]}");
        }
    }
}
