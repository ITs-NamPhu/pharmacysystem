using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PharmacyManagement.DTOs.StockTake;
using PharmacyManagement.Infrastructure.RateLimiting;
using PharmacyManagement.Services.Interfaces;
using PharmacyManagement.Validators.PermissionHandle;

namespace PharmacyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockTakeController : BaseController
    {
        private readonly IStockTakeService _service;

        public StockTakeController(IStockTakeService service)
        {
            _service = service;
        }

        [HttpPost]
        [HasPermission("STOCKTAKE_CREATE")]
        public async Task<IActionResult> StockTake(CreateStockTakeRequest request)
        {
            var userId = GetUserIdFromToken();
            var result = await _service.CreateAsync(request, userId);
            return Success(result, "Create stock take successfully.");
        }

        [HttpPut("{id}")]
        [HasPermission("STOCKTAKE_UPDATE")]
        public async Task<IActionResult> Update(long id, UpdateStockTakeRequest request)
        {
            var userId = GetUserIdFromToken();
            var result = await _service.UpdateAsync(id, request, userId);
            return Success(result, "Update stock take successfully.");
        }

        [HttpPut("{id}/complete")]
        [HasPermission("STOCKTAKE_UPDATE")]
        public async Task<IActionResult> Complete(long id)
        {
            var userId = GetUserIdFromToken();
            var result = await _service.CompleteAsync(id, userId);
            return Success(result, "Complete stock take successfully.");
        }

        [HttpPut("{id}/cancel")]
        [HasPermission("STOCKTAKE_UPDATE")]
        public async Task<IActionResult> Cancel(long id)
        {
            var userId = GetUserIdFromToken();
            await _service.CancelAsync(id, userId);
            return Success(null, "Cancel stock take successfully.");
        }

        [HttpPut("{id}/approve")]
        [HasPermission("STOCKTAKE_APPROVE")]
        public async Task<IActionResult> Approve(long id)
        {
            var userId = GetUserIdFromToken();
            await _service.ApproveAsync(id, userId);
            return Success(null, "Approve stock take successfully.");
        }

        [HttpPut("{id}/reject")]
        [HasPermission("STOCKTAKE_APPROVE")]
        public async Task<IActionResult> Reject(long id)
        {
            var userId = GetUserIdFromToken();
            await _service.RejectAsync(id, userId);
            return Success(null, "Reject stock take successfully.");
        }

        [HttpDelete("{id}")]
        [HasPermission("STOCKTAKE_DELETE")]
        public async Task<IActionResult> Delete(long id)
        {
            var userId = GetUserIdFromToken();
            await _service.DeleteAsync(id, userId);
            return Success(null, "Delete stock take successfully.");
        }

        [HttpGet("{id}")]
        [HasPermission("STOCKTAKE_VIEW")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result, "Get stock take successfully.");
        }

        [HttpGet("GetAll")]
        [HasPermission("STOCKTAKE_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetAll(int page, int count = 10, long warehouseId = 0)
        {
            var result = await _service.GetAllAsync(page, count, warehouseId);
            return Success(result, "Get stock takes successfully.");
        }
    }
}
