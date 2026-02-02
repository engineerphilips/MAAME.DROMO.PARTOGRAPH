using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // Require authentication for all sync endpoints
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
        [AllowAnonymous] // Health check doesn't require authentication
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                service = "Partograph Sync Service",
                version = "2.0.0"
            });
        }

        /// <summary>
        /// Get sync status summary for all tables
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetSyncStatus()
        {
            try
            {
                var status = new
                {
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    tables = new
                    {
                        patients = await _context.Patients.CountAsync(),
                        partographs = await _context.Partographs.CountAsync(),
                        staff = await _context.Staff.CountAsync(),
                        facilities = await _context.Facilities.CountAsync(),
                        regions = await _context.Regions.CountAsync(),
                        districts = await _context.Districts.CountAsync(),
                        birthOutcomes = await _context.BirthOutcomes.CountAsync(),
                        babyDetails = await _context.BabyDetails.CountAsync(),
                        referrals = await _context.Referrals.CountAsync(),
                        fhrs = await _context.FHRs.CountAsync(),
                        contractions = await _context.Contractions.CountAsync(),
                        cervixDilatations = await _context.CervixDilatations.CountAsync(),
                        fourthStageVitals = await _context.FourthStageVitals.CountAsync(),
                        bishopScores = await _context.BishopScores.CountAsync(),
                        diagnoses = await _context.PartographDiagnoses.CountAsync(),
                        riskFactors = await _context.PartographRiskFactors.CountAsync(),
                        plans = await _context.Plans.CountAsync()
                    }
                };
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync status");
                return StatusCode(500, new { error = "Failed to get sync status", message = ex.Message });
            }
        }

        #endregion

        #region Pull Endpoints - Core Entities

        [HttpPost("pull/patients")]
        public async Task<ActionResult<SyncPullResponse<Patient>>> PullPatients([FromBody] SyncPullRequest request)
        {
            try
            {
                var maxRecords = _configuration.GetValue<int>("SyncSettings:MaxRecordsPerPull", 100);

                // Filter by facility if provided - users only get data from their facility
                var query = _context.Patients
                    .Where(p => p.UpdatedTime > request.LastSyncTimestamp && p.Deleted == 0);

                if (request.FacilityID.HasValue)
                {
                    query = query.Where(p => p.FacilityID == request.FacilityID.Value);
                }

                var records = await query
                    .OrderBy(p => p.UpdatedTime)
                    .Take(maxRecords)
                    .Include(p => p.PartographEntries)
                    .Include(p => p.MedicalNotes)
                    .ToListAsync();

                var countQuery = _context.Patients
                    .Where(p => p.UpdatedTime > request.LastSyncTimestamp && p.Deleted == 0);

                if (request.FacilityID.HasValue)
                {
                    countQuery = countQuery.Where(p => p.FacilityID == request.FacilityID.Value);
                }

                var hasMore = await countQuery.CountAsync() > maxRecords;

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

                // Filter by facility if provided - users only get data from their facility
                var query = _context.Partographs
                    .Where(p => p.UpdatedTime > request.LastSyncTimestamp && p.Deleted == 0);

                if (request.FacilityID.HasValue)
                {
                    query = query.Where(p => p.FacilityID == request.FacilityID.Value);
                }

                var records = await query
                    .OrderBy(p => p.UpdatedTime)
                    .Take(maxRecords)
                    .ToListAsync();

                var countQuery = _context.Partographs
                    .Where(p => p.UpdatedTime > request.LastSyncTimestamp && p.Deleted == 0);

                if (request.FacilityID.HasValue)
                {
                    countQuery = countQuery.Where(p => p.FacilityID == request.FacilityID.Value);
                }

                var hasMore = await countQuery.CountAsync() > maxRecords;

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

        [HttpPost("pull/facilities")]
        public async Task<ActionResult<SyncPullResponse<Facility>>> PullFacilities([FromBody] SyncPullRequest request)
        {
            try
            {
                var maxRecords = _configuration.GetValue<int>("SyncSettings:MaxRecordsPerPull", 100);

                var records = await _context.Facilities
                    .Where(f => f.UpdatedTime > request.LastSyncTimestamp && f.Deleted == 0)
                    .OrderBy(f => f.UpdatedTime)
                    .Take(maxRecords)
                    .ToListAsync();

                var hasMore = await _context.Facilities
                    .CountAsync(f => f.UpdatedTime > request.LastSyncTimestamp && f.Deleted == 0) > maxRecords;

                return Ok(new SyncPullResponse<Facility>
                {
                    Records = records,
                    ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    HasMore = hasMore
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling facilities for device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to pull facilities", message = ex.Message });
            }
        }

        [HttpPost("pull/regions")]
        public async Task<ActionResult<SyncPullResponse<Region>>> PullRegions([FromBody] SyncPullRequest request)
        {
            try
            {
                var maxRecords = _configuration.GetValue<int>("SyncSettings:MaxRecordsPerPull", 100);

                var records = await _context.Regions
                    .Where(r => r.UpdatedTime > request.LastSyncTimestamp && r.Deleted == 0)
                    .OrderBy(r => r.UpdatedTime)
                    .Take(maxRecords)
                    .ToListAsync();

                var hasMore = await _context.Regions
                    .CountAsync(r => r.UpdatedTime > request.LastSyncTimestamp && r.Deleted == 0) > maxRecords;

                return Ok(new SyncPullResponse<Region>
                {
                    Records = records,
                    ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    HasMore = hasMore
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling regions for device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to pull regions", message = ex.Message });
            }
        }

        [HttpPost("pull/districts")]
        public async Task<ActionResult<SyncPullResponse<District>>> PullDistricts([FromBody] SyncPullRequest request)
        {
            try
            {
                var maxRecords = _configuration.GetValue<int>("SyncSettings:MaxRecordsPerPull", 100);

                var records = await _context.Districts
                    .Where(d => d.UpdatedTime > request.LastSyncTimestamp && d.Deleted == 0)
                    .OrderBy(d => d.UpdatedTime)
                    .Take(maxRecords)
                    .ToListAsync();

                var hasMore = await _context.Districts
                    .CountAsync(d => d.UpdatedTime > request.LastSyncTimestamp && d.Deleted == 0) > maxRecords;

                return Ok(new SyncPullResponse<District>
                {
                    Records = records,
                    ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    HasMore = hasMore
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling districts for device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to pull districts", message = ex.Message });
            }
        }

        #endregion

        #region Pull Endpoints - Outcome Entities

        [HttpPost("pull/birthoutcomes")]
        public async Task<ActionResult<SyncPullResponse<BirthOutcome>>> PullBirthOutcomes([FromBody] SyncPullRequest request)
        {
            try
            {
                var maxRecords = _configuration.GetValue<int>("SyncSettings:MaxRecordsPerPull", 100);

                var records = await _context.BirthOutcomes
                    .Where(b => b.UpdatedTime > request.LastSyncTimestamp && b.Deleted == 0)
                    .OrderBy(b => b.UpdatedTime)
                    .Take(maxRecords)
                    .ToListAsync();

                var hasMore = await _context.BirthOutcomes
                    .CountAsync(b => b.UpdatedTime > request.LastSyncTimestamp && b.Deleted == 0) > maxRecords;

                return Ok(new SyncPullResponse<BirthOutcome>
                {
                    Records = records,
                    ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    HasMore = hasMore
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling birth outcomes for device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to pull birth outcomes", message = ex.Message });
            }
        }

        [HttpPost("pull/babydetails")]
        public async Task<ActionResult<SyncPullResponse<BabyDetails>>> PullBabyDetails([FromBody] SyncPullRequest request)
        {
            try
            {
                var maxRecords = _configuration.GetValue<int>("SyncSettings:MaxRecordsPerPull", 100);

                var records = await _context.BabyDetails
                    .Where(b => b.UpdatedTime > request.LastSyncTimestamp && b.Deleted == 0)
                    .OrderBy(b => b.UpdatedTime)
                    .Take(maxRecords)
                    .ToListAsync();

                var hasMore = await _context.BabyDetails
                    .CountAsync(b => b.UpdatedTime > request.LastSyncTimestamp && b.Deleted == 0) > maxRecords;

                return Ok(new SyncPullResponse<BabyDetails>
                {
                    Records = records,
                    ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    HasMore = hasMore
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling baby details for device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to pull baby details", message = ex.Message });
            }
        }

        [HttpPost("pull/referrals")]
        public async Task<ActionResult<SyncPullResponse<Referral>>> PullReferrals([FromBody] SyncPullRequest request)
        {
            try
            {
                var maxRecords = _configuration.GetValue<int>("SyncSettings:MaxRecordsPerPull", 100);

                var records = await _context.Referrals
                    .Where(r => r.UpdatedTime > request.LastSyncTimestamp && r.Deleted == 0)
                    .OrderBy(r => r.UpdatedTime)
                    .Take(maxRecords)
                    .ToListAsync();

                var hasMore = await _context.Referrals
                    .CountAsync(r => r.UpdatedTime > request.LastSyncTimestamp && r.Deleted == 0) > maxRecords;

                return Ok(new SyncPullResponse<Referral>
                {
                    Records = records,
                    ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    HasMore = hasMore
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling referrals for device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to pull referrals", message = ex.Message });
            }
        }

        #endregion

        #region Pull Endpoints - Measurements (Generic)

        [HttpPost("pull/{tableName}")]
        public async Task<IActionResult> PullMeasurements(string tableName, [FromBody] SyncPullRequest request)
        {
            try
            {
                var maxRecords = _configuration.GetValue<int>("SyncSettings:MaxRecordsPerPull", 100);

                object? result = tableName.ToLower() switch
                {
                    // Original measurements
                    "fhrs" => await PullGenericMeasurements(_context.FHRs, request, maxRecords),
                    "contractions" => await PullGenericMeasurements(_context.Contractions, request, maxRecords),
                    "cervixdilatations" => await PullGenericMeasurements(_context.CervixDilatations, request, maxRecords),
                    "headdescents" => await PullGenericMeasurements(_context.HeadDescents, request, maxRecords),
                    "bps" => await PullGenericMeasurements(_context.BPs, request, maxRecords),
                    "temperatures" => await PullGenericMeasurements(_context.Temperatures, request, maxRecords),
                    "amnioticfluids" => await PullGenericMeasurements(_context.AmnioticFluids, request, maxRecords),
                    "urines" => await PullGenericMeasurements(_context.Urines, request, maxRecords),
                    "caputs" => await PullGenericMeasurements(_context.Caputs, request, maxRecords),
                    "mouldings" => await PullGenericMeasurements(_context.Mouldings, request, maxRecords),
                    "fetalpositions" => await PullGenericMeasurements(_context.FetalPositions, request, maxRecords),
                    "painreliefentries" => await PullGenericMeasurements(_context.PainReliefs, request, maxRecords),
                    "postureentries" => await PullGenericMeasurements(_context.Postures, request, maxRecords),
                    "oralfluidentries" => await PullGenericMeasurements(_context.OralFluids, request, maxRecords),
                    "ivfluidentries" => await PullGenericMeasurements(_context.IVFluids, request, maxRecords),
                    "medicationentries" => await PullGenericMeasurements(_context.Medications, request, maxRecords),
                    "oxytocins" => await PullGenericMeasurements(_context.Oxytocins, request, maxRecords),
                    "companionentries" => await PullGenericMeasurements(_context.Companions, request, maxRecords),
                    "assessmentplanentries" or "assessments" => await PullGenericMeasurements(_context.Assessments, request, maxRecords),
                    "medicalnotes" => await PullGenericMeasurements(_context.MedicalNotes, request, maxRecords),

                    // Extended measurements
                    "fourthstagevitals" => await PullGenericMeasurements(_context.FourthStageVitals, request, maxRecords),
                    "bishopscores" => await PullGenericMeasurements(_context.BishopScores, request, maxRecords),
                    "partographdiagnoses" or "diagnoses" => await PullGenericMeasurements(_context.PartographDiagnoses, request, maxRecords),
                    "partographriskfactors" or "riskfactors" => await PullGenericMeasurements(_context.PartographRiskFactors, request, maxRecords),
                    "plans" => await PullGenericMeasurements(_context.Plans, request, maxRecords),

                    _ => null
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

        #region Push Endpoints - Core Entities

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
                        // If FacilityID is not set but Handler is, get the facility from the handler (staff)
                        if (!patient.FacilityID.HasValue && patient.Handler.HasValue)
                        {
                            var handler = await _context.Staff.FirstOrDefaultAsync(s => s.ID == patient.Handler);
                            if (handler != null && handler.Facility.HasValue)
                            {
                                patient.FacilityID = handler.Facility;
                            }
                        }

                        var existing = await _context.Patients
                            .FirstOrDefaultAsync(p => p.ID == patient.ID);

                        if (existing != null)
                        {
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

                            _context.Entry(existing).CurrentValues.SetValues(patient);
                            existing.ServerVersion++;
                            existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            existing.SyncStatus = 1;
                        }
                        else
                        {
                            patient.ServerVersion = 1;
                            patient.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            patient.CreatedTime = patient.CreatedTime == 0
                                ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                : patient.CreatedTime;
                            patient.SyncStatus = 1;
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
                        // If FacilityID is not set but Handler is, get the facility from the handler (staff)
                        if (!partograph.FacilityID.HasValue && partograph.Handler.HasValue)
                        {
                            var handler = await _context.Staff.FirstOrDefaultAsync(s => s.ID == partograph.Handler);
                            if (handler != null && handler.Facility.HasValue)
                            {
                                partograph.FacilityID = handler.Facility;
                            }
                        }

                        var existing = await _context.Partographs
                            .FirstOrDefaultAsync(p => p.ID == partograph.ID);

                        if (existing != null)
                        {
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

                            // Update FacilityID if existing record doesn't have one but new record does
                            if (!existing.FacilityID.HasValue && partograph.FacilityID.HasValue)
                            {
                                existing.FacilityID = partograph.FacilityID;
                            }

                            _context.Entry(existing).CurrentValues.SetValues(partograph);
                            existing.ServerVersion++;
                            existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            existing.SyncStatus = 1;
                        }
                        else
                        {
                            partograph.ServerVersion = 1;
                            partograph.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            partograph.CreatedTime = partograph.CreatedTime == 0
                                ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                : partograph.CreatedTime;
                            partograph.SyncStatus = 1;
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

                            _context.Entry(existing).CurrentValues.SetValues(staff);
                            existing.ServerVersion++;
                            existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            existing.SyncStatus = 1;
                        }
                        else
                        {
                            staff.ServerVersion = 1;
                            staff.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            staff.CreatedTime = staff.CreatedTime == 0
                                ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                : staff.CreatedTime;
                            staff.SyncStatus = 1;
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

        [HttpPost("push/facilities")]
        public async Task<ActionResult<SyncPushResponse<Facility>>> PushFacilities([FromBody] SyncPushRequest<Facility> request)
        {
            try
            {
                var response = new SyncPushResponse<Facility>();

                foreach (var facility in request.Changes)
                {
                    try
                    {
                        var existing = await _context.Facilities
                            .FirstOrDefaultAsync(f => f.ID == facility.ID);

                        if (existing != null)
                        {
                            if (existing.ServerVersion > facility.Version)
                            {
                                response.Conflicts.Add(new ConflictRecord<Facility>
                                {
                                    Id = facility.ID.ToString()!,
                                    LocalRecord = facility,
                                    ServerRecord = existing,
                                    ConflictTime = DateTime.UtcNow,
                                    ConflictReason = "Server version is newer"
                                });
                                continue;
                            }

                            _context.Entry(existing).CurrentValues.SetValues(facility);
                            existing.ServerVersion++;
                            existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            existing.SyncStatus = 1;
                        }
                        else
                        {
                            facility.ServerVersion = 1;
                            facility.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            facility.CreatedTime = facility.CreatedTime == 0
                                ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                : facility.CreatedTime;
                            facility.SyncStatus = 1;
                            _context.Facilities.Add(facility);
                        }

                        response.SuccessIds.Add(facility.ID.ToString()!);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing facility {FacilityId}", facility.ID);
                        response.Errors.Add(new SyncError
                        {
                            Id = facility.ID.ToString()!,
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
                _logger.LogError(ex, "Error pushing facilities from device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to push facilities", message = ex.Message });
            }
        }

        #endregion

        #region Push Endpoints - Outcome Entities

        [HttpPost("push/birthoutcomes")]
        public async Task<ActionResult<SyncPushResponse<BirthOutcome>>> PushBirthOutcomes([FromBody] SyncPushRequest<BirthOutcome> request)
        {
            try
            {
                var response = new SyncPushResponse<BirthOutcome>();

                foreach (var birthOutcome in request.Changes)
                {
                    try
                    {
                        var existing = await _context.BirthOutcomes
                            .FirstOrDefaultAsync(b => b.ID == birthOutcome.ID);

                        if (existing != null)
                        {
                            if (existing.ServerVersion > birthOutcome.Version)
                            {
                                response.Conflicts.Add(new ConflictRecord<BirthOutcome>
                                {
                                    Id = birthOutcome.ID.ToString()!,
                                    LocalRecord = birthOutcome,
                                    ServerRecord = existing,
                                    ConflictTime = DateTime.UtcNow,
                                    ConflictReason = "Server version is newer"
                                });
                                continue;
                            }

                            _context.Entry(existing).CurrentValues.SetValues(birthOutcome);
                            existing.ServerVersion++;
                            existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            existing.SyncStatus = 1;
                        }
                        else
                        {
                            birthOutcome.ServerVersion = 1;
                            birthOutcome.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            birthOutcome.CreatedTime = birthOutcome.CreatedTime == 0
                                ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                : birthOutcome.CreatedTime;
                            birthOutcome.SyncStatus = 1;
                            _context.BirthOutcomes.Add(birthOutcome);
                        }

                        response.SuccessIds.Add(birthOutcome.ID.ToString()!);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing birth outcome {BirthOutcomeId}", birthOutcome.ID);
                        response.Errors.Add(new SyncError
                        {
                            Id = birthOutcome.ID.ToString()!,
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
                _logger.LogError(ex, "Error pushing birth outcomes from device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to push birth outcomes", message = ex.Message });
            }
        }

        [HttpPost("push/babydetails")]
        public async Task<ActionResult<SyncPushResponse<BabyDetails>>> PushBabyDetails([FromBody] SyncPushRequest<BabyDetails> request)
        {
            try
            {
                var response = new SyncPushResponse<BabyDetails>();

                foreach (var babyDetails in request.Changes)
                {
                    try
                    {
                        var existing = await _context.BabyDetails
                            .FirstOrDefaultAsync(b => b.ID == babyDetails.ID);

                        if (existing != null)
                        {
                            if (existing.ServerVersion > babyDetails.Version)
                            {
                                response.Conflicts.Add(new ConflictRecord<BabyDetails>
                                {
                                    Id = babyDetails.ID.ToString()!,
                                    LocalRecord = babyDetails,
                                    ServerRecord = existing,
                                    ConflictTime = DateTime.UtcNow,
                                    ConflictReason = "Server version is newer"
                                });
                                continue;
                            }

                            _context.Entry(existing).CurrentValues.SetValues(babyDetails);
                            existing.ServerVersion++;
                            existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            existing.SyncStatus = 1;
                        }
                        else
                        {
                            babyDetails.ServerVersion = 1;
                            babyDetails.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            babyDetails.CreatedTime = babyDetails.CreatedTime == 0
                                ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                : babyDetails.CreatedTime;
                            babyDetails.SyncStatus = 1;
                            _context.BabyDetails.Add(babyDetails);
                        }

                        response.SuccessIds.Add(babyDetails.ID.ToString()!);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing baby details {BabyDetailsId}", babyDetails.ID);
                        response.Errors.Add(new SyncError
                        {
                            Id = babyDetails.ID.ToString()!,
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
                _logger.LogError(ex, "Error pushing baby details from device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to push baby details", message = ex.Message });
            }
        }

        [HttpPost("push/referrals")]
        public async Task<ActionResult<SyncPushResponse<Referral>>> PushReferrals([FromBody] SyncPushRequest<Referral> request)
        {
            try
            {
                var response = new SyncPushResponse<Referral>();

                foreach (var referral in request.Changes)
                {
                    try
                    {
                        var existing = await _context.Referrals
                            .FirstOrDefaultAsync(r => r.ID == referral.ID);

                        if (existing != null)
                        {
                            if (existing.ServerVersion > referral.Version)
                            {
                                response.Conflicts.Add(new ConflictRecord<Referral>
                                {
                                    Id = referral.ID.ToString()!,
                                    LocalRecord = referral,
                                    ServerRecord = existing,
                                    ConflictTime = DateTime.UtcNow,
                                    ConflictReason = "Server version is newer"
                                });
                                continue;
                            }

                            _context.Entry(existing).CurrentValues.SetValues(referral);
                            existing.ServerVersion++;
                            existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            existing.SyncStatus = 1;
                        }
                        else
                        {
                            referral.ServerVersion = 1;
                            referral.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            referral.CreatedTime = referral.CreatedTime == 0
                                ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                : referral.CreatedTime;
                            referral.SyncStatus = 1;
                            _context.Referrals.Add(referral);
                        }

                        response.SuccessIds.Add(referral.ID.ToString()!);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing referral {ReferralId}", referral.ID);
                        response.Errors.Add(new SyncError
                        {
                            Id = referral.ID.ToString()!,
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
                _logger.LogError(ex, "Error pushing referrals from device {DeviceId}", request.DeviceId);
                return StatusCode(500, new { error = "Failed to push referrals", message = ex.Message });
            }
        }

        #endregion

        #region Push Endpoints - Measurements (Generic)

        [HttpPost("push/{tableName}")]
        public async Task<IActionResult> PushMeasurements(string tableName, [FromBody] object request)
        {
            try
            {
                object? result = tableName.ToLower() switch
                {
                    // Original measurements
                    "fhrs" => await PushGenericMeasurements(_context.FHRs, request),
                    "contractions" => await PushGenericMeasurements(_context.Contractions, request),
                    "cervixdilatations" => await PushGenericMeasurements(_context.CervixDilatations, request),
                    "headdescents" => await PushGenericMeasurements(_context.HeadDescents, request),
                    "bps" => await PushGenericMeasurements(_context.BPs, request),
                    "temperatures" => await PushGenericMeasurements(_context.Temperatures, request),
                    "amnioticfluids" => await PushGenericMeasurements(_context.AmnioticFluids, request),
                    "urines" => await PushGenericMeasurements(_context.Urines, request),
                    "caputs" => await PushGenericMeasurements(_context.Caputs, request),
                    "mouldings" => await PushGenericMeasurements(_context.Mouldings, request),
                    "fetalpositions" => await PushGenericMeasurements(_context.FetalPositions, request),
                    "painreliefentries" => await PushGenericMeasurements(_context.PainReliefs, request),
                    "postureentries" => await PushGenericMeasurements(_context.Postures, request),
                    "oralfluidentries" => await PushGenericMeasurements(_context.OralFluids, request),
                    "ivfluidentries" => await PushGenericMeasurements(_context.IVFluids, request),
                    "medicationentries" => await PushGenericMeasurements(_context.Medications, request),
                    "oxytocins" => await PushGenericMeasurements(_context.Oxytocins, request),
                    "companionentries" => await PushGenericMeasurements(_context.Companions, request),
                    "assessmentplanentries" or "assessments" => await PushGenericMeasurements(_context.Assessments, request),
                    "medicalnotes" => await PushGenericMeasurements(_context.MedicalNotes, request),

                    // Extended measurements
                    "fourthstagevitals" => await PushGenericMeasurements(_context.FourthStageVitals, request),
                    "bishopscores" => await PushGenericMeasurements(_context.BishopScores, request),
                    "partographdiagnoses" or "diagnoses" => await PushGenericMeasurements(_context.PartographDiagnoses, request),
                    "partographriskfactors" or "riskfactors" => await PushGenericMeasurements(_context.PartographRiskFactors, request),
                    "plans" => await PushGenericMeasurements(_context.Plans, request),

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

                        _context.Entry(existing).CurrentValues.SetValues(measurement);
                        existing.ServerVersion++;
                        existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        existing.SyncStatus = 1;
                    }
                    else
                    {
                        measurement.ServerVersion = 1;
                        measurement.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        measurement.CreatedTime = measurement.CreatedTime == 0
                            ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            : measurement.CreatedTime;
                        measurement.SyncStatus = 1;
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
