# Repository Update Pattern for WHO 2020 Enhancements

This document provides the pattern for updating the remaining 9 repositories with WHO 2020 database schema enhancements.

## Completed Repositories
- ✅ HeadDescentRepository
- ✅ FetalPositionRepository
- ✅ OxytocinRepository (from previous commit)

## Remaining Repositories
1. IVFluidEntryRepository
2. PainReliefEntryRepository
3. MedicalNoteRepository
4. PostureEntryRepository
5. CompanionEntryRepository
6. OralFluidEntryRepository
7. CaputRepository
8. MouldingRepository
9. PartographDiagnosisRepository

## Standard Pattern

For each repository, follow these steps:

### Step 1: Read the Model File
Identify new properties added in the corresponding MODEL file (e.g., `MAAME.DROMO.PARTOGRAPH.MODEL/IVFluidEntry.cs`)

### Step 2: Update CreateTableSql

Add new columns to the CREATE TABLE statement AND add ALTER TABLE statements for migration:

```sql
CREATE TABLE IF NOT EXISTS Tbl_[TableName] (
    -- Existing columns...
    datahash TEXT,
    -- New WHO 2020 columns
    newcolumn1 TEXT DEFAULT '',
    newcolumn2 INTEGER DEFAULT 0,
    newcolumn3 REAL DEFAULT 0.0
);

-- Add new columns to existing tables (WHO 2020 enhancements)
ALTER TABLE Tbl_[TableName] ADD COLUMN newcolumn1 TEXT DEFAULT '';
ALTER TABLE Tbl_[TableName] ADD COLUMN newcolumn2 INTEGER DEFAULT 0;
ALTER TABLE Tbl_[TableName] ADD COLUMN newcolumn3 REAL DEFAULT 0.0;
```

**Column Type Mapping:**
- `string` → `TEXT DEFAULT ''`
- `bool` → `INTEGER DEFAULT 0`
- `int` → `INTEGER DEFAULT 0`
- `decimal` → `REAL DEFAULT 0.0`
- `DateTime?` → `TEXT` (nullable, no default)

### Step 3: Update MapFromReader

Convert from positional indexing to GetOrdinal, add helper methods, and safely read new columns:

```csharp
protected override [ModelType] MapFromReader(SqliteDataReader reader)
{
    var item = new [ModelType]
    {
        ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
        PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
        // ... existing base properties using GetOrdinal
    };

    // WHO 2020 enhancements - safely read new columns
    try
    {
        item.StringProperty = GetStringOrDefault(reader, "columnname", "defaultvalue");
        item.BoolProperty = GetBoolFromInt(reader, "columnname");
        item.IntProperty = GetIntOrDefault(reader, "columnname", 0);
        item.DecimalProperty = GetDecimalOrDefault(reader, "columnname", 0.0m);
        item.DateTimeProperty = GetNullableDateTime(reader, "columnname");
    }
    catch { /* Columns don't exist yet in old databases */ }

    return item;
}

// Helper methods
private bool GetBoolFromInt(SqliteDataReader reader, string columnName)
{
    try
    {
        int ordinal = reader.GetOrdinal(columnName);
        return !reader.IsDBNull(ordinal) && reader.GetInt32(ordinal) == 1;
    }
    catch { return false; }
}

private string GetStringOrDefault(SqliteDataReader reader, string columnName, string defaultValue = "")
{
    try
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal);
    }
    catch { return defaultValue; }
}

private int GetIntOrDefault(SqliteDataReader reader, string columnName, int defaultValue = 0)
{
    try
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? defaultValue : reader.GetInt32(ordinal);
    }
    catch { return defaultValue; }
}

private decimal GetDecimalOrDefault(SqliteDataReader reader, string columnName, decimal defaultValue = 0)
{
    try
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? defaultValue : (decimal)reader.GetDouble(ordinal);
    }
    catch { return defaultValue; }
}

private DateTime? GetNullableDateTime(SqliteDataReader reader, string columnName)
{
    try
    {
        int ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal)) return null;
        return DateTime.Parse(reader.GetString(ordinal));
    }
    catch { return null; }
}
```

### Step 4: Update GetInsertSql

Add new column names to both the column list and VALUES list:

```csharp
protected override string GetInsertSql() => @"
INSERT INTO Tbl_[TableName] (ID, partographID, time, handler, notes, [existing_columns], createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash,
    newcolumn1, newcolumn2, newcolumn3)
VALUES (@id, @partographId, @time, @handler, @notes, @[existing_params], @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash,
    @newcolumn1, @newcolumn2, @newcolumn3);";
```

