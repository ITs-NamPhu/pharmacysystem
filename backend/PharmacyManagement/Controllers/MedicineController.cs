using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PharmacyManagement.DTOs.Medicine;
using PharmacyManagement.Infrastructure.RateLimiting;
using PharmacyManagement.Services.Interfaces;
using PharmacyManagement.Validators.PermissionHandle;

namespace PharmacyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicineController : BaseController
    {
        private readonly IMedicineService _service;

        public MedicineController(IMedicineService service)
        {
            _service = service;
        }

        [HttpPost]
        [HasPermission("MEDICINE_CREATE")]
        public async Task<IActionResult> Create(CreateMedicineRequest request)
        {
            var result = await _service.CreateAsync(request);
            return Success(result, "Create medicine successfully.");
        }

        [HttpPut("{id}")]
        [HasPermission("MEDICINE_UPDATE")]
        public async Task<IActionResult> Update(long id, UpdateMedicineRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return Success(result, "Update medicine successfully.");
        }

        [HttpDelete("{id}")]
        [HasPermission("MEDICINE_DELETE")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);
            return Success(null, "Delete medicine successfully.");
        }

        [HttpGet("{id}")]
        [HasPermission("MEDICINE_VIEW")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result, "Get medicine successfully.");
        }

        [HttpGet("GetAll")]
        [HasPermission("MEDICINE_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetAll([FromQuery] MedicineFilterDto filter)
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.GetAllAsync(filter ?? new MedicineFilterDto(), branchId);
            return Success(result, "Get medicines successfully.");
        }

        [HttpGet("All")]
        [HasPermission("MEDICINE_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetAllMedicine()
        {
            var result = await _service.GetAllMedicineAsync();
            return Success(result, "Get all medicines successfully.");
        }
    }
}
