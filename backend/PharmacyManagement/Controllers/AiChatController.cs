using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PharmacyManagement.DTOs.AiSearch;
using PharmacyManagement.Infrastructure.RateLimiting;
using PharmacyManagement.Services.Interfaces;
using PharmacyManagement.Validators.PermissionHandle;

namespace PharmacyManagement.Controllers
{
    [Route("api/ai")]
    [ApiController]
    [EnableRateLimiting(RateLimitPolicies.Expensive)]
    public class AiChatController : BaseController
    {
        private readonly IAiSearchService _service;

        public AiChatController(IAiSearchService service)
        {
            _service = service;
        }

        [HttpPost("analytics/report")]
        [HasPermission("INVOICE_VIEW")]
        public async Task<IActionResult> Report([FromBody] AiAnalyticsRequest? filter)
        {
            var result = await _service.SearchAnalyticsAsync(
                filter ?? new AiAnalyticsRequest(), ResolveBranchScope());
            return Success(result, "Analytics report generated successfully.");
        }

        [HttpPost("customer/search")]
        [HasPermission("CUSTOMER_VIEW")]
        public async Task<IActionResult> SearchCustomers([FromBody] AiCustomerFilterRequest? filter)
        {
            var result = await _service.SearchCustomersAsync(
                filter ?? new AiCustomerFilterRequest(), ResolveBranchScope());
            return Success(result, "Search customers successfully.");
        }

        [HttpPost("goodsreceipt/search")]
        [HasPermission("GOODS_RECEIPT_VIEW")]
        public async Task<IActionResult> SearchGoodsReceipts([FromBody] AiGoodsReceiptFilterRequest? filter)
        {
            var result = await _service.SearchGoodsReceiptsAsync(
                filter ?? new AiGoodsReceiptFilterRequest(), ResolveBranchScope());
            return Success(result, "Search goods receipts successfully.");
        }

        [HttpPost("inventory/search")]
        [HasPermission("MEDICINE_VIEW")]
        public async Task<IActionResult> SearchInventory([FromBody] AiInventoryFilterRequest? filter)
        {
            var result = await _service.SearchInventoryAsync(
                filter ?? new AiInventoryFilterRequest(), ResolveBranchScope());
            return Success(result, "Search inventory successfully.");
        }

        [HttpPost("sales/search")]
        [HasPermission("INVOICE_VIEW")]
        public async Task<IActionResult> SearchSales([FromBody] AiSalesFilterRequest? filter)
        {
            var result = await _service.SearchSalesAsync(
                filter ?? new AiSalesFilterRequest(), ResolveBranchScope());
            return Success(result, "Search sales successfully.");
        }

        [HttpPost("sales/history")]
        [HasPermission("INVOICE_VIEW")]
        public async Task<IActionResult> SalesHistory([FromBody] AiSalesHistoryRequest? filter)
        {
            var result = await _service.GetSalesHistoryAsync(
                filter ?? new AiSalesHistoryRequest(), ResolveBranchScope());
            return Success(result, "Sales history loaded successfully.");
        }
    }
}