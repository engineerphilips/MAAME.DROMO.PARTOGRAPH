using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly PartographDbContext _context;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(PartographDbContext context, ILogger<PatientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Patients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> GetPatients(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var query = _context.Patients
                    .Where(p => p.Deleted == 0)
                    .AsQueryable();

                // Filter by facility if specified
                if (facilityId.HasValue)
                {
                    query = query.Where(p => p.FacilityID == facilityId);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(p =>
                        p.FirstName.ToLower().Contains(search) ||
                        p.LastName.ToLower().Contains(search) ||
                        p.HospitalNumber.ToLower().Contains(search));
                }

                var total = await query.CountAsync();
                var patients = await query
                    .OrderByDescending(p => p.CreatedTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(p => p.PartographEntries)
                    .Include(p => p.MedicalNotes)
                    .ToListAsync();

                Response.Headers.Append("X-Total-Count", total.ToString());
                Response.Headers.Append("X-Page", page.ToString());
                Response.Headers.Append("X-Page-Size", pageSize.ToString());

                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients");
                return StatusCode(500, new { error = "Failed to retrieve patients", message = ex.Message });
            }
        }

        // GET: api/Patients/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Patient>> GetPatient(Guid id)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.PartographEntries)
                    .Include(p => p.MedicalNotes)
                    .FirstOrDefaultAsync(p => p.ID == id && p.Deleted == 0);

                if (patient == null)
                {
                    return NotFound(new { error = "Patient not found", id });
                }

                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient {PatientId}", id);
                return StatusCode(500, new { error = "Failed to retrieve patient", message = ex.Message });
            }
        }

        // GET: api/Patients/{id}/partographs
        [HttpGet("{id}/partographs")]
        public async Task<ActionResult<IEnumerable<Partograph>>> GetPatientPartographs(Guid id)
        {
            try
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.ID == id && p.Deleted == 0);

                if (patient == null)
                {
                    return NotFound(new { error = "Patient not found", id });
                }

                var partographs = await _context.Partographs
                    .Where(p => p.PatientID == id && p.Deleted == 0)
                    .OrderByDescending(p => p.AdmissionDate)
                    .ToListAsync();

                return Ok(partographs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partographs for patient {PatientId}", id);
                return StatusCode(500, new { error = "Failed to retrieve partographs", message = ex.Message });
            }
        }

        // POST: api/Patients
        [HttpPost]
        public async Task<ActionResult<Patient>> CreatePatient(Patient patient)
        {
            try
            {
                if (patient.ID == null || patient.ID == Guid.Empty)
                {
                    patient.ID = Guid.NewGuid();
                }

                patient.CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                patient.UpdatedTime = patient.CreatedTime;
                patient.ServerVersion = 1;
                patient.Version = 1;
                patient.SyncStatus = 1; // Synced
                patient.Deleted = 0;

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPatient), new { id = patient.ID }, patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                return StatusCode(500, new { error = "Failed to create patient", message = ex.Message });
            }
        }

        // PUT: api/Patients/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(Guid id, Patient patient)
        {
            try
            {
                if (id != patient.ID)
                {
                    return BadRequest(new { error = "Patient ID mismatch" });
                }

                var existing = await _context.Patients.FindAsync(id);
                if (existing == null || existing.Deleted == 1)
                {
                    return NotFound(new { error = "Patient not found", id });
                }

                // Update fields
                existing.FirstName = patient.FirstName;
                existing.LastName = patient.LastName;
                existing.HospitalNumber = patient.HospitalNumber;
                existing.DateOfBirth = patient.DateOfBirth;
                existing.Age = patient.Age;
                existing.BloodGroup = patient.BloodGroup;
                existing.PhoneNumber = patient.PhoneNumber;
                existing.EmergencyContactName = patient.EmergencyContactName;
                existing.EmergencyContactRelationship = patient.EmergencyContactRelationship;
                existing.EmergencyContactPhone = patient.EmergencyContactPhone;
                existing.HandlerName = patient.HandlerName;
                existing.Handler = patient.Handler;

                existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                existing.ServerVersion++;
                existing.SyncStatus = 1; // Synced

                await _context.SaveChangesAsync();

                return Ok(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient {PatientId}", id);
                return StatusCode(500, new { error = "Failed to update patient", message = ex.Message });
            }
        }

        // DELETE: api/Patients/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(Guid id)
        {
            try
            {
                var patient = await _context.Patients.FindAsync(id);
                if (patient == null || patient.Deleted == 1)
                {
                    return NotFound(new { error = "Patient not found", id });
                }

                // Soft delete
                patient.Deleted = 1;
                patient.DeletedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                patient.UpdatedTime = patient.DeletedTime.Value;
                patient.ServerVersion++;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient {PatientId}", id);
                return StatusCode(500, new { error = "Failed to delete patient", message = ex.Message });
            }
        }

        // GET: api/Patients/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Patient>>> SearchPatients(
            [FromQuery] string query,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { error = "Search query is required" });
                }

                query = query.ToLower();
                var patientQuery = _context.Patients
                    .Where(p => p.Deleted == 0 &&
                        (p.FirstName.ToLower().Contains(query) ||
                         p.LastName.ToLower().Contains(query) ||
                         p.HospitalNumber.ToLower().Contains(query) ||
                         p.PhoneNumber.Contains(query)));

                // Filter by facility if specified
                if (facilityId.HasValue)
                {
                    patientQuery = patientQuery.Where(p => p.FacilityID == facilityId);
                }

                var patients = await patientQuery.Take(50).ToListAsync();

                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching patients with query {Query}", query);
                return StatusCode(500, new { error = "Failed to search patients", message = ex.Message });
            }
        }

        // GET: api/Patients/stats
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetPatientStats([FromQuery] Guid? facilityId = null)
        {
            try
            {
                var patientsQuery = _context.Patients.Where(p => p.Deleted == 0);
                var partographsQuery = _context.Partographs.Where(p => p.Deleted == 0);

                // Filter by facility if specified
                if (facilityId.HasValue)
                {
                    patientsQuery = patientsQuery.Where(p => p.FacilityID == facilityId);
                    partographsQuery = partographsQuery.Where(p => p.FacilityID == facilityId);
                }

                var totalPatients = await patientsQuery.CountAsync();
                var activeLabor = await partographsQuery.CountAsync(p => p.Status == LaborStatus.FirstStage || p.Status == LaborStatus.SecondStage || p.Status == LaborStatus.ThirdStage || p.Status == LaborStatus.FourthStage);
                var completedToday = await partographsQuery.CountAsync(p =>
                    p.Status == LaborStatus.Completed &&
                    p.DeliveryTime.HasValue &&
                    p.DeliveryTime.Value.Date == DateTime.Today);

                return Ok(new
                {
                    totalPatients,
                    activeLabor,
                    completedToday,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient stats");
                return StatusCode(500, new { error = "Failed to retrieve stats", message = ex.Message });
            }
        }
    }
}
