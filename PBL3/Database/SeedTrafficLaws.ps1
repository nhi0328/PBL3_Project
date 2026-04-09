$ErrorActionPreference = 'Stop'

$connectionString = 'Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TrafficSafetyDB;Integrated Security=True;TrustServerCertificate=True;'
$csvPath = Join-Path $PSScriptRoot 'TrafficLaws.csv'
$decree = 'Nghi dinh: 168/2024/ND-CP'
$issueDate = 'Ngay ban hanh: 25/12/2024'
$effectiveDate = 'Ngay co hieu luc chung: 01/01/2025'

$connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
$connection.Open()
$transaction = $connection.BeginTransaction()

try {
    $schemaCommand = $connection.CreateCommand()
    $schemaCommand.Transaction = $transaction
    $schemaCommand.CommandText = @"
IF OBJECT_ID(N'dbo.TRAFFIC_LAWS', N'U') IS NOT NULL
    DROP TABLE dbo.TRAFFIC_LAWS;

CREATE TABLE dbo.TRAFFIC_LAWS
(
    LAW_ID INT NOT NULL PRIMARY KEY,
    VIOLATION_DESCRIPTION NVARCHAR(255) NOT NULL,
    FINE_RANGE NVARCHAR(100) NOT NULL,
    LAW_REFERENCE NVARCHAR(500) NOT NULL,
    VEHICLE_TYPE NVARCHAR(50) NOT NULL
);
"@
    [void]$schemaCommand.ExecuteNonQuery()

    $insertCommand = $connection.CreateCommand()
    $insertCommand.Transaction = $transaction
    $insertCommand.CommandText = 'INSERT INTO dbo.TRAFFIC_LAWS (LAW_ID, VIOLATION_DESCRIPTION, FINE_RANGE, LAW_REFERENCE, VEHICLE_TYPE) VALUES (@LAW_ID, @VIOLATION_DESCRIPTION, @FINE_RANGE, @LAW_REFERENCE, @VEHICLE_TYPE)'
    [void]$insertCommand.Parameters.Add('@LAW_ID', [System.Data.SqlDbType]::Int)
    [void]$insertCommand.Parameters.Add('@VIOLATION_DESCRIPTION', [System.Data.SqlDbType]::NVarChar, 255)
    [void]$insertCommand.Parameters.Add('@FINE_RANGE', [System.Data.SqlDbType]::NVarChar, 100)
    [void]$insertCommand.Parameters.Add('@LAW_REFERENCE', [System.Data.SqlDbType]::NVarChar, 500)
    [void]$insertCommand.Parameters.Add('@VEHICLE_TYPE', [System.Data.SqlDbType]::NVarChar, 50)

    $rows = Import-Csv -Path $csvPath -Encoding UTF8
    foreach ($row in $rows) {
        $lawReference = [string]::Format('Diem tru: {0} | {1} | {2} | {3}', $row.PenaltyPoints, $decree, $issueDate, $effectiveDate)
        $insertCommand.Parameters['@LAW_ID'].Value = [int]$row.STT
        $insertCommand.Parameters['@VIOLATION_DESCRIPTION'].Value = $row.ViolationDescription
        $insertCommand.Parameters['@FINE_RANGE'].Value = $row.FineRange
        $insertCommand.Parameters['@LAW_REFERENCE'].Value = $lawReference
        $insertCommand.Parameters['@VEHICLE_TYPE'].Value = $row.VehicleType
        [void]$insertCommand.ExecuteNonQuery()
    }

    $transaction.Commit()
    Write-Output ('Traffic laws seeded: ' + $rows.Count)
}
catch {
    if ($transaction) {
        $transaction.Rollback()
    }
    throw
}
finally {
    $connection.Close()
}
