using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Services;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartographsController : ControllerBase
    {
        private readonly PartographDbContext _context;
        private readonly ILogger<PartographsController> _logger;
        private readonly IPartographPdfService _pdfService;

        public PartographsController(PartographDbContext context, ILogger<PartographsController> logger, IPartographPdfService pdfService)
        {
            _context = context;
            _logger = logger;
            _pdfService = pdfService;
        }

        // GET: api/Partographs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Partograph>>> GetPartographs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] LaborStatus? status = null,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var query = _context.Partographs
                    .Where(p => p.Deleted == 0)
                    .AsQueryable();

                // Filter by facility if specified
                if (facilityId.HasValue)
                {
                    query = query.Where(p => p.FacilityID == facilityId);
                }

                if (status.HasValue)
                {
                    query = query.Where(p => p.Status == status.Value);
                }

                var total = await query.CountAsync();
                var partographs = await query
                    .OrderByDescending(p => p.AdmissionDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                Response.Headers.Append("X-Total-Count", total.ToString());
                Response.Headers.Append("X-Page", page.ToString());
                Response.Headers.Append("X-Page-Size", pageSize.ToString());

                return Ok(partographs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partographs");
                return StatusCode(500, new { error = "Failed to retrieve partographs", message = ex.Message });
            }
        }

        // GET: api/Partographs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Partograph>> GetPartograph(Guid id)
        {
            try
            {
                var partograph = await _context.Partographs
                    .FirstOrDefaultAsync(p => p.ID == id && p.Deleted == 0);

                if (partograph == null)
                {
                    return NotFound(new { error = "Partograph not found", id });
                }

                return Ok(partograph);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partograph {PartographId}", id);
                return StatusCode(500, new { error = "Failed to retrieve partograph", message = ex.Message });
            }
        }

        // GET: api/Partographs/{id}/measurements
        [HttpGet("{id}/measurements")]
        public async Task<ActionResult<object>> GetPartographMeasurements(Guid id)
        {
            try
            {
                var partograph = await _context.Partographs
                    .FirstOrDefaultAsync(p => p.ID == id && p.Deleted == 0);

                if (partograph == null)
                {
                    return NotFound(new { error = "Partograph not found", id });
                }

                var measurements = new
                {
                    fhrs = await _context.FHRs.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    contractions = await _context.Contractions.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    cervixDilatations = await _context.CervixDilatations.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    headDescents = await _context.HeadDescents.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    bps = await _context.BPs.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    temperatures = await _context.Temperatures.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    amnioticFluids = await _context.AmnioticFluids.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    urines = await _context.Urines.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    caputs = await _context.Caputs.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    mouldings = await _context.Mouldings.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    fetalPositions = await _context.FetalPositions.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    painReliefEntries = await _context.PainReliefs.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    postureEntries = await _context.Postures.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    oralFluidEntries = await _context.OralFluids.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    ivFluidEntries = await _context.IVFluids.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    medicationEntries = await _context.Medications.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    oxytocins = await _context.Oxytocins.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    companionEntries = await _context.Companions.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync(),
                    assessmentPlanEntries = await _context.Assessments.Where(m => m.PartographID == id && m.Deleted == 0).OrderBy(m => m.Time).ToListAsync()
                };

                return Ok(measurements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving measurements for partograph {PartographId}", id);
                return StatusCode(500, new { error = "Failed to retrieve measurements", message = ex.Message });
            }
        }

        // POST: api/Partographs
        [HttpPost]
        public async Task<ActionResult<Partograph>> CreatePartograph(Partograph partograph)
        {
            try
            {
                // Validate patient exists
                var patient = await _context.Patients.FindAsync(partograph.PatientID);
                if (patient == null || patient.Deleted == 1)
                {
                    return BadRequest(new { error = "Patient not found", patientId = partograph.PatientID });
                }

                if (partograph.ID == null || partograph.ID == Guid.Empty)
                {
                    partograph.ID = Guid.NewGuid();
                }

                partograph.CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                partograph.UpdatedTime = partograph.CreatedTime;
                partograph.ServerVersion = 1;
                partograph.Version = 1;
                partograph.SyncStatus = 1; // Synced
                partograph.Deleted = 0;

                _context.Partographs.Add(partograph);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPartograph), new { id = partograph.ID }, partograph);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating partograph");
                return StatusCode(500, new { error = "Failed to create partograph", message = ex.Message });
            }
        }

        // PUT: api/Partographs/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePartograph(Guid id, Partograph partograph)
        {
            try
            {
                if (id != partograph.ID)
                {
                    return BadRequest(new { error = "Partograph ID mismatch" });
                }

                var existing = await _context.Partographs.FindAsync(id);
                if (existing == null || existing.Deleted == 1)
                {
                    return NotFound(new { error = "Partograph not found", id });
                }

                // Update fields
                _context.Entry(existing).CurrentValues.SetValues(partograph);
                existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                existing.ServerVersion++;
                existing.SyncStatus = 1; // Synced

                await _context.SaveChangesAsync();

                return Ok(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating partograph {PartographId}", id);
                return StatusCode(500, new { error = "Failed to update partograph", message = ex.Message });
            }
        }

        // DELETE: api/Partographs/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePartograph(Guid id)
        {
            try
            {
                var partograph = await _context.Partographs.FindAsync(id);
                if (partograph == null || partograph.Deleted == 1)
                {
                    return NotFound(new { error = "Partograph not found", id });
                }

                // Soft delete
                partograph.Deleted = 1;
                partograph.DeletedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                partograph.UpdatedTime = partograph.DeletedTime.Value;
                partograph.ServerVersion++;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting partograph {PartographId}", id);
                return StatusCode(500, new { error = "Failed to delete partograph", message = ex.Message });
            }
        }

        // GET: api/Partographs/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Partograph>>> GetActiveLabor([FromQuery] Guid? facilityId = null)
        {
            try
            {
                var query = _context.Partographs
                    .Where(p => p.Deleted == 0 && p.Status == LaborStatus.FirstStage);

                // Filter by facility if specified
                if (facilityId.HasValue)
                {
                    query = query.Where(p => p.FacilityID == facilityId);
                }

                var activePartographs = await query
                    .OrderByDescending(p => p.AdmissionDate)
                    .ToListAsync();

                return Ok(activePartographs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active labour partographs");
                return StatusCode(500, new { error = "Failed to retrieve active labour partographs", message = ex.Message });
            }
        }

        // GET: api/Partographs/stats
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetPartographStats([FromQuery] Guid? facilityId = null)
        {
            try
            {
                var query = _context.Partographs.Where(p => p.Deleted == 0);

                // Filter by facility if specified
                if (facilityId.HasValue)
                {
                    query = query.Where(p => p.FacilityID == facilityId);
                }

                var total = await query.CountAsync();
                var active = await query.CountAsync(p => p.Status == LaborStatus.FirstStage);
                var pending = await query.CountAsync(p => p.Status == LaborStatus.Pending);
                var completed = await query.CountAsync(p => p.Status == LaborStatus.Completed);
                var emergency = await query.CountAsync(p => p.Status == LaborStatus.Emergency);

                var completedToday = await query.CountAsync(p =>
                    p.Status == LaborStatus.Completed &&
                    p.DeliveryTime.HasValue &&
                    p.DeliveryTime.Value.Date == DateTime.Today);

                return Ok(new
                {
                    total,
                    active,
                    pending,
                    completed,
                    emergency,
                    completedToday,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partograph stats");
                return StatusCode(500, new { error = "Failed to retrieve stats", message = ex.Message });
            }
        }

        // PATCH: api/Partographs/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdatePartographStatus(Guid id, [FromBody] LaborStatus status)
        {
            try
            {
                var partograph = await _context.Partographs.FindAsync(id);
                if (partograph == null || partograph.Deleted == 1)
                {
                    return NotFound(new { error = "Partograph not found", id });
                }

                partograph.Status = status;
                partograph.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                partograph.ServerVersion++;

                if (status == LaborStatus.Completed && !partograph.DeliveryTime.HasValue)
                {
                    partograph.DeliveryTime = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Ok(partograph);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating partograph status {PartographId}", id);
                return StatusCode(500, new { error = "Failed to update status", message = ex.Message });
            }
        }

        // GET: api/Partographs/{id}/pdf
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GeneratePartographPdf(Guid id)
        {
            try
            {
                var partograph = await _context.Partographs.FirstOrDefaultAsync(p => p.ID == id && p.Deleted == 0);
                if (partograph == null)
                {
                    return NotFound(new { error = "Partograph not found", id });
                }

                var pdfBytes = await _pdfService.GeneratePartographPdfAsync(id);
                var fileName = $"Partograph_{partograph.Patient?.Name?.Replace(" ", "_") ?? id.ToString()}_{DateTime.Now:yyyyMMdd}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating partograph PDF for {PartographId}", id);
                return StatusCode(500, new { error = "Failed to generate PDF", message = ex.Message });
            }
        }
    }
}
