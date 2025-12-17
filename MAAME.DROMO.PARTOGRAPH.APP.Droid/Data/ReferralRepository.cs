using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class ReferralRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public ReferralRepository(ILogger<ReferralRepository> logger)
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
                CREATE TABLE IF NOT EXISTS Tbl_Referral (
                    ID TEXT PRIMARY KEY,
                    partographid TEXT NOT NULL,
                    referraltime TEXT NOT NULL,
                    referraltype INTEGER NOT NULL,
                    urgency INTEGER NOT NULL,
                    referringfacilityname TEXT,
                    referringfacilitylevel TEXT,
                    referringphysician TEXT,
                    referringphysiciancontact TEXT,
                    destinationfacilityname TEXT,
                    destinationfacilitylevel TEXT,
                    destinationfacilitycontact TEXT,
                    destinationaddress TEXT,
                    destinationnotified INTEGER NOT NULL DEFAULT 0,
                    destinationnotificationtime TEXT,
                    destinationcontactperson TEXT,
                    prolongedlabor INTEGER NOT NULL DEFAULT 0,
                    obstructedlabor INTEGER NOT NULL DEFAULT 0,
                    foetaldistress INTEGER NOT NULL DEFAULT 0,
                    antepartumhemorrhage INTEGER NOT NULL DEFAULT 0,
                    postpartumhemorrhage INTEGER NOT NULL DEFAULT 0,
                    severepreeclampsia INTEGER NOT NULL DEFAULT 0,
                    eclampsia INTEGER NOT NULL DEFAULT 0,
                    septicshock INTEGER NOT NULL DEFAULT 0,
                    ruptureduterus INTEGER NOT NULL DEFAULT 0,
                    abnormalpresentation INTEGER NOT NULL DEFAULT 0,
                    cordprolapse INTEGER NOT NULL DEFAULT 0,
                    placentaprevia INTEGER NOT NULL DEFAULT 0,
                    placentalabruption INTEGER NOT NULL DEFAULT 0,
                    neonatalasphyxia INTEGER NOT NULL DEFAULT 0,
                    prematuritycomplications INTEGER NOT NULL DEFAULT 0,
                    lowbirthweight INTEGER NOT NULL DEFAULT 0,
                    respiratorydistress INTEGER NOT NULL DEFAULT 0,
                    congenitalabnormalities INTEGER NOT NULL DEFAULT 0,
                    neonatalsepsis INTEGER NOT NULL DEFAULT 0,
                    birthinjuries INTEGER NOT NULL DEFAULT 0,
                    lackofresources INTEGER NOT NULL DEFAULT 0,
                    requirescaesareansection INTEGER NOT NULL DEFAULT 0,
                    requiresbloodtransfusion INTEGER NOT NULL DEFAULT 0,
                    requiresspecializedcare INTEGER NOT NULL DEFAULT 0,
                    otherreasons TEXT,
                    primarydiagnosis TEXT,
                    clinicalsummary TEXT,
                    maternalcondition TEXT,
                    maternalpulse INTEGER,
                    maternalbpsystolic INTEGER,
                    maternalbpdiastolic INTEGER,
                    maternaltemperature REAL,
                    maternalconsciousness TEXT,
                    fetalheartrate INTEGER,
                    fetalcondition TEXT,
                    numberofbabiesbeingreferred INTEGER,
                    neonatalcondition TEXT,
                    cervicaldilationatreferral INTEGER,
                    membranesruptured INTEGER NOT NULL DEFAULT 0,
                    membranerupturetime TEXT,
                    liquorcolor TEXT,
                    interventionsperformed TEXT,
                    medicationsgiven TEXT,
                    ivfluidsgiven TEXT,
                    bloodsamplestaken INTEGER NOT NULL DEFAULT 0,
                    investigationsperformed TEXT,
                    transportmode INTEGER NOT NULL,
                    transportdetails TEXT,
                    departuretime TEXT,
                    arrivaltime TEXT,
                    skillfulattendantaccompanying INTEGER NOT NULL DEFAULT 1,
                    accompanyingstaffname TEXT,
                    accompanyingstaffdesignation TEXT,
                    partographsent INTEGER NOT NULL DEFAULT 1,
                    ivlineinsitu INTEGER NOT NULL DEFAULT 0,
                    catheterinsitu INTEGER NOT NULL DEFAULT 0,
                    oxygenprovided INTEGER NOT NULL DEFAULT 0,
                    equipmentsent TEXT,
                    status INTEGER NOT NULL,
                    acceptedtime TEXT,
                    completedtime TEXT,
                    outcomenotes TEXT,
                    feedbackreceived INTEGER NOT NULL DEFAULT 0,
                    feedbackdetails TEXT,
                    referralletterpath TEXT,
                    referralformgenerated INTEGER NOT NULL DEFAULT 0,
                    formgenerationtime TEXT, 
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

                CREATE INDEX IF NOT EXISTS idx_referral_partographid ON Tbl_Referral(partographid);
                CREATE INDEX IF NOT EXISTS idx_referral_status ON Tbl_Referral(status);
                CREATE INDEX IF NOT EXISTS idx_referral_sync ON Tbl_Referral(updatedtime, syncstatus);
                ";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating Referral table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<Referral>> GetByPartographIdAsync(Guid? partographId)
        {
            await Init();
            var referrals = new List<Referral>();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = @"SELECT r.*, s.name as staffname
                    FROM Tbl_Referral r
                    LEFT JOIN Tbl_Staff s ON r.handler = s.ID
                    WHERE r.partographid = @partographid AND r.deleted = 0 ORDER BY r.referraltime DESC";
                selectCmd.Parameters.AddWithValue("@partographid", partographId.ToString());

                await using var reader = await selectCmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    referrals.Add(MapFromReader(reader));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting referrals");
                throw;
            }

            return referrals;
        }

        public async Task<Referral?> GetByIdAsync(Guid? id)
        {
            await Init();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = @"SELECT r.*, s.name as staffname
                    FROM Tbl_Referral r
                    LEFT JOIN Tbl_Staff s ON r.handler = s.ID
                    WHERE r.ID = @id AND r.deleted = 0";
                selectCmd.Parameters.AddWithValue("@id", id.ToString());

                await using var reader = await selectCmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapFromReader(reader);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting referral by id");
                throw;
            }

            return null;
        }

        public async Task<Guid?> SaveItemAsync(Referral item)
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
                await saveCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving referral");
                throw;
            }

            return item.ID;
        }

        private string GetInsertSql()
        {
            return @"
            INSERT INTO Tbl_Referral (
                ID, partographid, referraltime, referraltype, urgency, referringfacilityname,
                referringfacilitylevel, referringphysician, referringphysiciancontact,
                destinationfacilityname, destinationfacilitylevel, destinationfacilitycontact,
                destinationaddress, destinationnotified, destinationnotificationtime,
                destinationcontactperson, prolongedlabor, obstructedlabor, foetaldistress,
                antepartumhemorrhage, postpartumhemorrhage, severepreeclampsia, eclampsia,
                septicshock, ruptureduterus, abnormalpresentation, cordprolapse,
                placentaprevia, placentalabruption, neonatalasphyxia, prematuritycomplications,
                lowbirthweight, respiratorydistress, congenitalabnormalities, neonatalsepsis,
                birthinjuries, lackofresources, requirescaesareansection, requiresbloodtransfusion,
                requiresspecializedcare, otherreasons, primarydiagnosis, clinicalsummary,
                maternalcondition, maternalpulse, maternalbpsystolic, maternalbpdiastolic,
                maternaltemperature, maternalconsciousness, fetalheartrate, fetalcondition,
                numberofbabiesbeingreferred, neonatalcondition, cervicaldilationatreferral,
                membranesruptured, membranerupturetime, liquorcolor, interventionsperformed,
                medicationsgiven, ivfluidsgiven, bloodsamplestaken, investigationsperformed,
                transportmode, transportdetails, departuretime, arrivaltime,
                skillfulattendantaccompanying, accompanyingstaffname, accompanyingstaffdesignation,
                partographsent, ivlineinsitu, catheterinsitu, oxygenprovided, equipmentsent,
                status, acceptedtime, completedtime, outcomenotes, feedbackreceived,
                feedbackdetails, referralletterpath, referralformgenerated, formgenerationtime,
                handler, notes, createdtime, updatedtime, deviceid, origindeviceid,
                syncstatus, version, serverversion, deleted, datahash
            ) VALUES (
                @id, @partographid, @referraltime, @referraltype, @urgency, @referringfacilityname,
                @referringfacilitylevel, @referringphysician, @referringphysiciancontact,
                @destinationfacilityname, @destinationfacilitylevel, @destinationfacilitycontact,
                @destinationaddress, @destinationnotified, @destinationnotificationtime,
                @destinationcontactperson, @prolongedlabor, @obstructedlabor, @foetaldistress,
                @antepartumhemorrhage, @postpartumhemorrhage, @severepreeclampsia, @eclampsia,
                @septicshock, @ruptureduterus, @abnormalpresentation, @cordprolapse,
                @placentaprevia, @placentalabruption, @neonatalasphyxia, @prematuritycomplications,
                @lowbirthweight, @respiratorydistress, @congenitalabnormalities, @neonatalsepsis,
                @birthinjuries, @lackofresources, @requirescaesareansection, @requiresbloodtransfusion,
                @requiresspecializedcare, @otherreasons, @primarydiagnosis, @clinicalsummary,
                @maternalcondition, @maternalpulse, @maternalbpsystolic, @maternalbpdiastolic,
                @maternaltemperature, @maternalconsciousness, @fetalheartrate, @fetalcondition,
                @numberofbabiesbeingreferred, @neonatalcondition, @cervicaldilationatreferral,
                @membranesruptured, @membranerupturetime, @liquorcolor, @interventionsperformed,
                @medicationsgiven, @ivfluidsgiven, @bloodsamplestaken, @investigationsperformed,
                @transportmode, @transportdetails, @departuretime, @arrivaltime,
                @skillfulattendantaccompanying, @accompanyingstaffname, @accompanyingstaffdesignation,
                @partographsent, @ivlineinsitu, @catheterinsitu, @oxygenprovided, @equipmentsent,
                @status, @acceptedtime, @completedtime, @outcomenotes, @feedbackreceived,
                @feedbackdetails, @referralletterpath, @referralformgenerated, @formgenerationtime,
                @handler, @notes, @createdtime, @updatedtime, @deviceid, @origindeviceid,
                @syncstatus, @version, @serverversion, @deleted, @datahash
            )";
        }

        private string GetUpdateSql()
        {
            return @"
            UPDATE Tbl_Referral SET
                referraltime = @referraltime, referraltype = @referraltype, urgency = @urgency,
                referringfacilityname = @referringfacilityname, referringfacilitylevel = @referringfacilitylevel,
                referringphysician = @referringphysician, referringphysiciancontact = @referringphysiciancontact,
                destinationfacilityname = @destinationfacilityname, destinationfacilitylevel = @destinationfacilitylevel,
                destinationfacilitycontact = @destinationfacilitycontact, destinationaddress = @destinationaddress,
                destinationnotified = @destinationnotified, destinationnotificationtime = @destinationnotificationtime,
                destinationcontactperson = @destinationcontactperson, prolongedlabor = @prolongedlabor,
                obstructedlabor = @obstructedlabor, foetaldistress = @foetaldistress,
                antepartumhemorrhage = @antepartumhemorrhage, postpartumhemorrhage = @postpartumhemorrhage,
                severepreeclampsia = @severepreeclampsia, eclampsia = @eclampsia, septicshock = @septicshock,
                ruptureduterus = @ruptureduterus, abnormalpresentation = @abnormalpresentation,
                cordprolapse = @cordprolapse, placentaprevia = @placentaprevia,
                placentalabruption = @placentalabruption, neonatalasphyxia = @neonatalasphyxia,
                prematuritycomplications = @prematuritycomplications, lowbirthweight = @lowbirthweight,
                respiratorydistress = @respiratorydistress, congenitalabnormalities = @congenitalabnormalities,
                neonatalsepsis = @neonatalsepsis, birthinjuries = @birthinjuries,
                lackofresources = @lackofresources, requirescaesareansection = @requirescaesareansection,
                requiresbloodtransfusion = @requiresbloodtransfusion, requiresspecializedcare = @requiresspecializedcare,
                otherreasons = @otherreasons, primarydiagnosis = @primarydiagnosis,
                clinicalsummary = @clinicalsummary, maternalcondition = @maternalcondition,
                maternalpulse = @maternalpulse, maternalbpsystolic = @maternalbpsystolic,
                maternalbpdiastolic = @maternalbpdiastolic, maternaltemperature = @maternaltemperature,
                maternalconsciousness = @maternalconsciousness, fetalheartrate = @fetalheartrate,
                fetalcondition = @fetalcondition, numberofbabiesbeingreferred = @numberofbabiesbeingreferred,
                neonatalcondition = @neonatalcondition, cervicaldilationatreferral = @cervicaldilationatreferral,
                membranesruptured = @membranesruptured, membranerupturetime = @membranerupturetime,
                liquorcolor = @liquorcolor, interventionsperformed = @interventionsperformed,
                medicationsgiven = @medicationsgiven, ivfluidsgiven = @ivfluidsgiven,
                bloodsamplestaken = @bloodsamplestaken, investigationsperformed = @investigationsperformed,
                transportmode = @transportmode, transportdetails = @transportdetails,
                departuretime = @departuretime, arrivaltime = @arrivaltime,
                skillfulattendantaccompanying = @skillfulattendantaccompanying,
                accompanyingstaffname = @accompanyingstaffname, accompanyingstaffdesignation = @accompanyingstaffdesignation,
                partographsent = @partographsent, ivlineinsitu = @ivlineinsitu,
                catheterinsitu = @catheterinsitu, oxygenprovided = @oxygenprovided,
                equipmentsent = @equipmentsent, status = @status, acceptedtime = @acceptedtime,
                completedtime = @completedtime, outcomenotes = @outcomenotes,
                feedbackreceived = @feedbackreceived, feedbackdetails = @feedbackdetails,
                referralletterpath = @referralletterpath, referralformgenerated = @referralformgenerated,
                formgenerationtime = @formgenerationtime, handler = @handler, notes = @notes, updatedtime = @updatedtime,
                syncstatus = 0, version = @version, datahash = @datahash
            WHERE ID = @id";
        }

        private void AddParameters(SqliteCommand cmd, Referral item)
        {
            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@partographid", item.PartographID.ToString());
            cmd.Parameters.AddWithValue("@referraltime", item.ReferralTime.ToString("o"));
            cmd.Parameters.AddWithValue("@referraltype", (int)item.ReferralType);
            cmd.Parameters.AddWithValue("@urgency", (int)item.Urgency);
            cmd.Parameters.AddWithValue("@referringfacilityname", item.ReferringFacilityName ?? string.Empty);
            cmd.Parameters.AddWithValue("@referringfacilitylevel", item.ReferringFacilityLevel ?? string.Empty);
            cmd.Parameters.AddWithValue("@referringphysician", item.ReferringPhysician ?? string.Empty);
            cmd.Parameters.AddWithValue("@referringphysiciancontact", item.ReferringPhysicianContact ?? string.Empty);
            cmd.Parameters.AddWithValue("@destinationfacilityname", item.DestinationFacilityName ?? string.Empty);
            cmd.Parameters.AddWithValue("@destinationfacilitylevel", item.DestinationFacilityLevel ?? string.Empty);
            cmd.Parameters.AddWithValue("@destinationfacilitycontact", item.DestinationFacilityContact ?? string.Empty);
            cmd.Parameters.AddWithValue("@destinationaddress", item.DestinationAddress ?? string.Empty);
            cmd.Parameters.AddWithValue("@destinationnotified", item.DestinationNotified ? 1 : 0);
            cmd.Parameters.AddWithValue("@destinationnotificationtime", item.DestinationNotificationTime?.ToString("o") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@destinationcontactperson", item.DestinationContactPerson ?? string.Empty);
            cmd.Parameters.AddWithValue("@prolongedlabor", item.ProlongedLabor ? 1 : 0);
            cmd.Parameters.AddWithValue("@obstructedlabor", item.ObstructedLabor ? 1 : 0);
            cmd.Parameters.AddWithValue("@foetaldistress", item.FoetalDistress ? 1 : 0);
            cmd.Parameters.AddWithValue("@antepartumhemorrhage", item.AntepartumHemorrhage ? 1 : 0);
            cmd.Parameters.AddWithValue("@postpartumhemorrhage", item.PostpartumHemorrhage ? 1 : 0);
            cmd.Parameters.AddWithValue("@severepreeclampsia", item.SeverePreeclampsia ? 1 : 0);
            cmd.Parameters.AddWithValue("@eclampsia", item.Eclampsia ? 1 : 0);
            cmd.Parameters.AddWithValue("@septicshock", item.SepticShock ? 1 : 0);
            cmd.Parameters.AddWithValue("@ruptureduterus", item.RupturedUterus ? 1 : 0);
            cmd.Parameters.AddWithValue("@abnormalpresentation", item.AbnormalPresentation ? 1 : 0);
            cmd.Parameters.AddWithValue("@cordprolapse", item.CordProlapse ? 1 : 0);
            cmd.Parameters.AddWithValue("@placentaprevia", item.PlacentaPrevia ? 1 : 0);
            cmd.Parameters.AddWithValue("@placentalabruption", item.PlacentalAbruption ? 1 : 0);
            cmd.Parameters.AddWithValue("@neonatalasphyxia", item.NeonatalAsphyxia ? 1 : 0);
            cmd.Parameters.AddWithValue("@prematuritycomplications", item.PrematurityComplications ? 1 : 0);
            cmd.Parameters.AddWithValue("@lowbirthweight", item.LowBirthWeight ? 1 : 0);
            cmd.Parameters.AddWithValue("@respiratorydistress", item.RespiratoryDistress ? 1 : 0);
            cmd.Parameters.AddWithValue("@congenitalabnormalities", item.CongenitalAbnormalities ? 1 : 0);
            cmd.Parameters.AddWithValue("@neonatalsepsis", item.NeonatalSepsis ? 1 : 0);
            cmd.Parameters.AddWithValue("@birthinjuries", item.BirthInjuries ? 1 : 0);
            cmd.Parameters.AddWithValue("@lackofresources", item.LackOfResources ? 1 : 0);
            cmd.Parameters.AddWithValue("@requirescaesareansection", item.RequiresCaesareanSection ? 1 : 0);
            cmd.Parameters.AddWithValue("@requiresbloodtransfusion", item.RequiresBloodTransfusion ? 1 : 0);
            cmd.Parameters.AddWithValue("@requiresspecializedcare", item.RequiresSpecializedCare ? 1 : 0);
            cmd.Parameters.AddWithValue("@otherreasons", item.OtherReasons ?? string.Empty);
            cmd.Parameters.AddWithValue("@primarydiagnosis", item.PrimaryDiagnosis ?? string.Empty);
            cmd.Parameters.AddWithValue("@clinicalsummary", item.ClinicalSummary ?? string.Empty);
            cmd.Parameters.AddWithValue("@maternalcondition", item.MaternalCondition ?? string.Empty);
            cmd.Parameters.AddWithValue("@maternalpulse", item.MaternalPulse.HasValue ? item.MaternalPulse.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maternalbpsystolic", item.MaternalBPSystolic.HasValue ? item.MaternalBPSystolic.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maternalbpdiastolic", item.MaternalBPDiastolic.HasValue ? item.MaternalBPDiastolic.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maternaltemperature", item.MaternalTemperature.HasValue ? item.MaternalTemperature.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maternalconsciousness", item.MaternalConsciousness ?? "Alert");
            cmd.Parameters.AddWithValue("@fetalheartrate", item.FetalHeartRate.HasValue ? item.FetalHeartRate.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@fetalcondition", item.FetalCondition ?? string.Empty);
            cmd.Parameters.AddWithValue("@numberofbabiesbeingreferred", item.NumberOfBabiesBeingReferred.HasValue ? item.NumberOfBabiesBeingReferred.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@neonatalcondition", item.NeonatalCondition ?? string.Empty);
            cmd.Parameters.AddWithValue("@cervicaldilationatreferral", item.CervicalDilationAtReferral.HasValue ? item.CervicalDilationAtReferral.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@membranesruptured", item.MembranesRuptured ? 1 : 0);
            cmd.Parameters.AddWithValue("@membranerupturetime", item.MembraneRuptureTime?.ToString("o") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@liquorcolor", item.LiquorColor ?? "Clear");
            cmd.Parameters.AddWithValue("@interventionsperformed", item.InterventionsPerformed ?? string.Empty);
            cmd.Parameters.AddWithValue("@medicationsgiven", item.MedicationsGiven ?? string.Empty);
            cmd.Parameters.AddWithValue("@ivfluidsgiven", item.IVFluidsGiven ?? string.Empty);
            cmd.Parameters.AddWithValue("@bloodsamplestaken", item.BloodSamplesTaken ? 1 : 0);
            cmd.Parameters.AddWithValue("@investigationsperformed", item.InvestigationsPerformed ?? string.Empty);
            cmd.Parameters.AddWithValue("@transportmode", (int)item.TransportMode);
            cmd.Parameters.AddWithValue("@transportdetails", item.TransportDetails ?? string.Empty);
            cmd.Parameters.AddWithValue("@departuretime", item.DepartureTime?.ToString("o") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@arrivaltime", item.ArrivalTime?.ToString("o") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@skillfulattendantaccompanying", item.SkillfulAttendantAccompanying ? 1 : 0);
            cmd.Parameters.AddWithValue("@accompanyingstaffname", item.AccompanyingStaffName ?? string.Empty);
            cmd.Parameters.AddWithValue("@accompanyingstaffdesignation", item.AccompanyingStaffDesignation ?? string.Empty);
            cmd.Parameters.AddWithValue("@partographsent", item.PartographSent ? 1 : 0);
            cmd.Parameters.AddWithValue("@ivlineinsitu", item.IVLineInsitu ? 1 : 0);
            cmd.Parameters.AddWithValue("@catheterinsitu", item.CatheterInsitu ? 1 : 0);
            cmd.Parameters.AddWithValue("@oxygenprovided", item.OxygenProvided ? 1 : 0);
            cmd.Parameters.AddWithValue("@equipmentsent", item.EquipmentSent ?? string.Empty);
            cmd.Parameters.AddWithValue("@status", (int)item.Status);
            cmd.Parameters.AddWithValue("@acceptedtime", item.AcceptedTime?.ToString("o") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@completedtime", item.CompletedTime?.ToString("o") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@outcomenotes", item.OutcomeNotes ?? string.Empty);
            cmd.Parameters.AddWithValue("@feedbackreceived", item.FeedbackReceived ? 1 : 0);
            cmd.Parameters.AddWithValue("@feedbackdetails", item.FeedbackDetails ?? string.Empty);
            cmd.Parameters.AddWithValue("@referralletterpath", item.ReferralLetterPath ?? string.Empty);
            cmd.Parameters.AddWithValue("@referralformgenerated", item.ReferralFormGenerated ? 1 : 0);
            cmd.Parameters.AddWithValue("@formgenerationtime", item.FormGenerationTime?.ToString("o") ?? (object)DBNull.Value);
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

        private Referral MapFromReader(SqliteDataReader reader)
        {
            return new Referral
            {
                ID = Guid.Parse(reader["ID"].ToString()),
                PartographID = Guid.Parse(reader["partographid"].ToString()),
                ReferralTime = DateTime.Parse(reader["referraltime"].ToString()),
                ReferralType = (ReferralType)Convert.ToInt32(reader["referraltype"]),
                Urgency = (ReferralUrgency)Convert.ToInt32(reader["urgency"]),
                ReferringFacilityName = reader["referringfacilityname"]?.ToString() ?? string.Empty,
                ReferringFacilityLevel = reader["referringfacilitylevel"]?.ToString() ?? string.Empty,
                ReferringPhysician = reader["referringphysician"]?.ToString() ?? string.Empty,
                ReferringPhysicianContact = reader["referringphysiciancontact"]?.ToString() ?? string.Empty,
                DestinationFacilityName = reader["destinationfacilityname"]?.ToString() ?? string.Empty,
                DestinationFacilityLevel = reader["destinationfacilitylevel"]?.ToString() ?? string.Empty,
                DestinationFacilityContact = reader["destinationfacilitycontact"]?.ToString() ?? string.Empty,
                DestinationAddress = reader["destinationaddress"]?.ToString() ?? string.Empty,
                DestinationNotified = Convert.ToBoolean(reader["destinationnotified"]),
                DestinationNotificationTime = reader["destinationnotificationtime"] == DBNull.Value ? null : DateTime.Parse(reader["destinationnotificationtime"].ToString()),
                DestinationContactPerson = reader["destinationcontactperson"]?.ToString() ?? string.Empty,
                ProlongedLabor = Convert.ToBoolean(reader["prolongedlabor"]),
                ObstructedLabor = Convert.ToBoolean(reader["obstructedlabor"]),
                FoetalDistress = Convert.ToBoolean(reader["foetaldistress"]),
                AntepartumHemorrhage = Convert.ToBoolean(reader["antepartumhemorrhage"]),
                PostpartumHemorrhage = Convert.ToBoolean(reader["postpartumhemorrhage"]),
                SeverePreeclampsia = Convert.ToBoolean(reader["severepreeclampsia"]),
                Eclampsia = Convert.ToBoolean(reader["eclampsia"]),
                SepticShock = Convert.ToBoolean(reader["septicshock"]),
                RupturedUterus = Convert.ToBoolean(reader["ruptureduterus"]),
                AbnormalPresentation = Convert.ToBoolean(reader["abnormalpresentation"]),
                CordProlapse = Convert.ToBoolean(reader["cordprolapse"]),
                PlacentaPrevia = Convert.ToBoolean(reader["placentaprevia"]),
                PlacentalAbruption = Convert.ToBoolean(reader["placentalabruption"]),
                NeonatalAsphyxia = Convert.ToBoolean(reader["neonatalasphyxia"]),
                PrematurityComplications = Convert.ToBoolean(reader["prematuritycomplications"]),
                LowBirthWeight = Convert.ToBoolean(reader["lowbirthweight"]),
                RespiratoryDistress = Convert.ToBoolean(reader["respiratorydistress"]),
                CongenitalAbnormalities = Convert.ToBoolean(reader["congenitalabnormalities"]),
                NeonatalSepsis = Convert.ToBoolean(reader["neonatalsepsis"]),
                BirthInjuries = Convert.ToBoolean(reader["birthinjuries"]),
                LackOfResources = Convert.ToBoolean(reader["lackofresources"]),
                RequiresCaesareanSection = Convert.ToBoolean(reader["requirescaesareansection"]),
                RequiresBloodTransfusion = Convert.ToBoolean(reader["requiresbloodtransfusion"]),
                RequiresSpecializedCare = Convert.ToBoolean(reader["requiresspecializedcare"]),
                OtherReasons = reader["otherreasons"]?.ToString() ?? string.Empty,
                PrimaryDiagnosis = reader["primarydiagnosis"]?.ToString() ?? string.Empty,
                ClinicalSummary = reader["clinicalsummary"]?.ToString() ?? string.Empty,
                MaternalCondition = reader["maternalcondition"]?.ToString() ?? string.Empty,
                MaternalPulse = reader["maternalpulse"] == DBNull.Value ? null : Convert.ToInt32(reader["maternalpulse"]),
                MaternalBPSystolic = reader["maternalbpsystolic"] == DBNull.Value ? null : Convert.ToInt32(reader["maternalbpsystolic"]),
                MaternalBPDiastolic = reader["maternalbpdiastolic"] == DBNull.Value ? null : Convert.ToInt32(reader["maternalbpdiastolic"]),
                MaternalTemperature = reader["maternaltemperature"] == DBNull.Value ? null : Convert.ToDecimal(reader["maternaltemperature"]),
                MaternalConsciousness = reader["maternalconsciousness"]?.ToString() ?? "Alert",
                FetalHeartRate = reader["fetalheartrate"] == DBNull.Value ? null : Convert.ToInt32(reader["fetalheartrate"]),
                FetalCondition = reader["fetalcondition"]?.ToString() ?? string.Empty,
                NumberOfBabiesBeingReferred = reader["numberofbabiesbeingreferred"] == DBNull.Value ? null : Convert.ToInt32(reader["numberofbabiesbeingreferred"]),
                NeonatalCondition = reader["neonatalcondition"]?.ToString() ?? string.Empty,
                CervicalDilationAtReferral = reader["cervicaldilationatreferral"] == DBNull.Value ? null : Convert.ToInt32(reader["cervicaldilationatreferral"]),
                MembranesRuptured = Convert.ToBoolean(reader["membranesruptured"]),
                MembraneRuptureTime = reader["membranerupturetime"] == DBNull.Value ? null : DateTime.Parse(reader["membranerupturetime"].ToString()),
                LiquorColor = reader["liquorcolor"]?.ToString() ?? "Clear",
                InterventionsPerformed = reader["interventionsperformed"]?.ToString() ?? string.Empty,
                MedicationsGiven = reader["medicationsgiven"]?.ToString() ?? string.Empty,
                IVFluidsGiven = reader["ivfluidsgiven"]?.ToString() ?? string.Empty,
                BloodSamplesTaken = Convert.ToBoolean(reader["bloodsamplestaken"]),
                InvestigationsPerformed = reader["investigationsperformed"]?.ToString() ?? string.Empty,
                TransportMode = (TransportMode)Convert.ToInt32(reader["transportmode"]),
                TransportDetails = reader["transportdetails"]?.ToString() ?? string.Empty,
                DepartureTime = reader["departuretime"] == DBNull.Value ? null : DateTime.Parse(reader["departuretime"].ToString()),
                ArrivalTime = reader["arrivaltime"] == DBNull.Value ? null : DateTime.Parse(reader["arrivaltime"].ToString()),
                SkillfulAttendantAccompanying = Convert.ToBoolean(reader["skillfulattendantaccompanying"]),
                AccompanyingStaffName = reader["accompanyingstaffname"]?.ToString() ?? string.Empty,
                AccompanyingStaffDesignation = reader["accompanyingstaffdesignation"]?.ToString() ?? string.Empty,
                PartographSent = Convert.ToBoolean(reader["partographsent"]),
                IVLineInsitu = Convert.ToBoolean(reader["ivlineinsitu"]),
                CatheterInsitu = Convert.ToBoolean(reader["catheterinsitu"]),
                OxygenProvided = Convert.ToBoolean(reader["oxygenprovided"]),
                EquipmentSent = reader["equipmentsent"]?.ToString() ?? string.Empty,
                Status = (ReferralStatus)Convert.ToInt32(reader["status"]),
                AcceptedTime = reader["acceptedtime"] == DBNull.Value ? null : DateTime.Parse(reader["acceptedtime"].ToString()),
                CompletedTime = reader["completedtime"] == DBNull.Value ? null : DateTime.Parse(reader["completedtime"].ToString()),
                OutcomeNotes = reader["outcomenotes"]?.ToString() ?? string.Empty,
                FeedbackReceived = Convert.ToBoolean(reader["feedbackreceived"]),
                FeedbackDetails = reader["feedbackdetails"]?.ToString() ?? string.Empty,
                ReferralLetterPath = reader["referralletterpath"]?.ToString() ?? string.Empty,
                ReferralFormGenerated = Convert.ToBoolean(reader["referralformgenerated"]),
                FormGenerationTime = reader["formgenerationtime"] == DBNull.Value ? null : DateTime.Parse(reader["formgenerationtime"].ToString()),
                Handler = reader["handler"] == DBNull.Value ? null : Guid.Parse(reader["handler"].ToString()),
                HandlerName = reader["staffname"] == DBNull.Value ? string.Empty : reader["staffname"].ToString(),
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
