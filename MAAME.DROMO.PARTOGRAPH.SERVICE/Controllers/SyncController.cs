using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly PartographDbContext _context;
        private readonly ILogger<SyncController> _logger;
        private readonly IConfiguration _configuration;

        public SyncController(
            PartographDbContext context,
            ILogger<SyncController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        #region Health Check

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                service = "Partograph Sync Service",
                version = "1.0.0"
            });
        }

        #endregion

        #region Pull Endpoints

        [HttpPost("pull/patients")]
        public async Task<ActionResult<SyncPullResponse<Patient>>> PullPatients([FromBody] SyncPullRequest request)
        {
            try
            {
                var maxRecords = _configuration.GetValue<int>("SyncSettings:MaxRecordsPerPull", 100);

                var records = await _context.Patients
                    .Where(p => p.UpdatedTime > request.LastSyncTimestamp && p.Deleted == 0)
                    .OrderBy(p => p.UpdatedTime)
                    .Take(maxRecords)
                    .Include(p => p.PartographEntries)
                    .Include(p => p.MedicalNotes)
                    .ToListAsync();

                var hasMore = await _context.Patients
                    .CountAsync(p => p.UpdatedTime > request.LastSyncTimestamp && p.Deleted == 0) > maxRecords;

                return Ok(new SyncPullResponse<Patient>
                {
                    Records = records,
                    ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    HasMore = hasMore
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling patients for device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to pull patients", message = ex.Message });
            }
        }

        [HttpPost("pull/partographs")]
        public async Task<ActionResult<SyncPullResponse<Partograph>>> PullPartographs([FromBody] SyncPullRequest request)
        {
            try
            {
                var maxRecords = _configuration.GetValue<int>("SyncSettings:MaxRecordsPerPull", 100);

                var records = await _context.Partographs
                    .Where(p => p.UpdatedTime > request.LastSyncTimestamp && p.Deleted == 0)
                    .OrderBy(p => p.UpdatedTime)
                    .Take(maxRecords)
                    .ToListAsync();

                var hasMore = await _context.Partographs
                    .CountAsync(p => p.UpdatedTime > request.LastSyncTimestamp && p.Deleted == 0) > maxRecords;

                return Ok(new SyncPullResponse<Partograph>
                {
                    Records = records,
                    ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    HasMore = hasMore
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling partographs for device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to pull partographs", message = ex.Message });
            }
        }

        [HttpPost("pull/staff")]
        public async Task<ActionResult<SyncPullResponse<Staff>>> PullStaff([FromBody] SyncPullRequest request)
        {
            try
            {
                var maxRecords = _configuration.GetValue<int>("SyncSettings:MaxRecordsPerPull", 100);

                var records = await _context.Staff
                    .Where(s => s.UpdatedTime > request.LastSyncTimestamp && s.Deleted == 0)
                    .OrderBy(s => s.UpdatedTime)
                    .Take(maxRecords)
                    .ToListAsync();

                var hasMore = await _context.Staff
                    .CountAsync(s => s.UpdatedTime > request.LastSyncTimestamp && s.Deleted == 0) > maxRecords;

                return Ok(new SyncPullResponse<Staff>
                {
                    Records = records,
                    ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    HasMore = hasMore
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling staff for device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to pull staff", message = ex.Message });
            }
        }

        [HttpPost("pull/{tableName}")]
        public async Task<IActionResult> PullMeasurements(string tableName, [FromBody] SyncPullRequest request)
        {
            try
            {
                var maxRecords = _configuration.GetValue<int>("SyncSettings:MaxRecordsPerPull", 100);

                object? result = null;
                result = tableName.ToLower() switch
                {
                    "fhrs" => await PullGenericMeasurements<FHR>(_context.FHRs, request, maxRecords),
                    "contractions" => await PullGenericMeasurements<Contraction>(_context.Contractions, request, maxRecords),
                    "cervixdilatations" => await PullGenericMeasurements<CervixDilatation>(_context.CervixDilatations, request, maxRecords),
                    "headdescents" => await PullGenericMeasurements<HeadDescent>(_context.HeadDescents, request, maxRecords),
                    "bps" => await PullGenericMeasurements<BP>(_context.BPs, request, maxRecords),
                    "temperatures" => await PullGenericMeasurements<Temperature>(_context.Temperatures, request, maxRecords),
                    "amnioticfluids" => await PullGenericMeasurements<AmnioticFluid>(_context.AmnioticFluids, request, maxRecords),
                    "urines" => await PullGenericMeasurements<Urine>(_context.Urines, request, maxRecords),
                    "caputs" => await PullGenericMeasurements<Caput>(_context.Caputs, request, maxRecords),
                    "mouldings" => await PullGenericMeasurements<Moulding>(_context.Mouldings, request, maxRecords),
                    "fetalpositions" => await PullGenericMeasurements<FetalPosition>(_context.FetalPositions, request, maxRecords),
                    "painreliefentries" => await PullGenericMeasurements<PainReliefEntry>(_context.PainReliefs, request, maxRecords),
                    "postureentries" => await PullGenericMeasurements<PostureEntry>(_context.Postures, request, maxRecords),
                    "oralfluidentries" => await PullGenericMeasurements<OralFluidEntry>(_context.OralFluids, request, maxRecords),
                    "ivfluidentries" => await PullGenericMeasurements<IVFluidEntry>(_context.IVFluids, request, maxRecords),
                    "medicationentries" => await PullGenericMeasurements<MedicationEntry>(_context.Medications, request, maxRecords),
                    "oxytocins" => await PullGenericMeasurements<Oxytocin>(_context.Oxytocins, request, maxRecords),
                    "companionentries" => await PullGenericMeasurements<CompanionEntry>(_context.Companions, request, maxRecords),
                    "assessmentplanentries" => await PullGenericMeasurements<Assessment>(_context.AssessmentPlans, request, maxRecords),
                    "medicalnotes" => await PullGenericMeasurements<MedicalNote>(_context.MedicalNotes, request, maxRecords),
                    _ => null,
                };
                if (result == null)
                    return BadRequest(new { error = "Unknown table name", tableName });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling {TableName} for device {DeviceId}", tableName, request.DeviceId);
                return StatusCode(500, new { error = $"Failed to pull {tableName}", message = ex.Message });
            }
        }

        #endregion

        #region Push Endpoints

        [HttpPost("push/patients")]
        public async Task<ActionResult<SyncPushResponse<Patient>>> PushPatients([FromBody] SyncPushRequest<Patient> request)
        {
            try
            {
                var response = new SyncPushResponse<Patient>();

                foreach (var patient in request.Changes)
                {
                    try
                    {
                        var existing = await _context.Patients
                            .FirstOrDefaultAsync(p => p.ID == patient.ID);

                        if (existing != null)
                        {
                            // Check for conflicts
                            if (existing.ServerVersion > patient.Version)
                            {
                                response.Conflicts.Add(new ConflictRecord<Patient>
                                {
                                    Id = patient.ID.ToString()!,
                                    LocalRecord = patient,
                                    ServerRecord = existing,
                                    ConflictTime = DateTime.UtcNow,
                                    ConflictReason = "Server version is newer"
                                });
                                continue;
                            }

                            // Update existing
                            _context.Entry(existing).CurrentValues.SetValues(patient);
                            existing.ServerVersion++;
                            existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            existing.SyncStatus = 1; // Synced
                        }
                        else
                        {
                            // Insert new
                            patient.ServerVersion = 1;
                            patient.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            patient.CreatedTime = patient.CreatedTime == 0
                                ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                : patient.CreatedTime;
                            patient.SyncStatus = 1; // Synced
                            _context.Patients.Add(patient);
                        }

                        response.SuccessIds.Add(patient.ID.ToString()!);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing patient {PatientId}", patient.ID);
                        response.Errors.Add(new SyncError
                        {
                            Id = patient.ID.ToString()!,
                            ErrorMessage = ex.Message,
                            StackTrace = ex.StackTrace
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pushing patients from device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to push patients", message = ex.Message });
            }
        }

        [HttpPost("push/partographs")]
        public async Task<ActionResult<SyncPushResponse<Partograph>>> PushPartographs([FromBody] SyncPushRequest<Partograph> request)
        {
            try
            {
                var response = new SyncPushResponse<Partograph>();

                foreach (var partograph in request.Changes)
                {
                    try
                    {
                        var existing = await _context.Partographs
                            .FirstOrDefaultAsync(p => p.ID == partograph.ID);

                        if (existing != null)
                        {
                            // Check for conflicts
                            if (existing.ServerVersion > partograph.Version)
                            {
                                response.Conflicts.Add(new ConflictRecord<Partograph>
                                {
                                    Id = partograph.ID.ToString()!,
                                    LocalRecord = partograph,
                                    ServerRecord = existing,
                                    ConflictTime = DateTime.UtcNow,
                                    ConflictReason = "Server version is newer"
                                });
                                continue;
                            }

                            // Update existing
                            _context.Entry(existing).CurrentValues.SetValues(partograph);
                            existing.ServerVersion++;
                            existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            existing.SyncStatus = 1; // Synced
                        }
                        else
                        {
                            // Insert new
                            partograph.ServerVersion = 1;
                            partograph.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            partograph.CreatedTime = partograph.CreatedTime == 0
                                ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                : partograph.CreatedTime;
                            partograph.SyncStatus = 1; // Synced
                            _context.Partographs.Add(partograph);
                        }

                        response.SuccessIds.Add(partograph.ID.ToString()!);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing partograph {PartographId}", partograph.ID);
                        response.Errors.Add(new SyncError
                        {
                            Id = partograph.ID.ToString()!,
                            ErrorMessage = ex.Message,
                            StackTrace = ex.StackTrace
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pushing partographs from device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to push partographs", message = ex.Message });
            }
        }

        [HttpPost("push/staff")]
        public async Task<ActionResult<SyncPushResponse<Staff>>> PushStaff([FromBody] SyncPushRequest<Staff> request)
        {
            try
            {
                var response = new SyncPushResponse<Staff>();

                foreach (var staff in request.Changes)
                {
                    try
                    {
                        var existing = await _context.Staff
                            .FirstOrDefaultAsync(s => s.ID == staff.ID);

                        if (existing != null)
                        {
                            // Check for conflicts
                            if (existing.ServerVersion > staff.Version)
                            {
                                response.Conflicts.Add(new ConflictRecord<Staff>
                                {
                                    Id = staff.ID.ToString()!,
                                    LocalRecord = staff,
                                    ServerRecord = existing,
                                    ConflictTime = DateTime.UtcNow,
                                    ConflictReason = "Server version is newer"
                                });
                                continue;
                            }

                            // Update existing
                            _context.Entry(existing).CurrentValues.SetValues(staff);
                            existing.ServerVersion++;
                            existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            existing.SyncStatus = 1; // Synced
                        }
                        else
                        {
                            // Insert new
                            staff.ServerVersion = 1;
                            staff.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            staff.CreatedTime = staff.CreatedTime == 0
                                ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                : staff.CreatedTime;
                            staff.SyncStatus = 1; // Synced
                            _context.Staff.Add(staff);
                        }

                        response.SuccessIds.Add(staff.ID.ToString()!);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing staff {StaffId}", staff.ID);
                        response.Errors.Add(new SyncError
                        {
                            Id = staff.ID.ToString()!,
                            ErrorMessage = ex.Message,
                            StackTrace = ex.StackTrace
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pushing staff from device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to push staff", message = ex.Message });
            }
        }

        [HttpPost("push/{tableName}")]
        public async Task<IActionResult> PushMeasurements(string tableName, [FromBody] object request)
        {
            try
            {
                object? result = null;
                result = tableName.ToLower() switch
                {
                    "fhrs" => await PushGenericMeasurements<FHR>(_context.FHRs, request),
                    "contractions" => await PushGenericMeasurements<Contraction>(_context.Contractions, request),
                    "cervixdilatations" => await PushGenericMeasurements<CervixDilatation>(_context.CervixDilatations, request),
                    "headdescents" => await PushGenericMeasurements<HeadDescent>(_context.HeadDescents, request),
                    "bps" => await PushGenericMeasurements<BP>(_context.BPs, request),
                    "temperatures" => await PushGenericMeasurements<Temperature>(_context.Temperatures, request),
                    "amnioticfluids" => await PushGenericMeasurements<AmnioticFluid>(_context.AmnioticFluids, request),
                    "urines" => await PushGenericMeasurements<Urine>(_context.Urines, request),
                    "caputs" => await PushGenericMeasurements<Caput>(_context.Caputs, request),
                    "mouldings" => await PushGenericMeasurements<Moulding>(_context.Mouldings, request),
                    "fetalpositions" => await PushGenericMeasurements<FetalPosition>(_context.FetalPositions, request),
                    "painreliefentries" => await PushGenericMeasurements<PainReliefEntry>(_context.PainReliefs, request),
                    "postureentries" => await PushGenericMeasurements<PostureEntry>(_context.Postures, request),
                    "oralfluidentries" => await PushGenericMeasurements<OralFluidEntry>(_context.OralFluids, request),
                    "ivfluidentries" => await PushGenericMeasurements<IVFluidEntry>(_context.IVFluids, request),
                    "medicationentries" => await PushGenericMeasurements<MedicationEntry>(_context.Medications, request),
                    "oxytocins" => await PushGenericMeasurements<Oxytocin>(_context.Oxytocins, request),
                    "companionentries" => await PushGenericMeasurements<CompanionEntry>(_context.Companions, request),
                    "assessmentplanentries" => await PushGenericMeasurements<Assessment>(_context.AssessmentPlans, request),
                    "medicalnotes" => await PushGenericMeasurements<MedicalNote>(_context.MedicalNotes, request),
                    _ => null
                };

                if (result == null)
                    return BadRequest(new { error = "Unknown table name", tableName });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pushing {TableName}", tableName);
                return StatusCode(500, new { error = $"Failed to push {tableName}", message = ex.Message });
            }
        }

        #endregion

        #region Helper Methods

        private async Task<SyncPullResponse<T>> PullGenericMeasurements<T>(
            DbSet<T> dbSet,
            SyncPullRequest request,
            int maxRecords) where T : BasePartographMeasurement
        {
            var records = await dbSet
                .Where(m => m.UpdatedTime > request.LastSyncTimestamp && m.Deleted == 0)
                .OrderBy(m => m.UpdatedTime)
                .Take(maxRecords)
                .ToListAsync();

            var hasMore = await dbSet
                .CountAsync(m => m.UpdatedTime > request.LastSyncTimestamp && m.Deleted == 0) > maxRecords;

            return new SyncPullResponse<T>
            {
                Records = records,
                ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                HasMore = hasMore
            };
        }

        private async Task<SyncPushResponse<T>> PushGenericMeasurements<T>(
            DbSet<T> dbSet,
            object requestObj) where T : BasePartographMeasurement
        {
            var request = System.Text.Json.JsonSerializer.Deserialize<SyncPushRequest<T>>(
                System.Text.Json.JsonSerializer.Serialize(requestObj));

            if (request == null)
                throw new ArgumentException("Invalid request format");

            var response = new SyncPushResponse<T>();

            foreach (var measurement in request.Changes)
            {
                try
                {
                    var existing = await dbSet.FirstOrDefaultAsync(m => m.ID == measurement.ID);

                    if (existing != null)
                    {
                        // Check for conflicts
                        if (existing.ServerVersion > measurement.Version)
                        {
                            response.Conflicts.Add(new ConflictRecord<T>
                            {
                                Id = measurement.ID.ToString()!,
                                LocalRecord = measurement,
                                ServerRecord = existing,
                                ConflictTime = DateTime.UtcNow,
                                ConflictReason = "Server version is newer"
                            });
                            continue;
                        }

                        // Update existing
                        _context.Entry(existing).CurrentValues.SetValues(measurement);
                        existing.ServerVersion++;
                        existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        existing.SyncStatus = 1; // Synced
                    }
                    else
                    {
                        // Insert new
                        measurement.ServerVersion = 1;
                        measurement.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        measurement.CreatedTime = measurement.CreatedTime == 0
                            ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            : measurement.CreatedTime;
                        measurement.SyncStatus = 1; // Synced
                        dbSet.Add(measurement);
                    }

                    response.SuccessIds.Add(measurement.ID.ToString()!);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing measurement {MeasurementId}", measurement.ID);
                    response.Errors.Add(new SyncError
                    {
                        Id = measurement.ID.ToString()!,
                        ErrorMessage = ex.Message,
                        StackTrace = ex.StackTrace
                    });
                }
            }

            await _context.SaveChangesAsync();
            return response;
        }

        #endregion
    }
}