### Step 5: Update GetUpdateSql

Add new column assignments:

```csharp
protected override string GetUpdateSql() => @"
UPDATE Tbl_[TableName]
SET partographID = @partographId,
    time = @time,
    handler = @handler,
    notes = @notes,
    [existing_columns] = @[existing_params],
    newcolumn1 = @newcolumn1,
    newcolumn2 = @newcolumn2,
    newcolumn3 = @newcolumn3,
    updatedtime = @updatedtime,
    deletedtime = @deletedtime,
    deviceid = @deviceid,
    syncstatus = @syncstatus,
    version = @version,
    datahash = @datahash
WHERE ID = @id";
```

### Step 6: Update AddInsertParameters

Add parameter bindings for new columns at the end (after datahash, before closing):

```csharp
cmd.Parameters.AddWithValue("@datahash", item.DataHash);

// WHO 2020 enhancements
cmd.Parameters.AddWithValue("@stringparam", item.StringProperty ?? "defaultvalue");
cmd.Parameters.AddWithValue("@boolparam", item.BoolProperty ? 1 : 0);
cmd.Parameters.AddWithValue("@intparam", item.IntProperty);
cmd.Parameters.AddWithValue("@decimalparam", (double)item.DecimalProperty);
cmd.Parameters.AddWithValue("@datetimeparam", item.DateTimeProperty?.ToString("O") ?? (object)DBNull.Value);
```

### Step 7: Update AddUpdateParameters

Same as Step 6, add parameter bindings at the end.

## Field Mapping Reference

### IVFluidEntry New Fields:
- RateMlPerHour (decimal)
- StartTime (DateTime?)
- EndTime (DateTime?)
- DurationMinutes (int?)
- Additives (string)
- AdditiveConcentration (string)
- AdditiveDose (string)
- IVSite (string)
- SiteHealthy (bool, default true)
- SiteCondition (string)
- PhlebitisScore (int)
- LastSiteAssessment (DateTime?)
- LastDressingChange (DateTime?)
- CannelaInsertionDate (DateTime?)
- Indication (string)
- BatchNumber (string)
- RunningTotalInput (int)
- ClinicalAlert (string)

### PainReliefEntry New Fields:
- PainScoreBefore (int?)
- PainScoreAfter (int?)
- PainAssessmentTool (string)
- PainReliefMethod (string)
- NonPharmacologicalMethods (string)
- AdministeredTime (DateTime?)
- AdministeredBy (string)
- Dose (string)
- Route (string)
- Effectiveness (string)
- TimeToEffectMinutes (int?)
- DurationOfEffectHours (int?)
- SideEffects (bool)
- SideEffectsDescription (string)
- ContinuousMonitoringRequired (bool)
- BladderCareRequired (bool)
- LastTopUpTime (DateTime?)
- TopUpCount (int)
- ContraindicationsChecked (bool)
- ContraindicationsPresent (bool)
- ContraindicationDetails (string)
- InformedConsentObtained (bool)
- PatientPreference (string)
- ClinicalAlert (string)

### MedicalNote New Fields:
- Role (string)
- UrgencyLevel (string)
- ClinicalCategory (string)
- WHOSection (string)
- LinkedMeasurableType (string)
- LinkedMeasurableID (Guid?)
- LinkedMeasurableTime (DateTime?)
- RequiresReview (bool)
- RequiresFollowUp (bool)
- ReviewedTime (DateTime?)
- ReviewedBy (string)
- ReviewOutcome (string)
- Escalated (bool)
- EscalatedTo (string)
- EscalationTime (DateTime?)
- EscalationReason (string)
- IncludeInHandover (bool)
- CommunicatedToPatient (bool)
- CommunicatedToCompanion (bool)
- AttachmentPath (string)
- ReferenceDocument (string)
- ClinicalAlert (string)

### PostureEntry New Fields:
- PostureCategory (string)
- StartTime (DateTime?)
- EndTime (DateTime?)
- DurationMinutes (int?)
- Reason (string)
- EffectOnLabor (string)
- EffectOnPain (string)
- EffectOnContractions (string)
- PatientChoice (bool, default true)
- MedicallyIndicated (bool)
- MobileAndActive (bool)
- RestrictedMobility (bool)
- MobilityRestriction (string)
- SupportEquipment (string)
- ClinicalAlert (string)

