$ErrorActionPreference = 'Stop'

$connectionString = 'Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TrafficSafetyDB;Integrated Security=True;TrustServerCertificate=True;'
$masterConnectionString = 'Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True;'
$csvPath = Join-Path $PSScriptRoot 'AuthAccounts.csv'
$culture = [System.Globalization.CultureInfo]::InvariantCulture

function Get-AdminFullName([int]$index) {
    return [string]::Concat([char]81, [char]117, [char]7843, [char]110, ' ', [char]116, [char]114, [char]7883, ' ', [char]118, [char]105, [char]234, [char]110, ' ', $index)
}

$masterConnection = New-Object System.Data.SqlClient.SqlConnection $masterConnectionString
$masterConnection.Open()
try {
    $createDb = $masterConnection.CreateCommand()
    $createDb.CommandText = "IF DB_ID(N'TrafficSafetyDB') IS NULL CREATE DATABASE [TrafficSafetyDB];"
    [void]$createDb.ExecuteNonQuery()
}
finally {
    $masterConnection.Close()
}

$connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
$connection.Open()
$transaction = $connection.BeginTransaction()

try {
    $schemaSql = @"
IF OBJECT_ID(N'dbo.USERS', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.USERS
    (
        CCCD VARCHAR(12) NOT NULL PRIMARY KEY,
        FULL_NAME NVARCHAR(100) NOT NULL,
        DOB DATE NULL,
        EMAIL VARCHAR(100) NOT NULL,
        PHONE VARCHAR(15) NOT NULL,
        PASS_HASH NVARCHAR(100) NOT NULL,
        GENDER NVARCHAR(10) NULL,
        IMAGE_PATH NVARCHAR(255) NULL
    );
END;

IF OBJECT_ID(N'dbo.OFFICERS', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OFFICERS
    (
        BADGE_NUMBER VARCHAR(20) NOT NULL PRIMARY KEY,
        FULL_NAME NVARCHAR(100) NOT NULL,
        PASS_HASH NVARCHAR(100) NOT NULL,
        IMAGE_PATH NVARCHAR(255) NULL
    );
END;

IF OBJECT_ID(N'dbo.ADMINS', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ADMINS
    (
        ADMIN_ID VARCHAR(20) NOT NULL PRIMARY KEY,
        USERNAME VARCHAR(50) NOT NULL UNIQUE,
        FULL_NAME NVARCHAR(100) NOT NULL,
        PASS_HASH NVARCHAR(100) NOT NULL,
        IMAGE_PATH NVARCHAR(255) NULL
    );
END;

DELETE FROM dbo.ADMINS;
DELETE FROM dbo.OFFICERS;
DELETE FROM dbo.USERS;
"@

    $schemaCommand = $connection.CreateCommand()
    $schemaCommand.Transaction = $transaction
    $schemaCommand.CommandText = $schemaSql
    [void]$schemaCommand.ExecuteNonQuery()

    $insertUser = $connection.CreateCommand()
    $insertUser.Transaction = $transaction
    $insertUser.CommandText = 'INSERT INTO dbo.USERS (CCCD, FULL_NAME, DOB, EMAIL, PHONE, PASS_HASH, GENDER, IMAGE_PATH) VALUES (@CCCD, @FULL_NAME, @DOB, @EMAIL, @PHONE, @PASS_HASH, @GENDER, @IMAGE_PATH)'
    [void]$insertUser.Parameters.Add('@CCCD', [System.Data.SqlDbType]::VarChar, 12)
    [void]$insertUser.Parameters.Add('@FULL_NAME', [System.Data.SqlDbType]::NVarChar, 100)
    [void]$insertUser.Parameters.Add('@DOB', [System.Data.SqlDbType]::Date)
    [void]$insertUser.Parameters.Add('@EMAIL', [System.Data.SqlDbType]::VarChar, 100)
    [void]$insertUser.Parameters.Add('@PHONE', [System.Data.SqlDbType]::VarChar, 15)
    [void]$insertUser.Parameters.Add('@PASS_HASH', [System.Data.SqlDbType]::NVarChar, 100)
    [void]$insertUser.Parameters.Add('@GENDER', [System.Data.SqlDbType]::NVarChar, 10)
    [void]$insertUser.Parameters.Add('@IMAGE_PATH', [System.Data.SqlDbType]::NVarChar, 255)

    $insertOfficer = $connection.CreateCommand()
    $insertOfficer.Transaction = $transaction
    $insertOfficer.CommandText = 'INSERT INTO dbo.OFFICERS (BADGE_NUMBER, FULL_NAME, PASS_HASH, IMAGE_PATH) VALUES (@BADGE_NUMBER, @FULL_NAME, @PASS_HASH, @IMAGE_PATH)'
    [void]$insertOfficer.Parameters.Add('@BADGE_NUMBER', [System.Data.SqlDbType]::VarChar, 20)
    [void]$insertOfficer.Parameters.Add('@FULL_NAME', [System.Data.SqlDbType]::NVarChar, 100)
    [void]$insertOfficer.Parameters.Add('@PASS_HASH', [System.Data.SqlDbType]::NVarChar, 100)
    [void]$insertOfficer.Parameters.Add('@IMAGE_PATH', [System.Data.SqlDbType]::NVarChar, 255)

    $insertAdmin = $connection.CreateCommand()
    $insertAdmin.Transaction = $transaction
    $insertAdmin.CommandText = 'INSERT INTO dbo.ADMINS (ADMIN_ID, USERNAME, FULL_NAME, PASS_HASH, IMAGE_PATH) VALUES (@ADMIN_ID, @USERNAME, @FULL_NAME, @PASS_HASH, @IMAGE_PATH)'
    [void]$insertAdmin.Parameters.Add('@ADMIN_ID', [System.Data.SqlDbType]::VarChar, 20)
    [void]$insertAdmin.Parameters.Add('@USERNAME', [System.Data.SqlDbType]::VarChar, 50)
    [void]$insertAdmin.Parameters.Add('@FULL_NAME', [System.Data.SqlDbType]::NVarChar, 100)
    [void]$insertAdmin.Parameters.Add('@PASS_HASH', [System.Data.SqlDbType]::NVarChar, 100)
    [void]$insertAdmin.Parameters.Add('@IMAGE_PATH', [System.Data.SqlDbType]::NVarChar, 255)

    $accounts = Import-Csv -Path $csvPath -Encoding UTF8
    foreach ($account in $accounts) {
        $insertUser.Parameters['@CCCD'].Value = $account.CCCD
        $insertUser.Parameters['@FULL_NAME'].Value = $account.FullName
        $insertUser.Parameters['@DOB'].Value = [DateTime]::ParseExact($account.Dob, 'dd/MM/yyyy', $culture)
        $insertUser.Parameters['@EMAIL'].Value = $account.Email
        $insertUser.Parameters['@PHONE'].Value = $account.Phone
        $insertUser.Parameters['@PASS_HASH'].Value = $account.Password
        $insertUser.Parameters['@GENDER'].Value = $account.Gender
        $insertUser.Parameters['@IMAGE_PATH'].Value = [DBNull]::Value
        [void]$insertUser.ExecuteNonQuery()

        if (-not [string]::IsNullOrWhiteSpace($account.BadgeNumber)) {
            $insertOfficer.Parameters['@BADGE_NUMBER'].Value = $account.BadgeNumber
            $insertOfficer.Parameters['@FULL_NAME'].Value = $account.FullName
            $insertOfficer.Parameters['@PASS_HASH'].Value = $account.Password
            $insertOfficer.Parameters['@IMAGE_PATH'].Value = [DBNull]::Value
            [void]$insertOfficer.ExecuteNonQuery()
        }
    }

    $admins = @(
        @{ AdminId = 'AD01'; Username = 'admin1'; FullName = (Get-AdminFullName 1); Password = 'admin' },
        @{ AdminId = 'AD02'; Username = 'admin2'; FullName = (Get-AdminFullName 2); Password = 'admin' },
        @{ AdminId = 'AD03'; Username = 'admin3'; FullName = (Get-AdminFullName 3); Password = 'admin' },
        @{ AdminId = 'AD04'; Username = 'admin4'; FullName = (Get-AdminFullName 4); Password = 'admin' }
    )

    foreach ($admin in $admins) {
        $insertAdmin.Parameters['@ADMIN_ID'].Value = $admin.AdminId
        $insertAdmin.Parameters['@USERNAME'].Value = $admin.Username
        $insertAdmin.Parameters['@FULL_NAME'].Value = $admin.FullName
        $insertAdmin.Parameters['@PASS_HASH'].Value = $admin.Password
        $insertAdmin.Parameters['@IMAGE_PATH'].Value = [DBNull]::Value
        [void]$insertAdmin.ExecuteNonQuery()
    }

    $transaction.Commit()
    Write-Output 'Reseed completed successfully.'
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
