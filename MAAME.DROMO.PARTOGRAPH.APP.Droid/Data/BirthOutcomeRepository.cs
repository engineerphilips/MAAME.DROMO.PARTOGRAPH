using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class BirthOutcomeRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public BirthOutcomeRepository(ILogger<BirthOutcomeRepository> logger)
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
                CREATE TABLE IF NOT EXISTS Tbl_BirthOutcome (
                    ID TEXT PRIMARY KEY,
                    partographid TEXT NOT NULL,
                    recordedtime TEXT NOT NULL,
                    maternalstatus INTEGER NOT NULL,
                    maternaldeathtime TEXT,
                    maternaldeathcause TEXT,
                    maternaldeathcircumstances TEXT,
                    deliverymode INTEGER NOT NULL,
                    deliverymodedetails TEXT,
                    deliverytime TEXT,
                    numberofbabies INTEGER NOT NULL DEFAULT 1,
                    perinealstatus INTEGER NOT NULL,
                    perinealdetails TEXT,
                    placentadeliverytime TEXT,
                    placentacomplete INTEGER NOT NULL DEFAULT 1,
                    estimatedbloodloss INTEGER NOT NULL DEFAULT 0,
                    maternalcomplications TEXT,
                    postpartumhemorrhage INTEGER NOT NULL DEFAULT 0,
                    eclampsia INTEGER NOT NULL DEFAULT 0,
                    septicshock INTEGER NOT NULL DEFAULT 0,
                    obstructedlabor INTEGER NOT NULL DEFAULT 0,
                    ruptureduterus INTEGER NOT NULL DEFAULT 0,
                    oxytocingiven INTEGER NOT NULL DEFAULT 1,
                    antibioticsgiven INTEGER NOT NULL DEFAULT 0,
                    bloodtransfusiongiven INTEGER NOT NULL DEFAULT 0, 
                    handler TEXT,
                    notes TEXT,
                    createdtime INTEGER NOT NULL,
                    updatedtime INTEGER NOT NULL,
                    deletedtime INTEGER,
                    deviceid TEXT NOT NULL,
                    origindeviceid TEXT NOT NULL,
                    syncstatus INTEGER DEFAULT 0,
                    version INTEGER DEFAULT 1,
                    serverversion INTEGER DEFAULT 0,
                    deleted INTEGER DEFAULT 0,
                    conflictdata TEXT,
                    datahash TEXT,
                    FOREIGN KEY (partographid) REFERENCES Tbl_Partograph(ID)
                );

                CREATE INDEX IF NOT EXISTS idx_birthoutcome_partographid ON Tbl_BirthOutcome(partographid);
                CREATE INDEX IF NOT EXISTS idx_birthoutcome_sync ON Tbl_BirthOutcome(updatedtime, syncstatus);
                
                DROP TRIGGER IF EXISTS trg_birthoutcome_insert;
                CREATE TRIGGER trg_birthoutcome_insert
                AFTER INSERT ON Tbl_BirthOutcome
                WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
                BEGIN
                    UPDATE Tbl_BirthOutcome 
                    SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                        updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                    WHERE ID = NEW.ID;
                END;

                DROP TRIGGER IF EXISTS trg_birthoutcome_update;
                CREATE TRIGGER trg_birthoutcome_update 
                AFTER UPDATE ON Tbl_BirthOutcome
                WHEN NEW.updatedtime = OLD.updatedtime
                BEGIN
                    UPDATE Tbl_BirthOutcome
                    SET updatedtime = (strftime('%s', 'now') * 1000),
                        version = OLD.version + 1,
                        syncstatus = 0
                    WHERE ID = NEW.ID;
                END;";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating BirthOutcome table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<BirthOutcome?> GetByPartographIdAsync(Guid? partographId)
        {
            await Init();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM Tbl_BirthOutcome WHERE partographid = @partographid AND deleted = 0";
                selectCmd.Parameters.AddWithValue("@partographid", partographId.ToString());

                await using var reader = await selectCmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapFromReader(reader);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting birth outcome");
                throw;
            }

            return null;
        }

        public async Task<List<BirthOutcome>> ListAsync()
        {
            await Init();
            var results = new List<BirthOutcome>();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM Tbl_BirthOutcome WHERE deleted = 0";
                //selectCmd.Parameters.AddWithValue("@partographid", partographId.ToString());

                await using var reader = await selectCmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(MapFromReader(reader));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting birth outcome");
                throw;
            }

            return results;
        }

        public async Task<Guid?> SaveItemAsync(BirthOutcome item)
        {
            await Init();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var saveCmd = connection.CreateCommand();
                if (item.ID == null || item.ID == Guid.Empty)
                {
                    var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    item.ID = item.ID ?? Guid.NewGuid();
                    item.CreatedTime = now;
                    item.UpdatedTime = now;
                    item.DeviceId = DeviceIdentity.GetOrCreateDeviceId();
                    item.OriginDeviceId = DeviceIdentity.GetOrCreateDeviceId();
                    item.Version = 1;
                    item.ServerVersion = 0;
                    item.SyncStatus = 0;
                    item.Deleted = 0;
                    item.DataHash = item.CalculateHash();

                    saveCmd.CommandText = @"INSERT INTO Tbl_BirthOutcome (ID, partographid, recordedtime, maternalstatus, maternaldeathtime,
                        maternaldeathcause, maternaldeathcircumstances, deliverymode, deliverymodedetails,
                        deliverytime, numberofbabies, perinealstatus, perinealdetails, placentadeliverytime, placentacomplete, estimatedbloodloss,
                        maternalcomplications, postpartumhemorrhage, eclampsia, septicshock,
                        obstructedlabor, ruptureduterus, oxytocingiven, antibioticsgiven, bloodtransfusiongiven, handler, notes,
                        createdtime, updatedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash) VALUES (
                        @id, @partographid, @recordedtime, @maternalstatus, @maternaldeathtime,
                        @maternaldeathcause, @maternaldeathcircumstances, @deliverymode, @deliverymodedetails,
                        @deliverytime, @numberofbabies, @perinealstatus, @perinealdetails,
                        @placentadeliverytime, @placentacomplete, @estimatedbloodloss,
                        @maternalcomplications, @postpartumhemorrhage, @eclampsia, @septicshock,
                        @obstructedlabor, @ruptureduterus, @oxytocingiven, @antibioticsgiven,
                        @bloodtransfusiongiven, @handler, @notes,
                        @createdtime, @updatedtime, @deviceid, @origindeviceid, @syncstatus,
                        @version, @serverversion, @deleted, @datahash
                    )";
                }
                else
                {
                    item.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    item.Version++;
                    item.DataHash = item.CalculateHash();

                    saveCmd.CommandText = @"
                    UPDATE Tbl_BirthOutcome SET
                        recordedtime = @recordedtime, maternalstatus = @maternalstatus,
                        maternaldeathtime = @maternaldeathtime, maternaldeathcause = @maternaldeathcause,
                        maternaldeathcircumstances = @maternaldeathcircumstances,
                        deliverymode = @deliverymode, deliverymodedetails = @deliverymodedetails,
                        deliverytime = @deliverytime, numberofbabies = @numberofbabies,
                        perinealstatus = @perinealstatus, perinealdetails = @perinealdetails,
                        placentadeliverytime = @placentadeliverytime, placentacomplete = @placentacomplete,
                        estimatedbloodloss = @estimatedbloodloss, maternalcomplications = @maternalcomplications,
                        postpartumhemorrhage = @postpartumhemorrhage, eclampsia = @eclampsia,
                        septicshock = @septicshock, obstructedlabor = @obstructedlabor,
                        ruptureduterus = @ruptureduterus, oxytocingiven = @oxytocingiven,
                        antibioticsgiven = @antibioticsgiven, bloodtransfusiongiven = @bloodtransfusiongiven,
                        handler = @handler, notes = @notes, updatedtime = @updatedtime, syncstatus = 0, version = @version,
                        datahash = @datahash
                    WHERE ID = @id";
                }

                AddParameters(saveCmd, item);
                if (await saveCmd.ExecuteNonQueryAsync() > 0)
                {
                    var x = item.ID;
                }
                else
                    item.ID = null;
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Error saving birth outcome");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving birth outcome");
                throw;
            }

            return item.ID;
        }

        private void AddParameters(SqliteCommand cmd, BirthOutcome item)
        {
            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@partographid", item.PartographID.ToString());
            cmd.Parameters.AddWithValue("@recordedtime", item.RecordedTime.ToString());
            cmd.Parameters.AddWithValue("@maternalstatus", (int)item.MaternalStatus);
            cmd.Parameters.AddWithValue("@maternaldeathtime", item.MaternalDeathTime != null ? item.MaternalDeathTime?.ToString() : DBNull.Value);
            cmd.Parameters.AddWithValue("@maternaldeathcause", item.MaternalDeathCause ?? string.Empty);
            cmd.Parameters.AddWithValue("@maternaldeathcircumstances", item.MaternalDeathCircumstances ?? string.Empty);
            cmd.Parameters.AddWithValue("@deliverymode", (int)item.DeliveryMode);
            cmd.Parameters.AddWithValue("@deliverymodedetails", item.DeliveryModeDetails ?? string.Empty);
            cmd.Parameters.AddWithValue("@deliverytime", item.DeliveryTime != null ? item.DeliveryTime?.ToString() : DBNull.Value);
            cmd.Parameters.AddWithValue("@numberofbabies", item.NumberOfBabies);
            cmd.Parameters.AddWithValue("@perinealstatus", (int)item.PerinealStatus);
            cmd.Parameters.AddWithValue("@perinealdetails", item.PerinealDetails ?? string.Empty);
            cmd.Parameters.AddWithValue("@placentadeliverytime", item.PlacentaDeliveryTime != null ? item.PlacentaDeliveryTime?.ToString() : DBNull.Value);
            cmd.Parameters.AddWithValue("@placentacomplete", item.PlacentaComplete ? 1 : 0);
            cmd.Parameters.AddWithValue("@estimatedbloodloss", item.EstimatedBloodLoss);
            cmd.Parameters.AddWithValue("@maternalcomplications", item.MaternalComplications ?? string.Empty);
            cmd.Parameters.AddWithValue("@postpartumhemorrhage", item.PostpartumHemorrhage ? 1 : 0);
            cmd.Parameters.AddWithValue("@eclampsia", item.Eclampsia ? 1 : 0);
            cmd.Parameters.AddWithValue("@septicshock", item.SepticShock ? 1 : 0);
            cmd.Parameters.AddWithValue("@obstructedlabor", item.ObstructedLabor ? 1 : 0);
            cmd.Parameters.AddWithValue("@ruptureduterus", item.RupturedUterus ? 1 : 0);
            cmd.Parameters.AddWithValue("@oxytocingiven", item.OxytocinGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@antibioticsgiven", item.AntibioticsGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@bloodtransfusiongiven", item.BloodTransfusionGiven ? 1 : 0); 
            cmd.Parameters.AddWithValue("@handler", item.Handler != null ? item.Handler?.ToString() : DBNull.Value);
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? string.Empty);
            cmd.Parameters.AddWithValue("@createdtime", item.CreatedTime);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId ?? string.Empty);
            cmd.Parameters.AddWithValue("@origindeviceid", item.OriginDeviceId ?? string.Empty);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@serverversion", item.ServerVersion);
            cmd.Parameters.AddWithValue("@deleted", item.Deleted);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash ?? string.Empty);
        }

        private BirthOutcome MapFromReader(SqliteDataReader reader)
        {
            return new BirthOutcome
            {
                ID = Guid.Parse(reader["ID"].ToString()),
                PartographID = Guid.Parse(reader["partographid"].ToString()),
                RecordedTime = DateTime.Parse(reader["recordedtime"].ToString()),
                MaternalStatus = (MaternalOutcomeStatus)Convert.ToInt32(reader["maternalstatus"]),
                MaternalDeathTime = reader["maternaldeathtime"] == DBNull.Value ? null : DateTime.Parse(reader["maternaldeathtime"].ToString()),
                MaternalDeathCause = reader["maternaldeathcause"]?.ToString() ?? string.Empty,
                MaternalDeathCircumstances = reader["maternaldeathcircumstances"]?.ToString() ?? string.Empty,
                DeliveryMode = (DeliveryMode)Convert.ToInt32(reader["deliverymode"]),
                DeliveryModeDetails = reader["deliverymodedetails"]?.ToString() ?? string.Empty,
                DeliveryTime = reader["deliverytime"] == DBNull.Value ? null : DateTime.Parse(reader["deliverytime"].ToString()),
                NumberOfBabies = Convert.ToInt32(reader["numberofbabies"]),
                PerinealStatus = (PerinealStatus)Convert.ToInt32(reader["perinealstatus"]),
                PerinealDetails = reader["perinealdetails"]?.ToString() ?? string.Empty,
                PlacentaDeliveryTime = reader["placentadeliverytime"] == DBNull.Value ? null : DateTime.Parse(reader["placentadeliverytime"].ToString()),
                PlacentaComplete = Convert.ToBoolean(reader["placentacomplete"]),
                EstimatedBloodLoss = Convert.ToInt32(reader["estimatedbloodloss"]),
                MaternalComplications = reader["maternalcomplications"]?.ToString() ?? string.Empty,
                PostpartumHemorrhage = Convert.ToBoolean(reader["postpartumhemorrhage"]),
                Eclampsia = Convert.ToBoolean(reader["eclampsia"]),
                SepticShock = Convert.ToBoolean(reader["septicshock"]),
                ObstructedLabor = Convert.ToBoolean(reader["obstructedlabor"]),
                RupturedUterus = Convert.ToBoolean(reader["ruptureduterus"]),
                OxytocinGiven = Convert.ToBoolean(reader["oxytocingiven"]),
                AntibioticsGiven = Convert.ToBoolean(reader["antibioticsgiven"]),
                BloodTransfusionGiven = Convert.ToBoolean(reader["bloodtransfusiongiven"]),
                Handler = reader["handler"] == DBNull.Value ? null : Guid.Parse(reader["handler"].ToString()),
                Notes = reader["notes"]?.ToString() ?? string.Empty,
                CreatedTime = Convert.ToInt64(reader["createdtime"]),
                UpdatedTime = Convert.ToInt64(reader["updatedtime"]),
                DeletedTime = reader["deletedtime"] == DBNull.Value ? null : Convert.ToInt64(reader["deletedtime"]),
                DeviceId = reader["deviceid"]?.ToString() ?? string.Empty,
                OriginDeviceId = reader["origindeviceid"]?.ToString() ?? string.Empty,
                SyncStatus = Convert.ToInt32(reader["syncstatus"]),
                Version = Convert.ToInt32(reader["version"]),
                ServerVersion = Convert.ToInt32(reader["serverversion"]),
                Deleted = Convert.ToInt32(reader["deleted"]),
                ConflictData = Convert.IsDBNull(reader["conflictdata"]) ? string.Empty : reader["conflictdata"]?.ToString(),
                DataHash = Convert.IsDBNull(reader["datahash"]) ? string.Empty : reader["datahash"]?.ToString()
            };
        }
    }
}
