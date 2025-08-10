using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class StaffRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public StaffRepository(ILogger<StaffRepository> logger)
        {
            _logger = logger;
        }

        private async Task Init()
        {
            if (_hasBeenInitialized)
                return;

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Staff (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    StaffID TEXT NOT NULL UNIQUE,
                    Email TEXT NOT NULL UNIQUE,
                    Role TEXT NOT NULL,
                    Department TEXT NOT NULL,
                    Password TEXT NOT NULL,
                    LastLogin TEXT,
                    IsActive INTEGER NOT NULL
                );";
                await createTableCmd.ExecuteNonQueryAsync();

                // Insert default staff for demo
                await InsertDefaultStaff();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating Staff table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        private async Task InsertDefaultStaff()
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Staff";
            var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

            if (count == 0)
            {
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                INSERT INTO Staff (Name, StaffID, Email, Role, Department, Password, IsActive)
                VALUES 
                ('Dr. Sarah Johnson', 'DOC001', 'sarah.johnson@hospital.com', 'Doctor', 'Labor Ward', 'password123', 1),
                ('Midwife Emma Brown', 'MW001', 'emma.brown@hospital.com', 'Midwife', 'Labor Ward', 'password123', 1),
                ('Nurse David Wilson', 'NUR001', 'david.wilson@hospital.com', 'Nurse', 'Labor Ward', 'password123', 1);";
                await insertCmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<Staff?> AuthenticateAsync(string emailOrStaffId, string password)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT * FROM Staff 
                WHERE (Email = @identifier OR StaffID = @identifier) 
                AND Password = @password 
                AND IsActive = 1";
            selectCmd.Parameters.AddWithValue("@identifier", emailOrStaffId);
            selectCmd.Parameters.AddWithValue("@password", password); // In production, use hashed passwords

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var staff = new Staff
                {
                    ID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    StaffID = reader.GetString(2),
                    Email = reader.GetString(3),
                    Role = reader.GetString(4),
                    Department = reader.GetString(5),
                    Password = reader.GetString(6),
                    LastLogin = reader.IsDBNull(7) ? DateTime.Now : DateTime.Parse(reader.GetString(7)),
                    IsActive = reader.GetBoolean(8)
                };

                // Update last login
                var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = "UPDATE Staff SET LastLogin = @lastLogin WHERE ID = @id";
                updateCmd.Parameters.AddWithValue("@lastLogin", DateTime.Now.ToString("O"));
                updateCmd.Parameters.AddWithValue("@id", staff.ID);
                await updateCmd.ExecuteNonQueryAsync();

                return staff;
            }

            return null;
        }
    }
}
