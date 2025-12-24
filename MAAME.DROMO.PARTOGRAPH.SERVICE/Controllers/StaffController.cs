using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly PartographDbContext _context;
        private readonly ILogger<StaffController> _logger;

        public StaffController(PartographDbContext context, ILogger<StaffController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Staff
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Staff>>> GetStaff(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.Staff
                    .Where(s => s.Deleted == 0)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(role))
                {
                    query = query.Where(s => s.Role.ToLower() == role.ToLower());
                }

                if (isActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == isActive.Value);
                }

                var total = await query.CountAsync();
                var staff = await query
                    .OrderBy(s => s.FacilityName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new
                    {
                        s.ID,
                        s.FacilityName,
                        s.StaffID,
                        s.Email,
                        s.Role,
                        s.Department,
                        s.LastLogin,
                        s.IsActive,
                        s.Facility,
                        s.CreatedTime,
                        s.UpdatedTime
                        // Password excluded for security
                    })
                    .ToListAsync();

                Response.Headers.Append("X-Total-Count", total.ToString());
                Response.Headers.Append("X-Page", page.ToString());
                Response.Headers.Append("X-Page-Size", pageSize.ToString());

                return Ok(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff");
                return StatusCode(500, new { error = "Failed to retrieve staff", message = ex.Message });
            }
        }

        // GET: api/Staff/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetStaff(Guid id)
        {
            try
            {
                var staff = await _context.Staff
                    .Where(s => s.ID == id && s.Deleted == 0)
                    .Select(s => new
                    {
                        s.ID,
                        s.FacilityName,
                        s.StaffID,
                        s.Email,
                        s.Role,
                        s.Department,
                        s.LastLogin,
                        s.IsActive,
                        s.Facility,
                        s.CreatedTime,
                        s.UpdatedTime
                        // Password excluded for security
                    })
                    .FirstOrDefaultAsync();

                if (staff == null)
                {
                    return NotFound(new { error = "Staff not found", id });
                }

                return Ok(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff {StaffId}", id);
                return StatusCode(500, new { error = "Failed to retrieve staff", message = ex.Message });
            }
        }

        // POST: api/Staff/authenticate
        [HttpPost("authenticate")]
        public async Task<ActionResult<object>> Authenticate([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { error = "Email and password are required" });
                }

                var staff = await _context.Staff
                    .FirstOrDefaultAsync(s => s.Email.ToLower() == request.Email.ToLower() && s.Deleted == 0);

                if (staff == null || !staff.IsActive)
                {
                    return Unauthorized(new { error = "Invalid credentials or inactive account" });
                }

                // Simple password verification (in production, use proper password hashing)
                if (staff.Password != HashPassword(request.Password))
                {
                    return Unauthorized(new { error = "Invalid credentials" });
                }

                // Update last login
                staff.LastLogin = DateTime.Now;
                staff.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    staff = new
                    {
                        staff.ID,
                        staff.FacilityName,
                        staff.StaffID,
                        staff.Email,
                        staff.Role,
                        staff.Department,
                        staff.Facility,
                        staff.LastLogin
                    },
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating staff");
                return StatusCode(500, new { error = "Authentication failed", message = ex.Message });
            }
        }

        // POST: api/Staff
        [HttpPost]
        public async Task<ActionResult<object>> CreateStaff(Staff staff)
        {
            try
            {
                // Check if email already exists
                var existingEmail = await _context.Staff
                    .AnyAsync(s => s.Email.ToLower() == staff.Email.ToLower() && s.Deleted == 0);

                if (existingEmail)
                {
                    return BadRequest(new { error = "Email already exists" });
                }

                // Check if StaffID already exists
                if (!string.IsNullOrWhiteSpace(staff.StaffID))
                {
                    var existingStaffId = await _context.Staff
                        .AnyAsync(s => s.StaffID == staff.StaffID && s.Deleted == 0);

                    if (existingStaffId)
                    {
                        return BadRequest(new { error = "Staff ID already exists" });
                    }
                }

                if (staff.ID == null || staff.ID == Guid.Empty)
                {
                    staff.ID = Guid.NewGuid();
                }

                // Hash password
                if (!string.IsNullOrWhiteSpace(staff.Password))
                {
                    staff.Password = HashPassword(staff.Password);
                }

                staff.CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                staff.UpdatedTime = staff.CreatedTime;
                staff.ServerVersion = 1;
                staff.Version = 1;
                staff.SyncStatus = 1; // Synced
                staff.Deleted = 0;
                staff.IsActive = true;

                _context.Staff.Add(staff);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetStaff), new { id = staff.ID }, new
                {
                    staff.ID,
                    staff.FacilityName,
                    staff.StaffID,
                    staff.Email,
                    staff.Role,
                    staff.Department,
                    staff.IsActive,
                    staff.Facility
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff");
                return StatusCode(500, new { error = "Failed to create staff", message = ex.Message });
            }
        }

        // PUT: api/Staff/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(Guid id, Staff staff)
        {
            try
            {
                if (id != staff.ID)
                {
                    return BadRequest(new { error = "Staff ID mismatch" });
                }

                var existing = await _context.Staff.FindAsync(id);
                if (existing == null || existing.Deleted == 1)
                {
                    return NotFound(new { error = "Staff not found", id });
                }

                // Check if email already exists (excluding current staff)
                var existingEmail = await _context.Staff
                    .AnyAsync(s => s.Email.ToLower() == staff.Email.ToLower() && s.ID != id && s.Deleted == 0);

                if (existingEmail)
                {
                    return BadRequest(new { error = "Email already exists" });
                }

                // Update fields
                existing.FacilityName = staff.FacilityName;
                existing.StaffID = staff.StaffID;
                existing.Email = staff.Email;
                existing.Role = staff.Role;
                existing.Department = staff.Department;
                existing.IsActive = staff.IsActive;
                existing.Facility = staff.Facility;

                // Only update password if provided
                if (!string.IsNullOrWhiteSpace(staff.Password))
                {
                    existing.Password = HashPassword(staff.Password);
                }

                existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                existing.ServerVersion++;
                existing.SyncStatus = 1; // Synced

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    existing.ID,
                    existing.FacilityName,
                    existing.StaffID,
                    existing.Email,
                    existing.Role,
                    existing.Department,
                    existing.IsActive,
                    existing.Facility
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff {StaffId}", id);
                return StatusCode(500, new { error = "Failed to update staff", message = ex.Message });
            }
        }

        // DELETE: api/Staff/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(Guid id)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff == null || staff.Deleted == 1)
                {
                    return NotFound(new { error = "Staff not found", id });
                }

                // Soft delete
                staff.Deleted = 1;
                staff.DeletedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                staff.UpdatedTime = staff.DeletedTime.Value;
                staff.ServerVersion++;
                staff.IsActive = false;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff {StaffId}", id);
                return StatusCode(500, new { error = "Failed to delete staff", message = ex.Message });
            }
        }

        // PATCH: api/Staff/{id}/activate
        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateStaff(Guid id)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff == null || staff.Deleted == 1)
                {
                    return NotFound(new { error = "Staff not found", id });
                }

                staff.IsActive = true;
                staff.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                staff.ServerVersion++;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Staff activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating staff {StaffId}", id);
                return StatusCode(500, new { error = "Failed to activate staff", message = ex.Message });
            }
        }

        // PATCH: api/Staff/{id}/deactivate
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateStaff(Guid id)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff == null || staff.Deleted == 1)
                {
                    return NotFound(new { error = "Staff not found", id });
                }

                staff.IsActive = false;
                staff.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                staff.ServerVersion++;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Staff deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating staff {StaffId}", id);
                return StatusCode(500, new { error = "Failed to deactivate staff", message = ex.Message });
            }
        }

        // GET: api/Staff/roles
        [HttpGet("roles")]
        public ActionResult<IEnumerable<string>> GetRoles()
        {
            var roles = new[]
            {
                "Administrator",
                "Doctor",
                "Midwife",
                "Nurse",
                "Medical Officer",
                "Resident",
                "Consultant"
            };

            return Ok(roles);
        }

        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }

    // Login request model
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DeviceId { get; set; }
    }
}
