using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PharmacyManagement.DTOs.Invoice;
using PharmacyManagement.Infrastructure.RateLimiting;
using PharmacyManagement.Services.Interfaces;
using PharmacyManagement.Validators.PermissionHandle;

namespace PharmacyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : BaseController
    {
        private readonly IInvoiceService _service;

        public InvoiceController(IInvoiceService service)
        {
            _service = service;
        }

        [HttpPost]
        [HasPermission("INVOICE_CREATE")]
        public async Task<IActionResult> Create(CreateInvoiceRequest request)
        {
            var branchId = GetBranchIdFromHeader();
            long userId;

            if (request.CreatedByUserID.HasValue && IsAdminOrManageSupply())
                userId = request.CreatedByUserID.Value;
            else
                userId = GetUserIdFromToken();

            var result = await _service.CreateAsync(request, userId, branchId);
            return Success(result, "Create invoice successfully.");
        }

        [HttpPut("{id}")]
        [HasPermission("INVOICE_UPDATE")]
        public async Task<IActionResult> Update(long id, UpdateInvoiceRequest request)
        {
            var branchId = GetBranchIdFromHeader();
            long userId;

            if (request.CreatedByUserID.HasValue && IsAdminOrManageSupply())
                userId = request.CreatedByUserID.Value;
            else
                userId = GetUserIdFromToken();

            var result = await _service.UpdateAsync(id, request, userId, branchId);
            return Success(result, "Update invoice successfully.");
        }

        [HttpDelete("{id}")]
        [HasPermission("INVOICE_DELETE")]
        public async Task<IActionResult> Delete(long id)
        {
            var branchId = GetBranchIdFromHeader();
            await _service.DeleteAsync(id, branchId);
            return Success(null, "Delete invoice successfully.");
        }

        [HttpGet("{id}")]
        [HasPermission("INVOICE_VIEW")]
        public async Task<IActionResult> GetById(long id)
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.GetByIdAsync(id, branchId);
            return Success(result, "Get invoice successfully.");
        }

        [HttpGet("GetAll")]
        [HasPermission("INVOICE_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetAll([FromQuery] InvoiceFilterDto filter)
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.GetAllAsync(filter ?? new InvoiceFilterDto(), branchId);
            return Success(result, "Get invoices successfully.");
        }

        [HttpGet("BatchesByMedicine")]
        [HasPermission("INVOICE_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetBatchesByMedicine(long medicineID)
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.GetBatchesByMedicineAsync(medicineID, branchId);
            return Success(result, "Get batches successfully.");
        }

        [HttpGet("FefoBatches")]
        [HasPermission("INVOICE_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetFefoBatches(long medicineID, decimal quantity)
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.GetFefoBatchesAsync(medicineID, quantity, branchId);
            return Success(result, "Get FEFO batches successfully.");
        }

        [HttpGet("UsersByBranch")]
        [HasPermission("INVOICE_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetUsersByBranch()
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.GetUsersByBranchAsync(branchId);
            return Success(result, "Get users by branch successfully.");
        }
    }
}
