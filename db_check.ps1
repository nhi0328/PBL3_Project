$connStr = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TrafficSafetyDB;Integrated Security=True"
$conn = New-Object -TypeName System.Data.SqlClient.SqlConnection -ArgumentList $connStr
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT t.name as TableName, c.name as ColName FROM sys.tables t JOIN sys.columns c ON t.object_id = c.object_id WHERE t.name LIKE '%Driving%' OR t.name LIKE '%DRIVING%'"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host "$($reader[0]) - $($reader[1])"
}
$conn.Close()
