-- 1: Create the Database
CREATE DATABASE SUSCloud
ON
(
    NAME = SUSCloud_dat,
    FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\SUSClouddat.mdf',
    SIZE = 8MB,
    MAXSIZE = 64MB,
    FILEGROWTH = 4MB
)
LOG ON
(
    NAME = SUSCloud_log,
    FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\SUSCloudlog.ldf',
    SIZE = 4MB,
    MAXSIZE = 34MB,
    FILEGROWTH = 2MB
);
GO


-- 2: Create Encryption Keys
-- Create Master Key
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'SUSCloud148';
GO

-- Create Certificate
CREATE CERTIFICATE EmployeeCertificate
WITH SUBJECT = 'Employee Data Encryption';
GO

-- Create Symmetric Key
CREATE SYMMETRIC KEY EmployeeSymmetricKey
WITH ALGORITHM = AES_256
ENCRYPTION BY CERTIFICATE EmployeeCertificate;
GO

-- 3: Create Tables
--  Employees
CREATE TABLE Employees (
    EmployeeID INT IDENTITY(1,1) PRIMARY KEY,
    Name VARBINARY(MAX), -- Encrypted field
    Position NVARCHAR(100),
    Department NVARCHAR(100),
    Salary VARBINARY(MAX), -- Encrypted field
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

--  EmployeeDetails
CREATE TABLE EmployeeDetails (
    DetailID INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeID INT NOT NULL,
    Project NVARCHAR(100),
    Address NVARCHAR(255),
    StartDate DATE,
    EndDate DATE,
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);
GO

-- 4: Insert Data
BEGIN TRANSACTION;

BEGIN TRY
    DECLARE @EmployeeID INT;

    -- Open the Symmetric Key for Encryption
    OPEN SYMMETRIC KEY EmployeeSymmetricKey
    DECRYPTION BY CERTIFICATE EmployeeCertificate;

    -- Insert into Employees
    INSERT INTO Employees (Name, Position, Department, Salary, CreatedDate)
    VALUES (
        ENCRYPTBYKEY(KEY_GUID('EmployeeSymmetricKey'), CAST(N'John Doe' AS NVARCHAR(MAX))),
        'Manager',
        'HR',
        ENCRYPTBYKEY(KEY_GUID('EmployeeSymmetricKey'), CAST(75000 AS NVARCHAR(MAX))),
        GETDATE()
    );

    -- Get the ID of the Newly Inserted Employee
    SET @EmployeeID = SCOPE_IDENTITY();

    -- Insert into EmployeeDetails
    INSERT INTO EmployeeDetails (EmployeeID, Project, Address, StartDate, EndDate)
    VALUES (@EmployeeID, 'Project A', '123 Elm Street', '2024-01-01', '2024-12-31');

    -- Close the Symmetric Key
    CLOSE SYMMETRIC KEY EmployeeSymmetricKey;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

--5: Retrieve Data 
OPEN SYMMETRIC KEY EmployeeSymmetricKey
DECRYPTION BY CERTIFICATE EmployeeCertificate;

SELECT 
    e.EmployeeID,
    CAST(DECRYPTBYKEY(e.Name) AS NVARCHAR(MAX)) AS Name,
    e.Position,
    e.Department,
    CAST(DECRYPTBYKEY(e.Salary) AS NVARCHAR(MAX)) AS Salary,
    e.CreatedDate,
    ed.Project,
    ed.Address,
    ed.StartDate,
    ed.EndDate
FROM Employees e
LEFT JOIN EmployeeDetails ed ON e.EmployeeID = ed.EmployeeID;

CLOSE SYMMETRIC KEY EmployeeSymmetricKey;
GO

-- 6: Update Data
BEGIN TRANSACTION;

BEGIN TRY
    -- Open the Symmetric Key for Encryption
    OPEN SYMMETRIC KEY EmployeeSymmetricKey
    DECRYPTION BY CERTIFICATE EmployeeCertificate;

    -- Update Employees
    UPDATE Employees
    SET 
        Name = ENCRYPTBYKEY(KEY_GUID('EmployeeSymmetricKey'), CAST(N'Jane Doe' AS NVARCHAR(MAX))),
        Salary = ENCRYPTBYKEY(KEY_GUID('EmployeeSymmetricKey'), CAST(95000 AS NVARCHAR(MAX))),
        Position = 'Senior Developer'
    WHERE EmployeeID = 3;

    -- Update EmployeeDetails
    UPDATE EmployeeDetails
    SET 
        Project = 'Updated Project',
        Address = '456 Oak Street'
    WHERE EmployeeID = 3;

    -- Close the Symmetric Key
    CLOSE SYMMETRIC KEY EmployeeSymmetricKey;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- 7: Delete Data
BEGIN TRANSACTION;

BEGIN TRY
    DELETE FROM EmployeeDetails WHERE EmployeeID = 2;
    DELETE FROM Employees WHERE EmployeeID = 2;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO