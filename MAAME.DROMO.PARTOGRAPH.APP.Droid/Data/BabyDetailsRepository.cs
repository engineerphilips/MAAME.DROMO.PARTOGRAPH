using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class BabyDetailsRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public BabyDetailsRepository(ILogger<BabyDetailsRepository> logger)
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
                CREATE TABLE IF NOT EXISTS Tbl_Baby (
                    ID TEXT PRIMARY KEY,
                    partographid TEXT NOT NULL,
                    birthoutcomeid TEXT,
                    babynumber INTEGER NOT NULL DEFAULT 1,
                    babytag TEXT,
                    birthtime TEXT NOT NULL,
                    sex INTEGER NOT NULL DEFAULT 3,
                    vitalstatus INTEGER NOT NULL DEFAULT 0,
                    deathtime TEXT,
                    deathcause TEXT,
                    stillbirthmacerated INTEGER NOT NULL DEFAULT 0,
                    birthweight REAL NOT NULL DEFAULT 0,
                    length REAL NOT NULL DEFAULT 0,
                    headcircumference REAL NOT NULL DEFAULT 0,
                    chestcircumference REAL NOT NULL DEFAULT 0,
                    apgar1min INTEGER,
                    apgar5min INTEGER,
                    apgar10min INTEGER,
                    apgar1heartrate INTEGER,
                    apgar1respiratoryeffort INTEGER,
                    apgar1muscletone INTEGER,
                    apgar1reflexirritability INTEGER,
                    apgar1color INTEGER,
                    apgar5heartrate INTEGER,
                    apgar5respiratoryeffort INTEGER,
                    apgar5muscletone INTEGER,
                    apgar5reflexirritability INTEGER,
                    apgar5color INTEGER,
                    resuscitationrequired INTEGER NOT NULL DEFAULT 0,
                    resuscitationsteps TEXT,
                    resuscitationduration INTEGER,
                    oxygengiven INTEGER NOT NULL DEFAULT 0,
                    intubationperformed INTEGER NOT NULL DEFAULT 0,
                    chestcompressionsgiven INTEGER NOT NULL DEFAULT 0,
                    medicationsgiven INTEGER NOT NULL DEFAULT 0,
                    medicationdetails TEXT,
                    earlybreastfeedinginitiated INTEGER NOT NULL DEFAULT 1,
                    delayedcordclamping INTEGER NOT NULL DEFAULT 1,
                    cordclampingtime INTEGER,
                    vitaminkgiven INTEGER NOT NULL DEFAULT 1,
                    eyeprophylaxisgiven INTEGER NOT NULL DEFAULT 1,
                    hepatitisbvaccinegiven INTEGER NOT NULL DEFAULT 0,
                    firsttemperature REAL,
                    kangaroomothercare INTEGER NOT NULL DEFAULT 0,
                    weightclassification INTEGER NOT NULL DEFAULT 3,
                    gestationalclassification INTEGER NOT NULL DEFAULT 4,
                    congenitalabnormalitiespresent INTEGER NOT NULL DEFAULT 0,
                    congenitalabnormalitiesdescription TEXT,
                    birthinjuriespresent INTEGER NOT NULL DEFAULT 0,
                    birthinjuriesdescription TEXT,
                    requiresspecialcare INTEGER NOT NULL DEFAULT 0,
                    specialcarereason TEXT,
                    admittedtonicu INTEGER NOT NULL DEFAULT 0,
                    nicuadmissiontime TEXT,
                    feedingmethod INTEGER NOT NULL DEFAULT 0,
                    feedingnotes TEXT,
                    asphyxianeonatorum INTEGER NOT NULL DEFAULT 0,
                    respiratorydistresssyndrome INTEGER NOT NULL DEFAULT 0,
                    sepsis INTEGER NOT NULL DEFAULT 0,
                    jaundice INTEGER NOT NULL DEFAULT 0,
                    hypothermia INTEGER NOT NULL DEFAULT 0,
                    hypoglycemia INTEGER NOT NULL DEFAULT 0,
                    othercomplications TEXT, 
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
                    FOREIGN KEY (partographid) REFERENCES Tbl_Partograph(ID),
                    FOREIGN KEY (birthoutcomeid) REFERENCES Tbl_BirthOutcome(ID)
                );

                CREATE INDEX IF NOT EXISTS idx_babydetails_partographid ON Tbl_Baby(partographid);
                CREATE INDEX IF NOT EXISTS idx_babydetails_birthoutcomeid ON Tbl_Baby(birthoutcomeid);
                CREATE INDEX IF NOT EXISTS idx_babydetails_sync ON Tbl_Baby(updatedtime, syncstatus);

                DROP TRIGGER IF EXISTS trg_baby_insert;
                CREATE TRIGGER trg_baby_insert 
                AFTER INSERT ON Tbl_Baby
                WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
                BEGIN
                    UPDATE Tbl_Baby 
                    SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                        updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                    WHERE ID = NEW.ID;
                END;

                DROP TRIGGER IF EXISTS trg_baby_update;
                CREATE TRIGGER trg_baby_update 
                AFTER UPDATE ON Tbl_Baby
                WHEN NEW.updatedtime = OLD.updatedtime
                BEGIN
                    UPDATE Tbl_Baby 
                    SET updatedtime = (strftime('%s', 'now') * 1000),
                        version = OLD.version + 1,
                        syncstatus = 0
                    WHERE ID = NEW.ID;
                END;
                ";

                //skintoskincontact INTEGER NOT NULL DEFAULT 1,
                //breathing INTEGER NOT NULL DEFAULT 1,
                //    crying INTEGER NOT NULL DEFAULT 1,
                //    goodmuscletone INTEGER NOT NULL DEFAULT 1,
                //    skincolor INTEGER NOT NULL DEFAULT 0,
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating BabyDetails table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<BabyDetails>> GetByPartographIdAsync(Guid? partographId)
        {
            await Init();
            var babies = new List<BabyDetails>();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM Tbl_Baby WHERE partographid = @partographid AND deleted = 0 ORDER BY babynumber";
                selectCmd.Parameters.AddWithValue("@partographid", partographId.ToString());

                await using var reader = await selectCmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    babies.Add(MapFromReader(reader));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting baby details");
                throw;
            }

            return babies;
        }

        public async Task<List<BabyDetails>> GetByBirthOutcomeIdAsync(Guid? birthOutcomeId)
        {
            await Init();
            var babies = new List<BabyDetails>();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM Tbl_Baby WHERE birthoutcomeid = @birthoutcomeid AND deleted = 0 ORDER BY babynumber";
                selectCmd.Parameters.AddWithValue("@birthoutcomeid", birthOutcomeId.ToString());

                await using var reader = await selectCmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    babies.Add(MapFromReader(reader));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting baby details by birth outcome");
                throw;
            }

            return babies;
        }

        public async Task<Guid?> SaveItemAsync(BabyDetails item)
        {
            await Init();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var saveCmd = connection.CreateCommand();
                if (item.ID == null || item.ID == Guid.Empty)
                {
                    item.ID = Guid.NewGuid();
                    item.CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    item.UpdatedTime = item.CreatedTime;
                    item.DataHash = item.CalculateHash();

                    saveCmd.CommandText = GetInsertSql();
                }
                else
                {
                    item.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    item.Version++;
                    item.DataHash = item.CalculateHash();

                    saveCmd.CommandText = GetUpdateSql();
                }

                AddParameters(saveCmd, item);
                if (await saveCmd.ExecuteNonQueryAsync() > 0)
                {
                    var x = item.ID;
                }
                else
                    item.ID = null;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving baby details");
                throw;
            }

            return item.ID;
        }

        private string GetInsertSql()
        {
            return @"
            INSERT INTO Tbl_BabyDetails (
                ID, partographid, birthoutcomeid, babynumber, babytag, birthtime, sex, vitalstatus,
                deathtime, deathcause, stillbirthmacerated, birthweight, length, headcircumference,
                chestcircumference, apgar1min, apgar5min, apgar10min, resuscitationrequired,
                resuscitationsteps, resuscitationduration, oxygengiven, intubationperformed,
                chestcompressionsgiven, medicationsgiven, medicationdetails,
                earlybreastfeedinginitiated, delayedcordclamping, cordclampingtime, vitaminkgiven,
                eyeprophylaxisgiven, hepatitisbvaccinegiven, firsttemperature, kangaroomothercare,
                weightclassification, gestationalclassification, congenitalabnormalitiespresent,
                congenitalabnormalitiesdescription, birthinjuriespresent, birthinjuriesdescription,
                breathing, crying, goodmuscletone, skincolor, requiresspecialcare, specialcarereason,
                admittedtonicu, nicuadmissiontime, feedingmethod, feedingnotes, asphyxianeonatorum,
                respiratorydistresssyndrome, sepsis, jaundice, hypothermia, hypoglycemia,
                othercomplications, handler, notes, createdtime, updatedtime,
                deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash
            ) VALUES (
                @id, @partographid, @birthoutcomeid, @babynumber, @babytag, @birthtime, @sex, @vitalstatus,
                @deathtime, @deathcause, @stillbirthmacerated, @birthweight, @length, @headcircumference,
                @chestcircumference, @apgar1min, @apgar5min, @apgar10min, @resuscitationrequired,
                @resuscitationsteps, @resuscitationduration, @oxygengiven, @intubationperformed,
                @chestcompressionsgiven, @medicationsgiven, @medicationdetails,
                @earlybreastfeedinginitiated, @delayedcordclamping, @cordclampingtime, @vitaminkgiven,
                @eyeprophylaxisgiven, @hepatitisbvaccinegiven, @firsttemperature, @kangaroomothercare,
                @weightclassification, @gestationalclassification, @congenitalabnormalitiespresent,
                @congenitalabnormalitiesdescription, @birthinjuriespresent, @birthinjuriesdescription,
                @breathing, @crying, @goodmuscletone, @skincolor, @requiresspecialcare, @specialcarereason,
                @admittedtonicu, @nicuadmissiontime, @feedingmethod, @feedingnotes, @asphyxianeonatorum,
                @respiratorydistresssyndrome, @sepsis, @jaundice, @hypothermia, @hypoglycemia,
                @othercomplications, @handler, @notes, @createdtime, @updatedtime,
                @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash
            )";
        }

        private string GetUpdateSql()
        {
            return @"
            UPDATE Tbl_Baby SET
                babynumber = @babynumber, babytag = @babytag, birthtime = @birthtime, sex = @sex,
                vitalstatus = @vitalstatus, deathtime = @deathtime, deathcause = @deathcause,
                stillbirthmacerated = @stillbirthmacerated, birthweight = @birthweight, length = @length,
                headcircumference = @headcircumference, chestcircumference = @chestcircumference,
                apgar1min = @apgar1min, apgar5min = @apgar5min, apgar10min = @apgar10min,
                apgar1heartrate = @apgar1heartrate, apgar1respiratoryeffort = @apgar1respiratoryeffort,
                apgar1muscletone = @apgar1muscletone, apgar1reflexirritability = @apgar1reflexirritability,
                apgar1color = @apgar1color, apgar5heartrate = @apgar5heartrate,
                apgar5respiratoryeffort = @apgar5respiratoryeffort, apgar5muscletone = @apgar5muscletone,
                apgar5reflexirritability = @apgar5reflexirritability, apgar5color = @apgar5color,
                resuscitationrequired = @resuscitationrequired, resuscitationsteps = @resuscitationsteps,
                resuscitationduration = @resuscitationduration, oxygengiven = @oxygengiven,
                intubationperformed = @intubationperformed, chestcompressionsgiven = @chestcompressionsgiven,
                medicationsgiven = @medicationsgiven, medicationdetails = @medicationdetails,
                earlybreastfeedinginitiated = @earlybreastfeedinginitiated,
                delayedcordclamping = @delayedcordclamping, cordclampingtime = @cordclampingtime,
                vitaminkgiven = @vitaminkgiven, eyeprophylaxisgiven = @eyeprophylaxisgiven,
                hepatitisbvaccinegiven = @hepatitisbvaccinegiven, firsttemperature = @firsttemperature,
                kangaroomothercare = @kangaroomothercare, weightclassification = @weightclassification,
                gestationalclassification = @gestationalclassification,
                congenitalabnormalitiespresent = @congenitalabnormalitiespresent,
                congenitalabnormalitiesdescription = @congenitalabnormalitiesdescription,
                birthinjuriespresent = @birthinjuriespresent, birthinjuriesdescription = @birthinjuriesdescription,
                breathing = @breathing, crying = @crying, goodmuscletone = @goodmuscletone,
                skincolor = @skincolor, requiresspecialcare = @requiresspecialcare,
                specialcarereason = @specialcarereason, admittedtonicu = @admittedtonicu,
                nicuadmissiontime = @nicuadmissiontime, feedingmethod = @feedingmethod,
                feedingnotes = @feedingnotes, asphyxianeonatorum = @asphyxianeonatorum,
                respiratorydistresssyndrome = @respiratorydistresssyndrome, sepsis = @sepsis,
                jaundice = @jaundice, hypothermia = @hypothermia, hypoglycemia = @hypoglycemia,
                othercomplications = @othercomplications, handler = @handler,
                notes = @notes, updatedtime = @updatedtime, syncstatus = 0, version = @version,
                datahash = @datahash
            WHERE ID = @id";
        }

        private void AddParameters(SqliteCommand cmd, BabyDetails item)
        {
            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@partographid", item.PartographID?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@birthoutcomeid", item.BirthOutcomeID?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@babynumber", item.BabyNumber);
            cmd.Parameters.AddWithValue("@babytag", item.BabyTag ?? string.Empty);
            cmd.Parameters.AddWithValue("@birthtime", item.BirthTime.ToString("o"));
            cmd.Parameters.AddWithValue("@sex", (int)item.Sex);
            cmd.Parameters.AddWithValue("@vitalstatus", (int)item.VitalStatus);
            cmd.Parameters.AddWithValue("@deathtime", item.DeathTime?.ToString("o") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@deathcause", item.DeathCause ?? string.Empty);
            cmd.Parameters.AddWithValue("@stillbirthmacerated", item.StillbirthMacerated ? 1 : 0);
            cmd.Parameters.AddWithValue("@birthweight", item.BirthWeight);
            cmd.Parameters.AddWithValue("@length", item.Length);
            cmd.Parameters.AddWithValue("@headcircumference", item.HeadCircumference);
            cmd.Parameters.AddWithValue("@chestcircumference", item.ChestCircumference);
            cmd.Parameters.AddWithValue("@apgar1min", item.Apgar1Min.HasValue ? item.Apgar1Min.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@apgar5min", item.Apgar5Min.HasValue ? item.Apgar5Min.Value : (object)DBNull.Value);
            //cmd.Parameters.AddWithValue("@apgar10min", item.Apgar10Min.HasValue ? item.Apgar10Min.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@apgar1heartrate", item.Apgar1HeartRate.HasValue ? item.Apgar1HeartRate.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@apgar1respiratoryeffort", item.Apgar1RespiratoryEffort.HasValue ? item.Apgar1RespiratoryEffort.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@apgar1muscletone", item.Apgar1MuscleTone.HasValue ? item.Apgar1MuscleTone.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@apgar1reflexirritability", item.Apgar1ReflexIrritability.HasValue ? item.Apgar1ReflexIrritability.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@apgar1color", item.Apgar1Color.HasValue ? item.Apgar1Color.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@apgar5heartrate", item.Apgar5HeartRate.HasValue ? item.Apgar5HeartRate.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@apgar5respiratoryeffort", item.Apgar5RespiratoryEffort.HasValue ? item.Apgar5RespiratoryEffort.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@apgar5muscletone", item.Apgar5MuscleTone.HasValue ? item.Apgar5MuscleTone.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@apgar5reflexirritability", item.Apgar5ReflexIrritability.HasValue ? item.Apgar5ReflexIrritability.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@apgar5color", item.Apgar5Color.HasValue ? item.Apgar5Color.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@resuscitationrequired", item.ResuscitationRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@resuscitationsteps", item.ResuscitationSteps ?? string.Empty);
            cmd.Parameters.AddWithValue("@resuscitationduration", item.ResuscitationDuration.HasValue ? item.ResuscitationDuration.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@oxygengiven", item.OxygenGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@intubationperformed", item.IntubationPerformed ? 1 : 0);
            cmd.Parameters.AddWithValue("@chestcompressionsgiven", item.ChestCompressionsGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@medicationsgiven", item.MedicationsGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@medicationdetails", item.MedicationDetails ?? string.Empty);
            //cmd.Parameters.AddWithValue("@skintoskincontact", item.SkinToSkinContact ? 1 : 0);
            cmd.Parameters.AddWithValue("@earlybreastfeedinginitiated", item.EarlyBreastfeedingInitiated ? 1 : 0);
            cmd.Parameters.AddWithValue("@delayedcordclamping", item.DelayedCordClamping ? 1 : 0);
            cmd.Parameters.AddWithValue("@cordclampingtime", item.CordClampingTime.HasValue ? item.CordClampingTime.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@vitaminkgiven", item.VitaminKGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@eyeprophylaxisgiven", item.EyeProphylaxisGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@hepatitisbvaccinegiven", item.HepatitisBVaccineGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@firsttemperature", item.FirstTemperature.HasValue ? item.FirstTemperature.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@kangaroomothercare", item.KangarooMotherCare ? 1 : 0);
            cmd.Parameters.AddWithValue("@weightclassification", (int)item.WeightClassification);
            cmd.Parameters.AddWithValue("@gestationalclassification", (int)item.GestationalClassification);
            cmd.Parameters.AddWithValue("@congenitalabnormalitiespresent", item.CongenitalAbnormalitiesPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@congenitalabnormalitiesdescription", item.CongenitalAbnormalitiesDescription ?? string.Empty);
            cmd.Parameters.AddWithValue("@birthinjuriespresent", item.BirthInjuriesPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@birthinjuriesdescription", item.BirthInjuriesDescription ?? string.Empty);
            //cmd.Parameters.AddWithValue("@breathing", item.Breathing ? 1 : 0);
            //cmd.Parameters.AddWithValue("@crying", item.Crying ? 1 : 0);
            //cmd.Parameters.AddWithValue("@goodmuscletone", item.GoodMuscleTone ? 1 : 0);
            //cmd.Parameters.AddWithValue("@skincolor", (int)item.SkinColor);
            cmd.Parameters.AddWithValue("@requiresspecialcare", item.RequiresSpecialCare ? 1 : 0);
            cmd.Parameters.AddWithValue("@specialcarereason", item.SpecialCareReason ?? string.Empty);
            cmd.Parameters.AddWithValue("@admittedtonicu", item.AdmittedToNICU ? 1 : 0);
            cmd.Parameters.AddWithValue("@nicuadmissiontime", item.NICUAdmissionTime?.ToString("o") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@feedingmethod", (int)item.FeedingMethod);
            cmd.Parameters.AddWithValue("@feedingnotes", item.FeedingNotes ?? string.Empty);
            cmd.Parameters.AddWithValue("@asphyxianeonatorum", item.AsphyxiaNeonatorum ? 1 : 0);
            cmd.Parameters.AddWithValue("@respiratorydistresssyndrome", item.RespiratorydistressSyndrome ? 1 : 0);
            cmd.Parameters.AddWithValue("@sepsis", item.Sepsis ? 1 : 0);
            cmd.Parameters.AddWithValue("@jaundice", item.Jaundice ? 1 : 0);
            cmd.Parameters.AddWithValue("@hypothermia", item.Hypothermia ? 1 : 0);
            cmd.Parameters.AddWithValue("@hypoglycemia", item.Hypoglycemia ? 1 : 0);
            cmd.Parameters.AddWithValue("@othercomplications", item.OtherComplications ?? string.Empty); 
            cmd.Parameters.AddWithValue("@handler", item.Handler?.ToString() ?? (object)DBNull.Value);
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

        private BabyDetails MapFromReader(SqliteDataReader reader)
        {
            return new BabyDetails
            {
                ID = Guid.Parse(reader["ID"].ToString()),
                PartographID = reader["partographid"] == DBNull.Value ? null : Guid.Parse(reader["partographid"].ToString()),
                BirthOutcomeID = reader["birthoutcomeid"] == DBNull.Value ? null : Guid.Parse(reader["birthoutcomeid"].ToString()),
                BabyNumber = Convert.ToInt32(reader["babynumber"]),
                BabyTag = reader["babytag"]?.ToString() ?? string.Empty,
                BirthTime = DateTime.Parse(reader["birthtime"].ToString()),
                Sex = (BabySex)Convert.ToInt32(reader["sex"]),
                VitalStatus = (BabyVitalStatus)Convert.ToInt32(reader["vitalstatus"]),
                DeathTime = reader["deathtime"] == DBNull.Value ? null : DateTime.Parse(reader["deathtime"].ToString()),
                DeathCause = reader["deathcause"]?.ToString() ?? string.Empty,
                StillbirthMacerated = Convert.ToBoolean(reader["stillbirthmacerated"]),
                BirthWeight = Convert.ToDecimal(reader["birthweight"]),
                Length = Convert.ToDecimal(reader["length"]),
                HeadCircumference = Convert.ToDecimal(reader["headcircumference"]),
                ChestCircumference = Convert.ToDecimal(reader["chestcircumference"]),
                Apgar1Min = reader["apgar1min"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar1min"]),
                Apgar5Min = reader["apgar5min"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar5min"]),
                //Apgar10Min = reader["apgar10min"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar10min"]),
                Apgar1HeartRate = reader["apgar1heartrate"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar1heartrate"]),
                Apgar1RespiratoryEffort = reader["apgar1respiratoryeffort"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar1respiratoryeffort"]),
                Apgar1MuscleTone = reader["apgar1muscletone"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar1muscletone"]),
                Apgar1ReflexIrritability = reader["apgar1reflexirritability"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar1reflexirritability"]),
                Apgar1Color = reader["apgar1color"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar1color"]),
                Apgar5HeartRate = reader["apgar5heartrate"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar5heartrate"]),
                Apgar5RespiratoryEffort = reader["apgar5respiratoryeffort"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar5respiratoryeffort"]),
                Apgar5MuscleTone = reader["apgar5muscletone"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar5muscletone"]),
                Apgar5ReflexIrritability = reader["apgar5reflexirritability"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar5reflexirritability"]),
                Apgar5Color = reader["apgar5color"] == DBNull.Value ? null : Convert.ToInt32(reader["apgar5color"]),
                ResuscitationRequired = Convert.ToBoolean(reader["resuscitationrequired"]),
                ResuscitationSteps = reader["resuscitationsteps"]?.ToString() ?? string.Empty,
                ResuscitationDuration = reader["resuscitationduration"] == DBNull.Value ? null : Convert.ToInt32(reader["resuscitationduration"]),
                OxygenGiven = Convert.ToBoolean(reader["oxygengiven"]),
                IntubationPerformed = Convert.ToBoolean(reader["intubationperformed"]),
                ChestCompressionsGiven = Convert.ToBoolean(reader["chestcompressionsgiven"]),
                MedicationsGiven = Convert.ToBoolean(reader["medicationsgiven"]),
                MedicationDetails = reader["medicationdetails"]?.ToString() ?? string.Empty,
                //SkinToSkinContact = Convert.ToBoolean(reader["skintoskincontact"]),
                EarlyBreastfeedingInitiated = Convert.ToBoolean(reader["earlybreastfeedinginitiated"]),
                DelayedCordClamping = Convert.ToBoolean(reader["delayedcordclamping"]),
                CordClampingTime = reader["cordclampingtime"] == DBNull.Value ? null : Convert.ToInt32(reader["cordclampingtime"]),
                VitaminKGiven = Convert.ToBoolean(reader["vitaminkgiven"]),
                EyeProphylaxisGiven = Convert.ToBoolean(reader["eyeprophylaxisgiven"]),
                HepatitisBVaccineGiven = Convert.ToBoolean(reader["hepatitisbvaccinegiven"]),
                FirstTemperature = reader["firsttemperature"] == DBNull.Value ? null : Convert.ToDecimal(reader["firsttemperature"]),
                KangarooMotherCare = Convert.ToBoolean(reader["kangaroomothercare"]),
                WeightClassification = (BirthWeightClassification)Convert.ToInt32(reader["weightclassification"]),
                GestationalClassification = (GestationalAgeClassification)Convert.ToInt32(reader["gestationalclassification"]),
                CongenitalAbnormalitiesPresent = Convert.ToBoolean(reader["congenitalabnormalitiespresent"]),
                CongenitalAbnormalitiesDescription = reader["congenitalabnormalitiesdescription"]?.ToString() ?? string.Empty,
                BirthInjuriesPresent = Convert.ToBoolean(reader["birthinjuriespresent"]),
                BirthInjuriesDescription = reader["birthinjuriesdescription"]?.ToString() ?? string.Empty,
                //Breathing = Convert.ToBoolean(reader["breathing"]),
                //Crying = Convert.ToBoolean(reader["crying"]),
                //GoodMuscleTone = Convert.ToBoolean(reader["goodmuscletone"]),
                //SkinColor = (SkinColor)Convert.ToInt32(reader["skincolor"]),
                RequiresSpecialCare = Convert.ToBoolean(reader["requiresspecialcare"]),
                SpecialCareReason = reader["specialcarereason"]?.ToString() ?? string.Empty,
                AdmittedToNICU = Convert.ToBoolean(reader["admittedtonicu"]),
                NICUAdmissionTime = reader["nicuadmissiontime"] == DBNull.Value ? null : DateTime.Parse(reader["nicuadmissiontime"].ToString()),
                FeedingMethod = (FeedingMethod)Convert.ToInt32(reader["feedingmethod"]),
                FeedingNotes = reader["feedingnotes"]?.ToString() ?? string.Empty,
                AsphyxiaNeonatorum = Convert.ToBoolean(reader["asphyxianeonatorum"]),
                RespiratorydistressSyndrome = Convert.ToBoolean(reader["respiratorydistresssyndrome"]),
                Sepsis = Convert.ToBoolean(reader["sepsis"]),
                Jaundice = Convert.ToBoolean(reader["jaundice"]),
                Hypothermia = Convert.ToBoolean(reader["hypothermia"]),
                Hypoglycemia = Convert.ToBoolean(reader["hypoglycemia"]),
                OtherComplications = reader["othercomplications"]?.ToString() ?? string.Empty,
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
                ConflictData = reader["conflictdata"]?.ToString(),
                DataHash = reader["datahash"]?.ToString()
            };
        }
    }
}