### CompanionEntry New Fields:
- CompanionPresent (bool)
- CompanionType (string)
- NumberOfCompanions (int)
- CompanionName (string)
- CompanionRelationship (string)
- ArrivalTime (DateTime?)
- DepartureTime (DateTime?)
- DurationMinutes (int?)
- ContinuousPresence (bool)
- ParticipationLevel (string)
- SupportActivities (string)
- PatientRequestedCompanion (bool)
- PatientDeclinedCompanion (bool)
- ReasonForNoCompanion (string)
- StaffOrientedCompanion (bool)
- CompanionInvolvedInDecisions (bool)
- LanguageBarrier (bool)
- InterpreterRequired (bool)
- CulturalPractices (bool)
- CulturalPracticesDetails (string)
- ClinicalAlert (string)

### OralFluidEntry New Fields:
- FluidType (string)
- AmountMl (int)
- RunningTotalOralIntake (int)
- Tolerated (bool, default true)
- Vomiting (bool)
- Nausea (bool)
- VomitingEpisodes (int?)
- VomitContent (string)
- FoodOffered (bool)
- FoodConsumed (bool)
- FoodType (string)
- NBM (bool)
- NBMReason (string)
- Restrictions (string)
- RestrictionReason (string)
- PatientRequestedFluids (bool)
- PatientDeclinedFluids (bool)
- AspirationRiskAssessed (bool)
- AspirationRiskLevel (string)
- ClinicalAlert (string)

### Caput New Fields:
- Location (string)
- Size (string)
- Consistency (string)
- Increasing (bool)
- Decreasing (bool)
- Stable (bool)
- ProgressionRate (string)
- FirstDetectedTime (DateTime?)
- DurationHours (int?)
- MouldingPresent (bool)
- MouldingDegree (string)
- SuggestsObstruction (bool)
- SuggestionProlongedLabor (bool)
- ChangeFromPrevious (string)
- ClinicalAlert (string)

### Moulding New Fields:
- SuturesOverlapping (bool)
- Reducible (bool)
- Location (string)
- SagittalSuture (bool)
- CoronalSuture (bool)
- LambdoidSuture (bool)
- Severity (string)
- Increasing (bool)
- Reducing (bool)
- Stable (bool)
- ProgressionRate (string)
- FirstDetectedTime (DateTime?)
- DurationHours (int?)
- CaputPresent (bool)
- CaputDegree (string)
- SuggestsObstruction (bool)
- SuggestsCPD (bool)
- ChangeFromPrevious (string)
- ClinicalAlert (string)

### PartographDiagnosis New Fields:
- Category (string)
- DiagnosisType (string)
- ICDCode (string)
- ICDDescription (string)
- Severity (string)
- OnsetTime (DateTime?)
- DurationHours (int?)
- OnsetType (string)
- ClinicalEvidence (string)
- SupportingFindings (string)
- LinkedMeasurableIDs (List<Guid>) - NOTE: serialize as comma-separated string or JSON
- LinkedMeasurableTypes (string)
- Status (string)
- DiagnosedBy (string)
- DiagnosedByRole (string)
- ConfidenceLevel (string)
- ManagementPlan (string)
- ManagementAction (string)
- RequiresEscalation (bool)
- EscalatedTo (string)
- EscalationTime (DateTime?)
- RequiresReview (bool)
- ReviewTime (DateTime?)
- ReviewOutcome (string)
- ResolvedTime (DateTime?)
- DiscussedWithPatient (bool)
- DiscussedWithCompanion (bool)
- PatientUnderstanding (string)
- ClinicalAlert (string)

**Note for PartographDiagnosis:** The LinkedMeasurableIDs is a List<Guid>. In SQLite, store as TEXT and serialize/deserialize as comma-separated GUIDs or JSON.

## Example: Complete IVFluidEntryRepository Update

See HeadDescentRepository.cs and FetalPositionRepository.cs as reference implementations that follow this pattern exactly.

## Testing Checklist

After updating each repository:
1. ✅ CREATE TABLE includes all new columns
2. ✅ ALTER TABLE statements added for all new columns
3. ✅ MapFromReader uses GetOrdinal for all columns
4. ✅ Helper methods added (GetBoolFromInt, GetStringOrDefault, etc.)
5. ✅ New columns in try-catch block in MapFromReader
6. ✅ GetInsertSql includes all new columns
7. ✅ GetUpdateSql includes all new columns
8. ✅ AddInsertParameters binds all new parameters
9. ✅ AddUpdateParameters binds all new parameters
10. ✅ Proper type conversions (bool to int, decimal to double, DateTime to "O" format)
