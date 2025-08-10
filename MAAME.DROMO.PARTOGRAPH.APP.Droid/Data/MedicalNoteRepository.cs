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
    public class MedicalNoteRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public MedicalNoteRepository(ILogger<MedicalNoteRepository> logger)
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
                CREATE TABLE IF NOT EXISTS MedicalNote (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PatientID INTEGER NOT NULL,
                    CreatedTime TEXT NOT NULL,
                    NoteType TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    CreatedBy TEXT NOT NULL,
                    IsImportant INTEGER NOT NULL
                );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating MedicalNote table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<MedicalNote>> ListByPatientAsync(int patientId)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM MedicalNote WHERE PatientID = @patientId ORDER BY CreatedTime DESC";
            selectCmd.Parameters.AddWithValue("@patientId", patientId);

            var notes = new List<MedicalNote>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                notes.Add(new MedicalNote
                {
                    ID = reader.GetInt32(0),
                    PatientID = reader.GetInt32(1),
                    CreatedTime = DateTime.Parse(reader.GetString(2)),
                    NoteType = reader.GetString(3),
                    Content = reader.GetString(4),
                    CreatedBy = reader.GetString(5),
                    IsImportant = reader.GetBoolean(6)
                });
            }

            return notes;
        }

        public async Task<int> SaveItemAsync(MedicalNote item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            if (item.ID == 0)
            {
                saveCmd.CommandText = @"
                INSERT INTO MedicalNote (PatientID, CreatedTime, NoteType, Content, CreatedBy, IsImportant)
                VALUES (@PatientID, @CreatedTime, @NoteType, @Content, @CreatedBy, @IsImportant);
                SELECT last_insert_rowid();";
            }
            else
            {
                saveCmd.CommandText = @"
                UPDATE MedicalNote SET 
                    NoteType = @NoteType, Content = @Content, IsImportant = @IsImportant
                WHERE ID = @ID";
                saveCmd.Parameters.AddWithValue("@ID", item.ID);
            }

            saveCmd.Parameters.AddWithValue("@PatientID", item.PatientID);
            saveCmd.Parameters.AddWithValue("@CreatedTime", item.CreatedTime.ToString("O"));
            saveCmd.Parameters.AddWithValue("@NoteType", item.NoteType);
            saveCmd.Parameters.AddWithValue("@Content", item.Content);
            saveCmd.Parameters.AddWithValue("@CreatedBy", item.CreatedBy);
            saveCmd.Parameters.AddWithValue("@IsImportant", item.IsImportant);

            var result = await saveCmd.ExecuteScalarAsync();
            if (item.ID == 0)
            {
                item.ID = Convert.ToInt32(result);
            }

            return item.ID;
        }
    }
}
