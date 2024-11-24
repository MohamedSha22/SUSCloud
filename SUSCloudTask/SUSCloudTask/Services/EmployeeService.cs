using Microsoft.Data.SqlClient;
using SUSCloudTask.DTOs;

namespace SUSCloudTask.Services
{
    public class EmployeeService
    {
        private readonly string _connectionString;

        public EmployeeService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private void ExecuteSymmetricKeyCommand(SqlConnection connection, SqlTransaction? transaction, string commandText)
        {
            using (var command = new SqlCommand(commandText, connection, transaction))
            {
                command.ExecuteNonQuery();
            }
        }

        private EmployeeDTO MapReaderToEmployee(SqlDataReader reader)
        {
            return new EmployeeDTO
            {
                id = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                name = reader.GetString(reader.GetOrdinal("Name")),
                position = reader.GetString(reader.GetOrdinal("Position")),
                department = reader.GetString(reader.GetOrdinal("Department")),
                salary = decimal.Parse(reader.GetString(reader.GetOrdinal("Salary"))),
                project = reader.IsDBNull(reader.GetOrdinal("Project")) ? null : reader.GetString(reader.GetOrdinal("Project")),
                address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                startDate = reader.IsDBNull(reader.GetOrdinal("StartDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("StartDate")),
                endDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("EndDate"))
            };
        }

        public List<EmployeeDTO> GetAllEmployees()
        {
            var employees = new List<EmployeeDTO>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                ExecuteSymmetricKeyCommand(connection, null, "OPEN SYMMETRIC KEY EmployeeSymmetricKey DECRYPTION BY CERTIFICATE EmployeeCertificate;");

                using (var command = new SqlCommand(@"
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
                LEFT JOIN EmployeeDetails ed ON e.EmployeeID = ed.EmployeeID;", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(MapReaderToEmployee(reader));
                        }
                    }
                }

                ExecuteSymmetricKeyCommand(connection, null, "CLOSE SYMMETRIC KEY EmployeeSymmetricKey;");
            }

            return employees;
        }

        public EmployeeDTO GetEmployeeById(int employeeId)
        {
            EmployeeDTO? employee = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                ExecuteSymmetricKeyCommand(connection, null, "OPEN SYMMETRIC KEY EmployeeSymmetricKey DECRYPTION BY CERTIFICATE EmployeeCertificate;");

                using (var command = new SqlCommand(@"
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
                LEFT JOIN EmployeeDetails ed ON e.EmployeeID = ed.EmployeeID
                WHERE e.EmployeeID = @EmployeeID;", connection))
                {
                    command.Parameters.AddWithValue("@EmployeeID", employeeId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            employee = MapReaderToEmployee(reader);
                        }
                    }
                }

                ExecuteSymmetricKeyCommand(connection, null, "CLOSE SYMMETRIC KEY EmployeeSymmetricKey;");
            }

            return employee;
        }

        public void InsertEmployeeWithDetails(EmployeeDTO employee)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    ExecuteSymmetricKeyCommand(connection, transaction, "OPEN SYMMETRIC KEY EmployeeSymmetricKey DECRYPTION BY CERTIFICATE EmployeeCertificate;");

