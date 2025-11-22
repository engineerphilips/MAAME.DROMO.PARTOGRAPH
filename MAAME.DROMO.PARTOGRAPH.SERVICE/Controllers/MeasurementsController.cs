using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeasurementsController : ControllerBase
    {
        private readonly PartographDbContext _context;
        private readonly ILogger<MeasurementsController> _logger;

        public MeasurementsController(PartographDbContext context, ILogger<MeasurementsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region FHR (Fetal Heart Rate) Endpoints

        [HttpGet("fhr")]
        public async Task<ActionResult<IEnumerable<FHR>>> GetFHRs([FromQuery] Guid? partographId = null)
        {
            var query = _context.FHRs.Where(m => m.Deleted == 0);
            if (partographId.HasValue)
                query = query.Where(m => m.PartographID == partographId.Value);

            return Ok(await query.OrderBy(m => m.Time).ToListAsync());
        }

        [HttpPost("fhr")]
        public async Task<ActionResult<FHR>> CreateFHR(FHR measurement)
        {
            return await CreateMeasurement(_context.FHRs, measurement, nameof(GetFHRs));
        }

        [HttpPut("fhr/{id}")]
        public async Task<IActionResult> UpdateFHR(Guid id, FHR measurement)
        {
            return await UpdateMeasurement(_context.FHRs, id, measurement);
        }

        [HttpDelete("fhr/{id}")]
        public async Task<IActionResult> DeleteFHR(Guid id)
        {
            return await DeleteMeasurement(_context.FHRs, id);
        }

        #endregion

        #region Contraction Endpoints

        [HttpGet("contractions")]
        public async Task<ActionResult<IEnumerable<Contraction>>> GetContractions([FromQuery] Guid? partographId = null)
        {
            var query = _context.Contractions.Where(m => m.Deleted == 0);
            if (partographId.HasValue)
                query = query.Where(m => m.PartographID == partographId.Value);

            return Ok(await query.OrderBy(m => m.Time).ToListAsync());
        }

        [HttpPost("contractions")]
        public async Task<ActionResult<Contraction>> CreateContraction(Contraction measurement)
        {
            return await CreateMeasurement(_context.Contractions, measurement, nameof(GetContractions));
        }

        [HttpPut("contractions/{id}")]
        public async Task<IActionResult> UpdateContraction(Guid id, Contraction measurement)
        {
            return await UpdateMeasurement(_context.Contractions, id, measurement);
        }

        [HttpDelete("contractions/{id}")]
        public async Task<IActionResult> DeleteContraction(Guid id)
        {
            return await DeleteMeasurement(_context.Contractions, id);
        }

        #endregion

        #region Cervix Dilatation Endpoints

        [HttpGet("cervix-dilatations")]
        public async Task<ActionResult<IEnumerable<CervixDilatation>>> GetCervixDilatations([FromQuery] Guid? partographId = null)
        {
            var query = _context.CervixDilatations.Where(m => m.Deleted == 0);
            if (partographId.HasValue)
                query = query.Where(m => m.PartographID == partographId.Value);

            return Ok(await query.OrderBy(m => m.Time).ToListAsync());
        }

        [HttpPost("cervix-dilatations")]
        public async Task<ActionResult<CervixDilatation>> CreateCervixDilatation(CervixDilatation measurement)
        {
            return await CreateMeasurement(_context.CervixDilatations, measurement, nameof(GetCervixDilatations));
        }

        [HttpPut("cervix-dilatations/{id}")]
        public async Task<IActionResult> UpdateCervixDilatation(Guid id, CervixDilatation measurement)
        {
            return await UpdateMeasurement(_context.CervixDilatations, id, measurement);
        }

        [HttpDelete("cervix-dilatations/{id}")]
        public async Task<IActionResult> DeleteCervixDilatation(Guid id)
        {
            return await DeleteMeasurement(_context.CervixDilatations, id);
        }

        #endregion

        #region Head Descent Endpoints

        [HttpGet("head-descents")]
        public async Task<ActionResult<IEnumerable<HeadDescent>>> GetHeadDescents([FromQuery] Guid? partographId = null)
        {
            var query = _context.HeadDescents.Where(m => m.Deleted == 0);
            if (partographId.HasValue)
                query = query.Where(m => m.PartographID == partographId.Value);

            return Ok(await query.OrderBy(m => m.Time).ToListAsync());
        }

        [HttpPost("head-descents")]
        public async Task<ActionResult<HeadDescent>> CreateHeadDescent(HeadDescent measurement)
        {
            return await CreateMeasurement(_context.HeadDescents, measurement, nameof(GetHeadDescents));
        }

        [HttpPut("head-descents/{id}")]
        public async Task<IActionResult> UpdateHeadDescent(Guid id, HeadDescent measurement)
        {
            return await UpdateMeasurement(_context.HeadDescents, id, measurement);
        }

        [HttpDelete("head-descents/{id}")]
        public async Task<IActionResult> DeleteHeadDescent(Guid id)
        {
            return await DeleteMeasurement(_context.HeadDescents, id);
        }

        #endregion

        #region Blood Pressure Endpoints

        [HttpGet("bp")]
        public async Task<ActionResult<IEnumerable<BP>>> GetBPs([FromQuery] Guid? partographId = null)
        {
            var query = _context.BPs.Where(m => m.Deleted == 0);
            if (partographId.HasValue)
                query = query.Where(m => m.PartographID == partographId.Value);

            return Ok(await query.OrderBy(m => m.Time).ToListAsync());
        }

        [HttpPost("bp")]
        public async Task<ActionResult<BP>> CreateBP(BP measurement)
        {
            return await CreateMeasurement(_context.BPs, measurement, nameof(GetBPs));
        }

        [HttpPut("bp/{id}")]
        public async Task<IActionResult> UpdateBP(Guid id, BP measurement)
        {
            return await UpdateMeasurement(_context.BPs, id, measurement);
        }

        [HttpDelete("bp/{id}")]
        public async Task<IActionResult> DeleteBP(Guid id)
        {
            return await DeleteMeasurement(_context.BPs, id);
        }

        #endregion

        #region Temperature Endpoints

        [HttpGet("temperatures")]
        public async Task<ActionResult<IEnumerable<Temperature>>> GetTemperatures([FromQuery] Guid? partographId = null)
        {
            var query = _context.Temperatures.Where(m => m.Deleted == 0);
            if (partographId.HasValue)
                query = query.Where(m => m.PartographID == partographId.Value);

            return Ok(await query.OrderBy(m => m.Time).ToListAsync());
        }

        [HttpPost("temperatures")]
        public async Task<ActionResult<Temperature>> CreateTemperature(Temperature measurement)
        {
            return await CreateMeasurement(_context.Temperatures, measurement, nameof(GetTemperatures));
        }

        [HttpPut("temperatures/{id}")]
        public async Task<IActionResult> UpdateTemperature(Guid id, Temperature measurement)
        {
            return await UpdateMeasurement(_context.Temperatures, id, measurement);
        }

        [HttpDelete("temperatures/{id}")]
        public async Task<IActionResult> DeleteTemperature(Guid id)
        {
            return await DeleteMeasurement(_context.Temperatures, id);
        }

        #endregion

        #region Amniotic Fluid Endpoints

        [HttpGet("amniotic-fluids")]
        public async Task<ActionResult<IEnumerable<AmnioticFluid>>> GetAmnioticFluids([FromQuery] Guid? partographId = null)
        {
            var query = _context.AmnioticFluids.Where(m => m.Deleted == 0);
            if (partographId.HasValue)
                query = query.Where(m => m.PartographID == partographId.Value);

            return Ok(await query.OrderBy(m => m.Time).ToListAsync());
        }

        [HttpPost("amniotic-fluids")]
        public async Task<ActionResult<AmnioticFluid>> CreateAmnioticFluid(AmnioticFluid measurement)
        {
            return await CreateMeasurement(_context.AmnioticFluids, measurement, nameof(GetAmnioticFluids));
        }

        [HttpPut("amniotic-fluids/{id}")]
        public async Task<IActionResult> UpdateAmnioticFluid(Guid id, AmnioticFluid measurement)
        {
            return await UpdateMeasurement(_context.AmnioticFluids, id, measurement);
        }

        [HttpDelete("amniotic-fluids/{id}")]
        public async Task<IActionResult> DeleteAmnioticFluid(Guid id)
        {
            return await DeleteMeasurement(_context.AmnioticFluids, id);
        }

        #endregion

        #region Urine Endpoints

        [HttpGet("urines")]
        public async Task<ActionResult<IEnumerable<Urine>>> GetUrines([FromQuery] Guid? partographId = null)
        {
            var query = _context.Urines.Where(m => m.Deleted == 0);
            if (partographId.HasValue)
                query = query.Where(m => m.PartographID == partographId.Value);

            return Ok(await query.OrderBy(m => m.Time).ToListAsync());
        }

        [HttpPost("urines")]
        public async Task<ActionResult<Urine>> CreateUrine(Urine measurement)
        {
            return await CreateMeasurement(_context.Urines, measurement, nameof(GetUrines));
        }

        [HttpPut("urines/{id}")]
        public async Task<IActionResult> UpdateUrine(Guid id, Urine measurement)
        {
            return await UpdateMeasurement(_context.Urines, id, measurement);
        }

        [HttpDelete("urines/{id}")]
        public async Task<IActionResult> DeleteUrine(Guid id)
        {
            return await DeleteMeasurement(_context.Urines, id);
        }

        #endregion

        #region Medication Endpoints

        [HttpGet("medications")]
        public async Task<ActionResult<IEnumerable<MedicationEntry>>> GetMedications([FromQuery] Guid? partographId = null)
        {
            var query = _context.Medications.Where(m => m.Deleted == 0);
            if (partographId.HasValue)
                query = query.Where(m => m.PartographID == partographId.Value);

            return Ok(await query.OrderBy(m => m.Time).ToListAsync());
        }

        [HttpPost("medications")]
        public async Task<ActionResult<MedicationEntry>> CreateMedication(MedicationEntry measurement)
        {
            return await CreateMeasurement(_context.Medications, measurement, nameof(GetMedications));
        }

        [HttpPut("medications/{id}")]
        public async Task<IActionResult> UpdateMedication(Guid id, MedicationEntry measurement)
        {
            return await UpdateMeasurement(_context.Medications, id, measurement);
        }

        [HttpDelete("medications/{id}")]
        public async Task<IActionResult> DeleteMedication(Guid id)
        {
            return await DeleteMeasurement(_context.Medications, id);
        }

        #endregion

        #region Medical Notes Endpoints

        [HttpGet("medical-notes")]
        public async Task<ActionResult<IEnumerable<MedicalNote>>> GetMedicalNotes([FromQuery] Guid? partographId = null)
        {
            var query = _context.MedicalNotes.Where(m => m.Deleted == 0);
            if (partographId.HasValue)
                query = query.Where(m => m.PartographID == partographId.Value);

            return Ok(await query.OrderBy(m => m.Time).ToListAsync());
        }

        [HttpPost("medical-notes")]
        public async Task<ActionResult<MedicalNote>> CreateMedicalNote(MedicalNote measurement)
        {
            return await CreateMeasurement(_context.MedicalNotes, measurement, nameof(GetMedicalNotes));
        }

        [HttpPut("medical-notes/{id}")]
        public async Task<IActionResult> UpdateMedicalNote(Guid id, MedicalNote measurement)
        {
            return await UpdateMeasurement(_context.MedicalNotes, id, measurement);
        }

        [HttpDelete("medical-notes/{id}")]
        public async Task<IActionResult> DeleteMedicalNote(Guid id)
        {
            return await DeleteMeasurement(_context.MedicalNotes, id);
        }

        #endregion

        #region Generic Helper Methods

        private async Task<ActionResult<T>> CreateMeasurement<T>(
            DbSet<T> dbSet,
            T measurement,
            string getActionName) where T : BasePartographMeasurement
        {
            try
            {
                // Validate partograph exists
                if (measurement.PartographID.HasValue)
                {
                    var partograph = await _context.Partographs.FindAsync(measurement.PartographID.Value);
                    if (partograph == null || partograph.Deleted == 1)
                    {
                        return BadRequest(new { error = "Partograph not found", partographId = measurement.PartographID });
                    }
                }

                if (measurement.ID == null || measurement.ID == Guid.Empty)
                {
                    measurement.ID = Guid.NewGuid();
                }

                measurement.CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                measurement.UpdatedTime = measurement.CreatedTime;
                measurement.ServerVersion = 1;
                measurement.Version = 1;
                measurement.SyncStatus = 1; // Synced
                measurement.Deleted = 0;

                dbSet.Add(measurement);
                await _context.SaveChangesAsync();

                return CreatedAtAction(getActionName, new { id = measurement.ID }, measurement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating measurement");
                return StatusCode(500, new { error = "Failed to create measurement", message = ex.Message });
            }
        }

        private async Task<IActionResult> UpdateMeasurement<T>(
            DbSet<T> dbSet,
            Guid id,
            T measurement) where T : BasePartographMeasurement
        {
            try
            {
                if (id != measurement.ID)
                {
                    return BadRequest(new { error = "Measurement ID mismatch" });
                }

                var existing = await dbSet.FindAsync(id);
                if (existing == null || existing.Deleted == 1)
                {
                    return NotFound(new { error = "Measurement not found", id });
                }

                // Update fields
                _context.Entry(existing).CurrentValues.SetValues(measurement);
                existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                existing.ServerVersion++;
                existing.SyncStatus = 1; // Synced

                await _context.SaveChangesAsync();

                return Ok(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating measurement {MeasurementId}", id);
                return StatusCode(500, new { error = "Failed to update measurement", message = ex.Message });
            }
        }

        private async Task<IActionResult> DeleteMeasurement<T>(
            DbSet<T> dbSet,
            Guid id) where T : BasePartographMeasurement
        {
            try
            {
                var measurement = await dbSet.FindAsync(id);
                if (measurement == null || measurement.Deleted == 1)
                {
                    return NotFound(new { error = "Measurement not found", id });
                }

                // Soft delete
                measurement.Deleted = 1;
                measurement.DeletedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                measurement.UpdatedTime = measurement.DeletedTime.Value;
                measurement.ServerVersion++;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting measurement {MeasurementId}", id);
                return StatusCode(500, new { error = "Failed to delete measurement", message = ex.Message });
            }
        }

        #endregion
    }
}
