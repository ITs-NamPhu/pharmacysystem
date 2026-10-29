using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PharmacyManagement.DTOs.Batch;
using PharmacyManagement.DTOs.Warehouse;
using PharmacyManagement.Exceptions;
using PharmacyManagement.Infrastructure.RateLimiting;
using PharmacyManagement.Services.Interfaces;
using PharmacyManagement.Validators.PermissionHandle;

namespace PharmacyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : BaseController
    {
        private readonly IWarehouseService _service;

        public WarehouseController(IWarehouseService service)
        {
            _service = service;
        }

        [HttpPost]
        [HasPermission("WAREHOUSE_CREATE")]
        public async Task<IActionResult> Create(CreateWarehouseRequest request)
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.CreateAsync(request, branchId);
            return Success(result, "Create warehouse successfully.");
        }

        [HttpPut("{id}")]
        [HasPermission("WAREHOUSE_UPDATE")]
        public async Task<IActionResult> Update(long id, UpdateWarehouseRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return Success(result, "Update warehouse successfully.");
        }

        [HttpDelete("{id}")]
        [HasPermission("WAREHOUSE_DELETE")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);
            return Success(null, "Delete warehouse successfully.");
        }

        [HttpGet("{id}")]
        [HasPermission("WAREHOUSE_VIEW")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result, "Get warehouse successfully.");
        }

        [HttpGet("GetAll")]
        [HasPermission("WAREHOUSE_VIEW")]
        public async Task<IActionResult> GetAll(int page, int count = 10)
        {
            var result = await _service.GetAllAsync(page, count);
            return Success(result, "Get warehouses successfully.");
        }

        [HttpGet("ByBranch/{branchId}")]
        [HasPermission("WAREHOUSE_VIEW")]
        public async Task<IActionResult> GetByBranch(long branchId, int page, int count = 10)
        {
            var result = await _service.GetByBranchAsync(branchId, page, count);
            return Success(result, "Get warehouses by branch successfully.");
        }

        [HttpGet("batches")]
        [HasPermission("WAREHOUSE_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetBatches([FromQuery] BatchFilterDto filter)
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.GetBatchesAsync(filter ?? new BatchFilterDto(), branchId);
            return Success(result, "Get warehouse batches successfully.");
        }
    }
}