                    int employeeId;
                    using (var insertEmployeeCommand = new SqlCommand(@"
                    INSERT INTO Employees (Name, Position, Department, Salary, CreatedDate)
                    VALUES (ENCRYPTBYKEY(KEY_GUID('EmployeeSymmetricKey'), @Name),
                            @Position,
                            @Department,
                            ENCRYPTBYKEY(KEY_GUID('EmployeeSymmetricKey'), @Salary),
                            GETDATE());
                    SELECT SCOPE_IDENTITY();", connection, transaction))
                    {
                        insertEmployeeCommand.Parameters.AddWithValue("@Name", employee.name);
                        insertEmployeeCommand.Parameters.AddWithValue("@Position", employee.position);
                        insertEmployeeCommand.Parameters.AddWithValue("@Department", employee.department);
                        insertEmployeeCommand.Parameters.AddWithValue("@Salary", employee.salary.ToString());

                        employeeId = Convert.ToInt32(insertEmployeeCommand.ExecuteScalar());
                    }

                    using (var insertDetailsCommand = new SqlCommand(@"
                    INSERT INTO EmployeeDetails (EmployeeID, Project, Address, StartDate, EndDate)
                    VALUES (@EmployeeID, @Project, @Address, @StartDate, @EndDate);", connection, transaction))
                    {
                        insertDetailsCommand.Parameters.AddWithValue("@EmployeeID", employeeId);
                        insertDetailsCommand.Parameters.AddWithValue("@Project", employee.project);
                        insertDetailsCommand.Parameters.AddWithValue("@Address", employee.address ?? (object)DBNull.Value);
                        insertDetailsCommand.Parameters.AddWithValue("@StartDate", employee.startDate ?? (object)DBNull.Value);
                        insertDetailsCommand.Parameters.AddWithValue("@EndDate", employee.endDate ?? (object)DBNull.Value);

                        insertDetailsCommand.ExecuteNonQuery();
                    }

                    ExecuteSymmetricKeyCommand(connection, transaction, "CLOSE SYMMETRIC KEY EmployeeSymmetricKey;");

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdateEmployeeWithDetails(int employeeId, EmployeeDTO employee)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    ExecuteSymmetricKeyCommand(connection, transaction, "OPEN SYMMETRIC KEY EmployeeSymmetricKey DECRYPTION BY CERTIFICATE EmployeeCertificate;");

                    using (var updateEmployeeCommand = new SqlCommand(@"
                    UPDATE Employees
                    SET 
                        Name = ENCRYPTBYKEY(KEY_GUID('EmployeeSymmetricKey'), @Name),
                        Salary = ENCRYPTBYKEY(KEY_GUID('EmployeeSymmetricKey'), @Salary),
                        Position = @Position,
                        Department = @Department
                    WHERE EmployeeID = @EmployeeID;", connection, transaction))
                    {
                        updateEmployeeCommand.Parameters.AddWithValue("@Name", employee.name);
                        updateEmployeeCommand.Parameters.AddWithValue("@Salary", employee.salary.ToString());
                        updateEmployeeCommand.Parameters.AddWithValue("@Position", employee.position);
                        updateEmployeeCommand.Parameters.AddWithValue("@Department", employee.department);
                        updateEmployeeCommand.Parameters.AddWithValue("@EmployeeID", employeeId);

                        updateEmployeeCommand.ExecuteNonQuery();
                    }

                    using (var updateDetailsCommand = new SqlCommand(@"
                    UPDATE EmployeeDetails
                    SET 
                        Project = @Project,
                        Address = @Address,
                        StartDate = @StartDate,
                        EndDate = @EndDate
                    WHERE EmployeeID = @EmployeeID;", connection, transaction))
                    {
                        updateDetailsCommand.Parameters.AddWithValue("@Project", employee.project);
                        updateDetailsCommand.Parameters.AddWithValue("@Address", employee.address ?? (object)DBNull.Value);
                        updateDetailsCommand.Parameters.AddWithValue("@StartDate", employee.startDate ?? (object)DBNull.Value);
                        updateDetailsCommand.Parameters.AddWithValue("@EndDate", employee.endDate ?? (object)DBNull.Value);
                        updateDetailsCommand.Parameters.AddWithValue("@EmployeeID", employeeId);

                        updateDetailsCommand.ExecuteNonQuery();
                    }

                    ExecuteSymmetricKeyCommand(connection, transaction, "CLOSE SYMMETRIC KEY EmployeeSymmetricKey;");

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void DeleteEmployeeWithDetails(int employeeId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    using (var deleteEmployeeDetailsCommand = new SqlCommand("DELETE FROM EmployeeDetails WHERE EmployeeID = @EmployeeID;", connection, transaction))
                    {
                        deleteEmployeeDetailsCommand.Parameters.AddWithValue("@EmployeeID", employeeId);
                        deleteEmployeeDetailsCommand.ExecuteNonQuery();
                    }

                    using (var deleteEmployeeCommand = new SqlCommand("DELETE FROM Employees WHERE EmployeeID = @EmployeeID;", connection, transaction))
                    {
                        deleteEmployeeCommand.Parameters.AddWithValue("@EmployeeID", employeeId);
                        deleteEmployeeCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
