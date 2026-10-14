using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PharmacyManagement.DTOs.StockAdjustment;
using PharmacyManagement.Infrastructure.RateLimiting;
using PharmacyManagement.Services.Interfaces;
using PharmacyManagement.Validators.PermissionHandle;

namespace PharmacyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockAdjustmentController : BaseController
    {
        private readonly IStockAdjustmentService _service;

        public StockAdjustmentController(IStockAdjustmentService service)
        {
            _service = service;
        }

        [HttpPost]
        [HasPermission("STOCK_ADJUSTMENT_CREATE")]
        public async Task<IActionResult> StockAdjustment(CreateStockAdjustmentRequest request)
        {
            var userId = GetUserIdFromToken();
            var result = await _service.CreateAsync(request, userId);
            return Success(result, "Create stock adjustment successfully.");
        }

        [HttpPut("{id}")]
        [HasPermission("STOCK_ADJUSTMENT_CREATE")]
        public async Task<IActionResult> Update(long id, UpdateStockAdjustmentRequest request)
        {
            var userId = GetUserIdFromToken();
            var result = await _service.UpdateAsync(id, request, userId);
            return Success(result, "Update stock adjustment successfully.");
        }

        [HttpPut("{id}/complete")]
        [HasPermission("STOCK_ADJUSTMENT_CREATE")]
        public async Task<IActionResult> Complete(long id)
        {
            var userId = GetUserIdFromToken();
            var result = await _service.CompleteAsync(id, userId);
            return Success(result, "Complete stock adjustment successfully.");
        }

        [HttpPut("{id}/approve")]
        [HasPermission("STOCK_ADJUSTMENT_APPROVE")]
        public async Task<IActionResult> Approve(long id)
        {
            var userId = GetUserIdFromToken();
            await _service.ApproveAsync(id, userId);
            return Success(null, "Approve stock adjustment successfully.");
        }

        [HttpPut("{id}/reject")]
        [HasPermission("STOCK_ADJUSTMENT_APPROVE")]
        public async Task<IActionResult> Reject(long id)
        {
            var userId = GetUserIdFromToken();
            await _service.RejectAsync(id, userId);
            return Success(null, "Reject stock adjustment successfully.");
        }

        [HttpDelete("{id}")]
        [HasPermission("STOCK_ADJUSTMENT_DELETE")]
        public async Task<IActionResult> Delete(long id)
        {
            var userId = GetUserIdFromToken();
            await _service.DeleteAsync(id, userId);
            return Success(null, "Delete stock adjustment successfully.");
        }

        [HttpGet("{id}")]
        [HasPermission("STOCK_ADJUSTMENT_VIEW")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result, "Get stock adjustment successfully.");
        }

        [HttpGet("GetAll")]
        [HasPermission("STOCK_ADJUSTMENT_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetAll(int page, int count = 10, long warehouseId = 0)
        {
            var result = await _service.GetAllAsync(page, count, warehouseId);
            return Success(result, "Get stock adjustments successfully.");
        }
    }
}
