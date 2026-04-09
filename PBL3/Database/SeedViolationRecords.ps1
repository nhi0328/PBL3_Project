$ErrorActionPreference = 'Stop'

$connectionString = 'Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TrafficSafetyDB;Integrated Security=True;TrustServerCertificate=True;'
$csvPath = Join-Path $PSScriptRoot 'ViolationRecords.csv'
$culture = [System.Globalization.CultureInfo]::InvariantCulture

$connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
$connection.Open()
$transaction = $connection.BeginTransaction()

try {
    $schemaCommand = $connection.CreateCommand()
    $schemaCommand.Transaction = $transaction
    $schemaCommand.CommandText = @"
IF OBJECT_ID(N'dbo.VIOLATION_RECORDS', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VIOLATION_RECORDS
    (
        RECORD_ID INT NOT NULL PRIMARY KEY,
        OWNER_NAME NVARCHAR(100) NOT NULL,
        LICENSE_PLATE VARCHAR(20) NOT NULL,
        VEHICLE_MODEL NVARCHAR(100) NOT NULL,
        LAW_SEQUENCE_NUMBER INT NOT NULL,
        VIOLATION_DETAIL NVARCHAR(255) NOT NULL,
        VIOLATION_DATE DATE NOT NULL,
        VIOLATION_TIME TIME NOT NULL,
        DEMERIT_POINTS INT NOT NULL,
        DETAILED_LOCATION NVARCHAR(255) NOT NULL
    );
END;

DELETE FROM dbo.VIOLATION_RECORDS;
"@
    [void]$schemaCommand.ExecuteNonQuery()

    $insertCommand = $connection.CreateCommand()
    $insertCommand.Transaction = $transaction
    $insertCommand.CommandText = 'INSERT INTO dbo.VIOLATION_RECORDS (RECORD_ID, OWNER_NAME, LICENSE_PLATE, VEHICLE_MODEL, LAW_SEQUENCE_NUMBER, VIOLATION_DETAIL, VIOLATION_DATE, VIOLATION_TIME, DEMERIT_POINTS, DETAILED_LOCATION) VALUES (@RECORD_ID, @OWNER_NAME, @LICENSE_PLATE, @VEHICLE_MODEL, @LAW_SEQUENCE_NUMBER, @VIOLATION_DETAIL, @VIOLATION_DATE, @VIOLATION_TIME, @DEMERIT_POINTS, @DETAILED_LOCATION)'
    [void]$insertCommand.Parameters.Add('@RECORD_ID', [System.Data.SqlDbType]::Int)
    [void]$insertCommand.Parameters.Add('@OWNER_NAME', [System.Data.SqlDbType]::NVarChar, 100)
    [void]$insertCommand.Parameters.Add('@LICENSE_PLATE', [System.Data.SqlDbType]::VarChar, 20)
    [void]$insertCommand.Parameters.Add('@VEHICLE_MODEL', [System.Data.SqlDbType]::NVarChar, 100)
    [void]$insertCommand.Parameters.Add('@LAW_SEQUENCE_NUMBER', [System.Data.SqlDbType]::Int)
    [void]$insertCommand.Parameters.Add('@VIOLATION_DETAIL', [System.Data.SqlDbType]::NVarChar, 255)
    [void]$insertCommand.Parameters.Add('@VIOLATION_DATE', [System.Data.SqlDbType]::Date)
    [void]$insertCommand.Parameters.Add('@VIOLATION_TIME', [System.Data.SqlDbType]::Time)
    [void]$insertCommand.Parameters.Add('@DEMERIT_POINTS', [System.Data.SqlDbType]::Int)
    [void]$insertCommand.Parameters.Add('@DETAILED_LOCATION', [System.Data.SqlDbType]::NVarChar, 255)

    $records = Import-Csv -Path $csvPath -Encoding UTF8
    foreach ($record in $records) {
        $insertCommand.Parameters['@RECORD_ID'].Value = [int]$record.STT
        $insertCommand.Parameters['@OWNER_NAME'].Value = $record.OwnerName
        $insertCommand.Parameters['@LICENSE_PLATE'].Value = $record.LicensePlate
        $insertCommand.Parameters['@VEHICLE_MODEL'].Value = $record.VehicleModel
        $insertCommand.Parameters['@LAW_SEQUENCE_NUMBER'].Value = [int]$record.LawSequenceNumber
        $insertCommand.Parameters['@VIOLATION_DETAIL'].Value = $record.ViolationDetail
        $insertCommand.Parameters['@VIOLATION_DATE'].Value = [DateTime]::ParseExact($record.ViolationDate, 'dd/MM/yyyy', $culture)
        $insertCommand.Parameters['@VIOLATION_TIME'].Value = [TimeSpan]::ParseExact($record.ViolationTime, 'hh\:mm', $culture)
        $insertCommand.Parameters['@DEMERIT_POINTS'].Value = [int]$record.DemeritPoints
        $insertCommand.Parameters['@DETAILED_LOCATION'].Value = $record.DetailedLocation
        [void]$insertCommand.ExecuteNonQuery()
    }

    $transaction.Commit()
    Write-Output ('Violation records seeded: ' + $records.Count)
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
