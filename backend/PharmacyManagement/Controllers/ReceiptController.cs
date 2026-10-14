using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PharmacyManagement.DTOs.Receipt;
using PharmacyManagement.Infrastructure.RateLimiting;
using PharmacyManagement.Services.Interfaces;
using PharmacyManagement.Validators.PermissionHandle;

namespace PharmacyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptController : BaseController
    {
        private readonly IReceiptService _service;

        public ReceiptController(IReceiptService service)
        {
            _service = service;
        }

        [HttpPost]
        [HasPermission("RECEIPT_CREATE")]
        public async Task<IActionResult> Create(CreateReceiptRequest request)
        {
            var userId = GetUserIdFromToken();
            var branchId = GetBranchIdFromHeader();
            var result = await _service.CreateAsync(request, userId, branchId);
            return Success(result, "Create receipt successfully.");
        }

        [HttpPut("{id}")]
        [HasPermission("RECEIPT_UPDATE")]
        public async Task<IActionResult> Update(long id, UpdateReceiptRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return Success(result, "Update receipt successfully.");
        }

        [HttpDelete("{id}")]
        [HasPermission("RECEIPT_DELETE")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);
            return Success(null, "Delete receipt successfully.");
        }

        [HttpGet("{id}")]
        [HasPermission("RECEIPT_VIEW")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result, "Get receipt successfully.");
        }

        [HttpGet]
        [HasPermission("RECEIPT_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetAll(long? customerId, int page = 1, int count = 10)
        {
            var result = await _service.GetAllAsync(customerId, page, count);
            return Success(result, "Get receipts successfully.");
        }
    }
}
