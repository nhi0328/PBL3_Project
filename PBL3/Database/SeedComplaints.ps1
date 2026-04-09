$ErrorActionPreference = 'Stop'

$connectionString = 'Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TrafficSafetyDB;Integrated Security=True;TrustServerCertificate=True;'
$csvPath = Join-Path $PSScriptRoot 'Complaints.csv'
$culture = [System.Globalization.CultureInfo]::InvariantCulture

$connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
$connection.Open()
$transaction = $connection.BeginTransaction()

try {
    $schemaCommand = $connection.CreateCommand()
    $schemaCommand.Transaction = $transaction
    $schemaCommand.CommandText = @"
IF OBJECT_ID(N'dbo.COMPLAINTS', N'U') IS NOT NULL
    DROP TABLE dbo.COMPLAINTS;

CREATE TABLE dbo.COMPLAINTS
(
    COMPLAINT_ID INT NOT NULL PRIMARY KEY,
    CCCD VARCHAR(12) NOT NULL,
    TITLE NVARCHAR(150) NOT NULL,
    CONTENT_DETAIL NVARCHAR(500) NOT NULL,
    STATUS NVARCHAR(50) NOT NULL,
    SUBMITTED_DATE DATE NOT NULL,
    REPORTED_LICENSE_PLATE VARCHAR(20) NOT NULL,
    OFFICER_BADGE_NUMBER VARCHAR(20) NULL
);
"@
    [void]$schemaCommand.ExecuteNonQuery()

    $insertCommand = $connection.CreateCommand()
    $insertCommand.Transaction = $transaction
    $insertCommand.CommandText = 'INSERT INTO dbo.COMPLAINTS (COMPLAINT_ID, CCCD, TITLE, CONTENT_DETAIL, STATUS, SUBMITTED_DATE, REPORTED_LICENSE_PLATE, OFFICER_BADGE_NUMBER) VALUES (@COMPLAINT_ID, @CCCD, @TITLE, @CONTENT_DETAIL, @STATUS, @SUBMITTED_DATE, @REPORTED_LICENSE_PLATE, @OFFICER_BADGE_NUMBER)'
    [void]$insertCommand.Parameters.Add('@COMPLAINT_ID', [System.Data.SqlDbType]::Int)
    [void]$insertCommand.Parameters.Add('@CCCD', [System.Data.SqlDbType]::VarChar, 12)
    [void]$insertCommand.Parameters.Add('@TITLE', [System.Data.SqlDbType]::NVarChar, 150)
    [void]$insertCommand.Parameters.Add('@CONTENT_DETAIL', [System.Data.SqlDbType]::NVarChar, 500)
    [void]$insertCommand.Parameters.Add('@STATUS', [System.Data.SqlDbType]::NVarChar, 50)
    [void]$insertCommand.Parameters.Add('@SUBMITTED_DATE', [System.Data.SqlDbType]::Date)
    [void]$insertCommand.Parameters.Add('@REPORTED_LICENSE_PLATE', [System.Data.SqlDbType]::VarChar, 20)
    [void]$insertCommand.Parameters.Add('@OFFICER_BADGE_NUMBER', [System.Data.SqlDbType]::VarChar, 20)

    $rows = Import-Csv -Path $csvPath -Encoding UTF8
    foreach ($row in $rows) {
        $insertCommand.Parameters['@COMPLAINT_ID'].Value = [int]$row.ComplaintId
        $insertCommand.Parameters['@CCCD'].Value = $row.CCCD.PadLeft(12, '0')
        $insertCommand.Parameters['@TITLE'].Value = $row.Title
        $insertCommand.Parameters['@CONTENT_DETAIL'].Value = $row.ContentDetail
        $insertCommand.Parameters['@STATUS'].Value = $row.Status
        $insertCommand.Parameters['@SUBMITTED_DATE'].Value = [DateTime]::ParseExact($row.SubmittedDate, 'dd/MM/yyyy', $culture)
        $insertCommand.Parameters['@REPORTED_LICENSE_PLATE'].Value = $row.ReportedLicensePlate
        if ($row.OfficerBadgeNumber -eq '-') {
            $insertCommand.Parameters['@OFFICER_BADGE_NUMBER'].Value = [DBNull]::Value
        }
        else {
            $insertCommand.Parameters['@OFFICER_BADGE_NUMBER'].Value = $row.OfficerBadgeNumber
        }
        [void]$insertCommand.ExecuteNonQuery()
    }

    $transaction.Commit()
    Write-Output ('Complaints seeded: ' + $rows.Count)
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
